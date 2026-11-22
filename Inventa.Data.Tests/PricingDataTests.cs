using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.04 P0-D: pricing real en solo lectura (dbo.Precio/Oferta, sin TipoPrec).
// S86: ids resueltos por codigo (ABA-T0001/T4 → MasterVarPresAsync/MasterPresIdsAsync).
// Aserciones de pertenencia (no conteos) para no acoplarse a datos productivos.
// Legacy: s57.62 (pricing canon) → C# COVERAGE parcial (specs NO se eliminan).
[Collection("OfficialLocal")]
public sealed class PricingDataTests(OfficialLocalFixture fx)
{
    [Fact]
    public async Task BuscadorPresentaciones_SoloDelTenant_ConPrecioVigente()
    {
        // S117 D6 (crítico: aislamiento entre tenants del buscador nuevo de venta/compra y precio sugerido).
        // Usa el [Fact] que precedía a TenantDePresProd_Resuelve_En_Boundary_Data; esa prueba recibe uno nuevo abajo.
        using var ctx = fx.NewContext();
        var repo = new PresProdRepositoryAsync(ctx);
        var ct = Xunit.TestContext.Current.CancellationToken;
        var idPres = (await fx.MasterVarPresAsync()).IdPresProd;

        var (propios, total) = await repo.SearchPagedByTenantAsync(4, "ABA-T0001", paraVenta: true, idBodega: null, 1, 50, ct);
        var fila = Assert.Single(propios, r => r.IdPresProd == idPres);
        Assert.True(fila.PrecioVigente > 0);
        Assert.Equal("ABA-T0001", fila.ProdCodigo);
        Assert.True(total >= propios.Count);

        var (ajenos, _) = await repo.SearchPagedByTenantAsync(5, "ABA-T0001", paraVenta: true, idBodega: null, 1, 50, ct);
        Assert.DoesNotContain(ajenos, r => r.IdPresProd == idPres);
    }

    [Fact]
    public async Task TenantDePresProd_Resuelve_En_Boundary_Data()
    {
        using var ctx = fx.NewContext();
        var repo = new PresProdRepositoryAsync(ctx);

        Assert.Equal(4, await repo.GetTenantIdAsync((await fx.MasterVarPresAsync()).IdPresProd, Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task TenantDePresProd_Inexistente_Null()
    {
        using var ctx = fx.NewContext();
        var repo = new PresProdRepositoryAsync(ctx);

        Assert.Null(await repo.GetTenantIdAsync(999999, Xunit.TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Precios_DePresentacion_Pertenecen_A_Ella()
    {
        using var ctx = fx.NewContext();
        var repo = new PrecioRepositoryAsync(ctx);

        var idPres = (await fx.MasterVarPresAsync()).IdPresProd;
        var items = await repo.GetByPresentacionAsync(idPres, Xunit.TestContext.Current.CancellationToken);

        Assert.NotEmpty(items);
        Assert.All(items, p => Assert.Equal(idPres, p.IdPresProd));
    }

    [Fact]
    public async Task Ofertas_DePresentacion_Pertenecen_A_Ella()
    {
        using var ctx = fx.NewContext();
        var repo = new OfertaRepositoryAsync(ctx);

        var idPres = (await fx.MasterPresIdsAsync())[1];
        var items = await repo.GetByPresProdAsync(idPres, Xunit.TestContext.Current.CancellationToken);

        Assert.NotEmpty(items);
        Assert.All(items, o => Assert.Equal(idPres, o.IdPresProd));
    }
}

