using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IConfiguracionRepositoryAsync
{
    Task<Configuracion?> GetAsync(CancellationToken ct = default);
    Task UpdateAsync(Configuracion cfg, CancellationToken ct = default);
}

public class ConfiguracionRepositoryAsync : IConfiguracionRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public ConfiguracionRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<Configuracion?> GetAsync(CancellationToken ct = default)
        => _ctx.Configuraciones.FirstOrDefaultAsync(x => x.Id == 1, ct);

    public async Task UpdateAsync(Configuracion cfg, CancellationToken ct = default)
    {
        var existing = await _ctx.Configuraciones.FirstOrDefaultAsync(x => x.Id == 1, ct);
        if (existing == null)
        {
            cfg.Id = 1;
            _ctx.Configuraciones.Add(cfg);
        }
        else
        {
            _ctx.Entry(existing).CurrentValues.SetValues(cfg);
        }
        await _ctx.SaveChangesAsync(ct);
    }
}

public interface ITenantConfigInventaRepositoryAsync
{
    Task<TenantConfigInventa?> GetByTenantAsync(int idTenant, CancellationToken ct = default);
    Task UpsertAsync(TenantConfigInventa cfg, CancellationToken ct = default);
    // S57.34: creación idempotente de defaults (sin contraparte sync).
    Task EnsureDefaultsAsync(int idTenant, CancellationToken ct = default);
}

public class TenantConfigInventaRepositoryAsync : ITenantConfigInventaRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public TenantConfigInventaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<TenantConfigInventa?> GetByTenantAsync(int idTenant, CancellationToken ct = default)
        => _ctx.TenantConfigs.FirstOrDefaultAsync(x => x.IdTenant == idTenant, ct);

    public async Task UpsertAsync(TenantConfigInventa cfg, CancellationToken ct = default)
    {
        var existing = await _ctx.TenantConfigs.FirstOrDefaultAsync(x => x.IdTenant == cfg.IdTenant, ct);
        if (existing == null) _ctx.TenantConfigs.Add(cfg);
        else _ctx.Entry(existing).CurrentValues.SetValues(cfg);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task EnsureDefaultsAsync(int idTenant, CancellationToken ct = default)
    {
        var exists = await _ctx.TenantConfigs.AnyAsync(x => x.IdTenant == idTenant, ct);
        if (!exists)
        {
            _ctx.TenantConfigs.Add(new TenantConfigInventa { IdTenant = idTenant });
            await _ctx.SaveChangesAsync(ct);
        }
    }
}




