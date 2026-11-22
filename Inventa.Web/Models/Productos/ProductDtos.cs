using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Productos;

// === Entities as returned by WebAPI (camelCase JSON) ===
public sealed class ProdDto
{
    public long IdProd { get; set; }
    public int IdTenant { get; set; }
    public long IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int IdUniBase { get; set; }
    public int? DiasVen { get; set; }
    public bool ManejaLot { get; set; }
    public bool EsPesable { get; set; }
    public bool Activo { get; set; }
    public bool VisibleStore { get; set; }
}

// S57.3 governance: centralized publishability diagnostics (rendered by UI, never recomputed client-side).
public sealed class PublishabilityDto
{
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
    public bool VisibleStore { get; set; }
    public bool TienePresVenta { get; set; }
    public bool TienePrecio { get; set; }
    public bool TieneImagen { get; set; }
    public bool TieneMarca { get; set; }
    public bool TieneCategoria { get; set; }
    public bool Publicable { get; set; }
    public List<string> Razones { get; set; } = new();
}

// S57.3 safe publication result: applied flag + non-blocking warnings.
public sealed class VisibilidadResultDto
{
    public long IdProd { get; set; }
    public bool VisibleStore { get; set; }
    public List<string> Advertencias { get; set; } = new();
}

// S57.3 P3 publication preview: mirrors the public catalog detail shape (PublicProductDetailDto).
// S57.69: precio normal único (sin TipoPrec/TipoNombre).
public sealed class PreviewPriceDto
{
    public long IdPrecio { get; set; }
    public decimal Valor { get; set; }
    public DateOnly FecVig { get; set; }
    public bool Vigente { get; set; }
}
public sealed class PreviewPresentacionDto
{
    public long IdPresProd { get; set; }
    public string Descrip { get; set; } = "";
    public string Unidad { get; set; } = "";
    public List<PreviewPriceDto> Precios { get; set; } = new();
}
public sealed class PreviewVarianteDto
{
    public long IdVarProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public List<PreviewPresentacionDto> Presentaciones { get; set; } = new();
}
public sealed class StorefrontProductDetailDto
{
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string CategoriaNombre { get; set; } = "";
    public string? MarcaNombre { get; set; }
    public List<PreviewVarianteDto> Variantes { get; set; } = new();
    public string? ImagenPrincipal { get; set; }
}
public sealed class VarProdDto
{
    public long IdVarProd { get; set; }
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public decimal CostoProm { get; set; }
    public bool Activo { get; set; }
}
public sealed class PresProdDto
{
    public long IdPresProd { get; set; }
    public long IdVarProd { get; set; }
    public int IdUnidad { get; set; }
    public string Descrip { get; set; } = "";
    public bool EsVenta { get; set; } = true;
    public bool EsCompra { get; set; } = true;
    public bool Activo { get; set; }
}
/// <summary>S117 (P19): fila del buscador de presentaciones (GET api/productos/presentaciones/search).</summary>
public sealed class PresentacionBusquedaDto
{
    public long IdPresProd { get; set; }
    public long IdProd { get; set; }
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarCodigo { get; set; } = "";
    public string VarNombre { get; set; } = "";
    public string PresDescrip { get; set; } = "";
    public string? CodigoBarra { get; set; }
    public decimal? PrecioVigente { get; set; }
    public decimal CostoProm { get; set; }
    public decimal? Disponible { get; set; }
}
public sealed class CodBarDto
{
    public long IdCodBar { get; set; }
    public long IdPresProd { get; set; }
    public string Codigo { get; set; } = "";
    public byte TipoCod { get; set; }
    public bool Principal { get; set; }
}
public sealed class CategoriaDto
{
    public long IdCategoria { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}
public sealed class MarcaDto
{
    public int IdMarca { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}
public sealed class UnidadDto
{
    public int IdUnidad { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}
public sealed class ProveedorDto
{
    public int IdProveedor { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string RazonSocial { get; set; } = "";
}

// === Write DTOs (must match controller records) ===
public class CreateProdDto
{
    public long IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    [Required, MinLength(1), MaxLength(50)] public string Codigo { get; set; } = "";
    [Required, MinLength(1), MaxLength(150)] public string Nombre { get; set; } = "";
    public int IdUniBase { get; set; }
}

/// <summary>S94: tipo de código que ingresa el usuario (el interno lo genera el sistema).</summary>
public static class TipoCodigoAlta
{
    public const byte Normal = 1;
    public const byte Proveedor = 4;
}

/// <summary>S94: variante nueva = variante + presentación 1:1 + código principal + precio oficial inicial.</summary>
public sealed class VarianteNuevaDto
{
    [Required, MaxLength(50)] public string Codigo { get; set; } = "";
    [Required, MaxLength(100)] public string Nombre { get; set; } = "";
    public decimal? PrecioVenta { get; set; }
    /// <summary>Vacío ⇒ el sistema genera un código interno (TipoCod 3).</summary>
    [MaxLength(20)] public string? CodigoBarras { get; set; }
    public byte TipoCodigo { get; set; } = TipoCodigoAlta.Normal;
}

/// <summary>S94: producto nuevo con su primera variante.</summary>
public sealed class ProductoNuevoDto : CreateProdDto
{
    public VarianteNuevaDto Variante { get; set; } = new();
}

public sealed class VarianteEdicionDto
{
    [Required, MaxLength(100)] public string Nombre { get; set; } = "";
    public bool Activo { get; set; } = true;
}

public sealed class CodigoNuevoDto
{
    [Required, MaxLength(20)] public string Codigo { get; set; } = "";
    public byte Tipo { get; set; } = TipoCodigoAlta.Normal;
    public bool Principal { get; set; }
}

/// <summary>S94: identificadores afectados por una operación de edición del catálogo.</summary>
public sealed class CatalogoEdicionResultadoDto
{
    public long IdProd { get; set; }
    public long? IdVarProd { get; set; }
    public long? IdPresProd { get; set; }
    public long? IdCodBar { get; set; }
    public string? CodigoBarras { get; set; }
    public long? IdPrecio { get; set; }
}

public sealed class ProductoImagenDto
{
    public int IdProductoImagen { get; set; }
    public long IdProd { get; set; }
    public string Url { get; set; } = "";
    // S108: variante 'listado' (400 px, WebP) que la API ya entrega (pública o firmada); null = pendiente.
    public string? UrlMiniatura { get; set; }
    public string NombreArchivo { get; set; } = "";
    public string ContentType { get; set; } = "";
    public long TamanoBytes { get; set; }
    public int Orden { get; set; }
    public bool EsPrincipal { get; set; }
    public bool Activo { get; set; }
    public DateTime FecCrea { get; set; }
}

// S104: autorización de subida directa (POST api/productos/{id}/imagenes/subidas). El navegador envía los bytes
// a Url con Metodo y Headers, sin pasar por la API, y después confirma IdArchivo.
public sealed class ProductoImagenSubidaDto
{
    public long IdArchivo { get; set; }
    public string Url { get; set; } = "";
    public string Metodo { get; set; } = "PUT";
    public Dictionary<string, string> Headers { get; set; } = new();
    public DateTimeOffset Expira { get; set; }
}

// Form helpers
public sealed class ProductoFormModel : CreateProdDto { }
