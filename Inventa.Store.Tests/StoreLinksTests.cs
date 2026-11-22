using Inventa.Store.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 reglas de WhatsApp y mapa (S62.1-SHELL §4).</summary>
public sealed class StoreLinksTests
{
    public static TheoryData<string?, string?, string?> WhatsAppCasos => new()
    {
        { "+56 9 1234 5678", null, "https://wa.me/56912345678" },
        { "912345678", null, "https://wa.me/56912345678" },
        { "56912345678", null, "https://wa.me/56912345678" },
        { "0056912345678", null, "https://wa.me/56912345678" },
        { "(+56) 9-1234-5678", null, "https://wa.me/56912345678" },
        { "223456789", null, null }, // fijo de 9: no es móvil
        { "123", null, null },
        { "abc", null, null },
        { null, null, null },
        { "", null, null },
        { "   ", null, null },
        { "1234567890123456", null, null }, // 16 dígitos > E.164
        { "14155552671", null, "https://wa.me/14155552671" }, // internacional 11
        { "349112345678", null, "https://wa.me/349112345678" }, // internacional 12
        { "912345678", "Hola, ¿tienen stock?", "https://wa.me/56912345678?text=Hola%2C%20%C2%BFtienen%20stock%3F" },
        { "912345678", "  ", "https://wa.me/56912345678" },
    };

    [Theory]
    [MemberData(nameof(WhatsAppCasos))]
    public void WhatsAppUrl_Casos(string? numero, string? mensaje, string? esperado) =>
        Assert.Equal(esperado, StoreLinks.WhatsAppUrl(numero, mensaje));

    public static TheoryData<string?, string?, string?, string?> MapsCasos => new()
    {
        { "Av. Principal 123", "Ñuñoa", "Santiago", "https://www.google.com/maps/search/?api=1&query=Av.%20Principal%20123%2C%20%C3%91u%C3%B1oa%2C%20Santiago" },
        { "Av. Principal 123", null, null, "https://www.google.com/maps/search/?api=1&query=Av.%20Principal%20123" },
        { "", "Ñuñoa", "Santiago", null },
        { "  ", null, null, null },
        { null, null, null, null },
        { "Calle A&B #12", null, null, "https://www.google.com/maps/search/?api=1&query=Calle%20A%26B%20%2312" },
    };

    [Theory]
    [MemberData(nameof(MapsCasos))]
    public void MapsUrl_Casos(string? direccion, string? comuna, string? ciudad, string? esperado) =>
        Assert.Equal(esperado, StoreLinks.MapsUrl(direccion, comuna, ciudad));
}

