namespace Inventa.Application.Dtos;

public sealed record TenantDto(int IdTenant, string Codigo, string Nombre, bool Activo);
public sealed record UnidadDto(int IdUnidad, string Codigo, string Nombre);
public sealed record MetodoPagoDto(int IdMetodoPago, string Codigo, string Nombre, bool Activo);
public sealed record BodegaDto(int IdBodega, int IdTenant, string Codigo, string Nombre, bool Activo);
public sealed record MarcaDto(int IdMarca, int IdTenant, string Codigo, string Nombre, bool Activo);
public sealed record CategoriaDto(long IdCategoria, int IdTenant, string Codigo, string Nombre, bool Activo, long? IdPadre = null, short Orden = 0, string? ImagenUrl = null);

public sealed record ClienteDto(
    int IdCliente, int IdTenant, string Codigo, string Nombre,
    string? RUT, string? Direccion, string? Telefono, string? Email,
    byte TipoCliente, bool Activo);

public sealed record ProveedorDto(
    int IdProveedor, int IdTenant, string Codigo, string RazonSocial,
    string? NombreComercial, string? RUT, string? Direccion, string? Telefono,
    string? Email, string? Contacto, bool Activo);

public sealed record ProdDto(
    long IdProd, int IdTenant, long IdCategoria, int? IdMarca,
    string Codigo, string Nombre, int IdUniBase, int? DiasVen,
    bool ManejaLot, bool EsPesable, bool Activo);

public sealed record VarProdDto(
    long IdVarProd, long IdProd, string Codigo, string Nombre,
    decimal CostoProm, bool Activo);

public sealed record PresProdDto(
    long IdPresProd, long IdVarProd, int IdUnidad, string Descrip,
    bool EsVenta, bool EsCompra, bool Activo);

public sealed record ProdConvDto(long IdProdConv, long IdVarProd, int IdUnidad, decimal Factor);
public sealed record CodBarDto(long IdCodBar, long IdPresProd, string Codigo, byte TipoCod, bool Principal);

public sealed record AtriDefDto(long IdAtriDef, long IdCategoria, string Codigo, string Nombre, bool EsOblig, bool Activo);
public sealed record AtriValDto(long IdAtriVal, long IdAtriDef, string Codigo, string Nombre, int Orden, bool Activo);
public sealed record VarAtriDto(long IdVarAtri, long IdVarProd, long IdAtriVal);

public sealed record LoteDto(long IdLote, int IdTenant, long IdVarProd, string Codigo, DateOnly FecVen, bool Activo);

public sealed record StockDto(
    long IdStock, int IdTenant, int IdBodega, long IdVarProd, long? IdLote,
    decimal CantAct, decimal CostoProm, DateTime FecAct);

public sealed record StockDispDto(
    long IdStockDisp, int IdTenant, long IdPresProd, int IdBodega, decimal CantDisp);

public sealed record MovInvDto(
    long IdMovInv, int IdTenant, int IdBodega, long IdPresProd, long? IdLote,
    DateTime FecMov, byte TipoMov, decimal CantCom, decimal CantBase, decimal CostoUni);

public sealed record PrecioDto(long IdPrecio, long IdPresProd, decimal Valor, DateOnly FecVig, bool Activo);

// S57.69: mayorista normalizado (sin discriminator).
public sealed record PrecioMayoristaDto(long IdPrecioMayorista, long IdPresProd, decimal Valor, DateOnly FecVig, bool Activo);

public sealed record VentaDto(
    long IdVenta, int IdTenant, int IdBodega, string NumVenta, DateTime FecVenta,
    int? IdCliente, byte TipoDoc, int? IdUsuario, decimal SubTotal,
    decimal Descuento, decimal Impuesto, decimal Total, byte Estado,
    DateTime? FecAnulacion, string? MotivoAnulacion);

public sealed record VentaDetalleDto(
    long IdVentaDetalle, long IdVenta, int NumLinea, long IdPresProd,
    long? IdMovInv, decimal Cantidad, decimal PrecioUnitario,
    decimal CostoUnitario, decimal Descuento, decimal Subtotal);

public sealed record VentaPagoDto(long IdVentaPago, long IdVenta, int IdMetodoPago, decimal Monto, string? Referencia);

public sealed record OrdenCompraDto(
    long IdOrdenCompra, int IdTenant, int IdProveedor, string NumOrden,
    DateTime FecOrden, DateTime FecEntrega, byte Estado, decimal Total, string? Observaciones);

public sealed record OrdenCompraDetalleDto(
    long IdOCDetalle, long IdOrdenCompra, long IdPresProd, decimal Cantidad,
    decimal PrecioUnitario, decimal Subtotal);
