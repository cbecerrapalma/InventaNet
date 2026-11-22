using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S67: contrato POST api/public/auth/refresh + logout (10 pruebas, host propio).
public sealed class StoreAuthRefreshLogoutTests : IClassFixture<StoreAuthFactory>
{
    private readonly HttpClient _client;
    private readonly FakePassPlatClient _fake;
    private readonly StoreAuthFactory _factory;

    public StoreAuthRefreshLogoutTests(StoreAuthFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
        _fake = factory.Fake;
    }

    private static async Task<JsonDocument> Body(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private static string Code(JsonDocument b) => b.RootElement.GetProperty("code").GetString()!;

    [Fact]
    public async Task Refresh_200_RotaTokens_YNoStore()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/refresh", new { refreshToken = "RT-viejo" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("AT2", b.RootElement.GetProperty("accessToken").GetString());
        Assert.Equal("RT2", b.RootElement.GetProperty("refreshToken").GetString());
        Assert.Equal("no-store", string.Join(",", r.Headers.CacheControl?.ToString() ?? ""));
    }

    [Fact]
    public async Task Refresh_Vacio_400()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/refresh", new { refreshToken = "" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("STORE_AUTH_VALIDATION", Code(await Body(r)));
    }

    [Fact]
    public async Task Refresh_Upstream401_401Tipado()
    {
        _fake.Reset();
        _fake.OnRefreshStore = _ => StoreAuthResult<LoginResponse>.Fail(new StoreAuthError(401, "REFRESH_EXPIRED"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/refresh", new { refreshToken = "RT-viejo" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("STORE_AUTH_REFRESH_INVALID", Code(b));
        Assert.Equal("REFRESH_EXPIRED", b.RootElement.GetProperty("upstream").GetString());
    }

    [Fact]
    public async Task Refresh_Upstream429_429ConRetryAfter()
    {
        _fake.Reset();
        _fake.OnRefreshStore = _ => StoreAuthResult<LoginResponse>.Fail(new StoreAuthError(429, "RATE", 7));
        var r = await _client.PostAsJsonAsync("/api/public/auth/refresh", new { refreshToken = "RT-viejo" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal((HttpStatusCode)429, r.StatusCode);
        Assert.Equal("STORE_AUTH_RATE_LIMITED", Code(await Body(r)));
        Assert.True(r.Headers.RetryAfter?.Delta?.TotalSeconds >= 1);
    }

    [Fact]
    public async Task Refresh_UpstreamCaido_503()
    {
        _fake.Reset();
        _fake.OnRefreshStore = _ => StoreAuthResult<LoginResponse>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/refresh", new { refreshToken = "RT-viejo" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_UPSTREAM", Code(await Body(r)));
    }

    [Fact]
    public async Task Logout_SinBearer_204()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/logout", new { }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
        Assert.Equal("no-store", string.Join(",", r.Headers.CacheControl?.ToString() ?? ""));
    }

    [Fact]
    public async Task Logout_ConBearer_204()
    {
        _fake.Reset();
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/public/auth/logout");
        req.Headers.Authorization = new("Bearer", "AT-vigente");
        var r = await _client.SendAsync(req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
    }

    [Fact]
    public async Task Logout_BearerInvalido_204Idempotente()
    {
        _fake.Reset();
        _fake.OnLogoutStore = _ => StoreAuthResult<bool>.Fail(new StoreAuthError(401, "INVALID_TOKEN"));
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/public/auth/logout");
        req.Headers.Authorization = new("Bearer", "AT-muerto");
        var r = await _client.SendAsync(req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
    }

    [Fact]
    public async Task Logout_BodyConRefreshToken_SeAcepta_204()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/logout", new { refreshToken = "RT-que-se-ignora" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NoContent, r.StatusCode);
    }

    [Fact]
    public void RutasPublicAuth_UsanLimiterPublic_SinDuplicados()
    {
        // Prueba el cableado S67 sin gastar presupuesto del limiter (0 HTTP):
        // cada endpoint public/auth trae EXACTAMENTE una politica y es "Public".
        var eps = _factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(d => d.Endpoints).OfType<RouteEndpoint>()
            .Where(e => (e.RoutePattern.RawText ?? "").Contains("public/auth")).ToList();
        Assert.Equal(3, eps.Count);
        foreach (var e in eps)
        {
            var attrs = e.Metadata.OfType<EnableRateLimitingAttribute>().ToList();
            Assert.Single(attrs);
            Assert.Equal("Public", Assert.Single(attrs).PolicyName);
        }
    }

    [Fact]
    public void RutaBackoffice_ConservaLimiterAuthenticated()
    {
        // La convencion global S67 sigue aplicando "Authenticated" al resto.
        var eps = _factory.Services.GetServices<EndpointDataSource>()
            .SelectMany(d => d.Endpoints).OfType<RouteEndpoint>()
            .Where(e => (e.RoutePattern.RawText ?? "").Equals("api/auth/login")).ToList();
        var ep = Assert.Single(eps);
        var attrs = ep.Metadata.OfType<EnableRateLimitingAttribute>().ToList();
        Assert.Single(attrs);
        Assert.Equal("Authenticated", Assert.Single(attrs).PolicyName);
    }

    [Fact]
    public async Task Logout_UpstreamCaido_503()
    {
        _fake.Reset();
        _fake.OnLogoutStore = _ => StoreAuthResult<bool>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE"));
        using var req = new HttpRequestMessage(HttpMethod.Post, "/api/public/auth/logout");
        req.Headers.Authorization = new("Bearer", "AT-vigente");
        var r = await _client.SendAsync(req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_UPSTREAM", Code(await Body(r)));
    }
}

