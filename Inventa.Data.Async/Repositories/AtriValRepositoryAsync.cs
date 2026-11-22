using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IAtriValRepositoryAsync
{
    Task<IReadOnlyList<AtriVal>> GetByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default);
    Task<AtriVal?> GetByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default);
    Task<bool> IsUsedByVariantesAsync(long idAtriVal, CancellationToken ct = default);
    Task<int> CountByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default);
}

public class AtriValRepositoryAsync : IAtriValRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public AtriValRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<AtriVal>> GetByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));

        var query = _ctx.AtributosVal.AsNoTracking().Where(v => v.IdAtriDef == idAtriDef);
        if (soloActivos) query = query.Where(v => v.Activo);

        return await query.OrderBy(v => v.Orden).ThenBy(v => v.Nombre).ToListAsync(ct);
    }

    public async Task<AtriVal?> GetByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.AtributosVal
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdAtriDef == idAtriDef && v.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.AtributosVal
            .AnyAsync(v => v.IdAtriDef == idAtriDef && v.Codigo == codigoNorm, ct);
    }

    public async Task<bool> IsUsedByVariantesAsync(long idAtriVal, CancellationToken ct = default)
    {
        if (idAtriVal <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriVal));

        return await _ctx.VarAtributos
            .AnyAsync(va => va.IdAtriVal == idAtriVal, ct);
    }

    public async Task<int> CountByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idAtriDef <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriDef));

        var query = _ctx.AtributosVal.Where(v => v.IdAtriDef == idAtriDef);
        if (soloActivos) query = query.Where(v => v.Activo);

        return await query.CountAsync(ct);
    }
}



