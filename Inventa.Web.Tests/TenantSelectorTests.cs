using System.Net;
using System.Text;
using Bunit;
using Inventa.Web.Layout;
using Inventa.Web.Models;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>S92.2-K5/K6: TenantSelector con MudBlazor 9 (bUnit, JSRuntimeMode.Loose):
/// muestra el tenant actual, lista los demás y cambiar funciona.</summary>
public sealed class TenantSelectorTests
{
    private readonly BunitContext _ctx = new();
    private IRenderedComponent<MudPopoverProvider> _provider = null!;
    private const string TenantsJson = """[{"idTenant":4,"idTenantPassPlat":40,"codigo":"ABARROTES","nombre":"Abarrotes"},{"idTenant":6,"idTenantPassPlat":60,"codigo":"VESTUARIO","nombre":"Vestuario"}]""";

    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; } = "tok";
        public int? TenantId { get; set; } = 4;
        public int? TenantPassPlatId { get; set; } = 40;
        public (int Local, int Pp)? UltimoSet { get; private set; }
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(Token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult(TenantId);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult(TenantPassPlatId);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default)
        { Token = accessToken; TenantId = tenantId; TenantPassPlatId = tenantPassPlatId; return Task.CompletedTask; }
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default)
        { TenantId = tenantId; TenantPassPlatId = tenantPassPlatId; UltimoSet = (tenantId, tenantPassPlatId); return Task.CompletedTask; }
        public Task ClearTenantIdsAsync(CancellationToken ct = default) { TenantId = null; TenantPassPlatId = null; return Task.CompletedTask; }
        public Task ClearAsync(CancellationToken ct = default) { Token = null; TenantId = null; TenantPassPlatId = null; return Task.CompletedTask; }
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public int Hits { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Hits++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(TenantsJson, Encoding.UTF8, "application/json") });
        }
    }

    private static readonly TimeSpan Espera = TimeSpan.FromSeconds(5);
    private readonly StubStorage _storage = new();
    private readonly StubHandler _http = new();

    public TenantSelectorTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton<ITokenStorage>(_storage);
        _ctx.Services.AddSingleton(_ => new ApiClient(new HttpClient(_http) { BaseAddress = new Uri("https://localhost:5002/") }));
        _ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        _ctx.Services.AddSingleton(_ => new AppSettings());
        _ctx.Services.AddSingleton<CustomAuthenticationStateProvider>();
        _ctx.Services.AddSingleton<TenantService>();
        _ctx.Services.AddSingleton<TenantContextService>();
        _ctx.Services.AddSingleton<AuthService>();
        // Los contenidos de popover (MudMenu) solo se renderizan con un
        // MudPopoverProvider en el árbol (guía bUnit de MudBlazor).
        _provider = _ctx.Render<MudPopoverProvider>();
    }

    [Fact]
    public void MuestraTenantActual_YListaLosDemas()
    {
        var cut = _ctx.Render<TenantSelector>();

        // S109: OnInitializedAsync (mis-tenants) y el menú son asíncronos: se espera su render.
        cut.WaitForAssertion(() => Assert.Contains("Abarrotes", cut.Find("button.inv-tenant-indicator").TextContent), Espera);

        cut.Find("button.inv-tenant-indicator").Click();
        // 2 tenants + «Cambiar de tenant…».
        _provider.WaitForAssertion(() => Assert.Equal(3, _provider.FindAll("div.mud-menu-item").Count), Espera);
        Assert.Contains("Vestuario", _provider.FindAll("div.mud-menu-item")[1].TextContent);
    }

    [Fact]
    public void ClickEnOtroTenant_CambiaElContexto()
    {
        var cut = _ctx.Render<TenantSelector>();
        var ctx = _ctx.Services.GetRequiredService<TenantContextService>();
        cut.WaitForAssertion(() => Assert.Contains("Abarrotes", cut.Find("button.inv-tenant-indicator").TextContent), Espera);

        cut.Find("button.inv-tenant-indicator").Click();
        _provider.WaitForAssertion(() => Assert.Equal(3, _provider.FindAll("div.mud-menu-item").Count), Espera);
        _provider.FindAll("div.mud-menu-item")[1].Click();

        // S109: MudMenuItem cierra el menú antes de invocar OnClick; el cambio de tenant
        // termina de forma asíncrona. Afirmar sin esperar era la causa de la intermitencia
        // (Expected 6, Actual 4).
        cut.WaitForAssertion(() => Assert.Equal(6, ctx.CurrentTenantId), Espera);
        Assert.Equal(60, ctx.CurrentTenantPassPlatId);
        Assert.Equal((6, 60), _storage.UltimoSet);
        Assert.Equal(1, _http.Hits); // init pidió mis-tenants una sola vez
    }
}
