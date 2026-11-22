using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IBodegaServiceAsync
{
    Task<Result<IReadOnlyList<Bodega>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<Bodega?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<bool>> IsActivaAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<Result<Bodega?>> GetByIdAsync(int idBodega, CancellationToken ct = default);
    Task<Result<int>> CountActivasAsync(int idTenant, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Bodega>>> GetOrderedByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Bodega> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Bodega?>> GetByIdInTenantAsync(int idBodega, int idTenant, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoExcludingAsync(int idTenant, string codigo, int excludeIdBodega, CancellationToken ct = default);
    Task<Result<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)>> CreateWithQuotaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<(Bodega? Bodega, bool Duplicado)>> UpdateAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default);
}

public sealed class BodegaServiceAsync : IBodegaServiceAsync
{
    private readonly IBodegaRepositoryAsync _repo;
    public BodegaServiceAsync(IBodegaRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Bodega>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivas, ct);
        return Result<IReadOnlyList<Bodega>>.Success(list);
    }

    public async Task<Result<Bodega?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Bodega?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> IsActivaAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        var ok = await _repo.IsActivaAsync(idTenant, idBodega, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<Bodega?>> GetByIdAsync(int idBodega, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idBodega, ct);
        return Result<Bodega?>.Success(e);
    }

    public async Task<Result<int>> CountActivasAsync(int idTenant, CancellationToken ct = default)
    {
        var c = await _repo.CountActivasAsync(idTenant, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<Bodega>>> GetOrderedByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetOrderedByTenantAsync(idTenant, ct);
        return Result<IReadOnlyList<Bodega>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Bodega> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, like, page, pageSize, ct);
        return Result<(IReadOnlyList<Bodega> Items, int Total)>.Success(result);
    }

    public async Task<Result<Bodega?>> GetByIdInTenantAsync(int idBodega, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idBodega, idTenant, ct);
        return Result<Bodega?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoExcludingAsync(int idTenant, string codigo, int excludeIdBodega, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoExcludingAsync(idTenant, codigo, excludeIdBodega, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)>> CreateWithQuotaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var result = await _repo.CreateWithQuotaAsync(idTenant, codigo, nombre, ct);
        return Result<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)>.Success(result);
    }

    public async Task<Result<(Bodega? Bodega, bool Duplicado)>> UpdateAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var result = await _repo.UpdateAsync(idBodega, idTenant, codigo, nombre, ct);
        return Result<(Bodega? Bodega, bool Duplicado)>.Success(result);
    }
}
