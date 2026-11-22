using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IAtriValRepository : IRepositorySync<AtriVal>
{
    Result<IReadOnlyList<AtriVal>> GetByAtriDef(long idAtriDef, bool soloActivos = true);
    Result<AtriVal?> GetByCodigo(long idAtriDef, string codigo);
    Result<bool> ExistsByCodigo(long idAtriDef, string codigo);
    Result<bool> IsUsedByVariantes(long idAtriVal);
    Result<int> CountByAtriDef(long idAtriDef, bool soloActivos = true);
}

public class AtriValRepository : RepositorySync<AtriVal>, IAtriValRepository
{
    public AtriValRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<AtriVal>> GetByAtriDef(long idAtriDef, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<AtriVal>>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(v => v.IdAtriDef == idAtriDef);
            if (soloActivos) query = query.Where(v => v.Activo);
            return Result<IReadOnlyList<AtriVal>>.Success(
                query.OrderBy(v => v.Orden).ThenBy(v => v.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<AtriVal>>.Failure(
                CreateError(ex, nameof(GetByAtriDef)).WithDetail("IdAtriDef", idAtriDef));
        }
    }

    public Result<AtriVal?> GetByCodigo(long idAtriDef, string codigo)
    {
        if (Guard.Fail<AtriVal?>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } errId) return errId;
        if (Guard.Fail<AtriVal?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<AtriVal?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(v => v.IdAtriDef == idAtriDef && v.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<AtriVal?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdAtriDef", idAtriDef).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> ExistsByCodigo(long idAtriDef, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(v => v.IdAtriDef == idAtriDef && v.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdAtriDef", idAtriDef).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> IsUsedByVariantes(long idAtriVal)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idAtriVal), idAtriVal)) is { } err) return err;
        try
        {
            return Result<bool>.Success(
                Context.Set<VarAtri>().Any(va => va.IdAtriVal == idAtriVal));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(IsUsedByVariantes)).WithDetail("IdAtriVal", idAtriVal));
        }
    }

    public Result<int> CountByAtriDef(long idAtriDef, bool soloActivos = true)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idAtriDef), idAtriDef)) is { } err) return err;
        try
        {
            return Result<int>.Success(
                DbSet.Count(v => v.IdAtriDef == idAtriDef && (!soloActivos || v.Activo)));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByAtriDef)).WithDetail("IdAtriDef", idAtriDef));
        }
    }
}
