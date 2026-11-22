using System.Text.Json;
using Inventa.WebAPI.Controllers;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S63: retrocompatibilidad de serialización (sin DB): precioDesde/tieneOferta
// intactos en camelCase + campos nuevos presentes.
public sealed class PublicCatalogSerializationTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private static string Nombres(object dto) =>
        string.Join(",", JsonDocument.Parse(JsonSerializer.Serialize(dto, Web)).RootElement.EnumerateObject().Select(p => p.Name));

    [Fact]
    public void Card_ConservaPrecioDesdeYTieneOferta_MasNuevos()
    {
        var dto = new PublicProductDto(1, "C1", "A", 2, "Cat", 3, "Marca", "UN",
            null, 100m, true, 80m, 100m, 20, new DateOnly(2026, 10, 31));
        var n = Nombres(dto);
        Assert.Contains("precioDesde", n);
        Assert.Contains("tieneOferta", n);
        Assert.Contains("precioFinalDesde", n);
        Assert.Contains("precioReferencia", n);
        Assert.Contains("pctDescuento", n);
        Assert.Contains("ofertaHasta", n);
    }

    [Fact]
    public void Card_Defaults_NuevosNulos_NoRompenStoreVigente()
    {
        var el = JsonDocument.Parse(JsonSerializer.Serialize(
            new PublicProductDto(1, "C1", "A", 2, "Cat", null, null, null, null, null, false), Web)).RootElement;
        Assert.Equal(JsonValueKind.Null, el.GetProperty("precioFinalDesde").ValueKind);
        Assert.False(el.GetProperty("tieneOferta").GetBoolean());
    }

    [Fact]
    public void Category_Brand_ConCantidad()
    {
        Assert.Contains("cantidad", Nombres(new PublicCategoryDto(1, "C", "Cat", 5)));
        Assert.Contains("cantidad", Nombres(new PublicBrandDto(1, "M", "Marca", 5)));
    }
}

