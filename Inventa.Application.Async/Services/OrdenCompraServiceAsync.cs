using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IOrdenCompraServiceAsync
{
    Task<Result<IReadOnlyList<OrdenCompra>>> GetByTenantAsync(int idTenant, byte? estado = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompra>>> GetByProveedorAsync(int idTenant, int idProveedor, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompra>>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<OrdenCompra?>> GetByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default);
    Task<Result<OrdenCompra?>> GetWithDetallesAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompra>>> GetPendientesVencimientoAsync(int idTenant, int diasAlerta = 3, CancellationToken ct = default);
    Task<Result<bool>> ExistsByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default);
    Task<Result<IReadOnlyDictionary<byte, decimal>>> GetTotalPorEstadoAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompra>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<OrdenCompraListadoRow> Items, int TotalCount)>> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<OrdenCompraFicha?>> GetFichaAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default);
    Task<Result<OrdenCompra>> CreateWithDetailsAsync(int idTenant, int idProveedor, DateOnly fecha, string? observacion, int idUsuario, string numOrden, IReadOnlyList<(long IdPresProd, decimal Cantidad, decimal PrecioUnitario)> detalles, CancellationToken ct = default);
    Task<Result<(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total)>> GetSummaryAsync(int idTenant, CancellationToken ct = default);
    Task<Result> RecibirOrdenCompraAsync(RecibirOrdenCompraCmd request, CancellationToken ct = default);
}

public sealed class OrdenCompraServiceAsync : IOrdenCompraServiceAsync
{
    private readonly IOrdenCompraRepositoryAsync _repo;
    public OrdenCompraServiceAsync(IOrdenCompraRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<OrdenCompra>>> GetByTenantAsync(int idTenant, byte? estado = null, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, estado, ct);
        return Result<IReadOnlyList<OrdenCompra>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<OrdenCompra>>> GetByProveedorAsync(int idTenant, int idProveedor, CancellationToken ct = default)
    {
        var list = await _repo.GetByProveedorAsync(idTenant, idProveedor, ct);
        return Result<IReadOnlyList<OrdenCompra>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<OrdenCompra>>> GetByRangoFechaAsync(int idTenant, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByRangoFechaAsync(idTenant, desde, hasta, ct);
        return Result<IReadOnlyList<OrdenCompra>>.Success(list);
    }

    public async Task<Result<OrdenCompra?>> GetByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default)
    {
        var e = await _repo.GetByNumOrdenAsync(idTenant, numOrden, ct);
        return Result<OrdenCompra?>.Success(e);
    }

    public async Task<Result<OrdenCompra?>> GetWithDetallesAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        var e = await _repo.GetWithDetallesAsync(idOrdenCompra, ct);
        return Result<OrdenCompra?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<OrdenCompra>>> GetPendientesVencimientoAsync(int idTenant, int diasAlerta = 3, CancellationToken ct = default)
    {
        var list = await _repo.GetPendientesVencimientoAsync(idTenant, diasAlerta, ct);
        return Result<IReadOnlyList<OrdenCompra>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByNumOrdenAsync(int idTenant, string numOrden, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByNumOrdenAsync(idTenant, numOrden, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyDictionary<byte, decimal>>> GetTotalPorEstadoAsync(int idTenant, CancellationToken ct = default)
    {
        var dict = await _repo.GetTotalPorEstadoAsync(idTenant, ct);
        return Result<IReadOnlyDictionary<byte, decimal>>.Success(dict);
    }

    public async Task<Result<IReadOnlyList<OrdenCompra>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<OrdenCompra>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<OrdenCompraListadoRow> Items, int TotalCount)>> SearchPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, q, page, pageSize, ct);
        return Result<(IReadOnlyList<OrdenCompraListadoRow> Items, int TotalCount)>.Success(result);
    }

    public async Task<Result<OrdenCompraFicha?>> GetFichaAsync(long idOrdenCompra, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetFichaAsync(idOrdenCompra, idTenant, ct);
        // S57.46 CLASS-A: null => Failure 404 (antes Success(e) lanzaba => 500 en GET /api/compras/ordenes/{id}).
        if (e == null)
            return Result<OrdenCompraFicha?>.Failure(new Error("OC_NO_ENCONTRADA", "Orden de compra no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<OrdenCompraFicha?>.Success(e);
    }

    public async Task<Result<OrdenCompra>> CreateWithDetailsAsync(int idTenant, int idProveedor, DateOnly fecha, string? observacion, int idUsuario, string numOrden, IReadOnlyList<(long IdPresProd, decimal Cantidad, decimal PrecioUnitario)> detalles, CancellationToken ct = default)
    {
        // S57.45-R1 GAP-3: el DTO async deja NumOrden opcional pero el repo lo exige
        // (excepcion => 500). El path sync exige numero provisto por cliente (NotEmpty,
        // max 20). En async, si se omite se autonumera deterministicamente por tenant
        // (varchar(50), UQ(IdTenant,NumOrden) lo respalda en BD). Valores explicitos
        // se preservan intactos. Formato documentado: OC-{tenant}-{utc:17digitos}.
        // S57.74 Fase 1 (C2/C3): un mismo IdPresProd no puede repetirse dentro de
        // los detalles de la misma OC. Validacion side-effect free ANTES de
        // autonumerar y de llamar al repositorio (cero escrituras parciales).
        var dupPres = detalles.GroupBy(d => d.IdPresProd).FirstOrDefault(g => g.Count() > 1);
        if (dupPres != null)
            return Result<OrdenCompra>.Failure(new Error("OC_DETALLE_DUPLICADO", $"La presentación {dupPres.Key} está duplicada en los detalles de la orden").WithType(ErrorType.Conflict).WithHttpStatus(409));
        if (string.IsNullOrWhiteSpace(numOrden))
            numOrden = $"OC-{idTenant}-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        var oc = await _repo.CreateWithDetailsAsync(idTenant, idProveedor, fecha, observacion, idUsuario, numOrden, detalles, ct);
        return Result<OrdenCompra>.Success(oc);
    }

    public async Task<Result<(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total)>> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        var result = await _repo.GetSummaryAsync(idTenant, ct);
        return Result<(int Registros, int Pendientes, int Recibidas, int Canceladas, decimal Total)>.Success(result);
    }

    public async Task<Result> RecibirOrdenCompraAsync(RecibirOrdenCompraCmd request, CancellationToken ct = default)
    {
        return await _repo.RecibirOrdenCompraAsync(request, ct);
    }
}
