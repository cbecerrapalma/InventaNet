using CBP.Data.Abstractions;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

// S57.69: escritura sync mayorista (W-mayorista). Una operación = un
// SaveChanges (mismo contrato que PrecioWriteService). Sin guards de tipo:
// la relación ES la semántica.
public interface IPrecioMayoristaWriteService
{
    Result<PrecioMayoristaDto> Create(long idPresProd, decimal valor, DateOnly fecVig, bool activo);
    Result<PrecioMayoristaDto> Update(long idPrecioMayorista, decimal valor, DateOnly fecVig, bool activo);
    Result<bool> Delete(long idPrecioMayorista);
    Result<PrecioMayoristaDto> Deactivate(long idPrecioMayorista);
    Result<PrecioMayoristaDto> Reactivate(long idPrecioMayorista);
}

public sealed class PrecioMayoristaWriteService : IPrecioMayoristaWriteService
{
    private readonly IPrecioMayoristaRepository _repo;
    private readonly IUnitOfWorkSync _uow;

    public PrecioMayoristaWriteService(IPrecioMayoristaRepository repo, IUnitOfWorkSync uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public Result<PrecioMayoristaDto> Create(long idPresProd, decimal valor, DateOnly fecVig, bool activo)
    {
        if (idPresProd <= 0 || valor <= 0)
            return Result<PrecioMayoristaDto>.Failure(
                new Error("VALIDATION", "IdPresProd debe ser >0 y Valor >0.").WithType(ErrorType.Validation));
        var e = new PrecioMayorista { IdPresProd = idPresProd, Valor = valor, FecVig = fecVig, Activo = activo };
        var add = _repo.Add(e);
        if (add.IsFailure) return Result<PrecioMayoristaDto>.Failure(add.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio mayorista en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioMayoristaDto>.Success(ToDto(e));
    }

    public Result<PrecioMayoristaDto> Update(long idPrecioMayorista, decimal valor, DateOnly fecVig, bool activo)
    {
        var cur = _repo.GetById(idPrecioMayorista);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        if (valor <= 0)
            return Result<PrecioMayoristaDto>.Failure(
                new Error("VALIDATION", "Valor debe ser >0.").WithType(ErrorType.Validation));
        e.Valor = valor;
        e.FecVig = fecVig;
        e.Activo = activo;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioMayoristaDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio mayorista en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioMayoristaDto>.Success(ToDto(e));
    }

    public Result<bool> Delete(long idPrecioMayorista)
    {
        var cur = _repo.GetById(idPrecioMayorista);
        if (!cur.IsSuccess || cur.Value == null) return Result<bool>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound));
        var rem = _repo.Remove(cur.Value);
        if (rem.IsFailure) return Result<bool>.Failure(rem.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<bool>.Failure(
            new Error("PRECIO_SAVE", "No se pudo eliminar el precio mayorista en la base de datos.").WithType(ErrorType.Database));
        return Result<bool>.Success(true);
    }

    public Result<PrecioMayoristaDto> Deactivate(long idPrecioMayorista)
    {
        var cur = _repo.GetById(idPrecioMayorista);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        e.Activo = false;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioMayoristaDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio mayorista en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioMayoristaDto>.Success(ToDto(e));
    }

    public Result<PrecioMayoristaDto> Reactivate(long idPrecioMayorista)
    {
        var cur = _repo.GetById(idPrecioMayorista);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio mayorista.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        e.Activo = true;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioMayoristaDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioMayoristaDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio mayorista en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioMayoristaDto>.Success(ToDto(e));
    }

    private static PrecioMayoristaDto ToDto(PrecioMayorista e) =>
        new(e.IdPrecioMayorista, e.IdPresProd, e.Valor, e.FecVig, e.Activo);
}
