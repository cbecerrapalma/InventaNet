using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IUsuarioSyncService
{
    Task<Result<Usuario>> EnsureUsuarioAsync(int idUsuario, string nomUsuario, string? email, string? nombre, string? apellido, bool activo, bool emailVerificado, CancellationToken ct = default);
}

public class UsuarioSyncService : IUsuarioSyncService
{
    private readonly IUsuarioRepositoryAsync _repo;
    public UsuarioSyncService(IUsuarioRepositoryAsync repo) => _repo = repo;

    public async Task<Result<Usuario>> EnsureUsuarioAsync(int idUsuario, string nomUsuario, string? email, string? nombre, string? apellido, bool activo, bool emailVerificado, CancellationToken ct = default)
    {
        var existing = await _repo.GetByIdAsync(idUsuario, ct);
        if (existing == null)
        {
            var u = new Usuario
            {
                IdUsuario = idUsuario,
                NomUsuario = nomUsuario,
                Email = email,
                Nombre = nombre,
                Apellido = apellido,
                Activo = activo,
                EmailVerificado = emailVerificado,
                FechaSync = DateTime.UtcNow,
                FecCrea = DateTime.UtcNow
            };
            await _repo.UpsertAsync(u, ct);
            return Result<Usuario>.Success(u);
        }
        existing.NomUsuario = nomUsuario;
        existing.Email = email;
        existing.Nombre = nombre;
        existing.Apellido = apellido;
        existing.Activo = activo;
        existing.EmailVerificado = emailVerificado;
        existing.FechaSync = DateTime.UtcNow;
        existing.FecMod = DateTime.UtcNow;
        await _repo.UpsertAsync(existing, ct);
        return Result<Usuario>.Success(existing);
    }
}
