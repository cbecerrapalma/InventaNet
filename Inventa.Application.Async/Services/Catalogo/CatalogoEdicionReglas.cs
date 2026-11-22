using Inventa.Application.Async.Services.Importacion;

namespace Inventa.Application.Async.Services.Catalogo;

/// <summary>Tipo de código de barras que el usuario puede ingresar (el Interno solo lo genera el sistema).</summary>
public enum TipoCodigoAlta : byte { Normal = 1, Proveedor = 4 }

/// <summary>
/// S94 — reglas puras de la edición del catálogo (ADR-PROD-001 rev. 2). Sin estado ni E/S; las prueba
/// <c>S94_CatalogoEdicionReglasTests</c>. Deben coincidir con <c>dbo.sp_ImportarProductos</c> v5:
/// <list type="bullet">
/// <item>presentación 1:1: <c>Descrip = LEFT(Prod.Nombre + ' / ' + Var.Nombre, 100)</c> (paso 7);</item>
/// <item>código interno: <c>2</c> + IdTenant (3) + secuencia (8) + DV EAN-13, secuencia = MAX existente + 1, <c>TipoCod = 3</c> (paso 3).</item>
/// </list>
/// </summary>
public static class CatalogoEdicionReglas
{
    public const int MaxDescrip = 100;
    public const int MaxCodigoProducto = 50;
    public const int MaxNombreProducto = 150;
    public const int MaxCodigoVariante = 50;
    public const int MaxNombreVariante = 100;
    public const byte TipoInterno = 3;

    /// <summary>Descripción de la presentación generada (misma expresión que el SP v5).</summary>
    public static string Descripcion(string nombreProducto, string nombreVariante)
    {
        var d = $"{nombreProducto} / {nombreVariante}";
        return d.Length <= MaxDescrip ? d : d[..MaxDescrip];
    }

    /// <summary>Prefijo de los códigos internos del tenant (<c>2</c> + IdTenant con 3 dígitos).</summary>
    public static string PrefijoInterno(int idTenant) => $"{CodigoBarraReglas.PrefijoInterno}{idTenant:D3}";

    /// <summary>Siguiente código interno a partir del mayor existente (o <c>null</c> si el tenant no tiene ninguno).</summary>
    public static string SiguienteInterno(int idTenant, string? mayorExistente)
    {
        long secuencia = 0;
        var prefijo = PrefijoInterno(idTenant);
        if (mayorExistente is { Length: 13 } m && m.StartsWith(prefijo, StringComparison.Ordinal)
            && long.TryParse(m.AsSpan(4, 8), out var s))
            secuencia = s;
        return CodigoBarraReglas.FormatearInterno(idTenant, secuencia + 1);
    }

    /// <summary>
    /// Valida un código ingresado por el usuario y devuelve su <c>TipoCod</c>, o el motivo del rechazo.
    /// Normal: EAN-13, EAN-8 o UPC-A con dígito verificador válido, fuera del prefijo interno del tenant.
    /// Proveedor: 6 a 20 dígitos, sin dígito verificador.
    /// </summary>
    public static (byte? TipoCod, string? Error) ClasificarCodigo(string? codigo, TipoCodigoAlta tipo, int idTenant)
    {
        var c = (codigo ?? "").Trim();
        if (c.Length == 0) return (null, "Ingresa el código de barras.");
        if (!c.All(char.IsAsciiDigit)) return (null, "El código de barras solo admite dígitos.");
        switch (tipo)
        {
            case TipoCodigoAlta.Normal:
                if (!EanCodigoValidator.EsEstandar(c, out _))
                    return (null, "El código no es un EAN-13, EAN-8 o UPC-A válido (revisa el dígito verificador). Si es un código del proveedor, elige «Proveedor».");
                if (c.Length == 13 && c.StartsWith(PrefijoInterno(idTenant), StringComparison.Ordinal))
                    return (null, $"El prefijo {PrefijoInterno(idTenant)} está reservado para los códigos internos que genera el sistema.");
                return ((byte)TipoCodigoAlta.Normal, null);
            case TipoCodigoAlta.Proveedor:
                if (c.Length is < 6 or > 20) return (null, "El código del proveedor debe tener entre 6 y 20 dígitos.");
                return ((byte)TipoCodigoAlta.Proveedor, null);
            default:
                return (null, "Tipo de código no válido. Los códigos internos los genera el sistema.");
        }
    }

    /// <summary>Recorta y valida un texto obligatorio; devuelve el mensaje de error o <c>null</c>.</summary>
    public static string? ValidarTexto(string? valor, string campo, int max, out string limpio)
    {
        limpio = (valor ?? "").Trim();
        if (limpio.Length == 0) return $"{campo} es obligatorio.";
        if (limpio.Length > max) return $"{campo} admite hasta {max} caracteres.";
        return null;
    }
}
