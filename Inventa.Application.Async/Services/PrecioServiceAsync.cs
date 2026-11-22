using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IPrecioServiceAsync
{
    // S57.69: precio normal único (sin discriminator).
    Task<Result<Precio?>> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Precio>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Precio>>> GetHistorialAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<bool>> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<decimal?>> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Precio>>> GetByPresProdOrRecentAsync(long? idPresProd, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Precio> Items, int Total)>> SearchPagedByTenantAsync(int idTenant, long? idPresProd, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Precio>>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default);
    Task<Result<IReadOnlyList<long>>> GetValidPriceProdIdsAsync(int idTenant, DateOnly today, CancellationToken ct = default);
    Task<Result<Precio>> InsertAsync(Precio entity, CancellationToken ct = default);
}

public sealed class PrecioServiceAsync : IPrecioServiceAsync
{
    private readonly IPrecioRepositoryAsync _repo;
    public PrecioServiceAsync(IPrecioRepositoryAsync repo) => _repo = repo;

    public async Task<Result<Precio?>> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var e = await _repo.GetVigenteAsync(idPresProd, fecha, ct);
        return Result<Precio?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Precio>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idPresProd, ct);
        return Result<IReadOnlyList<Precio>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Precio>>> GetHistorialAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetHistorialAsync(idPresProd, ct);
        return Result<IReadOnlyList<Precio>>.Success(list);
    }

    public async Task<Result<bool>> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var ok = await _repo.ExisteVigenteAsync(idPresProd, fecha, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<decimal?>> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var val = await _repo.GetValorVigenteAsync(idPresProd, fecha, ct);
        return Result<decimal?>.Success(val);
    }

    public async Task<Result<IReadOnlyList<Precio>>> GetByPresProdOrRecentAsync(long? idPresProd, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresProdOrRecentAsync(idPresProd, take, ct);
        return Result<IReadOnlyList<Precio>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Precio> Items, int Total)>> SearchPagedByTenantAsync(int idTenant, long? idPresProd, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedByTenantAsync(idTenant, idPresProd, like, page, pageSize, ct);
        return Result<(IReadOnlyList<Precio> Items, int Total)>.Success(result);
    }

    public async Task<Result<IReadOnlyList<Precio>>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default)
    {
        var list = await _repo.GetVigentesAsync(idsPresProd, today, ct);
        return Result<IReadOnlyList<Precio>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<long>>> GetValidPriceProdIdsAsync(int idTenant, DateOnly today, CancellationToken ct = default)
    {
        var list = await _repo.GetValidPriceProdIdsAsync(idTenant, today, ct);
        return Result<IReadOnlyList<long>>.Success(list);
    }

    public async Task<Result<Precio>> InsertAsync(Precio entity, CancellationToken ct = default)
    {
        // S57.69: precio normal único; sin discriminator que validar.
        ArgumentNullException.ThrowIfNull(entity);
        var e = await _repo.InsertAsync(entity, ct);
        return Result<Precio>.Success(e);
    }
}
