using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IVentaRepository : IRepositorySync<Venta>
{
    Result<IReadOnlyList<Venta>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<IReadOnlyList<Venta>> GetByCliente(int idTenant, int idCliente);
    Result<IReadOnlyList<Venta>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta);
    Result<Venta?> GetByNumVenta(int idTenant, string numVenta);
    Result<Venta?> GetWithDetalles(long idVenta);
    Result<decimal> GetTotalVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<int> CountVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<bool> ExistsByNumVenta(int idTenant, string numVenta);

    /// <summary>
    /// Resumen de ventas completadas agrupado por TipoDoc (Boleta/Factura/Nota de Venta).
    /// Útil para cuadre de caja al cierre del día.
    /// Solo incluye Estado=1 (Completadas).
    /// </summary>
    Result<IReadOnlyList<ResumenVentasPorTipoDoc>> GetResumenPorTipoDoc(int idTenant, DateTime desde, DateTime hasta);

    /// <summary>
    /// Ventas anuladas en el período con motivo y fecha de anulación.
    /// Usa los campos Venta.Estado=2, Venta.FecAnulacion y Venta.MotivoAnulacion.
    /// Útil para auditoría, reportes de conciliación y detección de patrones anómalos.
    /// </summary>
    Result<IReadOnlyList<Venta>> GetAnuladas(int idTenant, DateTime desde, DateTime hasta);

    /// <summary>
    /// Ejecuta sp_RegistrarVenta: inserta cabecera, líneas, pagos y actualiza Stock
    /// en una única transacción atómica en SQL Server.
    /// </summary>
    Result<RegistrarVentaResult> RegistrarVenta(RegistrarVentaRequest request);

    /// <summary>
    /// Ejecuta sp_AnularVenta: revierte MovInv, Stock y StockDisp y marca la venta
    /// como anulada (Estado=2), registrando FecAnulacion y MotivoAnulacion.
    /// Solo se pueden anular ventas en Estado=1 (Completada).
    /// </summary>
    Result AnularVenta(AnularVentaRequest request);
}

/// <summary>Proyección para resumen de ventas por tipo de documento.</summary>
public sealed record ResumenVentasPorTipoDoc(
    byte    TipoDoc,
    int     CantVentas,
    decimal TotalSubTotal,
    decimal TotalDescuento,
    decimal TotalImpuesto,
    decimal TotalFinal);

public class VentaRepository : RepositorySync<Venta>, IVentaRepository
{
    // Estado: 1=Completada, 2=Anulada (solo 2 valores según el modelo)
    private const byte EstadoVentaMin = 1;
    private const byte EstadoVentaMax = 2;

    // TipoDoc: 1=Boleta, 2=Factura, 3=Nota de Venta
    private const byte TipoDocMin = 1;
    private const byte TipoDocMax = 3;

    public VentaRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Venta>> GetByRangoFecha(
        int idTenant, DateTime desde, DateTime hasta, byte? estado = null)
    {
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        if (estado.HasValue)
            if (Guard.Fail<IReadOnlyList<Venta>>(Guard.EstadoInvalido(nameof(estado), estado.Value, EstadoVentaMin, EstadoVentaMax)) is { } errE) return errE;
        try
        {
            // IX_Venta_Fecha (IdTenant, FecVenta DESC) INCLUDE (Total, Estado)
            var query = DbSet.AsNoTracking()
                .Where(v => v.IdTenant == idTenant
                         && v.FecVenta >= desde
                         && v.FecVenta <= hasta);
            if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);
            return Result<IReadOnlyList<Venta>>.Success(
                query.Include(v => v.Cliente)
                     .Include(v => v.Bodega)
                     .OrderByDescending(v => v.FecVenta)
                     .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Venta>>.Failure(
                CreateError(ex, nameof(GetByRangoFecha)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Venta>> GetByCliente(int idTenant, int idCliente)
    {
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idCliente), idCliente)) is { } errC) return errC;
        try
        {
            // IX_Venta_Cliente (IdCliente, FecVenta DESC) WHERE IdCliente IS NOT NULL
            return Result<IReadOnlyList<Venta>>.Success(
                DbSet.AsNoTracking()
                    .Where(v => v.IdTenant == idTenant && v.IdCliente == idCliente)
                    .Include(v => v.Bodega)
                    .OrderByDescending(v => v.FecVenta)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Venta>>.Failure(
                CreateError(ex, nameof(GetByCliente))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdCliente", idCliente));
        }
    }

    public Result<IReadOnlyList<Venta>> GetByBodega(
        int idTenant, int idBodega, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            // IX_Venta_Bodega (IdBodega, FecVenta DESC)
            return Result<IReadOnlyList<Venta>>.Success(
                DbSet.AsNoTracking()
                    .Where(v => v.IdTenant == idTenant
                             && v.IdBodega == idBodega
                             && v.FecVenta >= desde
                             && v.FecVenta <= hasta)
                    .Include(v => v.Cliente)
                    .OrderByDescending(v => v.FecVenta)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Venta>>.Failure(
                CreateError(ex, nameof(GetByBodega))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }

    public Result<Venta?> GetByNumVenta(int idTenant, string numVenta)
    {
        if (Guard.Fail<Venta?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Venta?>(Guard.StringRequerido(nameof(numVenta), numVenta)) is { } errStr) return errStr;
        try
        {
            // UQ_Venta_Num (IdTenant, NumVenta) — seek de punto único
            return Result<Venta?>.Success(
                DbSet.AsNoTracking()
                    .Include(v => v.Cliente)
                    .Include(v => v.Bodega)
                    .FirstOrDefault(v => v.IdTenant == idTenant && v.NumVenta == numVenta));
        }
        catch (Exception ex)
        {
            return Result<Venta?>.Failure(
                CreateError(ex, nameof(GetByNumVenta))
                    .WithDetail("IdTenant", idTenant).WithDetail("NumVenta", numVenta));
        }
    }

    public Result<Venta?> GetWithDetalles(long idVenta)
    {
        if (Guard.Fail<Venta?>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            return Result<Venta?>.Success(
                DbSet.AsNoTracking()
                    .Include(v => v.Cliente)
                    .Include(v => v.Bodega)
                    // UQ_VentaDetalle_Cluster (IdVenta, NumLinea) — clustered
                    .Include(v => v.Detalles.OrderBy(d => d.NumLinea))
                        .ThenInclude(d => d.PresProd)
                            .ThenInclude(p => p.VarProd)
                                .ThenInclude(vp => vp.Prod)
                    .Include(v => v.Detalles)
                        .ThenInclude(d => d.PresProd.Unidad)
                    .Include(v => v.Pagos)
                        .ThenInclude(p => p.MetodoPago)
                    .FirstOrDefault(v => v.IdVenta == idVenta));
        }
        catch (Exception ex)
        {
            return Result<Venta?>.Failure(
                CreateError(ex, nameof(GetWithDetalles)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<decimal> GetTotalVentas(
        int idTenant, DateTime desde, DateTime hasta, byte? estado = null)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        if (estado.HasValue)
            if (Guard.Fail<decimal>(Guard.EstadoInvalido(nameof(estado), estado.Value, EstadoVentaMin, EstadoVentaMax)) is { } errE) return errE;
        try
        {
            // IX_Venta_Fecha INCLUDE (Total, Estado) — columnas cubrientes
            var query = DbSet.Where(v => v.IdTenant == idTenant
                                      && v.FecVenta >= desde
                                      && v.FecVenta <= hasta);
            if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);
            return Result<decimal>.Success(query.Sum(v => v.Total));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetTotalVentas)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<int> CountVentas(
        int idTenant, DateTime desde, DateTime hasta, byte? estado = null)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<int>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        if (estado.HasValue)
            if (Guard.Fail<int>(Guard.EstadoInvalido(nameof(estado), estado.Value, EstadoVentaMin, EstadoVentaMax)) is { } errE) return errE;
        try
        {
            var query = DbSet.Where(v => v.IdTenant == idTenant
                                      && v.FecVenta >= desde
                                      && v.FecVenta <= hasta);
            if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);
            return Result<int>.Success(query.Count());
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountVentas)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<bool> ExistsByNumVenta(int idTenant, string numVenta)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(numVenta), numVenta)) is { } errStr) return errStr;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(v => v.IdTenant == idTenant && v.NumVenta == numVenta));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByNumVenta))
                    .WithDetail("IdTenant", idTenant).WithDetail("NumVenta", numVenta));
        }
    }

    public Result<IReadOnlyList<ResumenVentasPorTipoDoc>> GetResumenPorTipoDoc(
        int idTenant, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<ResumenVentasPorTipoDoc>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<ResumenVentasPorTipoDoc>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            var resultado = DbSet
                .Where(v => v.IdTenant == idTenant
                         && v.FecVenta >= desde
                         && v.FecVenta <= hasta
                         && v.Estado   == 1)   // solo completadas para el cuadre
                .GroupBy(v => v.TipoDoc)
                .Select(g => new ResumenVentasPorTipoDoc(
                    g.Key,
                    g.Count(),
                    g.Sum(v => v.SubTotal),
                    g.Sum(v => v.Descuento),
                    g.Sum(v => v.Impuesto),
                    g.Sum(v => v.Total)))
                .OrderBy(r => r.TipoDoc)
                .ToList();
            return Result<IReadOnlyList<ResumenVentasPorTipoDoc>>.Success(resultado.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ResumenVentasPorTipoDoc>>.Failure(
                CreateError(ex, nameof(GetResumenPorTipoDoc)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Venta>> GetAnuladas(int idTenant, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Venta>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            // Estado=2 (Anulada); filtro por FecVenta del período (no por FecAnulacion)
            // para alinear con los reportes de caja del mismo período.
            // Se proyectan FecAnulacion y MotivoAnulacion — campos ignorados en el resto del repo.
            return Result<IReadOnlyList<Venta>>.Success(
                DbSet.AsNoTracking()
                    .Where(v => v.IdTenant == idTenant
                             && v.FecVenta >= desde
                             && v.FecVenta <= hasta
                             && v.Estado   == 2)
                    .Include(v => v.Cliente)
                    .Include(v => v.Bodega)
                    // Ordenar por fecha de anulación — más recientes primero
                    .OrderByDescending(v => v.FecAnulacion)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Venta>>.Failure(
                CreateError(ex, nameof(GetAnuladas)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<RegistrarVentaResult> RegistrarVenta(RegistrarVentaRequest request)
    {
        if (Guard.Fail<RegistrarVentaResult>(Guard.IdInvalido(nameof(request.IdTenant), request.IdTenant)) is { } errT) return errT;
        if (Guard.Fail<RegistrarVentaResult>(Guard.IdInvalido(nameof(request.IdBodega), request.IdBodega)) is { } errB) return errB;
        if (Guard.Fail<RegistrarVentaResult>(Guard.StringRequerido(nameof(request.NumVenta), request.NumVenta)) is { } errN) return errN;
        if (Guard.Fail<RegistrarVentaResult>(Guard.EstadoInvalido(nameof(request.TipoDoc), request.TipoDoc, TipoDocMin, TipoDocMax)) is { } errD) return errD;
        if (request.Items is null || request.Items.Count == 0)
            return Result<RegistrarVentaResult>.Failure(CommonErrors.Validation.Required(nameof(request.Items)));
        if (request.Pagos is null || request.Pagos.Count == 0)
            return Result<RegistrarVentaResult>.Failure(CommonErrors.Validation.Required(nameof(request.Pagos)));

        // Validar cantidades positivas por ítem
        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (Guard.Fail<RegistrarVentaResult>(Guard.CantidadPositiva($"Items[{i}].Cantidad", item.Cantidad)) is { } errCant)
                return errCant;
        }

        return SpExecutor.RegistrarVenta(Context, request);
    }

    public Result AnularVenta(AnularVentaRequest request)
    {
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdTenant), request.IdTenant)) is { } errT) return errT;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdVenta), request.IdVenta)) is { } errV) return errV;
        if (Guard.Fail(Guard.StringRequerido(nameof(request.MotivoAnulacion), request.MotivoAnulacion)) is { } errM) return errM;
        return SpExecutor.AnularVenta(Context, request);
    }
}
