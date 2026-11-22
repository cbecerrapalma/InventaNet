using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface ILoteRepositoryAsync
{
    Task<IReadOnlyList<Lote>> GetByVarianteAsync(int idTenant, long idVarProd, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetProximosAVencerAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetVencidosAsync(int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetPorVencerConStockAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    Task<Lote?> GetByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetOrdenFEFOAsync(int idTenant, long idVarProd, int idBodega, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetAlertasVencimientoByDiasVenAsync(int idTenant, CancellationToken ct = default);
    // S57.34: conteos de vencimiento para dashboard (sin contraparte sync).
    Task<(int Total, int Vencidos, int Proximos)> GetVencimientoSummaryAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default);
    // S57.34: lecturas para InventarioServiceAsync (sin contraparte sync).
    Task<Lote?> GetByIdAsync(long idLote, CancellationToken ct = default);
    Task<IReadOnlyList<Lote>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<Lote> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
}

public class LoteRepositoryAsync : ILoteRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public LoteRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Lote>> GetByVarianteAsync(int idTenant, long idVarProd, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        var query = _ctx.Lotes.AsNoTracking().Where(l => l.IdTenant == idTenant && l.IdVarProd == idVarProd);
        if (soloActivos) query = query.Where(l => l.Activo);

        return await query.OrderBy(l => l.FecVen).ThenBy(l => l.Codigo).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Lote>> GetProximosAVencerAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (diasAlerta is < 1 or > 365) throw new ArgumentOutOfRangeException(nameof(diasAlerta));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoy.AddDays(diasAlerta);

        return await _ctx.Lotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant
                     && l.Activo
                     && l.FecVen.HasValue
                     && l.FecVen.Value >= hoy
                     && l.FecVen.Value <= limite)
            .Include(l => l.VarProd)
            .OrderBy(l => l.FecVen)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Lote>> GetVencidosAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _ctx.Lotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant && l.Activo
                     && l.FecVen.HasValue && l.FecVen.Value < hoy)
            .Include(l => l.VarProd)
            .OrderBy(l => l.FecVen)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Lote>> GetPorVencerConStockAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (diasAlerta is < 1 or > 365) throw new ArgumentOutOfRangeException(nameof(diasAlerta));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        var limite = hoy.AddDays(diasAlerta);

        return await _ctx.Lotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant
                     && l.Activo
                     && l.FecVen.HasValue
                     && l.FecVen.Value >= hoy
                     && l.FecVen.Value <= limite
                     && _ctx.Stock.Any(s =>
                         s.IdTenant == idTenant
                      && s.IdVarProd == l.IdVarProd
                      && s.IdLote == l.IdLote
                      && s.CantAct > 0))
            .Include(l => l.VarProd)
            .OrderBy(l => l.FecVen)
            .ToListAsync(ct);
    }

    public async Task<Lote?> GetByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Lotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdTenant == idTenant
                                   && l.IdVarProd == idVarProd
                                   && l.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, long idVarProd, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Lotes
            .AnyAsync(l => l.IdTenant == idTenant
                        && l.IdVarProd == idVarProd
                        && l.Codigo == codigoNorm, ct);
    }

    public async Task<IReadOnlyList<Lote>> GetOrdenFEFOAsync(int idTenant, long idVarProd, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        return await _ctx.Lotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant
                     && l.IdVarProd == idVarProd
                     && l.Activo
                     && (!l.FecVen.HasValue || l.FecVen.Value > hoy)
                     && _ctx.Stock.Any(s =>
                         s.IdTenant == idTenant
                      && s.IdBodega == idBodega
                      && s.IdVarProd == idVarProd
                      && s.IdLote == l.IdLote
                      && s.CantAct > 0))
            .OrderBy(l => l.FecVen.HasValue ? 0 : 1)
            .ThenBy(l => l.FecVen)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Lote>> GetAlertasVencimientoByDiasVenAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

        // Paso 1: obtener el máximo DiasVen del tenant para filtro conservador en SQL
        var maxDiasVen = await _ctx.Productos
            .Where(p => p.IdTenant == idTenant && p.Activo && p.DiasVen.HasValue)
            .Select(p => (int?)p.DiasVen)
            .MaxAsync(ct) ?? 0;

        if (maxDiasVen == 0)
            return [];

        var limiteConservador = hoy.AddDays(maxDiasVen);

        // Paso 2: traer candidatos dentro del window conservador
        var candidatos = await _ctx.Lotes
            .AsNoTracking()
            .Include(l => l.VarProd)
                .ThenInclude(v => v.Prod)
            .Where(l => l.IdTenant == idTenant
                     && l.Activo
                     && l.FecVen.HasValue
                     && l.FecVen.Value >= hoy
                     && l.FecVen.Value <= limiteConservador
                     && l.VarProd.Prod.DiasVen.HasValue
                     && _ctx.Stock.Any(s =>
                         s.IdTenant == idTenant
                      && s.IdVarProd == l.IdVarProd
                      && s.IdLote == l.IdLote
                      && s.CantAct > 0))
            .ToListAsync(ct);

        // Paso 3: filtro exacto en memoria aplicando DiasVen específico del producto
        var alertas = candidatos
            .Where(l => l.FecVen!.Value <= hoy.AddDays(l.VarProd.Prod.DiasVen!.Value))
            .OrderBy(l => l.FecVen)
            .ThenBy(l => l.VarProd.Prod.Nombre)
            .ToList();

        return alertas;
    }

    public async Task<(int Total, int Vencidos, int Proximos)> GetVencimientoSummaryAsync(int idTenant, int diasAlerta = 30, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var total = await _ctx.Lotes.CountAsync(l => l.IdTenant == idTenant, ct);
        if (total == 0) return (0, 0, 0);
        var today = DateOnly.FromDateTime(DateTime.Today);
        var lim = today.AddDays(diasAlerta);
        var vencidos = await _ctx.Lotes.CountAsync(l => l.IdTenant == idTenant && l.FecVen != null && l.FecVen < today, ct);
        var proximos = await _ctx.Lotes.CountAsync(l => l.IdTenant == idTenant && l.FecVen != null && l.FecVen >= today && l.FecVen <= lim, ct);
        return (total, vencidos, proximos);
    }

    public async Task<Lote?> GetByIdAsync(long idLote, CancellationToken ct = default)
    {
        if (idLote <= 0) throw new ArgumentOutOfRangeException(nameof(idLote));

        return await _ctx.Lotes.AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdLote == idLote, ct);
    }

    public async Task<IReadOnlyList<Lote>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await _ctx.Lotes.AsNoTracking()
            .Where(l => l.IdTenant == idTenant)
            .OrderBy(l => l.Codigo).ThenBy(l => l.IdLote)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Lote> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var q = _ctx.Lotes.AsNoTracking().Where(l => l.IdTenant == idTenant);
        if (!string.IsNullOrWhiteSpace(like))
            q = q.Where(l => EF.Functions.Like(l.Codigo, like!));
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(l => l.Codigo).ThenBy(l => l.IdLote)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}



