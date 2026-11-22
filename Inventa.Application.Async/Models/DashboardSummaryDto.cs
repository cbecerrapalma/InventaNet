namespace Inventa.Application.Async.Models;

public sealed record DashboardSummaryDto(
    DateTime GeneratedAtUtc,
    ProductosSummaryDto? Productos,
    ClientesSummaryDto? Clientes,
    ProveedoresSummaryDto? Proveedores,
    BodegasSummaryDto? Bodegas,
    StockSummaryDto? Stock,
    LotesSummaryDto? Lotes,
    VentasSummaryDto? Ventas,
    ComprasSummaryDto? Compras);

public sealed record ProductosSummaryDto(int Total, int Activos, int Inactivos);
public sealed record ClientesSummaryDto(int Total, int Activos, int Inactivos);
public sealed record ProveedoresSummaryDto(int Total, int Activos, int Inactivos);
public sealed record BodegasSummaryDto(int Total, int Activas, int Inactivas);
public sealed record StockSummaryDto(int Registros, decimal CantidadActual, int Agotados);
public sealed record LotesSummaryDto(int Total, int Vencidos, int Proximos30Dias);
public sealed record VentasSummaryDto(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total);
public sealed record ComprasSummaryDto(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total);
