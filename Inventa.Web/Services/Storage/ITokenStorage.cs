namespace Inventa.Web.Services.Storage;

public interface ITokenStorage
{
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);
    Task<string?> GetRefreshTokenAsync(CancellationToken ct = default);
    Task<int?> GetTenantIdAsync(CancellationToken ct = default);
    Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default);
    Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default);
    Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default);
    Task ClearTenantIdsAsync(CancellationToken ct = default);
    Task ClearAsync(CancellationToken ct = default);
}
