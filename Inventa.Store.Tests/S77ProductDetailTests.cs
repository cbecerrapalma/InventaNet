using System.Net;
using System.Text;
using Bunit;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

// S77 R4': el detalle muestra como tachado la referencia congelada de la oferta
// ganadora (precioReferenciaValor), no el vigente recalculado. Sin oferta: sin tachado.
public sealed class S77ProductDetailTests
{
    const string DetailJson = """{"idProd":7,"codigo":"VES-P000015","nombre":"Poleras Rojo","idCategoria":26,"categoriaNombre":"Poleras","idMarca":27,"marcaNombre":"Marca propia","variantes":[{"idVarProd":11,"codigo":"VES-P000015-V01","nombre":"V01","presentaciones":[{"idPresProd":100,"descrip":"Unidad","unidad":"Unidad","precios":[],"ofertas":[{"idOferta":1,"precioOferta":4396,"fechaDesde":"2026-01-01","fechaHasta":null,"precioReferenciaValor":10990,"referenciaEsVigente":true,"pctDescuento":60}],"precioVigente":9990,"precioFinal":4396,"pctDescuento":60}]}],"preciosVigentes":[],"imagenUrl":null,"imagenes":[]}""";
    const string DetailSinOfertaJson = """{"idProd":8,"codigo":"ARROZ-1K","nombre":"Arroz 1k","idCategoria":2,"categoriaNombre":"Almac","idMarca":null,"marcaNombre":null,"variantes":[{"idVarProd":12,"codigo":"A-V01","nombre":"V01","presentaciones":[{"idPresProd":101,"descrip":"Unidad","unidad":"UN","precios":[],"ofertas":[],"precioVigente":1500,"precioFinal":1500,"pctDescuento":null}]}],"preciosVigentes":[],"imagenUrl":null,"imagenes":[]}""";

    sealed class StubHandler(Func<string> json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json(), Encoding.UTF8, "application/json")
            });
    }

    private static BunitContext Ctx(string json)
    {
        var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        var http = new HttpClient(new StubHandler(() => json)) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        ctx.Services.AddSingleton(new CatalogApiService(http));
        ctx.Services.AddSingleton(new StorefrontState(new StorefrontApiService(http), cfg));
        return ctx;
    }

    [Fact]
    public void DetalleConOferta_TachadoEsReferenciaCongelada()
    {
        // Sin using: BunitContext.Dispose() síncrono falla con MudBlazor
        // (KeyInterceptorService solo IAsyncDisposable); mismo patrón que CatalogPageTests.
        var ctx = Ctx(DetailJson);
        var cut = ctx.Render<ProductDetail>(p => p.Add(x => x.Id, 7));
        cut.WaitForAssertion(() => cut.Find("[data-testid='detail-final-price']"));
        // Final 4396 formateado; tachado = 10990 (referencia), NO 9990 (vigente actual).
        Assert.Contains("$4.396", cut.Find("[data-testid='detail-final-price']").TextContent);
        var tachado = cut.Find("[data-testid='detail-reference-price']").TextContent;
        Assert.Contains("$10.990", tachado);
        Assert.DoesNotContain("$9.990", tachado);
        Assert.Contains("-60 %", cut.Find("[data-testid='detail-discount']").TextContent); // S93.3 (C3): Porcentaje es-CL con espacio.
    }

    [Fact]
    public void DetalleSinOferta_SinTachado()
    {
        var ctx = Ctx(DetailSinOfertaJson);
        var cut = ctx.Render<ProductDetail>(p => p.Add(x => x.Id, 8));
        cut.WaitForAssertion(() => cut.Find("[data-testid='detail-final-price']"));
        Assert.Empty(cut.FindAll("[data-testid='detail-reference-price']"));
    }
}

