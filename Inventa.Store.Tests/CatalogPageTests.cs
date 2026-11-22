using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Inventa.Store.Models;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S64: página Catalog presenta el precio final del servidor y los
/// filtros de servidor viajan en la URL y al API (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class CatalogPageTests : IAsyncLifetime
{
    const string StorefrontJson = """{"nombreLegal":"Tienda Demo","nombreFantasia":null,"descripcionCorta":null,"descripcion":null,"logoUrl":null,"contacto":{},"redes":[],"horarios":[],"branding":null}""";
    const string CategoriesJson = """[{"idCategoria":2,"codigo":"ALM","nombre":"Almacén","cantidad":3}]""";
    const string BrandsJson = """[{"idMarca":3,"codigo":"ACME","nombre":"Acme","cantidad":12}]""";
    const string ProductsJson = """{"items":[{"idProd":7,"codigo":"YERBA-500","nombre":"Yerba 500g","idCategoria":2,"categoriaNombre":"Almacén","idMarca":3,"marcaNombre":"Acme","unidadNombre":"UN","imagenUrl":null,"precioDesde":12990,"tieneOferta":true,"precioFinalDesde":9990,"precioReferencia":12990,"pctDescuento":23,"ofertaHasta":"2026-10-31"},{"idProd":8,"codigo":"ARROZ-1K","nombre":"Arroz 1k","idCategoria":2,"categoriaNombre":"Almacén","idMarca":null,"marcaNombre":null,"unidadNombre":null,"imagenUrl":null,"precioDesde":1500,"tieneOferta":false},{"idProd":9,"codigo":"SINPRECIO","nombre":"Sin precio","idCategoria":2,"categoriaNombre":"Almacén","idMarca":null,"marcaNombre":null,"unidadNombre":null,"imagenUrl":null,"precioDesde":null,"tieneOferta":false}],"page":1,"pageSize":20,"totalCount":3,"totalPages":1}""";

    sealed class StubHandler : HttpMessageHandler
    {
        public List<string> Urls { get; } = new();
        public HttpStatusCode ProductsStatusCode { get; set; } = HttpStatusCode.OK;
        public string ExceptionMessage { get; set; } = "Internal Database Timeout Exception StackTrace Detail";

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Urls.Add(req.RequestUri?.PathAndQuery ?? "");
            var path = req.RequestUri?.AbsolutePath ?? "";
            if (path.Contains("/products") && ProductsStatusCode != HttpStatusCode.OK)
            {
                return Task.FromResult(new HttpResponseMessage(ProductsStatusCode)
                {
                    Content = new StringContent(ExceptionMessage, Encoding.UTF8, "text/plain")
                });
            }
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

    public CatalogPageTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        var http = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        var storefrontApi = new StorefrontApiService(http);
        var storefrontState = new StorefrontState(storefrontApi, cfg);
        _ctx.Services.AddSingleton(storefrontApi);
        _ctx.Services.AddSingleton(new CatalogApiService(http));
        _ctx.Services.AddSingleton(storefrontState);
        
        // S83: pre-poblar categorías en StorefrontState para que Catalog las use (simula MainLayout)
        var cats = JsonSerializer.Deserialize<List<PublicCategoryDto>>(CategoriesJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        storefrontState.SetCategorias(CategoryTree.Build(cats), cats);
    }

    IRenderedComponent<Catalog> Render(Action<ComponentParameterCollectionBuilder<Catalog>>? extra = null) =>
        _ctx.Render<Catalog>(p => extra?.Invoke(p));

    IRenderedComponent<Catalog> RenderLoaded(Action<ComponentParameterCollectionBuilder<Catalog>>? extra = null)
    {
        var cut = Render(extra);
        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"));
        return cut;
    }

    static string TextoCard(AngleSharp.Dom.IElement card, string testid) =>
        Assert.IsAssignableFrom<AngleSharp.Dom.IElement>(card.QuerySelector($"[data-testid='{testid}']")).TextContent;

    [Fact]
    public void CardOferta_MuestraFinal_Referencia_Descuento()
    {
        var cut = RenderLoaded();
        var card = cut.FindAll("[data-testid='product-card']")[0];
        Assert.Contains("$9.990", TextoCard(card, "store-card-final"));
        Assert.Contains("Desde", TextoCard(card, "store-card-final"));
        Assert.Contains("$12.990", TextoCard(card, "store-card-reference"));
        Assert.Contains("-23 %", TextoCard(card, "store-card-discount")); // S93.3 (C3): Porcentaje es-CL con espacio.
    }

    [Fact]
    public void CardSinOferta_FallbackLegacy_SinChip()
    {
        var cut = RenderLoaded();
        var card = cut.FindAll("[data-testid='product-card']")[1];
        Assert.Contains("Desde", TextoCard(card, "store-card-legacy"));
        Assert.Contains("$1.500", TextoCard(card, "store-card-legacy"));
        Assert.Empty(card.QuerySelectorAll("[data-testid='store-card-discount']"));
        Assert.Empty(card.QuerySelectorAll("[data-testid='store-card-offer']"));
    }

    [Fact]
    public void CardSinPrecio_MuestraConsultar()
    {
        var cut = RenderLoaded();
        var card = cut.FindAll("[data-testid='product-card']")[2];
        Assert.Contains("Precio a consultar", TextoCard(card, "store-card-noprice"));
    }

    [Fact]
    public void Categoria_MuestraCantidad()
    {
        // S83: chips solo se muestran fuera de home (EsHome=false)
        _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo("/?q=test");
        var cut = RenderLoaded();
        Assert.Contains("Almacén (3)", cut.Find("[data-testid='store-cat-chip']").TextContent);
    }

    [Fact]
    public void QueryOfertas_ViajaAlServidor()
    {
        _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo("/?ofertas=true");
        var cut = Render();
        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"));
        Assert.Contains(_handler.Urls.Where(u => u.Contains("/products")), u => u.Contains("soloOfertas=true"));
    }

    [Fact]
    public void QueryOrdenMarcaRango_ViajanAlServidor()
    {
        _ctx.Services.GetRequiredService<NavigationManager>()
            .NavigateTo("/?orden=precio_asc&marca=3&pmin=1000&pmax=5000");
        var cut = Render();
        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"));
        var url = Assert.Single(_handler.Urls, u => u.Contains("/products"));
        Assert.Contains("orden=precio_asc", url);
        Assert.Contains("idMarca=3", url);
        Assert.Contains("precioMin=1000", url);
        Assert.Contains("precioMax=5000", url);
    }

    [Fact]
    public async Task ToggleOfertas_PublicaUrl_Y_RecargaServidor()
    {
        var cut = RenderLoaded();
        var n0 = _handler.Urls.Count(u => u.Contains("/products"));
        var sw = cut.FindComponent<MudBlazor.MudSwitch<bool>>();
        await cut.InvokeAsync(() => sw.Instance.ValueChanged.InvokeAsync(true));
        cut.WaitForAssertion(() => Assert.True(
            _handler.Urls.Count(u => u.Contains("/products")) > n0 &&
            _handler.Urls[^1].Contains("soloOfertas=true")));
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Contains("ofertas=true", new Uri(nav.Uri).Query);
    }

    [Fact]
    public async Task Catalog503_MuestraAlertaWarning_SinExMessage_Y_BotonReintentarFunciona()
    {
        _handler.ProductsStatusCode = HttpStatusCode.ServiceUnavailable;
        var cut = Render();
        cut.WaitForAssertion(() => cut.Find("[data-testid='store-error-503']"));

        var alertText = cut.Find("[data-testid='store-error-503']").TextContent;
        Assert.Contains("El catálogo no está disponible en este momento.", alertText);
        Assert.DoesNotContain("Internal Database Timeout Exception StackTrace Detail", cut.Markup);

        // Ahora recuperamos el servicio (200 OK) y presionamos el botón Reintentar
        _handler.ProductsStatusCode = HttpStatusCode.OK;
        var retryBtn = cut.Find("[data-testid='store-retry-button']");
        await cut.InvokeAsync(() => retryBtn.Click());

        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"));
    }

    // S101: el layout carga las categorías después del primer render de la portada.
    [Fact]
    public async Task Home_MuestraCategorias_Cuando_Llegan_Tras_El_Primer_Render()
    {
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        var http = new HttpClient(_handler) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        var storefrontApi = new StorefrontApiService(http);
        var state = new StorefrontState(storefrontApi, cfg);
        ctx.Services.AddSingleton(storefrontApi);
        ctx.Services.AddSingleton(new CatalogApiService(http));
        ctx.Services.AddSingleton(state);

        var cut = ctx.Render<Catalog>();
        cut.WaitForAssertion(() => cut.Find("[data-testid='product-card']"));
        Assert.Empty(cut.FindAll("[data-testid='store-home-cat-tile']"));

        const string UnaRaizConHijas = """[{"idCategoria":10,"codigo":"V","nombre":"Vestuario","cantidad":9,"idPadre":null,"orden":0,"imagenUrl":null},{"idCategoria":11,"codigo":"V1","nombre":"Poleras","cantidad":4,"idPadre":10,"orden":0,"imagenUrl":null},{"idCategoria":12,"codigo":"V2","nombre":"Pantalones","cantidad":5,"idPadre":10,"orden":1,"imagenUrl":null}]""";
        var cats = JsonSerializer.Deserialize<List<PublicCategoryDto>>(UnaRaizConHijas, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        await cut.InvokeAsync(() => state.SetCategorias(CategoryTree.Build(cats), cats));

        cut.WaitForAssertion(() => Assert.NotEmpty(cut.FindAll("[data-testid='store-home-cat-tile']")));
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();
}


