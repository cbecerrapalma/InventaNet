using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Compras;

public sealed class OrdenCompraDto
{
    public long IdOrdenCompra { get; set; }
    public int IdTenant { get; set; }
    public int IdProveedor { get; set; }
    public string NumOrden { get; set; } = "";
    public DateOnly FecOrden { get; set; }
    public DateOnly? FecEntrega { get; set; }
    public byte Estado { get; set; }
    public decimal Total { get; set; }
    public string? Observaciones { get; set; }
    public DateTime FecCrea { get; set; }
    public int? UsuarioCrea { get; set; }
    public string ProveedorNombre { get; set; } = "";
    public List<OrdenCompraDetalleDto> Detalles { get; set; } = [];
}

public sealed class OrdenCompraDetalleDto
{
    public long IdProd { get; set; }
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarNombre { get; set; } = "";
    public long IdOCDetalle { get; set; }
    public long IdOrdenCompra { get; set; }
    public long IdPresProd { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class CreateOrdenDetalleDto
{
    [Required, Range(1, long.MaxValue)]
    public long IdPresProd { get; set; }
    [Required, Range(typeof(decimal), "0.0001", "999999999")]
    public decimal Cantidad { get; set; }
    [Required, Range(typeof(decimal), "0.0001", "999999999")]
    public decimal PrecioUnitario { get; set; }
}

public sealed class CreateOrdenDto
{
    [Required]
    public int IdProveedor { get; set; }
    [Required]
    public DateOnly Fecha { get; set; } = DateOnly.FromDateTime(DateTime.Today);
    [MaxLength(500)]
    public string? Observacion { get; set; }
    public List<CreateOrdenDetalleDto>? Detalles { get; set; }
}

// Reception (matches ComprasController RecepcionDto)
public sealed class OCRecepcionItemSp
{
    public long IdOCDetalle { get; set; }
    public long? IdLote { get; set; }
    public decimal CantRecibida { get; set; }
    public decimal CostoUnitario { get; set; }
}

public sealed class RecepcionDto
{
    [Required]
    public int IdBodega { get; set; }
    public List<OCRecepcionItemSp> Items { get; set; } = [];
}
