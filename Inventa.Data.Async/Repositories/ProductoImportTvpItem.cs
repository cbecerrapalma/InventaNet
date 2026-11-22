namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S88.2: ítem del TVP dbo.ProductoImportItemType ya parseado y resuelto.
/// Lo construye la capa Application (mapper desde ProductoImportRow:
/// partición de categoría, TipoCod, parseos); el repositorio solo lo
/// serializa al DataTable en orden ordinal. Sin atributos de Excel.
/// </summary>
public sealed class ProductoImportTvpItem
{
    public string CodigoProducto { get; set; } = string.Empty;
    public string NombreProducto { get; set; } = string.Empty;
    public string? CategoriaPadre { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string? Marca { get; set; }
    public string UnidadBase { get; set; } = string.Empty;
    // S91.4 (TVP v3): null = celda vacía = conservar el valor actual (existente) / N (nuevo).
    public string? Pesable { get; set; }
    public string? ManejaLote { get; set; }
    public int? DiasVencimiento { get; set; }
    public string? PublicarEnTienda { get; set; }
    public string CodigoVariante { get; set; } = string.Empty;
    public string NombreVariante { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    /// <summary>1=Normal, 4=Proveedor; null = el SP genera interno (3).</summary>
    public byte? TipoCod { get; set; }
    public string? CodigosAdicionales { get; set; }
    public decimal PrecioVenta { get; set; }
    public decimal? PrecioOferta { get; set; }
    public DateTime? OfertaDesde { get; set; }
    public DateTime? OfertaHasta { get; set; }
    public decimal? PrecioMayorista { get; set; }
    public decimal? CostoCompra { get; set; }
    public string? Proveedor { get; set; }
    public int StockInicial { get; set; }
    public string? Bodega { get; set; }
    /// <summary>S91.4 (TVP v3): S/N; null = conservar (existente) / S (nuevo).</summary>
    public string? Activo { get; set; }
}
