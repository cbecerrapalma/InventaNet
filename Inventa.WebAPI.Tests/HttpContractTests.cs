using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S58.03 P0-C: contrato HTTP en host real (WebApplicationFactory), sin DB.
// El único IHostedService del producto es el probe fail-fast de SQL
// (InventaDatabase.DatabaseProbeHostedService). El host de test lo retira
// —SOLO en el host de test, producto intacto— porque estos 3 contratos no
// tocan DB: /health no tiene checks y el 401 lo emite el middleware JWT
// antes de llegar al controller. Los 401/403/404 de negocio con DB quedan
// para cuando el entorno opere (OfficialLocal arriba).
public sealed class HttpContractTests : IClassFixture<HttpContractTests.TestFactory>
{
    private readonly HttpClient _client;

    public HttpContractTests(TestFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_Responde200()
    {
        var r = await _client.GetAsync("/health", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
    }

    [Fact]
    public async Task RutaInexistente_Responde404()
    {
        var r = await _client.GetAsync("/api/s5803-no-existe", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
    }

    [Fact]
    public async Task Negocio_SinToken_Responde401_AntesDeDB()
    {
        var r = await _client.GetAsync("/api/categorias", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Negocio_BearerBasura_Responde401()
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/categorias");
        req.Headers.Authorization = new("Bearer", "s5803-basura-sin-forma-jwt");
        var r = await _client.SendAsync(req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    [Fact]
    public async Task Negocio_JwtConFirmaFalsa_Responde401()
    {
        // Token con forma JWT (3 segmentos) pero contenido que el issuer jamás
        // emitió: el middleware lo rechaza antes del controller (sin DB).
        // NOTA: primer segmento deliberadamente NO-eyJ para no parecer un JWT
        // real (governance Assert-No-SecretLeakage). Garantía: tokens forjados
        // nunca llegan a negocio.
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/categorias");
        req.Headers.Authorization = new("Bearer",
            "bm90.dnd3aWQ.s5803firmafalsa");
        var r = await _client.SendAsync(req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
    }

    public sealed class TestFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureServices(s =>
            {
                // ÚNICO hosted service del producto = probe SQL: fuera solo aquí.
                foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService)).ToList())
                    s.Remove(d);
            });
    }
}

