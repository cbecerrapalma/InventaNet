using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface ICategoriaServiceAsync
{
    Task<Result<IReadOnlyList<Categoria>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<Categoria?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Categoria>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<int>> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default);
    Task<Result<Categoria?>> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default);
    Task<Result<Categoria>> CreateAsync(Categoria entity, CancellationToken ct = default);
    Task<Result<Categoria?>> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<Categoria?>> GetByIdAsync(long idCategoria, CancellationToken ct = default);
    Task<Result<IReadOnlyList<long>>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default);
}

public sealed class CategoriaServiceAsync : ICategoriaServiceAsync
{
    private readonly ICategoriaRepositoryAsync _repo;
    public CategoriaServiceAsync(ICategoriaRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Categoria>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivas, ct);
        return Result<IReadOnlyList<Categoria>>.Success(list);
    }

    public async Task<Result<Categoria?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Categoria?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Categoria>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        var list = await _repo.SearchByNombreAsync(idTenant, nombre, ct);
        return Result<IReadOnlyList<Categoria>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default)
    {
        var c = await _repo.CountProductosByCategoriaAsync(idCategoria, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<Categoria?>> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idCategoria, idTenant, ct);
        return Result<Categoria?>.Success(e);
    }

    public async Task<Result<Categoria>> CreateAsync(Categoria entity, CancellationToken ct = default)
    {
        var entityResult = await _repo.CreateAsync(entity, ct);
        return Result<Categoria>.Success(entityResult);
    }

    public async Task<Result<Categoria?>> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var e = await _repo.UpdateAsync(idCategoria, idTenant, codigo, nombre, ct);
        return Result<Categoria?>.Success(e);
    }

    public async Task<Result<Categoria?>> GetByIdAsync(long idCategoria, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idCategoria, ct);
        return Result<Categoria?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<long>>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetInactiveIdsAsync(idTenant, ct);
        return Result<IReadOnlyList<long>>.Success(list);
    }
}
