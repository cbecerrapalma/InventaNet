using CBP.Data.Abstractions;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

// S57.69: precio normal único (sin discriminator, sin guards de tipo).
// Mayorista vive en PrecioMayoristaWriteService.
public interface IPrecioWriteService
{
    Result<PrecioDto> Create(long idPresProd, decimal valor, DateOnly fecVig, bool activo);
    Result<PrecioDto> Update(long idPrecio, decimal valor, DateOnly fecVig, bool activo);
    Result<bool> Delete(long idPrecio);
    Result<PrecioDto> Deactivate(long idPrecio);
    Result<PrecioDto> Reactivate(long idPrecio);
}

public sealed class PrecioWriteService : IPrecioWriteService
{
    private readonly IPrecioRepository _repo;
    private readonly IUnitOfWorkSync _uow;

    public PrecioWriteService(IPrecioRepository repo, IUnitOfWorkSync uow)
    {
        _repo = repo;
        _uow = uow;
    }

    public Result<PrecioDto> Create(long idPresProd, decimal valor, DateOnly fecVig, bool activo)
    {
        if (idPresProd <= 0 || valor <= 0)
            return Result<PrecioDto>.Failure(
                new Error("VALIDATION", "IdPresProd debe ser >0 y Valor >0.").WithType(ErrorType.Validation));
        var e = new Precio { IdPresProd = idPresProd, Valor = valor, FecVig = fecVig, Activo = activo };
        var add = _repo.Add(e);
        if (add.IsFailure) return Result<PrecioDto>.Failure(add.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioDto>.Success(ToDto(e));
    }

    public Result<PrecioDto> Update(long idPrecio, decimal valor, DateOnly fecVig, bool activo)
    {
        var cur = _repo.GetById(idPrecio);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        if (valor <= 0)
            return Result<PrecioDto>.Failure(
                new Error("VALIDATION", "Valor debe ser >0.").WithType(ErrorType.Validation));
        e.Valor = valor;
        e.FecVig = fecVig;
        e.Activo = activo;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioDto>.Success(ToDto(e));
    }

    public Result<bool> Delete(long idPrecio)
    {
        var cur = _repo.GetById(idPrecio);
        if (!cur.IsSuccess || cur.Value == null) return Result<bool>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        var rem = _repo.Remove(e);
        if (rem.IsFailure) return Result<bool>.Failure(rem.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<bool>.Failure(
            new Error("PRECIO_SAVE", "No se pudo eliminar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<bool>.Success(true);
    }

    public Result<PrecioDto> Deactivate(long idPrecio)
    {
        var cur = _repo.GetById(idPrecio);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        e.Activo = false;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioDto>.Success(ToDto(e));
    }

    public Result<PrecioDto> Reactivate(long idPrecio)
    {
        var cur = _repo.GetById(idPrecio);
        if (!cur.IsSuccess || cur.Value == null) return Result<PrecioDto>.Failure(
            new Error("PRECIO_NOT_FOUND", "No se pudo encontrar el precio.").WithType(ErrorType.NotFound));
        var e = cur.Value;
        e.Activo = true;
        var upd = _repo.Update(e);
        if (upd.IsFailure) return Result<PrecioDto>.Failure(upd.Error!);
        var saved = _uow.SaveChanges();
        if (saved <= 0) return Result<PrecioDto>.Failure(
            new Error("PRECIO_SAVE", "No se pudo guardar el precio en la base de datos.").WithType(ErrorType.Database));
        return Result<PrecioDto>.Success(ToDto(e));
    }

    private static PrecioDto ToDto(Precio e) =>
        new(e.IdPrecio, e.IdPresProd, e.Valor, e.FecVig, e.Activo);
}
