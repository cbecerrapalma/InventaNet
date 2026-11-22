using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

/// <summary>
/// Resumen de pagos de una venta agrupado por método de pago.
/// Evita cargar las entidades completas cuando solo se necesita el total por método.
/// </summary>
public sealed record ResumenPagoVenta(
    byte    IdMetodoPago,
    string  NombreMetodo,
    decimal TotalMonto);

public interface IVentaPagoRepository : IRepositorySync<VentaPago>
{
    Result<IReadOnlyList<VentaPago>> GetByVenta(long idVenta);
    Result<decimal> GetTotalPagado(long idVenta);
    Result<IReadOnlyList<VentaPago>> GetByMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta);
    Result<decimal> GetTotalPorMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta);

    /// <summary>
    /// Resumen de pagos de una venta agrupado por método de pago con nombre incluido.
    /// Sin cargar los registros completos — útil para el ticket de venta y cuadre de caja.
    /// </summary>
    Result<IReadOnlyList<ResumenPagoVenta>> GetResumenByVenta(long idVenta);
}

public class VentaPagoRepository : RepositorySync<VentaPago>, IVentaPagoRepository
{
    public VentaPagoRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<VentaPago>> GetByVenta(long idVenta)
    {
        if (Guard.Fail<IReadOnlyList<VentaPago>>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            // IX_VentaPago_Venta (IdVenta)
            return Result<IReadOnlyList<VentaPago>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdVenta == idVenta)
                    .Include(p => p.MetodoPago)
                    .OrderBy(p => p.IdVentaPago)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VentaPago>>.Failure(
                CreateError(ex, nameof(GetByVenta)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<decimal> GetTotalPagado(long idVenta)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            return Result<decimal>.Success(
                DbSet.Where(p => p.IdVenta == idVenta).Sum(p => p.Monto));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetTotalPagado)).WithDetail("IdVenta", idVenta));
        }
    }

    public Result<IReadOnlyList<VentaPago>> GetByMetodoPago(
        int idTenant, int idMetodoPago, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<VentaPago>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<VentaPago>>(Guard.IdInvalido(nameof(idMetodoPago), idMetodoPago)) is { } errM) return errM;
        if (Guard.Fail<IReadOnlyList<VentaPago>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            return Result<IReadOnlyList<VentaPago>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.Venta.IdTenant    == idTenant
                             && p.IdMetodoPago       == idMetodoPago
                             && p.Venta.FecVenta     >= desde
                             && p.Venta.FecVenta     <= hasta)
                    .Include(p => p.Venta)
                    .OrderByDescending(p => p.Venta.FecVenta)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VentaPago>>.Failure(
                CreateError(ex, nameof(GetByMetodoPago))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("IdMetodoPago", idMetodoPago));
        }
    }

    public Result<decimal> GetTotalPorMetodoPago(
        int idTenant, int idMetodoPago, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idMetodoPago), idMetodoPago)) is { } errM) return errM;
        if (Guard.Fail<decimal>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        try
        {
            return Result<decimal>.Success(
                DbSet.Where(p => p.Venta.IdTenant    == idTenant
                              && p.IdMetodoPago       == idMetodoPago
                              && p.Venta.FecVenta     >= desde
                              && p.Venta.FecVenta     <= hasta)
                     .Sum(p => p.Monto));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetTotalPorMetodoPago))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("IdMetodoPago", idMetodoPago));
        }
    }

    public Result<IReadOnlyList<ResumenPagoVenta>> GetResumenByVenta(long idVenta)
    {
        if (Guard.Fail<IReadOnlyList<ResumenPagoVenta>>(Guard.IdInvalido(nameof(idVenta), idVenta)) is { } err) return err;
        try
        {
            // Proyección directa — sin cargar entidades completas
            // JOIN con MetodoPago para obtener el nombre en una sola query
            var resumen = DbSet.AsNoTracking()
                .Where(p => p.IdVenta == idVenta)
                .GroupBy(p => new { p.IdMetodoPago, p.MetodoPago.Nombre })
                .Select(g => new ResumenPagoVenta(
                    g.Key.IdMetodoPago,
                    g.Key.Nombre,
                    g.Sum(p => p.Monto)))
                .OrderBy(r => r.IdMetodoPago)
                .ToList();
            return Result<IReadOnlyList<ResumenPagoVenta>>.Success(resumen.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ResumenPagoVenta>>.Failure(
                CreateError(ex, nameof(GetResumenByVenta)).WithDetail("IdVenta", idVenta));
        }
    }
}
