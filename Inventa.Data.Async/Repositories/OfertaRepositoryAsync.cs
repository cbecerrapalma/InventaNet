using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

// S57.52: repo async-only (sin contraparte sync, como ProductoImagen/Storefront).
// Anti-solapamiento en transacción Serializable corta (invariante
// una-vigente-por-presentación; UNIQUE no expresa rangos, §40.2 S57.51).
public interface IOfertaRepositoryAsync
{
    Task<Oferta?> GetByIdAsync(long idOferta, CancellationToken ct = default);
    Task<IReadOnlyList<Oferta>> GetByPresProdAsync(long idPresProd, CancellationToken ct = default);
    Task<Oferta?> GetVigenteAsync(long idPresProd, DateOnly fecha, CancellationToken ct = default);
    Task<IReadOnlyList<Oferta>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly fecha, CancellationToken ct = default);
    Task<bool> ExistsOverlapAsync(long idPresProd, DateOnly desde, DateOnly? hasta, CancellationToken ct = default);
    // S57.64 R8: aprobación editorial (Publicado=true). Sin cambio de esquema.
    Task<Oferta?> SetPublicadoAsync(long idOferta, bool publicado, CancellationToken ct = default);
}

public class OfertaRepositoryAsync : IOfertaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public OfertaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<Oferta?> GetByIdAsync(long idOferta, CancellationToken ct = default)
    {
        if (idOferta <= 0) throw new ArgumentOutOfRangeException(nameof(idOferta));

        return await _ctx.Ofertas.AsNoTracking()
            .FirstOrDefaultAsync(o => o.IdOferta == idOferta, ct);
    }

    public async Task<IReadOnlyList<Oferta>> GetByPresProdAsync(long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Ofertas.AsNoTracking()
            .Where(o => o.IdPresProd == idPresProd)
            .OrderByDescending(o => o.FechaDesde).ThenByDescending(o => o.IdOferta)
            .ToListAsync(ct);
    }

    public async Task<Oferta?> GetVigenteAsync(long idPresProd, DateOnly fecha, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Ofertas.AsNoTracking()
            .Where(o => o.IdPresProd == idPresProd && o.Activo
                && o.FechaDesde <= fecha && (o.FechaHasta == null || o.FechaHasta >= fecha))
            .OrderByDescending(o => o.FechaDesde).ThenByDescending(o => o.IdOferta)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<Oferta>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly fecha, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        var ids = idsPresProd.Distinct().ToList();
        if (ids.Count == 0) return [];

        return await _ctx.Ofertas.AsNoTracking()
            .Where(o => ids.Contains(o.IdPresProd) && o.Activo
                && o.FechaDesde <= fecha && (o.FechaHasta == null || o.FechaHasta >= fecha))
            .OrderByDescending(o => o.FechaDesde).ToListAsync(ct);
    }

    public async Task<bool> ExistsOverlapAsync(long idPresProd, DateOnly desde, DateOnly? hasta, CancellationToken ct = default)
    {
        if (idPresProd <= 0) throw new ArgumentOutOfRangeException(nameof(idPresProd));

        return await _ctx.Ofertas.AnyAsync(o => o.IdPresProd == idPresProd && o.Activo
            && o.FechaDesde <= (hasta ?? DateOnly.MaxValue)
            && (o.FechaHasta == null || o.FechaHasta >= desde), ct);
    }

    public async Task<Oferta?> SetPublicadoAsync(long idOferta, bool publicado, CancellationToken ct = default)
    {
        if (idOferta <= 0) throw new ArgumentOutOfRangeException(nameof(idOferta));

        var e = await _ctx.Ofertas.FirstOrDefaultAsync(o => o.IdOferta == idOferta, ct);
        if (e == null) return null;
        e.Publicado = publicado;
        await _ctx.SaveChangesAsync(ct);
        return e;
    }
}
