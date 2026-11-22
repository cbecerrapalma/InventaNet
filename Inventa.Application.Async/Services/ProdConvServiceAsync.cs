using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IProdConvServiceAsync
{
    Task<Result<IReadOnlyList<ProdConv>>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<ProdConv?>> GetAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<Result<decimal?>> GetFactorAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<Result<bool>> ExistsAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProdConv>>> GetByUnidadAsync(int idUnidad, CancellationToken ct = default);
    Task<Result<decimal>> ConvertirCantidadAsync(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad, CancellationToken ct = default);
}

public sealed class ProdConvServiceAsync : IProdConvServiceAsync
{
    private readonly IProdConvRepositoryAsync _repo;
    public ProdConvServiceAsync(IProdConvRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ProdConv>>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteAsync(idVarProd, ct);
        return Result<IReadOnlyList<ProdConv>>.Success(list);
    }

    public async Task<Result<ProdConv?>> GetAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        var e = await _repo.GetAsync(idVarProd, idUnidad, ct);
        return Result<ProdConv?>.Success(e);
    }

    public async Task<Result<decimal?>> GetFactorAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        var f = await _repo.GetFactorAsync(idVarProd, idUnidad, ct);
        return Result<decimal?>.Success(f);
    }

    public async Task<Result<bool>> ExistsAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsAsync(idVarProd, idUnidad, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<ProdConv>>> GetByUnidadAsync(int idUnidad, CancellationToken ct = default)
    {
        var list = await _repo.GetByUnidadAsync(idUnidad, ct);
        return Result<IReadOnlyList<ProdConv>>.Success(list);
    }

    public async Task<Result<decimal>> ConvertirCantidadAsync(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad, CancellationToken ct = default)
    {
        return await _repo.ConvertirCantidadAsync(idVarProd, idUnidadOrigen, idUnidadDestino, cantidad, ct);
    }
}
