using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface ITenantRepository : IRepositorySync<Tenant>
{
    Result<Tenant?> GetByCodigo(string codigo);
    Result<IReadOnlyList<Tenant>> GetActivos();
    Result<bool> ExistsByCodigo(string codigo);
    Result<int> CountActivos();
}

public class TenantRepository : RepositorySync<Tenant>, ITenantRepository
{
    public TenantRepository(InventaDbContext context) : base(context) { }

    public Result<Tenant?> GetByCodigo(string codigo)
    {
        if (Guard.Fail<Tenant?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<Tenant?>.Success(
                DbSet.AsNoTracking().FirstOrDefault(t => t.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Tenant?>.Failure(
                CreateError(ex, nameof(GetByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<Tenant>> GetActivos()
    {
        try
        {
            return Result<IReadOnlyList<Tenant>>.Success(
                DbSet.AsNoTracking()
                    .Where(t => t.Activo)
                    .OrderBy(t => t.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Tenant>>.Failure(
                CreateError(ex, nameof(GetActivos)));
        }
    }

    public Result<bool> ExistsByCodigo(string codigo)
    {
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(DbSet.Any(t => t.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<int> CountActivos()
    {
        try
        {
            return Result<int>.Success(DbSet.Count(t => t.Activo));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(CreateError(ex, nameof(CountActivos)));
        }
    }
}
