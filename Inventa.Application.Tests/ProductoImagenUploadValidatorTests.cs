using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Xunit;

namespace Inventa.Application.Tests;

// S58.03 P0-A: contrato del validador de uploads (S57.71). Puro, sin DB.
// Garantía migrada: PLAYWRIGHT s57.71-product-image-upload → C# COVERAGE
// (el spec Playwright NO se elimina en este sprint).
public sealed class ProductoImagenUploadValidatorTests
{
    private static readonly byte[] Png =
        [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];
    private static readonly byte[] Jpg = [0xFF, 0xD8, 0xFF, 0x00];
    private static readonly byte[] Webp =
        [(byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0,
         (byte)'W', (byte)'E', (byte)'B', (byte)'P'];

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NombreRequerido(string? nombre)
    {
        var e = ProductoImagenUploadValidator.Validate(nombre, "image/png", Png);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Theory]
    [InlineData("..\\evil.png")]
    [InlineData("../evil.png")]
    [InlineData("sub/dir.png")]
    [InlineData("sub\\dir.png")]
    public void RechazaTraversal(string nombre)
    {
        var e = ProductoImagenUploadValidator.Validate(nombre, "image/png", Png);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Theory]
    [InlineData("foto.exe")]
    [InlineData("foto.gif")]
    [InlineData("foto")]
    [InlineData("foto.PNG.bak")]
    public void RechazaExtensionNoPermitida(string nombre)
    {
        var e = ProductoImagenUploadValidator.Validate(nombre, "image/png", Png);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Fact]
    public void RechazaArchivoVacio()
    {
        Assert.NotNull(ProductoImagenUploadValidator.Validate("a.png", "image/png", []));
        Assert.NotNull(ProductoImagenUploadValidator.Validate("a.png", "image/png", null));
    }

    [Fact]
    public void RechazaExcesoTamano()
    {
        var big = new byte[ProductoImagenUploadValidator.MaxBytes + 1];
        for (var i = 0; i < 8; i++) big[i] = Png[i];
        var e = ProductoImagenUploadValidator.Validate("a.png", "image/png", big);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Fact]
    public void RechazaMimeInconsistente()
    {
        var e = ProductoImagenUploadValidator.Validate("a.png", "image/jpeg", Png);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Fact]
    public void RechazaFirmaInvalida()
    {
        var e = ProductoImagenUploadValidator.Validate(
            "a.png", "image/png", [0x00, 0x01, 0x02, 0x03, 0x04, 0x05, 0x06, 0x07]);
        Assert.NotNull(e);
        Assert.Equal("INVALID_IMAGE", e.Code);
    }

    [Theory]
    [InlineData("foto.png", "image/png")]
    [InlineData("FOTO.PNG", "image/png")]
    [InlineData("foto.jpg", "image/jpeg")]
    [InlineData("foto.jpeg", "image/jpeg")]
    [InlineData("foto.webp", "image/webp")]
    public void AceptaFirmasValidasSegunExtension(string nombre, string mime)
    {
        var ext = Path.GetExtension(nombre).ToLowerInvariant();
        var content = ext == ".png" ? Png : ext == ".webp" ? Webp : Jpg;
        Assert.Null(ProductoImagenUploadValidator.Validate(nombre, mime, content));
    }

    [Fact]
    public void ErrorEs422Validacion()
    {
        var e = ProductoImagenUploadValidator.Validate("a.exe", "image/png", Png);
        Assert.NotNull(e);
        Assert.Equal(ErrorType.Validation, e.Type);
        Assert.Equal(422, e.HttpStatusCode);
    }
}

