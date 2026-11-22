using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IAtriDefRepositoryAsync
{
    Task<IReadOnlyList<AtriDef>> GetByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default);
    Task<AtriDef?> GetByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default);
    Task<AtriDef?> GetWithValoresAsync(long idAtriDef, CancellationToken ct = default);
    Task<IReadOnlyList<AtriDef>> GetObligatoriosByCategoriaAsync(long idCategoria, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default);
    Task<bool> TieneValoresAsync(long idAtriDef, CancellationToken ct = default);
    Task<int> CountByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default);
}

public class AtriDefRepositoryAsync : IAtriDefRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public AtriDefRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<AtriDef>> GetByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        var query = _ctx.AtributosDef.AsNoTracking().Where(a => a.IdCategoria == idCategoria);
        if (soloActivos) query = query.Where(a => a.Activo);

        return await query.OrderBy(a => a.Nombre).ToListAsync(ct);
    }

    public async Task<AtriDef?> GetByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.AtributosDef
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdCategoria == idCategoria && a.Codigo == codigoNorm, ct);
    }

    public async Task<AtriDef?> GetWithValoresAsync(long idAtriDef, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));

        return await _ctx.AtributosDef
            .AsNoTracking()
            .Include(a => a.Valores.Where(v => v.Activo).OrderBy(v => v.Orden).ThenBy(v => v.Nombre))
            .FirstOrDefaultAsync(a => a.IdAtriDef == idAtriDef, ct);
    }

    public async Task<IReadOnlyList<AtriDef>> GetObligatoriosByCategoriaAsync(long idCategoria, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        return await _ctx.AtributosDef
            .AsNoTracking()
            .Where(a => a.IdCategoria == idCategoria && a.Activo && a.EsOblig)
            .Include(a => a.Valores.Where(v => v.Activo).OrderBy(v => v.Orden))
            .OrderBy(a => a.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.AtributosDef
            .AnyAsync(a => a.IdCategoria == idCategoria && a.Codigo == codigoNorm, ct);
    }

    public async Task<bool> TieneValoresAsync(long idAtriDef, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));

        return await _ctx.AtributosVal
            .AnyAsync(v => v.IdAtriDef == idAtriDef && v.Activo, ct);
    }

    public async Task<int> CountByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        var query = _ctx.AtributosDef.Where(a => a.IdCategoria == idCategoria);
        if (soloActivos) query = query.Where(a => a.Activo);

        return await query.CountAsync(ct);
    }
}



