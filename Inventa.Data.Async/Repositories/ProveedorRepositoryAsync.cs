using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public sealed record ProveedorConOCPendiente(
    int     IdProveedor,
    string  RazonSocial,
    string? NombreComercial,
    int     CantOCActivas,
    decimal TotalComprometido);

public interface IProveedorRepositoryAsync
{
    Task<IReadOnlyList<Proveedor>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Proveedor?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Proveedor?> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<IReadOnlyList<Proveedor>> SearchByRazonSocialAsync(int idTenant, string texto, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<bool> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<IReadOnlyList<ProveedorConOCPendiente>> GetConOrdenesPendientesAsync(int idTenant, CancellationToken ct = default);
    // S57.34: lectura por id para validación tenant en Application.Async (sin contraparte sync).
    Task<Proveedor?> GetByIdAsync(int idProveedor, CancellationToken ct = default);
    // S57.34: CRUD/lecturas para CatalogoServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Proveedor>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<Proveedor> Items, int Total)> SearchPagedAsync(int idTenant, string? texto, bool? activo, int page, int pageSize, CancellationToken ct = default);
    Task<Proveedor?> GetByIdInTenantAsync(int idProveedor, int idTenant, CancellationToken ct = default);
    Task<(Proveedor? Proveedor, bool Duplicado)> CreateAsync(Proveedor entity, CancellationToken ct = default);
    Task<(Proveedor? Proveedor, bool Duplicado)> UpdateAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default);
}

public class ProveedorRepositoryAsync : IProveedorRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public ProveedorRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Proveedor>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Proveedores.AsNoTracking().Where(p => p.IdTenant == idTenant);
        if (soloActivos) query = query.Where(p => p.Activo);

        return await query.OrderBy(p => p.RazonSocial).ToListAsync(ct);
    }

    public async Task<Proveedor?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdTenant == idTenant && p.Codigo == codigoNorm, ct);
    }

    public async Task<Proveedor?> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(rut)) throw new ArgumentException("Requerido", nameof(rut));

        return await _ctx.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdTenant == idTenant && p.RUT == rut, ct);
    }

    public async Task<IReadOnlyList<Proveedor>> SearchByRazonSocialAsync(int idTenant, string texto, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(texto)) throw new ArgumentException("Requerido", nameof(texto));

        var patron = $"%{texto}%";

        return await _ctx.Proveedores
            .AsNoTracking()
            .Where(p => p.IdTenant == idTenant && p.Activo
                     && (EF.Functions.Like(p.RazonSocial, patron)
                      || EF.Functions.Like(p.NombreComercial, patron)))
            .OrderBy(p => p.RazonSocial)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Proveedores
            .AnyAsync(p => p.IdTenant == idTenant && p.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(rut)) throw new ArgumentException("Requerido", nameof(rut));

        return await _ctx.Proveedores
            .AnyAsync(p => p.IdTenant == idTenant && p.RUT == rut, ct);
    }

    public async Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Proveedores.Where(p => p.IdTenant == idTenant);
        if (soloActivos) query = query.Where(p => p.Activo);

        return await query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<ProveedorConOCPendiente>> GetConOrdenesPendientesAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        // Estados "activos": 1=Pendiente, 2=Aprobada
        var resultado = await _ctx.OrdenesCompra
            .AsNoTracking()
            .Where(oc => oc.IdTenant == idTenant
                      && (oc.Estado == 1 || oc.Estado == 2))
            .GroupBy(oc => new
            {
                oc.IdProveedor,
                oc.Proveedor!.RazonSocial,
                oc.Proveedor!.NombreComercial
            })
            .Select(g => new ProveedorConOCPendiente(
                g.Key.IdProveedor,
                g.Key.RazonSocial,
                g.Key.NombreComercial,
                g.Count(),
                g.Sum(oc => oc.Total)))
            .OrderByDescending(r => r.TotalComprometido)
            .ToListAsync(ct);

        return resultado;
    }

    public async Task<Proveedor?> GetByIdAsync(int idProveedor, CancellationToken ct = default)
    {
        if (idProveedor <= 0) throw new ArgumentOutOfRangeException(nameof(idProveedor));

        return await _ctx.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProveedor == idProveedor, ct);
    }

    public async Task<IReadOnlyList<Proveedor>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await _ctx.Proveedores
            .AsNoTracking()
            .Where(p => p.IdTenant == idTenant)
            .OrderBy(p => p.Codigo)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Proveedor> Items, int Total)> SearchPagedAsync(int idTenant, string? texto, bool? activo, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // S115 D5: sin texto pagina todos los proveedores del tenant; activo (opcional) filtra en la BD.
        var q = _ctx.Proveedores.AsNoTracking().Where(p => p.IdTenant == idTenant);
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            q = q.Where(p => p.RazonSocial.Contains(t) || p.Codigo.Contains(t));
        }
        if (activo.HasValue)
        {
            var a = activo.Value;
            q = q.Where(p => p.Activo == a);
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(p => p.Codigo)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Proveedor?> GetByIdInTenantAsync(int idProveedor, int idTenant, CancellationToken ct = default)
    {
        if (idProveedor <= 0) throw new ArgumentOutOfRangeException(nameof(idProveedor));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Proveedores
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.IdProveedor == idProveedor && p.IdTenant == idTenant, ct);
    }

    public async Task<(Proveedor? Proveedor, bool Duplicado)> CreateAsync(Proveedor entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Proveedores.Add(entity);
        try { await _ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return (null, true); }
        return (entity, false);
    }

    public async Task<(Proveedor? Proveedor, bool Duplicado)> UpdateAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default)
    {
        var p = await _ctx.Proveedores
            .FirstOrDefaultAsync(x => x.IdProveedor == idProveedor && x.IdTenant == idTenant, ct);
        if (p == null) return (null, false);
        p.Codigo = codigo;
        p.RazonSocial = razonSocial;
        try { await _ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex)) { return (null, true); }
        return (p, false);
    }

    private static bool IsUniqueViolation(DbUpdateException ex)
        => ex.ToString().Contains("UQ_Prov", StringComparison.OrdinalIgnoreCase)
        || ex.ToString().Contains("duplicate", StringComparison.OrdinalIgnoreCase);
}



