using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface ITenantSyncServiceAsync
{
    Task<Result<int>> ResolveLocalTenantIdAsync(int idTenantPassPlat, CancellationToken ct = default);
    Task<Result<Tenant>> EnsureTenantAsync(int idTenantPassPlat, string codigo, string nombre, CancellationToken ct = default);
}

public class TenantSyncServiceAsync : ITenantSyncServiceAsync
{
    private readonly ITenantRepositoryAsync _tenantRepo;
    private readonly ITenantConfigInventaRepositoryAsync _tenantCfgRepo;

    public TenantSyncServiceAsync(ITenantRepositoryAsync tenantRepo, ITenantConfigInventaRepositoryAsync tenantCfgRepo)
    {
        _tenantRepo = tenantRepo;
        _tenantCfgRepo = tenantCfgRepo;
    }

    public async Task<Result<int>> ResolveLocalTenantIdAsync(int idTenantPassPlat, CancellationToken ct = default)
    {
        var tenant = await _tenantRepo.GetByPassPlatIdAsync(idTenantPassPlat, ct);
        if (tenant == null)
            return Result<int>.Failure(new CBP.Results.Errors.Error("TENANT_NOT_SYNCED", $"Tenant PassPlat {idTenantPassPlat} no sincronizado").WithType(CBP.Results.Errors.ErrorType.NotFound).WithHttpStatus(404));
        return Result<int>.Success(tenant.IdTenant);
    }

    public async Task<Result<Tenant>> EnsureTenantAsync(int idTenantPassPlat, string codigo, string nombre, CancellationToken ct = default)
    {
        var existing = await _tenantRepo.GetByPassPlatIdAsync(idTenantPassPlat, ct);
        if (existing != null) return Result<Tenant>.Success(existing);

        var tenant = new Tenant
        {
            Codigo = codigo,
            Nombre = nombre,
            Activo = true,
            IdTenantPassPlat = idTenantPassPlat,
            FechaSync = DateTime.UtcNow
        };
        await _tenantRepo.UpsertAsync(tenant, ct);

        // ensure TenantConfigInventa defaults — async check
        await _tenantCfgRepo.EnsureDefaultsAsync(tenant.IdTenant, ct);

        var saved = await _tenantRepo.GetByPassPlatIdAsync(idTenantPassPlat, ct);
        return Result<Tenant>.Success(saved!);
    }
}
