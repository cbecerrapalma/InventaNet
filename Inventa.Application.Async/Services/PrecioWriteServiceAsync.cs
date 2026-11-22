using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

// S57.69: precio normal único (sin discriminator, sin guards).
// Mayorista vive en PrecioMayoristaServiceAsync.
public sealed record PrecioWriteResult(long IdPrecio, long IdPresProd, decimal Valor, DateOnly FecVig, bool Activo);

public interface IPrecioWriteServiceAsync
{
    Task<Result<PrecioWriteResult>> CreateAsync(long idPresProd, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default);
    Task<Result<PrecioWriteResult>> UpdateAsync(long idPrecio, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long idPrecio, CancellationToken ct = default);
    Task<Result<PrecioWriteResult>> DeactivateAsync(long idPrecio, CancellationToken ct = default);
    Task<Result<PrecioWriteResult>> ReactivateAsync(long idPrecio, CancellationToken ct = default);
}

public sealed class PrecioWriteServiceAsync : IPrecioWriteServiceAsync
{
    private readonly IPrecioRepositoryAsync _repo;
    public PrecioWriteServiceAsync(IPrecioRepositoryAsync repo) => _repo = repo;

    public async Task<Result<PrecioWriteResult>> CreateAsync(long idPresProd, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default)
    {
        if (idPresProd <= 0 || valor <= 0)
            return Result<PrecioWriteResult>.Failure(new Error("VALIDATION", "IdPresProd debe ser >0 y Valor >0").WithType(ErrorType.Validation));
        var e = await _repo.InsertAsync(new Precio { IdPresProd = idPresProd, Valor = valor, FecVig = fecVig, Activo = activo }, ct);
        return Result<PrecioWriteResult>.Success(ToResult(e));
    }

    public async Task<Result<PrecioWriteResult>> UpdateAsync(long idPrecio, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecio, ct);
        if (cur == null) return Result<PrecioWriteResult>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        if (valor <= 0)
            return Result<PrecioWriteResult>.Failure(new Error("VALIDATION", "Valor debe ser >0").WithType(ErrorType.Validation));
        cur.Valor = valor;
        cur.FecVig = fecVig;
        cur.Activo = activo;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioWriteResult>.Success(ToResult(e));
    }

    public async Task<Result<bool>> DeleteAsync(long idPrecio, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecio, ct);
        if (cur == null) return Result<bool>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        var ok = await _repo.DeleteAsync(idPrecio, ct);
        if (!ok) return Result<bool>.Failure(
            new Error("PRECIO_SAVE", "No se pudo eliminar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<bool>.Success(true);
    }

    public async Task<Result<PrecioWriteResult>> DeactivateAsync(long idPrecio, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecio, ct);
        if (cur == null) return Result<PrecioWriteResult>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        cur.Activo = false;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioWriteResult>.Success(ToResult(e));
    }

    public async Task<Result<PrecioWriteResult>> ReactivateAsync(long idPrecio, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecio, ct);
        if (cur == null) return Result<PrecioWriteResult>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        cur.Activo = true;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioWriteResult>.Success(ToResult(e));
    }

    private static PrecioWriteResult ToResult(Precio e) =>
        new(e.IdPrecio, e.IdPresProd, e.Valor, e.FecVig, e.Activo);
}
