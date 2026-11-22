using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IPrecioRepositoryAsync
{
    Task<Precio?> GetByIdAsync(long idPrecio, CancellationToken ct = default);
    Task<Precio?> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<IReadOnlyList<Precio>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<Precio>> GetHistorialAsync(long idPresProd, CancellationToken ct = default);
    Task<bool> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<decimal?> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    // S57.34: CRUD/listados/diagnóstico para ProductoServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Precio>> GetByPresProdOrRecentAsync(long? idPresProd, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<Precio> Items, int Total)> SearchPagedByTenantAsync(int idTenant, long? idPresProd, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyList<Precio>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default);
    Task<IReadOnlyList<long>> GetValidPriceProdIdsAsync(int idTenant, DateOnly today, CancellationToken ct = default);
    Task<Precio> InsertAsync(Precio entity, CancellationToken ct = default);
    // S57.62 M8.1: primitivas de persistencia para el servicio de escritura
    // async (el repo es dueño del commit, como InsertAsync).
    Task<Precio> UpdateAsync(Precio entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(long idPrecio, CancellationToken ct = default);
}

public class PrecioRepositoryAsync : IPrecioRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PrecioRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<Precio?> GetByIdAsync(long idPrecio, CancellationToken ct = default)
    {
        if (idPrecio <= 0) throw new ArgumentOutOfRangeException(nameof(idPrecio));

        return await _ctx.Precios.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPrecio == idPrecio, ct);
    }

    public async Task<Precio?> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.Precios
            .AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd
                     && p.Activo
                     && p.FecVig <= fechaRef)
            .OrderByDescending(p => p.FecVig)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Precio>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Precios
            .AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd && p.Activo)
            .OrderByDescending(p => p.FecVig)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Precio>> GetHistorialAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Precios
            .AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd)
            .OrderByDescending(p => p.FecVig)
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.Precios
            .AnyAsync(p => p.IdPresProd == idPresProd
                        && p.Activo
                        && p.FecVig <= fechaRef, ct);
    }

    public async Task<decimal?> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.Precios
            .Where(p => p.IdPresProd == idPresProd
                     && p.Activo
                     && p.FecVig <= fechaRef)
            .OrderByDescending(p => p.FecVig)
            .Select(p => (decimal?)p.Valor)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Precio>> GetByPresProdOrRecentAsync(long? idPresProd, int take, CancellationToken ct = default)
    {
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        var q = _ctx.Precios.AsNoTracking().AsQueryable();
        if (idPresProd.HasValue) q = q.Where(p => p.IdPresProd == idPresProd.Value);
        return await q.Take(take).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Precio> Items, int Total)> SearchPagedByTenantAsync(int idTenant, long? idPresProd, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var baseQ = _ctx.Precios.AsNoTracking().AsQueryable();
        if (idPresProd.HasValue) baseQ = baseQ.Where(p => p.IdPresProd == idPresProd.Value);
        var query = baseQ.Join(_ctx.Presentaciones, pr => pr.IdPresProd, pre => pre.IdPresProd, (pr, pre) => new { pr, pre })
            .Join(_ctx.Variantes, x => x.pre.IdVarProd, v => v.IdVarProd, (x, v) => new { x.pr, v })
            .Join(_ctx.Productos, x => x.v.IdProd, prod => prod.IdProd, (x, prod) => new { x.pr, prod })
            .Where(x => x.prod.IdTenant == idTenant).Select(x => x.pr);
        if (!string.IsNullOrWhiteSpace(like))
            query = query.Where(p => EF.Functions.Like(p.IdPresProd.ToString(), like!));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(p => p.FecVig).ThenBy(p => p.IdPrecio)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyList<Precio>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        var ids = idsPresProd.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _ctx.Precios.AsNoTracking()
            // S57.69: precio normal único; mayorista vive en PrecioMayorista.
            .Where(pc => ids.Contains(pc.IdPresProd) && pc.Activo && pc.FecVig <= today)
            .OrderByDescending(pc => pc.FecVig).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<long>> GetValidPriceProdIdsAsync(int idTenant, DateOnly today, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Precios.AsNoTracking()
            .Where(pc => pc.Activo && pc.FecVig <= today
                && _ctx.Presentaciones.Any(pp => pp.IdPresProd == pc.IdPresProd && pp.Activo
                    && _ctx.Variantes.Any(v => v.IdVarProd == pp.IdVarProd && v.Prod.IdTenant == idTenant)))
            .Select(pc => pc.PresProd.VarProd.IdProd).Distinct().ToListAsync(ct);
    }

    public async Task<Precio> InsertAsync(Precio entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Precios.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Precio> UpdateAsync(Precio entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Precios.Update(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(long idPrecio, CancellationToken ct = default)
    {
        if (idPrecio <= 0) throw new ArgumentOutOfRangeException(nameof(idPrecio));
        var e = await _ctx.Precios.FirstOrDefaultAsync(p => p.IdPrecio == idPrecio, ct);
        if (e == null) return false;
        _ctx.Precios.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}



