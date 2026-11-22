using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Data.Tests;

// S58.03 P0-B (corregido S58.04 Gate 1): tenant-scoping real contra
// OfficialLocal, SOLO LECTURA. Master = producto 1 / tenant 4 (ABARROTES).
// Garantía migrada: PLAYWRIGHT s56.25 (tenant isolation) → C# COVERAGE
// (el spec Playwright NO se elimina en este sprint).
[Collection("OfficialLocal")]
public sealed class TenantIsolationTests(OfficialLocalFixture fx)
{
    [Fact]
    public async Task Master_Legible_En_Su_Tenant()
    {
        using var ctx = fx.NewContext();
        var repo = new ProdRepositoryAsync(ctx);

        var p = await repo.GetByIdInTenantAsync(
            await fx.MasterIdProdAsync(), OfficialLocalFixture.MasterIdTenant, Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(p);
        Assert.Equal(OfficialLocalFixture.MasterIdTenant, p!.IdTenant);
    }

    [Fact]
    public async Task Master_Invisible_Desde_Otro_Tenant()
    {
        using var ctx = fx.NewContext();
        var repo = new ProdRepositoryAsync(ctx);

        var p = await repo.GetByIdInTenantAsync(
            await fx.MasterIdProdAsync(), OfficialLocalFixture.OtroTenant, Xunit.TestContext.Current.CancellationToken);

        Assert.Null(p);
    }

    [Fact]
    public async Task Listado_Solo_Contiene_Su_Tenant()
    {
        using var ctx = fx.NewContext();
        var repo = new ProdRepositoryAsync(ctx);

        var items = await repo.GetByTenantAsync(OfficialLocalFixture.MasterIdTenant, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.NotEmpty(items);
        Assert.All(items, p => Assert.Equal(OfficialLocalFixture.MasterIdTenant, p.IdTenant));
    }

    [Fact]
    public async Task Master_ConVariantes_Mantiene_Tenant()
    {
        using var ctx = fx.NewContext();
        var repo = new ProdRepositoryAsync(ctx);

        var p = await repo.GetWithVariantesAsync(
            await fx.MasterIdProdAsync(), OfficialLocalFixture.MasterIdTenant, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(p);
        Assert.Equal(OfficialLocalFixture.MasterIdTenant, p!.IdTenant);
    }
}