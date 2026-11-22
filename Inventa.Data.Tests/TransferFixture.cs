using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S58.05: fixture oficial de escritura para transferencia entre bodegas.
// Marca: Bodegas Codigo S5805-ORIG/DEST en tenant 4. Todo lo derivado cuelga
// de esas bodegas. Setup falla si hay residuo (no continúa, §32.5).
public sealed class TransferFixture
{
    public const int Tenant = 4;
    public const string CodOrig = "S5805-ORIG";
    public const string CodDest = "S5805-DEST";
    // S86: resueltos por codigo (ABA-T0001/T4) en SetupAsync, no por id fijo.
    public long IdPresProd { get; private set; }
    public long IdVarProd { get; private set; }
    public const decimal StockInicial = 100m;
    public const decimal CostoInicial = 1500m;

    private readonly OfficialLocalFixture _fx;
    public int IdBodegaOrig { get; private set; }
    public int IdBodegaDest { get; private set; }

    public TransferFixture(OfficialLocalFixture fx) => _fx = fx;

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();
        var residue = await ctx.Bodegas.CountAsync(b =>
            b.IdTenant == Tenant && (b.Codigo == CodOrig || b.Codigo == CodDest));
        if (residue > 0)
            throw new InvalidOperationException(
                "S5805: residuo de fixture previo; cleanup incompleto. STOP, no continuar.");

        var orig = new Bodega { IdTenant = Tenant, Codigo = CodOrig, Nombre = "S5805 Origen", Activo = true };
        var dest = new Bodega { IdTenant = Tenant, Codigo = CodDest, Nombre = "S5805 Destino", Activo = true };
        ctx.Bodegas.AddRange(orig, dest);
        await ctx.SaveChangesAsync();
        IdBodegaOrig = orig.IdBodega;
        IdBodegaDest = dest.IdBodega;

        (IdVarProd, IdPresProd) = await _fx.MasterVarPresAsync();

        ctx.Stock.Add(new Stock
        {
            IdTenant = Tenant, IdBodega = IdBodegaOrig, IdVarProd = IdVarProd,
            IdLote = null, CantAct = StockInicial, CostoProm = CostoInicial,
            FecAct = DateTime.UtcNow,
        });
        ctx.StockDisponible.Add(new StockDisp
        {
            IdTenant = Tenant, IdBodega = IdBodegaOrig, IdPresProd = IdPresProd,
            CantDisp = StockInicial,
        });
        await ctx.SaveChangesAsync();
    }

    public async Task CleanupAsync()
    {
        using var ctx = _fx.NewContext();
        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && (b.Codigo == CodOrig || b.Codigo == CodDest))
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
            .Where(b => b.IdTenant == Tenant && (b.Codigo == CodOrig || b.Codigo == CodDest))
            .Select(b => b.IdBodega)
            .ToListAsync();
        if (bodegas.Count == 0) return 0;
        return bodegas.Count
            + await ctx.Stock.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.StockDisponible.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.MovimientosInventario.CountAsync(m => bodegas.Contains(m.IdBodega));
    }

    public StockRepositoryAsync NewStockRepo(InventaDbContext ctx) => new(ctx);

    public async Task<decimal?> CantEnOrigenAsync()
    {
        using var ctx = _fx.NewContext();
        var row = await ctx.Stock.FirstOrDefaultAsync(s =>
            s.IdTenant == Tenant && s.IdBodega == IdBodegaOrig && s.IdVarProd == IdVarProd);
        return row?.CantAct;
    }

    public async Task<decimal?> CantEnDestinoAsync()
    {
        using var ctx = _fx.NewContext();
        var row = await ctx.Stock.FirstOrDefaultAsync(s =>
            s.IdTenant == Tenant && s.IdBodega == IdBodegaDest && s.IdVarProd == IdVarProd);
        return row?.CantAct;
    }

    public async Task<int> MovCountAsync()
    {
        using var ctx = _fx.NewContext();
        return await ctx.MovimientosInventario.CountAsync(m =>
            m.IdTenant == Tenant &&
            (m.IdBodega == IdBodegaOrig || m.IdBodega == IdBodegaDest));
    }
}

