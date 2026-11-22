using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Models;

/// <summary>
/// S57.34: inputs de operaciones SP propios de Application.Async.
/// La WebAPI solo conoce estos tipos. El mapeo a SpCommands (Data.Async)
/// vive aquí. Inventa.Data no se referencia.
/// </summary>
public sealed record VentaItemInput(long IdPresProd, long? IdLote, decimal Cantidad, decimal PrecioUnitario, decimal Descuento = 0m);
public sealed record VentaPagoInput(byte IdMetodoPago, decimal Monto, string? Referencia = null);
public sealed record RegistrarVentaInput(int IdTenant, int IdBodega, string NumVenta, int? IdCliente, byte TipoDoc, int? IdUsuario, IReadOnlyList<VentaItemInput> Items, IReadOnlyList<VentaPagoInput> Pagos);
public sealed record RegistrarVentaOutput(long IdVenta, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total);
public sealed record AnularVentaInput(int IdTenant, long IdVenta, string MotivoAnulacion, int? IdUsuario = null);
public sealed record OCRecepcionItemInput(long IdOCDetalle, long? IdLote, decimal CantRecibida, decimal CostoUnitario);
public sealed record RecibirOrdenCompraInput(int IdTenant, int IdBodega, long IdOrdenCompra, IReadOnlyList<OCRecepcionItemInput> Items, int? IdUsuario = null);
public sealed record AjusteItemInput(long IdPresProd, long? IdLote, decimal CantAjuste, decimal CostoUnitario = 0m);
public sealed record AjustarInventarioInput(int IdTenant, int IdBodega, IReadOnlyList<AjusteItemInput> Items, int? IdUsuario = null);
public sealed record TransferenciaItemInput(long IdPresProd, long? IdLote, decimal Cantidad);
public sealed record TransferirEntreBodegasInput(int IdTenant, int IdBodegaOrig, int IdBodegaDest, IReadOnlyList<TransferenciaItemInput> Items, int? IdUsuario = null);

internal static class SpInputMapper
{
    internal static RegistrarVentaCmd ToCmd(RegistrarVentaInput i)
        => new(i.IdTenant, i.IdBodega, i.NumVenta, i.IdCliente, i.TipoDoc, i.IdUsuario,
            i.Items.Select(x => new VentaItemCmd(x.IdPresProd, x.IdLote, x.Cantidad, x.PrecioUnitario, x.Descuento)).ToList(),
            i.Pagos.Select(p => new VentaPagoCmd(p.IdMetodoPago, p.Monto, p.Referencia)).ToList());

    internal static RegistrarVentaOutput FromCmd(RegistrarVentaCmdResult r)
        => new(r.IdVenta, r.SubTotal, r.Descuento, r.Impuesto, r.Total);

    internal static AnularVentaCmd ToCmd(AnularVentaInput i)
        => new(i.IdTenant, i.IdVenta, i.MotivoAnulacion, i.IdUsuario);

    internal static RecibirOrdenCompraCmd ToCmd(RecibirOrdenCompraInput i)
        => new(i.IdTenant, i.IdBodega, i.IdOrdenCompra,
            i.Items.Select(x => new OCRecepcionItemCmd(x.IdOCDetalle, x.IdLote, x.CantRecibida, x.CostoUnitario)).ToList(),
            i.IdUsuario);

    internal static AjustarInventarioCmd ToCmd(AjustarInventarioInput i)
        => new(i.IdTenant, i.IdBodega,
            i.Items.Select(x => new AjusteItemCmd(x.IdPresProd, x.IdLote, x.CantAjuste, x.CostoUnitario)).ToList(),
            i.IdUsuario);

    internal static TransferirEntreBodegasCmd ToCmd(TransferirEntreBodegasInput i)
        => new(i.IdTenant, i.IdBodegaOrig, i.IdBodegaDest,
            i.Items.Select(x => new TransferenciaItemCmd(x.IdPresProd, x.IdLote, x.Cantidad)).ToList(),
            i.IdUsuario);
}
