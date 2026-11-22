// S91: DTOs del canal api/importaciones (solo lectura/escritura vía WebAPI; Web nunca toca la BD).
namespace Inventa.Web.Models.Importacion;

public sealed class IncidenciaLoteDto
{
    public int Fila { get; set; }
    public string Columna { get; set; } = "";
    public string Codigo { get; set; } = "";
    public string Mensaje { get; set; } = "";
    public string Severidad { get; set; } = "";
}

public sealed class LoteCreadoDto
{
    public long Id { get; set; }
    public string Estado { get; set; } = "";
    public int Filas { get; set; }
    public int Productos { get; set; }
    public int Variantes { get; set; }
    public int Nuevos { get; set; }
    public int Existentes { get; set; }
    public int Errores { get; set; }
    public int Advertencias { get; set; }
    public bool YaAplicado { get; set; }
    public List<IncidenciaLoteDto> Incidencias { get; set; } = [];
    /// <summary>S91.4: vista previa de cambios (null si el archivo tiene errores).</summary>
    public ResumenCambiosDto? Cambios { get; set; }
}

public sealed class LoteDetalleDto
{
    public long Id { get; set; }
    public string Tipo { get; set; } = "";
    public string NombreArchivo { get; set; } = "";
    public string Estado { get; set; } = "";
    public int Filas { get; set; }
    public int Productos { get; set; }
    public int Variantes { get; set; }
    public int Nuevos { get; set; }
    public int Existentes { get; set; }
    public int Errores { get; set; }
    public int Advertencias { get; set; }
    public DateTime FecCrea { get; set; }
    public DateTime? FecAplicada { get; set; }
    public List<IncidenciaLoteDto> Incidencias { get; set; } = [];
    /// <summary>S91.4: resumen de cambios guardado al validar (null en lotes anteriores).</summary>
    public ResumenCambiosDto? Cambios { get; set; }
}

public sealed class LoteItemDto
{
    public long Id { get; set; }
    public string Tipo { get; set; } = "";
    public string Estado { get; set; } = "";
    public int Filas { get; set; }
    public int Errores { get; set; }
    public DateTime FecCrea { get; set; }
    public string NombreArchivo { get; set; } = "";
    public int Advertencias { get; set; }
    public DateTime? FecAplicada { get; set; }
    /// <summary>S91.4: filas modificadas (null en lotes anteriores).</summary>
    public int? Modificados { get; set; }
}

/// <summary>S118: textos y colores del estado y el tipo de un lote de importación (códigos del backend).</summary>
public static class EstadoLoteUi
{
    public static string Texto(string estado) => estado switch
    {
        "ConErrores" => "Con errores",
        _ => estado,
    };

    public static MudBlazor.Color Color(string estado) => estado switch
    {
        "Aplicado" => MudBlazor.Color.Success,
        "Validado" => MudBlazor.Color.Info,
        "ConErrores" => MudBlazor.Color.Warning,
        "Fallido" => MudBlazor.Color.Error,
        _ => MudBlazor.Color.Default,
    };

    public static string Tipo(string tipo) => tipo switch
    {
        "PRODUCTOS" => "Productos",
        _ => tipo,
    };
}

// ── S91.4: vista previa / auditoría de cambios ──
public sealed class ResumenCambiosDto
{
    public int Nuevos { get; set; }
    public int Modificados { get; set; }
    public int SinCambios { get; set; }
    public Dictionary<string, int> PorCampo { get; set; } = [];
    public bool HayCambios { get; set; }
}

public sealed class CambioCampoDto
{
    public string Campo { get; set; } = "";
    public string Actual { get; set; } = "";
    public string Nuevo { get; set; } = "";
}

public sealed class CambioFilaDto
{
    public int Fila { get; set; }
    public string CodigoProducto { get; set; } = "";
    public string CodigoVariante { get; set; } = "";
    public string Accion { get; set; } = "";
    public List<CambioCampoDto> Campos { get; set; } = [];
}

public sealed class CambiosPageDto
{
    public bool Disponible { get; set; }
    public List<CambioFilaDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class LotesPageDto
{
    public List<LoteItemDto> Items { get; set; } = [];
    public int Total { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
}

public sealed class AplicarResultadoDto
{
    public long IdImportacionLote { get; set; }
    public string Estado { get; set; } = "";
    public int ProductosNuevos { get; set; }
    public int ProductosExistentes { get; set; }
    public int VariantesNuevas { get; set; }
    public int VariantesExistentes { get; set; }
    public int PresentacionesNuevas { get; set; }
    public int PreciosCreados { get; set; }
    public int CodigosBarraCreados { get; set; }
    public int StockAjustado { get; set; }
    public string ResumenJson { get; set; } = "{}";
}
