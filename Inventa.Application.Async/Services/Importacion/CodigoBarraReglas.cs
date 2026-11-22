namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// Reglas puras de códigos de barra internos (EAN-13 de circulación restringida, prefijo 2).
/// Formato: 2 + IdTenant (3 dígitos) + secuencia (8 dígitos) + DV. Ejemplo tenant 5, secuencia 1: 200500000001 + DV.
/// S92 K7: sin estado. Antes guardaba la secuencia en un diccionario estático que volvía a 1 en cada proceso
/// (AGENTS §12) y no consultaba la BD. La secuencia la decide quien escribe: en la importación,
/// dbo.sp_ImportarProductos (paso 3, MAX de la BD, TipoCod = 3); en la UI (S93), el mismo criterio.
/// </summary>
public static class CodigoBarraReglas
{
    public const string PrefijoInterno = "2";

    /// <summary>Arma el EAN-13 interno para una secuencia dada (la secuencia la provee la BD).</summary>
    public static string FormatearInterno(int idTenant, long secuencia)
    {
        if (idTenant is <= 0 or > 999)
            throw new ArgumentOutOfRangeException(nameof(idTenant), "IdTenant debe ser 1..999");
        if (secuencia is <= 0 or > 99_999_999)
            throw new ArgumentOutOfRangeException(nameof(secuencia), "La secuencia debe ser 1..99999999");
        var base12 = $"{PrefijoInterno}{idTenant:D3}{secuencia:D8}";
        return base12 + CalcularDvEan13(base12);
    }

    /// <summary>
    /// Calcula el dígito verificador EAN-13 (Módulo 10) para 12 dígitos base.
    /// </summary>
    public static int CalcularDvEan13(string base12)
    {
        if (base12.Length != 12 || !base12.All(char.IsDigit))
            throw new ArgumentException("base12 debe ser 12 dígitos numéricos", nameof(base12));

        var suma = 0;
        for (var i = 0; i < 12; i++)
        {
            var d = base12[i] - '0';
            // EAN-13: pesos 1,3,1,3... desde la IZQUIERDA (pos 0 = peso 1)
            suma += (i % 2 == 0) ? d : d * 3;
        }
        return (10 - (suma % 10)) % 10;
    }
}
