// S93.3-C6: entrada numérica es-CL (bUnit, MudBlazor 9).
// - Editor rápido de Precios acepta «12.990» como 12990 y rechaza «1,5» con «monto entero».
// - TasaPct muestra 19 (no 19,00) y «Guardar» sin cambios no envía PUT.
// Los tests fijan el hilo en en-US para probar que el parseo NO depende de la cultura
// ambiente (los campos llevan Culture="@FormatoCl.Cultura" explícito, F4).
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Inventa.Web.Pages.Configuracion;
using Inventa.Web.Pages.Precios;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Configuracion;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Precios;
using Inventa.Web.Services.Productos;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

public sealed class S933_EntradaNumericaTests : IAsyncDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly StubHandler _http = new();
    private readonly CultureInfo _prevCulture = CultureInfo.CurrentCulture;
    private readonly CultureInfo _prevUiCulture = CultureInfo.CurrentUICulture;

    private const string GrillaJson = """{"items":[{"idPresProd":7,"idProd":1,"codigoProducto":"ARROZ","codigoVariante":"KG","producto":"Arroz","variante":"Kg","categoria":"Abarrotes","marca":null,"codigoBarras":null,"vendible":true,"publicable":true,"oficial":{"idPrecio":1,"valor":10000,"desde":"2026-01-01"},"programado":null,"oferta":null,"ofertaProgramada":null,"ofertasSinEfecto":0,"precioEfectivo":10000,"costoProm":8000,"margenPct":25}],"total":1,"page":1,"pageSize":20,"moneda":"CLP","decimales":0,"hoy":"2026-10-16"}""";
    private const string ResumenJson = """{"moneda":"CLP","decimales":0,"vendibles":1,"sinPrecio":0,"conOferta":0,"vencenEn7Dias":0,"ofertasSinEfecto":0,"programados":0,"hoy":"2026-10-16"}""";
    private const string TenantConfigJson = """{"idTenant":4,"maxBodegas":10,"permiteStockNegativo":false,"moneda":"CLP","tasaImpuesto":0.19,"permiteVentaSinCliente":true,"fecMod":null}""";

    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; } = FakeJwt("INVENTA_PRODUCTO_VER", "INVENTA_PRODUCTO_EDITAR", "INVENTA_CONFIG_VER", "INVENTA_CONFIG_EDITAR");
        public int? TenantId { get; set; } = 4;
        public int? TenantPassPlatId { get; set; } = 40;
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(Token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult(TenantId);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult(TenantPassPlatId);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default)
        { Token = accessToken; TenantId = tenantId; TenantPassPlatId = tenantPassPlatId; return Task.CompletedTask; }
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default)
        { TenantId = tenantId; TenantPassPlatId = tenantPassPlatId; return Task.CompletedTask; }
        public Task ClearTenantIdsAsync(CancellationToken ct = default) { TenantId = null; TenantPassPlatId = null; return Task.CompletedTask; }
        public Task ClearAsync(CancellationToken ct = default) { Token = null; TenantId = null; TenantPassPlatId = null; return Task.CompletedTask; }
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public int PutConfigHits { get; private set; }
        public string LastPutBody { get; private set; } = "";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var path = req.RequestUri?.AbsolutePath ?? "";
            string json = "{}";
            if (req.Method == HttpMethod.Put && path.EndsWith("/config"))
            {
                PutConfigHits++;
                LastPutBody = req.Content?.ReadAsStringAsync(ct).GetAwaiter().GetResult() ?? "";
                json = TenantConfigJson;
            }
            else if (path.Contains("api/precios/grilla")) json = GrillaJson;
            else if (path.EndsWith("api/precios/resumen")) json = ResumenJson;
            else if (path.EndsWith("api/catalogos/categorias")) json = "[]";
            else if (path.EndsWith("api/catalogos/marcas")) json = "[]";
            else if (path.EndsWith("api/tenants/4/config")) json = TenantConfigJson;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }

    private static string FakeJwt(params string[] permissions)
    {
        var payload = Convert.ToBase64String(Encoding.UTF8.GetBytes(
            JsonSerializer.Serialize(new { permissions })));
        return $"e30.{payload}.";
    }

    public S933_EntradaNumericaTests()
    {
        // Hilo en en-US: el parseo debe seguir siendo es-CL por el Culture explícito.
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        CultureInfo.CurrentUICulture = new CultureInfo("en-US");
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton<ITokenStorage, StubStorage>();
        _ctx.Services.AddSingleton(_ => new ApiClient(new HttpClient(_http) { BaseAddress = new Uri("https://localhost:5002/") }));
        _ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        _ctx.Services.AddSingleton<PermissionService>();
        _ctx.Services.AddSingleton<TenantContextService>();
        _ctx.Services.AddSingleton<PreciosApiService>();
        _ctx.Services.AddSingleton<ProductosApiService>();
        _ctx.Services.AddSingleton<ConfiguracionApiService>();
        var tenantCtx = _ctx.Services.GetRequiredService<TenantContextService>();
        tenantCtx.GetCurrentTenantIdAsync().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        CultureInfo.CurrentCulture = _prevCulture;
        CultureInfo.CurrentUICulture = _prevUiCulture;
        await _ctx.DisposeAsync();
    }

    [Fact]
    public void EditorRapido_Acepta12_990Como12990()
    {
        var cut = _ctx.Render<Precios>();
        cut.WaitForElement("[data-testid=celda-precio]").Click();

        var input = cut.Find("input[data-testid=celda-precio-input]");
        input.Input("12.990");
        input.Blur();

        var snack = _ctx.Services.GetRequiredService<ISnackbar>();
        Assert.Empty(snack.ShownSnackbars);
        Assert.Contains("$12.990", cut.Find("[data-testid=celda-precio]").TextContent);
    }

    [Fact]
    public void EditorRapido_Rechaza1_5ConMensajeMontoEntero()
    {
        var cut = _ctx.Render<Precios>();
        cut.WaitForElement("[data-testid=celda-precio]").Click();

        var input = cut.Find("input[data-testid=celda-precio-input]");
        input.Input("1,5");
        input.Blur();

        var snack = _ctx.Services.GetRequiredService<ISnackbar>();
        Assert.Contains(snack.ShownSnackbars, s => s.Message!.Contains("monto entero"));
        // El editor sigue abierto (no se confirmó).
        Assert.NotEmpty(cut.FindAll("input[data-testid=celda-precio-input]"));
    }

    [Fact]
    public void TasaPct_Muestra19_YGuardarSinCambios_NoEnviaPut()
    {
        var cut = _ctx.Render<TenantConfiguracion>();

        var input = cut.WaitForElement("input[data-testid=tasa-pct]");
        Assert.Equal("19", input.GetAttribute("value"));

        var guardar = cut.Find("[aria-label='Guardar configuración del tenant']");
        Assert.True(guardar.HasAttribute("disabled"));
        Assert.Equal(0, _http.PutConfigHits);
    }

    [Fact]
    public void TasaPct_150SeLimitaA100_EnElPut()
    {
        // MudNumericField limita al Max=100: el backend nunca recibe 1.5 (150 %).
        var cut = _ctx.Render<TenantConfiguracion>();

        cut.WaitForElement("input[data-testid=tasa-pct]").Change("150");
        cut.Find("[aria-label='Guardar configuración del tenant']").Click();

        Assert.Equal(1, _http.PutConfigHits);
        Assert.Contains("\"tasaImpuesto\":1", _http.LastPutBody);
    }
}
