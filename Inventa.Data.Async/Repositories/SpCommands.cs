using Inventa.Data.Repositories;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S57.34: comandos SP propios de Data.Async. La WebAPI y Application.Async
/// nunca referencian Inventa.Data.Repositories.SpContracts. El mapeo vive aquí,
/// único punto que conoce ambos mundos. Inventa.Data no se modifica.
/// </summary>
public sealed record VentaItemCmd(long IdPresProd, long? IdLote, decimal Cantidad, decimal PrecioUnitario, decimal Descuento = 0m);
public sealed record VentaPagoCmd(byte IdMetodoPago, decimal Monto, string? Referencia = null);
public sealed record RegistrarVentaCmd(int IdTenant, int IdBodega, string NumVenta, int? IdCliente, byte TipoDoc, int? IdUsuario, IReadOnlyList<VentaItemCmd> Items, IReadOnlyList<VentaPagoCmd> Pagos);
public sealed record RegistrarVentaCmdResult(long IdVenta, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total);
public sealed record AnularVentaCmd(int IdTenant, long IdVenta, string MotivoAnulacion, int? IdUsuario = null);
public sealed record OCRecepcionItemCmd(long IdOCDetalle, long? IdLote, decimal CantRecibida, decimal CostoUnitario);
public sealed record RecibirOrdenCompraCmd(int IdTenant, int IdBodega, long IdOrdenCompra, IReadOnlyList<OCRecepcionItemCmd> Items, int? IdUsuario = null);
public sealed record AjusteItemCmd(long IdPresProd, long? IdLote, decimal CantAjuste, decimal CostoUnitario = 0m);
public sealed record AjustarInventarioCmd(int IdTenant, int IdBodega, IReadOnlyList<AjusteItemCmd> Items, int? IdUsuario = null);
public sealed record TransferenciaItemCmd(long IdPresProd, long? IdLote, decimal Cantidad);
public sealed record TransferirEntreBodegasCmd(int IdTenant, int IdBodegaOrig, int IdBodegaDest, IReadOnlyList<TransferenciaItemCmd> Items, int? IdUsuario = null);

internal static class SpCommandMapper
{
    internal static RegistrarVentaRequest ToData(RegistrarVentaCmd c)
        => new(c.IdTenant, c.IdBodega, c.NumVenta, c.IdCliente, c.TipoDoc, c.IdUsuario,
            c.Items.Select(i => new VentaItemSp(i.IdPresProd, i.IdLote, i.Cantidad, i.PrecioUnitario, i.Descuento)).ToList(),
            c.Pagos.Select(p => new VentaPagoSp(p.IdMetodoPago, p.Monto, p.Referencia)).ToList());

    internal static RegistrarVentaCmdResult FromData(RegistrarVentaResult r)
        => new(r.IdVenta, r.SubTotal, r.Descuento, r.Impuesto, r.Total);

    internal static AnularVentaRequest ToData(AnularVentaCmd c)
        => new(c.IdTenant, c.IdVenta, c.MotivoAnulacion, c.IdUsuario);

    internal static RecibirOrdenCompraRequest ToData(RecibirOrdenCompraCmd c)
        => new(c.IdTenant, c.IdBodega, c.IdOrdenCompra,
            c.Items.Select(i => new OCRecepcionItemSp(i.IdOCDetalle, i.IdLote, i.CantRecibida, i.CostoUnitario)).ToList(),
            c.IdUsuario);

    internal static AjustarInventarioRequest ToData(AjustarInventarioCmd c)
        => new(c.IdTenant, c.IdBodega,
            c.Items.Select(i => new AjusteItemSp(i.IdPresProd, i.IdLote, i.CantAjuste, i.CostoUnitario)).ToList(),
            c.IdUsuario);

    internal static TransferirEntresBodegasRequest ToData(TransferirEntreBodegasCmd c)
        => new(c.IdTenant, c.IdBodegaOrig, c.IdBodegaDest,
            c.Items.Select(i => new TransferenciaItemSp(i.IdPresProd, i.IdLote, i.Cantidad)).ToList(),
            c.IdUsuario);
}
