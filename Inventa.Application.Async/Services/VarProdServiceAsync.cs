using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IVarProdServiceAsync
{
    Task<Result<IReadOnlyList<VarProd>>> GetByProductoAsync(long idProd, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<VarProd?>> GetByCodigoAsync(long idProd, string codigo, CancellationToken ct = default);
    Task<Result<VarProd?>> GetWithPresentacionesAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(long idProd, string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarProd>>> GetConCostoCargadoAsync(long idProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarProd>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarProd>>> GetConBajoStockAsync(int idTenant, decimal umbralStock = 0m, CancellationToken ct = default);
    Task<Result<VarProd?>> GetByIdAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<long>>> GetSaleProdIdsAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarProd>>> GetAllByProductoAsync(long idProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<VarProd>>> GetActiveByProdAsync(long idProd, CancellationToken ct = default);
}

public sealed class VarProdServiceAsync : IVarProdServiceAsync
{
    private readonly IVarProdRepositoryAsync _repo;
    public VarProdServiceAsync(IVarProdRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<VarProd>>> GetByProductoAsync(long idProd, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByProductoAsync(idProd, soloActivos, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }

    public async Task<Result<VarProd?>> GetByCodigoAsync(long idProd, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idProd, codigo, ct);
        return Result<VarProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<VarProd?>> GetWithPresentacionesAsync(long idVarProd, CancellationToken ct = default)
    {
        var e = await _repo.GetWithPresentacionesAsync(idVarProd, ct);
        return Result<VarProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(long idProd, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idProd, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<VarProd>>> GetConCostoCargadoAsync(long idProd, CancellationToken ct = default)
    {
        var list = await _repo.GetConCostoCargadoAsync(idProd, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<VarProd>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivos, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<VarProd>>> GetConBajoStockAsync(int idTenant, decimal umbralStock = 0m, CancellationToken ct = default)
    {
        var list = await _repo.GetConBajoStockAsync(idTenant, umbralStock, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }

    public async Task<Result<VarProd?>> GetByIdAsync(long idVarProd, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idVarProd, ct);
        return Result<VarProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<long>>> GetSaleProdIdsAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetSaleProdIdsAsync(idTenant, ct);
        return Result<IReadOnlyList<long>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<VarProd>>> GetAllByProductoAsync(long idProd, CancellationToken ct = default)
    {
        var list = await _repo.GetAllByProductoAsync(idProd, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<VarProd>>> GetActiveByProdAsync(long idProd, CancellationToken ct = default)
    {
        var list = await _repo.GetActiveByProdAsync(idProd, ct);
        return Result<IReadOnlyList<VarProd>>.Success(list);
    }
}