// S94 K0 (deuda S93.4): horarios de la tienda pública en el backoffice.
// - DayOfWeek (0 = Domingo) y orden de lunes a domingo.
// - Por defecto quedan cerrados sábado (6) y domingo (0).
// - Tenant sin perfil de tienda (API 404): igual se ofrecen los 7 días.
using System.Net;
using System.Text;
using System.Text.Json;
using AngleSharp.Html.Dom;
using Bunit;
using Inventa.Web.Pages.Configuracion;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Storefront;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

public sealed class S94_StorefrontHorariosTests : IAsyncDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly StubHandler _http = new();

    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; } = FakeJwt("INVENTA_CONFIGURACION_STOREFRONT_VER", "INVENTA_CONFIGURACION_STOREFRONT_EDITAR");
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
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;
        public string Json { get; set; } = "{}";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            var body = Status == HttpStatusCode.OK ? Json : """{"status":404,"title":"Not Found","detail":"Sin perfil"}""";
            var media = Status == HttpStatusCode.OK ? "application/json" : "application/problem+json";
            return Task.FromResult(new HttpResponseMessage(Status) { Content = new StringContent(body, Encoding.UTF8, media) });
        }
    }

    private static string FakeJwt(params string[] permissions) =>
        $"e30.{Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(new { permissions })))}.";

    public S94_StorefrontHorariosTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton<ITokenStorage, StubStorage>();
        _ctx.Services.AddSingleton(_ => new ApiClient(new HttpClient(_http) { BaseAddress = new Uri("https://localhost:5002/") }));
        _ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        _ctx.Services.AddSingleton<PermissionService>();
        _ctx.Services.AddSingleton<StorefrontApiService>();
    }

    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();

    private IReadOnlyList<(string Dia, bool Cerrado)> Filas()
    {
        var cut = _ctx.Render<StorefrontConfig>();
        cut.WaitForElement("[data-testid=tabla-horarios] tbody tr");
        return cut.FindAll("[data-testid=tabla-horarios] tbody tr")
            .Select(tr => (tr.QuerySelector("td")!.TextContent.Trim(),
                           ((IHtmlInputElement)tr.QuerySelector("input[type=checkbox]")!).IsChecked))
            .ToList();
    }

    [Fact]
    public void Tenant404_GeneraLos7Dias_DeLunesADomingo_ConFinDeSemanaCerrado()
    {
        _http.Status = HttpStatusCode.NotFound;
        var filas = Filas();
        Assert.Equal(["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"], filas.Select(f => f.Dia).ToArray());
        Assert.Equal([false, false, false, false, false, true, true], filas.Select(f => f.Cerrado).ToArray());
    }

    [Fact]
    public void SinHorariosGuardados_PorDefectoCierraSabadoYDomingo()
    {
        _http.Json = """{"p":null,"social":[],"hours":[]}""";
        var filas = Filas();
        Assert.Equal(7, filas.Count);
        Assert.Equal(("Sábado", true), filas[5]);
        Assert.Equal(("Domingo", true), filas[6]);
        Assert.All(filas.Take(5), f => Assert.False(f.Cerrado));
    }

    [Fact]
    public void DiaSemana0_SeMuestraComoDomingo_EnLaUltimaFila()
    {
        _http.Json = """
        {"p":null,"social":[],"hours":[
          {"diaSemana":0,"cerrado":true},
          {"diaSemana":1,"apertura1":"10:00:00","cierre1":"19:00:00","cerrado":false},
          {"diaSemana":6,"apertura1":"10:00:00","cierre1":"14:00:00","cerrado":false}]}
        """;
        var filas = Filas();
        Assert.Equal(["Lunes", "Sábado", "Domingo"], filas.Select(f => f.Dia).ToArray());
        Assert.Equal(("Domingo", true), filas[^1]);
    }

    [Fact]
    public void DiasPorDefecto_UsaDayOfWeek()
    {
        var dias = StorefrontConfig.DiasPorDefecto();
        Assert.Equal(Enumerable.Range(0, 7).Select(d => (byte)d), dias.Select(d => d.DiaSemana));
        Assert.Equal([(byte)DayOfWeek.Sunday, (byte)DayOfWeek.Saturday], dias.Where(d => d.Cerrado).Select(d => d.DiaSemana).ToArray());
    }
}
