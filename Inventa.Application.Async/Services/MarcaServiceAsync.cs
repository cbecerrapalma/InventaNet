using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IMarcaServiceAsync
{
    Task<Result<IReadOnlyList<Marca>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<Marca?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Marca>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<Marca?>> GetByIdInTenantAsync(int idMarca, int idTenant, CancellationToken ct = default);
    Task<Result<Marca>> CreateAsync(Marca entity, CancellationToken ct = default);
    Task<Result<Marca?>> UpdateAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<Marca?>> GetByIdAsync(int idMarca, CancellationToken ct = default);
    Task<Result<IReadOnlyList<int>>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default);
}

public sealed class MarcaServiceAsync : IMarcaServiceAsync
{
    private readonly IMarcaRepositoryAsync _repo;
    public MarcaServiceAsync(IMarcaRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Marca>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivas, ct);
        return Result<IReadOnlyList<Marca>>.Success(list);
    }

    public async Task<Result<Marca?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Marca?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Marca>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        var list = await _repo.SearchByNombreAsync(idTenant, nombre, ct);
        return Result<IReadOnlyList<Marca>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, soloActivas, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<Marca?>> GetByIdInTenantAsync(int idMarca, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idMarca, idTenant, ct);
        return Result<Marca?>.Success(e);
    }

    public async Task<Result<Marca>> CreateAsync(Marca entity, CancellationToken ct = default)
    {
        var e = await _repo.CreateAsync(entity, ct);
        return Result<Marca>.Success(e);
    }

    public async Task<Result<Marca?>> UpdateAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var e = await _repo.UpdateAsync(idMarca, idTenant, codigo, nombre, ct);
        return Result<Marca?>.Success(e);
    }

    public async Task<Result<Marca?>> GetByIdAsync(int idMarca, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idMarca, ct);
        return Result<Marca?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<int>>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetInactiveIdsAsync(idTenant, ct);
        return Result<IReadOnlyList<int>>.Success(list);
    }
}
