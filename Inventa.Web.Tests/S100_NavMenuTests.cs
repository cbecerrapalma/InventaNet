using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using CBP.Blazor.Acceso;
using Inventa.Web.Layout;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>S100 D7 — El NavMenu se dibuja con el árbol de <c>GET api/acceso/menu</c> (PassPlat); sin enlaces ni permisos escritos.</summary>
public sealed class S100_NavMenuTests
{
    private const string Arbol = """
        [{"codigo":"INVENTA","nombre":"INVENTA","orden":0,"esVisibleMenu":true,"hijos":[
          {"codigo":"INVENTA.INICIO","nombre":"Inicio","ruta":"/","icono":"Dashboard","orden":1,"esVisibleMenu":true,"hijos":[]},
          {"codigo":"INVENTA.PRODUCTOS","nombre":"Productos","orden":101,"esVisibleMenu":true,"hijos":[
            {"codigo":"INVENTA.PRODUCTOS.PRINCIPAL","nombre":"Productos","ruta":"/productos","icono":"Inventory2","orden":2,"esVisibleMenu":true,"hijos":[]},
            {"codigo":"INVENTA.PRODUCTOS.DETALLE","nombre":"Detalle","ruta":"/productos/{Id}","orden":3,"esVisibleMenu":true,"hijos":[]}]},
          {"codigo":"INVENTA.INVENTARIO","nombre":"Inventario","orden":102,"esVisibleMenu":true,"hijos":[
            {"codigo":"INVENTA.INVENTARIO.STOCK","nombre":"Stock","ruta":"/inventario/stock","icono":"Inventory","orden":4,"esVisibleMenu":true,"hijos":[]},
            {"codigo":"INVENTA.INVENTARIO.LOTES","nombre":"Lotes","ruta":"/inventario/lotes","icono":"QrCode","orden":5,"esVisibleMenu":true,"hijos":[]}]}]}]
        """;

    private sealed class ApiMenu(HttpStatusCode estado, string json, bool falla = false) : HttpMessageHandler
    {
        public int Llamadas { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            Llamadas++;
            if (falla) throw new HttpRequestException("sin red");
            return Task.FromResult(new HttpResponseMessage(estado) { Content = new StringContent(json, Encoding.UTF8, "application/json") });
        }
    }

    private sealed class StubStorage(string token) : ITokenStorage
    {
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(4);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(2);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearTenantIdsAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static string Token(bool maestro)
    {
        object payload = maestro ? new { permissions = new[] { "INVENTA_PRODUCTOS_PRINCIPAL_VER" }, is_system = "true" } : new { permissions = new[] { "INVENTA_PRODUCTOS_PRINCIPAL_VER" } };
        return "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))) + ".";
    }

    private static BunitContext Contexto(ApiMenu api, bool maestro = false, bool autenticado = true)
    {
        var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        ctx.Services.AddSingleton<ITokenStorage>(new StubStorage(Token(maestro)));
        ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        ctx.Services.AddSingleton<PermissionService>();
        ctx.Services.AddSingleton(new MenuAccesoCliente(new HttpClient(api) { BaseAddress = new Uri("https://api.prueba/") }, "api/acceso/menu"));
        var auth = ctx.AddAuthorization();
        if (autenticado) auth.SetAuthorized("admin_abarrotes");
        return ctx;
    }

    private static string[] TestIds(IRenderedComponent<NavMenu> cut) =>
        cut.FindAll("[data-testid]").Select(e => e.GetAttribute("data-testid")!).ToArray();

    [Fact]
    public async Task NavMenu_dibuja_el_menu_de_api_acceso_menu()
    {
        var api = new ApiMenu(HttpStatusCode.OK, Arbol);
        await using var ctx = Contexto(api);

        var cut = ctx.Render<NavMenu>();
        cut.WaitForAssertion(() => Assert.Contains("nav-inventa-inicio", TestIds(cut)), TimeSpan.FromSeconds(5));

        // Raíz transparente, grupos con 2 enlaces con título, ruta con parámetro fuera, sin aviso ni «Acceso y permisos».
        // Productos tiene un solo enlace visible (el detalle lleva {Id}): sin título; Inventario tiene dos: con título.
        Assert.Equal(["nav-menu", "nav-inventa-inicio", "nav-inventa-productos-principal",
                      "nav-group-inventa-inventario", "nav-inventa-inventario-stock", "nav-inventa-inventario-lotes"], TestIds(cut));
        Assert.Empty(cut.FindAll("[data-testid='nav-error']"));
        Assert.Empty(cut.FindAll("[data-testid='nav-config-acceso']"));
        Assert.Equal(1, api.Llamadas);
    }

    [Fact]
    public async Task NavMenu_sin_menu_muestra_inicio_y_aviso()
    {
        foreach (var api in new[] { new ApiMenu(HttpStatusCode.BadGateway, "{}"), new ApiMenu(HttpStatusCode.OK, "[]", falla: true) })
        {
            await using var ctx = Contexto(api);
            var cut = ctx.Render<NavMenu>();
            cut.WaitForAssertion(() => Assert.Single(cut.FindAll("[data-testid='nav-error']")), TimeSpan.FromSeconds(5));

            Assert.Contains("Menú no disponible: no se pudo consultar PassPlat.", cut.Find("[data-testid='nav-error']").TextContent);
            Assert.Single(cut.FindAll("[data-testid='nav-inicio']"));
            Assert.Empty(cut.FindAll("[data-testid^='nav-inventa-']"));
        }
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task NavMenu_muestra_Acceso_y_permisos_solo_a_maestro(bool maestro)
    {
        await using var ctx = Contexto(new ApiMenu(HttpStatusCode.OK, Arbol), maestro);

        var cut = ctx.Render<NavMenu>();
        cut.WaitForAssertion(() => Assert.Contains("nav-inventa-inicio", TestIds(cut)), TimeSpan.FromSeconds(5));

        Assert.Equal(maestro ? 1 : 0, cut.FindAll("[data-testid='nav-config-acceso']").Count);
    }

    [Fact]
    public async Task NavMenu_sin_sesion_solo_muestra_inicio_y_no_consulta()
    {
        var api = new ApiMenu(HttpStatusCode.OK, Arbol);
        await using var ctx = Contexto(api, autenticado: false);

        var cut = ctx.Render<NavMenu>();

        Assert.Single(cut.FindAll("[data-testid='nav-inicio']"));
        Assert.Empty(cut.FindAll("[data-testid='nav-error']"));
        Assert.Equal(0, api.Llamadas);
    }
}
