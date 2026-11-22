namespace Inventa.UC.Producto.DTOs;

/// <summary>
/// DTO para visualización de precios de venta en grilla con historial.
/// </summary>
public record PrecioVentaItem(
    long IdPrecio,
    long IdPresProd,
    decimal Valor,
    DateTime FechaVigencia,
    DateTime? FechaVigenciaHasta,
    string Tipo,
    bool Vigente,
    bool Activo
);
