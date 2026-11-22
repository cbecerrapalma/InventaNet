namespace Inventa.Store.Services;

// S62.2 enlaces externos puros (sin DI): WhatsApp y mapa. Reglas en S62.1-SHELL §4.
public static class StoreLinks
{
    public static string? WhatsAppUrl(string? numero, string? mensaje = null)
    {
        if (string.IsNullOrWhiteSpace(numero)) return null;
        var digitos = new string(numero.Where(char.IsDigit).ToArray());
        if (digitos.StartsWith("00", StringComparison.Ordinal)) digitos = digitos[2..];
        if (digitos.Length == 9 && digitos[0] == '9')
            digitos = "56" + digitos; // móvil chileno sin país
        else if (digitos.Length is < 10 or > 15)
            return null; // vacío, corto, fijo de 9 o fuera de rango E.164
        var url = $"https://wa.me/{digitos}";
        return string.IsNullOrWhiteSpace(mensaje) ? url : $"{url}?text={Uri.EscapeDataString(mensaje)}";
    }

    public static string? MapsUrl(string? direccion, string? comuna, string? ciudad)
    {
        if (string.IsNullOrWhiteSpace(direccion)) return null;
        var partes = new[] { direccion.Trim(), comuna?.Trim(), ciudad?.Trim() }
            .Where(p => !string.IsNullOrWhiteSpace(p));
        return $"https://www.google.com/maps/search/?api=1&query={Uri.EscapeDataString(string.Join(", ", partes))}";
    }
}
