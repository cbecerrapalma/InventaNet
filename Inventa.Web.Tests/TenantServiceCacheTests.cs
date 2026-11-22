using System.Net;
using System.Text;
using Inventa.Web.Models.Api;
using Inventa.Web.Models.Tenant;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>S92.2-K5/K7: la caché de mis-tenants hace 1 sola petición aunque
/// varios componentes la pidan a la vez; se invalida en login/logout/cambio.</summary>
public sealed class TenantServiceCacheTests
{
    private const string TenantsJson = """[{"idTenant":4,"idTenantPassPlat":40,"codigo":"ABARROTES","nombre":"Abarrotes"},{"idTenant":6,"idTenantPassPlat":60,"codigo":"VESTUARIO","nombre":"Vestuario"}]""";

    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; } = "tok-1";
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(Token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(null);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default)
        { Token = accessToken; return Task.CompletedTask; }
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearTenantIdsAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) { Token = null; return Task.CompletedTask; }
    }

    private sealed class CountingHandler : HttpMessageHandler
    {
        private int _hits;
        public int Hits => _hits;
        public bool FailFirst { get; set; }
        private int _calls;

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Interlocked.Increment(ref _hits);
            var n = Interlocked.Increment(ref _calls);
            if (FailFirst && n == 1)
                return new HttpResponseMessage(HttpStatusCode.InternalServerError)
                { Content = new StringContent("boom", Encoding.UTF8, "text/plain") };
            // Barrera para que las llamadas concurrentes se solapen en vuelo.
            await Task.Delay(50, ct);
            return new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(TenantsJson, Encoding.UTF8, "application/json") };
        }
    }

    private static (TenantService Svc, CountingHandler Handler, StubStorage Storage) Build(bool failFirst = false)
    {
        var handler = new CountingHandler { FailFirst = failFirst };
        var storage = new StubStorage();
        var api = new ApiClient(new HttpClient(handler) { BaseAddress = new Uri("https://localhost:5002/") });
        return (new TenantService(api, storage), handler, storage);
    }

    [Fact]
    public async Task DosLlamadasSeguidas_UnaSolaPeticion()
    {
        var (svc, handler, _) = Build();

        var a = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);
        var b = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(1, handler.Hits);
        Assert.Equal(2, a.Count);
        Assert.Same(a, b);
    }

    [Fact]
    public async Task DiezLlamadasConcurrentes_CompartenElMismoVuelo()
    {
        var (svc, handler, _) = Build();

        var tareas = Enumerable.Range(0, 10).Select(_ => svc.GetMyTenantsAsync()).ToArray();
        var resultados = await Task.WhenAll(tareas);

        Assert.Equal(1, handler.Hits);
        Assert.All(resultados, r => Assert.Equal(2, r.Count));
    }

    [Fact]
    public async Task InvalidateCache_FuerzaNuevaPeticion()
    {
        var (svc, handler, _) = Build();
        await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        svc.InvalidateCache();
        var luego = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Hits);
        Assert.Equal(2, luego.Count);
    }

    [Fact]
    public async Task CambioDeToken_ReleeSinInvalidateExplicito()
    {
        var (svc, handler, storage) = Build();
        await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        storage.Token = "tok-2"; // login nuevo = clave de sesión distinta
        await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Hits);
    }

    [Fact]
    public async Task Fallo_NoEnvenenaLaCache_ElReintentoPideDeNuevo()
    {
        var (svc, handler, _) = Build(failFirst: true);

        await Assert.ThrowsAsync<ApiException>(() => svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken));
        var luego = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(2, handler.Hits);
        Assert.Equal(2, luego.Count);
    }

    [Fact]
    public async Task PrimeCache_SirveSinPeticionHttp()
    {
        // S92.2-K7: el login deja su lista autoritativa en caché → tras el
        // login los componentes no repiten GET api/auth/mis-tenants.
        var (svc, handler, _) = Build();
        var primed = new List<TenantItemDto>
        {
            new() { IdTenant = 4, IdTenantPassPlat = 40, Codigo = "ABARROTES", Nombre = "Abarrotes" },
        };

        await svc.PrimeCacheAsync(primed, Xunit.TestContext.Current.CancellationToken);
        var a = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);
        var b = await svc.GetMyTenantsAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(0, handler.Hits);
        Assert.Same(primed, a);
        Assert.Same(a, b);
    }
}
