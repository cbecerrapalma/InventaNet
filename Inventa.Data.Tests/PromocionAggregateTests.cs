using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

// S58.10: atomicidad del agregado Promocion contra SQL real.
// SUT (repository contract, sin SP en este flujo):
//   PromocionRepositoryAsync.CreateAggregateAsync / DeleteAggregateAsync → EF txn.
// CREATE = cobertura de contrato de repositorio (sin caller service/controller; declarado).
// DELETE posee ruta oficial PromocionesController → PromocionServiceAsync.DeleteAsync → repo;
//   el test ejercita el método del repositorio en Inventa.Data.Tests (mismo nivel S58.05-09).
// TEST E = tenant BOUNDARY de persistencia (consultas tenant-aware), NO autorización:
//   DeleteAggregateAsync no recibe IdTenant; el 404 de DeleteAsync pertenece a
//   Application/Service, fuera del alcance S58.10.
[Collection("OfficialLocal")]
public sealed class PromocionAggregateTests(OfficialLocalFixture fx)
{
    // A — CREATE happy path: 4 filas asociadas, commit real.
    [Fact]
    public async Task CreateAggregate_PersisteCabeceraYTresHijos()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var result = await f.NewRepo(ctx).CreateAggregateAsync(
                PromocionFixture.Header("S5810-A"),
                [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)],
                [PromocionFixture.Regla()],
                [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken);

            Assert.True(result.IdPromocion > 0);
            var (cab, pres, reg, ben) = await f.CountAsync("S5810-A");
            Assert.Equal((1, 1, 1, 1), (cab, pres, reg, ben));

            using var verify = fx.NewContext();
            var full = await f.NewRepo(verify).GetByTenantAndCodigoAsync(PromocionFixture.Tenant, "S5810-A", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(full);
            Assert.Equal(PromocionFixture.Tenant, full!.IdTenant);
            Assert.Single(await verify.PromocionPresentaciones
                .Where(p => p.IdPromocion == full.IdPromocion && p.IdPresProd == f.IdPresProd)
                .ToListAsync(Xunit.TestContext.Current.CancellationToken));
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // B — DELETE happy path: elimina las 4 filas, commit real.
    [Fact]
    public async Task DeleteAggregate_EliminaCabeceraYTresHijos()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            long id;
            using (var ctx = fx.NewContext())
            {
                var created = await f.NewRepo(ctx).CreateAggregateAsync(
                    PromocionFixture.Header("S5810-B"),
                    [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)],
                    [PromocionFixture.Regla()],
                    [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken);
                id = created.IdPromocion;
            }
            Assert.Equal((1, 1, 1, 1), await f.CountAsync("S5810-B"));

            using (var ctx = fx.NewContext())
            {
                var repo = f.NewRepo(ctx);
                var entity = await repo.GetByTenantAndCodigoAsync(PromocionFixture.Tenant, "S5810-B", Xunit.TestContext.Current.CancellationToken);
                Assert.NotNull(entity);
                await repo.DeleteAggregateAsync(entity!, Xunit.TestContext.Current.CancellationToken);
            }

            Assert.Equal((0, 0, 0, 0), await f.CountAsync("S5810-B"));
            using var verify = fx.NewContext();
            Assert.Null(await verify.Promociones.FirstOrDefaultAsync(p => p.IdPromocion == id, Xunit.TestContext.Current.CancellationToken));
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // C — ROLLBACK REAL: FK inválida tras INSERT padre revierte TODO (verificado pre-cleanup).
    [Fact]
    public async Task CreateConHijoInvalido_RevierteCabeceraYCeroFilas()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            await Assert.ThrowsAsync<DbUpdateException>(() => f.NewRepo(ctx).CreateAggregateAsync(
                PromocionFixture.Header("S5810-C"),
                [PromocionFixture.Pres(idPresProd: 999999999, idPrecioRef: f.IdPrecioRef)], // FK inexistente (verificado Gate 0 + pre-check)
                [PromocionFixture.Regla()],
                [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken));

            // Evidencia de rollback ANTES del cleanup: cero filas en las 4 tablas.
            Assert.Equal((0, 0, 0, 0), await f.CountAsync("S5810-C"));
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // D — DUPLICADO: UQ_Promocion_CodigoTenant; el original permanece intacto.
    // Nota: Service.CreateAsync traduciría a PROMOCION_CODIGO_DUPLICADO/409; aquí SUT = repo.
    [Fact]
    public async Task CreateDuplicado_FallaYOriginalIntacto()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using (var ctx = fx.NewContext())
            {
                await f.NewRepo(ctx).CreateAggregateAsync(
                    PromocionFixture.Header("S5810-D"),
                    [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)],
                    [PromocionFixture.Regla()],
                    [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken);
            }

            using (var ctx = fx.NewContext())
            {
                await Assert.ThrowsAsync<DbUpdateException>(() => f.NewRepo(ctx).CreateAggregateAsync(
                    PromocionFixture.Header("S5810-D"),
                    [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)],
                    [PromocionFixture.Regla()],
                    [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken));
            }

            Assert.Equal((1, 1, 1, 1), await f.CountAsync("S5810-D"));
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // E — TENANT BOUNDARY (persistencia): T5 no ve la promo T4 vía query tenant-aware;
    // T4 sí. NO usa DeleteAggregateAsync (sin IdTenant; el 404 es de Service, fuera de alcance).
    [Fact]
    public async Task TenantBoundary_T5NoVePromoT4()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using (var ctx = fx.NewContext())
            {
                await f.NewRepo(ctx).CreateAggregateAsync(
                    PromocionFixture.Header("S5810-E"),
                    [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)],
                    [PromocionFixture.Regla()],
                    [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken);
            }

            using var ctx2 = fx.NewContext();
            var repo = f.NewRepo(ctx2);
            Assert.Null(await repo.GetByTenantAndCodigoAsync(5, "S5810-E", Xunit.TestContext.Current.CancellationToken));
            Assert.Empty(await repo.GetByTenantAsync(5, Xunit.TestContext.Current.CancellationToken));
            var own = await repo.GetByTenantAndCodigoAsync(PromocionFixture.Tenant, "S5810-E", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(own);
            Assert.Equal((1, 1, 1, 1), await f.CountAsync("S5810-E")); // intacta, cero modificaciones
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // F — DELETE inexistente: no-op sin excepción (FirstOrDefault null → return).
    [Fact]
    public async Task DeleteInexistente_NoOpSinExcepcion()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var ex = await Record.ExceptionAsync(() =>
                f.NewRepo(ctx).DeleteAggregateAsync(new Promocion { IdPromocion = 999999999 }, Xunit.TestContext.Current.CancellationToken));
            Assert.Null(ex);
            Assert.Equal(0, await f.ResidueCountAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // G — GUARDS pre-txn: null en cada argumento → ArgumentNullException, cero efectos.
    [Fact]
    public async Task GuardsNull_ArgumentNullYCeroEfectos()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = fx.NewContext();
            var repo = f.NewRepo(ctx);
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAggregateAsync(
                null!, [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)], [PromocionFixture.Regla()], [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAggregateAsync(
                PromocionFixture.Header("S5810-G"), null!, [PromocionFixture.Regla()], [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAggregateAsync(
                PromocionFixture.Header("S5810-G"), [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)], null!, [PromocionFixture.Beneficio()], Xunit.TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.CreateAggregateAsync(
                PromocionFixture.Header("S5810-G"), [PromocionFixture.Pres(f.IdPresProd, idPrecioRef: f.IdPrecioRef)], [PromocionFixture.Regla()], null!, Xunit.TestContext.Current.CancellationToken));
            await Assert.ThrowsAsync<ArgumentNullException>(() => repo.DeleteAggregateAsync(null!, Xunit.TestContext.Current.CancellationToken));
            Assert.Equal(0, await f.ResidueCountAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }

    // H — RESIDUE: sonda de limpieza global S5810 + fail-fast de maestros.
    [Fact]
    public async Task ResidueProbe_SinResiduoS5810YMaestrosDisponibles()
    {
        var f = new PromocionFixture(fx);
        await f.SetupAsync(); // incluye fail-fast: conexión, master ABA-T0001, residue 0
        try
        {
            Assert.Equal(0, await f.ResidueCountAsync());
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
    }
}

