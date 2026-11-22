namespace Inventa.Domain.Entities;

/// <summary>
/// S87: lote auditable de importación (ADR-PROD-002 §3). Solo cabecera+resumen;
/// el archivo vive en almacenamiento privado. Sin escritura de catálogo en S87.
/// </summary>
public class ImportacionLote
{
    public long IdImportacionLote { get; set; }
    public int IdTenant { get; set; }
    public string Tipo { get; set; } = null!;
    public string NombreArchivo { get; set; } = null!;
    public string Sha256 { get; set; } = null!;
    public string RutaArchivo { get; set; } = null!;
    public string Estado { get; set; } = null!;
    public int Filas { get; set; }
    public int Productos { get; set; }
    public int Variantes { get; set; }
    public int Nuevos { get; set; }
    public int Existentes { get; set; }
    public int Errores { get; set; }
    public int Advertencias { get; set; }
    public int? IdUsuario { get; set; }
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    public DateTime? FecAplicada { get; set; }
    public string? ResumenJson { get; set; }

    // Navigation
    public Tenant? Tenant { get; set; }
    public Usuario? Usuario { get; set; }
}

public static class ImportacionLoteEstados
{
    public const string Validado = "Validado";
    public const string ConErrores = "ConErrores";
    public const string Aplicado = "Aplicado";
    public const string Fallido = "Fallido";
}

public static class ImportacionLoteTipos
{
    public const string Productos = "PRODUCTOS";
}
