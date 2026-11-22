using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S58.09: fixture oficial de escritura para sp_RecibirOrdenCompra.
// Marca: Bodega S5809-ORDEN (T4) + Proveedor S5809-PROV + NumOrden 'S5809-*'.
// OC base por OrdenCompraRepositoryAsync.CreateWithDetailsAsync (ruta producto).
// Independiente de fixtures anteriores (no se tocan).
public sealed class RecepcionFixture
{
    public const int Tenant = 4;
    public const string CodBodega = "S5809-ORDEN";
    public const string CodProveedor = "S5809-PROV";
    public const string NumPrefix = "S5809-";
    // S86: resueltos por codigo (ABA-T0001/T4) en SetupAsync, no por id fijo.
    public long IdPresProd { get; private set; }
    public long IdVarProd { get; private set; }

    private readonly OfficialLocalFixture _fx;
    public int IdBodega { get; private set; }
    public int IdProveedor { get; private set; }

    public RecepcionFixture(OfficialLocalFixture fx) => _fx = fx;

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();
        if (await ctx.Bodegas.AnyAsync(b => b.IdTenant == Tenant && b.Codigo == CodBodega)
            || await ctx.Proveedores.AnyAsync(p => p.IdTenant == Tenant && p.Codigo == CodProveedor)
            || await ctx.OrdenesCompra.AnyAsync(o => o.IdTenant == Tenant && o.NumOrden.StartsWith(NumPrefix)))
            throw new InvalidOperationException(
                "S5809: residuo de fixture previo; cleanup incompleto. STOP, no continuar.");

        var bod = new Bodega { IdTenant = Tenant, Codigo = CodBodega, Nombre = "S5809 Recepcion", Activo = true };
        var prov = new Proveedor { IdTenant = Tenant, Codigo = CodProveedor, RazonSocial = "S5809 Proveedor", Activo = true };
        ctx.Bodegas.Add(bod);
        ctx.Proveedores.Add(prov);
        await ctx.SaveChangesAsync();
        IdBodega = bod.IdBodega;
        IdProveedor = prov.IdProveedor;

        (IdVarProd, IdPresProd) = await _fx.MasterVarPresAsync();
    }

    public OrdenCompraRepositoryAsync NewOCRepo(InventaDbContext ctx) => new(ctx);

    public async Task CleanupAsync()
    {
        using var ctx = _fx.NewContext();
        var ocs = await ctx.OrdenesCompra
            .Where(o => o.IdTenant == Tenant && o.NumOrden.StartsWith(NumPrefix))
            .Select(o => o.IdOrdenCompra)
            .ToListAsync();
        if (ocs.Count > 0)
        {
            var dets = await ctx.OrdenCompraDetalle
                .Where(d => ocs.Contains(d.IdOrdenCompra))
                .Select(d => d.IdOCDetalle)
                .ToListAsync();
            ctx.MovimientosInventario.RemoveRange(
                ctx.MovimientosInventario.Where(m => m.IdTenant == Tenant && m.IdBodega == IdBodega && m.TipoMov == 1));
            ctx.OrdenCompraDetalle.RemoveRange(ctx.OrdenCompraDetalle.Where(d => ocs.Contains(d.IdOrdenCompra)));
            await ctx.SaveChangesAsync();
            ctx.OrdenesCompra.RemoveRange(ctx.OrdenesCompra.Where(o => ocs.Contains(o.IdOrdenCompra)));
            await ctx.SaveChangesAsync();
        }

        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && b.Codigo == CodBodega)
            .Select(b => b.IdBodega)
            .ToListAsync();
        ctx.Stock.RemoveRange(ctx.Stock.Where(s => bodegas.Contains(s.IdBodega)));
        ctx.StockDisponible.RemoveRange(ctx.StockDisponible.Where(s => bodegas.Contains(s.IdBodega)));
        await ctx.SaveChangesAsync();

        ctx.Bodegas.RemoveRange(ctx.Bodegas.Where(b => bodegas.Contains(b.IdBodega)));
        ctx.Proveedores.RemoveRange(ctx.Proveedores.Where(p => p.IdTenant == Tenant && p.Codigo == CodProveedor));
        await ctx.SaveChangesAsync();
    }

    public async Task<int> ResidueCountAsync()
    {
        using var ctx = _fx.NewContext();
        var ocs = await ctx.OrdenesCompra
            .Where(o => o.IdTenant == Tenant && o.NumOrden.StartsWith(NumPrefix))
            .Select(o => o.IdOrdenCompra)
            .ToListAsync();
        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && b.Codigo == CodBodega)
            .Select(b => b.IdBodega)
            .ToListAsync();
        return ocs.Count
            + await ctx.OrdenCompraDetalle.CountAsync(d => ocs.Contains(d.IdOrdenCompra))
            + await ctx.MovimientosInventario.CountAsync(m => m.IdTenant == Tenant && bodegas.Contains(m.IdBodega))
            + await ctx.Stock.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.StockDisponible.CountAsync(s => bodegas.Contains(s.IdBodega))
            + bodegas.Count
            + await ctx.Proveedores.CountAsync(p => p.IdTenant == Tenant && p.Codigo == CodProveedor);
    }

    public async Task<decimal?> CantStockAsync()
    {
        using var ctx = _fx.NewContext();
        var row = await ctx.Stock.FirstOrDefaultAsync(s =>
            s.IdTenant == Tenant && s.IdBodega == IdBodega && s.IdVarProd == IdVarProd);
        return row?.CantAct;
    }

    public async Task<decimal?> CantDispAsync()
    {
        using var ctx = _fx.NewContext();
        var row = await ctx.StockDisponible.FirstOrDefaultAsync(s =>
            s.IdTenant == Tenant && s.IdBodega == IdBodega && s.IdPresProd == IdPresProd);
        return row?.CantDisp;
    }

    public async Task<List<MovInv>> MovsAsync()
    {
        using var ctx = _fx.NewContext();
        return await ctx.MovimientosInventario
            .Where(m => m.IdTenant == Tenant && m.IdBodega == IdBodega)
            .OrderBy(m => m.IdMovInv)
            .ToListAsync();
    }

    public async Task<decimal> VarCostoAsync()
    {
        using var ctx = _fx.NewContext();
        return (await ctx.Variantes.FirstAsync(v => v.IdVarProd == IdVarProd)).CostoProm;
    }

    public async Task RestoreVarCostoAsync(decimal costo)
    {
        using var ctx = _fx.NewContext();
        (await ctx.Variantes.FirstAsync(v => v.IdVarProd == IdVarProd)).CostoProm = costo;
        await ctx.SaveChangesAsync();
    }
}

