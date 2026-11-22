using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IUsuarioTenantSyncService
{
    Task<Result> EnsureAsync(int idUsuario, int idTenant, bool activo, bool esPrincipal, CancellationToken ct = default);
}

public class UsuarioTenantSyncService : IUsuarioTenantSyncService
{
    private readonly IUsuarioTenantRepositoryAsync _repo;
    public UsuarioTenantSyncService(IUsuarioTenantRepositoryAsync repo) => _repo = repo;

    public async Task<Result> EnsureAsync(int idUsuario, int idTenant, bool activo, bool esPrincipal, CancellationToken ct = default)
    {
        var ut = new UsuarioTenant
        {
            IdUsuario = idUsuario,
            IdTenant = idTenant,
            Activo = activo,
            EsPrincipal = esPrincipal,
            FechaSync = DateTime.UtcNow
        };
        await _repo.UpsertAsync(ut, ct);
        return Result.Success();
    }
}
