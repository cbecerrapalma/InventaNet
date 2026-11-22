using CBP.Data.Asynchronous;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IUsuarioRepositoryAsync
{
    Task<Usuario?> GetByIdAsync(int idUsuario, CancellationToken ct = default);
    Task<Usuario?> GetByNomAsync(string nomUsuario, CancellationToken ct = default);
    Task UpsertAsync(Usuario usuario, CancellationToken ct = default);
}

public class UsuarioRepositoryAsync : IUsuarioRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public UsuarioRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<Usuario?> GetByIdAsync(int idUsuario, CancellationToken ct = default)
        => _ctx.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == idUsuario, ct);

    public Task<Usuario?> GetByNomAsync(string nomUsuario, CancellationToken ct = default)
        => _ctx.Usuarios.FirstOrDefaultAsync(u => u.NomUsuario == nomUsuario, ct);

    public async Task UpsertAsync(Usuario usuario, CancellationToken ct = default)
    {
        var existing = await _ctx.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == usuario.IdUsuario, ct);
        if (existing == null)
        {
            _ctx.Usuarios.Add(usuario);
        }
        else
        {
            _ctx.Entry(existing).CurrentValues.SetValues(usuario);
            existing.FecMod = DateTime.UtcNow;
        }
        await _ctx.SaveChangesAsync(ct);
    }
}




