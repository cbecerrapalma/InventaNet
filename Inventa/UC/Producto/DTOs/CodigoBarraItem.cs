namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de códigos de barra en grilla.
/// </summary>
public record CodigoBarraItem(
    long IdCodBarra,
    long IdPresProd,
    string Codigo,
    int Tipo,
    bool EsPrincipal,
    bool Activo
);
