using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.08 P1: ciclo RegistrarVenta → AnularVenta contra SQL real.
// Rutas producto: VentaRepositoryAsync.RegistrarVentaAsync (S58.07) +
// VentaRepositoryAsync.AnularVentaAsync → SpExecutor → SP (transacción del SP).
// Rollback intra-txn: NO EXISTE escenario alcanzable por ruta oficial (las 3
// validaciones del SP son pre-txn por diseño; documentado en 01-MATRIX). D/E/F/G
// demuestran BEFORE==AFTER ante errores. Batch: NOT APPLICABLE (sin TVP).
[Collection("OfficialLocal")]
public sealed class CancellationAtomicityTests(OfficialLocalFixture fx)
{
    private const string Motivo = "S5808 motivo de prueba";

    [Fact]
    public async Task AnulacionValida_RevierteVenta_YMarcaEstado2()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var venta = await repo.RegistrarVentaAsync(f.VentaCmd("S5808-A", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(venta.IsSuccess);
            Assert.Equal(45m, await f.CantStockAsync()); // post-venta (BEFORE anulación)

            var r = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, venta.Value.IdVenta, Motivo), Xunit.TestContext.Current.CancellationToken);
            Assert.True(r.IsSuccess);

            var v = await repo.GetByNumVentaAsync(CancellationFixture.Tenant, "S5808-A", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(v);
            Assert.Equal(2, v!.Estado);
            Assert.Equal(Motivo, v.MotivoAnulacion);
            Assert.NotNull(v.FecAnulacion);
            Assert.Equal(10000m, v.Total); // importes intactos

            Assert.Equal(50m, await f.CantStockAsync()); // 45 + 5
            Assert.Equal(50m, await f.CantDispAsync());

            var movs = await f.MovsAsync();
            Assert.Equal(2, movs.Count);
            Assert.Equal(2, movs[0].TipoMov); // venta original intacta
            Assert.Equal(3, movs[1].TipoMov); // reversión
            Assert.Equal(5m, movs[1].CantCom);
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task AnulacionDosItems_RevierteTotal()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var venta = await repo.RegistrarVentaAsync(f.VentaCmd("S5808-B",
                f.Item(5m), f.Item(3m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(venta.IsSuccess);
            Assert.Equal(42m, await f.CantStockAsync());

            var r = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, venta.Value.IdVenta, Motivo), Xunit.TestContext.Current.CancellationToken);
            Assert.True(r.IsSuccess);

            Assert.Equal(50m, await f.CantStockAsync()); // 42 + 8
            Assert.Equal(50m, await f.CantDispAsync());

            var movs = await f.MovsAsync();
            Assert.Equal(4, movs.Count); // T2,T2 venta + T3,T3 reversión
            Assert.All(movs.Take(2), m => Assert.Equal(2, m.TipoMov));
            Assert.All(movs.Skip(2), m => Assert.Equal(3, m.TipoMov));

            var v = await repo.GetWithDetallesAsync(venta.Value.IdVenta, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(v);
            Assert.Equal(2, v!.Detalles.Count); // detalle intacto (historia)
            Assert.Single(v.Pagos);            // pago intacto
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task DobleAnulacion_SegundaFalla409_SinReaplicar()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var venta = await repo.RegistrarVentaAsync(f.VentaCmd("S5808-D", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(venta.IsSuccess);

            var anul1 = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, venta.Value.IdVenta, Motivo), Xunit.TestContext.Current.CancellationToken);
            Assert.True(anul1.IsSuccess);
            var movsPost1 = await f.MovsAsync();
            Assert.Equal(2, movsPost1.Count);

            var anul2 = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, venta.Value.IdVenta, Motivo), Xunit.TestContext.Current.CancellationToken);
            Assert.True(anul2.IsFailure);
            Assert.Equal("VENTA_ESTADO_INVALIDO", anul2.Error!.Code);
            Assert.Equal(409, anul2.Error.HttpStatusCode);

            // Sin re-aplicación: estado == post-primera.
            Assert.Equal(50m, await f.CantStockAsync());
            Assert.Equal(50m, await f.CantDispAsync());
            Assert.Equal(2, (await f.MovsAsync()).Count);
            var v = await repo.GetByNumVentaAsync(CancellationFixture.Tenant, "S5808-D", Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(2, v!.Estado);
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task VentaInexistente_Falla404_SinEfectos()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var r = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, 999999999L, Motivo), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("VENTA_NO_ENCONTRADA", r.Error!.Code);
            Assert.Equal(404, r.Error.HttpStatusCode);

            Assert.Equal(50m, await f.CantStockAsync());
            Assert.Equal(50m, await f.CantDispAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task TenantIncorrecto_Falla404_VentaIntacta()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var venta = await repo.RegistrarVentaAsync(f.VentaCmd("S5808-F", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(venta.IsSuccess);

            var r = await repo.AnularVentaAsync(new AnularVentaCmd(5, venta.Value.IdVenta, Motivo), Xunit.TestContext.Current.CancellationToken);
            Assert.True(r.IsFailure);
            Assert.Equal("VENTA_NO_ENCONTRADA", r.Error!.Code);

            var v = await repo.GetByNumVentaAsync(CancellationFixture.Tenant, "S5808-F", Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, v!.Estado); // sigue completada
            Assert.Equal(45m, await f.CantStockAsync());
            Assert.Single(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task MotivoVacio_GuardRepo400_SinEfectos()
    {
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewVentaRepo(ctx);
            var venta = await repo.RegistrarVentaAsync(f.VentaCmd("S5808-G", f.Item(5m)), Xunit.TestContext.Current.CancellationToken);
            Assert.True(venta.IsSuccess);

            var r = await repo.AnularVentaAsync(new AnularVentaCmd(CancellationFixture.Tenant, venta.Value.IdVenta, ""), Xunit.TestContext.Current.CancellationToken);
            Assert.True(r.IsFailure);
            Assert.Equal(400, r.Error!.HttpStatusCode);

            var v = await repo.GetByNumVentaAsync(CancellationFixture.Tenant, "S5808-G", Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, v!.Estado);
            Assert.Equal(45m, await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

