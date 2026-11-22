using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IUnidadRepository : IRepositorySync<Unidad>
{
    Result<Unidad?> GetByCodigo(string codigo);
    Result<IReadOnlyList<Unidad>> GetAllOrdenado();
    Result<bool> ExistsByCodigo(string codigo);
    Result<bool> IsUsedByProductos(int idUnidad);
}

public class UnidadRepository : RepositorySync<Unidad>, IUnidadRepository
{
    public UnidadRepository(InventaDbContext context) : base(context) { }

    public Result<Unidad?> GetByCodigo(string codigo)
    {
        if (Guard.Fail<Unidad?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<Unidad?>.Success(
                DbSet.AsNoTracking().FirstOrDefault(u => u.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Unidad?>.Failure(
                CreateError(ex, nameof(GetByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<Unidad>> GetAllOrdenado()
    {
        try
        {
            return Result<IReadOnlyList<Unidad>>.Success(
                DbSet.AsNoTracking().OrderBy(u => u.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Unidad>>.Failure(
                CreateError(ex, nameof(GetAllOrdenado)));
        }
    }

    public Result<bool> ExistsByCodigo(string codigo)
    {
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(DbSet.Any(u => u.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> IsUsedByProductos(int idUnidad)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } err) return err;
        try
        {
            var isUsed = Context.Set<Prod>().Any(p => p.IdUniBase == idUnidad)
                      || Context.Set<ProdConv>().Any(c => c.IdUnidad == idUnidad)
                      || Context.Set<PresProd>().Any(p => p.IdUnidad == idUnidad);
            return Result<bool>.Success(isUsed);
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(IsUsedByProductos)).WithDetail("IdUnidad", idUnidad));
        }
    }
}
