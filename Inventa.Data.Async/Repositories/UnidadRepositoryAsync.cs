using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IUnidadRepositoryAsync
{
    Task<Unidad?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Unidad>> GetAllOrdenadoAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<bool> IsUsedByProductosAsync(int idUnidad, CancellationToken ct = default);
    // S57.34: lectura por id + mapa para ProductoServiceAsync (sin contraparte sync).
    Task<Unidad?> GetByIdAsync(int idUnidad, CancellationToken ct = default);
    Task<IReadOnlyDictionary<int, string>> GetMapAsync(CancellationToken ct = default);
}

public class UnidadRepositoryAsync : IUnidadRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public UnidadRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<Unidad?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Unidades
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Codigo == codigoNorm, ct);
    }

    public async Task<IReadOnlyList<Unidad>> GetAllOrdenadoAsync(CancellationToken ct = default)
    {
        return await _ctx.Unidades
            .AsNoTracking()
            .OrderBy(u => u.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Unidades
            .AnyAsync(u => u.Codigo == codigoNorm, ct);
    }

    public async Task<bool> IsUsedByProductosAsync(int idUnidad, CancellationToken ct = default)
    {
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        var isUsed = await _ctx.Set<Prod>().AnyAsync(p => p.IdUniBase == idUnidad, ct)
                   || await _ctx.Set<ProdConv>().AnyAsync(c => c.IdUnidad == idUnidad, ct)
                   || await _ctx.Set<PresProd>().AnyAsync(p => p.IdUnidad == idUnidad, ct);

        return isUsed;
    }

    public async Task<Unidad?> GetByIdAsync(int idUnidad, CancellationToken ct = default)
    {
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Unidades.AsNoTracking()
            .FirstOrDefaultAsync(u => u.IdUnidad == idUnidad, ct);
    }

    public async Task<IReadOnlyDictionary<int, string>> GetMapAsync(CancellationToken ct = default)
        => await _ctx.Unidades.AsNoTracking().ToDictionaryAsync(u => u.IdUnidad, u => u.Nombre, ct);
}



