using System.Text.Json;
using Inventa.WebAPI.Controllers;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S69: contrato público de jerarquía (sin DB): serialización + sanitización
// de ImagenUrl (nunca 400 en el catálogo: lo inválido se omite => null).
public sealed class PublicCategoryHierarchyTests
{
    private static readonly JsonSerializerOptions Web = new(JsonSerializerDefaults.Web);

    private static string Nombres(object dto) =>
        string.Join(",", JsonDocument.Parse(JsonSerializer.Serialize(dto, Web)).RootElement.EnumerateObject().Select(p => p.Name));

    [Fact]
    public void Category_SerializaNuevosCampos_CamelCase()
    {
        var n = Nombres(new PublicCategoryDto(1, "C", "Cat", 5, 2, 3, "/img/c.png"));
        Assert.Contains("idPadre", n);
        Assert.Contains("orden", n);
        Assert.Contains("imagenUrl", n);
        Assert.Contains("cantidad", n);
    }

    [Fact]
    public void Category_Defaults_CompatApiSinS69()
    {
        var el = JsonDocument.Parse(JsonSerializer.Serialize(
            new PublicCategoryDto(1, "C", "Cat", 5), Web)).RootElement;
        Assert.Equal(JsonValueKind.Null, el.GetProperty("idPadre").ValueKind);
        Assert.Equal(0, el.GetProperty("orden").GetInt16());
        Assert.Equal(JsonValueKind.Null, el.GetProperty("imagenUrl").ValueKind);
    }

    // S70 (H5): ajustados a la regla única (CategoriaImagenUrlRule). Cambios vs S69:
    // /img/c.png ahora null (solo /uploads/ o /images/); agregados /uploads/,
    // protocol-relative y /uploads/private.
    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("   ", null)]
    [InlineData("https://cdn.tienda.cl/img/c.png", "https://cdn.tienda.cl/img/c.png")]
    [InlineData("http://cdn.tienda.cl/img/c.png", "http://cdn.tienda.cl/img/c.png")]
    [InlineData("/uploads/c.png", "/uploads/c.png")]
    [InlineData("/images/c.png", "/images/c.png")]
    [InlineData("/img/c.png", null)]
    [InlineData("//evil.com/c.png", null)]
    [InlineData("/uploads/private/c.png", null)]
    [InlineData("javascript:alert(1)", null)]
    [InlineData("img/c.png", null)]
    [InlineData("/img/../secreto.png", null)]
    [InlineData("/img\\c.png", null)]
    [InlineData("/img/c x.png", null)]
    public void SanitizeStoreImagenUrl_Casos(string? entrada, string? esperado) =>
        Assert.Equal(esperado, StorefrontFmt.SanitizeStoreImagenUrl(entrada));

    [Fact]
    public void SanitizeStoreImagenUrl_Larga_Null()
    {
        var larga = "/" + new string('a', 500);
        Assert.Null(StorefrontFmt.SanitizeStoreImagenUrl(larga));
    }
}

