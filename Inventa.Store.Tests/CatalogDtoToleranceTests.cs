using System.Text.Json;
using Inventa.Store.Models;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S64: espejos toleran JSON con y sin campos S63 (API viejo => nulls/defaults).</summary>
public sealed class CatalogDtoToleranceTests
{
    static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    const string LegacyCard = """{"idProd":8,"codigo":"ARROZ-1K","nombre":"Arroz 1k","idCategoria":2,"categoriaNombre":"Almacén","idMarca":null,"marcaNombre":null,"unidadNombre":null,"imagenUrl":null,"precioDesde":1500,"tieneOferta":false}""";

    const string FullCard = """{"idProd":7,"codigo":"YERBA-500","nombre":"Yerba 500g","idCategoria":2,"categoriaNombre":"Almacén","idMarca":3,"marcaNombre":"Acme","unidadNombre":"UN","imagenUrl":null,"precioDesde":12990,"tieneOferta":true,"precioFinalDesde":9990,"precioReferencia":12990,"pctDescuento":23,"ofertaHasta":"2026-10-31"}""";

    [Fact]
    public void CardLegacy_NuevosEnNull()
    {
        var p = JsonSerializer.Deserialize<PublicProductDto>(LegacyCard, Web);
        Assert.NotNull(p);
        Assert.Null(p.PrecioFinalDesde);
        Assert.Null(p.PrecioReferencia);
        Assert.Null(p.PctDescuento);
        Assert.Null(p.OfertaHasta);
        Assert.False(p.TieneOferta);
    }

    [Fact]
    public void CardCompleta_LeeNuevos()
    {
        var p = JsonSerializer.Deserialize<PublicProductDto>(FullCard, Web);
        Assert.NotNull(p);
        Assert.Equal(9990m, p.PrecioFinalDesde);
        Assert.Equal(12990m, p.PrecioReferencia);
        Assert.Equal(23, p.PctDescuento);
        Assert.Equal(new DateOnly(2026, 10, 31), p.OfertaHasta);
        Assert.Equal(3, p.IdMarca);
    }

    [Theory]
    [InlineData("""{"idCategoria":2,"codigo":"ALM","nombre":"Almacén"}""", 0)]
    [InlineData("""{"idCategoria":2,"codigo":"ALM","nombre":"Almacén","cantidad":3}""", 3)]
    public void Category_Cantidad_ToleraAusencia(string json, int esperado) =>
        Assert.Equal(esperado, JsonSerializer.Deserialize<PublicCategoryDto>(json, Web)?.Cantidad);

    [Fact]
    public void Brands_Lista()
    {
        var list = JsonSerializer.Deserialize<IReadOnlyList<PublicBrandDto>>(
            """[{"idMarca":3,"codigo":"ACME","nombre":"Acme","cantidad":12}]""", Web);
        var m = Assert.Single(list!);
        Assert.Equal(("Acme", 12), (m.Nombre, m.Cantidad));
    }

    [Fact]
    public void PresentacionLegacy_OfertasNull_PrecioFinalNull()
    {
        var pr = JsonSerializer.Deserialize<PublicPresentacionDto>(
            """{"idPresProd":11,"descrip":"Bolsa 500g","unidad":"UN","precios":[{"idPrecio":5,"valor":9990,"fecVig":"2026-10-01","vigente":true}]}""", Web);
        Assert.NotNull(pr);
        Assert.Null(pr.Ofertas);
        Assert.Null(pr.PrecioFinal);
        Assert.Single(pr.Precios);
    }

    [Fact]
    public void PresentacionCompleta_LeeFinalYOferta()
    {
        var pr = JsonSerializer.Deserialize<PublicPresentacionDto>(
            """{"idPresProd":11,"descrip":"Bolsa 500g","unidad":"UN","precios":[],"ofertas":[{"idOferta":5,"precioOferta":8990,"fechaDesde":"2026-10-01","fechaHasta":null,"precioReferenciaValor":9990,"referenciaEsVigente":true,"pctDescuento":10}],"precioVigente":9990,"precioFinal":8990,"pctDescuento":10}""", Web);
        Assert.NotNull(pr);
        Assert.Equal(8990m, pr.PrecioFinal);
        Assert.Equal(10, pr.PctDescuento);
        Assert.Equal(8990m, Assert.Single(pr.Ofertas!)!.PrecioOferta);
    }
}

