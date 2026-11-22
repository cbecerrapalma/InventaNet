namespace Inventa.Web.Services.Http;

/// <summary>
/// Provides current tenant context for HTTP requests.
/// Returns both local IdTenant and PassPlat IdTenantPassPlat (tid).
/// </summary>
public interface ITenantContextProvider
{
    Task<int?> GetCurrentTenantIdAsync(CancellationToken ct = default);
    Task<int?> GetCurrentTenantPassPlatIdAsync(CancellationToken ct = default);
}
