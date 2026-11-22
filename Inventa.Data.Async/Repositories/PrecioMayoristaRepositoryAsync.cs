using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

// S57.69: mayorista normalizado (relación dedicada, sin discriminator).
// Existe aquí = mayorista. IX_PrecioMayorista_Fast (IdPresProd, FecVig DESC).
public interface IPrecioMayoristaRepositoryAsync
{
    Task<PrecioMayorista?> GetByIdAsync(long idPrecioMayorista, CancellationToken ct = default);
    Task<PrecioMayorista?> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioMayorista>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioMayorista>> GetHistorialAsync(long idPresProd, CancellationToken ct = default);
    Task<bool> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<decimal?> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioMayorista>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default);
    Task<PrecioMayorista> InsertAsync(PrecioMayorista entity, CancellationToken ct = default);
    Task<PrecioMayorista> UpdateAsync(PrecioMayorista entity, CancellationToken ct = default);
    Task<bool> DeleteAsync(long idPrecioMayorista, CancellationToken ct = default);
}

public class PrecioMayoristaRepositoryAsync : IPrecioMayoristaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PrecioMayoristaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<PrecioMayorista?> GetByIdAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        if (idPrecioMayorista <= 0) throw new ArgumentOutOfRangeException(nameof(idPrecioMayorista));

        return await _ctx.PreciosMayorista.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPrecioMayorista == idPrecioMayorista, ct);
    }

    public async Task<PrecioMayorista?> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.PreciosMayorista.AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd
                     && p.Activo
                     && p.FecVig <= fechaRef)
            .OrderByDescending(p => p.FecVig)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PrecioMayorista>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.PreciosMayorista.AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd && p.Activo)
            .OrderByDescending(p => p.FecVig)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PrecioMayorista>> GetHistorialAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.PreciosMayorista.AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd)
            .OrderByDescending(p => p.FecVig)
            .ToListAsync(ct);
    }

    public async Task<bool> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.PreciosMayorista
            .AnyAsync(p => p.IdPresProd == idPresProd
                        && p.Activo
                        && p.FecVig <= fechaRef, ct);
    }

    public async Task<decimal?> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.PreciosMayorista
            .Where(p => p.IdPresProd == idPresProd
                     && p.Activo
                     && p.FecVig <= fechaRef)
            .OrderByDescending(p => p.FecVig)
            .Select(p => (decimal?)p.Valor)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<PrecioMayorista>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        var ids = idsPresProd.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _ctx.PreciosMayorista.AsNoTracking()
            .Where(pc => ids.Contains(pc.IdPresProd) && pc.Activo && pc.FecVig <= today)
            .OrderByDescending(pc => pc.FecVig).ToListAsync(ct);
    }

    public async Task<PrecioMayorista> InsertAsync(PrecioMayorista entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.PreciosMayorista.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<PrecioMayorista> UpdateAsync(PrecioMayorista entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.PreciosMayorista.Update(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<bool> DeleteAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        if (idPrecioMayorista <= 0) throw new ArgumentOutOfRangeException(nameof(idPrecioMayorista));
        var e = await _ctx.PreciosMayorista.FirstOrDefaultAsync(p => p.IdPrecioMayorista == idPrecioMayorista, ct);
        if (e == null) return false;
        _ctx.PreciosMayorista.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }
}
