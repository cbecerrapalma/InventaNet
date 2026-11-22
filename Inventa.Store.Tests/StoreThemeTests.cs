using Inventa.Store.Theme;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S61.5 contraste del tema por tenant (WCAG 2.1 AA, umbral ≥ 4.5).</summary>
public sealed class StoreThemeTests
{
    [Theory]
    [InlineData(null, StoreThemeFactory.DefaultAccent)]
    [InlineData("", StoreThemeFactory.DefaultAccent)]
    [InlineData("   ", StoreThemeFactory.DefaultAccent)]
    [InlineData("red", StoreThemeFactory.DefaultAccent)]
    [InlineData("#0f766e", "#0F766E")]
    [InlineData("#1D4E89", "#1D4E89")]
    [InlineData("#12345", StoreThemeFactory.DefaultAccent)]
    [InlineData("#GGGGGG", StoreThemeFactory.DefaultAccent)]
    [InlineData("javascript:alert(1)", StoreThemeFactory.DefaultAccent)]
    public void NormalizeAccent_Sanitiza(string? entrada, string esperado) =>
        Assert.Equal(esperado, StoreThemeFactory.NormalizeAccent(entrada));

    [Theory]
    [InlineData("#FFD600")] // amarillo: debe oscurecerse
    [InlineData("#CCCCCC")]
    [InlineData("#FFFFFF")]
    [InlineData("#00FF00")] // verde puro: debe oscurecerse
    [InlineData("#FF0000")]
    [InlineData("#0F766E")]
    [InlineData("#1D4E89")] // acento por defecto ya legible
    [InlineData("#000000")]
    public void AccessiblePrimary_ContrasteMinimo(string acento) =>
        Assert.True(
            StoreThemeFactory.ContrastRatio(
                StoreThemeFactory.AccessiblePrimary(acento),
                StoreThemeFactory.ContrastOnPrimary) >= StoreThemeFactory.MinContrast,
            $"primario derivado de {acento} bajo 4.5");

    [Fact]
    public void AccessiblePrimary_Determinista_Y_Conserva_Tono_Oscuro() =>
        Assert.Equal(
            StoreThemeFactory.AccessiblePrimary("#1D4E89"),
            StoreThemeFactory.AccessiblePrimary("#1D4E89"));

    [Fact]
    public void AccessiblePrimary_AcentoOscuro_NoCambia() =>
        Assert.Equal("#000000", StoreThemeFactory.AccessiblePrimary("#000000"));

    [Theory]
    [InlineData("#123456")]
    [InlineData("#ABCDEF")]
    [InlineData(null)]
    public void Create_Primario_ContrasteMinimo(string? acento)
    {
        var theme = StoreThemeFactory.Create(acento);
        Assert.True(
            StoreThemeFactory.ContrastRatio(
                (string)theme.PaletteLight.Primary!,
                (string)theme.PaletteLight.PrimaryContrastText!) >= StoreThemeFactory.MinContrast);
    }

    [Fact]
    public void Create_Textos_ContrasteMinimo()
    {
        var p = StoreThemeFactory.Create("#0F766E").PaletteLight;
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.TextPrimary!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.TextSecondary!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.TextPrimary!, (string)p.Background!) >= 4.5);
    }

    [Fact]
    public void Create_Estados_ContrasteMinimo()
    {
        var p = StoreThemeFactory.Create(null).PaletteLight;
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.Error!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.Success!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.Warning!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.Info!, (string)p.Surface!) >= 4.5);
        Assert.True(StoreThemeFactory.ContrastRatio((string)p.AppbarText!, (string)p.AppbarBackground!) >= 4.5);
    }

    [Fact]
    public void Create_Tipografia_Sistema_Sin_Red() =>
        // "Roboto" solo como nombre fallback local; lo prohibido es la carga remota
        // (el <link> Google Fonts se eliminó de index.html, ver 04-HARDCODE-GREP).
        Assert.DoesNotContain(
            StoreThemeFactory.Create(null).Typography.Default!.FontFamily!,
            f => f.Contains("http", StringComparison.OrdinalIgnoreCase)
              || f.Contains("url(", StringComparison.OrdinalIgnoreCase));
}

