using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IMarcaRepositoryAsync
{
    Task<IReadOnlyList<Marca>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Marca?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Marca>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<int> CountByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    // S57.34: CRUD para CatalogoServiceAsync (sin contraparte sync).
    Task<Marca?> GetByIdInTenantAsync(int idMarca, int idTenant, CancellationToken ct = default);
    Task<Marca> CreateAsync(Marca entity, CancellationToken ct = default);
    Task<Marca?> UpdateAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default);
    // S57.34: lectura global + inactivas para ProductoServiceAsync (sin contraparte sync).
    Task<Marca?> GetByIdAsync(int idMarca, CancellationToken ct = default);
    Task<IReadOnlyList<int>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default);
}

public class MarcaRepositoryAsync : IMarcaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public MarcaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Marca>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Marcas.AsNoTracking().Where(m => m.IdTenant == idTenant);
        if (soloActivas) query = query.Where(m => m.Activo);

        return await query.OrderBy(m => m.Nombre).ToListAsync(ct);
    }

    public async Task<Marca?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Marcas
            .AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdTenant == idTenant && m.Codigo == codigoNorm, ct);
    }

    public async Task<IReadOnlyList<Marca>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Requerido", nameof(nombre));

        return await _ctx.Marcas
            .AsNoTracking()
            .Where(m => m.IdTenant == idTenant && m.Activo && EF.Functions.Like(m.Nombre, $"%{nombre}%"))
            .OrderBy(m => m.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Marcas
            .AnyAsync(m => m.IdTenant == idTenant && m.Codigo == codigoNorm, ct);
    }

    public async Task<int> CountByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Marcas.Where(m => m.IdTenant == idTenant);
        if (soloActivas) query = query.Where(m => m.Activo);

        return await query.CountAsync(ct);
    }

    public async Task<Marca?> GetByIdInTenantAsync(int idMarca, int idTenant, CancellationToken ct = default)
    {
        if (idMarca <= 0) throw new ArgumentOutOfRangeException(nameof(idMarca));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Marcas
            .FirstOrDefaultAsync(m => m.IdMarca == idMarca && m.IdTenant == idTenant, ct);
    }

    public async Task<Marca> CreateAsync(Marca entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Marcas.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Marca?> UpdateAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var m = await _ctx.Marcas
            .FirstOrDefaultAsync(x => x.IdMarca == idMarca && x.IdTenant == idTenant, ct);
        if (m == null) return null;
        m.Codigo = codigo;
        m.Nombre = nombre;
        await _ctx.SaveChangesAsync(ct);
        return m;
    }

    public async Task<Marca?> GetByIdAsync(int idMarca, CancellationToken ct = default)
    {
        if (idMarca <= 0) throw new ArgumentOutOfRangeException(nameof(idMarca));

        return await _ctx.Marcas.AsNoTracking()
            .FirstOrDefaultAsync(m => m.IdMarca == idMarca, ct);
    }

    public async Task<IReadOnlyList<int>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Marcas.AsNoTracking()
            .Where(m => m.IdTenant == idTenant && !m.Activo)
            .Select(m => m.IdMarca).ToListAsync(ct);
    }
}



