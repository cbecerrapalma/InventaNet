namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de conversiones de unidades en la grilla.
/// Representa un factor de conversión entre la unidad de presentación y la unidad base.
/// </summary>
public record ConversionItem(
    long IdProdConv,
    long IdVarProd,
    string NombreUnidad,
    decimal Factor
);
