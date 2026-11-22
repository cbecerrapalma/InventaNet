using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IVarProdRepositoryAsync
{
    Task<IReadOnlyList<VarProd>> GetByProductoAsync(long idProd, bool soloActivos = true, CancellationToken ct = default);
    Task<VarProd?> GetByCodigoAsync(long idProd, string codigo, CancellationToken ct = default);
    Task<VarProd?> GetWithPresentacionesAsync(long idVarProd, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(long idProd, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<VarProd>> GetConCostoCargadoAsync(long idProd, CancellationToken ct = default);
    Task<IReadOnlyList<VarProd>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<VarProd>> GetConBajoStockAsync(int idTenant, decimal umbralStock = 0m, CancellationToken ct = default);
    // S57.34: CRUD/diagnóstico para ProductoServiceAsync (sin contraparte sync).
    Task<VarProd?> GetByIdAsync(long idVarProd, CancellationToken ct = default);
    Task<IReadOnlyList<long>> GetSaleProdIdsAsync(int idTenant, CancellationToken ct = default);
    // S57.34: espejos exactos del servicio (sin orden/include) para ProductoServiceAsync.
    Task<IReadOnlyList<VarProd>> GetAllByProductoAsync(long idProd, CancellationToken ct = default);
    Task<IReadOnlyList<VarProd>> GetActiveByProdAsync(long idProd, CancellationToken ct = default);
}

public class VarProdRepositoryAsync : IVarProdRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public VarProdRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<VarProd>> GetByProductoAsync(long idProd, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        var query = _ctx.Variantes.AsNoTracking().Where(v => v.IdProd == idProd);
        if (soloActivos) query = query.Where(v => v.Activo);

        return await query.OrderBy(v => v.Nombre).ToListAsync(ct);
    }

    public async Task<VarProd?> GetByCodigoAsync(long idProd, string codigo, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Variantes
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdProd == idProd && v.Codigo == codigoNorm, ct);
    }

    public async Task<VarProd?> GetWithPresentacionesAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Variantes
            .AsNoTracking()
            .Include(v => v.Prod)
                .ThenInclude(p => p.UnidadBase)
            .Include(v => v.Presentaciones.Where(p => p.Activo))
                .ThenInclude(p => p.Unidad)
            .Include(v => v.Presentaciones.Where(p => p.Activo))
                .ThenInclude(p => p.CodigosBarra)
            .Include(v => v.Presentaciones.Where(p => p.Activo))
                .ThenInclude(p => p.Precios.Where(pr => pr.Activo))
            .Include(v => v.Atributos)
                .ThenInclude(a => a.AtriVal)
                    .ThenInclude(av => av.AtriDef)
            .FirstOrDefaultAsync(v => v.IdVarProd == idVarProd, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(long idProd, string codigo, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Variantes
            .AnyAsync(v => v.IdProd == idProd && v.Codigo == codigoNorm, ct);
    }

    public async Task<IReadOnlyList<VarProd>> GetConCostoCargadoAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.Variantes
            .AsNoTracking()
            .Where(v => v.IdProd == idProd && v.Activo && v.CostoProm > 0)
            .OrderBy(v => v.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VarProd>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Variantes
            .AsNoTracking()
            .Include(v => v.Prod)
            .Where(v => v.Prod.IdTenant == idTenant);

        if (soloActivos)
            query = query.Where(v => v.Activo && v.Prod.Activo);

        return await query
            .OrderBy(v => v.Prod!.Nombre)
            .ThenBy(v => v.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VarProd>> GetConBajoStockAsync(int idTenant, decimal umbralStock = 0m, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        // Paso 1: obtener los IdVarProd con stock bajo o sin stock — una sola query SQL con LEFT JOIN
        var idsBajoStock = await (
            from v in _ctx.Variantes
                .Where(v => v.Prod.IdTenant == idTenant && v.Activo && v.Prod.Activo)
            join s in _ctx.Stock
                .Where(s => s.IdTenant == idTenant)
                on v.IdVarProd equals s.IdVarProd into stockGroup
            from sg in stockGroup.DefaultIfEmpty()     // LEFT JOIN
            group sg by v.IdVarProd into g
            where (g.Sum(s => (decimal?)s.CantAct) ?? 0m) < umbralStock
            select g.Key
        ).ToListAsync(ct);

        if (idsBajoStock.Count == 0)
            return [];

        // Paso 2: cargar las entidades completas con su Prod para el ordenamiento
        var variantes = await _ctx.Variantes
            .AsNoTracking()
            .Include(v => v.Prod)
            .Where(v => idsBajoStock.Contains(v.IdVarProd))
            .OrderBy(v => v.Prod!.Nombre)
            .ThenBy(v => v.Nombre)
            .ToListAsync(ct);

        return variantes;
    }

    public async Task<VarProd?> GetByIdAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Variantes.AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVarProd == idVarProd, ct);
    }

    public async Task<IReadOnlyList<long>> GetSaleProdIdsAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Variantes.AsNoTracking()
            .Where(v => v.Activo && v.Prod.IdTenant == idTenant
                && _ctx.Presentaciones.Any(pp => pp.IdVarProd == v.IdVarProd && pp.Activo && pp.EsVenta))
            .Select(v => v.IdProd).Distinct().ToListAsync(ct);
    }


    public async Task<IReadOnlyList<VarProd>> GetAllByProductoAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.Variantes.AsNoTracking()
            .Where(v => v.IdProd == idProd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VarProd>> GetActiveByProdAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.Variantes.AsNoTracking()
            .Where(v => v.IdProd == idProd && v.Activo)
            .ToListAsync(ct);
    }
}
