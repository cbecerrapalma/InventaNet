using Inventa.Application.Async.Services;
using Xunit;

namespace Inventa.Application.Tests;

// S70 (H5): regla única de URL de imagen de categoría.
public sealed class CategoriaImagenUrlRuleTests
{
    [Theory]
    [InlineData("https://cdn.tienda.cl/img/c.png")]
    [InlineData("http://cdn.tienda.cl/img/c.png")]
    [InlineData("/uploads/a.png")]
    [InlineData("/images/a.png")]
    [InlineData("/IMAGES/A.PNG")]
    public void EsValida_Aceptadas(string url) => Assert.True(CategoriaImagenUrlRule.EsValida(url));

    [Theory]
    [InlineData("//evil.com/a.png")]
    [InlineData("/uploads/private/a.png")]
    [InlineData("/UPLOADS/PRIVATE/a.png")]
    [InlineData("/otros/a.png")]
    [InlineData("/img/a.png")]
    [InlineData("ftp://x/a.png")]
    [InlineData("javascript:alert(1)")]
    [InlineData("/uploads/../x.png")]
    [InlineData("/uploads/a b.png")]
    [InlineData("/uploads\\a.png")]
    [InlineData("")]
    public void EsValida_Rechazadas(string url) => Assert.False(CategoriaImagenUrlRule.EsValida(url));

    [Fact]
    public void EsValida_501Chars_Rechaza() =>
        Assert.False(CategoriaImagenUrlRule.EsValida("/uploads/" + new string('a', 492)));

    [Fact]
    public void EsValida_500Chars_Acepta() =>
        Assert.True(CategoriaImagenUrlRule.EsValida("/uploads/" + new string('a', 491)));

    [Theory]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    [InlineData("/uploads/a.png", "/uploads/a.png")]
    [InlineData("  /uploads/a.png  ", "/uploads/a.png")]
    [InlineData("//evil.com/a.png", null)]
    public void Sanitizar_Casos(string? entrada, string? esperado) =>
        Assert.Equal(esperado, CategoriaImagenUrlRule.Sanitizar(entrada));
}

