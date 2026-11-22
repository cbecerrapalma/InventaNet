using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

/// <summary>Proyección para el ranking de productos más vendidos.</summary>
public sealed record ProductoVendidoResumen(
    long    IdPresProd,
    string  Descripcion,
    decimal TotalCantidad,
    decimal TotalSubtotal,
    int     NumeroTransacciones);

/// <summary>
/// Proyección para análisis de margen bruto por presentación.
/// CostoUnitario es el costo real registrado en VentaDetalle al momento de la venta.
/// MargenPct = (Venta - Costo) / Venta × 100.
/// </summary>
public sealed record MargenBrutoResumen(
    long    IdPresProd,
    string  Descripcion,
    decimal TotalVentas,
    decimal TotalCosto,
    decimal MargenBruto,
    decimal MargenPct);

public interface IVentaDetalleRepository : IRepositorySync<VentaDetalle>
{
    Result<IReadOnlyList<VentaDetalle>> GetByVenta(long idVenta);
    Result<VentaDetalle?> GetByVentaYLinea(long idVenta, int numLinea);
    Result<IReadOnlyList<VentaDetalle>> GetByPresentacion(long idPresProd, DateTime desde, DateTime hasta);
    Result<decimal> GetCantidadVendida(int idTenant, long idPresProd, DateTime desde, DateTime hasta);
    Result<decimal> GetSubtotalVenta(long idVenta);
    Result<int> CountLineasByVenta(long idVenta);

    /// <summary>
    /// Ranking de los N productos más vendidos por monto en el período.
    /// Solo ventas completadas (Estado=1).
    /// </summary>
    Result<IReadOnlyList<ProductoVendidoResumen>> GetTopProductosVendidos(int idTenant, DateTime desde, DateTime hasta, int top = 10);

    /// <summary>
    /// Margen bruto por presentación: usa CostoUnitario del VentaDetalle
    /// (costo real al momento de la venta, no el CostoProm actual de VarProd).
    /// Solo ventas completadas (Estado=1).
    /// </summary>
    Result<IReadOnlyList<MargenBrutoResumen>> GetMargenBruto(int idTenant, DateTime desde, DateTime hasta, int top = 20);
}

public class VentaDetalleRepository : RepositorySync<VentaDetalle>, IVentaDetalleRepository
{
    private const int MaxTop = 200;

    public VentaDetalleRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<VentaDetalle>> GetByVenta(long idVenta)
    {
        if (Guard.Fail<IReadOnlyList<VentaDetalle>>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            // UQ_VentaDetalle_Cluster (IdVenta, NumLinea) — clustered, seek eficiente
            return Result<IReadOnlyList<VentaDetalle>>.Success(
                DbSet.AsNoTracking()
                    .Where(d => d.IdVenta == idVenta)
                    .Include(d => d.PresProd)
                        .ThenInclude(p => p.VarProd)
                            .ThenInclude(v => v.Prod)
                    .Include(d => d.PresProd.Unidad)
                    .OrderBy(d => d.NumLinea)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VentaDetalle>>.Failure(
                CreateError(ex, nameof(GetByVenta)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<VentaDetalle?> GetByVentaYLinea(long idVenta, int numLinea)
    {
        if (Guard.Fail<VentaDetalle?>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } errV) return errV;
        if (Guard.Fail<VentaDetalle?>(Guard.IdInvalido(nameof(numLinea), numLinea)) is { } errL) return errL;
        try
        {
            // UQ_VentaDetalle_Cluster es clustered → seek de punto exacto
            return Result<VentaDetalle?>.Success(
                DbSet.AsNoTracking()
                    .Include(d => d.PresProd)
                    .FirstOrDefault(d => d.IdVenta == idVenta && d.NumLinea == numLinea));
        }
        catch (Exception ex)
        {
            return Result<VentaDetalle?>.Failure(
                CreateError(ex, nameof(GetByVentaYLinea))
                    .WithDetail("IdVenta",  idVenta)
                    .WithDetail("NumLinea", numLinea));
        }
    }

    public Result<IReadOnlyList<VentaDetalle>> GetByPresentacion(
        long idPresProd, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<VentaDetalle>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<IReadOnlyList<VentaDetalle>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            // IX_VentaDetalle_Reporte (IdPresProd, IdVenta) INCLUDE (Cantidad, PrecioUnitario, CostoUnitario, Descuento)
            return Result<IReadOnlyList<VentaDetalle>>.Success(
                DbSet.AsNoTracking()
                    .Where(d => d.IdPresProd     == idPresProd
                             && d.Venta.FecVenta >= desde
                             && d.Venta.FecVenta <= hasta
                             && d.Venta.Estado   == 1)
                    .Include(d => d.Venta)
                    .OrderByDescending(d => d.Venta.FecVenta)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VentaDetalle>>.Failure(
                CreateError(ex, nameof(GetByPresentacion)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<decimal> GetCantidadVendida(
        int idTenant, long idPresProd, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<decimal>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            // IX_VentaDetalle_Reporte INCLUDE (Cantidad) — cubriente para esta proyección
            return Result<decimal>.Success(
                DbSet.Where(d => d.IdPresProd      == idPresProd
                              && d.Venta.IdTenant  == idTenant
                              && d.Venta.FecVenta  >= desde
                              && d.Venta.FecVenta  <= hasta
                              && d.Venta.Estado    == 1)
                     .Sum(d => d.Cantidad));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetCantidadVendida))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<decimal> GetSubtotalVenta(long idVenta)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            return Result<decimal>.Success(DbSet.Where(d => d.IdVenta == idVenta).Sum(d => d.Subtotal));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetSubtotalVenta)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<int> CountLineasByVenta(long idVenta)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            return Result<int>.Success(DbSet.Count(d => d.IdVenta == idVenta));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountLineasByVenta)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<IReadOnlyList<ProductoVendidoResumen>> GetTopProductosVendidos(
        int idTenant, DateTime desde, DateTime hasta, int top = 10)
    {
        if (Guard.Fail<IReadOnlyList<ProductoVendidoResumen>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<ProductoVendidoResumen>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        if (Guard.Fail<IReadOnlyList<ProductoVendidoResumen>>(Guard.Rango(nameof(top), top, 1, MaxTop)) is { } errTop) return errTop;
        try
        {
            // OPTIMIZACIÓN: IX_VentaDetalle_Reporte (IdPresProd, IdVenta)
            // INCLUDE (Cantidad, PrecioUnitario, CostoUnitario, Descuento, Subtotal)
            // Descrip NO está en el INCLUDE — agrupar con Descrip en el GROUP BY
            // obliga al motor a hacer un join adicional fuera del índice.
            // Solución: agrupar solo por IdPresProd (cubierto por el INCLUDE),
            // luego enriquecer con Descrip en un segundo paso contra PresProd (tabla pequeña).
            var agrupados = DbSet.AsNoTracking()
                .Where(d => d.Venta.IdTenant == idTenant
                         && d.Venta.FecVenta >= desde
                         && d.Venta.FecVenta <= hasta
                         && d.Venta.Estado   == 1)
                .GroupBy(d => d.IdPresProd)
                .Select(g => new
                {
                    IdPresProd          = g.Key,
                    TotalCantidad       = g.Sum(d => d.Cantidad),
                    TotalSubtotal       = g.Sum(d => d.Subtotal),
                    NumeroTransacciones = g.Select(d => d.IdVenta).Distinct().Count()
                })
                .OrderByDescending(r => r.TotalSubtotal)
                .Take(top)
                .ToList();

            if (agrupados.Count == 0)
                return Result<IReadOnlyList<ProductoVendidoResumen>>.Success([]);

            // Segundo paso: enriquecer con Descrip desde PresProd
            // Solo N filas (top ≤ 200) — lookup puntual sin scan completo
            var ids     = agrupados.Select(r => r.IdPresProd).ToList();
            var descrips = Context.Set<PresProd>()
                .AsNoTracking()
                .Where(p => ids.Contains(p.IdPresProd))
                .Select(p => new { p.IdPresProd, p.Descrip })
                .ToDictionary(p => p.IdPresProd, p => p.Descrip);

            var resultado = agrupados
                .Select(r => new ProductoVendidoResumen(
                    r.IdPresProd,
                    descrips.GetValueOrDefault(r.IdPresProd, "—"),
                    r.TotalCantidad,
                    r.TotalSubtotal,
                    r.NumeroTransacciones))
                .ToList();

            return Result<IReadOnlyList<ProductoVendidoResumen>>.Success(resultado.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProductoVendidoResumen>>.Failure(
                CreateError(ex, nameof(GetTopProductosVendidos))
                    .WithDetail("IdTenant", idTenant).WithDetail("Top", top));
        }
    }

    public Result<IReadOnlyList<MargenBrutoResumen>> GetMargenBruto(
        int idTenant, DateTime desde, DateTime hasta, int top = 20)
    {
        if (Guard.Fail<IReadOnlyList<MargenBrutoResumen>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MargenBrutoResumen>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        if (Guard.Fail<IReadOnlyList<MargenBrutoResumen>>(Guard.Rango(nameof(top), top, 1, MaxTop)) is { } errTop) return errTop;
        try
        {
            // Mismo patrón de optimización: agrupar sin Descrip, enriquecer post-GROUP BY
            // IX_VentaDetalle_Reporte INCLUDE (Cantidad, PrecioUnitario, CostoUnitario, Descuento)
            var agrupados = DbSet.AsNoTracking()
                .Where(d => d.Venta.IdTenant == idTenant
                         && d.Venta.FecVenta >= desde
                         && d.Venta.FecVenta <= hasta
                         && d.Venta.Estado   == 1)
                .GroupBy(d => d.IdPresProd)
                .Select(g => new
                {
                    IdPresProd  = g.Key,
                    TotalVentas = g.Sum(d => d.Subtotal),
                    TotalCosto  = g.Sum(d => d.CostoUnitario * d.Cantidad)
                })
                .OrderByDescending(r => r.TotalVentas - r.TotalCosto)
                .Take(top)
                .ToList();

            if (agrupados.Count == 0)
                return Result<IReadOnlyList<MargenBrutoResumen>>.Success([]);

            var ids     = agrupados.Select(r => r.IdPresProd).ToList();
            var descrips = Context.Set<PresProd>()
                .AsNoTracking()
                .Where(p => ids.Contains(p.IdPresProd))
                .Select(p => new { p.IdPresProd, p.Descrip })
                .ToDictionary(p => p.IdPresProd, p => p.Descrip);

            // Calcular MargenPct en memoria — evita división por cero en SQL
            var resultado = agrupados
                .Select(r =>
                {
                    var margen = r.TotalVentas - r.TotalCosto;
                    var pct    = r.TotalVentas == 0m
                        ? 0m
                        : Math.Round(margen / r.TotalVentas * 100, 2);
                    return new MargenBrutoResumen(
                        r.IdPresProd,
                        descrips.GetValueOrDefault(r.IdPresProd, "—"),
                        r.TotalVentas,
                        r.TotalCosto,
                        margen,
                        pct);
                })
                .ToList();

            return Result<IReadOnlyList<MargenBrutoResumen>>.Success(resultado.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MargenBrutoResumen>>.Failure(
                CreateError(ex, nameof(GetMargenBruto)).WithDetail("IdTenant", idTenant));
        }
    }
}
