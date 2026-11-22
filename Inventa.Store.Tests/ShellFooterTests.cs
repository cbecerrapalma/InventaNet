using Bunit;
using Inventa.Store.Components;
using Inventa.Store.Models;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 shell: StoreFooter (bUnit, JSRuntimeMode.Loose).</summary>
public sealed class ShellFooterTests : IDisposable
{
    readonly BunitContext _ctx = new();

    public ShellFooterTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
    }

    IRenderedComponent<StoreFooter> Render(
        PublicContactDto? contacto = null,
        IReadOnlyList<PublicSocialDto>? redes = null,
        IReadOnlyList<PublicHourDto>? horarios = null,
        string? backoffice = null,
        string? descripcion = null,
        bool mostrarAdmin = false) =>
        _ctx.Render<StoreFooter>(p => p
            .Add(x => x.Nombre, "Tienda Demo")
            .Add(x => x.Contacto, contacto)
            .Add(x => x.Redes, redes ?? [])
            .Add(x => x.Horarios, horarios ?? [])
            .Add(x => x.BackofficeUrl, backoffice)
            .Add(x => x.Descripcion, descripcion)
            .Add(x => x.Anio, 2026)
            .Add(x => x.MostrarAccesoAdministracion, mostrarAdmin));

    [Fact]
    public void Vacios_NoRenderizaBloquesCondicionales()
    {
        var cut = Render();
        // S83: bloques condicionales solo se renderizan si hay datos; vacíos = no existen
        Assert.Empty(cut.FindAll("[data-testid='store-footer-contact']"));
        Assert.Empty(cut.FindAll("[data-testid='store-footer-hours']"));
        Assert.Empty(cut.FindAll("[data-testid='store-footer-social']"));
        Assert.Empty(cut.FindAll("[data-testid='store-footer-about']"));
        // Créditos siempre presentes
        Assert.Contains("© 2026 Tienda Demo", cut.Find("[data-testid='store-footer-credits']").TextContent);
    }

    [Fact]
    public void Contacto_Enlaces_Tel_Mail_Mapa()
    {
        var cut = Render(contacto: new PublicContactDto("22 123 4567", null, "a@b.cl", "Av. Uno 123", "Ñuñoa", "Santiago", null, null));
        Assert.Equal("tel:221234567",
            cut.Find("[data-testid='store-footer-contact'] a[href^='tel:']").GetAttribute("href"));
        Assert.Equal("mailto:a@b.cl",
            cut.Find("[data-testid='store-footer-contact'] a[href^='mailto:']").GetAttribute("href"));
        var mapa = cut.Find("[data-testid='store-footer-map']");
        Assert.StartsWith("https://www.google.com/maps/search/", mapa.GetAttribute("href"));
        Assert.Equal("_blank", mapa.GetAttribute("target"));
        Assert.Equal("noopener noreferrer", mapa.GetAttribute("rel"));
    }

    [Fact]
    public void SinBackoffice_NoRenderizaEnlace() =>
        Assert.Empty(Render().FindAll("[data-testid='store-backoffice-link']"));

    [Fact]
    public void ConBackoffice_EnlaceExterno() =>
        Assert.Equal("https://backoffice.local/login",
            Render(backoffice: "https://backoffice.local/login", mostrarAdmin: true)
                .Find("[data-testid='store-backoffice-link']").GetAttribute("href"));

    [Fact]
    public void Redes_Nav_AriaLabels()
    {
        var cut = Render(redes: [new("instagram", "https://instagram.com/t"), new("web", "https://t.cl")]);
        var nav = cut.Find("[data-testid='store-social-nav']");
        Assert.Equal("Redes sociales", nav.GetAttribute("aria-label"));
        var links = cut.FindAll("[data-testid='store-social-link']");
        Assert.Equal(2, links.Count);
        Assert.Contains("Instagram", links[0].GetAttribute("aria-label"));
        Assert.All(links, l => Assert.Equal("noopener noreferrer", l.GetAttribute("rel")));
    }

    [Fact]
    public void Horarios_Cerrado_Y_Rangos()
    {
        var cut = Render(horarios: [
            new(0, null, null, null, null, true),
            new(1, "09:00", "18:00", "19:00", "21:00", false)]);
        var texto = cut.Find("[data-testid='store-footer-hours']").TextContent;
        Assert.Contains("Domingo: Cerrado", texto);
        Assert.Contains("Lunes: 09:00–18:00 / 19:00–21:00", texto);
    }

    [Fact]
    public void Creditos_Anio_Y_Nombre() =>
        Assert.Contains("© 2026 Tienda Demo",
            Render().Find("[data-testid='store-footer-credits']").TextContent);

    // S83: sin descripción NO se renderiza el bloque "Nosotros".
    [Fact]
    public void About_Vacio_NoRenderizaBloque() =>
        Assert.Empty(Render().FindAll("[data-testid='store-footer-about']"));

    // S65.1 fix 4: con descripción se muestra el texto del negocio.
    [Fact]
    public void About_ConDescripcion_MuestraTexto() =>
        Assert.Contains("Somos la tienda demo.",
            Render(descripcion: "Somos la tienda demo.").Find("[data-testid='store-about-text']").TextContent);

    public void Dispose() => _ctx.Dispose();
}

