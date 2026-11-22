using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S57.34: Consultas de Usuario/UsuarioTenant para backend (lectura mirror).
/// La sincronización vive en IUsuarioSyncService/IUsuarioTenantSyncService. Sin DbContext.
/// Mirror/cache únicamente — nunca autoridad de identidad (PassPlat API ONLY).
/// </summary>
public interface IUsuarioServiceAsync
{
    Task<Result<Usuario?>> GetByIdAsync(int idUsuario, CancellationToken ct = default);
    Task<Result<IReadOnlyList<UsuarioTenant>>> GetTenantsByUsuarioAsync(int idUsuario, CancellationToken ct = default);
}

public sealed class UsuarioServiceAsync : IUsuarioServiceAsync
{
    private readonly IUsuarioRepositoryAsync _usuarioRepo;
    private readonly IUsuarioTenantRepositoryAsync _utRepo;

    public UsuarioServiceAsync(IUsuarioRepositoryAsync usuarioRepo, IUsuarioTenantRepositoryAsync utRepo)
    {
        _usuarioRepo = usuarioRepo;
        _utRepo = utRepo;
    }

    public async Task<Result<Usuario?>> GetByIdAsync(int idUsuario, CancellationToken ct = default)
    {
        var u = await _usuarioRepo.GetByIdAsync(idUsuario, ct);
        if (u == null)
            return Result<Usuario?>.Failure(new Error("NOT_FOUND", "Usuario no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Usuario?>.Success(u);
    }

    public async Task<Result<IReadOnlyList<UsuarioTenant>>> GetTenantsByUsuarioAsync(int idUsuario, CancellationToken ct = default)
    {
        var list = await _utRepo.GetByUsuarioAsync(idUsuario, ct);
        return Result<IReadOnlyList<UsuarioTenant>>.Success(list);
    }
}
