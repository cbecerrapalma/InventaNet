using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S57.34: Consultas de Tenant para backend (lectura).
/// La sincronización vive en ITenantSyncServiceAsync. Sin DbContext.
/// </summary>
public interface ITenantServiceAsync
{
    Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(CancellationToken ct = default);
    Task<Result<Tenant?>> GetByIdAsync(int idTenant, CancellationToken ct = default);
    Task<Result<Tenant?>> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Tenant>>> GetActivosAsync(CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<Result<int>> CountActivosAsync(CancellationToken ct = default);
}

public sealed class TenantServiceAsync : ITenantServiceAsync
{
    private readonly ITenantRepositoryAsync _tenantRepo;

    public TenantServiceAsync(ITenantRepositoryAsync tenantRepo) => _tenantRepo = tenantRepo;

    public async Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(CancellationToken ct = default)
    {
        var list = await _tenantRepo.GetAllAsync(ct);
        return Result<IReadOnlyList<Tenant>>.Success(list);
    }

    public async Task<Result<Tenant?>> GetByIdAsync(int idTenant, CancellationToken ct = default)
    {
        var t = await _tenantRepo.GetByIdAsync(idTenant, ct);
        if (t == null)
            return Result<Tenant?>.Failure(new Error("NOT_FOUND", "Tenant no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Tenant?>.Success(t);
    }

    public async Task<Result<Tenant?>> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var t = await _tenantRepo.GetByCodigoAsync(codigo, ct);
        return Result<Tenant?>.Success(t);
    }

    public async Task<Result<IReadOnlyList<Tenant>>> GetActivosAsync(CancellationToken ct = default)
    {
        var list = await _tenantRepo.GetActivosAsync(ct);
        return Result<IReadOnlyList<Tenant>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        var ok = await _tenantRepo.ExistsByCodigoAsync(codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountActivosAsync(CancellationToken ct = default)
    {
        var c = await _tenantRepo.CountActivosAsync(ct);
        return Result<int>.Success(c);
    }
}
