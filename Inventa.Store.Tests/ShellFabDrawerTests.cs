using Bunit;
using Inventa.Store.Components;
using Inventa.Store.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 shell: WhatsAppFab + CategoryDrawer (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class ShellFabDrawerTests : IDisposable
{
    readonly BunitContext _ctx = new();

    public ShellFabDrawerTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
    }

    [Fact]
    public void Fab_Null_SinDom() =>
        Assert.Empty(_ctx.Render<WhatsAppFab>().Markup.Trim());

    [Fact]
    public void Fab_ConUrl_EnlaceExterno()
    {
        var cut = _ctx.Render<WhatsAppFab>(p => p.Add(x => x.Url, "https://wa.me/56912345678"));
        var a = cut.Find("[data-testid='store-whatsapp-fab']");
        Assert.Equal("https://wa.me/56912345678", a.GetAttribute("href"));
        Assert.Equal("_blank", a.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", a.GetAttribute("rel"));
        Assert.Contains("WhatsApp", a.GetAttribute("aria-label"));
        Assert.Contains("WhatsApp", a.TextContent);
    }

    IRenderedComponent<CategoryDrawer> RenderDrawer(
        bool cargando = false, bool error = false,
        IReadOnlyList<CategoryNode>? cats = null) =>
        _ctx.Render<CategoryDrawer>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Cargando, cargando)
            .Add(x => x.Error, error)
            .Add(x => x.Categorias, cats ?? []));

    [Fact]
    public void Drawer_Cargando_Esqueleto() =>
        Assert.NotEmpty(RenderDrawer(cargando: true).FindAll("[data-testid='store-cats-loading']"));

    [Fact]
    public void Drawer_Error_Texto_Y_Reintentar()
    {
        var reintentado = false;
        var cut = _ctx.Render<CategoryDrawer>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Error, true)
            .Add(x => x.OnRetry, EventCallback.Factory.Create(this, () => reintentado = true)));
        Assert.Contains("No pudimos cargar las categorías.",
            cut.Find("[data-testid='store-cats-error']").TextContent);
        cut.Find("[data-testid='store-cats-retry']").Click();
        Assert.True(reintentado);
    }

    [Fact]
    public void Drawer_Vacio_Texto() =>
        Assert.Contains("Aún no hay categorías.",
            RenderDrawer().Find("[data-testid='store-cats-empty']").TextContent);

    [Fact]
    public void Drawer_Lista_Links_Y_Cantidades()
    {
        var cut = RenderDrawer(cats: [
            new(5, "Abarrotes", 12, []),
            new(9, "Lácteos", null, [])]);
        var links = cut.FindAll("[data-testid='store-cat-link']");
        Assert.Equal(2, links.Count);
        Assert.Equal("/?cat=5", links[0].GetAttribute("href"));
        Assert.Contains("Abarrotes (12)", links[0].TextContent);
        Assert.Contains("Lácteos", links[1].TextContent);
        Assert.DoesNotContain("(", links[1].TextContent.Replace("Lácteos", ""));
        Assert.Equal("/", cut.Find("[data-testid='store-cat-all-link']").GetAttribute("href"));
    }

    [Fact]
    public void Drawer_Nav_AriaLabel() =>
        Assert.Equal("Categorías", RenderDrawer(cats: [new(1, "Una", null, [])])
            .Find("nav[aria-label='Categorías']").GetAttribute("aria-label"));

    public void Dispose() => _ctx.DisposeAsync().AsTask().GetAwaiter().GetResult();
}

