using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

// S57.69: mayorista normalizado (relación dedicada, sin discriminator, sin guards de tipo).
public sealed record PrecioMayoristaResult(long IdPrecioMayorista, long IdPresProd, decimal Valor, DateOnly FecVig, bool Activo);

public interface IPrecioMayoristaServiceAsync
{
    Task<Result<PrecioMayorista?>> GetByIdAsync(long idPrecioMayorista, CancellationToken ct = default);
    Task<Result<PrecioMayorista?>> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PrecioMayorista>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PrecioMayorista>>> GetHistorialAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<bool>> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<decimal?>> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<PrecioMayoristaResult>> CreateAsync(long idPresProd, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default);
    Task<Result<PrecioMayoristaResult>> UpdateAsync(long idPrecioMayorista, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default);
    Task<Result<bool>> DeleteAsync(long idPrecioMayorista, CancellationToken ct = default);
    Task<Result<PrecioMayoristaResult>> DeactivateAsync(long idPrecioMayorista, CancellationToken ct = default);
    Task<Result<PrecioMayoristaResult>> ReactivateAsync(long idPrecioMayorista, CancellationToken ct = default);
}

public sealed class PrecioMayoristaServiceAsync : IPrecioMayoristaServiceAsync
{
    private readonly IPrecioMayoristaRepositoryAsync _repo;
    public PrecioMayoristaServiceAsync(IPrecioMayoristaRepositoryAsync repo) => _repo = repo;

    public async Task<Result<PrecioMayorista?>> GetByIdAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idPrecioMayorista, ct);
        return Result<PrecioMayorista?>.Success(e, allowNull: true);
    }

    public async Task<Result<PrecioMayorista?>> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var e = await _repo.GetVigenteAsync(idPresProd, fecha, ct);
        return Result<PrecioMayorista?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<PrecioMayorista>>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetByPresentacionAsync(idPresProd, ct);
        return Result<IReadOnlyList<PrecioMayorista>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<PrecioMayorista>>> GetHistorialAsync(long idPresProd, CancellationToken ct = default)
    {
        var list = await _repo.GetHistorialAsync(idPresProd, ct);
        return Result<IReadOnlyList<PrecioMayorista>>.Success(list);
    }

    public async Task<Result<bool>> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var ok = await _repo.ExisteVigenteAsync(idPresProd, fecha, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<decimal?>> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var val = await _repo.GetValorVigenteAsync(idPresProd, fecha, ct);
        return Result<decimal?>.Success(val, allowNull: true);
    }

    public async Task<Result<PrecioMayoristaResult>> CreateAsync(long idPresProd, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default)
    {
        if (idPresProd <= 0)
            return Result<PrecioMayoristaResult>.Failure(new Error("VALIDATION", "IdPresProd debe ser >0").WithType(ErrorType.Validation));
        if (valor <= 0)
            return Result<PrecioMayoristaResult>.Failure(new Error("VALIDATION", "Valor debe ser >0").WithType(ErrorType.Validation));
        var e = await _repo.InsertAsync(new PrecioMayorista { IdPresProd = idPresProd, Valor = valor, FecVig = fecVig, Activo = activo }, ct);
        return Result<PrecioMayoristaResult>.Success(ToResult(e));
    }

    public async Task<Result<PrecioMayoristaResult>> UpdateAsync(long idPrecioMayorista, decimal valor, DateOnly fecVig, bool activo, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecioMayorista, ct);
        if (cur == null) return Result<PrecioMayoristaResult>.Failure(
            new Error("PRECIO_MAYORISTA_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (valor <= 0)
            return Result<PrecioMayoristaResult>.Failure(new Error("VALIDATION", "Valor debe ser >0").WithType(ErrorType.Validation));
        cur.Valor = valor;
        cur.FecVig = fecVig;
        cur.Activo = activo;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioMayoristaResult>.Success(ToResult(e));
    }

    public async Task<Result<bool>> DeleteAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        var ok = await _repo.DeleteAsync(idPrecioMayorista, ct);
        if (!ok) return Result<bool>.Failure(
            new Error("PRECIO_MAYORISTA_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<bool>.Success(true);
    }

    public async Task<Result<PrecioMayoristaResult>> DeactivateAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecioMayorista, ct);
        if (cur == null) return Result<PrecioMayoristaResult>.Failure(
            new Error("PRECIO_MAYORISTA_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        cur.Activo = false;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioMayoristaResult>.Success(ToResult(e));
    }

    public async Task<Result<PrecioMayoristaResult>> ReactivateAsync(long idPrecioMayorista, CancellationToken ct = default)
    {
        var cur = await _repo.GetByIdAsync(idPrecioMayorista, ct);
        if (cur == null) return Result<PrecioMayoristaResult>.Failure(
            new Error("PRECIO_MAYORISTA_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        cur.Activo = true;
        var e = await _repo.UpdateAsync(cur, ct);
        return Result<PrecioMayoristaResult>.Success(ToResult(e));
    }

    private static PrecioMayoristaResult ToResult(PrecioMayorista e) =>
        new(e.IdPrecioMayorista, e.IdPresProd, e.Valor, e.FecVig, e.Activo);
}
