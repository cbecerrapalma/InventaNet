using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IAtriDefRepository : IRepositorySync<AtriDef>
{
    Result<IReadOnlyList<AtriDef>> GetByCategoria(long idCategoria, bool soloActivos = true);
    Result<AtriDef?> GetByCodigo(long idCategoria, string codigo);
    Result<AtriDef?> GetWithValores(long idAtriDef);
    Result<IReadOnlyList<AtriDef>> GetObligatoriosByCategoria(long idCategoria);
    Result<bool> ExistsByCodigo(long idCategoria, string codigo);
    Result<bool> TieneValores(long idAtriDef);
    Result<int> CountByCategoria(long idCategoria, bool soloActivos = true);
}

public class AtriDefRepository : RepositorySync<AtriDef>, IAtriDefRepository
{
    public AtriDefRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<AtriDef>> GetByCategoria(long idCategoria, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<AtriDef>>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(a => a.IdCategoria == idCategoria);
            if (soloActivos) query = query.Where(a => a.Activo);
            return Result<IReadOnlyList<AtriDef>>.Success(
                query.OrderBy(a => a.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<AtriDef>>.Failure(
                CreateError(ex, nameof(GetByCategoria)).WithDetail("IdCategoria", idCategoria));
        }
    }

    public Result<AtriDef?> GetByCodigo(long idCategoria, string codigo)
    {
        if (Guard.Fail<AtriDef?>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } errId) return errId;
        if (Guard.Fail<AtriDef?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<AtriDef?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(a => a.IdCategoria == idCategoria && a.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<AtriDef?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdCategoria", idCategoria).WithDetail("Codigo", codigo));
        }
    }

    public Result<AtriDef?> GetWithValores(long idAtriDef)
    {
        if (Guard.Fail<AtriDef?>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } err) return err;
        try
        {
            return Result<AtriDef?>.Success(
                DbSet.AsNoTracking()
                    .Include(a => a.Valores.Where(v => v.Activo).OrderBy(v => v.Orden).ThenBy(v => v.Nombre))
                    .FirstOrDefault(a => a.IdAtriDef == idAtriDef));
        }
        catch (Exception ex)
        {
            return Result<AtriDef?>.Failure(
                CreateError(ex, nameof(GetWithValores)).WithDetail("IdAtriDef", idAtriDef));
        }
    }

    public Result<IReadOnlyList<AtriDef>> GetObligatoriosByCategoria(long idCategoria)
    {
        if (Guard.Fail<IReadOnlyList<AtriDef>>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<AtriDef>>.Success(
                DbSet.AsNoTracking()
                    .Where(a => a.IdCategoria == idCategoria && a.Activo && a.EsOblig)
                    .Include(a => a.Valores.Where(v => v.Activo).OrderBy(v => v.Orden))
                    .OrderBy(a => a.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<AtriDef>>.Failure(
                CreateError(ex, nameof(GetObligatoriosByCategoria))
                    .WithDetail("IdCategoria", idCategoria));
        }
    }

    public Result<bool> ExistsByCodigo(long idCategoria, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(a => a.IdCategoria == idCategoria && a.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdCategoria", idCategoria).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> TieneValores(long idAtriDef)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } err) return err;
        try
        {
            return Result<bool>.Success(
                Context.Set<AtriVal>().Any(v => v.IdAtriDef == idAtriDef && v.Activo));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(TieneValores)).WithDetail("IdAtriDef", idAtriDef));
        }
    }

    public Result<int> CountByCategoria(long idCategoria, bool soloActivos = true)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } err) return err;
        try
        {
            return Result<int>.Success(
                DbSet.Count(a => a.IdCategoria == idCategoria && (!soloActivos || a.Activo)));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByCategoria)).WithDetail("IdCategoria", idCategoria));
        }
    }
}
