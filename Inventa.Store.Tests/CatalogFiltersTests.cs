using System.Net;
using System.Text;
using Bunit;
using CBP.Blazor.Formatting;
using Inventa.Store.Components;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S65.1 filtros de precio confiables: FormatoCl.TryParseCantidad + commit al confirmar (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class CatalogFiltersTests : IAsyncLifetime
{
    const string StorefrontJson = """{"nombreLegal":"Tienda Demo","nombreFantasia":null,"descripcionCorta":null,"descripcion":null,"logoUrl":null,"contacto":{},"redes":[],"horarios":[],"branding":null}""";
    const string CategoriesJson = """[{"idCategoria":2,"codigo":"ALM","nombre":"Almacén","cantidad":3}]""";
    const string BrandsJson = """[{"idMarca":3,"codigo":"ACME","nombre":"Acme","cantidad":12}]""";
    const string ProductsJson = """{"items":[{"idProd":7,"codigo":"YERBA-500","nombre":"Yerba 500g","idCategoria":2,"categoriaNombre":"Almacén","idMarca":3,"marcaNombre":"Acme","unidadNombre":"UN","imagenUrl":null,"precioDesde":12990,"tieneOferta":true,"precioFinalDesde":9990,"precioReferencia":12990,"pctDescuento":23,"ofertaHasta":"2026-10-31"}],"page":1,"pageSize":20,"totalCount":1,"totalPages":1}""";

    sealed class StubHandler : HttpMessageHandler
    {
        public List<string> Urls { get; } = new();
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Urls.Add(req.RequestUri?.PathAndQuery ?? "");
            var path = req.RequestUri?.AbsolutePath ?? "";
            var json = path.EndsWith("/storefront") ? StorefrontJson
                : path.EndsWith("/categories") ? CategoriesJson
                : path.EndsWith("/brands") ? BrandsJson
                : ProductsJson;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }

    readonly BunitContext _ctx = new();
    readonly StubHandler _handler = new();

    public CatalogFiltersTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        var http = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        _ctx.Services.AddSingleton(new CatalogApiService(http));
        _ctx.Services.AddSingleton(new StorefrontState(new StorefrontApiService(http), cfg));
    }

    IRenderedComponent<Catalog> RenderLoaded(string? url = null)
    {
        if (url != null) _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo(url);
        var cut = _ctx.Render<Catalog>();
        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"), TimeSpan.FromSeconds(15));
        return cut;
    }

    int ProductsCalls() => _handler.Urls.Count(u => u.Contains("/products"));

    void CommitMin(IRenderedComponent<Catalog> cut, string texto)
    {
        var input = cut.Find("[data-testid='store-pmin']");
        input.Input(texto);
        input.KeyDown("Enter");
    }

    void CommitMax(IRenderedComponent<Catalog> cut, string texto)
    {
        var input = cut.Find("[data-testid='store-pmax']");
        input.Input(texto);
        input.KeyDown("Enter");
    }

    // S65.1 FormatoCl: miles chilenos con punto o coma.
    [Theory]
    [InlineData("5.000", 5000)]
    [InlineData("5,000", 5000)]
    [InlineData("5000", 5000)]
    [InlineData("5.000.000", 5000000)]
    [InlineData(" 5.000 ", 5000)]
    public void Parse_MilesChilenos(string texto, decimal esperado)
    {
        Assert.True(FormatoCl.TryParseCantidad(texto, out var v));
        Assert.Equal(esperado, v);
    }

    // S65.1 FormatoCl: decimal real con punto o coma.
    [Theory]
    [InlineData("5.50", 5.50)]
    [InlineData("5,50", 5.50)]
    [InlineData("0,5", 0.5)]
    [InlineData(".5", 0.5)]
    public void Parse_DecimalReal(string texto, decimal esperado)
    {
        Assert.True(FormatoCl.TryParseCantidad(texto, out var v));
        Assert.Equal(esperado, v);
    }

    // S65.1 FormatoCl: con ambos separadores el último manda como decimal.
    [Theory]
    [InlineData("1.234,56", 1234.56)]
    [InlineData("1,234.56", 1234.56)]
    public void Parse_AmbosSeparadores_UltimoManda(string texto, decimal esperado)
    {
        Assert.True(FormatoCl.TryParseCantidad(texto, out var v));
        Assert.Equal(esperado, v);
    }

    // S65.1 FormatoCl: inválidos sin excepción.
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("abc")]
    [InlineData("12a34")]
    [InlineData("5.00.00")]
    [InlineData("5,5,5")]
    [InlineData(".")]
    [InlineData("-")]
    [InlineData("$5.000")]
    public void Parse_Invalidos_False(string? texto) =>
        Assert.False(FormatoCl.TryParseCantidad(texto, out _));

    [Fact]
    public void Normaliza_Acota_Y_Redondea()
    {
        Assert.Equal(0, FormatoCl.NormalizarCantidad(-5));
        Assert.Equal(FormatoCl.MaxCantidad, FormatoCl.NormalizarCantidad(1000000000));
        Assert.Equal(5.1235m, FormatoCl.NormalizarCantidad(5.12345m));
        Assert.Equal(5000, FormatoCl.NormalizarCantidad(5000));
        Assert.True(FormatoCl.TryParseCantidad("-5", out var neg));
        Assert.Equal(-5, neg);
    }

    [Fact]
    public void Cantidad_EcoEsCl()
    {
        Assert.Equal("5.000", FormatoCl.Cantidad(5000));
        Assert.Equal("0", FormatoCl.Cantidad(0));
        Assert.Equal("5,5", FormatoCl.Cantidad(5.5m));
    }

    // S65.1 fix 1: "5.000" tecleado filtra como 5000 (URL + API) con eco "5.000".
    [Fact]
    public void MinChileno_FiltraServidor_Y_Eco()
    {
        var cut = RenderLoaded();
        var n0 = ProductsCalls();
        CommitMin(cut, "5.000");
        cut.WaitForAssertion(() => Assert.True(ProductsCalls() > n0), TimeSpan.FromSeconds(15));
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Contains("pmin=5000", new Uri(nav.Uri).Query);
        Assert.Contains("precioMin=5000", _handler.Urls[^1]);
        cut.WaitForAssertion(() => Assert.Equal("5.000",
            cut.Find("[data-testid='store-pmin']").GetAttribute("value")), TimeSpan.FromSeconds(15));
    }

    // S65.1 fix 1: por tecla NO navega ni recarga (commit al confirmar).
    [Fact]
    public void Tecleo_SinCommit_NoNavega()
    {
        var cut = RenderLoaded();
        var n0 = ProductsCalls();
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        cut.Find("[data-testid='store-pmin']").Input("5.000");
        Assert.Equal(n0, ProductsCalls());
        Assert.DoesNotContain("pmin=", new Uri(nav.Uri).Query);
    }

    // S65.1 fix 1: blur también confirma.
    [Fact]
    public void Blur_Commitea()
    {
        var cut = RenderLoaded();
        var n0 = ProductsCalls();
        cut.Find("[data-testid='store-pmax']").Input("2500");
        cut.Find("[data-testid='store-pmax']").Blur();
        cut.WaitForAssertion(() => Assert.True(ProductsCalls() > n0), TimeSpan.FromSeconds(15));
        Assert.Contains("pmax=2500", new Uri(_ctx.Services.GetRequiredService<NavigationManager>().Uri).Query);
    }

    // S65.1 fix 1: texto inválido muestra mensaje y no navega.
    [Fact]
    public void TextoInvalido_Mensaje_SinNavegar()
    {
        var cut = RenderLoaded();
        var n0 = ProductsCalls();
        CommitMin(cut, "abc");
        Assert.Contains("Monto no válido.",
            cut.Find("[data-testid='store-price-format-error']").TextContent);
        Assert.Equal(n0, ProductsCalls());
        Assert.DoesNotContain("pmin=",
            new Uri(_ctx.Services.GetRequiredService<NavigationManager>().Uri).Query);
    }

    // S65.1 fix 1: mín>máx muestra mensaje y no navega ni recarga.
    [Fact]
    public void MinMayorQueMax_Mensaje_SinNavegar()
    {
        var cut = RenderLoaded();
        CommitMax(cut, "1000");
        cut.WaitForAssertion(() => Assert.Contains("pmax=1000",
            new Uri(_ctx.Services.GetRequiredService<NavigationManager>().Uri).Query),
            TimeSpan.FromSeconds(15));
        var n0 = ProductsCalls();
        CommitMin(cut, "5.000");
        Assert.Contains("mayor que el máximo",
            cut.Find("[data-testid='store-price-range-error']").TextContent);
        Assert.Equal(n0, ProductsCalls());
        Assert.DoesNotContain("pmin=",
            new Uri(_ctx.Services.GetRequiredService<NavigationManager>().Uri).Query);
    }

    // S65.1 fix 1: la URL inicializa los textos con eco es-CL.
    [Fact]
    public void Url_Inicializa_Textos()
    {
        var cut = RenderLoaded("/?pmin=1000&pmax=5000");
        Assert.Equal("1.000", cut.Find("[data-testid='store-pmin']").GetAttribute("value"));
        Assert.Equal("5.000", cut.Find("[data-testid='store-pmax']").GetAttribute("value"));
    }

    // S65.1 fix 2: sin badge "N en oferta" aunque haya ofertas en la página.
    [Fact]
    public void Hero_SinBadge_Ofertas() =>
        Assert.Empty(RenderLoaded().FindAll("[data-testid='store-offers-badge']"));

    // S65.1 fix 3: sin segundo buscador ni bloque info en la página.
    [Fact]
    public void Pagina_SinBuscador_NiBloqueInfo()
    {
        var cut = RenderLoaded();
        Assert.Empty(cut.FindAll("[data-testid='store-search']"));
        Assert.Empty(cut.FindAll("[data-testid='store-info']"));
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();
}


