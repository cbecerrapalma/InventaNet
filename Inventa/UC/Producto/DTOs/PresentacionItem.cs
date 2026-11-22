namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de presentaciones en grilla.
/// </summary>
public record PresentacionItem(
    long IdPresProd,
    long IdVarProd,
    string Descripcion,
    string Unidad,
    bool EsVenta,
    bool EsCompra,
    bool Activo
);
