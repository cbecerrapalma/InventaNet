using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public sealed record ResumenPagoVenta(
    byte    IdMetodoPago,
    string  NombreMetodo,
    decimal TotalMonto);

public interface IVentaPagoRepositoryAsync
{
    Task<IReadOnlyList<VentaPago>> GetByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<decimal> GetTotalPagadoAsync(long idVenta, CancellationToken ct = default);
    Task<IReadOnlyList<VentaPago>> GetByMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<decimal> GetTotalPorMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<ResumenPagoVenta>> GetResumenByVentaAsync(long idVenta, CancellationToken ct = default);
}

public class VentaPagoRepositoryAsync : IVentaPagoRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public VentaPagoRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<VentaPago>> GetByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.VentaPagos
            .AsNoTracking()
            .Where(p => p.IdVenta == idVenta)
            .Include(p => p.MetodoPago)
            .OrderBy(p => p.IdVentaPago)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalPagadoAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.VentaPagos
            .Where(p => p.IdVenta == idVenta)
            .SumAsync(p => p.Monto, ct);
    }

    public async Task<IReadOnlyList<VentaPago>> GetByMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idMetodoPago <= 0) throw new ArgumentOutOfRangeException(nameof(idMetodoPago));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.VentaPagos
            .AsNoTracking()
            .Where(p => p.Venta!.IdTenant == idTenant
                     && p.IdMetodoPago == idMetodoPago
                     && p.Venta!.FecVenta >= desde
                     && p.Venta!.FecVenta <= hasta)
            .Include(p => p.Venta)
            .OrderByDescending(p => p.Venta!.FecVenta)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetTotalPorMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idMetodoPago <= 0) throw new ArgumentOutOfRangeException(nameof(idMetodoPago));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.VentaPagos
            .Where(p => p.Venta!.IdTenant == idTenant
                     && p.IdMetodoPago == idMetodoPago
                     && p.Venta!.FecVenta >= desde
                     && p.Venta!.FecVenta <= hasta)
            .SumAsync(p => p.Monto, ct);
    }

    public async Task<IReadOnlyList<ResumenPagoVenta>> GetResumenByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        var resumen = await _ctx.VentaPagos
            .AsNoTracking()
            .Where(p => p.IdVenta == idVenta)
            .GroupBy(p => new { p.IdMetodoPago, p.MetodoPago!.Nombre })
            .Select(g => new ResumenPagoVenta(
                g.Key.IdMetodoPago,
                g.Key.Nombre,
                g.Sum(p => p.Monto)))
            .OrderBy(r => r.IdMetodoPago)
            .ToListAsync(ct);

        return resumen;
    }
}



