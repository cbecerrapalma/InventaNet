using System.Net;
using System.Text;
using System.Text.Json;
using Bunit;
using Inventa.Store.Components;
using Inventa.Store.Models;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S71 UI: ProductCard, HomeSections, CategoryMegaMenu, CategoryDrawer
/// anidado y chips raíz/hijas de Catalog (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class S71StoreUiTests : IAsyncLifetime
{
    readonly BunitContext _ctx = new();

    public S71StoreUiTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        var http = new HttpClient(new StubHandler()) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        _ctx.Services.AddSingleton(new CatalogApiService(http));
        var storefrontApi = new StorefrontApiService(http);
        var storefrontState = new StorefrontState(storefrontApi, cfg);
        _ctx.Services.AddSingleton(storefrontApi);
        _ctx.Services.AddSingleton(storefrontState);
        
        // S83: pre-poblar categorías en StorefrontState para que Catalog las use (simula MainLayout)
        var cats = System.Text.Json.JsonSerializer.Deserialize<List<PublicCategoryDto>>(CategoriesJson, new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        storefrontState.SetCategorias(CategoryTree.Build(cats), cats);
    }

    const string StorefrontJson = """{"nombreLegal":"Tienda Demo","nombreFantasia":null,"descripcionCorta":null,"descripcion":null,"logoUrl":null,"contacto":{},"redes":[],"horarios":[],"branding":null}""";
    const string CategoriesJson = """[{"idCategoria":1,"codigo":"R","nombre":"Raíz","cantidad":5,"idPadre":null,"orden":0,"imagenUrl":null},{"idCategoria":2,"codigo":"H1","nombre":"Hija A","cantidad":2,"idPadre":1,"orden":0,"imagenUrl":null},{"idCategoria":3,"codigo":"H2","nombre":"Hija B","cantidad":3,"idPadre":1,"orden":1,"imagenUrl":null},{"idCategoria":4,"codigo":"S","nombre":"Suelta","cantidad":7,"idPadre":null,"orden":1,"imagenUrl":null}]""";
    const string BrandsJson = """[{"idMarca":3,"codigo":"ACME","nombre":"Acme","cantidad":12}]""";
    const string ProductsJson = """{"items":[{"idProd":7,"codigo":"YERBA-500","nombre":"Yerba 500g","idCategoria":1,"categoriaNombre":"Raíz","idMarca":3,"marcaNombre":"Acme","unidadNombre":"UN","imagenUrl":null,"precioDesde":12990,"tieneOferta":true,"precioFinalDesde":9990,"precioReferencia":12990,"pctDescuento":23,"ofertaHasta":"2026-10-31"}],"page":1,"pageSize":20,"totalCount":1,"totalPages":1}""";

    sealed class StubHandler : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
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

    static PublicProductDto ProdConOferta() => new(7, "YERBA-500", "Yerba 500g", 1, "Raíz", 3, "Acme", "UN", null, 12990, true, 9990, 12990, 23, new DateOnly(2026, 10, 31));
    static PublicProductDto ProdSinPrecio() => new(9, "SINPRECIO", "Sin precio", 1, "Raíz", null, null, null, null, null, false);
    static IReadOnlyList<CategoryNode> Arbol() => CategoryTree.Build([
        new PublicCategoryDto(1, "R", "Raíz", 5),
        new PublicCategoryDto(2, "H1", "Hija A", 2, 1, 0),
        new PublicCategoryDto(3, "H2", "Hija B", 3, 1, 1),
        new PublicCategoryDto(4, "S", "Suelta", 7),
    ]);

    // --- ProductCard ---

    [Fact]
    public void Card_Enlace_Detalle_SinOnclickNiOnerror()
    {
        var cut = _ctx.Render<ProductCard>(p => p.Add(x => x.Producto, ProdConOferta()));
        var a = cut.Find("[data-testid='product-card']");
        Assert.Equal("A", a.TagName);
        Assert.Equal("/productos/7", a.GetAttribute("href"));
        Assert.DoesNotContain("onclick", cut.Markup, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("onerror", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Card_Badge_PrecioTachado_Y_Consultar()
    {
        var cut = _ctx.Render<ProductCard>(p => p.Add(x => x.Producto, ProdConOferta()));
        Assert.Contains("-23 %", cut.Find("[data-testid='store-card-discount']").TextContent); // S93.3 (C3): Porcentaje es-CL con espacio.
        Assert.Contains("$12.990", cut.Find("[data-testid='store-card-reference']").TextContent);
        Assert.Contains("Precio anterior", cut.Find("[data-testid='store-card-reference']").TextContent);
        var sinPrecio = _ctx.Render<ProductCard>(p => p.Add(x => x.Producto, ProdSinPrecio()));
        Assert.Contains("Precio a consultar", sinPrecio.Find("[data-testid='store-card-noprice']").TextContent);
    }

    [Fact]
    public void Card_SinImagen_Placeholder_ConTestid()
    {
        var cut = _ctx.Render<ProductCard>(p => p.Add(x => x.Producto, ProdSinPrecio()));
        Assert.NotNull(cut.Find("[data-testid='store-image-placeholder']"));
    }

    [Fact]
    public void Card_OfertaSinPct_MuestraOferta()
    {
        var prod = ProdConOferta() with { PctDescuento = null };
        var cut = _ctx.Render<ProductCard>(pp => pp.Add(x => x.Producto, prod));
        Assert.Contains("Oferta", cut.Find("[data-testid='store-card-offer']").TextContent);
    }

    // --- HomeSections ---

    [Fact]
    public void Home_SinDatos_OmiteSecciones()
    {
        var cut = _ctx.Render<HomeSections>();
        Assert.Empty(cut.FindAll("[data-testid='store-home-categorias']"));
        Assert.Empty(cut.FindAll("[data-testid='store-home-ofertas']"));
        Assert.Empty(cut.FindAll("[data-testid='store-home-marcas']"));
    }

    [Fact]
    public void Home_Tiles_EnlazanCat_Max12_Y_VerTodasOfertas()
    {
        var raices = Enumerable.Range(1, 14).Select(i => new CategoryNode(i, $"C{i}", null, [])).ToList();
        var cut = _ctx.Render<HomeSections>(p => p
            .Add(x => x.Raices, raices)
            .Add(x => x.Ofertas, [ProdConOferta()])
            .Add(x => x.Marcas, [new PublicBrandDto(3, "ACME", "Acme", 12)]));
        Assert.Equal(12, cut.FindAll("[data-testid='store-home-cat-tile']").Count);
        Assert.Equal("/?cat=1", cut.FindAll("[data-testid='store-home-cat-tile']")[0].GetAttribute("href"));
        Assert.Equal("/?ofertas=true&orden=descuento_desc", cut.Find("[data-testid='store-home-offers-all']").GetAttribute("href"));
    }

    // S101: con una sola raíz el tile útil es cada hija, no la raíz.
    [Fact]
    public void Home_RaizUnica_MuestraSusHijas_Como_Tiles()
    {
        List<CategoryNode> hijas =
        [
            new(21, "Poleras", null, []),
            new(22, "Pantalones", null, []),
            new(23, "Chaquetas", null, [])
        ];
        var raiz = new CategoryNode(20, "Vestuario", null, hijas);

        var cut = _ctx.Render<HomeSections>(p => p.Add(x => x.Raices, [raiz]));

        var tiles = cut.FindAll("[data-testid='store-home-cat-tile']");
        Assert.Equal(3, tiles.Count);
        Assert.Equal("/?cat=21", tiles[0].GetAttribute("href"));
        Assert.Equal("/?cat=22", tiles[1].GetAttribute("href"));
        Assert.Equal("/?cat=23", tiles[2].GetAttribute("href"));
        Assert.Empty(cut.FindAll("[data-testid='store-home-cats-all']"));
    }

    [Fact]
    public void Home_Marcas_Max16()
    {
        var marcas = Enumerable.Range(1, 20).Select(i => new PublicBrandDto(i, $"M{i}", $"Marca {i}", 1)).ToList();
        var cut = _ctx.Render<HomeSections>(p => p.Add(x => x.Marcas, marcas));
        Assert.Equal(16, cut.FindAll("[data-testid='store-home-brand-link']").Count);
        Assert.Equal("/?marca=1", cut.FindAll("[data-testid='store-home-brand-link']")[0].GetAttribute("href"));
    }

    // --- CategoryMegaMenu ---

    [Fact]
    public void Mega_RaizSinHijas_EsEnlace()
    {
        var cut = _ctx.Render<CategoryMegaMenu>(p => p.Add(x => x.Raices, Arbol()));
        var links = cut.FindAll("[data-testid='store-mega-link']");
        Assert.Single(links);
        Assert.Equal("/?cat=4", links[0].GetAttribute("href"));
    }

    [Fact]
    public void Mega_RaizConHijas_Boton_Abre_VerTodo_Y_EscapeCierra()
    {
        var cut = _ctx.Render<CategoryMegaMenu>(p => p.Add(x => x.Raices, Arbol()));
        var btn = cut.FindAll("[data-testid='store-mega-button']")[0];
        Assert.Equal("false", btn.GetAttribute("aria-expanded"));
        btn.Click();
        btn = cut.FindAll("[data-testid='store-mega-button']")[0];
        Assert.Equal("true", btn.GetAttribute("aria-expanded"));
        Assert.Contains("Ver todo Raíz", cut.Find("[data-testid='store-mega-all']").TextContent);
        Assert.Equal(2, cut.FindAll("[data-testid='store-mega-child']").Count);
        // un solo panel abierto: abrir la otra raíz no aplica (solo una con hijas); Escape cierra
        cut.Find("[data-testid='store-mega-nav']").KeyDown("Escape");
        Assert.Empty(cut.FindAll("[data-testid='store-mega-panel']"));
    }

    // --- CategoryDrawer ---

    [Fact]
    public void Drawer_Grupo_ConHijas_Y_VerTodo()
    {
        var cut = _ctx.Render<Inventa.Store.Components.CategoryDrawer>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Categorias, Arbol()));
        Assert.NotEmpty(cut.FindAll("[data-testid='store-cat-group']"));
        Assert.Equal(2, cut.FindAll("[data-testid='store-cat-child-link']").Count);
        Assert.Equal("/?cat=2", cut.FindAll("[data-testid='store-cat-child-link']")[0].GetAttribute("href"));
        // compat plana: raíces sin hijas siguen con store-cat-link
        Assert.NotEmpty(cut.FindAll("[data-testid='store-cat-link']"));
    }

    // --- Catalog ---

    IRenderedComponent<Catalog> RenderCatalog(string? url = null)
    {
        if (url is not null)
            _ctx.Services.GetRequiredService<NavigationManager>().NavigateTo(url);
        var cut = _ctx.Render<Catalog>();
        cut.WaitForAssertion(() => cut.Find("[data-testid='store-product-grid']"));
        return cut;
    }

    [Fact]
    public void Catalog_Home_Visible_SinFiltros_Y_Oculta_ConQuery()
    {
        var home = RenderCatalog();
        home.WaitForAssertion(() => home.Find("[data-testid='store-home-categorias']"));
        Assert.NotEmpty(home.FindAll("[data-testid='store-home-ofertas']"));
        var filtrado = RenderCatalog("/?q=yerba");
        filtrado.WaitForAssertion(() => filtrado.Find("[data-testid='store-product-grid']"));
        Assert.Empty(filtrado.FindAll("[data-testid='store-home-categorias']"));
        var porCat = RenderCatalog("/?cat=1");
        Assert.Empty(porCat.FindAll("[data-testid='store-home-categorias']"));
    }

    [Fact]
    public void Catalog_Chips_SoloRaices_Y_SegundaFila_Hijas()
    {
        // S83: chips no se muestran en home (EsHome=true); navegar a URL sin home para ver chips
        var cut = RenderCatalog("/?q=test");
        var chips = cut.FindAll("[data-testid='store-cat-chip']");
        Assert.Equal(2, chips.Count); // Raíz + Suelta, sin hijas mezcladas
        Assert.Empty(cut.FindAll("[data-testid='store-subcategories']"));
        var padre = RenderCatalog("/?cat=1");
        padre.WaitForAssertion(() => padre.Find("[data-testid='store-subcategories']"));
        Assert.Equal(3, padre.FindAll("[data-testid='store-subcat-chip']").Count); // Todo + 2 hijas
        Assert.Contains("Raíz", padre.Find("[data-testid='store-breadcrumb']").TextContent);
    }

    public ValueTask InitializeAsync() => ValueTask.CompletedTask;
    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();
}


