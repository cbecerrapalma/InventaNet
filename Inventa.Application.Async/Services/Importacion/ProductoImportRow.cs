using CBP.Excel.Attributes;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// S87.2 C1: una fila = una variante. Columnas exactas de ADR-PROD-002 §2.
/// Todo texto crudo: el validador parsea (enteros/fechas/SN) para no perder
/// errores de formato con los valores por defecto del lector tipado.
/// </summary>
public sealed class ProductoImportRow
{
    [ExcelColumn(Name = "CodigoProducto *", Order = 0, IsRequired = true)]
    public string CodigoProducto { get; set; } = string.Empty;

    [ExcelColumn(Name = "NombreProducto *", Order = 1, IsRequired = true)]
    public string NombreProducto { get; set; } = string.Empty;

    [ExcelColumn(Name = "Categoria *", Order = 2, IsRequired = true)]
    public string Categoria { get; set; } = string.Empty;

    [ExcelColumn(Name = "Marca", Order = 3)]
    public string Marca { get; set; } = string.Empty;

    [ExcelColumn(Name = "UnidadBase *", Order = 4, IsRequired = true)]
    public string UnidadBase { get; set; } = string.Empty;

    [ExcelColumn(Name = "Pesable (S/N)", Order = 5)]
    public string Pesable { get; set; } = string.Empty;

    [ExcelColumn(Name = "ManejaLote (S/N)", Order = 6)]
    public string ManejaLote { get; set; } = string.Empty;

    [ExcelColumn(Name = "DiasVencimiento", Order = 7)]
    public string DiasVencimiento { get; set; } = string.Empty;

    [ExcelColumn(Name = "PublicarEnTienda (S/N)", Order = 8)]
    public string PublicarEnTienda { get; set; } = string.Empty;

    [ExcelColumn(Name = "CodigoVariante *", Order = 9, IsRequired = true)]
    public string CodigoVariante { get; set; } = string.Empty;

    [ExcelColumn(Name = "NombreVariante *", Order = 10, IsRequired = true)]
    public string NombreVariante { get; set; } = string.Empty;

    [ExcelColumn(Name = "CodigoBarras", Order = 11)]
    public string CodigoBarras { get; set; } = string.Empty;

    [ExcelColumn(Name = "CodigosAdicionales", Order = 12)]
    public string CodigosAdicionales { get; set; } = string.Empty;

    [ExcelColumn(Name = "PrecioVenta *", Order = 13, IsRequired = true, DataType = ExcelDataType.Integer)]
    public string PrecioVenta { get; set; } = string.Empty;

    [ExcelColumn(Name = "PrecioOferta", Order = 14, DataType = ExcelDataType.Integer)]
    public string PrecioOferta { get; set; } = string.Empty;

    [ExcelColumn(Name = "OfertaDesde (dd-mm-aaaa)", Order = 15, DataType = ExcelDataType.Date)]
    public string OfertaDesde { get; set; } = string.Empty;

    [ExcelColumn(Name = "OfertaHasta (dd-mm-aaaa)", Order = 16, DataType = ExcelDataType.Date)]
    public string OfertaHasta { get; set; } = string.Empty;

    // S93.1: «PrecioMayorista» (D5, congelado) y «Proveedor» (D6, PrecioCompra retirada) ya no son columnas;
    // si un archivo las trae, el validador avisa COLUMNA_IGNORADA.

    [ExcelColumn(Name = "CostoCompra", Order = 18, DataType = ExcelDataType.Integer)]
    public string CostoCompra { get; set; } = string.Empty;

    [ExcelColumn(Name = "StockInicial", Order = 20, DataType = ExcelDataType.Integer)]
    public string StockInicial { get; set; } = string.Empty;

    [ExcelColumn(Name = "Bodega", Order = 21)]
    public string Bodega { get; set; } = string.Empty;

    [ExcelColumn(Name = "Activo (S/N)", Order = 22)]
    public string Activo { get; set; } = string.Empty;
}
