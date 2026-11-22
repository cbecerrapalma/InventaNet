using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IOrdenCompraDetalleRepositoryAsync
{
    Task<IReadOnlyList<OrdenCompraDetalle>> GetByOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<OrdenCompraDetalle?> GetByOrdenYPresentacionAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompraDetalle>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<OrdenCompraDetalle>> GetPendientesByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<decimal> GetCantidadPendienteAsync(long idPresProd, CancellationToken ct = default);
    Task<decimal> GetSubtotalOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<int> CountLineasByOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<bool> ExistsInOrdenAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default);
}

public class OrdenCompraDetalleRepositoryAsync : IOrdenCompraDetalleRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public OrdenCompraDetalleRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<OrdenCompraDetalle>> GetByOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));

        return await _ctx.OrdenCompraDetalle
            .AsNoTracking()
            .Where(d => d.IdOrdenCompra == idOrdenCompra)
            .Include(d => d.PresProd)
                .ThenInclude(p => p.VarProd)
                    .ThenInclude(v => v.Prod)
            .Include(d => d.PresProd.Unidad)
            .OrderBy(d => d.IdOCDetalle)
            .ToListAsync(ct);
    }

    public async Task<OrdenCompraDetalle?> GetByOrdenYPresentacionAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.OrdenCompraDetalle
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.IdOrdenCompra == idOrdenCompra && d.IdPresProd == idPresProd, ct);
    }

    public async Task<IReadOnlyList<OrdenCompraDetalle>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.OrdenCompraDetalle
            .AsNoTracking()
            .Where(d => d.IdPresProd == idPresProd)
            .Include(d => d.OrdenCompra)
            .OrderByDescending(d => d.OrdenCompra!.FecOrden)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<OrdenCompraDetalle>> GetPendientesByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.OrdenCompraDetalle
            .AsNoTracking()
            .Where(d => d.IdPresProd == idPresProd
                     && (d.OrdenCompra!.Estado == 1 || d.OrdenCompra!.Estado == 2))
            .Include(d => d.OrdenCompra)
                .ThenInclude(o => o!.Proveedor)
            .OrderByDescending(d => d.OrdenCompra!.FecOrden)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetCantidadPendienteAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.OrdenCompraDetalle
            .Where(d => d.IdPresProd == idPresProd
                     && (d.OrdenCompra!.Estado == 1 || d.OrdenCompra!.Estado == 2))
            .SumAsync(d => d.Cantidad, ct);
    }

    public async Task<decimal> GetSubtotalOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));

        return await _ctx.OrdenCompraDetalle
            .Where(d => d.IdOrdenCompra == idOrdenCompra)
            .SumAsync(d => d.Subtotal, ct);
    }

    public async Task<int> CountLineasByOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));

        return await _ctx.OrdenCompraDetalle
            .CountAsync(d => d.IdOrdenCompra == idOrdenCompra, ct);
    }

    public async Task<bool> ExistsInOrdenAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default)
    {
        if (idOrdenCompra <= 0) throw new ArgumentOutOfRangeException(nameof(idOrdenCompra));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.OrdenCompraDetalle
            .AnyAsync(d => d.IdOrdenCompra == idOrdenCompra && d.IdPresProd == idPresProd, ct);
    }
}



