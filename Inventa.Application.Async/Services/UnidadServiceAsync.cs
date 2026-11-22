using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IUnidadServiceAsync
{
    Task<Result<IReadOnlyList<Unidad>>> GetAllOrdenadoAsync(CancellationToken ct = default);
    Task<Result<Unidad?>> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<bool>> IsUsedByProductosAsync(int idUnidad, CancellationToken ct = default);
    Task<Result<Unidad?>> GetByIdAsync(int idUnidad, CancellationToken ct = default);
    Task<Result<IReadOnlyDictionary<int, string>>> GetMapAsync(CancellationToken ct = default);
}

public sealed class UnidadServiceAsync : IUnidadServiceAsync
{
    private readonly IUnidadRepositoryAsync _repo;
    public UnidadServiceAsync(IUnidadRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Unidad>>> GetAllOrdenadoAsync(CancellationToken ct = default)
    {
        var list = await _repo.GetAllOrdenadoAsync(ct);
        return Result<IReadOnlyList<Unidad>>.Success(list);
    }

    public async Task<Result<Unidad?>> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(codigo, ct);
        return Result<Unidad?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> IsUsedByProductosAsync(int idUnidad, CancellationToken ct = default)
    {
        var ok = await _repo.IsUsedByProductosAsync(idUnidad, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<Unidad?>> GetByIdAsync(int idUnidad, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idUnidad, ct);
        return Result<Unidad?>.Success(e);
    }

    public async Task<Result<IReadOnlyDictionary<int, string>>> GetMapAsync(CancellationToken ct = default)
    {
        var map = await _repo.GetMapAsync(ct);
        return Result<IReadOnlyDictionary<int, string>>.Success(map);
    }
}
