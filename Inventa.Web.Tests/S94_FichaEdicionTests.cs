// S94 K6/K7: edición del catálogo desde la ficha (bUnit, MudBlazor 9).
// - «Nueva variante», «Editar» y «Códigos» según permisos (hidden ≠ authz; el servidor valida).
// - Crear producto envía la primera variante con el precio leído en es-CL aunque el hilo esté en en-US.
// - Códigos: el principal no se puede quitar ni volver a marcar; marcar otro llama al endpoint nuevo.
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Inventa.Web.Models.Productos;
using Inventa.Web.Pages.Productos;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Precios;
using Inventa.Web.Services.Productos;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Inventa.Web.Shared.Product;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

public sealed class S94_FichaEdicionTests : IAsyncDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly StubHandler _http = new();
    private readonly StubStorage _storage = new();
    private readonly CultureInfo _prev = CultureInfo.CurrentCulture;

    private const string FichaJson = """
    {"idProd":10,"codigo":"P1","nombre":"Polera","categoria":"Ropa","marca":null,"unidad":"Unidad (UN)","esPesable":false,"manejaLote":false,
     "diasVencimiento":null,"activo":true,"visibleStore":false,"imagenes":0,"imagenPrincipal":null,"estado":"Pendiente","razones":[],
     "variantes":[{"idVarProd":20,"codigo":"4","nombre":"Talla 4","activo":true,"costoProm":0,
       "presentacion":{"idPresProd":30,"descrip":"Polera / Talla 4","activo":true,"esVenta":true},
       "codigos":[{"codigo":"2004000000013","tipo":"Interno","principal":true,"idCodBar":40,"tipoCod":3},
                  {"codigo":"7801234567894","tipo":"Normal","principal":false,"idCodBar":41,"tipoCod":1}],
       "precioVigente":{"id":50,"valor":12990,"desde":"2026-10-16"},"oferta":null,"stockDisponible":0}]}
    """;

    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; }
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(Token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(4);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(2);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearTenantIdsAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        public List<(HttpMethod Metodo, string Ruta, string Cuerpo)> Escrituras { get; } = [];
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var path = req.RequestUri?.AbsolutePath ?? "";
            var json = "{}";
            if (req.Method != HttpMethod.Get)
            {
                Escrituras.Add((req.Method, path, req.Content is null ? "" : await req.Content.ReadAsStringAsync(ct)));
                json = """{"idProd":10,"idVarProd":20,"idPresProd":30,"idCodBar":41,"codigoBarras":"2004000000020","idPrecio":51}""";
            }
            else if (path.EndsWith("/ficha")) json = FichaJson;
            else if (path.Contains("api/precios/grilla")) json = """{"items":[],"total":0,"page":1,"pageSize":100,"moneda":"CLP","decimales":0,"hoy":"2026-10-16"}""";
            else if (path.EndsWith("api/catalogos/categorias")) json = """[{"idCategoria":1,"nombre":"Ropa","codigo":"ROPA","activo":true}]""";
            else if (path.EndsWith("api/catalogos/marcas")) json = "[]";
            else if (path.EndsWith("api/catalogos/unidades")) json = """[{"idUnidad":1,"codigo":"UN","nombre":"Unidad"}]""";
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(json, Encoding.UTF8, "application/json") };
        }
    }

    private static string FakeJwt(params string[] permissions) =>
        $"e30.{Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { permissions })))}.";

    public S94_FichaEdicionTests()
    {
        CultureInfo.CurrentCulture = new CultureInfo("en-US");
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton<ITokenStorage>(_storage);
        _ctx.Services.AddSingleton(_ => new ApiClient(new HttpClient(_http) { BaseAddress = new Uri("https://localhost:5002/") }));
        _ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        _ctx.Services.AddSingleton<PermissionService>();
        _ctx.Services.AddSingleton<TenantContextService>();
        _ctx.Services.AddSingleton<PreciosApiService>();
        _ctx.Services.AddSingleton<ProductosApiService>();
    }

    public async ValueTask DisposeAsync()
    {
        CultureInfo.CurrentCulture = _prev;
        await _ctx.DisposeAsync();
    }

    private IRenderedComponent<ProductoDetalle> AbrirVariantes(params string[] permisos)
    {
        _storage.Token = FakeJwt(["INVENTA_PRODUCTOS_DETALLE_VER", .. permisos]);
        _ctx.Render<MudPopoverProvider>();
        _ctx.Render<MudDialogProvider>();
        var cut = _ctx.Render<ProductoDetalle>(p => p.Add(x => x.Id, "10"));
        cut.WaitForAssertion(() => Assert.Contains(cut.FindAll(".mud-tab"), t => t.TextContent.Contains("Variantes")));
        cut.FindAll(".mud-tab").First(t => t.TextContent.Contains("Variantes")).Click();
        cut.WaitForElement("[data-testid=tabla-variantes]");
        return cut;
    }

    [Fact]
    public void Ficha_ConPermisos_MuestraNuevaVarianteYAccionesPorVariante()
    {
        var cut = AbrirVariantes("INVENTA_PRODUCTOS_DETALLE_CREAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR");
        Assert.Single(cut.FindAll("[data-testid=nueva-variante]"));
        Assert.Single(cut.FindAll("button[aria-label='Editar variante 4']"));
        Assert.Single(cut.FindAll("button[aria-label='Códigos de 4']"));
    }

    [Fact]
    public void Ficha_SoloLectura_SinAccionesDeEdicion()
    {
        var cut = AbrirVariantes();
        Assert.Empty(cut.FindAll("[data-testid=nueva-variante]"));
        Assert.Empty(cut.FindAll("button[aria-label='Editar variante 4']"));
        Assert.Empty(cut.FindAll("button[aria-label='Códigos de 4']"));
    }

    [Fact]
    public async Task NuevoProducto_EnviaLaPrimeraVariante_ConPrecioEsCl()
    {
        _storage.Token = FakeJwt("INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRODUCTOS_DETALLE_CREAR");
        _ctx.Render<MudPopoverProvider>();
        var prov = _ctx.Render<MudDialogProvider>();
        var dialogos = _ctx.Services.GetRequiredService<IDialogService>();
        IDialogReference? r = null;
        await prov.InvokeAsync(async () => r = await dialogos.ShowAsync<ProductFormDialog>("Nuevo producto"));
        prov.WaitForElement("input[data-testid=var-precio]");

        prov.Find("input[data-testid=prod-codigo]").Input("P1");
        prov.Find("input[data-testid=prod-nombre]").Input("Polera");
        prov.Find("input[data-testid=var-codigo]").Input("4");
        prov.Find("input[data-testid=var-nombre]").Input("Talla 4");
        prov.Find("input[data-testid=var-precio]").Change("12.990");
        prov.Find("button[aria-label='Guardar producto']").Click();

        var res = await r!.Result;
        Assert.False(res!.Canceled);
        Assert.Equal(10L, res.Data);
        var (metodo, ruta, cuerpo) = Assert.Single(_http.Escrituras);
        Assert.Equal((HttpMethod.Post, "/api/productos"), (metodo, ruta));
        using var doc = JsonDocument.Parse(cuerpo);
        var v = doc.RootElement.GetProperty("variante");
        Assert.Equal(12990m, v.GetProperty("precioVenta").GetDecimal());
        Assert.Equal(JsonValueKind.Null, v.GetProperty("codigoBarras").ValueKind);
        Assert.Equal(("4", "Talla 4"), (v.GetProperty("codigo").GetString(), v.GetProperty("nombre").GetString()));
    }

    [Fact]
    public async Task Codigos_PrincipalSinAcciones_YMarcarOtroLlamaAlEndpoint()
    {
        _storage.Token = FakeJwt("INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRODUCTOS_DETALLE_EDITAR");
        var variante = JsonSerializer.Deserialize<FichaProductoDto>(FichaJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Variantes[0];
        _ctx.Render<MudPopoverProvider>();
        var prov = _ctx.Render<MudDialogProvider>();
        var dialogos = _ctx.Services.GetRequiredService<IDialogService>();
        await prov.InvokeAsync(async () => await dialogos.ShowAsync<CodigosVarianteDialog>("Códigos", new DialogParameters { ["IdProd"] = 10L, ["Variante"] = variante }));
        prov.WaitForElement("[data-testid=tabla-codigos]");

        Assert.Empty(prov.FindAll("button[aria-label='Quitar 2004000000013']"));
        Assert.Empty(prov.FindAll("button[aria-label='Marcar 2004000000013 como principal']"));
        Assert.Single(prov.FindAll("button[aria-label='Quitar 7801234567894']"));

        prov.Find("button[aria-label='Marcar 7801234567894 como principal']").Click();
        prov.WaitForAssertion(() => Assert.Contains(_http.Escrituras, e => e.Metodo == HttpMethod.Put && e.Ruta == "/api/productos/codbar/41/principal"));
    }
}
