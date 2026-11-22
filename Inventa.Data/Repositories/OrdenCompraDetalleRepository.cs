using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IOrdenCompraDetalleRepository : IRepositorySync<OrdenCompraDetalle>
{
    Result<IReadOnlyList<OrdenCompraDetalle>> GetByOrden(long idOrdenCompra);
    Result<OrdenCompraDetalle?> GetByOrdenYPresentacion(long idOrdenCompra, long idPresProd);
    Result<IReadOnlyList<OrdenCompraDetalle>> GetByPresentacion(long idPresProd);
    /// <summary>Detalles en órdenes Pendiente/Aprobada — útil para calcular stock en tránsito.</summary>
    Result<IReadOnlyList<OrdenCompraDetalle>> GetPendientesByPresentacion(long idPresProd);
    /// <summary>Suma de cantidades en órdenes activas para una presentación (stock en tránsito).</summary>
    Result<decimal> GetCantidadPendiente(long idPresProd);
    Result<decimal> GetSubtotalOrden(long idOrdenCompra);
    Result<int> CountLineasByOrden(long idOrdenCompra);
    Result<bool> ExistsInOrden(long idOrdenCompra, long idPresProd);
}

public class OrdenCompraDetalleRepository : RepositorySync<OrdenCompraDetalle>, IOrdenCompraDetalleRepository
{
    public OrdenCompraDetalleRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<OrdenCompraDetalle>> GetByOrden(long idOrdenCompra)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompraDetalle>>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(
                DbSet.AsNoTracking()
                    .Where(d => d.IdOrdenCompra == idOrdenCompra)
                    .Include(d => d.PresProd)
                        .ThenInclude(p => p.VarProd)
                            .ThenInclude(v => v.Prod)
                    .Include(d => d.PresProd.Unidad)
                    .OrderBy(d => d.IdOCDetalle)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Failure(
                CreateError(ex, nameof(GetByOrden)).WithDetail("IdOrdenCompra", idOrdenCompra));
        }
    }

    public Result<OrdenCompraDetalle?> GetByOrdenYPresentacion(long idOrdenCompra, long idPresProd)
    {
        if (Guard.Fail<OrdenCompraDetalle?>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } errO) return errO;
        if (Guard.Fail<OrdenCompraDetalle?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        try
        {
            return Result<OrdenCompraDetalle?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(d => d.IdOrdenCompra == idOrdenCompra && d.IdPresProd == idPresProd));
        }
        catch (Exception ex)
        {
            return Result<OrdenCompraDetalle?>.Failure(
                CreateError(ex, nameof(GetByOrdenYPresentacion))
                    .WithDetail("IdOrdenCompra", idOrdenCompra).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<OrdenCompraDetalle>> GetByPresentacion(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompraDetalle>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(
                DbSet.AsNoTracking()
                    .Where(d => d.IdPresProd == idPresProd)
                    .Include(d => d.OrdenCompra)
                    .OrderByDescending(d => d.OrdenCompra.FecOrden)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Failure(
                CreateError(ex, nameof(GetByPresentacion)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<OrdenCompraDetalle>> GetPendientesByPresentacion(long idPresProd)
    {
        if (Guard.Fail<IReadOnlyList<OrdenCompraDetalle>>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Success(
                DbSet.AsNoTracking()
                    .Where(d => d.IdPresProd == idPresProd
                             && (d.OrdenCompra.Estado == 1 || d.OrdenCompra.Estado == 2))
                    .Include(d => d.OrdenCompra)
                        .ThenInclude(o => o.Proveedor)
                    .OrderByDescending(d => d.OrdenCompra.FecOrden)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<OrdenCompraDetalle>>.Failure(
                CreateError(ex, nameof(GetPendientesByPresentacion)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<decimal> GetCantidadPendiente(long idPresProd)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        try
        {
            return Result<decimal>.Success(
                DbSet.Where(d => d.IdPresProd == idPresProd
                              && (d.OrdenCompra.Estado == 1 || d.OrdenCompra.Estado == 2))
                     .Sum(d => d.Cantidad));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetCantidadPendiente)).WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<decimal> GetSubtotalOrden(long idOrdenCompra)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } err) return err;
        try
        {
            return Result<decimal>.Success(
                DbSet.Where(d => d.IdOrdenCompra == idOrdenCompra).Sum(d => d.Subtotal));
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetSubtotalOrden)).WithDetail("IdOrdenCompra", idOrdenCompra));
        }
    }

    public Result<int> CountLineasByOrden(long idOrdenCompra)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } err) return err;
        try
        {
            return Result<int>.Success(DbSet.Count(d => d.IdOrdenCompra == idOrdenCompra));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountLineasByOrden)).WithDetail("IdOrdenCompra", idOrdenCompra));
        }
    }

    public Result<bool> ExistsInOrden(long idOrdenCompra, long idPresProd)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idOrdenCompra), idOrdenCompra)) is { } errO) return errO;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(d => d.IdOrdenCompra == idOrdenCompra && d.IdPresProd == idPresProd));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsInOrden))
                    .WithDetail("IdOrdenCompra", idOrdenCompra).WithDetail("IdPresProd", idPresProd));
        }
    }
}
