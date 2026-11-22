using Microsoft.JSInterop;

namespace Inventa.Web.Services.Storage;

public sealed class TokenStorage : ITokenStorage
{
    private const string KeyAccessToken = "access_token";
    private const string KeyRefreshToken = "refresh_token";
    private const string KeyTenantId = "id_tenant";
    private const string KeyTenantPassPlatId = "id_tenant_pp";

    private readonly IJSRuntime _js;

    public TokenStorage(IJSRuntime js) => _js = js;

    public async Task<string?> GetAccessTokenAsync(CancellationToken ct = default)
    {
        try { return await _js.InvokeAsync<string?>( "localStorage.getItem", ct, KeyAccessToken); }
        catch { return null; }
    }

    public async Task<string?> GetRefreshTokenAsync(CancellationToken ct = default)
    {
        try { return await _js.InvokeAsync<string?>( "localStorage.getItem", ct, KeyRefreshToken); }
        catch { return null; }
    }

    public async Task<int?> GetTenantIdAsync(CancellationToken ct = default)
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>( "localStorage.getItem", ct, KeyTenantId);
            if (int.TryParse(raw, out var id)) return id;
            return null;
        }
        catch { return null; }
    }

    public async Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default)
    {
        try
        {
            var raw = await _js.InvokeAsync<string?>( "localStorage.getItem", ct, KeyTenantPassPlatId);
            if (int.TryParse(raw, out var id)) return id;
            return null;
        }
        catch { return null; }
    }

    public async Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyAccessToken, accessToken);
            if (!string.IsNullOrWhiteSpace(refreshToken))
                await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyRefreshToken, refreshToken);
            else
                await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyRefreshToken);

            if (tenantId.HasValue)
                await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyTenantId, tenantId.Value.ToString());
            else
                await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantId);

            if (tenantPassPlatId.HasValue)
                await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyTenantPassPlatId, tenantPassPlatId.Value.ToString());
            else
                await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantPassPlatId);
        }
        catch { }
    }

    public async Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyTenantId, tenantId.ToString());
            await _js.InvokeVoidAsync("localStorage.setItem", ct, KeyTenantPassPlatId, tenantPassPlatId.ToString());
        }
        catch { }
    }

    public async Task ClearTenantIdsAsync(CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantId);
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantPassPlatId);
        }
        catch { }
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyAccessToken);
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyRefreshToken);
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantId);
            await _js.InvokeVoidAsync("localStorage.removeItem", ct, KeyTenantPassPlatId);
        }
        catch { }
    }
}
