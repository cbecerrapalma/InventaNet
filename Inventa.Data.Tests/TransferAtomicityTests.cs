using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.05 P0: atomicidad de sp_TransferirEntresBodegas contra SQL real.
// Ruta producto: StockRepositoryAsync → SpExecutor → SP (transacción del SP).
// Cada test: setup fixture → try/op → finally cleanup → residuo 0.
// Legacy: s57.76 (atomicidad transferencia) → C# COVERAGE (spec NO se elimina).
[Collection("OfficialLocal")]
public sealed class TransferAtomicityTests(OfficialLocalFixture fx)
{
    private static TransferirEntreBodegasCmd Cmd(TransferFixture f, params TransferenciaItemCmd[] items) =>
        new(TransferFixture.Tenant, f.IdBodegaOrig, f.IdBodegaDest, items);

    [Fact]
    public async Task TransferenciaValida_AplicaEnAmbasBodegas_YRegistraMovimientos()
    {
        var f = new TransferFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).TransferirEntresBodegasAsync(
                Cmd(f, new TransferenciaItemCmd(f.IdPresProd, null, 10m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);
            Assert.Equal(90m, await f.CantEnOrigenAsync());

            using var vctx = fx.NewContext();
            Assert.Equal(10m, await f.CantEnDestinoAsync());

            Assert.Equal(2, await f.MovCountAsync()); // salida T6 + entrada T5
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task StockInsuficiente_Falla409_SinCambios()
    {
        var f = new TransferFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).TransferirEntresBodegasAsync(
                Cmd(f, new TransferenciaItemCmd(f.IdPresProd, null, 1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);
            Assert.Equal(409, r.Error.HttpStatusCode);

            Assert.Equal(100m, await f.CantEnOrigenAsync());
            Assert.Equal(0, await f.MovCountAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task LoteConItemExcesivo_NoPersisteNadaDelLote()
    {
        // El check del SP es por lote: si UN item excede, TODO el lote revierte
        // (incluido el item válido). Prueba batch-atomicity, no solo single-item.
        var f = new TransferFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var r = await f.NewStockRepo(ctx).TransferirEntresBodegasAsync(
                Cmd(f,
                    new TransferenciaItemCmd(f.IdPresProd, null, 10m),
                    new TransferenciaItemCmd(f.IdPresProd, null, 1000m)), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("STOCK_INSUFICIENTE", r.Error!.Code);

            Assert.Equal(100m, await f.CantEnOrigenAsync());
            Assert.Equal(0, await f.MovCountAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task Fixture_Cleanup_DejaCeroResiduos()
    {
        // Garantía de aislamiento (rollback = producto; cleanup = entorno test).
        var f = new TransferFixture(fx);
        await f.SetupAsync();
        Assert.True(await f.ResidueCountAsync() > 0);
        await f.CleanupAsync();
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

