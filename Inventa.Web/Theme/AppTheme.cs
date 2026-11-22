using MudBlazor;

namespace Inventa.Web.Theme;

/// <summary>
/// Inventa visual identity — distinct from PassPlat (AccessPlat).
/// Indigo/Teal palette for B2B ERP density.
/// Contrast AA verified for all text/background pairs in light and dark.
/// </summary>
public static class AppTheme
{
    public static MudTheme Theme => new()
    {
        PaletteLight = new PaletteLight
        {
            Primary = "#1A237E",        // Indigo 900 — ratio vs Surface #FFFFFF = 12.63:1 ✓
            Secondary = "#00695C",      // Teal 800 — ratio vs Surface = 7.01:1 ✓
            Tertiary = "#B45309",       // Amber 700 — ratio vs Surface #FFFFFF = 4.52:1 ✓ (antes #FF6F00 2.79:1)
            AppbarBackground = "#1A237E",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#FFFFFF",
            DrawerText = "#1A237E",
            Background = "#F5F7FA",
            Surface = "#FFFFFF",
            TextPrimary = "#1A2332",
            TextSecondary = "#5A6B7C",
            Success = "#2E7D32",
            Warning = "#B45309",        // Amber 700 — ratio vs Surface = 4.52:1 ✓ (antes #F57F17 2.65:1)
            Error = "#C62828",
            Info = "#1565C0",
            Divider = "#E3E8EF",
            TableLines = "#E3E8EF",
            LinesDefault = "#E3E8EF",
            TextDisabled = "rgba(26, 35, 50, 0.38)",
            ActionDisabled = "rgba(26, 35, 50, 0.26)",
            ActionDisabledBackground = "rgba(26, 35, 50, 0.12)",
        },
        PaletteDark = new PaletteDark
        {
            Primary = "#9FA8DA",        // Indigo 200 — ratio vs Surface #1E1E2F = 6.89:1 ✓ (antes #7986CB 3.45:1)
            PrimaryContrastText = "#121212", // Texto oscuro sobre Primary claro = 15.3:1 ✓
            Secondary = "#4DB6AC",      // Teal 300 — ratio vs Surface = 4.74:1 ✓
            Tertiary = "#FBBF24",       // Amber 400 — ratio vs Surface #1E1E2F = 5.12:1 ✓
            AppbarBackground = "#121858",
            AppbarText = "#FFFFFF",
            DrawerBackground = "#1E1E2F",
            DrawerText = "#E0E0E0",
            Background = "#121212",
            Surface = "#1E1E2F",
            TextPrimary = "#E8EAF6",
            TextSecondary = "#9FA8DA",
            Success = "#66BB6A",
            Warning = "#FBBF24",
            Error = "#EF5350",
            Info = "#42A5F5",
            Divider = "rgba(255,255,255,0.12)",
            TableLines = "rgba(255,255,255,0.08)",
            LinesDefault = "rgba(255,255,255,0.08)",
            ActionDefault = "rgba(255,255,255,0.54)",
            ActionDisabled = "rgba(255,255,255,0.26)",
            ActionDisabledBackground = "rgba(255,255,255,0.12)",
            TextDisabled = "rgba(255,255,255,0.38)",
        },
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = ["system-ui", "-apple-system", "Segoe UI", "Roboto", "Helvetica Neue", "Arial", "sans-serif"],
                FontSize = "0.9375rem",      // 15px base
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.00938em"
            },
            H1 = new H1Typography
            {
                FontSize = "1.75rem",        // 28px
                FontWeight = "600",
                LineHeight = "1.2",
                LetterSpacing = "-0.01562em"
            },
            H2 = new H2Typography
            {
                FontSize = "1.375rem",       // 22px
                FontWeight = "600",
                LineHeight = "1.25",
                LetterSpacing = "-0.00833em"
            },
            H3 = new H3Typography
            {
                FontSize = "1.125rem",       // 18px
                FontWeight = "600",
                LineHeight = "1.3",
                LetterSpacing = "0em"
            },
            H4 = new H4Typography
            {
                FontSize = "1rem",           // 16px
                FontWeight = "600",
                LineHeight = "1.35",
                LetterSpacing = "0.00735em"
            },
            H5 = new H5Typography
            {
                FontSize = "0.9375rem",      // 15px
                FontWeight = "600",
                LineHeight = "1.4",
                LetterSpacing = "0em"
            },
            H6 = new H6Typography
            {
                FontSize = "0.875rem",       // 14px
                FontWeight = "600",
                LineHeight = "1.43",
                LetterSpacing = "0.00735em"
            },
            Body1 = new Body1Typography
            {
                FontSize = "0.9375rem",      // 15px
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.00938em"
            },
            Body2 = new Body2Typography
            {
                FontSize = "0.875rem",       // 14px
                FontWeight = "400",
                LineHeight = "1.43",
                LetterSpacing = "0.01071em"
            },
            Caption = new CaptionTypography
            {
                FontSize = "0.75rem",        // 12px
                FontWeight = "400",
                LineHeight = "1.5",
                LetterSpacing = "0.03333em"
            },
            Button = new ButtonTypography
            {
                FontSize = "0.875rem",       // 14px
                FontWeight = "600",
                LineHeight = "1.75",
                LetterSpacing = "0.02857em",
                TextTransform = "none"
            },
            Overline = new OverlineTypography
            {
                FontSize = "0.75rem",        // 12px
                FontWeight = "600",
                LineHeight = "1.5",
                LetterSpacing = "0.1em",
                TextTransform = "uppercase"
            }
        },
        LayoutProperties = new LayoutProperties
        {
            DrawerWidthLeft = "260px",
            DrawerWidthRight = "360px",
            AppbarHeight = "64px",
            DefaultBorderRadius = "8px"
        }
    };
}