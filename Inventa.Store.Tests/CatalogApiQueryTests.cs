using System.Net;
using System.Text;
using Inventa.Store.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S64: filtros/orden viajan al servidor (contrato S63). Handler stub que captura URLs.</summary>
public sealed class CatalogApiQueryTests
{
    sealed class CaptureHandler : HttpMessageHandler
    {
        public List<string> Urls { get; } = new();
        public string ProductsJson { get; set; } = """{"items":[],"page":1,"pageSize":20,"totalCount":0,"totalPages":0}""";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Urls.Add(req.RequestUri?.PathAndQuery ?? "");
            var path = req.RequestUri?.AbsolutePath ?? "";
            var json = path.EndsWith("/brands") ? "[]" : ProductsJson;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            });
        }
    }

    static (CatalogApiService Api, CaptureHandler Handler) Setup()
    {
        var h = new CaptureHandler();
        var http = new HttpClient(h) { BaseAddress = new Uri("http://localhost/") };
        return (new CatalogApiService(http), h);
    }

    [Fact]
    public async Task Defaults_SeOmiten()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync("q", 1, 20, null, false, null, null, null, null, Xunit.TestContext.Current.CancellationToken);
        var url = Assert.Single(h.Urls);
        Assert.Equal("/api/public/catalog/products?q=q&page=1&pageSize=20", url);
    }

    [Fact]
    public async Task OrdenDefecto_Null_SeOmite()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync(null, 1, 20, null, false, null, null, null, "defecto", Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain("orden=", Assert.Single(h.Urls));
    }

    [Fact]
    public async Task SoloOfertas_Viaja()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync(null, 2, 50, 7, true, null, null, null, null, Xunit.TestContext.Current.CancellationToken);
        var url = Assert.Single(h.Urls);
        Assert.Contains("idCategoria=7", url);
        Assert.Contains("soloOfertas=true", url);
    }

    [Fact]
    public async Task Rango_InvarianteConPunto()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync(null, 1, 20, null, false, 100.5m, 5000m, null, null, Xunit.TestContext.Current.CancellationToken);
        var url = Assert.Single(h.Urls);
        Assert.Contains("precioMin=100.5", url);
        Assert.Contains("precioMax=5000", url);
    }

    [Fact]
    public async Task Marca_CeroONegativa_SeOmite_PositivaViaja()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync(null, 1, 20, null, false, null, null, 0, null, Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain("idMarca=", Assert.Single(h.Urls));
        await api.GetProductsAsync(null, 1, 20, null, false, null, null, 3, null, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains("idMarca=3", h.Urls[^1]);
    }

    [Theory]
    [InlineData("precio_asc")]
    [InlineData("precio_desc")]
    [InlineData("descuento_desc")]
    [InlineData("nombre_asc")]
    public async Task Orden_Conocido_Viaja(string orden)
    {
        var (api, h) = Setup();
        await api.GetProductsAsync(null, 1, 20, null, false, null, null, null, orden, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains($"orden={orden}", Assert.Single(h.Urls));
    }

    [Fact]
    public async Task Q_SeCodifica()
    {
        var (api, h) = Setup();
        await api.GetProductsAsync("zapatos ñ", 1, 20, null, false, null, null, null, null, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains("q=zapatos%20%C3%B1", Assert.Single(h.Urls));
    }

    [Fact]
    public void BuildProductsUrl_TodoJunto()
    {
        var url = CatalogApiService.BuildProductsUrl("yerba", 3, 50, 2, true, 1000m, 5000m, 3, "precio_asc");
        Assert.Equal("api/public/catalog/products?q=yerba&page=3&pageSize=50&idCategoria=2&soloOfertas=true&precioMin=1000&precioMax=5000&idMarca=3&orden=precio_asc", url);
    }

    [Fact]
    public async Task GetBrandsAsync_RutaBrands()
    {
        var (api, h) = Setup();
        var brands = await api.GetBrandsAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("/api/public/catalog/brands", Assert.Single(h.Urls));
        Assert.NotNull(brands);
    }
}

