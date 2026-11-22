using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Ventas;

public sealed class VentaDto
{
    public long IdVenta { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public string NumVenta { get; set; } = "";
    public DateTime FecVenta { get; set; }
    public int? IdCliente { get; set; }
    public byte TipoDoc { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
    public byte Estado { get; set; }
    public DateTime? FecAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public string BodegaNombre { get; set; } = "";
    public string? ClienteNombre { get; set; }
    public List<VentaDetalleDto> Detalles { get; set; } = [];
    public List<VentaPagoDto> Pagos { get; set; } = [];
}

public sealed class VentaDetalleDto
{
    public long IdVentaDetalle { get; set; }
    public long IdVenta { get; set; }
    public int NumLinea { get; set; }
    public long IdPresProd { get; set; }
    public long IdMovInv { get; set; }
    public long IdProd { get; set; }
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarNombre { get; set; } = "";
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Descuento { get; set; }
    public decimal Subtotal { get; set; }
}

public sealed class VentaPagoDto
{
    public long IdVentaPago { get; set; }
    public string MetodoNombre { get; set; } = "";
    public long IdVenta { get; set; }
    public byte IdMetodoPago { get; set; }
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }
}

// Write contracts must match VentasController records exactly (camelCase)
public sealed class VentaItemSp
{
    public long IdPresProd { get; set; }
    public long? IdLote { get; set; }
    [Range(0.0001, 999999999)] public decimal Cantidad { get; set; }
    [Range(0.0001, 999999999)] public decimal PrecioUnitario { get; set; }
    public decimal Descuento { get; set; }
}

public sealed class VentaPagoSp
{
    [Range(1, 255)] public byte IdMetodoPago { get; set; }
    [Range(0.01, 999999999)] public decimal Monto { get; set; }
    public string? Referencia { get; set; }
}

public sealed class RegistrarVentaDto
{
    [Required] public int IdBodega { get; set; }
    [Required, MinLength(1), MaxLength(30)] public string NumVenta { get; set; } = "";
    public int? IdCliente { get; set; }
    [Range(1, 3)] public byte TipoDoc { get; set; } = 1;
    [MinLength(1)] public List<VentaItemSp> Items { get; set; } = [];
    [MinLength(1)] public List<VentaPagoSp> Pagos { get; set; } = [];
}

public sealed class RegistrarVentaResult
{
    public long IdVenta { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Descuento { get; set; }
    public decimal Impuesto { get; set; }
    public decimal Total { get; set; }
}

public sealed class AnularDto
{
    [Required, MinLength(1), MaxLength(500)] public string Motivo { get; set; } = "";
}
