using System.Text.RegularExpressions;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S60.1: sanitización del branding público en UN solo lugar.
/// Estos valores terminan en CSS/HTML del cliente: todo lo inválido se
/// degrada a null/default seguro. Solo lo usa GetPublishedProfileAsync;
/// el backoffice (GetProfileAsync/Upsert) trabaja con valores crudos.
/// </summary>
public static partial class StorefrontBranding
{
    public static readonly IReadOnlySet<string> PerfilesPermitidos =
        new HashSet<string>(StringComparer.Ordinal) { "General", "Apparel", "Grocery", "Pharmacy", "Hardware" };

    public const string PerfilDefault = "General";
    public const string MonedaDefault = "CLP";

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex ColorRegex();

    [GeneratedRegex("^[A-Z]{3}$")]
    private static partial Regex MonedaRegex();

    public static string? SanitizeColor(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        v = v.Trim();
        return ColorRegex().IsMatch(v) ? v.ToUpperInvariant() : null;
    }

    public static string? SanitizeFavicon(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        v = v.Trim();
        if (v.Length > 500) return null;
        // S60.1: se rechaza `//host/…` (protocol-relative → hereda el esquema).
        if (v.StartsWith("//")) return null;
        return v.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
            || v.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            || v.StartsWith('/')
            ? v : null;
    }

    public static string? SanitizeAnuncio(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        v = v.Trim();
        return v[..Math.Min(v.Length, 200)];
    }

    public static string SanitizePerfil(string? v) =>
        v != null && PerfilesPermitidos.Contains(v) ? v : PerfilDefault;

    public static string SanitizeMoneda(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return MonedaDefault;
        v = v.Trim().ToUpperInvariant();
        return MonedaRegex().IsMatch(v) ? v : MonedaDefault;
    }

    /// <summary>Sanitiza in-place las 4 columnas de branding (entidad detached).</summary>
    public static void SanitizePublished(TenantStorefront p)
    {
        p.ColorAcento = SanitizeColor(p.ColorAcento);
        p.FaviconUrl = SanitizeFavicon(p.FaviconUrl);
        p.AnuncioTexto = SanitizeAnuncio(p.AnuncioTexto);
        p.PerfilStore = SanitizePerfil(p.PerfilStore);
    }
}
