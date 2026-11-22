using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S58.06: fixture oficial de escritura para sp_AjustarInventario.
// Marca: Bodega Codigo S5806-AJUSTE en tenant 4. Separada de TransferFixture
// (S58.05) por decisión documentada en Evidence/S58.06/02-FIXTURE-DESIGN.md:
// una sola bodega, sin dependencia accidental con transferencia.
// Setup falla si hay residuo (no continúa).
public sealed class AdjustmentFixture
{
    public const int Tenant = 4;
    public const string Codigo = "S5806-AJUSTE";
    // S86: resueltos por codigo (ABA-T0001/T4) en SetupAsync, no por id fijo.
    public long IdPresProd { get; private set; }
    public long IdVarProd { get; private set; }

    private readonly OfficialLocalFixture _fx;
    public int IdBodega { get; private set; }

    public AdjustmentFixture(OfficialLocalFixture fx) => _fx = fx;

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();
        var residue = await ctx.Bodegas.CountAsync(b =>
            b.IdTenant == Tenant && b.Codigo == Codigo);
        if (residue > 0)
            throw new InvalidOperationException(
                "S5806: residuo de fixture previo; cleanup incompleto. STOP, no continuar.");

        var bod = new Bodega { IdTenant = Tenant, Codigo = Codigo, Nombre = "S5806 Ajuste", Activo = true };
        ctx.Bodegas.Add(bod);
        await ctx.SaveChangesAsync();
        IdBodega = bod.IdBodega;

        (IdVarProd, IdPresProd) = await _fx.MasterVarPresAsync();
    }

    // Siembra mínima propia del test (existencia previa para salidas).
    public async Task SeedExistenciaAsync(decimal cantidad, decimal costo)
    {
        using var ctx = _fx.NewContext();
        ctx.Stock.Add(new Stock
        {
            IdTenant = Tenant, IdBodega = IdBodega, IdVarProd = IdVarProd,
            IdLote = null, CantAct = cantidad, CostoProm = costo,
            FecAct = DateTime.UtcNow,
        });
        ctx.StockDisponible.Add(new StockDisp
        {
            IdTenant = Tenant, IdBodega = IdBodega, IdPresProd = IdPresProd,
            CantDisp = cantidad,
        });
        await ctx.SaveChangesAsync();
    }

    public async Task CleanupAsync()
    {
        using var ctx = _fx.NewContext();
        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && b.Codigo == Codigo)
            .Select(b => b.IdBodega)
            .ToListAsync();
        if (bodegas.Count == 0) return;

        var movs = ctx.MovimientosInventario.Where(m => bodegas.Contains(m.IdBodega));
        ctx.MovimientosInventario.RemoveRange(movs);
        var stocks = ctx.Stock.Where(s => bodegas.Contains(s.IdBodega));
        ctx.Stock.RemoveRange(stocks);
        var disp = ctx.StockDisponible.Where(s => bodegas.Contains(s.IdBodega));
        ctx.StockDisponible.RemoveRange(disp);
        await ctx.SaveChangesAsync();

        var bods = ctx.Bodegas.Where(b => bodegas.Contains(b.IdBodega));
        ctx.Bodegas.RemoveRange(bods);
        await ctx.SaveChangesAsync();
    }

    public async Task<int> ResidueCountAsync()
    {
        using var ctx = _fx.NewContext();
        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && b.Codigo == Codigo)
            .Select(b => b.IdBodega)
            .ToListAsync();
        if (bodegas.Count == 0) return 0;
        return bodegas.Count
            + await ctx.Stock.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.StockDisponible.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.MovimientosInventario.CountAsync(m => bodegas.Contains(m.IdBodega));
    }

    public StockRepositoryAsync NewStockRepo(InventaDbContext ctx) => new(ctx);

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

    public async Task<decimal?> CostoStockAsync()
    {
        using var ctx = _fx.NewContext();
        var row = await ctx.Stock.FirstOrDefaultAsync(s =>
            s.IdTenant == Tenant && s.IdBodega == IdBodega && s.IdVarProd == IdVarProd);
        return row?.CostoProm;
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
        var vp = await ctx.Variantes.FirstAsync(v => v.IdVarProd == IdVarProd);
        return vp.CostoProm;
    }

    public async Task RestoreVarCostoAsync(decimal costo)
    {
        using var ctx = _fx.NewContext();
        var vp = await ctx.Variantes.FirstAsync(v => v.IdVarProd == IdVarProd);
        vp.CostoProm = costo;
        await ctx.SaveChangesAsync();
    }
}

