using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface ICodBarRepositoryAsync
{
    Task<CodBar?> GetByCodigoAsync(string codigo, int idTenant, CancellationToken ct = default);
    Task<CodBar?> GetByCodigoConPresentacionAsync(string codigo, int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<CodBar>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<CodBar?> GetPrincipalByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<CodBar>> GetByTipoAsync(long idPresProd, byte tipoCod, CancellationToken ct = default);
    Task<IReadOnlyList<CodBar>> GetByTenantAsync(int idTenant, CancellationToken ct = default);
    // S57.34: lectura global + alta para ProductoServiceAsync (sin contraparte sync).
    Task<CodBar?> GetByCodigoGlobalAsync(string codigo, CancellationToken ct = default);
}

public class CodBarRepositoryAsync : ICodBarRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public CodBarRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<CodBar?> GetByCodigoAsync(string codigo, int idTenant, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .Include(c => c.PresProd)
            .FirstOrDefaultAsync(c => c.Codigo == codigo && c.PresProd!.VarProd!.Prod!.IdTenant == idTenant, ct);
    }

public async Task<CodBar?> GetByCodigoConPresentacionAsync(string codigo, int idTenant, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .Include(c => c.PresProd)
                .ThenInclude(p => p.VarProd)
                    .ThenInclude(v => v.Prod)
            .Include(c => c.PresProd!.Unidad)
            .FirstOrDefaultAsync(c => c.Codigo == codigo
                && c.PresProd!.VarProd!.Prod!.IdTenant == idTenant
                && c.PresProd.Activo
                && c.PresProd.EsVenta
                && c.PresProd.VarProd!.Activo, ct);
    }

    public async Task<IReadOnlyList<CodBar>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .Where(c => c.IdPresProd == idPresProd)
            .OrderByDescending(c => c.Principal)
            .ThenBy(c => c.TipoCod)
            .ToListAsync(ct);
    }

    public async Task<CodBar?> GetPrincipalByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdPresProd == idPresProd && c.Principal, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        return await _ctx.CodigosBarra
            .AnyAsync(c => c.Codigo == codigo, ct);
    }

    public async Task<IReadOnlyList<CodBar>> GetByTipoAsync(long idPresProd, byte tipoCod, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .Where(c => c.IdPresProd == idPresProd && c.TipoCod == tipoCod)
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<CodBar>> GetByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.CodigosBarra
            .AsNoTracking()
            .Include(c => c.PresProd)
                .ThenInclude(p => p.VarProd)
                    .ThenInclude(v => v.Prod)
            .Where(c => c.PresProd!.VarProd!.Prod!.IdTenant == idTenant)
            .OrderBy(c => c.Codigo)
            .ToListAsync(ct);
    }

    public async Task<CodBar?> GetByCodigoGlobalAsync(string codigo, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        return await _ctx.CodigosBarra.AsNoTracking()
            .FirstOrDefaultAsync(c => c.Codigo == codigo, ct);
    }

}
