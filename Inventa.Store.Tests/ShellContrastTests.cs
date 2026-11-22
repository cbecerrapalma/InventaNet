using Inventa.Store.Theme;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 contraste del shell (tokens nuevos + anuncio sobre primario).</summary>
public sealed class ShellContrastTests
{
    [Fact]
    public void Whatsapp_TextoBlanco_AA() =>
        Assert.True(StoreThemeFactory.ContrastRatio("#075E54", "#FFFFFF") >= 4.5);

    [Fact]
    public void Whatsapp_ContraSurface_NoTextual() =>
        Assert.True(StoreThemeFactory.ContrastRatio("#075E54", "#FFFFFF") >= 3.0);

    [Theory]
    [InlineData(null)]
    [InlineData("#0F766E")]
    [InlineData("#FFD600")]
    public void Anuncio_PrimarioVsTexto_AA(string? acento)
    {
        var theme = StoreThemeFactory.Create(acento);
        Assert.True(StoreThemeFactory.ContrastRatio(
            (string)theme.PaletteLight.Primary!,
            (string)theme.PaletteLight.PrimaryContrastText!) >= 4.5);
    }
}

