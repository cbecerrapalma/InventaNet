using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>S114: fila del listado paginado de movimientos con producto, variante y presentación.</summary>
public sealed record MovInvListadoRow(
    long IdMovInv, int IdTenant, int IdBodega, long IdPresProd, long? IdLote, DateTime FecMov, byte TipoMov,
    decimal CantCom, decimal CantBase, decimal CostoUni, int? UsuarioCrea,
    string ProdCodigo, string ProdNombre, string VarCodigo, string VarNombre, string PresDescrip);

public interface IMovInvRepositoryAsync
{
    Task<IReadOnlyList<MovInv>> GetByPresentacionAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<MovInv>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<MovInv>> GetByTipoMovAsync(int idTenant, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<MovInv>> GetByLoteAsync(int idTenant, long idLote, CancellationToken ct = default);
    Task<decimal> GetSumaCantBaseAsync(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default);
    Task<IReadOnlyList<MovInv>> GetUltimosMovimientosAsync(int idTenant, int idBodega, int cantidad = 50, CancellationToken ct = default);
    Task<IReadOnlyList<MovInv>> GetKardexVarianteAsync(int idTenant, long idVarProd, DateTime desde, DateTime hasta, CancellationToken ct = default);
    // S57.34: listados para InventarioServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<MovInv>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<MovInvListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? texto, int page, int pageSize, CancellationToken ct = default);
}

public class MovInvRepositoryAsync : IMovInvRepositoryAsync
{
    private const int MaxUltimosMovimientos = 500;
    private readonly InventaDbContext _ctx;

    public MovInvRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    private static DateTime FinDelDia(DateTime fecha) => fecha.Date.AddDays(1).AddTicks(-1);

    public async Task<IReadOnlyList<MovInv>> GetByPresentacionAsync(int idTenant, long idPresProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (desde > hasta) throw new ArgumentException("Fecha 'desde' no puede ser mayor que 'hasta'");

        var hastaFin = FinDelDia(hasta);

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant
                     && m.IdPresProd == idPresProd
                     && m.FecMov >= desde
                     && m.FecMov <= hastaFin)
            .Include(m => m.Bodega)
            .Include(m => m.Lote)
            .OrderByDescending(m => m.FecMov)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetByBodegaAsync(int idTenant, int idBodega, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (desde > hasta) throw new ArgumentException("Fecha 'desde' no puede ser mayor que 'hasta'");

        var hastaFin = FinDelDia(hasta);

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant
                     && m.IdBodega == idBodega
                     && m.FecMov >= desde
                     && m.FecMov <= hastaFin)
            .Include(m => m.PresProd)
            .Include(m => m.Lote)
            .OrderByDescending(m => m.FecMov)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetByTipoMovAsync(int idTenant, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (tipoMov is < 1 or > 6) throw new ArgumentOutOfRangeException(nameof(tipoMov), "TipoMov debe estar entre 1 y 6");
        if (desde > hasta) throw new ArgumentException("Fecha 'desde' no puede ser mayor que 'hasta'");

        var hastaFin = FinDelDia(hasta);

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant
                     && m.TipoMov == tipoMov
                     && m.FecMov >= desde
                     && m.FecMov <= hastaFin)
            .Include(m => m.Bodega)
            .Include(m => m.PresProd)
            .OrderByDescending(m => m.FecMov)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetByLoteAsync(int idTenant, long idLote, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idLote <= 0) throw new ArgumentOutOfRangeException(nameof(idLote));

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant && m.IdLote == idLote)
            .Include(m => m.Bodega)
            .Include(m => m.PresProd)
            .OrderByDescending(m => m.FecMov)
            .ToListAsync(ct);
    }

    public async Task<decimal> GetSumaCantBaseAsync(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (tipoMov is < 1 or > 6) throw new ArgumentOutOfRangeException(nameof(tipoMov), "TipoMov debe estar entre 1 y 6");
        if (desde > hasta) throw new ArgumentException("Fecha 'desde' no puede ser mayor que 'hasta'");

        var hastaFin = FinDelDia(hasta);

        return await _ctx.MovimientosInventario
            .Where(m => m.IdTenant == idTenant
                     && m.IdPresProd == idPresProd
                     && m.IdBodega == idBodega
                     && m.TipoMov == tipoMov
                     && m.FecMov >= desde
                     && m.FecMov <= hastaFin)
            .SumAsync(m => m.CantBase, ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetUltimosMovimientosAsync(int idTenant, int idBodega, int cantidad = 50, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (cantidad is < 1 or > MaxUltimosMovimientos) throw new ArgumentOutOfRangeException(nameof(cantidad));

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant && m.IdBodega == idBodega)
            .Include(m => m.PresProd)
            .Include(m => m.Lote)
            .OrderByDescending(m => m.FecMov)
            .ThenByDescending(m => m.IdMovInv)
            .Take(cantidad)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetKardexVarianteAsync(int idTenant, long idVarProd, DateTime desde, DateTime hasta, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (desde > hasta) throw new ArgumentException("Fecha 'desde' no puede ser mayor que 'hasta'");

        var hastaFin = FinDelDia(hasta);

        return await _ctx.MovimientosInventario
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant
                     && m.PresProd!.IdVarProd == idVarProd
                     && m.FecMov >= desde
                     && m.FecMov <= hastaFin)
            .Include(m => m.Bodega)
            .Include(m => m.PresProd)
            .Include(m => m.Lote)
            .OrderByDescending(m => m.FecMov)
            .ThenByDescending(m => m.IdMovInv)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MovInv>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await _ctx.MovimientosInventario.AsNoTracking()
            .Where(m => m.IdTenant == idTenant)
            .OrderByDescending(m => m.FecMov).ThenByDescending(m => m.IdMovInv)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<MovInvListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var q = _ctx.MovimientosInventario.AsNoTracking().Where(m => m.IdTenant == idTenant);
        if (idBodega.HasValue) q = q.Where(m => m.IdBodega == idBodega.Value);
        // S114 D3: el tipo se filtra en la consulta (antes, la Web filtraba solo la página recibida).
        if (tipoMov.HasValue) q = q.Where(m => m.TipoMov == tipoMov.Value);
        // S114 D2: texto = código o nombre de producto o variante, o código de barras (misma regla que Productos, S91.3).
        var ids = ProdRepositoryAsync.IdsPorTexto(_ctx, idTenant, texto);
        if (ids is not null) q = q.Where(m => ids.Contains(m.PresProd.VarProd.IdProd));
        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(m => m.FecMov).ThenByDescending(m => m.IdMovInv)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(m => new MovInvListadoRow(m.IdMovInv, m.IdTenant, m.IdBodega, m.IdPresProd, m.IdLote, m.FecMov, m.TipoMov,
                m.CantCom, m.CantBase, m.CostoUni, m.UsuarioCrea,
                m.PresProd.VarProd.Prod.Codigo, m.PresProd.VarProd.Prod.Nombre, m.PresProd.VarProd.Codigo, m.PresProd.VarProd.Nombre, m.PresProd.Descrip))
            .ToListAsync(ct);
        return (items, total);
    }
}



