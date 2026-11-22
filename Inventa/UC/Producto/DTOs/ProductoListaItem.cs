namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de productos en la lista/grilla del formulario.
/// </summary>
public record ProductoListaItem(
    long IdProd,
    string Codigo,
    string Nombre,
    string Categoria,
    string Marca,
    bool Activo,
    int StockTotal,
    int CantidadVariantes
);
