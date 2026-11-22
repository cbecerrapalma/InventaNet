using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IProveedorRepository : IRepositorySync<Proveedor>
{
    Result<IReadOnlyList<Proveedor>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<Proveedor?> GetByCodigo(int idTenant, string codigo);
    Result<Proveedor?> GetByRUT(int idTenant, string rut);
    Result<IReadOnlyList<Proveedor>> SearchByRazonSocial(int idTenant, string texto);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByRUT(int idTenant, string rut);
    Result<int> CountByTenant(int idTenant, bool soloActivos = true);

    /// <summary>
    /// Proveedores que tienen al menos una Orden de Compra en estado Pendiente (1) o Aprobada (2).
    /// Útil para el dashboard de compras — muestra qué proveedores tienen entregas pendientes.
    /// Incluye el conteo de OCs activas y el total monetario comprometido por proveedor.
    /// </summary>
    Result<IReadOnlyList<ProveedorConOCPendiente>> GetConOrdenesPendientes(int idTenant);
}

/// <summary>
/// Proyección de proveedor con sus órdenes de compra activas.
/// Evita cargar todas las OCs; solo expone los agregados relevantes para el dashboard.
/// </summary>
public sealed record ProveedorConOCPendiente(
    int     IdProveedor,
    string  RazonSocial,
    string? NombreComercial,
    int     CantOCActivas,
    decimal TotalComprometido);

public class ProveedorRepository : RepositorySync<Proveedor>, IProveedorRepository
{
    public ProveedorRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Proveedor>> GetByTenant(int idTenant, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Proveedor>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            // IX_Prov_Tenant (IdTenant, Activo)
            var query = DbSet.AsNoTracking().Where(p => p.IdTenant == idTenant);
            if (soloActivos) query = query.Where(p => p.Activo);
            return Result<IReadOnlyList<Proveedor>>.Success(
                query.OrderBy(p => p.RazonSocial).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Proveedor>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<Proveedor?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Proveedor?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Proveedor?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            // UQ_Prov (IdTenant, Codigo)
            return Result<Proveedor?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(p => p.IdTenant == idTenant && p.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Proveedor?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<Proveedor?> GetByRUT(int idTenant, string rut)
    {
        if (Guard.Fail<Proveedor?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Proveedor?>(Guard.StringRequerido(nameof(rut), rut)) is { } errStr) return errStr;
        try
        {
            return Result<Proveedor?>.Success(
                DbSet.AsNoTracking().FirstOrDefault(p => p.IdTenant == idTenant && p.RUT == rut));
        }
        catch (Exception ex)
        {
            return Result<Proveedor?>.Failure(
                CreateError(ex, nameof(GetByRUT))
                    .WithDetail("IdTenant", idTenant).WithDetail("RUT", rut));
        }
    }

    public Result<IReadOnlyList<Proveedor>> SearchByRazonSocial(int idTenant, string texto)
    {
        if (Guard.Fail<IReadOnlyList<Proveedor>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Proveedor>>(Guard.StringRequerido(nameof(texto), texto)) is { } errStr) return errStr;
        try
        {
            var patron = $"%{texto}%";
            return Result<IReadOnlyList<Proveedor>>.Success(
                DbSet.AsNoTracking()
                    .Where(p => p.IdTenant == idTenant && p.Activo
                             && (EF.Functions.Like(p.RazonSocial, patron)
                              || EF.Functions.Like(p.NombreComercial, patron)))
                    .OrderBy(p => p.RazonSocial)
                    .Take(50)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Proveedor>>.Failure(
                CreateError(ex, nameof(SearchByRazonSocial))
                    .WithDetail("IdTenant", idTenant).WithDetail("Texto", texto));
        }
    }

    public Result<bool> ExistsByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(p => p.IdTenant == idTenant && p.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> ExistsByRUT(int idTenant, string rut)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(rut), rut)) is { } errStr) return errStr;
        try
        {
            return Result<bool>.Success(DbSet.Any(p => p.IdTenant == idTenant && p.RUT == rut));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByRUT))
                    .WithDetail("IdTenant", idTenant).WithDetail("RUT", rut));
        }
    }

    public Result<int> CountByTenant(int idTenant, bool soloActivos = true)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<int>.Success(
                DbSet.Count(p => p.IdTenant == idTenant && (!soloActivos || p.Activo)));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<ProveedorConOCPendiente>> GetConOrdenesPendientes(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<ProveedorConOCPendiente>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            // Estados "activos": 1=Pendiente, 2=Aprobada
            // IX_OC_Prov (IdProveedor, Estado) + IX_Prov_Tenant (IdTenant, Activo)
            // JOIN OC → Proveedor para agregar en una sola query
            var resultado = Context.Set<OrdenCompra>()
                .AsNoTracking()
                .Where(oc => oc.IdTenant == idTenant
                          && (oc.Estado == 1 || oc.Estado == 2))
                .GroupBy(oc => new
                {
                    oc.IdProveedor,
                    oc.Proveedor.RazonSocial,
                    oc.Proveedor.NombreComercial
                })
                .Select(g => new ProveedorConOCPendiente(
                    g.Key.IdProveedor,
                    g.Key.RazonSocial,
                    g.Key.NombreComercial,
                    g.Count(),
                    g.Sum(oc => oc.Total)))
                .OrderByDescending(r => r.TotalComprometido)
                .ToList();

            return Result<IReadOnlyList<ProveedorConOCPendiente>>.Success(resultado.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProveedorConOCPendiente>>.Failure(
                CreateError(ex, nameof(GetConOrdenesPendientes)).WithDetail("IdTenant", idTenant));
        }
    }
}
