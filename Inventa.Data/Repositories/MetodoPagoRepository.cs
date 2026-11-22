using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IMetodoPagoRepository : IRepositorySync<MetodoPago>
{
    Result<IReadOnlyList<MetodoPago>> GetActivos();
    Result<MetodoPago?> GetByCodigo(string codigo);
    Result<bool> ExistsByCodigo(string codigo);
}

public class MetodoPagoRepository : RepositorySync<MetodoPago>, IMetodoPagoRepository
{
    public MetodoPagoRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<MetodoPago>> GetActivos()
    {
        try
        {
            return Result<IReadOnlyList<MetodoPago>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.Activo)
                    .OrderBy(m => m.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MetodoPago>>.Failure(
                CreateError(ex, nameof(GetActivos)));
        }
    }

    public Result<MetodoPago?> GetByCodigo(string codigo)
    {
        if (Guard.Fail<MetodoPago?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<MetodoPago?>.Success(
                DbSet.AsNoTracking().FirstOrDefault(m => m.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<MetodoPago?>.Failure(
                CreateError(ex, nameof(GetByCodigo)).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> ExistsByCodigo(string codigo)
    {
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } err) return err;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(DbSet.Any(m => m.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo)).WithDetail("Codigo", codigo));
        }
    }
}
