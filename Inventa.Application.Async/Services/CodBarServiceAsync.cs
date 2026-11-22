using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface ICodBarServiceAsync
{
    Task<Result<CodBar?>> GetByCodigoAsync(string codigo, int idTenant, CancellationToken ct = default);
    Task<Result<CodBar?>> GetByCodigoConPresentacionAsync(string codigo, int idTenant, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CodBar>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<CodBar?>> GetPrincipalByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CodBar>>> GetByTipoAsync(long idPresProd, byte tipoCod, CancellationToken ct = default);
    Task<Result<IReadOnlyList<CodBar>>> GetByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<Result<CodBar?>> GetByCodigoGlobalAsync(string codigo, CancellationToken ct = default);
}

public sealed class CodBarServiceAsync : ICodBarServiceAsync
{
    private readonly ICodBarRepositoryAsync _repo;
    public CodBarServiceAsync(ICodBarRepositoryAsync repo) => _repo = repo;

    public async Task<Result<CodBar?>> GetByCodigoAsync(string codigo, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(codigo, idTenant, ct);
        return Result<CodBar?>.Success(e, allowNull: true);
    }

    public async Task<Result<CodBar?>> GetByCodigoConPresentacionAsync(string codigo, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoConPresentacionAsync(codigo, idTenant, ct);
        return Result<CodBar?>.Success(e, allowNull: true);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<CodBar>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idPresProd, ct);
        return Result<IReadOnlyList<CodBar>>.Success(list);
    }

    public async Task<Result<CodBar?>> GetPrincipalByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var e = await _repo.GetPrincipalByPresentacionAsync(idPresProd, ct);
        return Result<CodBar?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<CodBar>>> GetByTipoAsync(long idPresProd, byte tipoCod, CancellationToken ct = default)
    {
        var list = await _repo.GetByTipoAsync(idPresProd, tipoCod, ct);
        return Result<IReadOnlyList<CodBar>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<CodBar>>> GetByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, ct);
        return Result<IReadOnlyList<CodBar>>.Success(list);
    }

    public async Task<Result<CodBar?>> GetByCodigoGlobalAsync(string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoGlobalAsync(codigo, ct);
        return Result<CodBar?>.Success(e, allowNull: true);
    }

}
