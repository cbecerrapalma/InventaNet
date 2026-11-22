using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IBodegaRepository : IRepositorySync<Bodega>
{
    Result<IReadOnlyList<Bodega>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<Bodega?> GetByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> IsActiva(int idTenant, int idBodega);
}

public class BodegaRepository : RepositorySync<Bodega>, IBodegaRepository
{
    public BodegaRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Bodega>> GetByTenant(int idTenant, bool soloActivas = true)
    {
        if (Guard.Fail<IReadOnlyList<Bodega>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(b => b.IdTenant == idTenant);
            if (soloActivas) query = query.Where(b => b.Activo);
            return Result<IReadOnlyList<Bodega>>.Success(
                query.OrderBy(b => b.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Bodega>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<Bodega?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Bodega?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Bodega?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<Bodega?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(b => b.IdTenant == idTenant && b.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Bodega?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> ExistsByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(b => b.IdTenant == idTenant && b.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> IsActiva(int idTenant, int idBodega)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(b => b.IdTenant == idTenant && b.IdBodega == idBodega && b.Activo));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(IsActiva))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }
}
