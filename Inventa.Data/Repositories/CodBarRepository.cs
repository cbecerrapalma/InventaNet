using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface ICodBarRepository : IRepositorySync<CodBar>
{
    Result<CodBar?> GetByCodigo(string codigo, int idTenant);
    Result<CodBar?> GetByCodigoConPresentacion(string codigo, int idTenant);
    Result<IReadOnlyList<CodBar>> GetByPresentacion(long idPresProd);
    Result<CodBar?> GetPrincipalByPresentacion(long idPresProd);
    Result<bool> ExistsByCodigo(string codigo);
    Result<IReadOnlyList<CodBar>> GetByTipo(long idPresProd, byte tipoCod);
    Result<IReadOnlyList<CodBar>> GetByTenant(int idTenant);
}

public class CodBarRepository : RepositorySync<CodBar>, ICodBarRepository
{
    public CodBarRepository(InventaDbContext context) : base(context) { }

    public Result<CodBar?> GetByCodigo(string codigo, int idTenant)
    {
        if (Guard.Fail<CodBar?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        if (Guard.Fail<CodBar?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        try
        {
            // IX_CodBar_Fast (Codigo, IdPresProd) — seek puntual, latencia crítica POS
            // UQ_CBar (Codigo) garantiza unicidad global
            return Result<CodBar?>.Success(
                DbSet.AsNoTracking()
                    .Include(c => c.PresProd)
                    .FirstOrDefault(c => c.Codigo == codigo && c.PresProd.VarProd.Prod.IdTenant == idTenant));
        }
        catch (Exception ex)
        {
            return Result<CodBar?>.Failure(
                CreateError(ex, nameof(GetByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<CodBar>> GetByPresentacion(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<CodBar>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<CodBar>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdPresProd == idPresProd)
                    // Principal primero, luego por tipo
                    .OrderByDescending(c => c.Principal)
                    .ThenBy(c => c.TipoCod)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CodBar>>.Failure(
                CreateError(ex, nameof(GetByPresentacion)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<CodBar?> GetPrincipalByPresentacion(long idPresProd)
    {
        if (Guard.Fail<CodBar?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<CodBar?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(c => c.IdPresProd == idPresProd && c.Principal));
        }
        catch (Exception ex)
        {
            return Result<CodBar?>.Failure(
                CreateError(ex, nameof(GetPrincipalByPresentacion)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<bool> ExistsByCodigo(string codigo)
    {
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            // UQ_CBar (Codigo) — seek de punto único sin escanear filas
            return Result<bool>.Success(DbSet.Any(c => c.Codigo == codigo));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<CodBar>> GetByTipo(long idPresProd, byte tipoCod)
    {
        if (Guard.Fail<IReadOnlyList<CodBar>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<CodBar>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdPresProd == idPresProd && c.TipoCod == tipoCod)
                    .OrderBy(c => c.Codigo)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CodBar>>.Failure(
                CreateError(ex, nameof(GetByTipo))
                    .WithDetail("IdPresProd", idPresProd).WithDetail("TipoCod", tipoCod));
        }
    }

    public Result<IReadOnlyList<CodBar>> GetByTenant(int idTenant)
    {
        try
        {
            return Result<IReadOnlyList<CodBar>>.Success(
                DbSet.AsNoTracking()
                    .Include(c => c.PresProd)
                        .ThenInclude(p => p.VarProd)
                            .ThenInclude(v => v.Prod)
                    .Where(c => c.PresProd.VarProd.Prod.IdTenant == idTenant)
                    .OrderBy(c => c.Codigo)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<CodBar>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<CodBar?> GetByCodigoConPresentacion(string codigo, int idTenant)
    {
        if (Guard.Fail<CodBar?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        if (Guard.Fail<CodBar?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        try
        {
            // IX_CodBar_Fast (Codigo, IdPresProd) — seek; luego join con PresProd/VarProd/Unidad
            return Result<CodBar?>.Success(
                DbSet.AsNoTracking()
                    .Include(c => c.PresProd)
                        .ThenInclude(p => p.VarProd)
                            .ThenInclude(v => v.Prod)
                    .Include(c => c.PresProd.Unidad)
                    .FirstOrDefault(c => c.Codigo == codigo && c.PresProd.VarProd.Prod.IdTenant == idTenant));
        }
        catch (Exception ex)
        {
            return Result<CodBar?>.Failure(
                CreateError(ex, nameof(GetByCodigoConPresentacion)).WithDetail("Codigo", codigo));
        }
    }
}
