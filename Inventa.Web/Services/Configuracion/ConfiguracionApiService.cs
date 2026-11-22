using Inventa.Web.Models.Configuracion;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Configuracion;

public sealed class ConfiguracionApiService
{
    private readonly ApiClient _api;
    public ConfiguracionApiService(ApiClient api) => _api = api;

    public Task<ConfiguracionDto?> GetGlobalAsync(CancellationToken ct) => _api.GetAsync<ConfiguracionDto>("api/configuracion", ct);
    public Task<ConfiguracionDto?> UpdateGlobalAsync(UpdateConfiguracionDto dto, CancellationToken ct) => _api.PutAsync<UpdateConfiguracionDto, ConfiguracionDto>("api/configuracion", dto, ct);

    public Task<TenantConfigDto?> GetTenantAsync(int idTenant, CancellationToken ct) => _api.GetAsync<TenantConfigDto>($"api/tenants/{idTenant}/config", ct);
    public Task<TenantConfigDto?> UpdateTenantAsync(int idTenant, UpdateTenantConfigDto dto, CancellationToken ct) => _api.PutAsync<UpdateTenantConfigDto, TenantConfigDto>($"api/tenants/{idTenant}/config", dto, ct);
}
