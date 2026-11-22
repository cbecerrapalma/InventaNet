using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IMetodoPagoRepositoryAsync
{
    Task<IReadOnlyList<MetodoPago>> GetActivosAsync(CancellationToken ct = default);
    // S57.34: lista completa para CatalogoServiceAsync (sin filtro Activo, sin contraparte sync).
    Task<IReadOnlyList<MetodoPago>> GetAllAsync(CancellationToken ct = default);
    Task<MetodoPago?> GetByCodigoAsync(string codigo, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default);
    // S57.34: existencia por id para validación en Application.Async (sin contraparte sync).
    Task<bool> ExistsAsync(byte idMetodoPago, CancellationToken ct = default);
}

public class MetodoPagoRepositoryAsync : IMetodoPagoRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public MetodoPagoRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<MetodoPago>> GetActivosAsync(CancellationToken ct = default)
    {
        return await _ctx.MetodosPago
            .AsNoTracking()
            .Where(m => m.Activo)
            .OrderBy(m => m.Nombre)
            .ToListAsync(ct);
    }

    public async Task<MetodoPago?> GetByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.MetodosPago
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.MetodosPago
            .AnyAsync(m => m.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsAsync(byte idMetodoPago, CancellationToken ct = default)
    {
        return await _ctx.MetodosPago
            .AnyAsync(m => m.IdMetodoPago == idMetodoPago, ct);
    }

    public async Task<IReadOnlyList<MetodoPago>> GetAllAsync(CancellationToken ct = default)
        => await _ctx.MetodosPago.AsNoTracking().ToListAsync(ct);
}



