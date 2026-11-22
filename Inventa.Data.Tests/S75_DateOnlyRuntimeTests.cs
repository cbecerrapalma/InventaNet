using CBP.Data.Asynchronous;
using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.Data.Tests;

// S75 §7: DateOnly en runtime real — inserta una Oferta con FechaHasta vía EF,
// lee el catálogo por el SP (CBP QuerySPWithOutputAsync → DbValueConverter) y
// verifica OfertaHasta DateOnly? de extremo a extremo. Limpieza en finally.
// Solo lectura + 1 INSERT + 1 DELETE controlados (OfficialLocal).
[Collection("OfficialLocal")]
public sealed class S75DateOnlyRuntimeTests : IDisposable
{
    private readonly OfficialLocalFixture _fx;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public S75DateOnlyRuntimeTests(OfficialLocalFixture fx)
    {
        _fx = fx;
        var services = new ServiceCollection();
        services.AddDbContext<InventaDbContext>(o => o.UseSqlServer(OfficialLocalFixture.ConnectionString));
        services.AddUnitOfWorkAsync<InventaDbContext>();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    [Fact]
    public async Task Oferta_Con_FechaHasta_Llega_DateOnly_Al_ReadModel()
    {
        var ctx = _scope.ServiceProvider.GetRequiredService<InventaDbContext>();
        var uow = _scope.ServiceProvider.GetRequiredService<CBP.Data.Abstractions.IUnitOfWorkAsync>();
        var repo = new StorefrontRepositoryAsync(ctx, uow);

        // S86: T4 (ABARROTES/PP2). T5 no tiene productos en la semilla S86.
        var tenant = await ctx.Tenants.AsNoTracking()
            .Where(t => t.IdTenantPassPlat == 2).Select(t => t.IdTenant).SingleAsync(Xunit.TestContext.Current.CancellationToken);
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var hasta = today.AddDays(30);

        // Presentación vendible con precio vigente. S86: la oferta insertada
        // (50% del vigente, prioridad 1) es la mas barata del producto, asi
        // que el SP la agrega aunque haya ofertas hermanas (toda presentacion
        // S86 trae 2 efectivas).
        var pres = await (
            from pp in ctx.Presentaciones.AsNoTracking()
            join v in ctx.Variantes.AsNoTracking() on pp.IdVarProd equals v.IdVarProd
            join p in ctx.Productos.AsNoTracking() on v.IdProd equals p.IdProd
            where pp.Activo && pp.EsVenta && v.Activo
                && p.IdTenant == tenant && p.Activo && p.VisibleStore
                && ctx.Precios.Any(pc => pc.IdPresProd == pp.IdPresProd && pc.Activo && pc.FecVig <= today)
            orderby pp.IdPresProd
            select new { pp.IdPresProd, p.IdProd })
            .FirstAsync(Xunit.TestContext.Current.CancellationToken);
        // Mismo desempate que el SP (FecVig DESC, IdPrecio DESC) para garantizar
        // PrecioOferta < vig.Valor (condición de oferta efectiva del SP).
        var vigente = await ctx.Precios.AsNoTracking()
            .Where(pc => pc.IdPresProd == pres.IdPresProd && pc.Activo && pc.FecVig <= today)
            .OrderByDescending(pc => pc.FecVig).ThenByDescending(pc => pc.IdPrecio).FirstAsync(Xunit.TestContext.Current.CancellationToken);

        var oferta = new Oferta
        {
            IdPresProd = pres.IdPresProd,
            IdPrecioReferencia = vigente.IdPrecio,
            FechaDesde = today.AddDays(-1),
            FechaHasta = hasta,
            PrecioOferta = vigente.Valor * 0.5m,
            Prioridad = 1,
            Activo = true,
            Publicado = true,
            FecCrea = DateTime.UtcNow,
        };
        ctx.Ofertas.Add(oferta);
        await ctx.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

        try
        {
            PublicCardItem? card = null;
            var page = 1;
            while (card == null)
            {
                var (_, items) = await repo.GetPublicCardsAsync(
                    tenant, null, page, 100, null, soloOfertas: true, ct: Xunit.TestContext.Current.CancellationToken);
                card = items.FirstOrDefault(c => c.IdProd == pres.IdProd);
                if (items.Count < 100) break;
                page++;
                Assert.True(page <= 30, "La card con oferta no apareció en 30 páginas.");
            }
            Assert.NotNull(card);
            Assert.True(card.TieneOferta);
            Assert.Equal(hasta, card.OfertaHasta);
        }
        finally
        {
            ctx.Ofertas.Remove(oferta);
            await ctx.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);
        }
    }
}
