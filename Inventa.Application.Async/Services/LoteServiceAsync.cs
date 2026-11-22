using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface ILoteServiceAsync
{
    Task<Result<IReadOnlyList<Lote>>> GetByVarianteAsync(int idTenant, long idVarProd, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetProximosAVencerAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetVencidosAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetPorVencerConStockAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    Task<Result<Lote?>> GetByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetOrdenFEFOAsync(int idTenant, long idVarProd, int idBodega, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetAlertasVencimientoByDiasVenAsync(int idTenant, CancellationToken ct = default);
    Task<Result<(int Total, int Vencidos, int Proximos)>> GetVencimientoSummaryAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    Task<Result<Lote?>> GetByIdAsync(long idLote, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Lote> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
}

public sealed class LoteServiceAsync : ILoteServiceAsync
{
    private readonly ILoteRepositoryAsync _repo;
    public LoteServiceAsync(ILoteRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Lote>>> GetByVarianteAsync(int idTenant, long idVarProd, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteAsync(idTenant, idVarProd, soloActivos, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetProximosAVencerAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        var list = await _repo.GetProximosAVencerAsync(idTenant, diasAlerta, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetVencidosAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetVencidosAsync(idTenant, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetPorVencerConStockAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        var list = await _repo.GetPorVencerConStockAsync(idTenant, diasAlerta, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<Lote?>> GetByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, idVarProd, codigo, ct);
        return Result<Lote?>.Success(e);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, idVarProd, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetOrdenFEFOAsync(int idTenant, long idVarProd, int idBodega, CancellationToken ct = default)
    {
        var list = await _repo.GetOrdenFEFOAsync(idTenant, idVarProd, idBodega, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetAlertasVencimientoByDiasVenAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetAlertasVencimientoByDiasVenAsync(idTenant, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<(int Total, int Vencidos, int Proximos)>> GetVencimientoSummaryAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        var result = await _repo.GetVencimientoSummaryAsync(idTenant, diasAlerta, ct);
        return Result<(int Total, int Vencidos, int Proximos)>.Success(result);
    }

    public async Task<Result<Lote?>> GetByIdAsync(long idLote, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idLote, ct);
        return Result<Lote?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Lote> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, like, page, pageSize, ct);
        return Result<(IReadOnlyList<Lote> Items, int Total)>.Success(result);
    }
}
