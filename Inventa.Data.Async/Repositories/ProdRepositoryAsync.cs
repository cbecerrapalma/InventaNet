using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

[Flags]
public enum VarianteLoadOption
{
    None = 0,
    Presentaciones = 1,
    Atributos = 2,
    Both = Presentaciones | Atributos
}

public interface IProdRepositoryAsync
{
    Task<IReadOnlyList<Prod>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default);
    Task<Prod?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Prod?> GetWithVariantesAsync(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> GetPesablesAsync(int idTenant, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default);
    Task<Prod?> GetWithVariantesYStockAsync(long idProd, int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<Prod>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default);
    Task<Prod?> GetByIdAsync(long idProd, CancellationToken ct = default);
    // S57.34: CRUD/listados/diagnóstico para ProductoServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Prod>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<Prod> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<Prod?> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default);
    Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, long excludeIdProd, CancellationToken ct = default);
    Task<Prod?> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default);
    Task<IReadOnlyList<ProdBaseRow>> GetBaseRowsAsync(int idTenant, CancellationToken ct = default);
    Task<ProdPreviewHeaderData?> GetPreviewHeaderAsync(long idProd, int idTenant, CancellationToken ct = default);
}

/// <summary>Fila base de producto para diagnóstico de publicabilidad (S57.3).</summary>
public sealed record ProdBaseRow(long IdProd, string Codigo, string Nombre, bool Activo, bool VisibleStore, long IdCategoria, int? IdMarca);

/// <summary>Encabezado de vista previa de publicación (S57.34).</summary>
public sealed record ProdPreviewHeaderData(long IdProd, string Codigo, string Nombre, long IdCategoria, string CatNombre, int? IdMarca, string? MarcaNombre);

public class ProdRepositoryAsync : IProdRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public ProdRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    private IQueryable<Prod> BaseQuery(int idTenant, bool soloActivos)
    {
        var q = _ctx.Productos.AsNoTracking().Where(p => p.IdTenant == idTenant);
        return soloActivos ? q.Where(p => p.Activo) : q;
    }

    public async Task<IReadOnlyList<Prod>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await BaseQuery(idTenant, soloActivos)
            .OrderBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Prod>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        return await BaseQuery(idTenant, soloActivos)
            .Where(p => p.IdCategoria == idCategoria)
            .OrderBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Prod>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idMarca <= 0) throw new ArgumentOutOfRangeException(nameof(idMarca));

        return await BaseQuery(idTenant, soloActivos)
            .Where(p => p.IdMarca == idMarca)
            .OrderBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Prod?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Productos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdTenant == idTenant && p.Codigo == codigoNorm, ct);
    }

    public async Task<Prod?> GetWithVariantesAsync(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        IQueryable<Prod> q = _ctx.Productos.AsNoTracking()
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .Include(p => p.Variantes.Where(v => v.Activo));

        if ((loadOptions & VarianteLoadOption.Presentaciones) != 0)
        {
            q = q.Include(p => p.Variantes.Where(v => v.Activo))
                .ThenInclude(v => v.Presentaciones.Where(pp => pp.Activo));
        }

        if ((loadOptions & VarianteLoadOption.Atributos) != 0)
        {
            q = q.Include(p => p.Variantes.Where(v => v.Activo))
                .ThenInclude(v => v.Atributos)
                    .ThenInclude(a => a.AtriVal)
                        .ThenInclude(av => av.AtriDef);
        }

        return await q.FirstOrDefaultAsync(p => p.IdProd == idProd && p.IdTenant == idTenant, ct);
    }

    public async Task<IReadOnlyList<Prod>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Requerido", nameof(nombre));

        return await BaseQuery(idTenant, soloActivos)
            .Where(p => EF.Functions.Like(p.Nombre, $"%{nombre}%"))
            .OrderBy(p => p.Nombre)
            .Take(100)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Prod>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await BaseQuery(idTenant, soloActivos: true)
            .Where(p => p.ManejaLot)
            .OrderBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Prod>> GetPesablesAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await BaseQuery(idTenant, soloActivos: true)
            .Where(p => p.EsPesable)
            .OrderBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Productos
            .AnyAsync(p => p.IdTenant == idTenant && p.Codigo == codigoNorm, ct);
    }

    public async Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Productos
            .CountAsync(p => p.IdTenant == idTenant && (!soloActivos || p.Activo), ct);
    }

    public async Task<IReadOnlyList<Prod>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(texto)) throw new ArgumentException("Requerido", nameof(texto));

        var textoNorm = texto.Trim().ToUpperInvariant();

        // 1. Seek exacto por código — UQ_Prod (IdTenant, Codigo), latencia mínima
        var porCodigo = await _ctx.Productos
            .AsNoTracking()
            .Where(p => p.IdTenant == idTenant
                     && p.Codigo == textoNorm
                     && (!soloActivos || p.Activo))
            .ToListAsync(ct);

        if (porCodigo.Count > 0)
            return porCodigo;

        // 2. Búsqueda parcial por nombre si no hubo match exacto
        return await BaseQuery(idTenant, soloActivos)
            .Where(p => EF.Functions.Like(p.Nombre, $"%{texto.Trim()}%"))
            .OrderBy(p => p.Nombre)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<Prod?> GetWithVariantesYStockAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        // Query 1: estructura del producto sin stock
        var prod = await _ctx.Productos.AsNoTracking()
            .Include(p => p.Marca)
            .Include(p => p.Categoria)
            .Include(p => p.UnidadBase)
            .Include(p => p.Variantes.Where(v => v.Activo))
                .ThenInclude(v => v.Presentaciones.Where(pp => pp.Activo))
                    .ThenInclude(pp => pp.Unidad)
            .Include(p => p.Variantes.Where(v => v.Activo))
                .ThenInclude(v => v.Atributos)
                    .ThenInclude(a => a.AtriVal)
                        .ThenInclude(av => av.AtriDef)
            .FirstOrDefaultAsync(p => p.IdProd == idProd && p.IdTenant == idTenant, ct);

        if (prod is null)
            return null;

        // Query 2: stocks del tenant para las variantes del producto
        var idVariantes = prod.Variantes.Select(v => v.IdVarProd).ToList();

        if (idVariantes.Count > 0)
        {
            var stocks = await _ctx.Stock
                .AsNoTracking()
                .Include(s => s.Bodega)
                .Where(s => s.IdTenant == idTenant && idVariantes.Contains(s.IdVarProd))
                .ToListAsync(ct);

            // Mapeo en memoria: asignar cada Stock a su VarProd
            var stocksPorVariante = stocks.GroupBy(s => s.IdVarProd)
                .ToDictionary(g => g.Key, g => g.ToList());

            foreach (var variante in prod.Variantes)
            {
                if (stocksPorVariante.TryGetValue(variante.IdVarProd, out var stocksVariante))
                {
                    foreach (var s in stocksVariante)
                        variante.Stocks.Add(s);
                }
            }
        }

        return prod;
    }

    public async Task<IReadOnlyList<Prod>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await BaseQuery(idTenant, soloActivos: true)
            .Where(p => p.DiasVen.HasValue)
            .OrderBy(p => p.DiasVen)
            .ThenBy(p => p.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Prod?> GetByIdAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.Productos.AsNoTracking()
            .Include(p => p.Categoria)
            .Include(p => p.Marca)
            .FirstOrDefaultAsync(p => p.IdProd == idProd, ct);
    }

    public async Task<IReadOnlyList<Prod>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await _ctx.Productos.AsNoTracking()
            .Where(p => p.IdTenant == idTenant)
            .OrderBy(p => p.Codigo).ThenBy(p => p.IdProd)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Prod> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var q = _ctx.Productos.AsNoTracking().Where(p => p.IdTenant == idTenant);
        // S91.3: búsqueda "contiene" (EF traduce Contains a LIKE '%…%' escapando % _ [) sobre código y nombre
        // del producto, código y nombre de variante y código de barras. Antes el texto se usaba como patrón
        // LIKE literal y solo acertaba con valores exactos. Se arma como UNION de ids: el OR con EXISTS
        // anidados correlacionados generaba un plan de > 2 min en VESTUARIO; la unión tarda ~45 ms.
        var ids = IdsPorTexto(_ctx, idTenant, like);
        if (ids is not null)
            q = q.Where(p => ids.Contains(p.IdProd));
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.Codigo).ThenBy(p => p.IdProd)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    /// <summary>
    /// S91.3/S92: ids de productos del tenant que "contienen" el texto en código/nombre de producto, código/nombre
    /// de variante o código de barras (UNION de ids; ver S91.3). Null si no hay texto. Reutilizada por el listado S92.
    /// </summary>
    public static IQueryable<long>? IdsPorTexto(InventaDbContext ctx, int idTenant, string? texto)
    {
        var t = texto?.Trim();
        if (string.IsNullOrEmpty(t)) return null;
        if (t.Length > 100) t = t[..100];
        var porProducto = ctx.Productos
            .Where(p => p.IdTenant == idTenant && (p.Codigo.Contains(t) || p.Nombre.Contains(t)))
            .Select(p => p.IdProd);
        var porVariante = ctx.Variantes
            .Where(v => v.Prod.IdTenant == idTenant && (v.Codigo.Contains(t) || v.Nombre.Contains(t)))
            .Select(v => v.IdProd);
        var porCodigoBarras = ctx.CodigosBarra
            .Where(cb => cb.PresProd.VarProd.Prod.IdTenant == idTenant && cb.Codigo.Contains(t))
            .Select(cb => cb.PresProd.VarProd.IdProd);
        return porProducto.Union(porVariante).Union(porCodigoBarras);
    }

    public async Task<Prod?> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Productos.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProd == idProd && p.IdTenant == idTenant, ct);
    }

    public async Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, long excludeIdProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        return await _ctx.Productos
            .AnyAsync(p => p.IdTenant == idTenant && p.Codigo == codigo && p.IdProd != excludeIdProd, ct);
    }

    public async Task<Prod?> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default)
    {
        var p = await _ctx.Productos
            .FirstOrDefaultAsync(x => x.IdProd == idProd && x.IdTenant == idTenant, ct);
        if (p == null) return null;
        p.VisibleStore = visible;
        await _ctx.SaveChangesAsync(ct);
        return p;
    }

    public async Task<IReadOnlyList<ProdBaseRow>> GetBaseRowsAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Productos.AsNoTracking()
            .Where(p => p.IdTenant == idTenant)
            .Select(p => new ProdBaseRow(p.IdProd, p.Codigo, p.Nombre, p.Activo, p.VisibleStore, p.IdCategoria, p.IdMarca))
            .ToListAsync(ct);
    }

    public async Task<ProdPreviewHeaderData?> GetPreviewHeaderAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Productos.AsNoTracking()
            .Where(p => p.IdProd == idProd && p.IdTenant == idTenant && p.Activo)
            .Select(p => new ProdPreviewHeaderData(p.IdProd, p.Codigo, p.Nombre, p.IdCategoria, p.Categoria.Nombre, p.IdMarca, p.Marca != null ? p.Marca.Nombre : null))
            .FirstOrDefaultAsync(ct);
    }

}



