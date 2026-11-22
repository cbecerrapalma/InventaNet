using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S57.34: Configuración global (singleton Id=1) + config por tenant.
/// Usa exclusivamente repositorios Async. Sin DbContext.
/// </summary>
public interface IConfiguracionServiceAsync
{
    Task<Result<Configuracion?>> GetAsync(CancellationToken ct = default);
    Task<Result<Configuracion>> UpdateAsync(Configuracion cfg, CancellationToken ct = default);
    Task<Result<TenantConfigInventa?>> GetTenantConfigAsync(int idTenant, CancellationToken ct = default);
    Task<Result<TenantConfigInventa>> UpsertTenantConfigAsync(TenantConfigInventa cfg, CancellationToken ct = default);
}

public sealed class ConfiguracionServiceAsync : IConfiguracionServiceAsync
{
    private readonly IConfiguracionRepositoryAsync _cfgRepo;
    private readonly ITenantConfigInventaRepositoryAsync _tenantCfgRepo;

    public ConfiguracionServiceAsync(
        IConfiguracionRepositoryAsync cfgRepo,
        ITenantConfigInventaRepositoryAsync tenantCfgRepo)
    {
        _cfgRepo = cfgRepo;
        _tenantCfgRepo = tenantCfgRepo;
    }

    public async Task<Result<Configuracion?>> GetAsync(CancellationToken ct = default)
    {
        var cfg = await _cfgRepo.GetAsync(ct);
        return Result<Configuracion?>.Success(cfg);
    }

    public async Task<Result<Configuracion>> UpdateAsync(Configuracion cfg, CancellationToken ct = default)
    {
        await _cfgRepo.UpdateAsync(cfg, ct);
        return Result<Configuracion>.Success(cfg);
    }

    public async Task<Result<TenantConfigInventa?>> GetTenantConfigAsync(int idTenant, CancellationToken ct = default)
    {
        var cfg = await _tenantCfgRepo.GetByTenantAsync(idTenant, ct);
        return Result<TenantConfigInventa?>.Success(cfg);
    }

    public async Task<Result<TenantConfigInventa>> UpsertTenantConfigAsync(TenantConfigInventa cfg, CancellationToken ct = default)
    {
        await _tenantCfgRepo.UpsertAsync(cfg, ct);
        return Result<TenantConfigInventa>.Success(cfg);
    }
}
