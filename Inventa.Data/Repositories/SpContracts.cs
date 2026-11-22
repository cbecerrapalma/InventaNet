namespace Inventa.Data.Repositories;

// =============================================================================
// CONTRATOS PÚBLICOS DE STORED PROCEDURES
// =============================================================================
// Tipos utilizados por IVentaRepository, IOrdenCompraRepository e IStockRepository.
// Son parte de la API pública de Inventa.Data — accesibles desde la capa Application.
// Las listas se aceptan como IReadOnlyList<T> para garantizar inmutabilidad post-construcción.
// =============================================================================

// ── sp_RegistrarVenta ─────────────────────────────────────────────────────────

/// <summary>
/// Ítem de venta para sp_RegistrarVenta.
/// Descuento es el descuento aplicado a esta línea específica (no el total de la venta).
/// El SP calcula @Descuento = SUM(Descuento de todos los ítems).
/// </summary>
public sealed record VentaItemSp(
    long    IdPresProd,
    long?   IdLote,           // null = sin lote (producto que no maneja lotes)
    decimal Cantidad,
    decimal PrecioUnitario,
    decimal Descuento = 0m    // descuento por línea, no total
);

/// <summary>Pago de venta para sp_RegistrarVenta.</summary>
public sealed record VentaPagoSp(
    byte    IdMetodoPago,
    decimal Monto,
    string? Referencia = null
);

/// <summary>Datos de entrada para sp_RegistrarVenta.</summary>
public sealed record RegistrarVentaRequest(
    int    IdTenant,
    int    IdBodega,
    string NumVenta,
    int?   IdCliente,
    /// <summary>1=Boleta, 2=Factura, 3=Nota de Venta</summary>
    byte   TipoDoc,
    int?   IdUsuario,
    IReadOnlyList<VentaItemSp>  Items,
    IReadOnlyList<VentaPagoSp>  Pagos
);

/// <summary>
/// Resultado de sp_RegistrarVenta con el IdVenta generado por SCOPE_IDENTITY()
/// y los totales calculados por SQL Server (SubTotal, Descuento, Impuesto, Total).
/// </summary>
public sealed record RegistrarVentaResult(
    long    IdVenta,
    decimal SubTotal,
    decimal Descuento,
    decimal Impuesto,
    decimal Total
);

// ── sp_AnularVenta ────────────────────────────────────────────────────────────

/// <summary>
/// Datos de entrada para sp_AnularVenta.
/// Solo se pueden anular ventas en estado Completada (1).
/// MotivoAnulacion no puede ser nulo, vacío ni solo espacios (el SP valida NULLIF+TRIM).
/// </summary>
public sealed record AnularVentaRequest(
    int    IdTenant,
    long   IdVenta,
    string MotivoAnulacion,
    int?   IdUsuario = null
);

// ── sp_RecibirOrdenCompra ─────────────────────────────────────────────────────

/// <summary>
/// Línea de recepción para sp_RecibirOrdenCompra.
/// CostoUnitario es el costo REAL de compra al momento de la recepción — puede diferir
/// del PrecioUnitario pactado en OrdenCompraDetalle. El SP usa este valor para
/// recalcular el costo promedio ponderado en Stock y VarProd.CostoProm.
/// </summary>
public sealed record OCRecepcionItemSp(
    long    IdOCDetalle,      // debe pertenecer a la OC indicada en la request
    long?   IdLote,           // null = sin lote
    decimal CantRecibida,
    decimal CostoUnitario     // costo real recibido (≠ precio OC en recepciones con diferencias)
);

/// <summary>
/// Datos de entrada para sp_RecibirOrdenCompra.
/// IdBodega es la bodega DESTINO donde se recibe la mercadería.
/// Si todos los ítems de la OC se incluyen, el SP marca la OC como Recibida (3);
/// si la recepción es parcial, la deja en Aprobada (2).
/// </summary>
public sealed record RecibirOrdenCompraRequest(
    int    IdTenant,
    int    IdBodega,           // bodega DESTINO de la recepción
    long   IdOrdenCompra,
    IReadOnlyList<OCRecepcionItemSp> Items,
    int?   IdUsuario = null
);

// ── sp_AjustarInventario ──────────────────────────────────────────────────────

/// <summary>
/// Ítem de ajuste para sp_AjustarInventario.
/// CantAjuste DEBE ser distinto de cero:
///   · Positivo  → Ajuste Entrada (TipoMov = 3) — ej: mercadería encontrada en conteo físico
///   · Negativo  → Ajuste Salida  (TipoMov = 4) — ej: merma, rotura, diferencia de inventario
/// El SP lanza error 50032 si cualquier ítem tiene CantAjuste = 0.
/// StockRepository.AjustarInventario valida esto antes de llamar al SP.
/// </summary>
public sealed record AjusteItemSp(
    long    IdPresProd,
    long?   IdLote,           // null = sin lote
    decimal CantAjuste,       // ≠ 0; positivo=entrada, negativo=salida
    decimal CostoUnitario = 0m
);

/// <summary>Datos de entrada para sp_AjustarInventario.</summary>
public sealed record AjustarInventarioRequest(
    int    IdTenant,
    int    IdBodega,
    IReadOnlyList<AjusteItemSp> Items,
    int?   IdUsuario = null
);

// ── sp_TransferirEntresBodegas ────────────────────────────────────────────────

/// <summary>
/// Ítem de transferencia para sp_TransferirEntresBodegas.
/// Cantidad debe ser estrictamente positiva (el SP usa el mismo valor
/// para la salida en bodega origen y la entrada en bodega destino).
/// </summary>
public sealed record TransferenciaItemSp(
    long    IdPresProd,
    long?   IdLote,           // null = sin lote
    decimal Cantidad          // > 0 siempre
);

/// <summary>
/// Datos de entrada para sp_TransferirEntresBodegas.
/// IdBodegaOrig e IdBodegaDest deben ser distintos y pertenecer al mismo tenant.
/// StockRepository.TransferirEntresBodegas valida esto antes de llamar al SP.
/// </summary>
public sealed record TransferirEntresBodegasRequest(
    int    IdTenant,
    int    IdBodegaOrig,
    int    IdBodegaDest,
    IReadOnlyList<TransferenciaItemSp> Items,
    int?   IdUsuario = null
);
