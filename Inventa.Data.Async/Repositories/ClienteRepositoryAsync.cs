using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IClienteRepositoryAsync
{
    Task<IReadOnlyList<Cliente>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Cliente?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Cliente?> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> GetByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default);
    Task<IReadOnlyList<Cliente>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<bool> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<int> CountByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default);
    // S57.34: conteo total/activos para dashboard (sin contraparte sync).
    Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    // S57.34: lectura por id para validación tenant en Application.Async (sin contraparte sync).
    Task<Cliente?> GetByIdAsync(int idCliente, CancellationToken ct = default);
    // S57.34: CRUD/lecturas para CatalogoServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<Cliente>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<(IReadOnlyList<Cliente> Items, int Total)> SearchPagedAsync(int idTenant, string? texto, bool? activo, int page, int pageSize, CancellationToken ct = default);
    Task<Cliente?> GetByIdInTenantAsync(int idCliente, int idTenant, CancellationToken ct = default);
    Task<(Cliente? Cliente, bool Duplicado)> CreateAsync(Cliente entity, CancellationToken ct = default);
    Task<(Cliente? Cliente, bool Duplicado)> UpdateAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default);
}

public class ClienteRepositoryAsync : IClienteRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    // TipoCliente: 1=Público General, 2=Frecuente, 3=Mayorista, 4=Corporativo
    private const byte TipoClienteMin = 1;
    private const byte TipoClienteMax = 4;

    public ClienteRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<Cliente>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Clientes.AsNoTracking().Where(c => c.IdTenant == idTenant);
        if (soloActivos) query = query.Where(c => c.Activo);

        return await query.OrderBy(c => c.Nombre).ToListAsync(ct);
    }

    public async Task<Cliente?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdTenant == idTenant && c.Codigo == codigoNorm, ct);
    }

    public async Task<Cliente?> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(rut)) throw new ArgumentException("Requerido", nameof(rut));

        return await _ctx.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdTenant == idTenant && c.RUT == rut, ct);
    }

    public async Task<IReadOnlyList<Cliente>> GetByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (tipoCliente < TipoClienteMin || tipoCliente > TipoClienteMax)
            throw new ArgumentOutOfRangeException(nameof(tipoCliente));

        return await _ctx.Clientes
            .AsNoTracking()
            .Where(c => c.IdTenant == idTenant && c.Activo && c.TipoCliente == tipoCliente)
            .OrderBy(c => c.Nombre)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Cliente>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(nombre)) throw new ArgumentException("Requerido", nameof(nombre));

        return await _ctx.Clientes
            .AsNoTracking()
            .Where(c => c.IdTenant == idTenant && c.Activo
                     && EF.Functions.Like(c.Nombre, $"%{nombre}%"))
            .OrderBy(c => c.Nombre)
            .Take(50)
            .ToListAsync(ct);
    }

    public async Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(codigo)) throw new ArgumentException("Requerido", nameof(codigo));

        var codigoNorm = codigo.ToUpperInvariant();

        return await _ctx.Clientes
            .AnyAsync(c => c.IdTenant == idTenant && c.Codigo == codigoNorm, ct);
    }

    public async Task<bool> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (string.IsNullOrWhiteSpace(rut)) throw new ArgumentException("Requerido", nameof(rut));

        return await _ctx.Clientes
            .AnyAsync(c => c.IdTenant == idTenant && c.RUT == rut, ct);
    }

    public async Task<int> CountByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (tipoCliente < TipoClienteMin || tipoCliente > TipoClienteMax)
            throw new ArgumentOutOfRangeException(nameof(tipoCliente));

        return await _ctx.Clientes
            .CountAsync(c => c.IdTenant == idTenant && c.Activo && c.TipoCliente == tipoCliente, ct);
    }

    public async Task<Cliente?> GetByIdAsync(int idCliente, CancellationToken ct = default)
    {
        if (idCliente <= 0) throw new ArgumentOutOfRangeException(nameof(idCliente));

        return await _ctx.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente, ct);
    }

    public async Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var query = _ctx.Clientes.Where(c => c.IdTenant == idTenant);
        if (soloActivos) query = query.Where(c => c.Activo);
        return await query.CountAsync(ct);
    }

    public async Task<IReadOnlyList<Cliente>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (take <= 0) throw new ArgumentOutOfRangeException(nameof(take));

        return await _ctx.Clientes
            .AsNoTracking()
            .Where(c => c.IdTenant == idTenant)
            .OrderBy(c => c.Codigo)
            .Take(take)
            .ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<Cliente> Items, int Total)> SearchPagedAsync(int idTenant, string? texto, bool? activo, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 500) throw new ArgumentOutOfRangeException(nameof(pageSize));

        // S115 D5: sin texto pagina todos los clientes del tenant; activo (opcional) filtra en la BD.
        var q = _ctx.Clientes.AsNoTracking().Where(c => c.IdTenant == idTenant);
        if (!string.IsNullOrWhiteSpace(texto))
        {
            var t = texto.Trim();
            q = q.Where(c => c.Nombre.Contains(t) || c.Codigo.Contains(t));
        }
        if (activo.HasValue)
        {
            var a = activo.Value;
            q = q.Where(c => c.Activo == a);
        }
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(c => c.Codigo)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<Cliente?> GetByIdInTenantAsync(int idCliente, int idTenant, CancellationToken ct = default)
    {
        if (idCliente <= 0) throw new ArgumentOutOfRangeException(nameof(idCliente));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.Clientes
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.IdCliente == idCliente && c.IdTenant == idTenant, ct);
    }

    public async Task<(Cliente? Cliente, bool Duplicado)> CreateAsync(Cliente entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        _ctx.Clientes.Add(entity);
        try { await _ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, "UQ_Cliente")) { return (null, true); }
        return (entity, false);
    }

    public async Task<(Cliente? Cliente, bool Duplicado)> UpdateAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default)
    {
        var c = await _ctx.Clientes
            .FirstOrDefaultAsync(x => x.IdCliente == idCliente && x.IdTenant == idTenant, ct);
        if (c == null) return (null, false);
        c.Codigo = codigo;
        c.Nombre = nombre;
        c.RUT = rut;
        try { await _ctx.SaveChangesAsync(ct); }
        catch (DbUpdateException ex) when (IsUniqueViolation(ex, "UQ_Cliente")) { return (null, true); }
        return (c, false);
    }

    private static bool IsUniqueViolation(DbUpdateException ex, string constraint)
        => ex.ToString().Contains(constraint, StringComparison.OrdinalIgnoreCase)
        || ex.ToString().Contains("duplicate", StringComparison.OrdinalIgnoreCase);
}



