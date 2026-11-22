using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.06 P0: atomicidad de sp_AjustarInventario contra SQL real.
// Ruta producto: StockRepositoryAsync → SpExecutor → SP (transacción del SP).
// Cada test: setup fixture → try/op+asserts PRE-cleanup → finally cleanup → residuo 0.
// IdLote null en todos (lotes fuera de alcance §21). Legacy: s57.76 → C# COVERAGE.
[Collection("OfficialLocal")]
public sealed class AdjustmentAtomicityTests(OfficialLocalFixture fx)
{
    private static AjustarInventarioCmd Cmd(AdjustmentFixture f, int tenant, params AjusteItemCmd[] items) =>
        new(tenant, f.IdBodega, items);

    [Fact]
    public async Task EntradaValida_CreaStockDispYMovimientoT3()
    {
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        var costoVarBefore = await f.VarCostoAsync();
        try
        {
            Assert.Null(await f.CantStockAsync());
            Assert.Null(await f.CantDispAsync());

            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, AdjustmentFixture.Tenant,
                    new AjusteItemCmd(f.IdPresProd, null, 25m, 1500m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);
            Assert.Equal(25m, await f.CantStockAsync());
            Assert.Equal(25m, await f.CantDispAsync());
            Assert.Equal(1500m, await f.CostoStockAsync());

            var movs = await f.MovsAsync();
            var mov = Assert.Single(movs);
            Assert.Equal(3, mov.TipoMov); // 3 = Entrada
            Assert.Equal(25m, mov.CantCom);
            Assert.Equal(1500m, mov.CostoUni);
            Assert.Equal(f.IdPresProd, mov.IdPresProd);
            Assert.Equal(AdjustmentFixture.Tenant, mov.IdTenant);
        }
        finally
        {
            await f.CleanupAsync();
            await f.RestoreVarCostoAsync(costoVarBefore);
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task SalidaValida_DescuentaStockDispYMovimientoT4()
    {
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        try
        {
            await f.SeedExistenciaAsync(100m, 1500m);

            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, AdjustmentFixture.Tenant,
                    new AjusteItemCmd(f.IdPresProd, null, -10m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);
            Assert.Equal(90m, await f.CantStockAsync());
            Assert.Equal(90m, await f.CantDispAsync());

            var movs = await f.MovsAsync();
            var mov = Assert.Single(movs);
            Assert.Equal(4, mov.TipoMov); // 4 = Salida
            Assert.Equal(10m, mov.CantCom); // CantCom siempre positiva
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task SalidaSinStock_Falla409_SinCambios()
    {
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        try
        {
            await f.SeedExistenciaAsync(25m, 1500m);

            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, AdjustmentFixture.Tenant,
                    new AjusteItemCmd(f.IdPresProd, null, -1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);
            Assert.Equal(409, r.Error.HttpStatusCode);

            Assert.Equal(25m, await f.CantStockAsync());
            Assert.Equal(25m, await f.CantDispAsync());
            Assert.Empty(await f.MovsAsync());
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
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        try
        {
            await f.SeedExistenciaAsync(25m, 1500m);

            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, AdjustmentFixture.Tenant,
                    new AjusteItemCmd(f.IdPresProd, null, 10m, 1500m),
                    new AjusteItemCmd(f.IdPresProd, null, -1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);

            // El item válido (+10) tampoco persistió: batch atómico.
            Assert.Equal(25m, await f.CantStockAsync());
            Assert.Equal(25m, await f.CantDispAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task AjusteFallidoEnTransaccionExterna_RevierteLoAnteriorSinLanzar()
    {
        // La cantidad cero la rechaza el repo en C# antes de llegar al SP; para que el SP falle de verdad
        // (THROW 50033 fuera de su BEGIN TRAN, con XACT_ABORT ON) se pide una salida mayor a lo disponible.
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        var costoVarBefore = await f.VarCostoAsync();
        try
        {
            Assert.Null(await f.CantStockAsync());

            using var ctx = fx.NewContext();
            var stock = f.NewStockRepo(ctx);
            var confirmado = await new CatalogoEdicionRepositoryAsync(ctx).EnTransaccionAsync<bool>(async t =>
            {
                var r1 = await stock.AjustarInventarioAsync(
                    Cmd(f, AdjustmentFixture.Tenant, new AjusteItemCmd(f.IdPresProd, null, 25m, 1500m)), t);
                Assert.True(r1.IsSuccess);

                var r2 = await stock.AjustarInventarioAsync(
                    Cmd(f, AdjustmentFixture.Tenant, new AjusteItemCmd(f.IdPresProd, null, -1000m)), t);
                Assert.True(r2.IsFailure);
                Assert.Equal("STOCK_INSUFICIENTE", r2.Error!.Code);
                return (false, false);
            }, Xunit.TestContext.Current.CancellationToken);

            Assert.False(confirmado);
            // El +25 del primer ajuste se revirtió junto con el fallido.
            Assert.Null(await f.CantStockAsync());
            Assert.Null(await f.CantDispAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
            await f.RestoreVarCostoAsync(costoVarBefore);
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task TenantIncorrecto_RechazaSinEfectos()
    {
        // Por ruta repo-directa el SP solo conoce Bodega×tenant: bodega T4 no
        // existe en T5 → 50030 BODEGA_INACTIVA/400. (El 403 de producto vive en
        // InventarioServiceAsync, fuera del alcance transaccional.)
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        try
        {
            await f.SeedExistenciaAsync(25m, 1500m);

            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, 5,
                    new AjusteItemCmd(f.IdPresProd, null, 10m, 1500m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("BODEGA_INACTIVA", r.Error!.Code);
            Assert.Equal(400, r.Error.HttpStatusCode);

            Assert.Equal(25m, await f.CantStockAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task Cleanup_EliminaTodoRastroFixture()
    {
        var f = new AdjustmentFixture(fx);
        await f.SetupAsync();
        await f.SeedExistenciaAsync(25m, 1500m);

        using (var ctx = fx.NewContext())
        {
            var r = await f.NewStockRepo(ctx).AjustarInventarioAsync(
                Cmd(f, AdjustmentFixture.Tenant,
                    new AjusteItemCmd(f.IdPresProd, null, 5m, 1500m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(r.IsSuccess);
        }

        var costoVarBefore = await f.VarCostoAsync();
        await f.CleanupAsync();
        await f.RestoreVarCostoAsync(costoVarBefore);
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

