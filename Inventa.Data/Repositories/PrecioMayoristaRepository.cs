using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

// S57.69: mayorista normalizado (relación dedicada, sin discriminator).
// Existe aquí = mayorista. IX_PrecioMayorista_Fast (IdPresProd, FecVig DESC).
public interface IPrecioMayoristaRepository : IRepositorySync<PrecioMayorista>
{
    Result<PrecioMayorista?> GetVigente(long idPresProd, DateOnly? fecha = null);
    Result<IReadOnlyList<PrecioMayorista>> GetByPresentacion(long idPresProd);
    Result<IReadOnlyList<PrecioMayorista>> GetHistorial(long idPresProd);
    Result<bool> ExisteVigente(long idPresProd, DateOnly? fecha = null);
    Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null);
}

public class PrecioMayoristaRepository : RepositorySync<PrecioMayorista>, IPrecioMayoristaRepository
{
    public PrecioMayoristaRepository(InventaDbContext context) : base(context) { }

    public Result<PrecioMayorista?> GetVigente(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<PrecioMayorista?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.Now);
        try
        {
            return Result<PrecioMayorista?>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd
                             && p.Activo
                             && p.FecVig <= fechaRef)
                    .OrderByDescending(p => p.FecVig)
                    .FirstOrDefault());
        }
        catch (Exception ex)
        {
            return Result<PrecioMayorista?>.Failure(
                CreateError(ex, nameof(GetVigente))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha", fechaRef));
        }
    }

    public Result<IReadOnlyList<PrecioMayorista>> GetByPresentacion(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<PrecioMayorista>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<PrecioMayorista>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd && p.Activo)
                    .OrderByDescending(p => p.FecVig)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PrecioMayorista>>.Failure(
                CreateError(ex, nameof(GetByPresentacion))
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<PrecioMayorista>> GetHistorial(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<PrecioMayorista>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<PrecioMayorista>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd)
                    .OrderByDescending(p => p.FecVig)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PrecioMayorista>>.Failure(
                CreateError(ex, nameof(GetHistorial))
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<bool> ExisteVigente(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            return Result<bool>.Success(
                DbSet.Any(p => p.IdPresProd == idPresProd
                            && p.Activo
                            && p.FecVig <= fechaRef));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExisteVigente))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha", fechaRef));
        }
    }

    public Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            var valor = DbSet
                .Where(p => p.IdPresProd == idPresProd
                         && p.Activo
                         && p.FecVig <= fechaRef)
                .OrderByDescending(p => p.FecVig)
                .Select(p => (decimal?)p.Valor)
                .FirstOrDefault();

            return Result<decimal?>.Success(valor);
        }
        catch (Exception ex)
        {
            return Result<decimal?>.Failure(
                CreateError(ex, nameof(GetValorVigente))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha", fechaRef));
        }
    }
}
