using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface ICategoriaRepositoryAsync
{
    Task<IReadOnlyList<Categoria>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Categoria?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<IReadOnlyList<Categoria>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<int> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default);
    // S57.34: CRUD para CatalogoServiceAsync (sin contraparte sync).
    Task<Categoria?> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default);
    Task<Categoria> CreateAsync(Categoria entity, CancellationToken ct = default);
    Task<Categoria?> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default);
    // S57.34: lectura global + inactivas para ProductoServiceAsync (sin contraparte sync).
    Task<Categoria?> GetByIdAsync(long idCategoria, CancellationToken ct = default);
    Task<IReadOnlyList<long>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default);
}

public class CategoriaRepositoryAsync : ICategoriaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public CategoriaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Categoria>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Categorias.AsNoTracking().Where(c => c.IdTenant == idTenant);
        if (soloActivas) query = query.Where(c => c.Activo);

        return await query.OrderBy(c => c.Nombre).ToListAsync(ct);
    }

    public async Task<Categoria?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Categorias
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdTenant == idTenant && c.Codigo == codigoNorm, ct);
    }

    public async Task<IReadOnlyList<Categoria>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Requerido", nameof(nombre));

        return await _ctx.Categorias
            .AsNoTracking()
            .Where(c => c.IdTenant == idTenant && c.Activo && EF.Functions.Like(c.Nombre, $"%{nombre}%"))
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Categorias
            .AnyAsync(c => c.IdTenant == idTenant && c.Codigo == codigoNorm, ct);
    }

    public async Task<int> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        return await _ctx.Set<Prod>()
            .CountAsync(p => p.IdCategoria == idCategoria && p.Activo, ct);
    }

    public async Task<Categoria?> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Categorias
            .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria && c.IdTenant == idTenant, ct);
    }

    public async Task<Categoria> CreateAsync(Categoria entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Categorias.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<Categoria?> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var c = await _ctx.Categorias
            .FirstOrDefaultAsync(x => x.IdCategoria == idCategoria && x.IdTenant == idTenant, ct);
        if (c == null) return null;
        c.Codigo = codigo;
        c.Nombre = nombre;
        await _ctx.SaveChangesAsync(ct);
        return c;
    }

    public async Task<Categoria?> GetByIdAsync(long idCategoria, CancellationToken ct = default)
    {
        if (idCategoria <= 0) throw new ArgumentOutOfRangeException(nameof(idCategoria));

        return await _ctx.Categorias.AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCategoria == idCategoria, ct);
    }

    public async Task<IReadOnlyList<long>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Categorias.AsNoTracking()
            .Where(c => c.IdTenant == idTenant && !c.Activo)
            .Select(c => c.IdCategoria).ToListAsync(ct);
    }
}



