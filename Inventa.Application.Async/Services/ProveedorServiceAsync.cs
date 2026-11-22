using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IProveedorServiceAsync
{
    Task<Result<IReadOnlyList<Proveedor>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Proveedor?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<Proveedor?>> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Proveedor>>> SearchByRazonSocialAsync(int idTenant, string texto, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProveedorConOCPendiente>>> GetConOrdenesPendientesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<Proveedor?>> GetByIdAsync(int idProveedor, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Proveedor>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Proveedor> Items, int Total)>> SearchPagedAsync(int idTenant, string texto, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Proveedor?>> GetByIdInTenantAsync(int idProveedor, int idTenant, CancellationToken ct = default);
    Task<Result<(Proveedor? Proveedor, bool Duplicado)>> CreateAsync(Proveedor entity, CancellationToken ct = default);
    Task<Result<(Proveedor? Proveedor, bool Duplicado)>> UpdateAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default);
}

public sealed class ProveedorServiceAsync : IProveedorServiceAsync
{
    private readonly IProveedorRepositoryAsync _repo;
    public ProveedorServiceAsync(IProveedorRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Proveedor>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivos, ct);
        return Result<IReadOnlyList<Proveedor>>.Success(list);
    }

    public async Task<Result<Proveedor?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Proveedor?>.Success(e);
    }

    public async Task<Result<Proveedor?>> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        var e = await _repo.GetByRUTAsync(idTenant, rut, ct);
        return Result<Proveedor?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Proveedor>>> SearchByRazonSocialAsync(int idTenant, string texto, CancellationToken ct = default)
    {
        var list = await _repo.SearchByRazonSocialAsync(idTenant, texto, ct);
        return Result<IReadOnlyList<Proveedor>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByRUTAsync(idTenant, rut, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, soloActivos, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<ProveedorConOCPendiente>>> GetConOrdenesPendientesAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetConOrdenesPendientesAsync(idTenant, ct);
        return Result<IReadOnlyList<ProveedorConOCPendiente>>.Success(list);
    }

    public async Task<Result<Proveedor?>> GetByIdAsync(int idProveedor, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idProveedor, ct);
        return Result<Proveedor?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Proveedor>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<Proveedor>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Proveedor> Items, int Total)>> SearchPagedAsync(int idTenant, string texto, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, texto, null, page, pageSize, ct);
        return Result<(IReadOnlyList<Proveedor> Items, int Total)>.Success(result);
    }

    public async Task<Result<Proveedor?>> GetByIdInTenantAsync(int idProveedor, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idProveedor, idTenant, ct);
        return Result<Proveedor?>.Success(e);
    }

    public async Task<Result<(Proveedor? Proveedor, bool Duplicado)>> CreateAsync(Proveedor entity, CancellationToken ct = default)
    {
        var result = await _repo.CreateAsync(entity, ct);
        return Result<(Proveedor? Proveedor, bool Duplicado)>.Success(result);
    }

    public async Task<Result<(Proveedor? Proveedor, bool Duplicado)>> UpdateAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default)
    {
        var result = await _repo.UpdateAsync(idProveedor, idTenant, codigo, razonSocial, ct);
        return Result<(Proveedor? Proveedor, bool Duplicado)>.Success(result);
    }
}
