using System.Net;
using System.Text;
using Bunit;
using Inventa.Web.Models.Precios;
using Inventa.Web.Pages.Precios;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Precios;
using Inventa.Web.Services.Productos;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Inventa.Web.Shared.Price;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>
/// S93.1 C9 (bUnit): página «Precios y ofertas» — filas y KPIs, edición en celda (Enter/Esc) con barra de
/// pendientes, editor de precio/oferta (% ↔ precio) y revisión con avisos D3 y errores 422. Más las funciones puras.
/// </summary>
public sealed class S931_PreciosTests : IAsyncDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly Stub _http = new();
    private static readonly DateOnly Hoy = new(2026, 10, 15);

    private const string GrillaJson = """
    {"items":[
      {"idPresProd":100,"idProd":1,"codigoProducto":"P1","codigoVariante":"P1-V1","producto":"Polera","variante":"Talla 2",
       "categoria":"Poleras","marca":"Ficcus","codigoBarras":"7800000000017","vendible":true,"publicable":true,
       "oficial":{"idPrecio":10,"valor":12990,"desde":"2026-10-01"},"programado":null,"oferta":null,"ofertaProgramada":null,
       "ofertasSinEfecto":0,"precioEfectivo":12990,"costoProm":6000,"margenPct":53.8},
      {"idPresProd":101,"idProd":1,"codigoProducto":"P1","codigoVariante":"P1-V2","producto":"Polera","variante":"Talla 4",
       "categoria":"Poleras","marca":"Ficcus","codigoBarras":null,"vendible":true,"publicable":true,
       "oficial":{"idPrecio":11,"valor":12990,"desde":"2026-10-01"},"programado":null,
       "oferta":{"idOferta":7,"precio":9990,"pct":23,"desde":"2026-10-10","hasta":"2026-10-31"},"ofertaProgramada":null,
       "ofertasSinEfecto":1,"precioEfectivo":9990,"costoProm":0,"margenPct":null}
    ],"total":2,"page":1,"pageSize":20,"moneda":"CLP","decimales":0,"hoy":"2026-10-15"}
    """;
    private const string ResumenJson = """{"moneda":"CLP","decimales":0,"vendibles":2,"sinPrecio":0,"conOferta":1,"vencenEn7Dias":0,"ofertasSinEfecto":1,"programados":0,"hoy":"2026-10-15"}""";

    private sealed class Stub : HttpMessageHandler
    {
        public HttpStatusCode SimularStatus { get; set; } = HttpStatusCode.OK;
        public string SimularJson { get; set; } = "";
        public List<string> Peticiones { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var path = req.RequestUri!.PathAndQuery;
            Peticiones.Add($"{req.Method} {path}{(req.Content is null ? "" : " " + await req.Content.ReadAsStringAsync(ct))}");
            var (status, json) = path switch
            {
                _ when path.StartsWith("/api/precios/grilla") => (HttpStatusCode.OK, GrillaJson),
                _ when path.StartsWith("/api/precios/resumen") => (HttpStatusCode.OK, ResumenJson),
                _ when path.StartsWith("/api/precios/cambios") => (SimularStatus, SimularJson),
                _ => (HttpStatusCode.OK, "[]"),
            };
            return new HttpResponseMessage(status) { Content = new StringContent(json, Encoding.UTF8, status == HttpStatusCode.OK ? "application/json" : "application/problem+json") };
        }
    }

    private sealed class StubStorage : ITokenStorage
    {
        // JWT sin firma con los permisos de edición (PermissionService solo lee el payload).
        private static readonly string Jwt = "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes(
            """{"permissions":["INVENTA_PRECIOS_PRINCIPAL_VER","INVENTA_PRECIOS_PRINCIPAL_EDITAR"]}""")).TrimEnd('=').Replace('+', '-').Replace('/', '_') + ".x";
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(Jwt);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(4);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(2);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearTenantIdsAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    public S931_PreciosTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        var api = new ApiClient(new HttpClient(_http) { BaseAddress = new Uri("https://localhost:5002/") });
        _ctx.Services.AddSingleton(api);
        _ctx.Services.AddSingleton<PreciosApiService>();
        _ctx.Services.AddSingleton<ProductosApiService>();
        var storage = new StubStorage();
        _ctx.Services.AddSingleton<ITokenStorage>(storage);
        _ctx.Services.AddSingleton<JwtClaimsReader>();
        _ctx.Services.AddSingleton<PermissionService>();
        _ctx.Services.AddSingleton(new TenantContextService(storage));
    }

    public ValueTask DisposeAsync() => _ctx.DisposeAsync();

    private (IRenderedComponent<MudPopoverProvider> Pop, IRenderedComponent<MudDialogProvider> Dlg, IRenderedComponent<Precios> Page) RenderPagina()
    {
        var pop = _ctx.Render<MudPopoverProvider>();
        var dlg = _ctx.Render<MudDialogProvider>();
        var page = _ctx.Render<Precios>();
        page.WaitForAssertion(() => Assert.Equal(2, page.FindAll("[data-testid='celda-precio']").Count));
        return (pop, dlg, page);
    }

    // ── Página ──

    [Fact]
    public void Pagina_MuestraFilasYKpis_YPideGrillaConEstado()
    {
        var (_, _, page) = RenderPagina();
        Assert.Contains("P1-V1", page.Find("[data-testid='precios-tabla']").TextContent);
        Assert.Contains("En oferta: 1", page.Find("[data-testid='kpi-conoferta']").TextContent);
        Assert.Contains("1 sin efecto", page.Find("[data-testid='precios-tabla']").TextContent);
        Assert.Contains(_http.Peticiones, p => p.StartsWith("GET /api/precios/grilla?page=1&pageSize=20&estado=0"));
        Assert.Empty(page.FindAll("[data-testid='precios-pendientes']"));
    }

    [Fact]
    public void EdicionEnCelda_Enter_AgregaPendiente_YMuestraBarra()
    {
        var (_, _, page) = RenderPagina();
        page.FindAll("[data-testid='celda-precio']")[0].Click();
        var input = page.Find("[data-testid='celda-precio-input'] input, input[data-testid='celda-precio-input']");
        input.Input("13990");
        input.KeyDown(new KeyboardEventArgs { Key = "Enter" });
        page.WaitForAssertion(() => Assert.Contains("1 cambio(s) sin guardar", page.Find("[data-testid='precios-pendientes']").TextContent));
        Assert.Contains("$13.990", page.FindAll("[data-testid='celda-precio']")[0].TextContent);
    }

    [Fact]
    public void EdicionEnCelda_Esc_Cancela_SinPendientes()
    {
        var (_, _, page) = RenderPagina();
        page.FindAll("[data-testid='celda-precio']")[0].Click();
        var input = page.Find("[data-testid='celda-precio-input'] input, input[data-testid='celda-precio-input']");
        input.Input("13990");
        input.KeyDown(new KeyboardEventArgs { Key = "Escape" });
        page.WaitForAssertion(() => Assert.Empty(page.FindAll("[data-testid='celda-precio-input']")));
        Assert.Empty(page.FindAll("[data-testid='precios-pendientes']"));
    }

    // ── Editor ──

    private static FilaPrecioDto Fila(OfertaFilaDto? oferta = null) => new(
        100, 1, "P1", "P1-V1", "Polera", "Talla 2", "Poleras", "Ficcus", null, true, true,
        new PrecioVigenciaDto(10, 12990m, Hoy.AddDays(-14)), null, oferta, null, 0, oferta?.Precio ?? 12990m, 6000m, 53.8m);

    [Fact]
    public async Task Editor_OfertaPorPorcentaje_CalculaPrecio_YDevuelveCambio()
    {
        var dlgProv = _ctx.Render<MudDialogProvider>();
        _ctx.Render<MudPopoverProvider>();
        var svc = _ctx.Services.GetRequiredService<IDialogService>();
        IDialogReference? r = null;
        await dlgProv.InvokeAsync(async () => r = await svc.ShowAsync<EditorPrecioDialog>("Editar", new DialogParameters
        {
            ["Fila"] = Fila(), ["Decimales"] = 0, ["Hoy"] = Hoy
        }));
        // MudRadio (MudBlazor 9) cambia con onclick sobre el input; el segundo es «Poner en oferta».
        dlgProv.FindAll("[data-testid='editor-modo-oferta'] input.mud-radio-input")[1].Click();
        dlgProv.WaitForAssertion(() => dlgProv.Find("[data-testid='editor-oferta-pct'] input, input[data-testid='editor-oferta-pct']"));
        dlgProv.Find("[data-testid='editor-oferta-pct'] input, input[data-testid='editor-oferta-pct']").Input("20");
        dlgProv.WaitForAssertion(() => Assert.Equal("10392",
            dlgProv.Find("[data-testid='editor-oferta-precio'] input, input[data-testid='editor-oferta-precio']").GetAttribute("value")?.Replace(".", "")));
        dlgProv.Find("[data-testid='editor-aceptar']").Click();
        var res = await r!.Result;
        var cambios = Assert.IsType<List<CambioPrecioDto>>(res!.Data);
        var c = Assert.Single(cambios);
        Assert.Equal((TipoCambioPrecio.Oferta, 10392m, 10L), (c.Tipo, c.PrecioOferta!.Value, c.IdPrecioVigenteVisto!.Value));
    }

    // ── Revisión ──

    private const string PlanD3 = """
    {"aplicado":false,"errores":[],"desactualizados":[],"omitidos":[],"filas":[],"preciosCreados":[],
     "presentaciones":[{"idPresProd":101,"codigo":"P1-V2","nombre":"Polera / Talla 4","oficialAntes":12990,"oficialDespues":14990,
       "ofertaAntes":9990,"ofertaDespues":9990,"efectivoAntes":9990,"efectivoDespues":9990,
       "acciones":["Precio oficial $14.990 desde hoy."],"avisos":["La oferta de $9.990 se mantiene con el nuevo precio desde el 15-10-2026."],"afectaOferta":true}]}
    """;

    private async Task<IRenderedComponent<MudDialogProvider>> AbrirRevision()
    {
        var dlgProv = _ctx.Render<MudDialogProvider>();
        _ctx.Render<MudPopoverProvider>();
        var svc = _ctx.Services.GetRequiredService<IDialogService>();
        await dlgProv.InvokeAsync(() => svc.ShowAsync<RevisionCambiosDialog>("Revisar", new DialogParameters
        {
            ["Cambios"] = new List<CambioPrecioDto> { new(101, TipoCambioPrecio.PrecioOficial, Valor: 14990, IdPrecioVigenteVisto: 11) },
            ["Hoy"] = Hoy
        }));
        return dlgProv;
    }

    [Fact]
    public async Task Revision_MuestraAvisoD3_ConOpcionMantenerTerminar_YPermiteGuardar()
    {
        _http.SimularJson = PlanD3;
        var dlg = await AbrirRevision();
        dlg.WaitForAssertion(() => Assert.Contains("se mantiene con el nuevo precio", dlg.Find("[data-testid='revision-tabla']").TextContent));
        Assert.NotEmpty(dlg.FindAll("[data-testid='revision-opcion-oferta']"));
        Assert.False(dlg.Find("[data-testid='revision-guardar']").HasAttribute("disabled"));
        Assert.Contains(_http.Peticiones, p => p.StartsWith("POST /api/precios/cambios?simular=true"));
    }

    [Fact]
    public async Task Revision_ConErrores422_LosMuestra_YNoPermiteGuardar()
    {
        _http.SimularStatus = HttpStatusCode.UnprocessableEntity;
        _http.SimularJson = """{"title":"Hay cambios con errores","status":422,"errores":[{"indice":0,"idPresProd":101,"codigo":"OFERTA_QUEDARIA_SOBRE_PRECIO","mensaje":"La oferta de $9.990 no es menor que el nuevo precio: termínela."}]}""";
        var dlg = await AbrirRevision();
        dlg.WaitForAssertion(() => Assert.Contains("no es menor que el nuevo precio", dlg.Find("[data-testid='revision-errores']").TextContent));
        Assert.True(dlg.Find("[data-testid='revision-guardar']").HasAttribute("disabled"));
    }

    [Fact]
    public async Task Revision_SesionExpirada401_MuestraMensaje_YNoPermiteGuardar()
    {
        _http.SimularStatus = HttpStatusCode.Unauthorized;
        _http.SimularJson = """{"title":"Unauthorized","status":401}""";
        var dlg = await AbrirRevision();
        dlg.WaitForAssertion(() => Assert.Contains("La sesión expiró", dlg.Find("[data-testid='revision-mensaje']").TextContent));
        Assert.True(dlg.Find("[data-testid='revision-guardar']").HasAttribute("disabled"));
    }

    // ── Menú (K6) ──

    private sealed class AuthAutenticado : Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider
    {
        public override Task<Microsoft.AspNetCore.Components.Authorization.AuthenticationState> GetAuthenticationStateAsync() =>
            Task.FromResult(new Microsoft.AspNetCore.Components.Authorization.AuthenticationState(
                new System.Security.Claims.ClaimsPrincipal(new System.Security.Claims.ClaimsIdentity([new System.Security.Claims.Claim("sub", "1")], "test"))));
    }

    private sealed class MenuPrecios : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""[{"codigo":"INVENTA","nombre":"INVENTA","orden":0,"esVisibleMenu":true,"hijos":[{"codigo":"INVENTA.PRECIOS","nombre":"Precios","orden":103,"esVisibleMenu":true,"hijos":[{"codigo":"INVENTA.PRECIOS.PRINCIPAL","nombre":"Precios y ofertas","ruta":"/precios","icono":"Sell","orden":3,"esVisibleMenu":true,"hijos":[]}]}]}]""",
                    Encoding.UTF8, "application/json")
            });
    }

    [Fact]
    public void Menu_PreciosYOfertas_SinMayoristaNiCompra()
    {
        // S100: el menú lo entrega PassPlat (aquí, un árbol simulado con Precios y ofertas y sin mayorista ni compra).
        _ctx.Services.AddSingleton<Microsoft.AspNetCore.Components.Authorization.AuthenticationStateProvider, AuthAutenticado>();
        _ctx.Services.AddSingleton(new CBP.Blazor.Acceso.MenuAccesoCliente(new HttpClient(new MenuPrecios()) { BaseAddress = new Uri("https://api.prueba/") }, "api/acceso/menu"));
        var menu = _ctx.Render<Inventa.Web.Layout.NavMenu>();
        menu.WaitForAssertion(() => Assert.Contains("Precios y ofertas", menu.Find("[data-testid='nav-inventa-precios-principal']").TextContent));
        Assert.Empty(menu.FindAll("[data-testid='nav-precios-mayorista']"));
        Assert.Empty(menu.FindAll("[data-testid='nav-precios-compra']"));
        Assert.DoesNotContain("Precios de compra", menu.Markup);
    }

    // ── Funciones puras ──

    [Theory]
    [InlineData(12990, 20, 10392)]
    [InlineData(9990, 50, 4995)]
    [InlineData(1000, 33, 670)]
    public void PrecioDesdePct_RedondeaComoElServidor(decimal referencia, decimal pct, decimal esperado) =>
        Assert.Equal(esperado, PreciosUi.PrecioDesdePct(referencia, pct, 0));

    [Fact]
    public void PctDesdePrecio_Y_Montos()
    {
        Assert.Equal(20, PreciosUi.PctDesdePrecio(12990m, 10392m));
        Assert.Null(PreciosUi.PctDesdePrecio(12990m, 12990m));
        Assert.True(PreciosUi.MontoValido(1000m, 0));
        Assert.False(PreciosUi.MontoValido(1000.5m, 0));
        Assert.True(PreciosUi.MontoValido(1000.5m, 2));
        Assert.Equal(new DateOnly(2026, 2, 28), PreciosUi.FinDeMes(new DateOnly(2026, 2, 3)));
        // S93.3 (C3): porcentajes via FormatoCl.Porcentaje(p, 2) — "10,00 %" (antes Cantidad recortaba a "10 %").
        Assert.Equal("+10,00 % (termina en 990)", PreciosUi.TextoPrecioPendiente(new CambioPrecioDto(1, TipoCambioPrecio.PrecioOficial, PctAjuste: 10, Redondeo: RedondeoPrecioModo.Termina990)));
    }

    [Fact]
    public void Pendientes_OfertaYTerminar_SeExcluyen_YCuentan()
    {
        var p = new PendientesPrecio(Fila(new OfertaFilaDto(7, 9990m, 23, Hoy.AddDays(-5), null)));
        p.Aplicar([PreciosUi.TerminarOferta(p.Fila)]);
        p.Aplicar([PreciosUi.NuevaOferta(p.Fila, 9000m, null, null, null), PreciosUi.NuevoPrecio(p.Fila, 13990m)]);
        Assert.Null(p.Terminar);
        Assert.Equal(2, p.Cantidad);
        Assert.Equal(7, p.Oferta!.IdOfertaVista);
    }
}
