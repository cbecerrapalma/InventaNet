using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S93.1 C3/C9: aplicación atómica de operaciones de precio contra SQL real.
/// Las escrituras se hacen DENTRO de una transacción abierta por la prueba y se revierten:
/// la prueba no deja filas en localhost/Inventa.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S931_PreciosEscrituraDataTests(OfficialLocalFixture fx)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static int Abarrotes(Inventa.Data.Context.InventaDbContext ctx) =>
        ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == 2).Select(t => t.IdTenant).Single();

    [Fact]
    public async Task MantenerOferta_AlCambiarElPrecio_EnUnaTransaccion_YRevertir()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var repo = new PreciosGrillaRepositoryAsync(ctx);
        var hoy = PublicCatalogSpecs.Hoy();
        var (conOferta, _) = await repo.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Estado: FiltroEstadoPrecio.ConOferta), hoy, 1, 1, Ct);
        var antes = Assert.Single(conOferta);
        var idOferta = antes.IdOferta!.Value;
        var desdeOferta = antes.OfertaDesde!.Value;
        var nuevoValor = antes.PrecioValor!.Value + 100;
        var nPrecios = await ctx.Precios.CountAsync(Ct);
        var nOfertas = await ctx.Ofertas.CountAsync(Ct);

        await using (var tx = await ctx.Database.BeginTransactionAsync(Ct))
        {
            var ops = new List<OperacionPrecio>
            {
                new OpInsertarPrecio(antes.IdPresProd, 1, nuevoValor, hoy),
                desdeOferta < hoy
                    ? new OpCerrarOferta(antes.IdPresProd, idOferta, hoy.AddDays(-1))
                    : new OpAnularOferta(antes.IdPresProd, idOferta),
                new OpInsertarOferta(antes.IdPresProd, null, 1, hoy, antes.OfertaHasta, antes.OfertaPrecio!.Value, 12.5m),
            };
            var (resultado, nuevos) = await repo.EjecutarAsync([antes.IdPresProd], _ => (ops, "ok"), Ct);
            Assert.Equal("ok", resultado);
            var idNuevo = Assert.Single(nuevos).Value;

            var despues = Assert.Single(await repo.GetPorIdsAsync(tenant, [antes.IdPresProd], hoy, Ct));
            Assert.Equal(idNuevo, despues.IdPrecio);
            Assert.Equal(nuevoValor, despues.PrecioValor);
            Assert.NotNull(despues.IdOferta);
            Assert.NotEqual(idOferta, despues.IdOferta);
            Assert.Equal(antes.OfertaPrecio, despues.OfertaPrecio);
            var nueva = await ctx.Ofertas.AsNoTracking().SingleAsync(o => o.IdOferta == despues.IdOferta, Ct);
            Assert.Equal((idNuevo, true), (nueva.IdPrecioReferencia, nueva.Publicado));
            await tx.RollbackAsync(Ct);
        }

        ctx.ChangeTracker.Clear();
        Assert.Equal(nPrecios, await ctx.Precios.CountAsync(Ct));
        Assert.Equal(nOfertas, await ctx.Ofertas.CountAsync(Ct));
        var revertida = Assert.Single(await repo.GetPorIdsAsync(tenant, [antes.IdPresProd], hoy, Ct));
        Assert.Equal((antes.IdPrecio, antes.IdOferta), (revertida.IdPrecio, revertida.IdOferta));
    }

    [Fact]
    public async Task PlanSinOperaciones_NoEscribeNada()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var repo = new PreciosGrillaRepositoryAsync(ctx);
        var (p1, _) = await repo.GetPaginaAsync(tenant, new PreciosGrillaFiltro(), PublicCatalogSpecs.Hoy(), 1, 1, Ct);
        var id = Assert.Single(p1).IdPresProd;
        var nPrecios = await ctx.Precios.CountAsync(Ct);
        SnapshotPrecios? visto = null;
        var (r, nuevos) = await repo.EjecutarAsync<int>([id], s => { visto = s[id]; return (null, 42); }, Ct);
        Assert.Equal(42, r);
        Assert.Empty(nuevos);
        Assert.NotNull(visto);
        Assert.Equal(tenant, visto!.IdTenant);
        Assert.Equal(nPrecios, await ctx.Precios.CountAsync(Ct));
    }
}
