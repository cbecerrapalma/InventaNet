using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface ITenantRepositoryAsync
{
    Task<Tenant?> GetByIdAsync(int idTenant, CancellationToken ct = default);
    Task<Tenant?> GetByPassPlatIdAsync(int idTenantPassPlat, CancellationToken ct = default);
    Task<Tenant?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetActivosAsync(CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<int> CountActivosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default);
    Task UpsertAsync(Tenant tenant, CancellationToken ct = default);
}

public class TenantRepositoryAsync : ITenantRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public TenantRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<Tenant?> GetByIdAsync(int idTenant, CancellationToken ct = default)
        => _ctx.Tenants.FirstOrDefaultAsync(t => t.IdTenant == idTenant, ct);

    public Task<Tenant?> GetByPassPlatIdAsync(int idTenantPassPlat, CancellationToken ct = default)
        => _ctx.Tenants.FirstOrDefaultAsync(t => t.IdTenantPassPlat == idTenantPassPlat, ct);

    public Task<Tenant?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
        => _ctx.Tenants.FirstOrDefaultAsync(t => t.Codigo == codigo, ct);

    public async Task<IReadOnlyList<Tenant>> GetActivosAsync(CancellationToken ct = default)
        => await _ctx.Tenants.AsNoTracking().Where(t => t.Activo).OrderBy(t => t.Nombre).ToListAsync(ct);

    public Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
        => _ctx.Tenants.AnyAsync(t => t.Codigo == codigo.ToUpperInvariant(), ct);

    public Task<int> CountActivosAsync(CancellationToken ct = default)
        => _ctx.Tenants.CountAsync(t => t.Activo, ct);

    public async Task<IReadOnlyList<Tenant>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.Tenants.Take(100).ToListAsync(ct);

    public async Task UpsertAsync(Tenant tenant, CancellationToken ct = default)
    {
        var existing = tenant.IdTenant != 0
            ? await _ctx.Tenants.FirstOrDefaultAsync(t => t.IdTenant == tenant.IdTenant, ct)
            : null;
        if (existing == null && tenant.IdTenantPassPlat.HasValue)
            existing = await _ctx.Tenants.FirstOrDefaultAsync(t => t.IdTenantPassPlat == tenant.IdTenantPassPlat, ct);

        if (existing == null)
        {
            _ctx.Tenants.Add(tenant);
        }
        else
        {
            _ctx.Entry(existing).CurrentValues.SetValues(tenant);
            existing.FecMod = DateTime.UtcNow;
        }
        await _ctx.SaveChangesAsync(ct);
    }
}




