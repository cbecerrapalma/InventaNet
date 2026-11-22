using Bunit;
using Inventa.Store.Components;
using Inventa.Store.Models;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>
/// S94 K0 (deuda S93.4): pie de la tienda de lunes a domingo (DayOfWeek, 0 = Domingo)
/// y conteos de categorías con separador de miles es-CL.
/// </summary>
public sealed class S94_StoreHorariosYConteosTests : IAsyncDisposable
{
    private readonly BunitContext _ctx = new();

    public S94_StoreHorariosYConteosTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
    }

    public async ValueTask DisposeAsync() => await _ctx.DisposeAsync();

    [Fact]
    public void Pie_OrdenaDeLunesADomingo_YDomingoCerradoAlFinal()
    {
        // Llegan en orden DayOfWeek (0..6), como los devuelve la API.
        var horarios = Enumerable.Range(0, 7)
            .Select(d => new PublicHourDto((byte)d, d is 0 ? null : "10:00", d is 0 ? null : d == 6 ? "14:00" : "19:00", null, null, d == 0))
            .ToList();
        var cut = _ctx.Render<StoreFooter>(p => p
            .Add(x => x.Nombre, "Tienda Demo")
            .Add(x => x.Horarios, horarios)
            .Add(x => x.Redes, [])
            .Add(x => x.Anio, 2026));

        var items = cut.FindAll("[data-testid='store-footer-hours'] li").Select(li => li.TextContent.Trim()).ToList();
        Assert.Equal(7, items.Count);
        Assert.StartsWith("Lunes:", items[0]);
        Assert.StartsWith("Sábado: 10:00–14:00", items[5]);
        Assert.Equal("Domingo: Cerrado", items[6]);
    }

    [Fact]
    public void MenuDeCategorias_MuestraConteosConMiles()
    {
        var cats = new List<CategoryNode>
        {
            new(1, "Vestuario", 2754, []),
            new(2, "Calzado", 1234567, [new(3, "Zapatillas", 1500, [])]),
        };
        var cut = _ctx.Render<CategoryDrawer>(p => p
            .Add(x => x.Open, true)
            .Add(x => x.Categorias, cats));

        var texto = cut.Find("[data-testid='store-cats-nav']").TextContent;
        Assert.Contains("Vestuario (2.754)", texto);
        Assert.Contains("Calzado (1.234.567)", texto);
        Assert.Contains("Zapatillas (1.500)", texto);
        Assert.DoesNotContain("(2754)", texto);
    }
}
