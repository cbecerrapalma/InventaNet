using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IVentaPagoServiceAsync
{
    Task<Result<IReadOnlyList<VentaPago>>> GetByVentaAsync(long idVenta, CancellationToken ct = default);
    Task<Result<decimal>> GetTotalPagadoAsync(long idVenta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VentaPago>>> GetByMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<decimal>> GetTotalPorMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ResumenPagoVenta>>> GetResumenByVentaAsync(long idVenta, CancellationToken ct = default);
}

public sealed class VentaPagoServiceAsync : IVentaPagoServiceAsync
{
    private readonly IVentaPagoRepositoryAsync _repo;
    public VentaPagoServiceAsync(IVentaPagoRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VentaPago>>> GetByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        var list = await _repo.GetByVentaAsync(idVenta, ct);
        return Result<IReadOnlyList<VentaPago>>.Success(list);
    }

    public async Task<Result<decimal>> GetTotalPagadoAsync(long idVenta, CancellationToken ct = default)
    {
        var sum = await _repo.GetTotalPagadoAsync(idVenta, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<IReadOnlyList<VentaPago>>> GetByMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByMetodoPagoAsync(idTenant, idMetodoPago, desde, hasta, ct);
        return Result<IReadOnlyList<VentaPago>>.Success(list);
    }

    public async Task<Result<decimal>> GetTotalPorMetodoPagoAsync(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var sum = await _repo.GetTotalPorMetodoPagoAsync(idTenant, idMetodoPago, desde, hasta, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<IReadOnlyList<ResumenPagoVenta>>> GetResumenByVentaAsync(long idVenta, CancellationToken ct = default)
    {
        var list = await _repo.GetResumenByVentaAsync(idVenta, ct);
        return Result<IReadOnlyList<ResumenPagoVenta>>.Success(list);
    }
}
