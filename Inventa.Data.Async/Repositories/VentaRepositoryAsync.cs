using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public sealed record ResumenVentasPorTipoDoc(
    byte    TipoDoc,
    int     CantVentas,
    decimal TotalSubTotal,
    decimal TotalDescuento,
    decimal TotalImpuesto,
    decimal TotalFinal);

/// <summary>S116: fila del listado de ventas: los campos de la entidad + nombres de bodega y cliente (contrato ampliado, solo campos nuevos).</summary>
public sealed record VentaListadoRow(
    long IdVenta, int IdTenant, int IdBodega, string NumVenta, DateTime FecVenta, int? IdCliente, byte TipoDoc, int? IdUsuario,
    decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total, byte Estado, DateTime? FecAnulacion, string? MotivoAnulacion, int? UsuarioAnula,
    string BodegaNombre, string? ClienteNombre);

/// <summary>S116: línea de la ficha de venta con producto y variante.</summary>
public sealed record VentaFichaLinea(
    long IdVentaDetalle, long IdVenta, int NumLinea, long IdPresProd, long IdMovInv,
    decimal Cantidad, decimal PrecioUnitario, decimal CostoUnitario, decimal Descuento, decimal Subtotal,
    long IdProd, string ProdCodigo, string ProdNombre, string VarNombre);

/// <summary>S116: pago de la ficha de venta con el nombre del método.</summary>
public sealed record VentaFichaPago(long IdVentaPago, long IdVenta, byte IdMetodoPago, decimal Monto, string? Referencia, string MetodoNombre);

/// <summary>S116: ficha de venta: los campos de la entidad + nombres, líneas y pagos.</summary>
public sealed record VentaFicha(
    long IdVenta, int IdTenant, int IdBodega, string NumVenta, DateTime FecVenta, int? IdCliente, byte TipoDoc, int? IdUsuario,
    decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total, byte Estado, DateTime? FecAnulacion, string? MotivoAnulacion, int? UsuarioAnula,
    string BodegaNombre, string? ClienteNombre, IReadOnlyList<VentaFichaLinea> Detalles, IReadOnlyList<VentaFichaPago> Pagos);

public interface IVentaRepositoryAsync
{
    Task<IReadOnlyList<Venta>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> GetByClienteAsync(int idTenant, int idCliente, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Venta?> GetByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default);
    Task<Venta?> GetWithDetallesAsync(long idVenta, CancellationToken ct = default);
    Task<decimal> GetTotalVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default);
    Task<int> CountVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default);
    Task<bool> ExistsByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default);
    Task<IReadOnlyList<ResumenVentasPorTipoDoc>> GetResumenPorTipoDocAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<Venta>> GetAnuladasAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    // S57.34: lecturas para Application.Async (listado reciente, búsqueda paginada, detalle por tenant).
    Task<IReadOnlyList<Venta>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<VentaListadoRow> Items, int TotalCount)> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<VentaFicha?> GetFichaAsync(long idVenta, int idTenant, CancellationToken ct = default);
    // S57.34: agregados para dashboard (sin contraparte sync).
    Task<(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total)> GetSummaryAsync(int idTenant, CancellationToken ct = default);
    Task<Result<RegistrarVentaCmdResult>> RegistrarVentaAsync(RegistrarVentaCmd request, CancellationToken ct = default);
    Task<Result> AnularVentaAsync(AnularVentaCmd request, CancellationToken ct = default);
}

public class VentaRepositoryAsync : IVentaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    // Estado: 1=Completada, 2=Anulada
    private const byte EstadoVentaMin = 1;
    private const byte EstadoVentaMax = 2;
    private const byte TipoDocMin = 1;
    private const byte TipoDocMax = 3;

    public VentaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Venta>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));
        if (estado.HasValue && (estado.Value < EstadoVentaMin || estado.Value > EstadoVentaMax))
            throw new ArgumentOutOfRangeException(nameof(estado));

        var query = _ctx.Ventas.AsNoTracking()
            .Where(v => v.IdTenant == idTenant
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta);
        if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);

        return await query
            .Include(v => v.Cliente)
            .Include(v => v.Bodega)
            .OrderByDescending(v => v.FecVenta)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Venta>> GetByClienteAsync(int idTenant, int idCliente, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idCliente <= 0) throw new ArgumentOutOfRangeException(nameof(idCliente));

        return await _ctx.Ventas
            .AsNoTracking()
            .Where(v => v.IdTenant == idTenant && v.IdCliente == idCliente)
            .Include(v => v.Bodega)
            .OrderByDescending(v => v.FecVenta)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Venta>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.Ventas
            .AsNoTracking()
            .Where(v => v.IdTenant == idTenant
                     && v.IdBodega == idBodega
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta)
            .Include(v => v.Cliente)
            .OrderByDescending(v => v.FecVenta)
            .ToListAsync(ct);
    }

    public async Task<Venta?> GetByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(numVenta)) throw new ArgumentException("Requerido", nameof(numVenta));

        return await _ctx.Ventas
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Bodega)
            .FirstOrDefaultAsync(v => v.IdTenant == idTenant && v.NumVenta == numVenta, ct);
    }

    public async Task<Venta?> GetWithDetallesAsync(long idVenta, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));

        return await _ctx.Ventas
            .AsNoTracking()
            .Include(v => v.Cliente)
            .Include(v => v.Bodega)
            .Include(v => v.Detalles.OrderBy(d => d.NumLinea))
                .ThenInclude(d => d.PresProd)
                    .ThenInclude(p => p.VarProd)
                        .ThenInclude(vp => vp.Prod)
            .Include(v => v.Detalles)
                .ThenInclude(d => d.PresProd.Unidad)
            .Include(v => v.Pagos)
                .ThenInclude(p => p.MetodoPago)
            .FirstOrDefaultAsync(v => v.IdVenta == idVenta, ct);
    }

    public async Task<decimal> GetTotalVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));
        if (estado.HasValue && (estado.Value < EstadoVentaMin || estado.Value > EstadoVentaMax))
            throw new ArgumentOutOfRangeException(nameof(estado));

        var query = _ctx.Ventas
            .Where(v => v.IdTenant == idTenant
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta);
        if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);

        return await query.SumAsync(v => v.Total, ct);
    }

    public async Task<int> CountVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado = null, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));
        if (estado.HasValue && (estado.Value < EstadoVentaMin || estado.Value > EstadoVentaMax))
            throw new ArgumentOutOfRangeException(nameof(estado));

        var query = _ctx.Ventas
            .Where(v => v.IdTenant == idTenant
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta);
        if (estado.HasValue) query = query.Where(v => v.Estado == estado.Value);

        return await query.CountAsync(ct);
    }

    public async Task<bool> ExistsByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(numVenta)) throw new ArgumentException("Requerido", nameof(numVenta));

        return await _ctx.Ventas
            .AnyAsync(v => v.IdTenant == idTenant && v.NumVenta == numVenta, ct);
    }

    public async Task<IReadOnlyList<ResumenVentasPorTipoDoc>> GetResumenPorTipoDocAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        var resultado = await _ctx.Ventas
            .Where(v => v.IdTenant == idTenant
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta
                     && v.Estado == 1)
            .GroupBy(v => v.TipoDoc)
            .Select(g => new ResumenVentasPorTipoDoc(
                g.Key,
                g.Count(),
                g.Sum(v => v.SubTotal),
                g.Sum(v => v.Descuento),
                g.Sum(v => v.Impuesto),
                g.Sum(v => v.Total)))
            .OrderBy(r => r.TipoDoc)
            .ToListAsync(ct);

        return resultado;
    }

    public async Task<IReadOnlyList<Venta>> GetAnuladasAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (desde > hasta) throw new ArgumentOutOfRangeException(nameof(desde));

        return await _ctx.Ventas
            .AsNoTracking()
            .Where(v => v.IdTenant == idTenant
                     && v.FecVenta >= desde
                     && v.FecVenta <= hasta
                     && v.Estado == 2)
            .Include(v => v.Cliente)
            .Include(v => v.Bodega)
            .OrderByDescending(v => v.FecAnulacion)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Venta>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        take = Math.Clamp(take, 1, 200);

        return await _ctx.Ventas.AsNoTracking()
            .Where(v => v.IdTenant == idTenant)
            .OrderByDescending(v => v.FecVenta)
            .ThenByDescending(v => v.IdVenta)
            .Take(take)
            .ToListAsync(ct);
    }

    // S116 D1: proyección del listado y de la cabecera de la ficha (nombres resueltos en la BD).
    private static readonly System.Linq.Expressions.Expression<Func<Venta, VentaListadoRow>> ProyListado = v => new VentaListadoRow(
        v.IdVenta, v.IdTenant, v.IdBodega, v.NumVenta, v.FecVenta, v.IdCliente, v.TipoDoc, v.IdUsuario,
        v.SubTotal, v.Descuento, v.Impuesto, v.Total, v.Estado, v.FecAnulacion, v.MotivoAnulacion, v.UsuarioAnula,
        v.Bodega.Nombre, v.Cliente != null ? v.Cliente.Nombre : null);

    public async Task<(IReadOnlyList<VentaListadoRow> Items, int TotalCount)> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is not (20 or 50 or 100)) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var query = _ctx.Ventas.AsNoTracking().Where(v => v.IdTenant == idTenant);
        q = q?.Trim();
        if (!string.IsNullOrWhiteSpace(q))
        {
            var like = $"%{q}%";
            // S116 D2: número de venta o nombre del cliente.
            query = query.Where(v => EF.Functions.Like(v.NumVenta, like)
                || (v.Cliente != null && EF.Functions.Like(v.Cliente.Nombre, like)));
        }
        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(v => v.FecVenta)
            .ThenByDescending(v => v.IdVenta)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(ProyListado)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task<VentaFicha?> GetFichaAsync(long idVenta, int idTenant, CancellationToken ct = default)
    {
        if (idVenta <= 0) throw new ArgumentOutOfRangeException(nameof(idVenta));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var h = await _ctx.Ventas.AsNoTracking()
            .Where(v => v.IdVenta == idVenta && v.IdTenant == idTenant)
            .Select(ProyListado)
            .FirstOrDefaultAsync(ct);
        if (h is null) return null;

        var detalles = await _ctx.VentaDetalle.AsNoTracking()
            .Where(d => d.IdVenta == idVenta)
            .OrderBy(d => d.NumLinea)
            .Select(d => new VentaFichaLinea(d.IdVentaDetalle, d.IdVenta, d.NumLinea, d.IdPresProd, d.IdMovInv,
                d.Cantidad, d.PrecioUnitario, d.CostoUnitario, d.Descuento, d.Subtotal,
                d.PresProd.VarProd.IdProd, d.PresProd.VarProd.Prod.Codigo, d.PresProd.VarProd.Prod.Nombre, d.PresProd.VarProd.Nombre))
            .ToListAsync(ct);
        var pagos = await _ctx.VentaPagos.AsNoTracking()
            .Where(p => p.IdVenta == idVenta)
            .OrderBy(p => p.IdVentaPago)
            .Select(p => new VentaFichaPago(p.IdVentaPago, p.IdVenta, p.IdMetodoPago, p.Monto, p.Referencia, p.MetodoPago.Nombre))
            .ToListAsync(ct);

        return new VentaFicha(h.IdVenta, h.IdTenant, h.IdBodega, h.NumVenta, h.FecVenta, h.IdCliente, h.TipoDoc, h.IdUsuario,
            h.SubTotal, h.Descuento, h.Impuesto, h.Total, h.Estado, h.FecAnulacion, h.MotivoAnulacion, h.UsuarioAnula,
            h.BodegaNombre, h.ClienteNombre, detalles, pagos);
    }

    public async Task<(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total)> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Ventas.AsNoTracking().Where(v => v.IdTenant == idTenant);
        var registros = await query.CountAsync(ct);
        if (registros == 0) return (0, 0, 0, 0m, 0m, 0m, 0m);
        var completadas = await query.CountAsync(v => v.Estado == 1, ct);
        var anuladas = await query.CountAsync(v => v.Estado == 2, ct);
        var sub = await query.SumAsync(v => v.SubTotal, ct);
        var desc = await query.SumAsync(v => v.Descuento, ct);
        var imp = await query.SumAsync(v => v.Impuesto, ct);
        var tot = await query.SumAsync(v => v.Total, ct);
        return (registros, completadas, anuladas, sub, desc, imp, tot);
    }

    public async Task<Result<RegistrarVentaCmdResult>> RegistrarVentaAsync(RegistrarVentaCmd request, CancellationToken ct = default)
    {
        if (request.IdTenant <= 0) return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdTenant), 1, int.MaxValue));
        if (request.IdBodega <= 0) return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdBodega), 1, int.MaxValue));
        if (string.IsNullOrWhiteSpace(request.NumVenta)) return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.Required(nameof(request.NumVenta)));
        if (request.TipoDoc < TipoDocMin || request.TipoDoc > TipoDocMax)
            return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.OutOfRange(nameof(request.TipoDoc), TipoDocMin, TipoDocMax));
        if (request.Items is null || request.Items.Count == 0)
            return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.Required(nameof(request.Items)));
        if (request.Pagos is null || request.Pagos.Count == 0)
            return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.Required(nameof(request.Pagos)));

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (item.Cantidad <= 0m)
                return Result<RegistrarVentaCmdResult>.Failure(CommonErrors.Validation.OutOfRange($"Items[{i}].Cantidad", 0.0001m, decimal.MaxValue));
        }

        var r = await SpExecutorAsync.RegistrarVentaAsync(_ctx, SpCommandMapper.ToData(request), ct);
        if (!r.IsSuccess) return Result<RegistrarVentaCmdResult>.Failure(r.Error!);
        return Result<RegistrarVentaCmdResult>.Success(SpCommandMapper.FromData(r.Value));
    }

    public async Task<Result> AnularVentaAsync(AnularVentaCmd request, CancellationToken ct = default)
    {
        if (request.IdTenant <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdTenant), 1, int.MaxValue));
        if (request.IdVenta <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdVenta), 1, long.MaxValue));
        if (string.IsNullOrWhiteSpace(request.MotivoAnulacion)) return Result.Failure(CommonErrors.Validation.Required(nameof(request.MotivoAnulacion)));

        return await SpExecutorAsync.AnularVentaAsync(_ctx, SpCommandMapper.ToData(request), ct);
    }
}



