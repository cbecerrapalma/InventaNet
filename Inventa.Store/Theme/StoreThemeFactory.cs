using System.Globalization;
using System.Text.RegularExpressions;
using MudBlazor;

namespace Inventa.Store.Theme;

/// <summary>
/// S61.1 tema por tenant: estático y puro, sin DI. Todo el color de marca deriva
/// de un único ColorAcento; el primario se oscurece hasta ser legible (WCAG 2.1 AA).
/// </summary>
public static partial class StoreThemeFactory
{
    public const string DefaultAccent = "#1D4E89";
    public const string ContrastOnPrimary = "#FFFFFF";
    public const double MinContrast = 4.5;

    public static readonly string[] SystemFontStack =
    [
        "system-ui", "-apple-system", "Segoe UI", "Roboto",
        "Helvetica Neue", "Arial", "sans-serif",
    ];

    [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
    private static partial Regex HexRegex();

    public static string NormalizeAccent(string? hex)
    {
        if (string.IsNullOrWhiteSpace(hex)) return DefaultAccent;
        hex = hex.Trim();
        return HexRegex().IsMatch(hex) ? hex.ToUpperInvariant() : DefaultAccent;
    }

    public static (double R, double G, double B) ToRgb(string hex)
    {
        // MudBlazor devuelve #RRGGBBAA al leer MudColor: se ignora el alfa
        // antes de normalizar (si no, ambos colapsan al DefaultAccent y el ratio da 1).
        if (hex is not null && hex.Length == 9 && hex.StartsWith('#')) hex = hex[..7];
        hex = NormalizeAccent(hex).TrimStart('#');
        return (
            int.Parse(hex[..2], NumberStyles.HexNumber) / 255.0,
            int.Parse(hex[2..4], NumberStyles.HexNumber) / 255.0,
            int.Parse(hex[4..6], NumberStyles.HexNumber) / 255.0);
    }

    static double Lin(double c) => c <= 0.03928 ? c / 12.92 : Math.Pow((c + 0.055) / 1.055, 2.4);

    public static double Luminance(string hex)
    {
        var (r, g, b) = ToRgb(hex);
        return 0.2126 * Lin(r) + 0.7152 * Lin(g) + 0.0722 * Lin(b);
    }

    // WCAG 2.x: (L1 + 0.05) / (L2 + 0.05), L1 la mayor.
    public static double ContrastRatio(string hexA, string hexB)
    {
        var (l1, l2) = (Luminance(hexA), Luminance(hexB));
        if (l1 < l2) (l1, l2) = (l2, l1);
        return (l1 + 0.05) / (l2 + 0.05);
    }

    /// <summary>
    /// Oscurece el acento (HSL, tono conservado) hasta contraste ≥ 4.5 contra blanco.
    /// Determinista, con límite de iteraciones; negro como último recurso.
    /// </summary>
    public static string AccessiblePrimary(string? accentHex)
    {
        var hex = NormalizeAccent(accentHex);
        if (ContrastRatio(hex, ContrastOnPrimary) >= MinContrast) return hex;
        RgbToHsl(ToRgb(hex), out var h, out var s, out var l);
        for (var i = 0; i < 50; i++)
        {
            l = Math.Max(0, l - 0.02);
            hex = ToHex(HslToRgb(h, s, l));
            if (ContrastRatio(hex, ContrastOnPrimary) >= MinContrast) return hex;
            if (l <= 0) break;
        }
        return "#000000";
    }

    static void RgbToHsl((double R, double G, double B) c, out double h, out double s, out double l)
    {
        var (max, min) = (Math.Max(c.R, Math.Max(c.G, c.B)), Math.Min(c.R, Math.Min(c.G, c.B)));
        l = (max + min) / 2;
        if (max == min) { h = 0; s = 0; return; }
        var d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        h = max == c.R ? (c.G - c.B) / d + (c.G < c.B ? 6 : 0)
            : max == c.G ? (c.B - c.R) / d + 2 : (c.R - c.G) / d + 4;
        h /= 6;
    }

    static (double R, double G, double B) HslToRgb(double h, double s, double l)
    {
        if (s == 0) return (l, l, l);
        double Hue(double p, double q, double t)
        {
            if (t < 0) t += 1; if (t > 1) t -= 1;
            if (t < 1.0 / 6) return p + (q - p) * 6 * t;
            if (t < 0.5) return q;
            if (t < 2.0 / 3) return p + (q - p) * (2.0 / 3 - t) * 6;
            return p;
        }
        var q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        var p = 2 * l - q;
        return (Hue(p, q, h + 1.0 / 3), Hue(p, q, h), Hue(p, q, h - 1.0 / 3));
    }

    static string ToHex((double R, double G, double B) c) =>
        $"#{(int)Math.Round(c.R * 255):X2}{(int)Math.Round(c.G * 255):X2}{(int)Math.Round(c.B * 255):X2}";

    public static MudTheme Create(string? colorAcento)
    {
        var primary = AccessiblePrimary(NormalizeAccent(colorAcento));
        return new MudTheme
        {
            PaletteLight = new PaletteLight
            {
                Primary = primary,
                PrimaryContrastText = ContrastOnPrimary,
                Secondary = "#374151",
                Background = "#FAFAF9",
                Surface = "#FFFFFF",
                AppbarBackground = "#FFFFFF",
                AppbarText = "#111827",
                TextPrimary = "#111827",
                TextSecondary = "#4B5563",
                Error = "#B91C1C",
                Success = "#047857",
                Warning = "#B45309",
                Info = "#0369A1",
                Divider = "#E5E7EB",
                LinesDefault = "#E5E7EB",
            },
            Typography = new Typography
            {
                Default = new DefaultTypography { FontFamily = SystemFontStack },
            },
            LayoutProperties = new LayoutProperties { DefaultBorderRadius = "8px" },
        };
    }
}
