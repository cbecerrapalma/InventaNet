using Inventa.Store.Models;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas puras: StoreReturnUrl sanitizacion (anti open-redirect).</summary>
public sealed class StoreReturnUrlTests
{
    [Theory]
    [InlineData(null, "/")]
    [InlineData("", "/")]
    [InlineData("   ", "/")]
    [InlineData("/", "/")]
    [InlineData("/productos", "/productos")]
    [InlineData("/productos/123", "/productos/123")]
    [InlineData("/categoria/abc?orden=precio", "/categoria/abc?orden=precio")]
    public void Sanitize_ValidPaths_ReturnsSame(string? input, string expected) =>
        Assert.Equal(expected, StoreReturnUrl.Sanitize(input));

    [Theory]
    [InlineData("//evil.com", "/")]          // doble slash inicio
    [InlineData("/\\", "/")]                 // backslash segundo
    [InlineData("/\\evil", "/")]             // backslash segundo
    [InlineData("/path\\evil", "/")]         // backslash medio
    [InlineData("/path with spaces", "/")]   // espacios
    [InlineData("/path\twith\ttabs", "/")]   // tabs
    [InlineData("/path\nwith\nnewlines", "/")] // newlines
    [InlineData("http://evil.com", "/")]     // esquema http
    [InlineData("https://evil.com", "/")]    // esquema https
    [InlineData("javascript:alert(1)", "/")] // javascript:
    [InlineData("ftp://evil.com", "/")]      // ftp
    [InlineData("mailto:test@test.com", "/")] // mailto
    [InlineData("/path:colon", "/")]         // colon antes que slash (relativo con esquema-like)
    [InlineData("/a:b/c", "/")]              // colon antes slash
    [InlineData("/a:b", "/")]                // colon sin slash posterior
    public void Sanitize_BlockedPatterns_ReturnsRoot(string input, string expected) =>
        Assert.Equal(expected, StoreReturnUrl.Sanitize(input));

    [Fact]
    public void Sanitize_ColonAfterSlash_Allowed()
    {
        // puerto en path no estandar pero valido segun regla: colon > slash posterior
        Assert.Equal("/api/v1:8080/resource", StoreReturnUrl.Sanitize("/api/v1:8080/resource"));
    }

    [Fact]
    public void Sanitize_TooLong_ReturnsRoot()
    {
        var longPath = "/" + new string('a', 2048);
        Assert.Equal("/", StoreReturnUrl.Sanitize(longPath));
    }

    [Fact]
    public void Sanitize_ExactMaxLength_Allowed()
    {
        var maxPath = "/" + new string('a', 2047); // total 2048
        Assert.Equal(maxPath, StoreReturnUrl.Sanitize(maxPath));
    }
}
