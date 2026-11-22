using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IProdRepository : IRepositorySync<Prod>
{
    Result<IReadOnlyList<Prod>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<Prod>> GetByCategoria(int idTenant, long idCategoria, bool soloActivos = true);
    Result<IReadOnlyList<Prod>> GetByMarca(int idTenant, int idMarca, bool soloActivos = true);
    Result<Prod?> GetByCodigo(int idTenant, string codigo);

    /// <summary>
    /// Carga un producto con sus variantes activas.
    /// 
    /// Parámetro `loadOptions` controla qué datos relacionados cargar:
    /// - None: solo Variantes (más rápido)
    /// - Presentaciones: Variantes + Presentaciones
    /// - Atributos: Variantes + Atributos (con cadena AtriVal → AtriDef)
    /// - Both: Variantes + Presentaciones + Atributos (lento, usar solo si se necesita todo)
    /// 
    /// Recomendación: usar el mínimo necesario para reducir carga de red.
    /// </summary>
    Result<Prod?> GetWithVariantes(long idProd, int idTenant, ProdRepository.VarianteLoadOption loadOptions = ProdRepository.VarianteLoadOption.Both);
    Result<IReadOnlyList<Prod>> SearchByNombre(int idTenant, string nombre, bool soloActivos = true);
    Result<IReadOnlyList<Prod>> GetQueManejanLote(int idTenant);
    Result<IReadOnlyList<Prod>> GetPesables(int idTenant);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountByTenant(int idTenant, bool soloActivos = true);

    /// <summary>
    /// Búsqueda unificada POS: código exacto (UQ_Prod) OR nombre parcial (LIKE).
    /// Intenta match exacto por código primero — si encuentra, retorna ese resultado.
    /// Si no, busca por nombre parcial. Límite 50 resultados en búsqueda por nombre.
    /// </summary>
    Result<IReadOnlyList<Prod>> SearchByCodigoOrNombre(int idTenant, string texto, bool soloActivos = true);

    /// <summary>
    /// Carga un producto con sus variantes activas y el stock consolidado por bodega.
    /// Útil para la pantalla de ficha de producto.
    ///
    /// IMPLEMENTACIÓN: dos queries separadas para evitar el problema de filtered includes
    /// en colecciones anidadas (EF Core puede ignorar el filtro de tenant en el tercer
    /// nivel de Include y traer silenciosamente todos los stocks).
    ///   Query 1: Prod + Variantes + Presentaciones + Atributos (sin Stock)
    ///   Query 2: Stock del tenant para esas variantes, mapeado en memoria
    /// </summary>
    Result<Prod?> GetWithVariantesYStock(long idProd, int idTenant);

    /// <summary>
    /// Productos con Prod.DiasVen definido (plazo de vida útil esperado en días).
    /// Útil para alertas de vencimiento personalizadas por producto en LoteRepository.
    /// Ordenados por DiasVen ASC (vencimiento más corto primero).
    /// </summary>
    Result<IReadOnlyList<Prod>> GetConControlVencimiento(int idTenant);
}

public class ProdRepository : RepositorySync<Prod>, IProdRepository
{
    /// <summary>
    /// Controls which related data to load with GetWithVariantes().
    /// Use flags to avoid over-fetching unnecessary relationships.
    /// </summary>
    [Flags]
    public enum VarianteLoadOption
    {
        None = 0,
        Presentaciones = 1,
        Atributos = 2,
        Both = Presentaciones | Atributos
    }

    public ProdRepository(InventaDbContext context) : base(context) { }

    private IQueryable<Prod> BaseQuery(int idTenant, bool soloActivos)
    {
        var q = DbSet.AsNoTracking().Where(p => p.IdTenant == idTenant);
        return soloActivos ? q.Where(p => p.Activo) : q;
    }

    public Result<IReadOnlyList<Prod>> GetByTenant(int idTenant, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos).OrderBy(p => p.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Prod>> GetByCategoria(int idTenant, long idCategoria, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idCategoria), idCategoria)) is { } errCat) return errCat;
        try
        {
            // IX_Prod_Tenant_Cat (IdTenant, IdCategoria)
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos)
                    .Where(p => p.IdCategoria == idCategoria)
                    .OrderBy(p => p.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetByCategoria))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdCategoria", idCategoria));
        }
    }

    public Result<IReadOnlyList<Prod>> GetByMarca(int idTenant, int idMarca, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idMarca), idMarca)) is { } errMarca) return errMarca;
        try
        {
            // IX_Prod_Marca (IdMarca)
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos)
                    .Where(p => p.IdMarca == idMarca)
                    .OrderBy(p => p.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetByMarca))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdMarca", idMarca));
        }
    }

    public Result<Prod?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Prod?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Prod?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            // UQ_Prod (IdTenant, Codigo)
            return Result<Prod?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(p => p.IdTenant == idTenant && p.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Prod?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<Prod?> GetWithVariantes(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both)
    {
        if (Guard.Fail<Prod?>(Guard.IdInvalido(nameof(idProd), idProd)) is { } err) return err;
        try
        {
            IQueryable<Prod> q = DbSet.AsNoTracking()
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .Include(p => p.Variantes.Where(v => v.Activo));

            // Si necesitamos Presentaciones
            if ((loadOptions & VarianteLoadOption.Presentaciones) != 0)
            {
                q = q.Include(p => p.Variantes.Where(v => v.Activo))
                    .ThenInclude(v => v.Presentaciones.Where(pp => pp.Activo));
            }

            // Si necesitamos Atributos
            if ((loadOptions & VarianteLoadOption.Atributos) != 0)
            {
                q = q.Include(p => p.Variantes.Where(v => v.Activo))
                    .ThenInclude(v => v.Atributos)
                        .ThenInclude(a => a.AtriVal)
                            .ThenInclude(av => av.AtriDef);
            }

            return Result<Prod?>.Success(
                q.FirstOrDefault(p => p.IdProd == idProd && p.IdTenant == idTenant));
        }
        catch (Exception ex)
        {
            return Result<Prod?>.Failure(
                CreateError(ex, nameof(GetWithVariantes)).WithDetail("IdProd", idProd));
        }
    }

    public Result<IReadOnlyList<Prod>> SearchByNombre(int idTenant, string nombre, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.StringRequerido(nameof(nombre), nombre)) is { } errStr) return errStr;
        try
        {
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos)
                    .Where(p => EF.Functions.Like(p.Nombre, $"%{nombre}%"))
                    .OrderBy(p => p.Nombre)
                    .Take(100)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(SearchByNombre))
                    .WithDetail("IdTenant", idTenant).WithDetail("Nombre", nombre));
        }
    }

    public Result<IReadOnlyList<Prod>> GetQueManejanLote(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos: true)
                    .Where(p => p.ManejaLot).OrderBy(p => p.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetQueManejanLote)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Prod>> GetPesables(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos: true)
                    .Where(p => p.EsPesable).OrderBy(p => p.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetPesables)).WithDetail("IdTenant", idTenant));
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

    public Result<IReadOnlyList<Prod>> SearchByCodigoOrNombre(
        int idTenant, string texto, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.StringRequerido(nameof(texto), texto)) is { } errStr) return errStr;
        try
        {
            var textoNorm = texto.Trim().ToUpperInvariant();

            // 1. Seek exacto por código — UQ_Prod (IdTenant, Codigo), latencia mínima
            var porCodigo = DbSet.AsNoTracking()
                .Where(p => p.IdTenant == idTenant
                         && p.Codigo   == textoNorm
                         && (!soloActivos || p.Activo))
                .ToList();

            if (porCodigo.Count > 0)
                return Result<IReadOnlyList<Prod>>.Success(porCodigo.AsReadOnly());

            // 2. Búsqueda parcial por nombre si no hubo match exacto
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos)
                    .Where(p => EF.Functions.Like(p.Nombre, $"%{texto.Trim()}%"))
                    .OrderBy(p => p.Nombre)
                    .Take(50)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(SearchByCodigoOrNombre))
                    .WithDetail("IdTenant", idTenant).WithDetail("Texto", texto));
        }
    }

    public Result<Prod?> GetWithVariantesYStock(long idProd, int idTenant)
    {
        if (Guard.Fail<Prod?>(Guard.IdInvalido(nameof(idProd), idProd)) is { } errP) return errP;
        if (Guard.Fail<Prod?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        try
        {
            // BUG FIX v3: la cadena de filtered includes (Variantes → Stocks.Where(tenant))
            // puede ignorar silenciosamente el filtro de tenant en el tercer nivel de Include
            // y traer todos los stocks de todos los tenants, o fallar según la versión del
            // proveedor EF Core. No es seguro depender de filtered includes anidados.
            //
            // SOLUCIÓN: dos queries separadas + mapeo en memoria.
            //   Query 1: Prod + Variantes + Presentaciones + Atributos (sin Stock)
            //   Query 2: todos los Stock del tenant para esas variantes
            // El mapeo en memoria asigna cada Stock a su VarProd.Stocks.

            // Query 1: estructura del producto sin stock
            var prod = DbSet.AsNoTracking()
                .Include(p => p.Marca)
                .Include(p => p.Categoria)
                .Include(p => p.UnidadBase)
                .Include(p => p.Variantes.Where(v => v.Activo))
                    .ThenInclude(v => v.Presentaciones.Where(pp => pp.Activo))
                        .ThenInclude(pp => pp.Unidad)
                .Include(p => p.Variantes.Where(v => v.Activo))
                    .ThenInclude(v => v.Atributos)
                        .ThenInclude(a => a.AtriVal)
                            .ThenInclude(av => av.AtriDef)
                .FirstOrDefault(p => p.IdProd == idProd && p.IdTenant == idTenant);

            if (prod is null)
                return Result<Prod?>.Success(null);

            // Query 2: stocks del tenant para las variantes del producto
            var idVariantes = prod.Variantes.Select(v => v.IdVarProd).ToList();

            if (idVariantes.Count > 0)
            {
                var stocks = Context.Set<Stock>()
                    .AsNoTracking()
                    .Include(s => s.Bodega)
                    .Where(s => s.IdTenant == idTenant && idVariantes.Contains(s.IdVarProd))
                    .ToList();

                // Mapeo en memoria: asignar cada Stock a su VarProd
                var stocksPorVariante = stocks.GroupBy(s => s.IdVarProd)
                    .ToDictionary(g => g.Key, g => g.ToList());

                foreach (var variante in prod.Variantes)
                {
                    if (stocksPorVariante.TryGetValue(variante.IdVarProd, out var stocksVariante))
                    {
                        // Poblar la colección de navegación manualmente
                        // (AsNoTracking no usa el change tracker, así que es seguro mutar)
                        foreach (var s in stocksVariante)
                            variante.Stocks.Add(s);
                    }
                }
            }

            return Result<Prod?>.Success(prod);
        }
        catch (Exception ex)
        {
            return Result<Prod?>.Failure(
                CreateError(ex, nameof(GetWithVariantesYStock))
                    .WithDetail("IdProd", idProd).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Prod>> GetConControlVencimiento(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Prod>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<Prod>>.Success(
                BaseQuery(idTenant, soloActivos: true)
                    .Where(p => p.DiasVen.HasValue)
                    .OrderBy(p => p.DiasVen)
                    .ThenBy(p => p.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Prod>>.Failure(
                CreateError(ex, nameof(GetConControlVencimiento)).WithDetail("IdTenant", idTenant));
        }
    }

    /// <summary>
    /// Override GetById to include Categoria and Marca navigations.
    /// This ensures product identification data is available for UI display.
    /// </summary>
    public override Result<Prod> GetById<TId>(TId id)
    {
        if (id == null)
            return Result<Prod>.Failure(CommonErrors.Validation.Required(nameof(id)));

        try
        {
            var entity = DbSet.AsNoTracking()
                .Include(p => p.Categoria)
                .Include(p => p.Marca)
                .FirstOrDefault(p => EF.Property<TId>(p, "IdProd").Equals(id));

            if (entity == null)
                return Result<Prod>.Failure(CommonErrors.NotFound.Entity(EntityName, id));

            return Result<Prod>.Success(entity);
        }
        catch (Exception ex)
        {
            return Result<Prod>.Failure(CreateError(ex, nameof(GetById)).WithDetail("Id", id));
        }
    }
}
