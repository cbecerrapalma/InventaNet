using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

/// <summary>Proyección POS: stock + precio en un solo round-trip. S57.69: precio normal único.</summary>
public sealed record StockConPrecio(
    long    IdPresProd,
    string  Descrip,
    decimal CantDisp,
    decimal PrecioVigente);

public interface IStockDispRepository : IRepositorySync<StockDisp>
{
    Result<StockDisp?> Get(int idTenant, long idPresProd, int idBodega);
    Result<decimal> GetCantidad(int idTenant, long idPresProd, int idBodega);

    /// <summary>
    /// Verifica disponibilidad sin doble roundtrip.
    /// Consulta directa: evita GetCantidad + comparación en memoria.
    /// </summary>
    Result<bool> TieneStockSuficiente(int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida);

    Result<IReadOnlyList<StockDisp>> GetByBodega(int idTenant, int idBodega, bool soloConStock = false);

    /// <summary>Presentaciones con CantDisp = 0 (agotadas). Útil para reposición.</summary>
    Result<IReadOnlyList<StockDisp>> GetAgotados(int idTenant, int idBodega);

    /// <summary>
    /// Presentaciones con CantDisp &lt; 0 — inconsistencia de inventario.
    /// Indica error de integridad; requiere auditoría o ajuste.
    /// </summary>
    Result<IReadOnlyList<StockDisp>> GetConStockNegativo(int idTenant, int idBodega);

    /// <summary>
    /// Lookup POS crítico: dado un código de barras retorna el stock disponible
    /// y el precio normal vigente en una sola consulta compuesta.
    /// Usa IX_CodBar_Fast (Codigo, IdPresProd) + IX_StockDisp_Busq + IX_Precio_Fast.
    /// S57.69: precio normal único (mayorista vive en PrecioMayorista).
    /// </summary>
    Result<StockConPrecio?> GetByCodigoBarraPOS(int idTenant, int idBodega, string codigoBarra);
}

public class StockDispRepository : RepositorySync<StockDisp>, IStockDispRepository
{
    public StockDispRepository(InventaDbContext context) : base(context) { }

    public Result<StockDisp?> Get(int idTenant, long idPresProd, int idBodega)
    {
        if (Guard.Fail<StockDisp?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<StockDisp?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<StockDisp?>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            // IX_StockDisp_Busq (IdTenant, IdPresProd, IdBodega) — seek preciso
            return Result<StockDisp?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(s => s.IdTenant  == idTenant
                                      && s.IdPresProd == idPresProd
                                      && s.IdBodega  == idBodega));
        }
        catch (Exception ex)
        {
            return Result<StockDisp?>.Failure(
                CreateError(ex, nameof(Get))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("IdBodega",  idBodega));
        }
    }

    public Result<decimal> GetCantidad(int idTenant, long idPresProd, int idBodega)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            var cantidad = DbSet
                .Where(s => s.IdTenant  == idTenant
                         && s.IdPresProd == idPresProd
                         && s.IdBodega  == idBodega)
                .Select(s => (decimal?)s.CantDisp)
                .FirstOrDefault() ?? 0m;
            return Result<decimal>.Success(cantidad);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(GetCantidad))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("IdBodega",  idBodega));
        }
    }

    public Result<bool> TieneStockSuficiente(
        int idTenant, long idPresProd, int idBodega, decimal cantidadRequerida)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } errP) return errP;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<bool>(Guard.Rango(nameof(cantidadRequerida), cantidadRequerida, 0.0001m, decimal.MaxValue)) is { } errC) return errC;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(s => s.IdTenant   == idTenant
                            && s.IdPresProd == idPresProd
                            && s.IdBodega   == idBodega
                            && s.CantDisp   >= cantidadRequerida));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(TieneStockSuficiente))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdPresProd", idPresProd));
        }
    }

    public Result<IReadOnlyList<StockDisp>> GetByBodega(
        int idTenant, int idBodega, bool soloConStock = false)
    {
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            var query = DbSet.AsNoTracking()
                .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega);
            if (soloConStock) query = query.Where(s => s.CantDisp > 0);
            return Result<IReadOnlyList<StockDisp>>.Success(
                query.Include(s => s.PresProd)
                        .ThenInclude(p => p.VarProd)
                    .OrderBy(s => s.PresProd.VarProd.Nombre)
                    .ThenBy(s => s.PresProd.Descrip)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<StockDisp>>.Failure(
                CreateError(ex, nameof(GetByBodega))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }

    public Result<IReadOnlyList<StockDisp>> GetAgotados(int idTenant, int idBodega)
    {
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            return Result<IReadOnlyList<StockDisp>>.Success(
                DbSet.AsNoTracking()
                    .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega && s.CantDisp == 0)
                    .Include(s => s.PresProd).ThenInclude(p => p.VarProd)
                    .OrderBy(s => s.PresProd.VarProd.Nombre).ThenBy(s => s.PresProd.Descrip)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<StockDisp>>.Failure(
                CreateError(ex, nameof(GetAgotados))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }

    public Result<IReadOnlyList<StockDisp>> GetConStockNegativo(int idTenant, int idBodega)
    {
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<StockDisp>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            return Result<IReadOnlyList<StockDisp>>.Success(
                DbSet.AsNoTracking()
                    .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega && s.CantDisp < 0)
                    .Include(s => s.PresProd).ThenInclude(p => p.VarProd)
                    // Los más negativos primero: mayor urgencia de corrección
                    .OrderBy(s => s.CantDisp).ThenBy(s => s.PresProd.VarProd.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<StockDisp>>.Failure(
                CreateError(ex, nameof(GetConStockNegativo))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdBodega", idBodega));
        }
    }

    public Result<StockConPrecio?> GetByCodigoBarraPOS(
        int idTenant, int idBodega, string codigoBarra)
    {
        if (Guard.Fail<StockConPrecio?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<StockConPrecio?>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        if (Guard.Fail<StockConPrecio?>(Guard.StringRequerido(nameof(codigoBarra), codigoBarra)) is { } errC) return errC;

        var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            // 1. IX_CodBar_Fast (Codigo, IdPresProd) — seek puntual por código de barras
            // 2. IX_StockDisp_Busq (IdTenant, IdPresProd, IdBodega) — stock disponible
            // 3. IX_Precio_Fast (IdPresProd, FecVig DESC) — precio normal vigente
            // Todo en una sola consulta compuesta; sin JOIN explícito en memoria.
            var resultado = (
                from cb in Context.Set<CodBar>()
                    .Where(c => c.Codigo == codigoBarra)
                join sd in Context.Set<StockDisp>()
                    .Where(s => s.IdTenant == idTenant && s.IdBodega == idBodega)
                    on cb.IdPresProd equals sd.IdPresProd
                join pp in Context.Set<PresProd>()
                    on sd.IdPresProd equals pp.IdPresProd
                let precio = Context.Set<Precio>()
                    .Where(p => p.IdPresProd == cb.IdPresProd
                             && p.Activo
                             && p.FecVig     <= hoy)
                    .OrderByDescending(p => p.FecVig)
                    .Select(p => p.Valor)
                    .FirstOrDefault()
                select new StockConPrecio(
                    sd.IdPresProd,
                    pp.Descrip,
                    sd.CantDisp,
                    precio)
            ).FirstOrDefault();

            return Result<StockConPrecio?>.Success(resultado);
        }
        catch (Exception ex)
        {
            return Result<StockConPrecio?>.Failure(
                CreateError(ex, nameof(GetByCodigoBarraPOS))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("IdBodega",    idBodega)
                    .WithDetail("CodigoBarra", codigoBarra));
        }
    }
}
