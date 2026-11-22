using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IAtriValServiceAsync
{
    Task<Result<IReadOnlyList<AtriVal>>> GetByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<AtriVal?>> GetByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default);
    Task<Result<bool>> IsUsedByVariantesAsync(long idAtriVal, CancellationToken ct = default);
    Task<Result<int>> CountByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default);
}

public sealed class AtriValServiceAsync : IAtriValServiceAsync
{
    private readonly IAtriValRepositoryAsync _repo;
    public AtriValServiceAsync(IAtriValRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<AtriVal>>> GetByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByAtriDefAsync(idAtriDef, soloActivos, ct);
        return Result<IReadOnlyList<AtriVal>>.Success(list);
    }

    public async Task<Result<AtriVal?>> GetByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idAtriDef, codigo, ct);
        return Result<AtriVal?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(long idAtriDef, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idAtriDef, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> IsUsedByVariantesAsync(long idAtriVal, CancellationToken ct = default)
    {
        var ok = await _repo.IsUsedByVariantesAsync(idAtriVal, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByAtriDefAsync(long idAtriDef, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByAtriDefAsync(idAtriDef, soloActivos, ct);
        return Result<int>.Success(c);
    }
}
