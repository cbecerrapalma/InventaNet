using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IOrdenCompraRepository : IRepositorySync<OrdenCompra>
{
    Result<IReadOnlyList<OrdenCompra>> GetByTenant(int idTenant, byte? estado = null);
    Result<IReadOnlyList<OrdenCompra>> GetByProveedor(int idTenant, int idProveedor);
    Result<IReadOnlyList<OrdenCompra>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta);
    Result<OrdenCompra?> GetByNumOrden(int idTenant, string numOrden);
    Result<OrdenCompra?> GetWithDetalles(long idOrdenCompra);

    /// <summary>
    /// Órdenes en estado Pendiente (1) o Aprobada (2) cuya fecha de entrega
    /// vence en los próximos diasAlerta días.
    /// </summary>
    Result<IReadOnlyList<OrdenCompra>> GetPendientesVencimiento(int idTenant, int diasAlerta = 3);

    Result<bool> ExistsByNumOrden(int idTenant, string numOrden);

    /// <summary>
    /// Total monetario agrupado por estado para el tenant.
    /// 1=Pendiente, 2=Aprobada, 3=Recibida, 4=Cancelada.
    /// </summary>
    Result<IReadOnlyDictionary<byte, decimal>> GetTotalPorEstado(int idTenant);

    /// <summary>
    /// Ejecuta sp_RecibirOrdenCompra: registra la recepción total o parcial de una OC,
    /// generando MovInv, actualizando Stock con costo promedio ponderado y StockDisp,
    /// todo en una única transacción atómica.
    /// </summary>
    Result RecibirOrdenCompra(RecibirOrdenCompraRequest request);
}

public class OrdenCompraRepository : RepositorySync<OrdenCompra>, IOrdenCompraRepository
{
    // Estado: 1=Pendiente, 2=Aprobada, 3=Recibida, 4=Cancelada
    private const byte EstadoOCMin = 1;
    private const byte EstadoOCMax = 4;

    // Estados "activos" (pendientes de recibir) — usados en varias queries
    private const byte EstadoPendiente = 1;
    private const byte EstadoAprobada  = 2;

    public OrdenCompraRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<OrdenCompra>> GetByTenant(int idTenant, byte? estado = null)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (estado.HasValue)
            if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.EstadoInvalido(nameof(estado), estado.Value, EstadoOCMin, EstadoOCMax)) is { } errE) return errE;
        try
        {
            // IX_OC_Tenant_Fecha (IdTenant, FecOrden DESC)
            var query = DbSet.AsNoTracking().Where(o => o.IdTenant == idTenant);
            if (estado.HasValue) query = query.Where(o => o.Estado == estado.Value);
            return Result<IReadOnlyList<OrdenCompra>>.Success(
                query.Include(o => o.Proveedor)
                     .OrderByDescending(o => o.FecOrden)
                     .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompra>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<OrdenCompra>> GetByProveedor(int idTenant, int idProveedor)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.IdInvalido(nameof(idProveedor), idProveedor)) is { } errP) return errP;
        try
        {
            // IX_OC_Prov (IdProveedor, Estado) — cubre el filtro; sin filtro de estado incluye todas
            return Result<IReadOnlyList<OrdenCompra>>.Success(
                DbSet.AsNoTracking()
                    .Where(o => o.IdTenant == idTenant && o.IdProveedor == idProveedor)
                    .OrderByDescending(o => o.FecOrden)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompra>>.Failure(
                CreateError(ex, nameof(GetByProveedor))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdProveedor", idProveedor));
        }
    }

    public Result<IReadOnlyList<OrdenCompra>> GetByRangoFecha(
        int idTenant, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            // FecOrden es DATE → DateOnly en EF Core; se convierte para la comparación
            var desdeDate = DateOnly.FromDateTime(desde);
            var hastaDate = DateOnly.FromDateTime(hasta);
            // IX_OC_Tenant_Fecha (IdTenant, FecOrden DESC)
            return Result<IReadOnlyList<OrdenCompra>>.Success(
                DbSet.AsNoTracking()
                    .Where(o => o.IdTenant  == idTenant
                             && o.FecOrden  >= desdeDate
                             && o.FecOrden  <= hastaDate)
                    .Include(o => o.Proveedor)
                    .OrderByDescending(o => o.FecOrden)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompra>>.Failure(
                CreateError(ex, nameof(GetByRangoFecha)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<OrdenCompra?> GetByNumOrden(int idTenant, string numOrden)
    {
        if (Guard.Fail<OrdenCompra?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<OrdenCompra?>(Guard.StringRequerido(nameof(numOrden), numOrden)) is { } errStr) return errStr;
        try
        {
            // UQ_OC (IdTenant, NumOrden) — seek de punto único
            return Result<OrdenCompra?>.Success(
                DbSet.AsNoTracking()
                    .Include(o => o.Proveedor)
                    .FirstOrDefault(o => o.IdTenant == idTenant && o.NumOrden == numOrden));
        }
        catch (Exception ex)
        {
            return Result<OrdenCompra?>.Failure(
                CreateError(ex, nameof(GetByNumOrden))
                    .WithDetail("IdTenant", idTenant).WithDetail("NumOrden", numOrden));
        }
    }

    public Result<OrdenCompra?> GetWithDetalles(long idOrdenCompra)
    {
        if (Guard.Fail<OrdenCompra?>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } err) return err;
        try
        {
            return Result<OrdenCompra?>.Success(
                DbSet.AsNoTracking()
                    .Include(o => o.Proveedor)
                    // IX_OCDet_OC (IdOrdenCompra) — seek por OC
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.PresProd)
                            .ThenInclude(p => p.VarProd)
                                .ThenInclude(v => v.Prod)
                    .Include(o => o.Detalles)
                        .ThenInclude(d => d.PresProd.Unidad)
                    .FirstOrDefault(o => o.IdOrdenCompra == idOrdenCompra));
        }
        catch (Exception ex)
        {
            return Result<OrdenCompra?>.Failure(
                CreateError(ex, nameof(GetWithDetalles)).WithDetail("IdOrdenCompra", idOrdenCompra));
        }
    }

    public Result<IReadOnlyList<OrdenCompra>> GetPendientesVencimiento(int idTenant, int diasAlerta = 3)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<OrdenCompra>>(Guard.Rango(nameof(diasAlerta), diasAlerta, 1, 365)) is { } errD) return errD;
        try
        {
            // FecEntrega es DATE NULL → DateOnly? en EF Core
            var limiteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(diasAlerta));
            return Result<IReadOnlyList<OrdenCompra>>.Success(
                DbSet.AsNoTracking()
                    .Where(o => o.IdTenant == idTenant
                             && (o.Estado == EstadoPendiente || o.Estado == EstadoAprobada)
                             && o.FecEntrega.HasValue
                             && o.FecEntrega.Value <= limiteDate)
                    .Include(o => o.Proveedor)
                    .OrderBy(o => o.FecEntrega)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompra>>.Failure(
                CreateError(ex, nameof(GetPendientesVencimiento))
                    .WithDetail("IdTenant", idTenant).WithDetail("DiasAlerta", diasAlerta));
        }
    }

    public Result<bool> ExistsByNumOrden(int idTenant, string numOrden)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(numOrden), numOrden)) is { } errStr) return errStr;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(o => o.IdTenant == idTenant && o.NumOrden == numOrden));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByNumOrden))
                    .WithDetail("IdTenant", idTenant).WithDetail("NumOrden", numOrden));
        }
    }

    public Result<IReadOnlyDictionary<byte, decimal>> GetTotalPorEstado(int idTenant)
    {
        if (Guard.Fail<IReadOnlyDictionary<byte, decimal>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var totales = DbSet.Where(o => o.IdTenant == idTenant)
                .GroupBy(o => o.Estado)
                .Select(g => new { Estado = g.Key, Total = g.Sum(o => o.Total) })
                .ToList();
            return Result<IReadOnlyDictionary<byte, decimal>>.Success(
                totales.ToDictionary(x => x.Estado, x => x.Total));
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyDictionary<byte, decimal>>.Failure(
                CreateError(ex, nameof(GetTotalPorEstado)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result RecibirOrdenCompra(RecibirOrdenCompraRequest request)
    {
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdTenant), request.IdTenant)) is { } errT) return errT;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdBodega), request.IdBodega)) is { } errB) return errB;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdOrdenCompra), request.IdOrdenCompra)) is { } errOC) return errOC;
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));
        return SpExecutor.RecibirOrdenCompra(Context, request);
    }
}
