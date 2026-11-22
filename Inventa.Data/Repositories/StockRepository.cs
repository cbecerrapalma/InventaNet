using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IStockRepository : IRepositorySync<Stock>
{
    /// <summary>
    /// Obtiene el stock consolidado para una bodega.
    /// 
    /// Parámetro `incluirLote`:
    /// - false (default): Solo carga Stock + VarProd (optimizado, 20-30% más rápido)
    /// - true: Carga Stock + VarProd + Lote (para operaciones que requieren datos de lote)
    /// 
    /// Recomendación: usar false por defecto, true solo si se necesita información de Lote.
    /// </summary>
    Result<IReadOnlyList<Stock>> GetByTenantBodega(int idTenant, int idBodega, bool incluirLote = false);
    Result<IReadOnlyList<Stock>> GetByVariante(int idTenant, long idVarProd);

    /// <summary>
    /// Busca el registro de stock por clave natural (IdTenant, IdBodega, IdVarProd, IdLote).
    /// IdLote=null busca el registro sin lote (NULL en BD).
    /// EF Core traduce correctamente la comparación nullable (IS NULL / = @p).
    /// </summary>
    Result<Stock?> Get(int idTenant, int idBodega, long idVarProd, long? idLote = null);

    Result<IReadOnlyList<Stock>> GetConStockNegativo(int idTenant);

    /// <summary>
    /// Suma CantAct de todos los registros para la variante (todas las bodegas y lotes).
    /// Retorna null si no existe ningún registro — distingue "sin stock registrado" de "stock = 0".
    /// </summary>
    Result<decimal?> GetCantidadTotal(int idTenant, long idVarProd);

    /// <summary>
    /// Suma CantAct para la variante en una bodega específica.
    /// Retorna null si no existe registro para esa combinación.
    /// </summary>
    Result<decimal?> GetCantidadEnBodega(int idTenant, int idBodega, long idVarProd);

    /// <summary>
    /// Costo promedio ponderado vigente para una variante en una bodega:
    /// SUM(CantAct × CostoProm) / SUM(CantAct), solo sobre registros con CantAct > 0.
    /// Retorna null si no hay stock positivo.
    /// </summary>
    Result<decimal?> GetCostoPromedio(int idTenant, int idBodega, long idVarProd);

    /// <summary>
    /// Ejecuta sp_AjustarInventario.
    /// Valida en C# que ningún ítem tenga CantAjuste = 0 antes de ir al SP
    /// (el SP lanza error 50032 en ese caso — esta validación evita el round-trip).
    /// CantAjuste positivo = Ajuste Entrada (TipoMov 3).
    /// CantAjuste negativo = Ajuste Salida  (TipoMov 4).
    /// </summary>
    Result AjustarInventario(AjustarInventarioRequest request);

    /// <summary>
    /// Ejecuta sp_TransferirEntresBodegas.
    /// Valida en C# que IdBodegaOrig ≠ IdBodegaDest y que todos los ítems tengan
    /// Cantidad > 0 antes de ir al SP.
    /// </summary>
    Result TransferirEntresBodegas(TransferirEntresBodegasRequest request);
}

public class StockRepository : RepositorySync<Stock>, IStockRepository
{
    public StockRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Stock>> GetByTenantBodega(int idTenant, int idBodega, bool incluirLote = false)
    {
        if (Guard.Fail<IReadOnlyList<Stock>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Stock>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            // IX_Stock_Busq (IdTenant, IdVarProd, IdBodega)
            IQueryable<Stock> q = DbSet.AsNoTracking()
                .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega)
                .Include(s => s.VarProd);  // Siempre necesario para el OrderBy

            // Lote es opcional — incluir solo si se necesita
            if (incluirLote)
                q = q.Include(s => s.Lote);

            return Result<IReadOnlyList<Stock>>.Success(
                q.OrderBy(s => s.VarProd.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Stock>>.Failure(
                CreateError(ex, nameof(GetByTenantBodega))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }

    public Result<IReadOnlyList<Stock>> GetByVariante(int idTenant, long idVarProd)
    {
        if (Guard.Fail<IReadOnlyList<Stock>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Stock>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            return Result<IReadOnlyList<Stock>>.Success(
                DbSet.AsNoTracking()
                    .Where(s => s.IdTenant == idTenant && s.IdVarProd == idVarProd)
                    .Include(s => s.Bodega)
                    .Include(s => s.Lote)
                    .OrderBy(s => s.Bodega.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Stock>>.Failure(
                CreateError(ex, nameof(GetByVariante))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<Stock?> Get(int idTenant, int idBodega, long idVarProd, long? idLote = null)
    {
        if (Guard.Fail<Stock?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<Stock?>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<Stock?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            // UQ_Stock (IdTenant, IdBodega, IdVarProd, IdLote) con IdLote NULLABLE.
            // EF Core genera: s.IdLote == idLote  →  IS NULL / = @p según corresponda.
            return Result<Stock?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(s => s.IdTenant  == idTenant
                                      && s.IdBodega  == idBodega
                                      && s.IdVarProd == idVarProd
                                      && s.IdLote    == idLote));
        }
        catch (Exception ex)
        {
            return Result<Stock?>.Failure(
                CreateError(ex, nameof(Get))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdBodega",  idBodega)
                    .WithDetail("IdVarProd", idVarProd)
                    .WithDetail("IdLote",    idLote));
        }
    }

    public Result<IReadOnlyList<Stock>> GetConStockNegativo(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Stock>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Stock>>.Success(
                DbSet.AsNoTracking()
                    .Where(s => s.IdTenant == idTenant && s.CantAct < 0)
                    .Include(s => s.Bodega)
                    .Include(s => s.VarProd)
                    .Include(s => s.Lote)
                    .OrderBy(s => s.CantAct)   // más negativos primero
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Stock>>.Failure(
                CreateError(ex, nameof(GetConStockNegativo)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<decimal?> GetCantidadTotal(int idTenant, long idVarProd)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            // Sum sobre nullable retorna null si no hay filas — distingue "sin registros" de "stock = 0"
            return Result<decimal?>.Success(
                DbSet.Where(s => s.IdTenant == idTenant && s.IdVarProd == idVarProd)
                     .Select(s => (decimal?)s.CantAct)
                     .Sum());
        }
        catch (Exception ex)
        {
            return Result<decimal?>.Failure(
                CreateError(ex, nameof(GetCantidadTotal))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<decimal?> GetCantidadEnBodega(int idTenant, int idBodega, long idVarProd)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            return Result<decimal?>.Success(
                DbSet.Where(s => s.IdTenant  == idTenant
                              && s.IdBodega  == idBodega
                              && s.IdVarProd == idVarProd)
                     .Select(s => (decimal?)s.CantAct)
                     .Sum());
        }
        catch (Exception ex)
        {
            return Result<decimal?>.Failure(
                CreateError(ex, nameof(GetCantidadEnBodega))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdBodega",  idBodega)
                    .WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<decimal?> GetCostoPromedio(int idTenant, int idBodega, long idVarProd)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            // Trae solo los campos necesarios — evita cargar la entidad completa
            var registros = DbSet
                .Where(s => s.IdTenant  == idTenant
                         && s.IdBodega  == idBodega
                         && s.IdVarProd == idVarProd
                         && s.CantAct   > 0)
                .Select(s => new { s.CantAct, s.CostoProm })
                .ToList();

            if (registros.Count == 0) return Result<decimal?>.Success(null);

            var totalCant = registros.Sum(r => r.CantAct);
            var costoProm = totalCant == 0
                ? 0m
                : registros.Sum(r => r.CantAct * r.CostoProm) / totalCant;

            return Result<decimal?>.Success(costoProm);
        }
        catch (Exception ex)
        {
            return Result<decimal?>.Failure(
                CreateError(ex, nameof(GetCostoPromedio))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdBodega",  idBodega)
                    .WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result AjustarInventario(AjustarInventarioRequest request)
    {
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdTenant), request.IdTenant)) is { } errT) return errT;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdBodega), request.IdBodega)) is { } errB) return errB;
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));

        // BUG FIX: validar CantAjuste != 0 antes del round-trip al SP
        // El SP lanza error 50032 para cualquier ítem con CantAjuste=0.
        // Validar aquí evita el overhead de la conexión SQL y da un mensaje más claro.
        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (Guard.Fail(Guard.CantidadAjuste($"Items[{i}].CantAjuste", item.CantAjuste)) is { } errCant)
                return errCant;
        }

        return SpExecutor.AjustarInventario(Context, request);
    }

    public Result TransferirEntresBodegas(TransferirEntresBodegasRequest request)
    {
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdTenant), request.IdTenant)) is { } errT) return errT;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdBodegaOrig), request.IdBodegaOrig)) is { } errBO) return errBO;
        if (Guard.Fail(Guard.IdInvalido(nameof(request.IdBodegaDest), request.IdBodegaDest)) is { } errBD) return errBD;
        if (request.IdBodegaOrig == request.IdBodegaDest)
            return Result.Failure(
                new Error("BODEGA_MISMA_ORIGEN_DEST",
                          "La bodega de origen y destino no pueden ser la misma.")
                    .WithType(ErrorType.Validation).WithHttpStatus(400)
                    .WithDetail("IdBodegaOrig", request.IdBodegaOrig)
                    .WithDetail("IdBodegaDest", request.IdBodegaDest));
        if (request.Items is null || request.Items.Count == 0)
            return Result.Failure(CommonErrors.Validation.Required(nameof(request.Items)));

        // Validar que todas las cantidades sean positivas
        for (int i = 0; i < request.Items.Count; i++)
        {
            var item = request.Items[i];
            if (Guard.Fail(Guard.CantidadPositiva($"Items[{i}].Cantidad", item.Cantidad)) is { } errCant)
                return errCant;
        }

        return SpExecutor.TransferirEntresBodegas(Context, request);
    }
}
