namespace Inventa.Application.Async.Services.Precios;

/// <summary>S93.1 (D7): decimales permitidos según la moneda del tenant. CLP no usa decimales.</summary>
public static class ReglasMoneda
{
    public static int Decimales(string? moneda) =>
        string.Equals(moneda?.Trim(), "CLP", StringComparison.OrdinalIgnoreCase) || string.IsNullOrWhiteSpace(moneda) ? 0 : 2;

    public static bool RespetaDecimales(decimal valor, int decimales) => valor == Math.Round(valor, decimales);

    public static decimal Redondear(decimal valor, int decimales) => Math.Round(valor, decimales, MidpointRounding.AwayFromZero);
}

/// <summary>S93.1 (K3): redondeo comercial de un precio calculado por porcentaje.</summary>
public enum RedondeoPrecioModo
{
    Ninguno = 0,
    A10 = 1,
    A100 = 2,
    Termina990 = 3
}

public static class RedondeoPrecio
{
    /// <summary>
    /// A10/A100: múltiplo más cercano (mitades hacia arriba). Termina990: el menor valor ≥ v que termina en 990
    /// (v ≤ 990 → 990; si no, 1000 × ceil((v − 990) / 1000) + 990).
    /// </summary>
    public static decimal Aplicar(decimal valor, RedondeoPrecioModo modo) => modo switch
    {
        RedondeoPrecioModo.A10 => Math.Round(valor / 10m, 0, MidpointRounding.AwayFromZero) * 10m,
        RedondeoPrecioModo.A100 => Math.Round(valor / 100m, 0, MidpointRounding.AwayFromZero) * 100m,
        RedondeoPrecioModo.Termina990 => valor <= 990m ? 990m : 1000m * Math.Ceiling((valor - 990m) / 1000m) + 990m,
        _ => valor,
    };
}
