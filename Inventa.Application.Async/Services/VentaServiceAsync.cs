using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Models;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IVentaServiceAsync
{
    Task<Result<RegistrarVentaOutput>> RegistrarVentaAsync(RegistrarVentaInput req, CancellationToken ct = default);
    Task<Result> AnularVentaAsync(AnularVentaInput req, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Venta>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<PagedResult<VentaListadoRow>>> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<VentaFicha?>> GetFichaAsync(long idVenta, int idTenant, CancellationToken ct = default);
    // S57.36: paridad custom Sync VentaService (delegación 1:1 a IVentaRepositoryAsync)
    Task<Result<IReadOnlyList<Venta>>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default);
    Task<Result<Venta?>> GetByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default);
    Task<Result<bool>> ExistsByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default);
    Task<Result<decimal>> GetTotalVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default);
    Task<Result<int>> CountVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ResumenVentasPorTipoDoc>>> GetResumenPorTipoDocAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Venta>>> GetAnuladasAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Venta>>> GetByClienteAsync(int idTenant, int idCliente, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Venta>>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total)>> GetSummaryAsync(int idTenant, CancellationToken ct = default);
}

public sealed class VentaServiceAsync : IVentaServiceAsync
{
    private readonly IVentaRepositoryAsync _ventaRepo;
    private readonly IBodegaRepositoryAsync _bodegaRepo;
    private readonly IClienteRepositoryAsync _clienteRepo;
    private readonly IPresProdRepositoryAsync _presProdRepo;
    private readonly IMetodoPagoRepositoryAsync _metodoPagoRepo;
    private readonly ITenantConfigInventaRepositoryAsync _tenantCfgRepo;
    private readonly IPrecioReferenceResolverAsync _precioResolver;

    public VentaServiceAsync(
        IVentaRepositoryAsync ventaRepo,
        IBodegaRepositoryAsync bodegaRepo,
        IClienteRepositoryAsync clienteRepo,
        IPresProdRepositoryAsync presProdRepo,
        IMetodoPagoRepositoryAsync metodoPagoRepo,
        ITenantConfigInventaRepositoryAsync tenantCfgRepo,
        IPrecioReferenceResolverAsync precioResolver)
    {
        _ventaRepo = ventaRepo;
        _bodegaRepo = bodegaRepo;
        _clienteRepo = clienteRepo;
        _presProdRepo = presProdRepo;
        _metodoPagoRepo = metodoPagoRepo;
        _tenantCfgRepo = tenantCfgRepo;
        _precioResolver = precioResolver;
    }

    public async Task<Result<RegistrarVentaOutput>> RegistrarVentaAsync(RegistrarVentaInput req, CancellationToken ct = default)
    {
        // S55.2 governance: enforce PermiteVentaSinCliente
        if (req.IdCliente == null)
        {
            var cfg = await _tenantCfgRepo.GetByTenantAsync(req.IdTenant, ct);
            if (cfg != null && !cfg.PermiteVentaSinCliente)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "Este tenant requiere cliente en la venta (PermiteVentaSinCliente = false)").WithType(ErrorType.Validation).WithHttpStatus(422));
        }
        // --- Basic field validation ---
        if (string.IsNullOrWhiteSpace(req.NumVenta))
            return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "NumVenta es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.NumVenta.Length > 50)
            return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "NumVenta excede longitud máxima").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.TipoDoc is < 1 or > 3)
            return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "TipoDoc inválido (1-3)").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.IdBodega <= 0)
            return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "IdBodega debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.Items == null || req.Items.Count == 0)
            return Result<RegistrarVentaOutput>.Failure(new Error("VENTA_SIN_ITEMS", "La venta debe contener al menos un ítem.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.Pagos == null || req.Pagos.Count == 0)
            return Result<RegistrarVentaOutput>.Failure(new Error("VENTA_SIN_PAGOS", "La venta debe contener al menos un método de pago.").WithType(ErrorType.Validation).WithHttpStatus(400));

        // Bodega tenant chain
        var bodega = await _bodegaRepo.GetByIdAsync(req.IdBodega, ct);
        if (bodega == null)
            return Result<RegistrarVentaOutput>.Failure(new Error("BODEGA_NOT_FOUND", "Bodega no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (!bodega.Activo)
            return Result<RegistrarVentaOutput>.Failure(new Error("BODEGA_INACTIVA", "La bodega no está activa.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (bodega.IdTenant != req.IdTenant)
            return Result<RegistrarVentaOutput>.Failure(new Error("BODEGA_TENANT_MISMATCH", "Bodega no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));

        // Cliente tenant validation
        if (req.IdCliente != null)
        {
            var cli = await _clienteRepo.GetByIdAsync(req.IdCliente.Value, ct);
            if (cli == null)
                return Result<RegistrarVentaOutput>.Failure(new Error("CLIENTE_NOT_FOUND", "Cliente no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
            if (cli.IdTenant != req.IdTenant)
                return Result<RegistrarVentaOutput>.Failure(new Error("CLIENTE_TENANT_MISMATCH", "Cliente no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
            if (!cli.Activo)
                return Result<RegistrarVentaOutput>.Failure(new Error("CLIENTE_INACTIVO", "Cliente no está activo").WithType(ErrorType.Validation).WithHttpStatus(400));
        }

        // Items validation + duplicate + PresProd tenant + Descuento bounds
        if (req.Items.Select(i => i.IdPresProd).Distinct().Count() != req.Items.Count)
            return Result<RegistrarVentaOutput>.Failure(new Error("DUPLICATE_ITEM", "No se permiten IdPresProd duplicados en la misma venta.").WithType(ErrorType.Validation).WithHttpStatus(400));
        foreach (var it in req.Items)
        {
            if (it.IdPresProd <= 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "IdPresProd debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.Cantidad <= 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "Cantidad debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.Descuento < 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "Descuento no puede ser negativo").WithType(ErrorType.Validation).WithHttpStatus(400));
            // PresProd existence + tenant chain (vía VarProd→Prod)
            var presTenant = await _presProdRepo.GetTenantIdAsync(it.IdPresProd, ct);
            if (presTenant == null)
                return Result<RegistrarVentaOutput>.Failure(new Error("PRES_PROD_NOT_FOUND", $"Presentación {it.IdPresProd} no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
            if (presTenant.Value != req.IdTenant)
                return Result<RegistrarVentaOutput>.Failure(new Error("PRES_PROD_TENANT_MISMATCH", "Presentación no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
        }

        // S126 (S93 D8): el precio lo decide el servidor (R3, Precios-Modelo.md).
        var precios = await _precioResolver.ResolveManyAsync(req.IdTenant, req.Items.Select(i => i.IdPresProd), null, ct);
        if (!precios.IsSuccess) return Result<RegistrarVentaOutput>.Failure(precios.Error!);
        var items = new List<VentaItemInput>(req.Items.Count);
        foreach (var it in req.Items)
        {
            if (!precios.Value.TryGetValue(it.IdPresProd, out var res) || res.Efectivo is not decimal efectivo || efectivo <= 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("SIN_PRECIO_VIGENTE", $"La presentación {it.IdPresProd} no tiene precio vigente.").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.Descuento > it.Cantidad * efectivo)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "Descuento no puede exceder subtotal de la línea").WithType(ErrorType.Validation).WithHttpStatus(400));
            var linea = it with { PrecioUnitario = efectivo };
            items.Add(linea);
        }
        req = req with { Items = items };

        // Pagos validation + MetodoPago existence
        foreach (var pg in req.Pagos)
        {
            if (pg.IdMetodoPago == 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "IdMetodoPago inválido").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (pg.Monto <= 0)
                return Result<RegistrarVentaOutput>.Failure(new Error("VALIDATION", "Monto de pago debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            var mpExists = await _metodoPagoRepo.ExistsAsync(pg.IdMetodoPago, ct);
            if (!mpExists)
                return Result<RegistrarVentaOutput>.Failure(new Error("METODO_PAGO_NOT_FOUND", $"Método de pago {pg.IdMetodoPago} no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        }

        // S127 (P23): los pagos suman exactamente el total, al peso (la cantidad admite fracciones); el vuelto no se guarda.
        var totalVenta = req.Items.Sum(i => i.Cantidad * i.PrecioUnitario - i.Descuento);
        if (Math.Round(req.Pagos.Sum(p => p.Monto), 0, MidpointRounding.AwayFromZero) != Math.Round(totalVenta, 0, MidpointRounding.AwayFromZero))
            return Result<RegistrarVentaOutput>.Failure(new Error("PAGOS_DISTINTOS_DEL_TOTAL", "La suma de los pagos debe ser igual al total de la venta.").WithType(ErrorType.Validation).WithHttpStatus(400));

        var r = await _ventaRepo.RegistrarVentaAsync(SpInputMapper.ToCmd(req), ct);
        if (!r.IsSuccess) return Result<RegistrarVentaOutput>.Failure(r.Error!);
        return Result<RegistrarVentaOutput>.Success(SpInputMapper.FromCmd(r.Value));
    }

    public async Task<Result> AnularVentaAsync(AnularVentaInput req, CancellationToken ct = default)
    {
        if (req.IdVenta <= 0)
            return Result.Failure(new Error("VALIDATION", "IdVenta debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (string.IsNullOrWhiteSpace(req.MotivoAnulacion))
            return Result.Failure(new Error("MOTIVO_REQUERIDO", "Debe indicar el motivo de anulación.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.MotivoAnulacion.Trim().Length == 0)
            return Result.Failure(new Error("MOTIVO_REQUERIDO", "Debe indicar el motivo de anulación.").WithType(ErrorType.Validation).WithHttpStatus(400));
        return await _ventaRepo.AnularVentaAsync(SpInputMapper.ToCmd(req), ct);
    }

    public async Task<Result<IReadOnlyList<Venta>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<Venta>>.Success(list);
    }

    public async Task<Result<PagedResult<VentaListadoRow>>> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch { 20 => 20, 50 => 50, 100 => 100, _ => Math.Clamp(pageSize, 1, 100) };
        if (pageSize is not (20 or 50 or 100)) pageSize = 20;
        q = q?.Trim(); if (q != null && q.Length > 100) q = q[..100];
        var (items, total) = await _ventaRepo.SearchPagedAsync(idTenant, q, page, pageSize, ct);
        return Result<PagedResult<VentaListadoRow>>.Success(PagedResult<VentaListadoRow>.Create(items, page, pageSize, total));
    }

    public async Task<Result<VentaFicha?>> GetFichaAsync(long idVenta, int idTenant, CancellationToken ct = default)
    {
        var v = await _ventaRepo.GetFichaAsync(idVenta, idTenant, ct);
        if (v == null)
            return Result<VentaFicha?>.Failure(new Error("VENTA_NO_ENCONTRADA", "Venta no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<VentaFicha?>.Success(v);
    }

    public async Task<Result<IReadOnlyList<Venta>>> GetByClienteAsync(int idTenant, int idCliente, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetByClienteAsync(idTenant, idCliente, ct);
        return Result<IReadOnlyList<Venta>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Venta>>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetByBodegaAsync(idTenant, idBodega, desde, hasta, ct);
        return Result<IReadOnlyList<Venta>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Venta>>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetByRangoFechaAsync(idTenant, desde, hasta, estado, ct);
        return Result<IReadOnlyList<Venta>>.Success(list);
    }

    public async Task<Result<Venta?>> GetByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default)
    {
        var v = await _ventaRepo.GetByNumVentaAsync(idTenant, numVenta, ct);
        return Result<Venta?>.Success(v);
    }

    public async Task<Result<bool>> ExistsByNumVentaAsync(int idTenant, string numVenta, CancellationToken ct = default)
    {
        var b = await _ventaRepo.ExistsByNumVentaAsync(idTenant, numVenta, ct);
        return Result<bool>.Success(b);
    }

    public async Task<Result<decimal>> GetTotalVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default)
    {
        var t = await _ventaRepo.GetTotalVentasAsync(idTenant, desde, hasta, estado, ct);
        return Result<decimal>.Success(t);
    }

    public async Task<Result<int>> CountVentasAsync(int idTenant, DateTime desde, DateTime hasta, byte? estado, CancellationToken ct = default)
    {
        var c = await _ventaRepo.CountVentasAsync(idTenant, desde, hasta, estado, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<ResumenVentasPorTipoDoc>>> GetResumenPorTipoDocAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetResumenPorTipoDocAsync(idTenant, desde, hasta, ct);
        return Result<IReadOnlyList<ResumenVentasPorTipoDoc>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Venta>>> GetAnuladasAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _ventaRepo.GetAnuladasAsync(idTenant, desde, hasta, ct);
        return Result<IReadOnlyList<Venta>>.Success(list);
    }

    public async Task<Result<(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total)>> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        var summary = await _ventaRepo.GetSummaryAsync(idTenant, ct);
        return Result<(int Registros, int Completadas, int Anuladas, decimal SubTotal, decimal Descuento, decimal Impuesto, decimal Total)>.Success(summary);
    }
}
