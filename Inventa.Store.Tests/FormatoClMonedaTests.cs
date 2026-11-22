using System.Globalization;
using CBP.Blazor.Formatting;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S93.2 formato de moneda es-CL vía CBP FormatoCl (miles '.', decimal ','),
/// sin depender de la cultura del hilo. Reemplaza a StoreMoney (S61.x, eliminado).</summary>
public sealed class FormatoClMonedaTests
{
    public static TheoryData<decimal?, string?, string> Casos => new()
    {
        { 12990, "CLP", "$12.990" },
        { 0, "CLP", "$0" },
        { 1234567, "CLP", "$1.234.567" },
        { -500, "CLP", "-$500" },
        { 12990.5m, "CLP", "$12.991" },
        { null, "CLP", FormatoCl.Vacio },
        { 999, null, "$999" },
        { 999, "", "$999" },
        { 999, "xx", "$999" },
        { 1234.5m, "USD", "US$ 1.234,50" },
        { 1234.5m, "EUR", "EUR 1.234,50" },
        { 1234.567m, "USD", "US$ 1.234,57" },
    };

    [Theory]
    [MemberData(nameof(Casos))]
    public void Moneda_Casos(decimal? valor, string? moneda, string esperado) =>
        Assert.Equal(esperado, FormatoCl.Moneda(valor, moneda));

    [Theory]
    [InlineData("en-US")]
    [InlineData("es-CL")]
    [InlineData("de-DE")]
    public void Moneda_NoDependeDeCulturaActual(string cultura)
    {
        var previa = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(cultura);
            Assert.Equal("$12.990", FormatoCl.Moneda(12990, "CLP"));
            Assert.Equal("US$ 1.234,50", FormatoCl.Moneda(1234.5m, "USD"));
        }
        finally
        {
            CultureInfo.CurrentCulture = previa;
        }
    }
}
