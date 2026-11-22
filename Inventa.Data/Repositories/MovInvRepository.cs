using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IMovInvRepository : IRepositorySync<MovInv>
{
    /// <summary>
    /// Movimientos de una presentación en el período.
    /// Usa IX_MovInv_Prod (IdTenant, IdPresProd, FecMov).
    /// </summary>
    Result<IReadOnlyList<MovInv>> GetByPresentacion(int idTenant, long idPresProd, DateTime desde, DateTime hasta);

    /// <summary>
    /// Movimientos de una bodega en el período.
    /// Usa UQ_MovInv_Cluster (IdTenant, FecMov, IdMovInv).
    /// </summary>
    Result<IReadOnlyList<MovInv>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta);

    /// <summary>
    /// Movimientos filtrados por tipo.
    /// TipoMov: 1=Compra, 2=Venta, 3=Ajuste Entrada, 4=Ajuste Salida,
    ///          5=Transferencia Entrada, 6=Transferencia Salida.
    /// </summary>
    Result<IReadOnlyList<MovInv>> GetByTipoMov(int idTenant, byte tipoMov, DateTime desde, DateTime hasta);

    /// <summary>Todos los movimientos de un lote específico.</summary>
    Result<IReadOnlyList<MovInv>> GetByLote(int idTenant, long idLote);

    /// <summary>
    /// Suma CantBase (en unidad base) filtrada. Útil para calcular rotación de stock.
    /// </summary>
    Result<decimal> GetSumaCantBase(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta);

    /// <summary>
    /// Últimos N movimientos de una bodega.
    /// ORDER BY FecMov DESC, IdMovInv DESC — alineado con UQ_MovInv_Cluster.
    /// </summary>
    Result<IReadOnlyList<MovInv>> GetUltimosMovimientos(int idTenant, int idBodega, int cantidad = 50);

    /// <summary>
    /// Kardex completo de una variante en todas sus bodegas para el período.
    /// Permite reconstruir el historial de entradas/salidas por variante.
    /// </summary>
    Result<IReadOnlyList<MovInv>> GetKardexVariante(int idTenant, long idVarProd, DateTime desde, DateTime hasta);
}

public class MovInvRepository : RepositorySync<MovInv>, IMovInvRepository
{
    private const int MaxUltimosMovimientos = 500;

    /// <summary>
    /// Extiende el límite superior al último instante del día.
    /// FecMov es DATETIME2 — sin esto, movimientos de las 14:00 quedan fuera
    /// cuando el caller pasa hasta = fecha.Date (que equivale a 00:00:00).
    /// </summary>
    private static DateTime FinDelDia(DateTime fecha) => fecha.Date.AddDays(1).AddTicks(-1);

    public MovInvRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<MovInv>> GetByPresentacion(
        int idTenant, long idPresProd, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        var hastaFin = FinDelDia(hasta);
        try
        {
            // IX_MovInv_Prod (IdTenant, IdPresProd, FecMov)
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant   == idTenant
                             && m.IdPresProd == idPresProd
                             && m.FecMov     >= desde
                             && m.FecMov     <= hastaFin)
                    .Include(m => m.Bodega)
                    .Include(m => m.Lote)
                    .OrderByDescending(m => m.FecMov)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetByPresentacion))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<MovInv>> GetByBodega(
        int idTenant, int idBodega, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        var hastaFin = FinDelDia(hasta);
        try
        {
            // UQ_MovInv_Cluster (IdTenant, FecMov, IdMovInv) — clustered
            // No existe índice por IdBodega; el clustered cubre IdTenant+FecMov.
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant == idTenant
                             && m.IdBodega == idBodega
                             && m.FecMov   >= desde
                             && m.FecMov   <= hastaFin)
                    .Include(m => m.PresProd)
                    .Include(m => m.Lote)
                    .OrderByDescending(m => m.FecMov)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetByBodega))
                    .WithDetail("IdTenant", idTenant)
                    .WithDetail("IdBodega", idBodega));
        }
    }

    public Result<IReadOnlyList<MovInv>> GetByTipoMov(
        int idTenant, byte tipoMov, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        // BUG FIX: validar que TipoMov esté en el rango 1-6
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.TipoMovInvalido(tipoMov)) is { } errMov) return errMov;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        var hastaFin = FinDelDia(hasta);
        try
        {
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant == idTenant
                             && m.TipoMov  == tipoMov
                             && m.FecMov   >= desde
                             && m.FecMov   <= hastaFin)
                    .Include(m => m.Bodega)
                    .Include(m => m.PresProd)
                    .OrderByDescending(m => m.FecMov)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetByTipoMov))
                    .WithDetail("IdTenant", idTenant)
                    .WithDetail("TipoMov",  tipoMov));
        }
    }

    public Result<IReadOnlyList<MovInv>> GetByLote(int idTenant, long idLote)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idLote), idLote)) is { } errL) return errL;
        try
        {
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant == idTenant && m.IdLote == idLote)
                    .Include(m => m.Bodega)
                    .Include(m => m.PresProd)
                    .OrderByDescending(m => m.FecMov)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetByLote))
                    .WithDetail("IdTenant", idTenant)
                    .WithDetail("IdLote",   idLote));
        }
    }

    public Result<decimal> GetSumaCantBase(
        int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<decimal>(Guard.TipoMovInvalido(tipoMov)) is { } errMov) return errMov;
        if (Guard.Fail<decimal>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        var hastaFin = FinDelDia(hasta);
        try
        {
            return Result<decimal>.Success(
                DbSet.Where(m => m.IdTenant   == idTenant
                              && m.IdPresProd == idPresProd
                              && m.IdBodega   == idBodega
                              && m.TipoMov    == tipoMov
                              && m.FecMov     >= desde
                              && m.FecMov     <= hastaFin)
                     .Sum(m => m.CantBase));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetSumaCantBase))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<MovInv>> GetUltimosMovimientos(
        int idTenant, int idBodega, int cantidad = 50)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.Rango(nameof(cantidad), cantidad, 1, MaxUltimosMovimientos)) is { } errC) return errC;
        try
        {
            // ORDER BY FecMov DESC, IdMovInv DESC — alineado con UQ_MovInv_Cluster; TOP N eficiente
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant == idTenant && m.IdBodega == idBodega)
                    .Include(m => m.PresProd)
                    .Include(m => m.Lote)
                    .OrderByDescending(m => m.FecMov)
                    .ThenByDescending(m => m.IdMovInv)
                    .Take(cantidad)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetUltimosMovimientos))
                    .WithDetail("IdTenant", idTenant)
                    .WithDetail("IdBodega", idBodega));
        }
    }

    public Result<IReadOnlyList<MovInv>> GetKardexVariante(
        int idTenant, long idVarProd, DateTime desde, DateTime hasta)
    {
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<IReadOnlyList<MovInv>>(Guard.RangoFechas(desde, hasta)) is { } errF) return errF;
        var hastaFin = FinDelDia(hasta);
        try
        {
            // Navega PresProd → VarProd para filtrar por IdVarProd.
            // IX_MovInv_Prod (IdTenant, IdPresProd, FecMov) — un seek por cada presentación.
            return Result<IReadOnlyList<MovInv>>.Success(
                DbSet.AsNoTracking()
                    .Where(m => m.IdTenant           == idTenant
                             && m.PresProd.IdVarProd == idVarProd
                             && m.FecMov             >= desde
                             && m.FecMov             <= hastaFin)
                    .Include(m => m.Bodega)
                    .Include(m => m.PresProd)
                    .Include(m => m.Lote)
                    .OrderByDescending(m => m.FecMov)
                    .ThenByDescending(m => m.IdMovInv)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<MovInv>>.Failure(
                CreateError(ex, nameof(GetKardexVariante))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdVarProd", idVarProd));
        }
    }
}
