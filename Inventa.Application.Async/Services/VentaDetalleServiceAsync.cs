using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IVentaDetalleServiceAsync
{
    Task<Result<IReadOnlyList<VentaDetalle>>> GetByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<Result<VentaDetalle?>> GetByVentaYLineaAsync(long idVenta, int numLinea, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VentaDetalle>>> GetByPresentacionAsync(long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<decimal>> GetCantidadVendidaAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<decimal>> GetSubtotalVentaAsync(long idVenta, CancellationToken ct = default);
    Task<Result<int>> CountLineasByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProductoVendidoResumen>>> GetTopProductosVendidosAsync(int idTenant, DateTime desde, DateTime hasta, int top = 10, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MargenBrutoResumen>>> GetMargenBrutoAsync(int idTenant, DateTime desde, DateTime hasta, int top = 20, CancellationToken ct = default);
}

public sealed class VentaDetalleServiceAsync : IVentaDetalleServiceAsync
{
    private readonly IVentaDetalleRepositoryAsync _repo;
    public VentaDetalleServiceAsync(IVentaDetalleRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VentaDetalle>>> GetByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        var list = await _repo.GetByVentaAsync(idVenta, ct);
        return Result<IReadOnlyList<VentaDetalle>>.Success(list);
    }

    public async Task<Result<VentaDetalle?>> GetByVentaYLineaAsync(long idVenta, int numLinea, CancellationToken ct = default)
    {
        var e = await _repo.GetByVentaYLineaAsync(idVenta, numLinea, ct);
        return Result<VentaDetalle?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<VentaDetalle>>> GetByPresentacionAsync(long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idPresProd, desde, hasta, ct);
        return Result<IReadOnlyList<VentaDetalle>>.Success(list);
    }

    public async Task<Result<decimal>> GetCantidadVendidaAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var sum = await _repo.GetCantidadVendidaAsync(idTenant, idPresProd, desde, hasta, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<decimal>> GetSubtotalVentaAsync(long idVenta, CancellationToken ct = default)
    {
        var sum = await _repo.GetSubtotalVentaAsync(idVenta, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<int>> CountLineasByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        var c = await _repo.CountLineasByVentaAsync(idVenta, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<ProductoVendidoResumen>>> GetTopProductosVendidosAsync(int idTenant, DateTime desde, DateTime hasta, int top = 10, CancellationToken ct = default)
    {
        var list = await _repo.GetTopProductosVendidosAsync(idTenant, desde, hasta, top, ct);
        return Result<IReadOnlyList<ProductoVendidoResumen>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MargenBrutoResumen>>> GetMargenBrutoAsync(int idTenant, DateTime desde, DateTime hasta, int top = 20, CancellationToken ct = default)
    {
        var list = await _repo.GetMargenBrutoAsync(idTenant, desde, hasta, top, ct);
        return Result<IReadOnlyList<MargenBrutoResumen>>.Success(list);
    }
}
