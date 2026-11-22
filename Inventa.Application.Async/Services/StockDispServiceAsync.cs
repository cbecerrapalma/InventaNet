using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IStockDispServiceAsync
{
    Task<Result<StockDisp?>> GetAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default);
    Task<Result<decimal>> GetCantidadAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default);
    Task<Result<bool>> TieneStockSuficienteAsync(int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockDisp>>> GetByBodegaAsync(int idTenant, int idBodega, bool soloConStock = false, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockDisp>>> GetAgotadosAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockDisp>>> GetConStockNegativoAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<Result<StockConPrecio?>> GetByCodigoBarraPOSAsync(int idTenant, int idBodega, string codigoBarra, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockDisp>>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<StockDispListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default);
}

public sealed class StockDispServiceAsync : IStockDispServiceAsync
{
    private readonly IStockDispRepositoryAsync _repo;
    public StockDispServiceAsync(IStockDispRepositoryAsync repo) => _repo = repo;

    public async Task<Result<StockDisp?>> GetAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default)
    {
        var e = await _repo.GetAsync(idTenant, idPresProd, idBodega, ct);
        return Result<StockDisp?>.Success(e);
    }

    public async Task<Result<decimal>> GetCantidadAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default)
    {
        var c = await _repo.GetCantidadAsync(idTenant, idPresProd, idBodega, ct);
        return Result<decimal>.Success(c);
    }

    public async Task<Result<bool>> TieneStockSuficienteAsync(int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida, CancellationToken ct = default)
    {
        var ok = await _repo.TieneStockSuficienteAsync(idTenant, idPresProd, idBodega, cantidadRequerida, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<StockDisp>>> GetByBodegaAsync(int idTenant, int idBodega, bool soloConStock = false, CancellationToken ct = default)
    {
        var list = await _repo.GetByBodegaAsync(idTenant, idBodega, soloConStock, ct);
        return Result<IReadOnlyList<StockDisp>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<StockDisp>>> GetAgotadosAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        var list = await _repo.GetAgotadosAsync(idTenant, idBodega, ct);
        return Result<IReadOnlyList<StockDisp>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<StockDisp>>> GetConStockNegativoAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        var list = await _repo.GetConStockNegativoAsync(idTenant, idBodega, ct);
        return Result<IReadOnlyList<StockDisp>>.Success(list);
    }

    public async Task<Result<StockConPrecio?>> GetByCodigoBarraPOSAsync(int idTenant, int idBodega, string codigoBarra, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoBarraPOSAsync(idTenant, idBodega, codigoBarra, ct);
        return Result<StockConPrecio?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<StockDisp>>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, idBodega, take, ct);
        return Result<IReadOnlyList<StockDisp>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<StockDispListadoRow> Items, int Total)>> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, idBodega, texto, page, pageSize, ct);
        return Result<(IReadOnlyList<StockDispListadoRow> Items, int Total)>.Success(result);
    }
}
