using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface ICategoriaRepository : IRepositorySync<Categoria>
{
    Result<IReadOnlyList<Categoria>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<Categoria?> GetByCodigo(int idTenant, string codigo);
    Result<IReadOnlyList<Categoria>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountProductosByCategoria(long idCategoria);
}

public class CategoriaRepository : RepositorySync<Categoria>, ICategoriaRepository
{
    public CategoriaRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Categoria>> GetByTenant(int idTenant, bool soloActivas = true)
    {
        if (Guard.Fail<IReadOnlyList<Categoria>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(c => c.IdTenant == idTenant);
            if (soloActivas) query = query.Where(c => c.Activo);
            return Result<IReadOnlyList<Categoria>>.Success(
                query.OrderBy(c => c.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Categoria>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<Categoria?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Categoria?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Categoria?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<Categoria?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(c => c.IdTenant == idTenant && c.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Categoria?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<Categoria>> SearchByNombre(int idTenant, string nombre)
    {
        if (Guard.Fail<IReadOnlyList<Categoria>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Categoria>>(Guard.StringRequerido(nameof(nombre), nombre)) is { } errStr) return errStr;
        try
        {
            return Result<IReadOnlyList<Categoria>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdTenant == idTenant && c.Activo
                             && EF.Functions.Like(c.Nombre, $"%{nombre}%"))
                    .OrderBy(c => c.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Categoria>>.Failure(
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
                DbSet.Any(c => c.IdTenant == idTenant && c.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<int> CountProductosByCategoria(long idCategoria)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } err) return err;
        try
        {
            return Result<int>.Success(
                Context.Set<Prod>().Count(p => p.IdCategoria == idCategoria && p.Activo));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountProductosByCategoria))
                    .WithDetail("IdCategoria", idCategoria));
        }
    }
}
