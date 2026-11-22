using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.07 P1: atomicidad de sp_RegistrarVenta contra SQL real.
// Ruta producto: VentaRepositoryAsync → SpExecutorAsync → SpExecutor → SP (transacción del SP).
// Cada test: setup fixture → try/op+asserts PRE-cleanup → finally cleanup → residuo 0.
// IdLote null, IdCliente null, TipoDoc 1 en todos.
[Collection("OfficialLocal")]
public sealed class SaleAtomicityTests(OfficialLocalFixture fx)
{
    [Fact]
    public async Task VentaValida_RegistraCabeceraDetallePagoMov_YDescuentaStock()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var r = await repo.RegistrarVentaAsync(f.Cmd("S5807-A", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);
            Assert.Equal(10000m, r.Value.Total);
            Assert.Equal(10000m, r.Value.SubTotal);

            var v = await repo.GetWithDetallesAsync(r.Value.IdVenta, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(v);
            Assert.Equal(1, v!.Estado);
            Assert.Equal(10000m, v.Total);
            var det = Assert.Single(v.Detalles);
            Assert.Equal(5m, det.Cantidad);
            Assert.Equal(10000m, det.Subtotal);
            Assert.True(det.IdMovInv > 0); // link a MovInv T2 creado por el SP
            Assert.Single(v.Pagos);

            Assert.Equal(45m, await f.CantStockAsync());
            Assert.Equal(45m, await f.CantDispAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task VentaDosItems_RegistraAmbosDetalles_YDescuentaTotal()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var r = await repo.RegistrarVentaAsync(
                f.Cmd("S5807-B", f.Item(5m), f.Item(3m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);
            Assert.Equal(16000m, r.Value.Total);

            var v = await repo.GetWithDetallesAsync(r.Value.IdVenta, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(v);
            Assert.Equal(2, v!.Detalles.Count);
            Assert.Equal(16000m, v.Detalles.Sum(d => d.Subtotal));

            Assert.Equal(42m, await f.CantStockAsync());
            Assert.Equal(42m, await f.CantDispAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task StockInsuficiente_Falla409_SinNingunEfecto()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var r = await repo.RegistrarVentaAsync(f.Cmd("S5807-C", f.Item(1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);
            Assert.Equal(409, r.Error.HttpStatusCode);

            Assert.Null(await repo.GetByNumVentaAsync(SaleFixture.Tenant, "S5807-C", Xunit.TestContext.Current.CancellationToken));
            Assert.Equal(50m, await f.CantStockAsync());
            Assert.Equal(50m, await f.CantDispAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task LoteValidoMasExcesivo_NoPersisteNadaDelLote()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var r = await repo.RegistrarVentaAsync(
                f.Cmd("S5807-D", f.Item(5m), f.Item(1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);

            Assert.Null(await repo.GetByNumVentaAsync(SaleFixture.Tenant, "S5807-D", Xunit.TestContext.Current.CancellationToken));
            Assert.Equal(50m, await f.CantStockAsync());
            Assert.Equal(50m, await f.CantDispAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task NumVentaDuplicado_SegundaFalla409_PrimeraIntacta()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var ok = await repo.RegistrarVentaAsync(f.Cmd("S5807-E", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(ok.IsSuccess);

            var dup = await repo.RegistrarVentaAsync(f.Cmd("S5807-E", f.Item(2m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(dup.IsFailure);
            Assert.Equal("VENTA_DUPLICADA", dup.Error!.Code);
            Assert.Equal(409, dup.Error.HttpStatusCode);

            // Solo la primera persistió, una sola vez.
            var v = await repo.GetByNumVentaAsync(SaleFixture.Tenant, "S5807-E", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(v);
            Assert.Equal(10000m, v!.Total);
            Assert.Equal(45m, await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task BodegaInexistente_Falla400_SinEfectos()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var cmd = new RegistrarVentaCmd(SaleFixture.Tenant, 999999, "S5807-F",
                null, 1, null, [f.Item(5m)],
                [new VentaPagoCmd(f.IdMetodoPago, 10000m, null)]);
            var r = await repo.RegistrarVentaAsync(cmd, Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("BODEGA_INACTIVA", r.Error!.Code);
            Assert.Equal(400, r.Error.HttpStatusCode);

            Assert.Null(await repo.GetByNumVentaAsync(SaleFixture.Tenant, "S5807-F", Xunit.TestContext.Current.CancellationToken));
            Assert.Equal(50m, await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task TenantIncorrecto_RechazaSinEfectos()
    {
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var cmd = new RegistrarVentaCmd(5, f.IdBodega, "S5807-G",
                null, 1, null, [f.Item(5m)],
                [new VentaPagoCmd(f.IdMetodoPago, 10000m, null)]);
            var r = await repo.RegistrarVentaAsync(cmd, Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("BODEGA_INACTIVA", r.Error!.Code);
            Assert.Equal(400, r.Error.HttpStatusCode);

            Assert.Equal(50m, await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

