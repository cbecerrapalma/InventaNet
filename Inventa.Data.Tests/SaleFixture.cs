using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S58.07: fixture oficial de escritura para sp_RegistrarVenta.
// Marca: Bodega Codigo S5807-VENTA (T4) + MetodoPago sembrado EFE (S90, solo lectura) +
// Venta.NumVenta 'S5807-%'. Todo lo derivado cuelga de esos registros.
// Setup falla si hay residuo (no continúa).
public sealed class SaleFixture
{
    public const int Tenant = 4;
    public const string CodBodega = "S5807-VENTA";
    // S90: metodo de pago SEMBRADO (S86), resuelto por codigo. No se inserta ni se borra:
    // insertar uno por corrida agotaba la identidad tinyint de MetodoPago.
    public const string CodMetodo = "EFE";
    public const string NumPrefix = "S5807-";
    // S86: resueltos por codigo (ABA-T0001/T4) en SetupAsync, no por id fijo.
    public long IdPresProd { get; private set; }
    public long IdVarProd { get; private set; }
    public const decimal Existencia = 50m;
    public const decimal CostoInicial = 1500m;
    public const decimal Precio = 2000m;

    private readonly OfficialLocalFixture _fx;
    public int IdBodega { get; private set; }
    public byte IdMetodoPago { get; private set; }

    public SaleFixture(OfficialLocalFixture fx) => _fx = fx;

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();
        if (await ctx.Bodegas.AnyAsync(b => b.IdTenant == Tenant && b.Codigo == CodBodega)
            || await ctx.Ventas.AnyAsync(v => v.IdTenant == Tenant && v.NumVenta.StartsWith(NumPrefix)))
            throw new InvalidOperationException(
                "S5807: residuo de fixture previo; cleanup incompleto. STOP, no continuar.");

        var bod = new Bodega { IdTenant = Tenant, Codigo = CodBodega, Nombre = "S5807 Venta", Activo = true };
        ctx.Bodegas.Add(bod);
        await ctx.SaveChangesAsync();
        IdBodega = bod.IdBodega;
        IdMetodoPago = await ctx.MetodosPago
            .Where(m => m.Codigo == CodMetodo && m.Activo)
            .Select(m => m.IdMetodoPago)
            .SingleAsync();

        (IdVarProd, IdPresProd) = await _fx.MasterVarPresAsync();

        ctx.Stock.Add(new Stock
        {
            IdTenant = Tenant, IdBodega = IdBodega, IdVarProd = IdVarProd,
            IdLote = null, CantAct = Existencia, CostoProm = CostoInicial,
            FecAct = DateTime.UtcNow,
        });
        ctx.StockDisponible.Add(new StockDisp
        {
            IdTenant = Tenant, IdBodega = IdBodega, IdPresProd = IdPresProd,
            CantDisp = Existencia,
        });
        await ctx.SaveChangesAsync();
    }

    public async Task CleanupAsync()
    {
        using var ctx = _fx.NewContext();
        var ventas = await ctx.Ventas
            .Where(v => v.IdTenant == Tenant && v.NumVenta.StartsWith(NumPrefix))
            .Select(v => v.IdVenta)
            .ToListAsync();
        if (ventas.Count > 0)
        {
            ctx.VentaPagos.RemoveRange(ctx.VentaPagos.Where(p => ventas.Contains(p.IdVenta)));
            ctx.VentaDetalle.RemoveRange(ctx.VentaDetalle.Where(d => ventas.Contains(d.IdVenta)));
            await ctx.SaveChangesAsync(); // FK_VentaDet_Mov: detalle referencia MovInv
            ctx.MovimientosInventario.RemoveRange(
                ctx.MovimientosInventario.Where(m => m.IdTenant == Tenant && m.IdBodega == IdBodega && m.TipoMov == 2));
            ctx.Ventas.RemoveRange(ctx.Ventas.Where(v => ventas.Contains(v.IdVenta)));
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
        await ctx.SaveChangesAsync();
    }

    public async Task<int> ResidueCountAsync()
    {
        using var ctx = _fx.NewContext();
        var ventas = await ctx.Ventas
            .Where(v => v.IdTenant == Tenant && v.NumVenta.StartsWith(NumPrefix))
            .Select(v => v.IdVenta)
            .ToListAsync();
        var bodegas = await ctx.Bodegas
            .Where(b => b.IdTenant == Tenant && b.Codigo == CodBodega)
            .Select(b => b.IdBodega)
            .ToListAsync();
        return ventas.Count
            + await ctx.VentaDetalle.CountAsync(d => ventas.Contains(d.IdVenta))
            + await ctx.VentaPagos.CountAsync(p => ventas.Contains(p.IdVenta))
            + await ctx.MovimientosInventario.CountAsync(m => m.IdTenant == Tenant && bodegas.Contains(m.IdBodega))
            + await ctx.Stock.CountAsync(s => bodegas.Contains(s.IdBodega))
            + await ctx.StockDisponible.CountAsync(s => bodegas.Contains(s.IdBodega))
            + bodegas.Count;
    }

    public VentaRepositoryAsync NewVentaRepo(InventaDbContext ctx) => new(ctx);

    public RegistrarVentaCmd Cmd(string num, params VentaItemCmd[] items) =>
        new(Tenant, IdBodega, num, null, 1, null, items,
            [new VentaPagoCmd(IdMetodoPago, items.Sum(i => i.Cantidad * i.PrecioUnitario - i.Descuento), null)]);

    public VentaItemCmd Item(decimal cant, decimal precio = Precio, decimal dto = 0m) =>
        new(IdPresProd, null, cant, precio, dto);

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
}

