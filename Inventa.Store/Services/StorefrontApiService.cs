using System.Net;
using System.Net.Http.Json;
using Inventa.Store.Models;

namespace Inventa.Store.Services;

/// <summary>S57.1 public storefront profile (server-resolved tenant). Null = unpublished.</summary>
public sealed class StorefrontApiService
{
    private readonly HttpClient _http;
    public StorefrontApiService(HttpClient http) => _http = http;

    public async Task<PublicStorefrontDto?> GetStorefrontAsync(CancellationToken ct = default)
    {
        try
        {
            var res = await _http.GetAsync("api/public/catalog/storefront", ct);
            if (res.StatusCode == HttpStatusCode.NoContent) return null;
            res.EnsureSuccessStatusCode();
            return await res.Content.ReadFromJsonAsync<PublicStorefrontDto>(cancellationToken: ct);
        }
        catch { return null; }
    }
}
