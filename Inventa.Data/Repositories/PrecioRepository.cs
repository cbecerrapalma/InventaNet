using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IPrecioRepository : IRepositorySync<Precio>
{
    /// <summary>
    /// Retorna el precio normal vigente para una presentación.
    /// El índice IX_Precio_Fast (IdPresProd, FecVig DESC) cubre esta consulta.
    /// S57.69: sin discriminator (mayorista vive en PrecioMayorista).
    /// </summary>
    Result<Precio?> GetVigente(long idPresProd, DateOnly? fecha = null);

    /// <summary>Retorna todos los precios normales activos de una presentación.</summary>
    Result<IReadOnlyList<Precio>> GetByPresentacion(long idPresProd);

    Result<IReadOnlyList<Precio>> GetHistorial(long idPresProd);

    /// <summary>Verifica si existe al menos un precio normal vigente.</summary>
    Result<bool> ExisteVigente(long idPresProd, DateOnly? fecha = null);

    /// <summary>
    /// Retorna el valor del precio normal vigente directamente, sin cargar la entidad completa.
    /// Útil en punto de venta donde solo se necesita el decimal.
    /// </summary>
    Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null);
}

public class PrecioRepository : RepositorySync<Precio>, IPrecioRepository
{
    public PrecioRepository(InventaDbContext context) : base(context) { }

    public Result<Precio?> GetVigente(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<Precio?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.Now);
        try
        {
            // IX_Precio_Fast (IdPresProd, FecVig DESC) — seek preciso + top 1
            return Result<Precio?>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd
                             && p.Activo
                             && p.FecVig     <= fechaRef)
                    .OrderByDescending(p => p.FecVig)
                    .FirstOrDefault());
        }
        catch (Exception ex)
        {
            return Result<Precio?>.Failure(
                CreateError(ex, nameof(GetVigente))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha",      fechaRef));
        }
    }

    public Result<IReadOnlyList<Precio>> GetByPresentacion(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<Precio>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Precio>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd && p.Activo)
                    .OrderByDescending(p => p.FecVig)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Precio>>.Failure(
                CreateError(ex, nameof(GetByPresentacion))
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<Precio>> GetHistorial(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<Precio>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Precio>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdPresProd == idPresProd)
                    .OrderByDescending(p => p.FecVig)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Precio>>.Failure(
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
                            && p.FecVig     <= fechaRef));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExisteVigente))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha",      fechaRef));
        }
    }

    public Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            // Proyección directa al campo Valor — evita cargar la entidad completa.
            // IX_Precio_Fast (IdPresProd, FecVig DESC) — cubriente para este selector.
            var valor = DbSet
                .Where(p => p.IdPresProd == idPresProd
                         && p.Activo
                         && p.FecVig     <= fechaRef)
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
                    .WithDetail("Fecha",      fechaRef));
        }
    }
}
