using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>S117 (P19): fila del buscador de presentaciones de los formularios de venta y compra.</summary>
public sealed record PresentacionBusquedaRow(
    long IdPresProd, long IdProd, string ProdCodigo, string ProdNombre, string VarCodigo, string VarNombre, string PresDescrip,
    string? CodigoBarra, decimal? PrecioVigente, decimal CostoProm, decimal? Disponible);

public interface IPresProdRepositoryAsync
{
    Task<IReadOnlyList<PresProd>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default);
    Task<IReadOnlyList<PresProd>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default);
    Task<PresProd?> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<bool> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<IReadOnlyList<PresProd>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<IReadOnlyList<PresProd>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default);
    // S57.34: tenant de la presentación (vía VarProd→Prod) para validación en Application.Async.
    Task<int?> GetTenantIdAsync(long idPresProd, CancellationToken ct = default);
    // S57.34: mapa IdPresProd→IdTenant en una sola query (evita N+1 en validación por lotes).
    Task<IReadOnlyDictionary<long, int>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default);
    // S57.34: presentación con jerarquía VarProd→Prod para validación tenant/variante en InventarioServiceAsync.
    Task<PresProd?> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default);
    // S57.34: CRUD/listados para ProductoServiceAsync (sin contraparte sync).
    Task<PresProd?> GetByIdAsync(long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<PresProd>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<IReadOnlyList<PresProd>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default);
    // S117 (P19): buscador de los formularios de venta y compra (antes sin consumidor).
    Task<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default);
}

public class PresProdRepositoryAsync : IPresProdRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PresProdRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<PresProd>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        var query = _ctx.Presentaciones.AsNoTracking().Where(p => p.IdVarProd == idVarProd);
        if (soloActivas) query = query.Where(p => p.Activo);

        return await query.Include(p => p.Unidad).OrderBy(p => p.Descrip).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PresProd>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        var query = _ctx.Presentaciones.AsNoTracking().Where(p => p.IdVarProd == idVarProd && p.Activo);
        query = paraVenta ? query.Where(p => p.EsVenta) : query.Where(p => p.EsCompra);

        return await query.Include(p => p.Unidad).OrderBy(p => p.Descrip).ToListAsync(ct);
    }

    public async Task<PresProd?> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        return await _ctx.Presentaciones
            .AsNoTracking()
            .Include(p => p.Unidad)
            .Include(p => p.CodigosBarra)
            .Include(p => p.Precios.Where(pr => pr.Activo && pr.FecVig <= fechaRef))
            .FirstOrDefaultAsync(p => p.IdPresProd == idPresProd, ct);
    }

    public async Task<bool> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Presentaciones
            .AnyAsync(p => p.IdVarProd == idVarProd && p.IdUnidad == idUnidad, ct);
    }

    public async Task<IReadOnlyList<PresProd>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Presentaciones
            .AsNoTracking()
            .Include(p => p.VarProd).ThenInclude(v => v.Prod)
            .Include(p => p.Unidad)
            .Where(p => p.VarProd!.Prod!.IdTenant == idTenant);

        if (soloActivas)
            query = query.Where(p => p.Activo);

        return await query
            .OrderBy(p => p.VarProd!.Prod!.Nombre)
            .ThenBy(p => p.Descrip)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PresProd>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        var query = _ctx.Presentaciones
            .AsNoTracking()
            .Include(p => p.Unidad)
            .Include(p => p.VarProd)
            .Where(p => p.VarProd!.IdProd == idProd);

        if (soloActivas)
            query = query.Where(p => p.Activo);

        return await query
            .OrderBy(p => p.VarProd!.Nombre)
            .ThenBy(p => p.Descrip)
            .ToListAsync(ct);
    }

    public async Task<int?> GetTenantIdAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Presentaciones
            .AsNoTracking()
            .Where(p => p.IdPresProd == idPresProd)
            .Select(p => (int?)p.VarProd!.Prod!.IdTenant)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyDictionary<long, int>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        var ids = idsPresProd.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<long, int>();

        return await _ctx.Presentaciones
            .AsNoTracking()
            .Where(p => ids.Contains(p.IdPresProd))
            .Select(p => new { p.IdPresProd, IdTenant = p.VarProd!.Prod!.IdTenant })
            .ToDictionaryAsync(x => x.IdPresProd, x => x.IdTenant, ct);
    }

    public async Task<PresProd?> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Presentaciones.AsNoTracking()
            .Include(p => p.VarProd!).ThenInclude(v => v.Prod!)
            .FirstOrDefaultAsync(p => p.IdPresProd == idPresProd, ct);
    }

    public async Task<PresProd?> GetByIdAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Presentaciones.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPresProd == idPresProd, ct);
    }

    public async Task<IReadOnlyList<PresProd>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Presentaciones.AsNoTracking()
            .Where(p => p.IdVarProd == idVarProd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<PresProd>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsVarProd);
        var ids = idsVarProd.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _ctx.Presentaciones.AsNoTracking()
            .Where(pr => ids.Contains(pr.IdVarProd) && pr.Activo && pr.EsVenta)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // S117 D2: activas del tenant (presentación, variante y producto), de venta o de compra;
        // texto = código o nombre de producto o variante, o código de barras (misma regla que Productos y Stock, S91.3/S114).
        var q = _ctx.Presentaciones.AsNoTracking()
            .Where(pr => pr.VarProd.Prod.IdTenant == idTenant && pr.Activo && pr.VarProd.Activo && pr.VarProd.Prod.Activo);
        q = paraVenta ? q.Where(pr => pr.EsVenta) : q.Where(pr => pr.EsCompra);
        var t = texto?.Trim();
        if (string.IsNullOrEmpty(t)) t = null;
        else if (t.Length > 100) t = t[..100];
        var ids = ProdRepositoryAsync.IdsPorTexto(_ctx, idTenant, t);
        if (ids is not null) q = q.Where(pr => ids.Contains(pr.VarProd.IdProd));
        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var total = await q.CountAsync(ct);
        // El código de barras exacto va primero (lector de códigos); precio vigente con el criterio de StockDispRepositoryAsync.GetByCodigoBarraPOSAsync, por la navegación (lectura: sin _ctx de precios, guarda Type3WriterBoundary).
        var items = await q
            .OrderByDescending(pr => t != null && pr.CodigosBarra.Any(cb => cb.Codigo == t))
            .ThenBy(pr => pr.VarProd.Prod.Nombre).ThenBy(pr => pr.VarProd.Codigo).ThenBy(pr => pr.IdPresProd)
            .Skip((page - 1) * pageSize).Take(pageSize)
            .Select(pr => new PresentacionBusquedaRow(
                pr.IdPresProd, pr.VarProd.IdProd, pr.VarProd.Prod.Codigo, pr.VarProd.Prod.Nombre, pr.VarProd.Codigo, pr.VarProd.Nombre, pr.Descrip,
                pr.CodigosBarra.OrderByDescending(cb => cb.Principal).ThenBy(cb => cb.IdCodBar).Select(cb => cb.Codigo).FirstOrDefault(),
                pr.Precios.Where(p => p.Activo && p.FecVig <= hoy)
                    .OrderByDescending(p => p.FecVig).Select(p => (decimal?)p.Valor).FirstOrDefault(),
                pr.VarProd.CostoProm,
                idBodega.HasValue
                    ? _ctx.StockDisponible.Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega.Value && s.IdPresProd == pr.IdPresProd)
                        .Select(s => (decimal?)s.CantDisp).FirstOrDefault()
                    : null))
            .ToListAsync(ct);
        return (items, total);
    }

}



