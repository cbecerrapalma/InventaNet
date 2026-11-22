using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>S116: fila del listado de órdenes de compra: los campos de la entidad + nombre del proveedor (contrato ampliado, solo campos nuevos).</summary>
public sealed record OrdenCompraListadoRow(
    long IdOrdenCompra, int IdTenant, int IdProveedor, string NumOrden, DateOnly FecOrden, DateOnly? FecEntrega, byte Estado,
    decimal Total, string? Observaciones, DateTime FecCrea, int? UsuarioCrea, int? UsuarioMod, string ProveedorNombre);

/// <summary>S116: línea de la ficha de orden de compra con producto y variante.</summary>
public sealed record OrdenCompraFichaLinea(
    long IdOCDetalle, long IdOrdenCompra, long IdPresProd, decimal Cantidad, decimal PrecioUnitario, decimal Subtotal,
    long IdProd, string ProdCodigo, string ProdNombre, string VarNombre);

/// <summary>S116: ficha de orden de compra: los campos de la entidad + proveedor y líneas.</summary>
public sealed record OrdenCompraFicha(
    long IdOrdenCompra, int IdTenant, int IdProveedor, string NumOrden, DateOnly FecOrden, DateOnly? FecEntrega, byte Estado,
    decimal Total, string? Observaciones, DateTime FecCrea, int? UsuarioCrea, int? UsuarioMod, string ProveedorNombre,
    IReadOnlyList<OrdenCompraFichaLinea> Detalles);

public interface IOrdenCompraRepositoryAsync
{
    Task<IReadOnlyList<OrdenCompra>> GetByTenantAsync(int idTenant, byte? estado = null, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompra>> GetByProveedorAsync(int idTenant, int idProveedor, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompra>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<OrdenCompra?> GetByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default);
    Task<OrdenCompra?> GetWithDetallesAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompra>> GetPendientesVencimientoAsync(int idTenant, int diasAlerta = 3, CancellationToken ct = default);
    Task<bool> ExistsByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default);
    Task<IReadOnlyDictionary<byte, decimal>> GetTotalPorEstadoAsync(int idTenant, CancellationToken ct = default);
    // S57.34: lecturas para Application.Async (listado reciente, búsqueda paginada, detalle por tenant).
    Task<IReadOnlyList<OrdenCompra>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<OrdenCompraListadoRow> Items, int TotalCount)> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<OrdenCompra?> GetByIdInTenantAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default);
    Task<OrdenCompraFicha?> GetFichaAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default);
    // S57.34: creación atómica cabecera+detalles (tx propiedad del repo).
    Task<OrdenCompra> CreateWithDetailsAsync(int idTenant, int idProveedor, DateOnly fecha, string? observacion, int idUsuario, string numOrden, IReadOnlyList<(long IdPresProd, decimal Cantidad, decimal PrecioUnitario)> detalles, CancellationToken ct = default);
    // S57.34: agregados para dashboard (sin contraparte sync).
    Task<(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total)> GetSummaryAsync(int idTenant, CancellationToken ct = default);
    Task<Result> RecibirOrdenCompraAsync(RecibirOrdenCompraCmd request, CancellationToken ct = default);
}

public class OrdenCompraRepositoryAsync : IOrdenCompraRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    // Estado: 1=Pendiente, 2=Aprobada, 3=Recibida, 4=Cancelada
    private const byte EstadoOCMin = 1;
    private const byte EstadoOCMax = 4;
    private const byte EstadoPendiente = 1;
    private const byte EstadoAprobada = 2;

    public OrdenCompraRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<OrdenCompra>> GetByTenantAsync(int idTenant, byte? estado = null, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (estado.HasValue && (estado.Value < EstadoOCMin || estado.Value > EstadoOCMax))
            throw new ArgumentOutOfRangeException(nameof(estado));

        var query = _ctx.OrdenesCompra.AsNoTracking().Where(o => o.IdTenant == idTenant);
        if (estado.HasValue) query = query.Where(o => o.Estado == estado.Value);

        return await query
            .Include(o => o.Proveedor)
            .OrderByDescending(o => o.FecOrden)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdenCompra>> GetByProveedorAsync(int idTenant, int idProveedor, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idProveedor <= 0) throw new ArgumentOutOfRangeException(nameof(idProveedor));

        return await _ctx.OrdenesCompra
            .AsNoTracking()
            .Where(o => o.IdTenant == idTenant && o.IdProveedor == idProveedor)
            .OrderByDescending(o => o.FecOrden)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdenCompra>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        var desdeDate = DateOnly.FromDateTime(desde);
        var hastaDate = DateOnly.FromDateTime(hasta);

        return await _ctx.OrdenesCompra
            .AsNoTracking()
            .Where(o => o.IdTenant == idTenant
                     && o.FecOrden >= desdeDate
                     && o.FecOrden <= hastaDate)
            .Include(o => o.Proveedor)
            .OrderByDescending(o => o.FecOrden)
            .ToListAsync(ct);
    }

    public async Task<OrdenCompra?> GetByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(numOrden)) throw new ArgumentException("Requerido", nameof(numOrden));

        return await _ctx.OrdenesCompra
            .AsNoTracking()
            .Include(o => o.Proveedor)
            .FirstOrDefaultAsync(o => o.IdTenant == idTenant && o.NumOrden == numOrden, ct);
    }

    public async Task<OrdenCompra?> GetWithDetallesAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));

        return await _ctx.OrdenesCompra
            .AsNoTracking()
            .Include(o => o.Proveedor)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.PresProd)
                    .ThenInclude(p => p.VarProd)
                        .ThenInclude(v => v.Prod)
            .Include(o => o.Detalles)
                .ThenInclude(d => d.PresProd.Unidad)
            .FirstOrDefaultAsync(o => o.IdOrdenCompra == idOrdenCompra, ct);
    }

    public async Task<IReadOnlyList<OrdenCompra>> GetPendientesVencimientoAsync(int idTenant, int diasAlerta = 3, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (diasAlerta < 1 || diasAlerta > 365) throw new ArgumentOutOfRangeException(nameof(diasAlerta));

        var limiteDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(diasAlerta));

        return await _ctx.OrdenesCompra
            .AsNoTracking()
            .Where(o => o.IdTenant == idTenant
                     && (o.Estado == EstadoPendiente || o.Estado == EstadoAprobada)
                     && o.FecEntrega.HasValue
                     && o.FecEntrega.Value <= limiteDate)
            .Include(o => o.Proveedor)
            .OrderBy(o => o.FecEntrega)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(numOrden)) throw new ArgumentException("Requerido", nameof(numOrden));

        return await _ctx.OrdenesCompra
            .AnyAsync(o => o.IdTenant == idTenant && o.NumOrden == numOrden, ct);
    }

    public async Task<IReadOnlyDictionary<byte, decimal>> GetTotalPorEstadoAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var totales = await _ctx.OrdenesCompra
            .Where(o => o.IdTenant == idTenant)
            .GroupBy(o => o.Estado)
            .Select(g => new { Estado = g.Key, Total = g.Sum(o => o.Total) })
            .ToListAsync(ct);

        return totales.ToDictionary(x => x.Estado, x => x.Total);
    }

    public async Task<IReadOnlyList<OrdenCompra>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        take = Math.Clamp(take, 1, 200);

        return await _ctx.OrdenesCompra.AsNoTracking()
            .Where(o => o.IdTenant == idTenant)
            .OrderByDescending(o => o.FecOrden)
            .ThenByDescending(o => o.IdOrdenCompra)
            .Take(take)
            .ToListAsync(ct);
    }

    // S116 D1: proyección del listado y de la cabecera de la ficha (nombre del proveedor resuelto en la BD).
    private static readonly System.Linq.Expressions.Expression<Func<OrdenCompra, OrdenCompraListadoRow>> ProyListado = o => new OrdenCompraListadoRow(
        o.IdOrdenCompra, o.IdTenant, o.IdProveedor, o.NumOrden, o.FecOrden, o.FecEntrega, o.Estado,
        o.Total, o.Observaciones, o.FecCrea, o.UsuarioCrea, o.UsuarioMod, o.Proveedor.RazonSocial);

    public async Task<(IReadOnlyList<OrdenCompraListadoRow> Items, int TotalCount)> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is not (20 or 50 or 100)) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = _ctx.OrdenesCompra.AsNoTracking().Where(o => o.IdTenant == idTenant);
        q = q?.Trim();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            query = query.Where(o => EF.Functions.Like(o.NumOrden, like)
                || (o.Observaciones != null && EF.Functions.Like(o.Observaciones, like))
                || EF.Functions.Like(o.Proveedor.RazonSocial, like));
        }
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(o => o.FecOrden)
            .ThenByDescending(o => o.IdOrdenCompra)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProyListado)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<OrdenCompra?> GetByIdInTenantAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.OrdenesCompra.AsNoTracking()
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.IdOrdenCompra == idOrdenCompra && o.IdTenant == idTenant, ct);
    }

    public async Task<OrdenCompraFicha?> GetFichaAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var h = await _ctx.OrdenesCompra.AsNoTracking()
            .Where(o => o.IdOrdenCompra == idOrdenCompra && o.IdTenant == idTenant)
            .Select(ProyListado)
            .FirstOrDefaultAsync(ct);
        if (h is null) return null;

        var detalles = await _ctx.OrdenCompraDetalle.AsNoTracking()
            .Where(d => d.IdOrdenCompra == idOrdenCompra)
            .OrderBy(d => d.IdOCDetalle)
            .Select(d => new OrdenCompraFichaLinea(d.IdOCDetalle, d.IdOrdenCompra, d.IdPresProd, d.Cantidad, d.PrecioUnitario, d.Subtotal,
                d.PresProd.VarProd.IdProd, d.PresProd.VarProd.Prod.Codigo, d.PresProd.VarProd.Prod.Nombre, d.PresProd.VarProd.Nombre))
            .ToListAsync(ct);

        return new OrdenCompraFicha(h.IdOrdenCompra, h.IdTenant, h.IdProveedor, h.NumOrden, h.FecOrden, h.FecEntrega, h.Estado,
            h.Total, h.Observaciones, h.FecCrea, h.UsuarioCrea, h.UsuarioMod, h.ProveedorNombre, detalles);
    }

    public async Task<OrdenCompra> CreateWithDetailsAsync(int idTenant, int idProveedor, DateOnly fecha, string? observacion, int idUsuario, string numOrden, IReadOnlyList<(long IdPresProd, decimal Cantidad, decimal PrecioUnitario)> detalles, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idProveedor <= 0) throw new ArgumentOutOfRangeException(nameof(idProveedor));
        if (string.IsNullOrWhiteSpace(numOrden)) throw new ArgumentException("Requerido", nameof(numOrden));
        ArgumentNullException.ThrowIfNull(detalles);

        var total = detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        var oc = new OrdenCompra
        {
            IdTenant = idTenant,
            IdProveedor = idProveedor,
            FecOrden = fecha,
            Observaciones = observacion,
            UsuarioCrea = idUsuario,
            Estado = EstadoPendiente,
            NumOrden = numOrden,
            Total = total
        };
        _ctx.OrdenesCompra.Add(oc);
        await _ctx.SaveChangesAsync(ct);

        if (detalles.Count > 0)
        {
            var entities = detalles.Select(d => new OrdenCompraDetalle
            {
                IdOrdenCompra = oc.IdOrdenCompra,
                IdPresProd = d.IdPresProd,
                Cantidad = d.Cantidad,
                PrecioUnitario = d.PrecioUnitario,
                Subtotal = d.Cantidad * d.PrecioUnitario
            }).ToList();
            _ctx.OrdenCompraDetalle.AddRange(entities);
            await _ctx.SaveChangesAsync(ct);
            await _ctx.Entry(oc).Collection(o => o.Detalles).LoadAsync(ct);
        }
        await tx.CommitAsync(ct);
        return oc;
    }

    public async Task<(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total)> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.OrdenesCompra.AsNoTracking().Where(o => o.IdTenant == idTenant);
        var registros = await query.CountAsync(ct);
        if (registros == 0) return (0, 0, 0, 0, 0m);
        var pend = await query.CountAsync(o => o.Estado == EstadoPendiente, ct);
        var rec = await query.CountAsync(o => o.Estado == 3, ct);
        var canc = await query.CountAsync(o => o.Estado == 4, ct);
        var tot = await query.SumAsync(o => o.Total, ct);
        return (registros, pend, rec, canc, tot);
    }

    public async Task<Result> RecibirOrdenCompraAsync(RecibirOrdenCompraCmd request, CancellationToken ct = default)
    {
        if (request.IdTenant <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdTenant), 1, int.MaxValue));
        if (request.IdBodega <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdBodega), 1, int.MaxValue));
        if (request.IdOrdenCompra <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdOrdenCompra), 1, long.MaxValue));
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));

        return await SpExecutorAsync.RecibirOrdenCompraAsync(_ctx, SpCommandMapper.ToData(request), ct);
    }
}



