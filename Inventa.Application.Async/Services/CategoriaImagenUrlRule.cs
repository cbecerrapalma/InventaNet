namespace Inventa.Application.Async.Services;

// S70 (H5): UNA sola regla de URL de imagen de categoría. La usan
// CatalogoServiceAsync (EsValida, backoffice => 400) y PublicCatalogController
// vía StorefrontFmt.SanitizeStoreImagenUrl (Sanitizar, público => null).
// Válida: longitud 1..500, sin espacios, sin '\', sin '..', y (a) absoluta
// http(s) con host no vacío o (b) relativa bajo /uploads/ o /images/,
// nunca '//...' (protocol-relative) ni /uploads/private/... (case-insensitive).
public static class CategoriaImagenUrlRule
{
    public static bool EsValida(string url)
    {
        if (string.IsNullOrEmpty(url) || url.Length > 500) return false;
        if (url.Any(char.IsWhiteSpace) || url.Contains('\\') || url.Contains("..")) return false;
        if (url.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
            return Uri.TryCreate(url, UriKind.Absolute, out var abs)
                && !string.IsNullOrEmpty(abs.Host);
        if (url.StartsWith("//", StringComparison.Ordinal)) return false;
        if (url.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
            || url.StartsWith("/images/", StringComparison.OrdinalIgnoreCase))
            return !url.StartsWith("/uploads/private", StringComparison.OrdinalIgnoreCase);
        return false;
    }

    public static string? Sanitizar(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        var u = url.Trim();
        return EsValida(u) ? u : null;
    }
}
