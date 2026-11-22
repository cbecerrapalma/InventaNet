using Bunit;
using Inventa.Store.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 shell: StoreHeader (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class ShellHeaderTests : IDisposable
{
    readonly BunitContext _ctx = new();

    public ShellHeaderTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
    }

    IRenderedComponent<StoreHeader> Render(
        string? anuncio = null, bool menuOpen = false, string? query = null) =>
        _ctx.Render<StoreHeader>(p => p
            .Add(x => x.Nombre, "Tienda Demo")
            .Add(x => x.AnuncioTexto, anuncio)
            .Add(x => x.MenuOpen, menuOpen)
            .Add(x => x.Query, query));

    [Fact]
    public void SinAnuncio_NoRenderizaRegion() =>
        Assert.Empty(Render().FindAll("[data-testid='store-announce']"));

    [Fact]
    public void ConAnuncio_RegionConTexto()
    {
        var cut = Render("Despacho gratis");
        var region = cut.Find("[data-testid='store-announce']");
        Assert.Equal("region", region.GetAttribute("role"));
        Assert.Equal("Anuncio de la tienda", region.GetAttribute("aria-label"));
        Assert.Contains("Despacho gratis", region.TextContent);
    }

    [Fact]
    public void Buscador_Rol_LabelAsociado()
    {
        var cut = Render();
        Assert.Equal("search", cut.Find("form[data-testid='store-search-form']").GetAttribute("role"));
        var input = cut.Find("[data-testid='store-search-input']");
        var label = cut.Find("form label");
        Assert.Equal(input.GetAttribute("id"), label.GetAttribute("for"));
        Assert.Equal("search", input.GetAttribute("type"));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void MenuBoton_AriaExpanded_ReflejaEstado(bool open, string esperado) =>
        Assert.Equal(esperado, Render(menuOpen: open)
            .Find("[data-testid='store-menu-button']").GetAttribute("aria-expanded"));

    [Fact]
    public void MenuBoton_AriaControls_Y_Label()
    {
        var btn = Render().Find("[data-testid='store-menu-button']");
        Assert.Equal("store-category-drawer", btn.GetAttribute("aria-controls"));
        Assert.Equal("Abrir categorías", btn.GetAttribute("aria-label"));
    }

    [Fact]
    public void Submit_ConTexto_NavegaQCodificada()
    {
        var cut = Render();
        cut.Find("[data-testid='store-search-input']").Input("zapatos ñ");
        cut.Find("form[data-testid='store-search-form']").Submit();
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("/?q=zapatos%20%C3%B1", new Uri(nav.Uri).PathAndQuery);
    }

    [Fact]
    public void Submit_Vacio_NavegaRaiz()
    {
        var cut = Render(query: "algo");
        cut.Find("[data-testid='store-search-input']").Input("");
        cut.Find("form[data-testid='store-search-form']").Submit();
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        Assert.Equal("/", new Uri(nav.Uri).PathAndQuery);
    }

    [Fact]
    public void Marca_EnlaceInicio_ConNombre()
    {
        var cut = Render();
        var link = cut.Find("[data-testid='store-brand-link']");
        Assert.Equal("/", link.GetAttribute("href"));
        Assert.Contains("Tienda Demo", link.GetAttribute("aria-label"));
    }

    public void Dispose() => _ctx.Dispose();
}

