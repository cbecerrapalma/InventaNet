using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IStockServiceAsync
{
    Task<Result<IReadOnlyList<Stock>>> GetByTenantBodegaAsync(int idTenant, int idBodega, bool incluirLote = false, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Stock>>> GetByVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default);
    Task<Result<Stock?>> GetAsync(int idTenant, int idBodega, long idVarProd, long? idLote = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Stock>>> GetConStockNegativoAsync(int idTenant, CancellationToken ct = default);
    Task<Result<decimal?>> GetCantidadTotalAsync(int idTenant, long idVarProd, CancellationToken ct = default);
    Task<Result<decimal?>> GetCantidadEnBodegaAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default);
    Task<Result<decimal?>> GetCostoPromedioAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default);
    Task<Result<(int Registros, decimal Cantidad, int Agotados)>> GetSummaryAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Stock>>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<StockListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default);
    Task<Result> AjustarInventarioAsync(AjustarInventarioCmd request, CancellationToken ct = default);
    Task<Result> TransferirEntresBodegasAsync(TransferirEntreBodegasCmd request, CancellationToken ct = default);
}

public sealed class StockServiceAsync : IStockServiceAsync
{
    private readonly IStockRepositoryAsync _repo;
    public StockServiceAsync(IStockRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Stock>>> GetByTenantBodegaAsync(int idTenant, int idBodega, bool incluirLote = false, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantBodegaAsync(idTenant, idBodega, incluirLote, ct);
        return Result<IReadOnlyList<Stock>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Stock>>> GetByVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteAsync(idTenant, idVarProd, ct);
        return Result<IReadOnlyList<Stock>>.Success(list);
    }

    public async Task<Result<Stock?>> GetAsync(int idTenant, int idBodega, long idVarProd, long? idLote = null, CancellationToken ct = default)
    {
        var e = await _repo.GetAsync(idTenant, idBodega, idVarProd, idLote, ct);
        return Result<Stock?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Stock>>> GetConStockNegativoAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetConStockNegativoAsync(idTenant, ct);
        return Result<IReadOnlyList<Stock>>.Success(list);
    }

    public async Task<Result<decimal?>> GetCantidadTotalAsync(int idTenant, long idVarProd, CancellationToken ct = default)
    {
        var sum = await _repo.GetCantidadTotalAsync(idTenant, idVarProd, ct);
        return Result<decimal?>.Success(sum);
    }

    public async Task<Result<decimal?>> GetCantidadEnBodegaAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default)
    {
        var sum = await _repo.GetCantidadEnBodegaAsync(idTenant, idBodega, idVarProd, ct);
        return Result<decimal?>.Success(sum);
    }

    public async Task<Result<decimal?>> GetCostoPromedioAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default)
    {
        var sum = await _repo.GetCostoPromedioAsync(idTenant, idBodega, idVarProd, ct);
        return Result<decimal?>.Success(sum);
    }

    public async Task<Result<(int Registros, decimal Cantidad, int Agotados)>> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        var result = await _repo.GetSummaryAsync(idTenant, ct);
        return Result<(int Registros, decimal Cantidad, int Agotados)>.Success(result);
    }

    public async Task<Result<IReadOnlyList<Stock>>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, idBodega, take, ct);
        return Result<IReadOnlyList<Stock>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<StockListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, idBodega, texto, page, pageSize, ct);
        return Result<(IReadOnlyList<StockListadoRow> Items, int Total)>.Success(result);
    }

    public async Task<Result> AjustarInventarioAsync(AjustarInventarioCmd request, CancellationToken ct = default)
    {
        return await _repo.AjustarInventarioAsync(request, ct);
    }

    public async Task<Result> TransferirEntresBodegasAsync(TransferirEntreBodegasCmd request, CancellationToken ct = default)
    {
        return await _repo.TransferirEntresBodegasAsync(request, ct);
    }
}
