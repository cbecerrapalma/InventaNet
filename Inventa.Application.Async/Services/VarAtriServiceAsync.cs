using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IVarAtriServiceAsync
{
    Task<Result<IReadOnlyList<VarAtri>>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarAtri>>> GetByAtriValAsync(long idAtriVal, CancellationToken ct = default);
    Task<Result<VarAtri?>> GetAsync(long idVarProd, long idAtriVal, CancellationToken ct = default);
    Task<Result<bool>> ExistsAsync(long idVarProd, long idAtriVal, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarAtri>>> GetByVarianteConDetalleAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<long>>> GetVarianteIdsByAtriValAsync(long idAtriVal, CancellationToken ct = default);
    Task<Result<int>> CountByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<bool>> VarianteCumpleAtributosObligatoriosAsync(long idVarProd, long idCategoria, CancellationToken ct = default);
}

public sealed class VarAtriServiceAsync : IVarAtriServiceAsync
{
    private readonly IVarAtriRepositoryAsync _repo;
    public VarAtriServiceAsync(IVarAtriRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VarAtri>>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteAsync(idVarProd, ct);
        return Result<IReadOnlyList<VarAtri>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<VarAtri>>> GetByAtriValAsync(long idAtriVal, CancellationToken ct = default)
    {
        var list = await _repo.GetByAtriValAsync(idAtriVal, ct);
        return Result<IReadOnlyList<VarAtri>>.Success(list);
    }

    public async Task<Result<VarAtri?>> GetAsync(long idVarProd, long idAtriVal, CancellationToken ct = default)
    {
        var e = await _repo.GetAsync(idVarProd, idAtriVal, ct);
        return Result<VarAtri?>.Success(e);
    }

    public async Task<Result<bool>> ExistsAsync(long idVarProd, long idAtriVal, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsAsync(idVarProd, idAtriVal, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<VarAtri>>> GetByVarianteConDetalleAsync(long idVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteConDetalleAsync(idVarProd, ct);
        return Result<IReadOnlyList<VarAtri>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<long>>> GetVarianteIdsByAtriValAsync(long idAtriVal, CancellationToken ct = default)
    {
        var list = await _repo.GetVarianteIdsByAtriValAsync(idAtriVal, ct);
        return Result<IReadOnlyList<long>>.Success(list);
    }

    public async Task<Result<int>> CountByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        var c = await _repo.CountByVarianteAsync(idVarProd, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<bool>> VarianteCumpleAtributosObligatoriosAsync(long idVarProd, long idCategoria, CancellationToken ct = default)
    {
        var ok = await _repo.VarianteCumpleAtributosObligatoriosAsync(idVarProd, idCategoria, ct);
        return Result<bool>.Success(ok);
    }
}
