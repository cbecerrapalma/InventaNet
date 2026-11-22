using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IOrdenCompraDetalleServiceAsync
{
    Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetByOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<Result<OrdenCompraDetalle?>> GetByOrdenYPresentacionAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetPendientesByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<decimal>> GetCantidadPendienteAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<decimal>> GetSubtotalOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<Result<int>> CountLineasByOrdenAsync(long idOrdenCompra, CancellationToken ct = default);
    Task<Result<bool>> ExistsInOrdenAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default);
}

public sealed class OrdenCompraDetalleServiceAsync : IOrdenCompraDetalleServiceAsync
{
    private readonly IOrdenCompraDetalleRepositoryAsync _repo;
    public OrdenCompraDetalleServiceAsync(IOrdenCompraDetalleRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetByOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        var list = await _repo.GetByOrdenAsync(idOrdenCompra, ct);
        return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(list);
    }

    public async Task<Result<OrdenCompraDetalle?>> GetByOrdenYPresentacionAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default)
    {
        var e = await _repo.GetByOrdenYPresentacionAsync(idOrdenCompra, idPresProd, ct);
        return Result<OrdenCompraDetalle?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idPresProd, ct);
        return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<OrdenCompraDetalle>>> GetPendientesByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetPendientesByPresentacionAsync(idPresProd, ct);
        return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(list);
    }

    public async Task<Result<decimal>> GetCantidadPendienteAsync(long idPresProd, CancellationToken ct = default)
    {
        var c = await _repo.GetCantidadPendienteAsync(idPresProd, ct);
        return Result<decimal>.Success(c);
    }

    public async Task<Result<decimal>> GetSubtotalOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        var sum = await _repo.GetSubtotalOrdenAsync(idOrdenCompra, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<int>> CountLineasByOrdenAsync(long idOrdenCompra, CancellationToken ct = default)
    {
        var c = await _repo.CountLineasByOrdenAsync(idOrdenCompra, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<bool>> ExistsInOrdenAsync(long idOrdenCompra, long idPresProd, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsInOrdenAsync(idOrdenCompra, idPresProd, ct);
        return Result<bool>.Success(ok);
    }
}
