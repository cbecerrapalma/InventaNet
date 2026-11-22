namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// S87.2 C2: validación de dígito verificador EAN-13 / EAN-8 / UPC-A.
/// Puro BCL, sin dependencias. Otro formato ⇒ no estándar (advertencia, no error).
/// </summary>
public static class EanCodigoValidator
{
    public enum FormatoCodigo
    {
        NoEstandar,
        Ean13,
        Ean8,
        UpcA
    }

    public static bool EsEstandar(string? codigo, out FormatoCodigo formato)
    {
        formato = FormatoCodigo.NoEstandar;
        if (string.IsNullOrWhiteSpace(codigo)) return false;
        var digitos = codigo.Trim();
        if (digitos.Length is not (8 or 12 or 13) || !digitos.All(char.IsDigit))
            return false;
        if (!DigitoValido(digitos)) return false;
        formato = digitos.Length switch
        {
            8 => FormatoCodigo.Ean8,
            12 => FormatoCodigo.UpcA,
            _ => FormatoCodigo.Ean13
        };
        return true;
    }

    public static bool DigitoValido(string digitos)
    {
        // Módulo 10 EAN sobre la base (sin el DV): base par ⇒ pesos 1,3,…;
        // base impar ⇒ pesos 3,1,… (contando desde la izquierda).
        var baseLen = digitos.Length - 1;
        var suma = 0;
        for (var i = 0; i < baseLen; i++)
        {
            var d = digitos[i] - '0';
            var pesoUnoPrimero = (i % 2 == 0) == (baseLen % 2 == 0);
            suma += pesoUnoPrimero ? d : d * 3;
        }
        var dv = (10 - (suma % 10)) % 10;
        return dv == digitos[^1] - '0';
    }
}
