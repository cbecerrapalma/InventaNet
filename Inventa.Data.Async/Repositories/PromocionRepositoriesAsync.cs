using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

// S57.66: repos async-only (sin contraparte sync, como Oferta/ProductoImagen/Storefront).
// Aggregate root = Promocion (owns Presentaciones/Reglas/Beneficios, doc 04).
// Creación agregada transaccional (ReadCommitted; sin Serializable indiscriminado).
public interface IPromocionRepositoryAsync
{
    Task<Promocion?> GetByIdAsync(long idPromocion, CancellationToken ct = default);
    Task<Promocion?> GetByIdWithChildrenAsync(long idPromocion, CancellationToken ct = default);
    Task<IReadOnlyList<Promocion>> GetByTenantAsync(int idTenant, CancellationToken ct = default);
    Task<Promocion?> GetByTenantAndCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task AddAsync(Promocion entity, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task<Promocion> CreateAggregateAsync(Promocion promocion, IEnumerable<PromocionPresentacion> presentaciones, IEnumerable<PromocionRegla> reglas, IEnumerable<PromocionBeneficio> beneficios, CancellationToken ct = default);
    Task DeleteAggregateAsync(Promocion promocion, CancellationToken ct = default);
}

public interface IPromocionPresentacionRepositoryAsync
{
    Task<PromocionPresentacion?> GetByIdAsync(long idPart, CancellationToken ct = default);
    Task<IReadOnlyList<PromocionPresentacion>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default);
    Task AddAsync(PromocionPresentacion entity, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
    Task RemoveAsync(PromocionPresentacion entity, CancellationToken ct = default);
}

public interface IPromocionReglaRepositoryAsync
{
    Task<PromocionRegla?> GetByIdAsync(long idRegla, CancellationToken ct = default);
    Task<IReadOnlyList<PromocionRegla>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default);
    Task AddAsync(PromocionRegla entity, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public interface IPromocionBeneficioRepositoryAsync
{
    Task<PromocionBeneficio?> GetByIdAsync(long idBeneficio, CancellationToken ct = default);
    Task<IReadOnlyList<PromocionBeneficio>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default);
    Task AddAsync(PromocionBeneficio entity, CancellationToken ct = default);
    Task SaveAsync(CancellationToken ct = default);
}

public class PromocionRepositoryAsync : IPromocionRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PromocionRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<Promocion?> GetByIdAsync(long idPromocion, CancellationToken ct = default)
    {
        if (idPromocion <= 0) throw new ArgumentOutOfRangeException(nameof(idPromocion));
        return await _ctx.Promociones.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPromocion == idPromocion, ct);
    }

    public async Task<Promocion?> GetByIdWithChildrenAsync(long idPromocion, CancellationToken ct = default)
    {
        if (idPromocion <= 0) throw new ArgumentOutOfRangeException(nameof(idPromocion));
        return await _ctx.Promociones.AsNoTracking()
            .Include(p => p.Presentaciones)
            .Include(p => p.Reglas)
            .Include(p => p.Beneficios)
            .FirstOrDefaultAsync(p => p.IdPromocion == idPromocion, ct);
    }

    public async Task<IReadOnlyList<Promocion>> GetByTenantAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        return await _ctx.Promociones.AsNoTracking()
            .Where(p => p.IdTenant == idTenant)
            .OrderByDescending(p => p.FechaDesde).ThenByDescending(p => p.IdPromocion)
            .ToListAsync(ct);
    }

    public async Task<Promocion?> GetByTenantAndCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(codigo);
        return await _ctx.Promociones.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdTenant == idTenant && p.Codigo == codigo, ct);
    }

    public async Task AddAsync(Promocion entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _ctx.Promociones.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<Promocion> CreateAggregateAsync(Promocion promocion, IEnumerable<PromocionPresentacion> presentaciones, IEnumerable<PromocionRegla> reglas, IEnumerable<PromocionBeneficio> beneficios, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(promocion);
        ArgumentNullException.ThrowIfNull(presentaciones);
        ArgumentNullException.ThrowIfNull(reglas);
        ArgumentNullException.ThrowIfNull(beneficios);

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        try
        {
            await _ctx.Promociones.AddAsync(promocion, ct);
            await _ctx.SaveChangesAsync(ct);
            foreach (var pres in presentaciones)
            {
                pres.IdPromocion = promocion.IdPromocion;
                await _ctx.PromocionPresentaciones.AddAsync(pres, ct);
            }
            foreach (var regla in reglas)
            {
                regla.IdPromocion = promocion.IdPromocion;
                await _ctx.PromocionReglas.AddAsync(regla, ct);
            }
            foreach (var ben in beneficios)
            {
                ben.IdPromocion = promocion.IdPromocion;
                await _ctx.PromocionBeneficios.AddAsync(ben, ct);
            }
            await _ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return promocion;
        }
        catch
        {
            try { await tx.RollbackAsync(ct); } catch { }
            throw;
        }
    }

    public async Task DeleteAggregateAsync(Promocion promocion, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(promocion);

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        try
        {
            var full = await _ctx.Promociones
                .Include(p => p.Presentaciones)
                .Include(p => p.Reglas)
                .Include(p => p.Beneficios)
                .FirstOrDefaultAsync(p => p.IdPromocion == promocion.IdPromocion, ct);
            if (full == null) return;
            _ctx.PromocionPresentaciones.RemoveRange(full.Presentaciones);
            _ctx.PromocionReglas.RemoveRange(full.Reglas);
            _ctx.PromocionBeneficios.RemoveRange(full.Beneficios);
            _ctx.Promociones.Remove(full);
            await _ctx.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch
        {
            try { await tx.RollbackAsync(ct); } catch { }
            throw;
        }
    }
}

public class PromocionPresentacionRepositoryAsync : IPromocionPresentacionRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PromocionPresentacionRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<PromocionPresentacion?> GetByIdAsync(long idPart, CancellationToken ct = default)
    {
        if (idPart <= 0) throw new ArgumentOutOfRangeException(nameof(idPart));
        return await _ctx.PromocionPresentaciones.AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdPart == idPart, ct);
    }

    public async Task<IReadOnlyList<PromocionPresentacion>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default)
    {
        if (idPromocion <= 0) throw new ArgumentOutOfRangeException(nameof(idPromocion));
        return await _ctx.PromocionPresentaciones.AsNoTracking()
            .Where(p => p.IdPromocion == idPromocion)
            .OrderBy(p => p.IdPart)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PromocionPresentacion entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _ctx.PromocionPresentaciones.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task RemoveAsync(PromocionPresentacion entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        var tracked = await _ctx.PromocionPresentaciones
            .FirstOrDefaultAsync(p => p.IdPart == entity.IdPart, ct);
        if (tracked == null) return;
        _ctx.PromocionPresentaciones.Remove(tracked);
        await _ctx.SaveChangesAsync(ct);
    }
}

public class PromocionReglaRepositoryAsync : IPromocionReglaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PromocionReglaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<PromocionRegla?> GetByIdAsync(long idRegla, CancellationToken ct = default)
    {
        if (idRegla <= 0) throw new ArgumentOutOfRangeException(nameof(idRegla));
        return await _ctx.PromocionReglas.AsNoTracking()
            .FirstOrDefaultAsync(r => r.IdRegla == idRegla, ct);
    }

    public async Task<IReadOnlyList<PromocionRegla>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default)
    {
        if (idPromocion <= 0) throw new ArgumentOutOfRangeException(nameof(idPromocion));
        return await _ctx.PromocionReglas.AsNoTracking()
            .Where(r => r.IdPromocion == idPromocion)
            .OrderBy(r => r.IdRegla)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PromocionRegla entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _ctx.PromocionReglas.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }
}

public class PromocionBeneficioRepositoryAsync : IPromocionBeneficioRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public PromocionBeneficioRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<PromocionBeneficio?> GetByIdAsync(long idBeneficio, CancellationToken ct = default)
    {
        if (idBeneficio <= 0) throw new ArgumentOutOfRangeException(nameof(idBeneficio));
        return await _ctx.PromocionBeneficios.AsNoTracking()
            .FirstOrDefaultAsync(b => b.IdBeneficio == idBeneficio, ct);
    }

    public async Task<IReadOnlyList<PromocionBeneficio>> GetByPromocionAsync(long idPromocion, CancellationToken ct = default)
    {
        if (idPromocion <= 0) throw new ArgumentOutOfRangeException(nameof(idPromocion));
        return await _ctx.PromocionBeneficios.AsNoTracking()
            .Where(b => b.IdPromocion == idPromocion)
            .OrderBy(b => b.IdBeneficio)
            .ToListAsync(ct);
    }

    public async Task AddAsync(PromocionBeneficio entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        await _ctx.PromocionBeneficios.AddAsync(entity, ct);
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task SaveAsync(CancellationToken ct = default)
    {
        await _ctx.SaveChangesAsync(ct);
    }
}
