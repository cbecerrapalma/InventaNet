using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IAtriDefServiceAsync
{
    Task<Result<IReadOnlyList<AtriDef>>> GetByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<AtriDef?>> GetByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default);
    Task<Result<AtriDef?>> GetWithValoresAsync(long idAtriDef, CancellationToken ct = default);
    Task<Result<IReadOnlyList<AtriDef>>> GetObligatoriosByCategoriaAsync(long idCategoria, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default);
    Task<Result<bool>> TieneValoresAsync(long idAtriDef, CancellationToken ct = default);
    Task<Result<int>> CountByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default);
}

public sealed class AtriDefServiceAsync : IAtriDefServiceAsync
{
    private readonly IAtriDefRepositoryAsync _repo;
    public AtriDefServiceAsync(IAtriDefRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<AtriDef>>> GetByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByCategoriaAsync(idCategoria, soloActivos, ct);
        return Result<IReadOnlyList<AtriDef>>.Success(list);
    }

    public async Task<Result<AtriDef?>> GetByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idCategoria, codigo, ct);
        return Result<AtriDef?>.Success(e);
    }

    public async Task<Result<AtriDef?>> GetWithValoresAsync(long idAtriDef, CancellationToken ct = default)
    {
        var e = await _repo.GetWithValoresAsync(idAtriDef, ct);
        return Result<AtriDef?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<AtriDef>>> GetObligatoriosByCategoriaAsync(long idCategoria, CancellationToken ct = default)
    {
        var list = await _repo.GetObligatoriosByCategoriaAsync(idCategoria, ct);
        return Result<IReadOnlyList<AtriDef>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(long idCategoria, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idCategoria, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> TieneValoresAsync(long idAtriDef, CancellationToken ct = default)
    {
        var ok = await _repo.TieneValoresAsync(idAtriDef, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByCategoriaAsync(long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByCategoriaAsync(idCategoria, soloActivos, ct);
        return Result<int>.Success(c);
    }
}
