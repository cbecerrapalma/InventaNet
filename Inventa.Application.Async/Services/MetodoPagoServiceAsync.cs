using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IMetodoPagoServiceAsync
{
    Task<Result<IReadOnlyList<MetodoPago>>> GetActivosAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<MetodoPago>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<MetodoPago?>> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsAsync(byte idMetodoPago, CancellationToken ct = default);
}

public sealed class MetodoPagoServiceAsync : IMetodoPagoServiceAsync
{
    private readonly IMetodoPagoRepositoryAsync _repo;
    public MetodoPagoServiceAsync(IMetodoPagoRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<MetodoPago>>> GetActivosAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetActivosAsync(ct);
        return Result<IReadOnlyList<MetodoPago>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MetodoPago>>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllAsync(ct);
        return Result<IReadOnlyList<MetodoPago>>.Success(list);
    }

    public async Task<Result<MetodoPago?>> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(codigo, ct);
        return Result<MetodoPago?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> ExistsAsync(byte idMetodoPago, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsAsync(idMetodoPago, ct);
        return Result<bool>.Success(ok);
    }
}
