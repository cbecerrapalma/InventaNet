using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IVarAtriRepository : IRepositorySync<VarAtri>
{
    Result<IReadOnlyList<VarAtri>> GetByVariante(long idVarProd);
    Result<IReadOnlyList<VarAtri>> GetByAtriVal(long idAtriVal);
    Result<VarAtri?> Get(long idVarProd, long idAtriVal);
    Result<bool> Exists(long idVarProd, long idAtriVal);
    Result<IReadOnlyList<VarAtri>> GetByVarianteConDetalle(long idVarProd);
    Result<IReadOnlyList<long>> GetVarianteIdsByAtriVal(long idAtriVal);
    Result<int> CountByVariante(long idVarProd);
    /// <summary>
    /// Verifica en una sola consulta que la variante cubra todos los AtriDef
    /// obligatorios de la categoría indicada.
    /// </summary>
    Result<bool> VarianteCumpleAtributosObligatorios(long idVarProd, long idCategoria);
}

public class VarAtriRepository : RepositorySync<VarAtri>, IVarAtriRepository
{
    public VarAtriRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<VarAtri>> GetByVariante(long idVarProd)
    {
        if (Guard.Fail<IReadOnlyList<VarAtri>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<VarAtri>>.Success(
                DbSet.AsNoTracking().Where(va => va.IdVarProd == idVarProd).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarAtri>>.Failure(
                CreateError(ex, nameof(GetByVariante)).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<IReadOnlyList<VarAtri>> GetByAtriVal(long idAtriVal)
    {
        if (Guard.Fail<IReadOnlyList<VarAtri>>(Guard.IdInvalido(nameof(idAtriVal), idAtriVal)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<VarAtri>>.Success(
                DbSet.AsNoTracking().Where(va => va.IdAtriVal == idAtriVal).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarAtri>>.Failure(
                CreateError(ex, nameof(GetByAtriVal)).WithDetail("IdAtriVal", idAtriVal));
        }
    }

    public Result<VarAtri?> Get(long idVarProd, long idAtriVal)
    {
        if (Guard.Fail<VarAtri?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errVP) return errVP;
        if (Guard.Fail<VarAtri?>(Guard.IdInvalido(nameof(idAtriVal), idAtriVal)) is { } errAV) return errAV;
        try
        {
            return Result<VarAtri?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(va => va.IdVarProd == idVarProd && va.IdAtriVal == idAtriVal));
        }
        catch (Exception ex)
        {
            return Result<VarAtri?>.Failure(
                CreateError(ex, nameof(Get))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdAtriVal", idAtriVal));
        }
    }

    public Result<bool> Exists(long idVarProd, long idAtriVal)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errVP) return errVP;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idAtriVal), idAtriVal)) is { } errAV) return errAV;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(va => va.IdVarProd == idVarProd && va.IdAtriVal == idAtriVal));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(Exists))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdAtriVal", idAtriVal));
        }
    }

    public Result<IReadOnlyList<VarAtri>> GetByVarianteConDetalle(long idVarProd)
    {
        if (Guard.Fail<IReadOnlyList<VarAtri>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<VarAtri>>.Success(
                DbSet.AsNoTracking()
                    .Where(va => va.IdVarProd == idVarProd)
                    .Include(va => va.AtriVal)
                        .ThenInclude(av => av.AtriDef)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarAtri>>.Failure(
                CreateError(ex, nameof(GetByVarianteConDetalle)).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<IReadOnlyList<long>> GetVarianteIdsByAtriVal(long idAtriVal)
    {
        if (Guard.Fail<IReadOnlyList<long>>(Guard.IdInvalido(nameof(idAtriVal), idAtriVal)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<long>>.Success(
                DbSet.AsNoTracking()
                    .Where(va => va.IdAtriVal == idAtriVal)
                    .Select(va => va.IdVarProd)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<long>>.Failure(
                CreateError(ex, nameof(GetVarianteIdsByAtriVal)).WithDetail("IdAtriVal", idAtriVal));
        }
    }

    public Result<int> CountByVariante(long idVarProd)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            return Result<int>.Success(DbSet.Count(va => va.IdVarProd == idVarProd));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByVariante)).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<bool> VarianteCumpleAtributosObligatorios(long idVarProd, long idCategoria)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errVP) return errVP;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } errCat) return errCat;
        try
        {
            var obligatorios = Context.Set<AtriDef>()
                .Where(a => a.IdCategoria == idCategoria && a.Activo && a.EsOblig)
                .Select(a => a.IdAtriDef)
                .ToList();

            if (obligatorios.Count == 0) return Result<bool>.Success(true);

            var cubiertos = DbSet
                .Where(va => va.IdVarProd == idVarProd)
                .Join(Context.Set<AtriVal>(),
                    va => va.IdAtriVal, av => av.IdAtriVal,
                    (va, av) => av.IdAtriDef)
                .Distinct()
                .ToList();

            return Result<bool>.Success(obligatorios.All(id => cubiertos.Contains(id)));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(VarianteCumpleAtributosObligatorios))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdCategoria", idCategoria));
        }
    }
}
