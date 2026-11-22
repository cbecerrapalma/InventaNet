namespace Inventa.Domain.Entities;

/// <summary>
/// Archivo guardado en el storage (tabla genérica <c>Archivo</c>): dónde está y en qué estado quedó su subida.
/// La entidad de negocio que lo usa (por ejemplo <see cref="ProductoImagen"/>) lo referencia por <see cref="IdArchivo"/>.
/// </summary>
public class Archivo
{
    public long IdArchivo { get; set; }
    public int IdTenant { get; set; }
    public string Proveedor { get; set; } = null!;
    public string Contenedor { get; set; } = null!;
    public string Clave { get; set; } = null!;
    public string Entidad { get; set; } = null!;
    public long IdEntidad { get; set; }
    public string NombreArchivo { get; set; } = null!;
    public string ContentType { get; set; } = null!;
    public long TamanoBytes { get; set; }
    public string? ETag { get; set; }
    public byte Estado { get; set; }
    public DateTime FecCrea { get; set; }
    public DateTime? FecEstado { get; set; }

    public Tenant Tenant { get; set; } = null!;
}

/// <summary>Valores de <see cref="Archivo.Estado"/>.</summary>
public static class ArchivoEstado
{
    public const byte Pendiente = 0;
    public const byte Confirmado = 1;
    public const byte Rechazado = 2;
    public const byte Eliminado = 3;
}
