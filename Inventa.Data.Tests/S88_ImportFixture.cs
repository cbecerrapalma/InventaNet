using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

/// <summary>
/// S88: fixture para tests de importación (SP + repo + aplicador) en tenant 4 (ABARROTES).
/// Crea datos mínimos: categoría, marca, unidad, bodega, proveedor.
/// Limpia residuos en Dispose.
/// </summary>
public sealed class S88ImportFixture : IAsyncDisposable
{
    private readonly OfficialLocalFixture _fx;
    private bool _disposed;

    public const int Tenant = 4; // ABARROTES

    public long IdCategoria { get; private set; }
    public int IdMarca { get; private set; }
    public int IdUnidad { get; private set; }
    public int IdBodega { get; private set; }
    public int IdProveedor { get; private set; }
    public int IdUsuario { get; private set; } = 3; // admin_abarrotes en tenant 4

    public S88ImportFixture(OfficialLocalFixture fx) => _fx = fx;

    public InventaDbContext NewContext() => _fx.NewContext();

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();

        // S91.2: el fixture no crea lotes de importación (los tests llaman al SP directamente), así que
        // no los verifica ni los borra: los lotes del tenant son datos de uso real de la UI.

        // Crear/obtener datos mínimos
        var cat = await ctx.Categorias.FirstOrDefaultAsync(c => c.IdTenant == Tenant && c.Codigo == "S88-CAT");
        if (cat is null)
        {
            cat = new Categoria { IdTenant = Tenant, Codigo = "S88-CAT", Nombre = "S88 Import Test", Activo = true };
            ctx.Categorias.Add(cat);
            await ctx.SaveChangesAsync();
        }
        IdCategoria = cat.IdCategoria;

        var marca = await ctx.Marcas.FirstOrDefaultAsync(m => m.IdTenant == Tenant && m.Codigo == "S88-MAR");
        if (marca is null)
        {
            marca = new Marca { IdTenant = Tenant, Codigo = "S88-MAR", Nombre = "S88 Marca Test", Activo = true };
            ctx.Marcas.Add(marca);
            await ctx.SaveChangesAsync();
        }
        IdMarca = marca.IdMarca;

        var unid = await ctx.Unidades.FirstOrDefaultAsync(u => u.Codigo == "S88-UN");
        if (unid is null)
        {
            unid = new Unidad { Codigo = "S88-UN", Nombre = "S88 Unidad Test" };
            ctx.Unidades.Add(unid);
            await ctx.SaveChangesAsync();
        }
        IdUnidad = unid.IdUnidad;

        var bod = await ctx.Bodegas.FirstOrDefaultAsync(b => b.IdTenant == Tenant && b.Codigo == "S88-BOD");
        if (bod is null)
        {
            bod = new Bodega { IdTenant = Tenant, Codigo = "S88-BOD", Nombre = "S88 Bodega Test", Activo = true };
            ctx.Bodegas.Add(bod);
            await ctx.SaveChangesAsync();
        }
        IdBodega = bod.IdBodega;

        var prov = await ctx.Proveedores.FirstOrDefaultAsync(p => p.IdTenant == Tenant && p.Codigo == "S88-PROV");
        if (prov is null)
        {
            prov = new Proveedor { IdTenant = Tenant, Codigo = "S88-PROV", RazonSocial = "S88 Proveedor Test", Activo = true };
            ctx.Proveedores.Add(prov);
            await ctx.SaveChangesAsync();
        }
        IdProveedor = prov.IdProveedor;
    }

    public IProductoImportSpRepositoryAsync NewSpRepo(InventaDbContext ctx) => new ProductoImportSpRepositoryAsync(ctx);

    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        using var ctx = _fx.NewContext();
        using var tx = await ctx.Database.BeginTransactionAsync();
        try
        {
            // S88.2: limpieza con EF/LINQ puro (cero SQL en strings, §0 DATA ACCESS RULE).
            // Cubre Oferta/PrecioMayorista (Restrict) antes de PresProd.
            var prodIds = await ctx.Productos
                .Where(p => p.IdTenant == S88ImportFixture.Tenant && p.Codigo.StartsWith("S88-"))
                .Select(p => p.IdProd)
                .ToListAsync();

            if (prodIds.Count > 0)
            {
                var varIds = await ctx.Variantes
                    .Where(v => prodIds.Contains(v.IdProd))
                    .Select(v => v.IdVarProd)
                    .ToListAsync();

                var presIds = varIds.Count > 0
                    ? await ctx.Presentaciones
                        .Where(pp => varIds.Contains(pp.IdVarProd))
                        .Select(pp => pp.IdPresProd)
                        .ToListAsync()
                    : new List<long>();

                if (presIds.Count > 0)
                {
                    ctx.MovimientosInventario.RemoveRange(
                        await ctx.MovimientosInventario.Where(m => presIds.Contains(m.IdPresProd)).ToListAsync());
                    ctx.StockDisponible.RemoveRange(
                        await ctx.StockDisponible.Where(s => presIds.Contains(s.IdPresProd)).ToListAsync());
                    ctx.Ofertas.RemoveRange(
                        await ctx.Ofertas.Where(o => presIds.Contains(o.IdPresProd)).ToListAsync());
                    ctx.PreciosMayorista.RemoveRange(
                        await ctx.PreciosMayorista.Where(m => presIds.Contains(m.IdPresProd)).ToListAsync());
                    ctx.Precios.RemoveRange(
                        await ctx.Precios.Where(p => presIds.Contains(p.IdPresProd)).ToListAsync());
                    ctx.CodigosBarra.RemoveRange(
                        await ctx.CodigosBarra.Where(c => presIds.Contains(c.IdPresProd)).ToListAsync());
                    ctx.PromocionPresentaciones.RemoveRange(
                        await ctx.PromocionPresentaciones.Where(p => presIds.Contains(p.IdPresProd)).ToListAsync());
                }

                if (varIds.Count > 0)
                {
                    ctx.Stock.RemoveRange(
                        await ctx.Stock.Where(s => varIds.Contains(s.IdVarProd)).ToListAsync());
                }

                ctx.ProductoImagenes.RemoveRange(
                    await ctx.ProductoImagenes.Where(i => prodIds.Contains(i.IdProd)).ToListAsync());

                ctx.Presentaciones.RemoveRange(
                    await ctx.Presentaciones.Where(pp => prodIds.Contains(pp.VarProd.IdProd)).ToListAsync());
                ctx.Variantes.RemoveRange(
                    await ctx.Variantes.Where(v => prodIds.Contains(v.IdProd)).ToListAsync());
                ctx.Productos.RemoveRange(
                    await ctx.Productos.Where(p => prodIds.Contains(p.IdProd)).ToListAsync());
            }

            // S91: limpieza TOTAL de maestros de prueba, incluidos S88-CAT/S88-MAR
            // del setup (cada test los recrea en SetupAsync; misma Collection → secuencial).
            var catsTest = await ctx.Categorias
                .Where(c => c.IdTenant == S88ImportFixture.Tenant && c.Codigo.StartsWith("S88-"))
                .ToListAsync();
            // Solo borrar si no tienen productos (los S88-* ya se borraron arriba)
            foreach (var c in catsTest)
            {
                var conProd = await ctx.Productos.AnyAsync(p => p.IdCategoria == c.IdCategoria);
                if (!conProd) ctx.Categorias.Remove(c);
            }
            var marcasTest = await ctx.Marcas
                .Where(m => m.IdTenant == S88ImportFixture.Tenant && m.Codigo.StartsWith("S88-"))
                .ToListAsync();
            foreach (var m in marcasTest)
            {
                var conProd = await ctx.Productos.AnyAsync(p => p.IdMarca == m.IdMarca);
                if (!conProd) ctx.Marcas.Remove(m);
            }
            var bodsTest = await ctx.Bodegas
                .Where(b => b.IdTenant == S88ImportFixture.Tenant && b.Codigo.StartsWith("S88-"))
                .ToListAsync();
            foreach (var b in bodsTest)
            {
                var enUso = await ctx.Stock.AnyAsync(s => s.IdBodega == b.IdBodega)
                    || await ctx.MovimientosInventario.AnyAsync(m => m.IdBodega == b.IdBodega);
                if (!enUso) ctx.Bodegas.Remove(b);
            }
            var provsTest = await ctx.Proveedores
                .Where(p => p.IdTenant == S88ImportFixture.Tenant && p.Codigo.StartsWith("S88-"))
                .ToListAsync();
            foreach (var p in provsTest)
            {
                var enUso = await ctx.OrdenesCompra.AnyAsync(o => o.IdProveedor == p.IdProveedor);
                if (!enUso) ctx.Proveedores.Remove(p);
            }
            var unidsTest = await ctx.Unidades
                .Where(u => u.Codigo.StartsWith("S88-"))
                .ToListAsync();
            foreach (var u in unidsTest)
            {
                var enUso = await ctx.Productos.AnyAsync(p => p.IdUniBase == u.IdUnidad)
                    || await ctx.Presentaciones.AnyAsync(pp => pp.IdUnidad == u.IdUnidad);
                if (!enUso) ctx.Unidades.Remove(u);
            }

            await ctx.SaveChangesAsync();
            await tx.CommitAsync();
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }
}
