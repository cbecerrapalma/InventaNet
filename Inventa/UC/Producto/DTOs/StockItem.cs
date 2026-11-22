namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización consolidado de stock por bodega, variante y lote.
/// </summary>
public record StockItem(
    long IdStock,
    long IdVarProd,
    string CodigoVariante,
    int IdBodega,
    string NombreBodega,
    string? NumeroLote,
    decimal CantActual,
    decimal CantDisponible,
    decimal CostoProm,
    decimal Valorizado
);
