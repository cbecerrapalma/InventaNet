using Inventa.Data.Async;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Data.Repositories.Internal;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

// S75 §4/§8: integración de errores por la ruta NUEVA (SpExecutor → CBP
// ExecuteSPRaw/ExecuteSPRawAsync → SpErrorMapper). Anular una venta inexistente
// provoca THROW 50010 real del SP; se verifica el contrato (código + HTTP).
// Cero escrituras (OfficialLocal).
[Collection("OfficialLocal")]
public sealed class S75SpErrorIntegrationTests : IDisposable
{
    private readonly OfficialLocalFixture _fx;
    private readonly InventaDbContext _ctx;

    public S75SpErrorIntegrationTests(OfficialLocalFixture fx)
    {
        _fx = fx;
        var options = new DbContextOptionsBuilder<InventaDbContext>()
            .UseSqlServer(OfficialLocalFixture.ConnectionString).Options;
        _ctx = new InventaDbContext(options);
    }

    public void Dispose() => _ctx.Dispose();

    private int ResolveTenant() =>
        _ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == 3)
            .Select(t => t.IdTenant).Single();

    [Fact]
    public void AnularVenta_Inexistente_Sync_Devuelve_404()
    {
        var r = SpExecutor.AnularVenta(_ctx,
            new AnularVentaRequest(ResolveTenant(), long.MaxValue, "prueba S75"));
        Assert.True(r.IsFailure);
        Assert.Equal("VENTA_NO_ENCONTRADA", r.Error!.Code);
        Assert.Equal(404, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task AnularVenta_Inexistente_Async_Devuelve_404()
    {
        var r = await SpExecutorAsync.AnularVentaAsync(_ctx,
            new AnularVentaRequest(ResolveTenant(), long.MaxValue, "prueba S75"), Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsFailure);
        Assert.Equal("VENTA_NO_ENCONTRADA", r.Error!.Code);
        Assert.Equal(404, r.Error.HttpStatusCode);
    }
}

