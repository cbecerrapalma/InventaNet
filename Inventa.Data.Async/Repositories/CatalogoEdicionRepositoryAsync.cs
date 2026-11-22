using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S94 — acceso a datos de la edición del catálogo desde la UI (producto, variante, presentación 1:1 y códigos de barra).
/// Solo EF/LINQ (sin SQL en strings). Las reglas las decide <c>CatalogoEdicionServiceAsync</c> (Application.Async);
/// aquí solo se lee lo necesario, se agregan entidades y se confirma todo en UNA transacción por operación.
/// </summary>
public interface ICatalogoEdicionRepositoryAsync
{
    /// <summary>
    /// Ejecuta <paramref name="trabajo"/> dentro de una transacción. Si ya hay una abierta (p. ej. una prueba que revierte)
    /// participa de ella sin confirmar. Si <c>Confirmar</c> es <c>false</c> revierte. Reintenta una vez ante colisión de
    /// clave única de <c>UQ_CBar</c> (dos altas simultáneas que calcularon el mismo código interno).
    /// </summary>
    Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<(T Resultado, bool Confirmar)>> trabajo, CancellationToken ct = default);

    Task<Prod?> GetProductoAsync(int idTenant, long idProd, CancellationToken ct = default);
    Task<VarProd?> GetVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default);
    Task<CodBar?> GetCodigoAsync(int idTenant, long idCodBar, CancellationToken ct = default);

    Task<bool> CategoriaValidaAsync(int idTenant, long idCategoria, CancellationToken ct = default);
    Task<bool> MarcaValidaAsync(int idTenant, int idMarca, CancellationToken ct = default);
    Task<bool> UnidadExisteAsync(int idUnidad, CancellationToken ct = default);
    Task<bool> ExisteCodigoProductoAsync(int idTenant, string codigo, long? excluirIdProd, CancellationToken ct = default);
    Task<bool> ExisteCodigoVarianteAsync(long idProd, string codigo, CancellationToken ct = default);
    /// <summary>Unicidad GLOBAL (índice <c>UQ_CBar</c>): no revela a qué tenant pertenece.</summary>
    Task<bool> ExisteCodigoBarrasAsync(string codigo, CancellationToken ct = default);
    /// <summary>Mayor código interno (TipoCod 3) con el prefijo dado; todos tienen el mismo largo, así que el MAX de texto es el numérico.</summary>
    Task<string?> GetMaxCodigoInternoAsync(string prefijo, CancellationToken ct = default);
    /// <summary>¿Las presentaciones/variantes tienen stock, lotes, movimientos, ventas u órdenes de compra?</summary>
    Task<bool> TieneHistoriaAsync(IReadOnlyCollection<long> idsVarProd, IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default);

    void AgregarProducto(Prod producto);
    void AgregarVariante(VarProd variante);
    void AgregarPresentacion(PresProd presentacion);
    void AgregarCodigo(CodBar codigo);
    void QuitarCodigo(CodBar codigo);
    Task GuardarAsync(CancellationToken ct = default);
}

public sealed class CatalogoEdicionRepositoryAsync : ICatalogoEdicionRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public CatalogoEdicionRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<(T Resultado, bool Confirmar)>> trabajo, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(trabajo);
        if (_ctx.Database.CurrentTransaction is not null)
            return (await trabajo(ct)).Resultado;

        const int maxReintentos = 1;
        for (var intento = 0; ; intento++)
        {
            _ctx.ChangeTracker.Clear();
            await using var tx = await _ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.ReadCommitted, ct);
            try
            {
                var (resultado, confirmar) = await trabajo(ct);
                if (confirmar) await tx.CommitAsync(ct);
                else await tx.RollbackAsync(ct);
                return resultado;
            }
            catch (DbUpdateException ex) when (EsColisionCodigoBarras(ex) && intento < maxReintentos)
            {
                try { await tx.RollbackAsync(ct); } catch (InvalidOperationException) { /* ya revertida */ }
            }
            finally
            {
                _ctx.ChangeTracker.Clear();
            }
        }
    }

    public Task<Prod?> GetProductoAsync(int idTenant, long idProd, CancellationToken ct = default) =>
        _ctx.Productos
            .Include(p => p.Variantes).ThenInclude(v => v.Presentaciones)
            .FirstOrDefaultAsync(p => p.IdProd == idProd && p.IdTenant == idTenant, ct);

    public Task<VarProd?> GetVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default) =>
        _ctx.Variantes
            .Include(v => v.Prod)
            .Include(v => v.Presentaciones).ThenInclude(pp => pp.CodigosBarra)
            .FirstOrDefaultAsync(v => v.IdVarProd == idVarProd && v.Prod.IdTenant == idTenant, ct);

    public Task<CodBar?> GetCodigoAsync(int idTenant, long idCodBar, CancellationToken ct = default) =>
        _ctx.CodigosBarra
            .Include(c => c.PresProd).ThenInclude(pp => pp.CodigosBarra)
            .Include(c => c.PresProd).ThenInclude(pp => pp.VarProd)
            .FirstOrDefaultAsync(c => c.IdCodBar == idCodBar && c.PresProd.VarProd.Prod.IdTenant == idTenant, ct);

    public Task<bool> CategoriaValidaAsync(int idTenant, long idCategoria, CancellationToken ct = default) =>
        _ctx.Categorias.AsNoTracking().AnyAsync(c => c.IdCategoria == idCategoria && c.IdTenant == idTenant && c.Activo, ct);

    public Task<bool> MarcaValidaAsync(int idTenant, int idMarca, CancellationToken ct = default) =>
        _ctx.Marcas.AsNoTracking().AnyAsync(m => m.IdMarca == idMarca && m.IdTenant == idTenant && m.Activo, ct);

    public Task<bool> UnidadExisteAsync(int idUnidad, CancellationToken ct = default) =>
        _ctx.Unidades.AsNoTracking().AnyAsync(u => u.IdUnidad == idUnidad, ct);

    public Task<bool> ExisteCodigoProductoAsync(int idTenant, string codigo, long? excluirIdProd, CancellationToken ct = default) =>
        _ctx.Productos.AsNoTracking().AnyAsync(p => p.IdTenant == idTenant && p.Codigo == codigo && (excluirIdProd == null || p.IdProd != excluirIdProd), ct);

    public Task<bool> ExisteCodigoVarianteAsync(long idProd, string codigo, CancellationToken ct = default) =>
        _ctx.Variantes.AsNoTracking().AnyAsync(v => v.IdProd == idProd && v.Codigo == codigo, ct);

    public Task<bool> ExisteCodigoBarrasAsync(string codigo, CancellationToken ct = default) =>
        _ctx.CodigosBarra.AsNoTracking().AnyAsync(c => c.Codigo == codigo, ct);

    public Task<string?> GetMaxCodigoInternoAsync(string prefijo, CancellationToken ct = default) =>
        _ctx.CodigosBarra.AsNoTracking()
            .Where(c => c.TipoCod == 3 && c.Codigo.StartsWith(prefijo) && c.Codigo.Length == 13)
            .MaxAsync(c => (string?)c.Codigo, ct);

    public async Task<bool> TieneHistoriaAsync(IReadOnlyCollection<long> idsVarProd, IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default)
    {
        var vars = idsVarProd.Distinct().ToList();
        var pres = idsPresProd.Distinct().ToList();
        if (pres.Count > 0)
        {
            if (await _ctx.StockDisponible.AsNoTracking().AnyAsync(x => pres.Contains(x.IdPresProd), ct)) return true;
            if (await _ctx.MovimientosInventario.AsNoTracking().AnyAsync(x => pres.Contains(x.IdPresProd), ct)) return true;
            if (await _ctx.VentaDetalle.AsNoTracking().AnyAsync(x => pres.Contains(x.IdPresProd), ct)) return true;
            if (await _ctx.OrdenCompraDetalle.AsNoTracking().AnyAsync(x => pres.Contains(x.IdPresProd), ct)) return true;
        }
        if (vars.Count > 0)
        {
            if (await _ctx.Stock.AsNoTracking().AnyAsync(x => vars.Contains(x.IdVarProd), ct)) return true;
            if (await _ctx.Lotes.AsNoTracking().AnyAsync(x => vars.Contains(x.IdVarProd), ct)) return true;
        }
        return false;
    }

    public void AgregarProducto(Prod producto) => _ctx.Productos.Add(producto);
    public void AgregarVariante(VarProd variante) => _ctx.Variantes.Add(variante);
    public void AgregarPresentacion(PresProd presentacion) => _ctx.Presentaciones.Add(presentacion);
    public void AgregarCodigo(CodBar codigo) => _ctx.CodigosBarra.Add(codigo);
    public void QuitarCodigo(CodBar codigo) => _ctx.CodigosBarra.Remove(codigo);
    public Task GuardarAsync(CancellationToken ct = default) => _ctx.SaveChangesAsync(ct);

    /// <summary>Violación de clave única (2601/2627) sobre el índice global de códigos de barra.</summary>
    public static bool EsColisionCodigoBarras(DbUpdateException ex) =>
        ex.InnerException is SqlException { Number: 2601 or 2627 } sql
        && sql.Message.Contains("UQ_CBar", StringComparison.OrdinalIgnoreCase);
}
