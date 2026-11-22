namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de variantes en grilla.
/// </summary>
public record VarianteItem(
    long IdVarProd,
    string Codigo,
    string Nombre,
    string Atributos,
    decimal CostoProm,
    bool Activo
);
