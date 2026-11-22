using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IUsuarioTenantRepositoryAsync
{
    Task<UsuarioTenant?> GetAsync(int idUsuario, int idTenant, CancellationToken ct = default);
    Task<List<UsuarioTenant>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default);
    Task UpsertAsync(UsuarioTenant ut, CancellationToken ct = default);
}

public class UsuarioTenantRepositoryAsync : IUsuarioTenantRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public UsuarioTenantRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<UsuarioTenant?> GetAsync(int idUsuario, int idTenant, CancellationToken ct = default)
        => _ctx.UsuarioTenants.FirstOrDefaultAsync(x => x.IdUsuario == idUsuario && x.IdTenant == idTenant, ct);

    public Task<List<UsuarioTenant>> GetByUsuarioAsync(int idUsuario, CancellationToken ct = default)
        => _ctx.UsuarioTenants.Where(x => x.IdUsuario == idUsuario && x.Activo).ToListAsync(ct);

    public async Task UpsertAsync(UsuarioTenant ut, CancellationToken ct = default)
    {
        var existing = await _ctx.UsuarioTenants.FirstOrDefaultAsync(x => x.IdUsuario == ut.IdUsuario && x.IdTenant == ut.IdTenant, ct);
        if (existing == null)
        {
            _ctx.UsuarioTenants.Add(ut);
        }
        else
        {
            existing.Activo = ut.Activo;
            existing.EsPrincipal = ut.EsPrincipal;
            existing.FechaSync = DateTime.UtcNow;
        }
        await _ctx.SaveChangesAsync(ct);
    }
}




