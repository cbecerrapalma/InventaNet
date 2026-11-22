using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>S114: fila del listado paginado de stock con producto y variante (Inventario → Stock).</summary>
public sealed record StockListadoRow(
    long IdStock, int IdTenant, int IdBodega, long IdVarProd, long? IdLote,
    decimal CantAct, decimal CostoProm, DateTime FecAct,
    string ProdCodigo, string ProdNombre, string VarCodigo, string VarNombre);

public interface IStockRepositoryAsync
{
    Task<IReadOnlyList<Stock>> GetByTenantBodegaAsync(int idTenant, int idBodega, bool incluirLote = false, CancellationToken ct = default);
    Task<IReadOnlyList<Stock>> GetByVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default);
    Task<Stock?> GetAsync(int idTenant, int idBodega, long idVarProd, long? idLote = null, CancellationToken ct = default);
    Task<IReadOnlyList<Stock>> GetConStockNegativoAsync(int idTenant, CancellationToken ct = default);
    Task<decimal?> GetCantidadTotalAsync(int idTenant, long idVarProd, CancellationToken ct = default);
    Task<decimal?> GetCantidadEnBodegaAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default);
    Task<decimal?> GetCostoPromedioAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default);
    // S57.34: agregados para dashboard en una sola pasada (sin contraparte sync).
    Task<(int Registros, decimal Cantidad, int Agotados)> GetSummaryAsync(int idTenant, CancellationToken ct = default);
    // S57.34: listados para InventarioServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Stock>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<StockListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default);
    Task<Result> AjustarInventarioAsync(AjustarInventarioCmd request, CancellationToken ct = default);
    Task<Result> TransferirEntresBodegasAsync(TransferirEntreBodegasCmd request, CancellationToken ct = default);
}

public class StockRepositoryAsync : IStockRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public StockRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Stock>> GetByTenantBodegaAsync(int idTenant, int idBodega, bool incluirLote = false, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        IQueryable<Stock> q = _ctx.Stock
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega)
            .Include(s => s.VarProd);

        if (incluirLote)
            q = q.Include(s => s.Lote);

        return await q.OrderBy(s => s.VarProd!.Nombre).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Stock>> GetByVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Stock
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.IdVarProd == idVarProd)
            .Include(s => s.Bodega)
            .Include(s => s.Lote)
            .OrderBy(s => s.Bodega!.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Stock?> GetAsync(int idTenant, int idBodega, long idVarProd, long? idLote = null, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Stock
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdTenant == idTenant
                                   && s.IdBodega == idBodega
                                   && s.IdVarProd == idVarProd
                                   && s.IdLote == idLote, ct);
    }

    public async Task<IReadOnlyList<Stock>> GetConStockNegativoAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Stock
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.CantAct < 0)
            .Include(s => s.Bodega)
            .Include(s => s.VarProd)
            .Include(s => s.Lote)
            .OrderBy(s => s.CantAct)
            .ToListAsync(ct);
    }

    public async Task<decimal?> GetCantidadTotalAsync(int idTenant, long idVarProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Stock
            .Where(s => s.IdTenant == idTenant && s.IdVarProd == idVarProd)
            .Select(s => (decimal?)s.CantAct)
            .SumAsync(ct);
    }

    public async Task<decimal?> GetCantidadEnBodegaAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Stock
            .Where(s => s.IdTenant == idTenant
                     && s.IdBodega == idBodega
                     && s.IdVarProd == idVarProd)
            .Select(s => (decimal?)s.CantAct)
            .SumAsync(ct);
    }

    public async Task<decimal?> GetCostoPromedioAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        var registros = await _ctx.Stock
            .Where(s => s.IdTenant == idTenant
                     && s.IdBodega == idBodega
                     && s.IdVarProd == idVarProd
                     && s.CantAct > 0)
            .Select(s => new { s.CantAct, s.CostoProm })
            .ToListAsync(ct);

        if (registros.Count == 0) return null;

        var totalCant = registros.Sum(r => r.CantAct);
        return totalCant == 0
            ? 0m
            : registros.Sum(r => r.CantAct * r.CostoProm) / totalCant;
    }

    public async Task<Result> AjustarInventarioAsync(AjustarInventarioCmd request, CancellationToken ct = default)
    {
        if (request.IdTenant <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdTenant), 1, int.MaxValue));
        if (request.IdBodega <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdBodega), 1, int.MaxValue));
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (item.CantAjuste == 0m)
                return Result.Failure(
                    new Error("AJUSTE_CANTIDAD_CERO", $"La cantidad de ajuste 'Items[{i}].CantAjuste' no puede ser cero.")
                        .WithType(ErrorType.Validation)
                        .WithHttpStatus(400)
                        .WithDetail("Campo", $"Items[{i}].CantAjuste"));
        }

        return await SpExecutor.AjustarInventarioAsync(_ctx, SpCommandMapper.ToData(request), ct);
    }

    public async Task<Result> TransferirEntresBodegasAsync(TransferirEntreBodegasCmd request, CancellationToken ct = default)
    {
        if (request.IdTenant <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdTenant), 1, int.MaxValue));
        if (request.IdBodegaOrig <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdBodegaOrig), 1, int.MaxValue));
        if (request.IdBodegaDest <= 0) return Result.Failure(CommonErrors.Validation.OutOfRange(nameof(request.IdBodegaDest), 1, int.MaxValue));
        if (request.IdBodegaOrig == request.IdBodegaDest)
            return Result.Failure(
                new Error("BODEGA_MISMA_ORIGEN_DEST", "La bodega de origen y destino no pueden ser la misma.")
                    .WithType(ErrorType.Validation).WithHttpStatus(400)
                    .WithDetail("IdBodegaOrig", request.IdBodegaOrig)
                    .WithDetail("IdBodegaDest", request.IdBodegaDest));
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));

        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (item.Cantidad <= 0m)
                return Result.Failure(
                    CommonErrors.Validation.OutOfRange($"Items[{i}].Cantidad", 0.0001m, decimal.MaxValue));
        }

        return await SpExecutor.TransferirEntresBodegasAsync(_ctx, SpCommandMapper.ToData(request), ct);
    }

    public async Task<(int Registros, decimal Cantidad, int Agotados)> GetSummaryAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Stock.AsNoTracking().Where(s => s.IdTenant == idTenant);
        var registros = await query.CountAsync(ct);
        if (registros == 0) return (0, 0m, 0);
        var cantidad = await query.SumAsync(s => s.CantAct, ct);
        var agotados = await query.CountAsync(s => s.CantAct <= 0, ct);
        return (registros, cantidad, agotados);
    }

    public async Task<IReadOnlyList<Stock>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        var q = _ctx.Stock.AsNoTracking().Where(s => s.IdTenant == idTenant);
        if (idBodega.HasValue) q = q.Where(s => s.IdBodega == idBodega.Value);
        return await q.OrderBy(s => s.IdStock).Take(take).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<StockListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var q = _ctx.Stock.AsNoTracking().Where(s => s.IdTenant == idTenant);
        if (idBodega.HasValue) q = q.Where(s => s.IdBodega == idBodega.Value);
        // S114 D2: texto = código o nombre de producto o variante, o código de barras (misma regla que Productos, S91.3).
        var ids = ProdRepositoryAsync.IdsPorTexto(_ctx, idTenant, texto);
        if (ids is not null) q = q.Where(s => ids.Contains(s.VarProd.IdProd));
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(s => s.VarProd.Prod.Codigo).ThenBy(s => s.VarProd.Codigo).ThenBy(s => s.IdBodega).ThenBy(s => s.IdStock)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new StockListadoRow(s.IdStock, s.IdTenant, s.IdBodega, s.IdVarProd, s.IdLote, s.CantAct, s.CostoProm, s.FecAct,
                s.VarProd.Prod.Codigo, s.VarProd.Prod.Nombre, s.VarProd.Codigo, s.VarProd.Nombre))
            .ToListAsync(ct);
        return (items, total);
    }
}



