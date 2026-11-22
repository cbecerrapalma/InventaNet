using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.09 P1: atomicidad de sp_RecibirOrdenCompra contra SQL real.
// Rutas producto: OrdenCompraRepositoryAsync.CreateWithDetailsAsync (OC base) +
// OrdenCompraRepositoryAsync.RecibirOrdenCompraAsync → SpExecutor → SP (transacción del SP).
// Rollback intra-txn: NO EXISTE escenario alcanzable por ruta oficial (las 4 validaciones
// del SP son pre-txn por diseño; documentado en 01-MATRIX). C/D/E/F/G demuestran
// BEFORE==AFTER ante errores. Batch: SÍ (TVP; escenario C).
[Collection("OfficialLocal")]
public sealed class RecepcionAtomicityTests(OfficialLocalFixture fx)
{
    private static async Task<(long IdOC, long IdDet)> CrearOCAsync(OfficialLocalFixture fx, RecepcionFixture f, string num,
        params (long PresProd, decimal Cant, decimal Precio)[] lineas)
    {
        using var ctx = fx.NewContext();
        var repo = f.NewOCRepo(ctx);
        var oc = await repo.CreateWithDetailsAsync(RecepcionFixture.Tenant, f.IdProveedor,
            DateOnly.FromDateTime(DateTime.UtcNow), "S5809", 1, num,
            lineas.Select(l => (l.PresProd, l.Cant, l.Precio)).ToList());
        var full = await repo.GetByIdInTenantAsync(oc.IdOrdenCompra, RecepcionFixture.Tenant);
        return (oc.IdOrdenCompra, full!.Detalles.OrderBy(d => d.IdOCDetalle).First().IdOCDetalle);
    }

    [Fact]
    public async Task RecepcionTotal_ActualizaOC_YCreaStockDispMovT1()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        var costoVarBefore = await f.VarCostoAsync();
        try
        {
            var (idOC, idDet) = await CrearOCAsync(fx, f, "S5809-A", (f.IdPresProd, 10m, 1500m));

            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                RecepcionFixture.Tenant, f.IdBodega, idOC,
                [new OCRecepcionItemCmd(idDet, null, 10m, 1500m)]), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsSuccess);

            var oc = await repo.GetByIdInTenantAsync(idOC, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(3, oc!.Estado); // Recibida

            Assert.Equal(10m, await f.CantStockAsync()); // UPSERT-INSERT en bodega vacía
            Assert.Equal(10m, await f.CantDispAsync());

            var movs = await f.MovsAsync();
            var mov = Assert.Single(movs);
            Assert.Equal(1, mov.TipoMov); // 1 = Compra
            Assert.Equal(10m, mov.CantCom);
            Assert.Equal(1500m, mov.CostoUni);
        }
        finally
        {
            await f.CleanupAsync();
            await f.RestoreVarCostoAsync(costoVarBefore);
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task RecepcionParcial_OCQuedaAprobada_ConEfectosDeLineaRecibida()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        var costoVarBefore = await f.VarCostoAsync();
        try
        {
            using (var ctx = fx.NewContext())
            {
                var repo = f.NewOCRepo(ctx);
                var oc = await repo.CreateWithDetailsAsync(RecepcionFixture.Tenant, f.IdProveedor,
                    DateOnly.FromDateTime(DateTime.UtcNow), "S5809", 1, "S5809-B",
                    [(f.IdPresProd, 10m, 1500m), (f.IdPresProd, 6m, 1500m)], Xunit.TestContext.Current.CancellationToken);
                var full = await repo.GetByIdInTenantAsync(oc.IdOrdenCompra, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
                var det1 = full!.Detalles.OrderBy(d => d.IdOCDetalle).First();

                var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                    RecepcionFixture.Tenant, f.IdBodega, oc.IdOrdenCompra,
                    [new OCRecepcionItemCmd(det1.IdOCDetalle, null, 10m, 1500m)]), Xunit.TestContext.Current.CancellationToken);
                Assert.True(r.IsSuccess);

                var ocPost = await repo.GetByIdInTenantAsync(oc.IdOrdenCompra, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
                Assert.Equal(2, ocPost!.Estado); // Aprobada (parcial, venía de Pendiente)
            }

            Assert.Equal(10m, await f.CantStockAsync());
            Assert.Single(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
            await f.RestoreVarCostoAsync(costoVarBefore);
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task LoteConDetalleAjeno_NoPersisteNada()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        try
        {
            var (idOCA, idDetA) = await CrearOCAsync(fx, f, "S5809-C1", (f.IdPresProd, 10m, 1500m));
            var (_, idDetB) = await CrearOCAsync(fx, f, "S5809-C2", (f.IdPresProd, 4m, 1500m));

            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                RecepcionFixture.Tenant, f.IdBodega, idOCA,
                [new OCRecepcionItemCmd(idDetA, null, 10m, 1500m),
                 new OCRecepcionItemCmd(idDetB, null, 4m, 1500m)]), Xunit.TestContext.Current.CancellationToken); // detB no pertenece a OC-A

            Assert.True(r.IsFailure);
            Assert.Equal("OC_DETALLE_INVALIDO", r.Error!.Code);
            Assert.Equal(400, r.Error.HttpStatusCode);

            var ocA = await repo.GetByIdInTenantAsync(idOCA, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, ocA!.Estado); // sigue Pendiente
            Assert.Null(await f.CantStockAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task OrdenInexistente_Falla404_SinEfectos()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                RecepcionFixture.Tenant, f.IdBodega, 999999999L,
                [new OCRecepcionItemCmd(999999999L, null, 10m, 1500m)]), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("OC_NO_ENCONTRADA", r.Error!.Code);
            Assert.Equal(404, r.Error.HttpStatusCode);

            Assert.Null(await f.CantStockAsync());
            Assert.Empty(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task SegundaRecepcionTotal_Falla409_SinReaplicar()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        var costoVarBefore = await f.VarCostoAsync();
        try
        {
            var (idOC, idDet) = await CrearOCAsync(fx, f, "S5809-E", (f.IdPresProd, 10m, 1500m));

            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var cmd = new RecibirOrdenCompraCmd(RecepcionFixture.Tenant, f.IdBodega, idOC,
                [new OCRecepcionItemCmd(idDet, null, 10m, 1500m)]);
            Assert.True((await repo.RecibirOrdenCompraAsync(cmd, Xunit.TestContext.Current.CancellationToken)).IsSuccess);

            var dup = await repo.RecibirOrdenCompraAsync(cmd, Xunit.TestContext.Current.CancellationToken);
            Assert.True(dup.IsFailure);
            Assert.Equal("OC_ESTADO_INVALIDO", dup.Error!.Code);
            Assert.Equal(409, dup.Error.HttpStatusCode);

            Assert.Equal(10m, await f.CantStockAsync()); // sin duplicar
            Assert.Single(await f.MovsAsync());
        }
        finally
        {
            await f.CleanupAsync();
            await f.RestoreVarCostoAsync(costoVarBefore);
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task TenantIncorrecto_Falla404_OCIntacta()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        try
        {
            var (idOC, idDet) = await CrearOCAsync(fx, f, "S5809-F", (f.IdPresProd, 10m, 1500m));

            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                5, f.IdBodega, idOC,
                [new OCRecepcionItemCmd(idDet, null, 10m, 1500m)]), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal("OC_NO_ENCONTRADA", r.Error!.Code);

            var oc = await repo.GetByIdInTenantAsync(idOC, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, oc!.Estado);
            Assert.Null(await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    [Fact]
    public async Task ItemsVacios_GuardRepo400_SinEfectos()
    {
        var f = new RecepcionFixture(fx);
        await f.SetupAsync();
        try
        {
            var (idOC, _) = await CrearOCAsync(fx, f, "S5809-G", (f.IdPresProd, 10m, 1500m));

            using var ctx = fx.NewContext();
            var repo = f.NewOCRepo(ctx);
            var r = await repo.RecibirOrdenCompraAsync(new RecibirOrdenCompraCmd(
                RecepcionFixture.Tenant, f.IdBodega, idOC, []), Xunit.TestContext.Current.CancellationToken);

            Assert.True(r.IsFailure);
            Assert.Equal(400, r.Error!.HttpStatusCode);

            var oc = await repo.GetByIdInTenantAsync(idOC, RecepcionFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, oc!.Estado);
            Assert.Null(await f.CantStockAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

