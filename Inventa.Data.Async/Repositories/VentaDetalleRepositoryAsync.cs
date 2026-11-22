using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public sealed record ProductoVendidoResumen(
    long    IdPresProd,
    string  Descripcion,
    decimal TotalCantidad,
    decimal TotalSubtotal,
    int     NumeroTransacciones);

public sealed record MargenBrutoResumen(
    long    IdPresProd,
    string  Descripcion,
    decimal TotalVentas,
    decimal TotalCosto,
    decimal MargenBruto,
    decimal MargenPct);

public interface IVentaDetalleRepositoryAsync
{
    Task<IReadOnlyList<VentaDetalle>> GetByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<VentaDetalle?> GetByVentaYLineaAsync(long idVenta, int numLinea, CancellationToken ct = default);
    Task<IReadOnlyList<VentaDetalle>> GetByPresentacionAsync(long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<decimal> GetCantidadVendidaAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<decimal> GetSubtotalVentaAsync(long idVenta, CancellationToken ct = default);
    Task<int> CountLineasByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<IReadOnlyList<ProductoVendidoResumen>> GetTopProductosVendidosAsync(int idTenant, DateTime desde, DateTime hasta, int top = 10, CancellationToken ct = default);
    Task<IReadOnlyList<MargenBrutoResumen>> GetMargenBrutoAsync(int idTenant, DateTime desde, DateTime hasta, int top = 20, CancellationToken ct = default);
}

public class VentaDetalleRepositoryAsync : IVentaDetalleRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    private const int MaxTop = 200;

    public VentaDetalleRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<VentaDetalle>> GetByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.VentaDetalle
            .AsNoTracking()
            .Where(d => d.IdVenta == idVenta)
            .Include(d => d.PresProd)
                .ThenInclude(p => p.VarProd)
                    .ThenInclude(v => v.Prod)
            .Include(d => d.PresProd.Unidad)
            .OrderBy(d => d.NumLinea)
            .ToListAsync(ct);
    }

    public async Task<VentaDetalle?> GetByVentaYLineaAsync(long idVenta, int numLinea, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));
        if (numLinea <= 0) throw new ArgumentOutOfRangeException(nameof(numLinea));

        return await _ctx.VentaDetalle
            .AsNoTracking()
            .Include(d => d.PresProd)
            .FirstOrDefaultAsync(d => d.IdVenta == idVenta && d.NumLinea == numLinea, ct);
    }

    public async Task<IReadOnlyList<VentaDetalle>> GetByPresentacionAsync(long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.VentaDetalle
            .AsNoTracking()
            .Where(d => d.IdPresProd == idPresProd
                     && d.Venta!.FecVenta >= desde
                     && d.Venta!.FecVenta <= hasta
                     && d.Venta!.Estado == 1)
            .Include(d => d.Venta)
            .OrderByDescending(d => d.Venta!.FecVenta)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetCantidadVendidaAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.VentaDetalle
            .Where(d => d.IdPresProd == idPresProd
                     && d.Venta!.IdTenant == idTenant
                     && d.Venta!.FecVenta >= desde
                     && d.Venta!.FecVenta <= hasta
                     && d.Venta!.Estado == 1)
            .SumAsync(d => d.Cantidad, ct);
    }

    public async Task<decimal> GetSubtotalVentaAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.VentaDetalle
            .Where(d => d.IdVenta == idVenta)
            .SumAsync(d => d.Subtotal, ct);
    }

    public async Task<int> CountLineasByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.VentaDetalle
            .CountAsync(d => d.IdVenta == idVenta, ct);
    }

    public async Task<IReadOnlyList<ProductoVendidoResumen>> GetTopProductosVendidosAsync(int idTenant, DateTime desde, DateTime hasta, int top = 10, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));
        if (top < 1 || top > MaxTop) throw new ArgumentOutOfRangeException(nameof(top));

        var agrupados = await _ctx.VentaDetalle
            .AsNoTracking()
            .Where(d => d.Venta!.IdTenant == idTenant
                     && d.Venta!.FecVenta >= desde
                     && d.Venta!.FecVenta <= hasta
                     && d.Venta!.Estado == 1)
            .GroupBy(d => d.IdPresProd)
            .Select(g => new
            {
                IdPresProd = g.Key,
                TotalCantidad = g.Sum(d => d.Cantidad),
                TotalSubtotal = g.Sum(d => d.Subtotal),
                NumeroTransacciones = g.Select(d => d.IdVenta).Distinct().Count()
            })
            .OrderByDescending(r => r.TotalSubtotal)
            .Take(top)
            .ToListAsync(ct);

        if (agrupados.Count == 0)
            return [];

        var ids = agrupados.Select(r => r.IdPresProd).ToList();
        var descrips = await _ctx.Presentaciones
            .AsNoTracking()
            .Where(p => ids.Contains(p.IdPresProd))
            .Select(p => new { p.IdPresProd, p.Descrip })
            .ToDictionaryAsync(p => p.IdPresProd, p => p.Descrip, ct);

        return agrupados
            .Select(r => new ProductoVendidoResumen(
                r.IdPresProd,
                descrips.GetValueOrDefault(r.IdPresProd, "—"),
                r.TotalCantidad,
                r.TotalSubtotal,
                r.NumeroTransacciones))
            .ToList();
    }

    public async Task<IReadOnlyList<MargenBrutoResumen>> GetMargenBrutoAsync(int idTenant, DateTime desde, DateTime hasta, int top = 20, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));
        if (top < 1 || top > MaxTop) throw new ArgumentOutOfRangeException(nameof(top));

        var agrupados = await _ctx.VentaDetalle
            .AsNoTracking()
            .Where(d => d.Venta!.IdTenant == idTenant
                     && d.Venta!.FecVenta >= desde
                     && d.Venta!.FecVenta <= hasta
                     && d.Venta!.Estado == 1)
            .GroupBy(d => d.IdPresProd)
            .Select(g => new
            {
                IdPresProd = g.Key,
                TotalVentas = g.Sum(d => d.Subtotal),
                TotalCosto = g.Sum(d => d.CostoUnitario * d.Cantidad)
            })
            .OrderByDescending(r => r.TotalVentas - r.TotalCosto)
            .Take(top)
            .ToListAsync(ct);

        if (agrupados.Count == 0)
            return [];

        var ids = agrupados.Select(r => r.IdPresProd).ToList();
        var descrips = await _ctx.Presentaciones
            .AsNoTracking()
            .Where(p => ids.Contains(p.IdPresProd))
            .Select(p => new { p.IdPresProd, p.Descrip })
            .ToDictionaryAsync(p => p.IdPresProd, p => p.Descrip, ct);

        return agrupados
            .Select(r =>
            {
                var margen = r.TotalVentas - r.TotalCosto;
                var pct = r.TotalVentas == 0m
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
    }
}



