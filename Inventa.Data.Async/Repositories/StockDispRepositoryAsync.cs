using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>Proyección POS: stock + precio en un solo round-trip. S57.69: precio normal único.</summary>
public sealed record StockConPrecio(
    long    IdPresProd,
    string  Descrip,
    decimal CantDisp,
    decimal PrecioVigente);

/// <summary>S114: fila del listado paginado de stock disponible con producto, variante y presentación.</summary>
public sealed record StockDispListadoRow(
    long IdStockDisp, int IdTenant, long IdPresProd, int IdBodega, decimal CantDisp,
    string ProdCodigo, string ProdNombre, string VarCodigo, string VarNombre, string PresDescrip);

public interface IStockDispRepositoryAsync
{
    Task<StockDisp?> GetAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default);
    Task<decimal> GetCantidadAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default);
    Task<bool> TieneStockSuficienteAsync(int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida, CancellationToken ct = default);
    Task<IReadOnlyList<StockDisp>> GetByBodegaAsync(int idTenant, int idBodega, bool soloConStock = false, CancellationToken ct = default);
    Task<IReadOnlyList<StockDisp>> GetAgotadosAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<IReadOnlyList<StockDisp>> GetConStockNegativoAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<StockConPrecio?> GetByCodigoBarraPOSAsync(int idTenant, int idBodega, string codigoBarra, CancellationToken ct = default);
    // S57.34: listados por tenant para InventarioServiceAsync (tenant vía bodegas, igual que el servicio original).
    Task<IReadOnlyList<StockDisp>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<StockDispListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default);
}

public class StockDispRepositoryAsync : IStockDispRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public StockDispRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<StockDisp?> GetAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.StockDisponible
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.IdTenant == idTenant
                                   && s.IdPresProd == idPresProd
                                   && s.IdBodega == idBodega, ct);
    }

    public async Task<decimal> GetCantidadAsync(int idTenant, long idPresProd, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.StockDisponible
            .Where(s => s.IdTenant == idTenant
                     && s.IdPresProd == idPresProd
                     && s.IdBodega == idBodega)
            .Select(s => (decimal?)s.CantDisp)
            .FirstOrDefaultAsync(ct) ?? 0m;
    }

    public async Task<bool> TieneStockSuficienteAsync(int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (cantidadRequerida <= 0m) throw new ArgumentOutOfRangeException(nameof(cantidadRequerida));

        return await _ctx.StockDisponible
            .AnyAsync(s => s.IdTenant == idTenant
                        && s.IdPresProd == idPresProd
                        && s.IdBodega == idBodega
                        && s.CantDisp >= cantidadRequerida, ct);
    }

    public async Task<IReadOnlyList<StockDisp>> GetByBodegaAsync(int idTenant, int idBodega, bool soloConStock = false, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        var query = _ctx.StockDisponible
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega);

        if (soloConStock)
            query = query.Where(s => s.CantDisp > 0);

        return await query
            .Include(s => s.PresProd)
                .ThenInclude(p => p.VarProd)
            .OrderBy(s => s.PresProd!.VarProd!.Nombre)
            .ThenBy(s => s.PresProd!.Descrip)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StockDisp>> GetAgotadosAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.StockDisponible
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega && s.CantDisp == 0)
            .Include(s => s.PresProd).ThenInclude(p => p.VarProd)
            .OrderBy(s => s.PresProd!.VarProd!.Nombre)
            .ThenBy(s => s.PresProd!.Descrip)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<StockDisp>> GetConStockNegativoAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.StockDisponible
            .AsNoTracking()
            .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega && s.CantDisp < 0)
            .Include(s => s.PresProd).ThenInclude(p => p.VarProd)
            .OrderBy(s => s.CantDisp)
            .ThenBy(s => s.PresProd!.VarProd!.Nombre)
            .ToListAsync(ct);
    }

    public async Task<StockConPrecio?> GetByCodigoBarraPOSAsync(int idTenant, int idBodega, string codigoBarra, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (string.IsNullOrWhiteSpace(codigoBarra)) throw new ArgumentException("Requerido", nameof(codigoBarra));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        try
        {
            var resultado = await (
                from cb in _ctx.CodigosBarra
                    .Where(c => c.Codigo == codigoBarra)
                join sd in _ctx.StockDisponible
                    .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega)
                    on cb.IdPresProd equals sd.IdPresProd
                join pp in _ctx.Presentaciones
                    on sd.IdPresProd equals pp.IdPresProd
                let precio = _ctx.Precios
                    .Where(p => p.IdPresProd == cb.IdPresProd
                             && p.Activo
                             && p.FecVig <= hoy)
                    .OrderByDescending(p => p.FecVig)
                    .Select(p => p.Valor)
                    .FirstOrDefault()
                select new StockConPrecio(
                    sd.IdPresProd,
                    pp.Descrip,
                    sd.CantDisp,
                    precio)
            ).FirstOrDefaultAsync(ct);

            return resultado;
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Error en GetByCodigoBarraPOSAsync: {ex.Message}", ex);
        }
    }

    public async Task<IReadOnlyList<StockDisp>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        var bodegaIds = await _ctx.Bodegas.AsNoTracking()
            .Where(b => b.IdTenant == idTenant).Select(b => b.IdBodega).ToListAsync(ct);
        var q = _ctx.StockDisponible.AsNoTracking().Where(s => bodegaIds.Contains(s.IdBodega));
        if (idBodega.HasValue) q = q.Where(s => s.IdBodega == idBodega.Value);
        return await q.OrderBy(s => s.IdStockDisp).Take(take).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<StockDispListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var bodegaIds = await _ctx.Bodegas.AsNoTracking()
            .Where(b => b.IdTenant == idTenant).Select(b => b.IdBodega).ToListAsync(ct);
        var q = _ctx.StockDisponible.AsNoTracking().Where(s => bodegaIds.Contains(s.IdBodega));
        if (idBodega.HasValue) q = q.Where(s => s.IdBodega == idBodega.Value);
        // S114 D2: texto = código o nombre de producto o variante, o código de barras (misma regla que Productos, S91.3).
        var ids = ProdRepositoryAsync.IdsPorTexto(_ctx, idTenant, texto);
        if (ids is not null) q = q.Where(s => ids.Contains(s.PresProd.VarProd.IdProd));
        var total = await q.CountAsync(ct);
        var items = await q
            .OrderBy(s => s.PresProd.VarProd.Prod.Codigo).ThenBy(s => s.PresProd.VarProd.Codigo).ThenBy(s => s.IdBodega).ThenBy(s => s.IdStockDisp)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(s => new StockDispListadoRow(s.IdStockDisp, s.IdTenant, s.IdPresProd, s.IdBodega, s.CantDisp,
                s.PresProd.VarProd.Prod.Codigo, s.PresProd.VarProd.Prod.Nombre, s.PresProd.VarProd.Codigo, s.PresProd.VarProd.Nombre, s.PresProd.Descrip))
            .ToListAsync(ct);
        return (items, total);
    }
}



