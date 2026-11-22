using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IMovInvServiceAsync
{
    Task<Result<IReadOnlyList<MovInv>>> GetByPresentacionAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetByTipoMovAsync(int idTenant, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetByLoteAsync(int idTenant, long idLote, CancellationToken ct = default);
    Task<Result<decimal>> GetSumaCantBaseAsync(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetUltimosMovimientosAsync(int idTenant, int idBodega, int cantidad = 50, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetKardexVarianteAsync(int idTenant, long idVarProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<MovInvListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? texto, int page, int pageSize, CancellationToken ct = default);
}

public sealed class MovInvServiceAsync : IMovInvServiceAsync
{
    private readonly IMovInvRepositoryAsync _repo;
    public MovInvServiceAsync(IMovInvRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<MovInv>>> GetByPresentacionAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idTenant, idPresProd, desde, hasta, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByBodegaAsync(idTenant, idBodega, desde, hasta, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetByTipoMovAsync(int idTenant, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetByTipoMovAsync(idTenant, tipoMov, desde, hasta, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetByLoteAsync(int idTenant, long idLote, CancellationToken ct = default)
    {
        var list = await _repo.GetByLoteAsync(idTenant, idLote, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<decimal>> GetSumaCantBaseAsync(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var sum = await _repo.GetSumaCantBaseAsync(idTenant, idPresProd, idBodega, tipoMov, desde, hasta, ct);
        return Result<decimal>.Success(sum);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetUltimosMovimientosAsync(int idTenant, int idBodega, int cantidad = 50, CancellationToken ct = default)
    {
        var list = await _repo.GetUltimosMovimientosAsync(idTenant, idBodega, cantidad, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetKardexVarianteAsync(int idTenant, long idVarProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        var list = await _repo.GetKardexVarianteAsync(idTenant, idVarProd, desde, hasta, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<MovInvListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, idBodega, tipoMov, texto, page, pageSize, ct);
        return Result<(IReadOnlyList<MovInvListadoRow> Items, int Total)>.Success(result);
    }
}
