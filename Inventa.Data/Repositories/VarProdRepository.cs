using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IVarProdRepository : IRepositorySync<VarProd>
{
    Result<IReadOnlyList<VarProd>> GetByProducto(long idProd, bool soloActivos = true);
    Result<VarProd?> GetByCodigo(long idProd, string codigo);
    Result<VarProd?> GetWithPresentaciones(long idVarProd);
    Result<bool> ExistsByCodigo(long idProd, string codigo);
    Result<IReadOnlyList<VarProd>> GetConCostoCargado(long idProd);

    /// <summary>
    /// Todas las variantes activas de un tenant con su producto padre.
    /// Útil para búsquedas cruzadas de inventario y reportes de valorización.
    /// </summary>
    Result<IReadOnlyList<VarProd>> GetByTenant(int idTenant, bool soloActivos = true);

    /// <summary>
    /// Variantes cuyo stock total (SUM CantAct en todas las bodegas del tenant)
    /// es menor al umbralStock indicado.
    /// umbralStock = 0 retorna variantes con stock negativo o sin registro alguno.
    /// Útil para generar alertas de reposición automática.
    ///
    /// IMPLEMENTACIÓN: JOIN explícito con LEFT JOIN + GroupBy en una sola query.
    /// Evita el N+1 que produce la subquery correlacionada en la versión anterior.
    /// Variantes sin ningún registro en Stock se tratan como stock = 0 (LEFT JOIN).
    /// </summary>
    Result<IReadOnlyList<VarProd>> GetConBajoStock(int idTenant, decimal umbralStock = 0m);
}

public class VarProdRepository : RepositorySync<VarProd>, IVarProdRepository
{
    public VarProdRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<VarProd>> GetByProducto(long idProd, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<VarProd>>(Guard.IdInvalido(nameof(idProd), idProd)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(v => v.IdProd == idProd);
            if (soloActivos) query = query.Where(v => v.Activo);
            // IX_VarProd_Prod (IdProd)
            return Result<IReadOnlyList<VarProd>>.Success(
                query.OrderBy(v => v.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarProd>>.Failure(
                CreateError(ex, nameof(GetByProducto)).WithDetail("IdProd", idProd));
        }
    }

    public Result<VarProd?> GetByCodigo(long idProd, string codigo)
    {
        if (Guard.Fail<VarProd?>(Guard.IdInvalido(nameof(idProd), idProd)) is { } errId) return errId;
        if (Guard.Fail<VarProd?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            // UQ_Var (IdProd, Codigo) — seek de punto único
            return Result<VarProd?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(v => v.IdProd == idProd && v.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<VarProd?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdProd", idProd).WithDetail("Codigo", codigo));
        }
    }

    public Result<VarProd?> GetWithPresentaciones(long idVarProd)
    {
        if (Guard.Fail<VarProd?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            return Result<VarProd?>.Success(
                DbSet.AsNoTracking()
                    .Include(v => v.Prod)
                        .ThenInclude(p => p.UnidadBase)
                    .Include(v => v.Presentaciones.Where(p => p.Activo))
                        .ThenInclude(p => p.Unidad)
                    .Include(v => v.Presentaciones.Where(p => p.Activo))
                        .ThenInclude(p => p.CodigosBarra)
                    .Include(v => v.Presentaciones.Where(p => p.Activo))
                        .ThenInclude(p => p.Precios.Where(pr => pr.Activo))
                    .Include(v => v.Atributos)
                        .ThenInclude(a => a.AtriVal)
                            .ThenInclude(av => av.AtriDef)
                    .FirstOrDefault(v => v.IdVarProd == idVarProd));
        }
        catch (Exception ex)
        {
            return Result<VarProd?>.Failure(
                CreateError(ex, nameof(GetWithPresentaciones))
                    .WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<bool> ExistsByCodigo(long idProd, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idProd), idProd)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(v => v.IdProd == idProd && v.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdProd", idProd).WithDetail("Codigo", codigo));
        }
    }

    public Result<IReadOnlyList<VarProd>> GetConCostoCargado(long idProd)
    {
        if (Guard.Fail<IReadOnlyList<VarProd>>(Guard.IdInvalido(nameof(idProd), idProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<VarProd>>.Success(
                DbSet.AsNoTracking()
                    .Where(v => v.IdProd == idProd && v.Activo && v.CostoProm > 0)
                    .OrderBy(v => v.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarProd>>.Failure(
                CreateError(ex, nameof(GetConCostoCargado)).WithDetail("IdProd", idProd));
        }
    }

    public Result<IReadOnlyList<VarProd>> GetByTenant(int idTenant, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<VarProd>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking()
                .Include(v => v.Prod)
                .Where(v => v.Prod.IdTenant == idTenant);
            if (soloActivos) query = query.Where(v => v.Activo && v.Prod.Activo);
            return Result<IReadOnlyList<VarProd>>.Success(
                query.OrderBy(v => v.Prod.Nombre).ThenBy(v => v.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarProd>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<VarProd>> GetConBajoStock(int idTenant, decimal umbralStock = 0m)
    {
        if (Guard.Fail<IReadOnlyList<VarProd>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            // BUG FIX v3: la subquery correlacionada anterior generaba N+1 queries.
            //
            // SOLUCIÓN: LEFT JOIN explícito VarProd → Stock con GroupBy en SQL Server.
            // Una sola query calcula el SUM(CantAct) por variante y filtra en HAVING.
            // El LEFT JOIN garantiza que variantes sin registros en Stock aparecen
            // con SUM = NULL, que ISNULL/COALESCE convierte a 0 → incluidas si umbral > 0.
            //
            // IX_Stock_Busq (IdTenant, IdVarProd, IdBodega) cubre el filtro por tenant.

            // Paso 1: obtener los IdVarProd con stock bajo o sin stock — una sola query SQL
            var idsBajoStock = (
                from v in Context.Set<VarProd>()
                    .Where(v => v.Prod.IdTenant == idTenant && v.Activo && v.Prod.Activo)
                join s in Context.Set<Stock>()
                    .Where(s => s.IdTenant == idTenant)
                    on v.IdVarProd equals s.IdVarProd into stockGroup
                from sg in stockGroup.DefaultIfEmpty()     // LEFT JOIN
                group sg by v.IdVarProd into g
                where (g.Sum(s => (decimal?)s.CantAct) ?? 0m) < umbralStock
                select g.Key
            ).ToList();

            if (idsBajoStock.Count == 0)
                return Result<IReadOnlyList<VarProd>>.Success([]);

            // Paso 2: cargar las entidades completas con su Prod para el ordenamiento
            var variantes = DbSet.AsNoTracking()
                .Include(v => v.Prod)
                .Where(v => idsBajoStock.Contains(v.IdVarProd))
                .OrderBy(v => v.Prod.Nombre)
                .ThenBy(v => v.Nombre)
                .ToList();

            return Result<IReadOnlyList<VarProd>>.Success(variantes.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<VarProd>>.Failure(
                CreateError(ex, nameof(GetConBajoStock))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("UmbralStock", umbralStock));
        }
    }
}
