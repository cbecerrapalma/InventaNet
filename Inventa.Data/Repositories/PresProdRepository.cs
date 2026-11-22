using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IPresProdRepository : IRepositorySync<PresProd>
{
    Result<IReadOnlyList<PresProd>> GetByVariante(long idVarProd, bool soloActivas = true);

    /// <summary>
    /// Filtra presentaciones habilitadas para venta (paraVenta=true) o compra (paraVenta=false).
    /// Reemplaza los antiguos GetParaVentaAsync / GetParaCompraAsync — evita duplicar la query.
    /// </summary>
    Result<IReadOnlyList<PresProd>> GetByUsoPresentacion(long idVarProd, bool paraVenta);

    /// <summary>
    /// Carga la presentación con sus precios vigentes filtrados por IX_Precio_Fast.
    /// Incluye Unidad y CodigosBarra para el despliegue completo en pantalla.
    /// </summary>
    Result<PresProd?> GetWithPreciosVigentes(long idPresProd, DateOnly? fecha = null);

    Result<bool> ExistsByVarianteUnidad(long idVarProd, int idUnidad);
    Result<IReadOnlyList<PresProd>> GetByTenant(int idTenant, bool soloActivas = true);

    /// <summary>Todas las presentaciones de todas las variantes de un producto.</summary>
    Result<IReadOnlyList<PresProd>> GetByProducto(long idProd, bool soloActivas = true);
}

public class PresProdRepository : RepositorySync<PresProd>, IPresProdRepository
{
    public PresProdRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<PresProd>> GetByVariante(long idVarProd, bool soloActivas = true)
    {
        if (Guard.Fail<IReadOnlyList<PresProd>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            // IX_PresProd_Var (IdVarProd)
            var query = DbSet.AsNoTracking().Where(p => p.IdVarProd == idVarProd);
            if (soloActivas) query = query.Where(p => p.Activo);
            return Result<IReadOnlyList<PresProd>>.Success(
                query.Include(p => p.Unidad).OrderBy(p => p.Descrip).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PresProd>>.Failure(
                CreateError(ex, nameof(GetByVariante)).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<IReadOnlyList<PresProd>> GetByUsoPresentacion(long idVarProd, bool paraVenta)
    {
        if (Guard.Fail<IReadOnlyList<PresProd>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking().Where(p => p.IdVarProd == idVarProd && p.Activo);
            query = paraVenta ? query.Where(p => p.EsVenta) : query.Where(p => p.EsCompra);
            return Result<IReadOnlyList<PresProd>>.Success(
                query.Include(p => p.Unidad).OrderBy(p => p.Descrip).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PresProd>>.Failure(
                CreateError(ex, nameof(GetByUsoPresentacion))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("ParaVenta", paraVenta));
        }
    }

    public Result<PresProd?> GetWithPreciosVigentes(long idPresProd, DateOnly? fecha = null)
    {
        if (Guard.Fail<PresProd?>(Guard.IdInvalido(nameof(idPresProd), idPresProd)) is { } err) return err;
        var fechaRef = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);
        try
        {
            // Precios filtrados: Activo=true AND FecVig <= hoy
            // IX_Precio_Fast (IdPresProd, FecVig DESC) — cubriente para el filtro
            // Nota: Precio.Valor es el campo decimal del precio (no Precio.Precio)
            return Result<PresProd?>.Success(
                DbSet.AsNoTracking()
                    .Include(p => p.Unidad)
                    .Include(p => p.CodigosBarra)
                    .Include(p => p.Precios.Where(pr => pr.Activo && pr.FecVig <= fechaRef))
                    .FirstOrDefault(p => p.IdPresProd == idPresProd));
        }
        catch (Exception ex)
        {
            return Result<PresProd?>.Failure(
                CreateError(ex, nameof(GetWithPreciosVigentes))
                    .WithDetail("IdPresProd", idPresProd)
                    .WithDetail("Fecha",      fechaRef));
        }
    }

    public Result<bool> ExistsByVarianteUnidad(long idVarProd, int idUnidad)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } errUni) return errUni;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(p => p.IdVarProd == idVarProd && p.IdUnidad == idUnidad));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByVarianteUnidad))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdUnidad", idUnidad));
        }
    }

    public Result<IReadOnlyList<PresProd>> GetByTenant(int idTenant, bool soloActivas = true)
    {
        try
        {
            var query = DbSet.AsNoTracking()
                .Include(p => p.VarProd).ThenInclude(v => v.Prod)
                .Include(p => p.Unidad)
                .Where(p => p.VarProd.Prod.IdTenant == idTenant);
            if (soloActivas) query = query.Where(p => p.Activo);
            return Result<IReadOnlyList<PresProd>>.Success(
                query.OrderBy(p => p.VarProd.Prod.Nombre).ThenBy(p => p.Descrip).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PresProd>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<PresProd>> GetByProducto(long idProd, bool soloActivas = true)
    {
        if (Guard.Fail<IReadOnlyList<PresProd>>(Guard.IdInvalido(nameof(idProd), idProd)) is { } err) return err;
        try
        {
            var query = DbSet.AsNoTracking()
                .Include(p => p.Unidad)
                .Include(p => p.VarProd)
                .Where(p => p.VarProd.IdProd == idProd);
            if (soloActivas) query = query.Where(p => p.Activo);
            return Result<IReadOnlyList<PresProd>>.Success(
                query.OrderBy(p => p.VarProd.Nombre).ThenBy(p => p.Descrip).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<PresProd>>.Failure(
                CreateError(ex, nameof(GetByProducto)).WithDetail("IdProd", idProd));
        }
    }
}
