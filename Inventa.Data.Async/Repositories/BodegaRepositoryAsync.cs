using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IBodegaRepositoryAsync
{
    Task<IReadOnlyList<Bodega>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Bodega?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<bool> IsActivaAsync(int idTenant, int idBodega, CancellationToken ct = default);
    Task<Bodega?> GetByIdAsync(int idBodega, CancellationToken ct = default);
    Task<int> CountActivasAsync(int idTenant, CancellationToken ct = default);
    // S57.34: conteo total para dashboard (sin contraparte sync).
    Task<int> CountByTenantAsync(int idTenant, CancellationToken ct = default);
    // S57.34: CRUD/lecturas para InventarioServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Bodega>> GetOrderedByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<(IReadOnlyList<Bodega> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<Bodega?> GetByIdInTenantAsync(int idBodega, int idTenant, CancellationToken ct = default);
    Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, int excludeIdBodega, CancellationToken ct = default);
    Task<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)> CreateWithQuotaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<(Bodega? Bodega, bool Duplicado)> UpdateAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default);
}

public class BodegaRepositoryAsync : IBodegaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public BodegaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Bodega>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Bodegas.AsNoTracking().Where(b => b.IdTenant == idTenant);
        if (soloActivas) query = query.Where(b => b.Activo);

        return await query.OrderBy(b => b.Nombre).ToListAsync(ct);
    }

    public async Task<Bodega?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Bodegas
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdTenant == idTenant && b.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();
        return await _ctx.Bodegas
            .AnyAsync(b => b.IdTenant == idTenant && b.Codigo == codigoNorm, ct);
    }

    public async Task<bool> IsActivaAsync(int idTenant, int idBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.Bodegas
            .AnyAsync(b => b.IdTenant == idTenant && b.IdBodega == idBodega && b.Activo, ct);
    }

    public async Task<Bodega?> GetByIdAsync(int idBodega, CancellationToken ct = default)
    {
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));

        return await _ctx.Bodegas
            .AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdBodega == idBodega, ct);
    }

    public async Task<int> CountActivasAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Bodegas
            .CountAsync(b => b.IdTenant == idTenant && b.Activo, ct);
    }

    public async Task<int> CountByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Bodegas
            .CountAsync(b => b.IdTenant == idTenant, ct);
    }

    public async Task<IReadOnlyList<Bodega>> GetOrderedByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Bodegas.AsNoTracking()
            .Where(b => b.IdTenant == idTenant)
            .OrderBy(b => b.Codigo).ThenBy(b => b.IdBodega)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Bodega> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var q = _ctx.Bodegas.AsNoTracking().Where(b => b.IdTenant == idTenant);
        if (!string.IsNullOrWhiteSpace(like))
            q = q.Where(b => EF.Functions.Like(b.Codigo, like!) || EF.Functions.Like(b.Nombre, like!));
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(b => b.Codigo).ThenBy(b => b.IdBodega)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Bodega?> GetByIdInTenantAsync(int idBodega, int idTenant, CancellationToken ct = default)
    {
        if (idBodega <= 0) throw new ArgumentOutOfRangeException(nameof(idBodega));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Bodegas.AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdBodega == idBodega && b.IdTenant == idTenant, ct);
    }

    public async Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, int excludeIdBodega, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);

        return await _ctx.Bodegas
            .AnyAsync(b => b.IdTenant == idTenant && b.Codigo == codigo && b.IdBodega != excludeIdBodega, ct);
    }

    public async Task<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)> CreateWithQuotaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        ArgumentException.ThrowIfNullOrWhiteSpace(nombre);

        // MaxBodegas atomicity: Serializable transaction prevents count+insert race.
        await using var tx = await _ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
        try
        {
            if (await _ctx.Bodegas.AnyAsync(b => b.IdTenant == idTenant && b.Codigo == codigo, ct))
                return (null, true, null);
            var cfg = await _ctx.TenantConfigs.FirstOrDefaultAsync(c => c.IdTenant == idTenant, ct);
            var max = cfg?.MaxBodegas ?? 10;
            var count = await _ctx.Bodegas.CountAsync(b => b.IdTenant == idTenant, ct);
            if (count >= max)
                return (null, false, max);
            var bodega = new Bodega { IdTenant = idTenant, Codigo = codigo, Nombre = nombre };
            _ctx.Bodegas.Add(bodega);
            try { await _ctx.SaveChangesAsync(ct); }
            catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return (null, true, null); }
            await tx.CommitAsync(ct);
            return (bodega, false, null);
        }
        catch { try { await tx.RollbackAsync(ct); } catch { } throw; }
    }

    public async Task<(Bodega? Bodega, bool Duplicado)> UpdateAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var b = await _ctx.Bodegas
            .FirstOrDefaultAsync(x => x.IdBodega == idBodega && x.IdTenant == idTenant, ct);
        if (b == null) return (null, false);
        b.Codigo = codigo;
        b.Nombre = nombre;
        try { await _ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return (null, true); }
        return (b, false);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.ToString().Contains("UQ_Bod", StringComparison.OrdinalIgnoreCase)
        || ex.ToString().Contains("duplicate", StringComparison.OrdinalIgnoreCase);
}



