using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IVarAtriRepositoryAsync
{
    Task<IReadOnlyList<VarAtri>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<IReadOnlyList<VarAtri>> GetByAtriValAsync(long idAtriVal, CancellationToken ct = default);
    Task<VarAtri?> GetAsync(long idVarProd, long idAtriVal, CancellationToken ct = default);
    Task<bool> ExistsAsync(long idVarProd, long idAtriVal, CancellationToken ct = default);
    Task<IReadOnlyList<VarAtri>> GetByVarianteConDetalleAsync(long idVarProd, CancellationToken ct = default);
    Task<IReadOnlyList<long>> GetVarianteIdsByAtriValAsync(long idAtriVal, CancellationToken ct = default);
    Task<int> CountByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<bool> VarianteCumpleAtributosObligatoriosAsync(long idVarProd, long idCategoria, CancellationToken ct = default);
}

public class VarAtriRepositoryAsync : IVarAtriRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public VarAtriRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<VarAtri>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.VarAtributos
            .AsNoTracking()
            .Where(va => va.IdVarProd == idVarProd)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<VarAtri>> GetByAtriValAsync(long idAtriVal, CancellationToken ct = default)
    {
        if (idAtriVal <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriVal));

        return await _ctx.VarAtributos
            .AsNoTracking()
            .Where(va => va.IdAtriVal == idAtriVal)
            .ToListAsync(ct);
    }

    public async Task<VarAtri?> GetAsync(long idVarProd, long idAtriVal, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idAtriVal <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriVal));

        return await _ctx.VarAtributos
            .AsNoTracking()
            .FirstOrDefaultAsync(va => va.IdVarProd == idVarProd && va.IdAtriVal == idAtriVal, ct);
    }

    public async Task<bool> ExistsAsync(long idVarProd, long idAtriVal, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idAtriVal <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriVal));

        return await _ctx.VarAtributos
            .AnyAsync(va => va.IdVarProd == idVarProd && va.IdAtriVal == idAtriVal, ct);
    }

    public async Task<IReadOnlyList<VarAtri>> GetByVarianteConDetalleAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.VarAtributos
            .AsNoTracking()
            .Where(va => va.IdVarProd == idVarProd)
            .Include(va => va.AtriVal)
                .ThenInclude(av => av.AtriDef)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<long>> GetVarianteIdsByAtriValAsync(long idAtriVal, CancellationToken ct = default)
    {
        if (idAtriVal <= 0) throw new ArgumentOutOfRangeException(nameof(idAtriVal));

        return await _ctx.VarAtributos
            .AsNoTracking()
            .Where(va => va.IdAtriVal == idAtriVal)
            .Select(va => va.IdVarProd)
            .ToListAsync(ct);
    }

    public async Task<int> CountByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.VarAtributos
            .CountAsync(va => va.IdVarProd == idVarProd, ct);
    }

    public async Task<bool> VarianteCumpleAtributosObligatoriosAsync(long idVarProd, long idCategoria, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        var obligatorios = await _ctx.AtributosDef
            .Where(a => a.IdCategoria == idCategoria && a.Activo && a.EsOblig)
            .Select(a => a.IdAtriDef)
            .ToListAsync(ct);

        if (obligatorios.Count == 0) return true;

        var cubiertos = await _ctx.VarAtributos
            .Where(va => va.IdVarProd == idVarProd)
            .Join(_ctx.AtributosVal,
                va => va.IdAtriVal, av => av.IdAtriVal,
                (va, av) => av.IdAtriDef)
            .Distinct()
            .ToListAsync(ct);

        return obligatorios.All(id => cubiertos.Contains(id));
    }
}



