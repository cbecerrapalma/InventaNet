using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IMarcaRepository : IRepositorySync<Marca>
{
    Result<IReadOnlyList<Marca>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<Marca?> GetByCodigo(int idTenant, string codigo);
    Result<IReadOnlyList<Marca>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountByTenant(int idTenant, bool soloActivas = true);
}

public class MarcaRepository : RepositorySync<Marca>, IMarcaRepository
{
    public MarcaRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Marca>> GetByTenant(int idTenant, bool soloActivas = true)
    {
        if (Guard.Fail<IReadOnlyList<Marca>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(m => m.IdTenant == idTenant);
            if (soloActivas) query = query.Where(m => m.Activo);
            return Result<IReadOnlyList<Marca>>.Success(
                query.OrderBy(m => m.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Marca>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<Marca?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Marca?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Marca?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<Marca?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(m => m.IdTenant == idTenant && m.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Marca?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<Marca>> SearchByNombre(int idTenant, string nombre)
    {
        if (Guard.Fail<IReadOnlyList<Marca>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Marca>>(Guard.StringRequerido(nameof(nombre), nombre)) is { } errStr) return errStr;
        try
        {
            return Result<IReadOnlyList<Marca>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant == idTenant && m.Activo
                             && EF.Functions.Like(m.Nombre, $"%{nombre}%"))
                    .OrderBy(m => m.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Marca>>.Failure(
                CreateError(ex, nameof(SearchByNombre))
                    .WithDetail("IdTenant", idTenant).WithDetail("Nombre", nombre));
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
                DbSet.Any(m => m.IdTenant == idTenant && m.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<int> CountByTenant(int idTenant, bool soloActivas = true)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<int>.Success(
                DbSet.Count(m => m.IdTenant == idTenant && (!soloActivas || m.Activo)));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByTenant)).WithDetail("IdTenant", idTenant));
        }
    }
}
