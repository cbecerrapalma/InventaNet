using CBP.Data.Abstractions;
using CBP.Data.Asynchronous;
using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.Data.Tests;

// S77 R2' negativa: si el precio vigente de la presentación cambia (la referencia
// congelada deja de ser el vigente), la oferta deja de mostrarse en card y detalle.
// Escritura temporal DENTRO de transacción revertida + verificación sin residuo.
// El producto candidato tiene UNA SOLA presentación para que la card sea unívoca.
[Collection("OfficialLocal")]
public sealed class S77_OfertaNoVigenteTests : IDisposable
{
    private readonly OfficialLocalFixture _fx;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public S77_OfertaNoVigenteTests(OfficialLocalFixture fx)
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
    public async Task Referencia_No_Vigente_Oculta_Oferta_En_Card_Y_Detalle()
    {
        var ctx = _scope.ServiceProvider.GetRequiredService<InventaDbContext>();
        var uow = _scope.ServiceProvider.GetRequiredService<IUnitOfWorkAsync>();
        var repo = new StorefrontRepositoryAsync(ctx, uow);
        var hoy = PublicCatalogSpecs.Hoy();
        // S86: T4 (ABARROTES/PP2). T5 no tiene productos en la semilla S86.
        var tenant = ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == 2)
            .Select(t => t.IdTenant).Single();
        var nPreciosAntes = ctx.Precios.Count();

        // Oferta efectiva actual del tenant, en un producto de UNA SOLA presentación:
        // referencia == vigente real de su presentación.
        var cand = (from o in ctx.Ofertas.AsNoTracking()
                    join p in ctx.Precios.AsNoTracking() on o.IdPrecioReferencia equals p.IdPrecio
                    join pp in ctx.Presentaciones.AsNoTracking() on o.IdPresProd equals pp.IdPresProd
                    join v in ctx.Variantes.AsNoTracking() on pp.IdVarProd equals v.IdVarProd
                    join pr in ctx.Productos.AsNoTracking() on v.IdProd equals pr.IdProd
                    where pr.IdTenant == tenant && pr.Activo && pr.VisibleStore
                        && o.Activo && o.Publicado && o.FechaDesde <= hoy
                        && (o.FechaHasta == null || o.FechaHasta >= hoy)
                        && o.PrecioOferta < p.Valor && p.Activo && p.FecVig <= hoy
                        && !ctx.Precios.Any(p2 => p2.IdPresProd == o.IdPresProd && p2.Activo
                            && p2.FecVig <= hoy
                            && (p2.FecVig > p.FecVig || (p2.FecVig == p.FecVig && p2.IdPrecio > p.IdPrecio)))
                        && (from pp2 in ctx.Presentaciones.AsNoTracking()
                            join v2 in ctx.Variantes.AsNoTracking() on pp2.IdVarProd equals v2.IdVarProd
                            where v2.IdProd == pr.IdProd
                            select pp2.IdPresProd).Count() == 1
                    select new { o.IdPresProd, VigId = p.IdPrecio, RefValor = p.Valor, o.PrecioOferta, pr.Codigo, pr.IdProd })
            .First();
        Assert.True(cand.PrecioOferta < cand.RefValor);

        // Control positivo: antes del cambio la card MUESTRA la oferta.
        var (_, previos) = await repo.GetPublicCardsAsync(tenant, cand.Codigo, 1, 5, null, ct: Xunit.TestContext.Current.CancellationToken);
        var cardPrevia = Assert.Single(previos);
        Assert.True(cardPrevia.TieneOferta);

        await using var tx = await ctx.Database.BeginTransactionAsync(Xunit.TestContext.Current.CancellationToken);
        try
        {
            ctx.Precios.Add(new Domain.Entities.Precio
            {
                IdPresProd = cand.IdPresProd,
                Valor = cand.RefValor + 1000m,
                FecVig = hoy,
                Activo = true,
            });
            await ctx.SaveChangesAsync(Xunit.TestContext.Current.CancellationToken);

            var (total, items) = await repo.GetPublicCardsAsync(tenant, cand.Codigo, 1, 5, null, ct: Xunit.TestContext.Current.CancellationToken);
            var card = Assert.Single(items);
            Assert.False(card.TieneOferta);
            Assert.Equal(cand.RefValor + 1000m, card.PrecioFinalDesde);
            Assert.Null(card.PrecioReferencia);

            var bundle = await repo.GetPublicDetailAsync(tenant, cand.IdProd, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(bundle);
            // Exclusión R2' con la misma regla del servicio: el vigente se
            // resuelve desde el bundle (incluye el precio recién insertado).
            var esCand = PublicCatalogSpecs.PrecioCandidato(hoy).Compile();
            var esVig = PublicCatalogSpecs.OfertaVigente(hoy).Compile();
            var vigenteNuevo = bundle!.Precios
                .Where(p => p.IdPresProd == cand.IdPresProd && esCand(p))
                .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio)
                .FirstOrDefault();
            Assert.NotNull(vigenteNuevo);
            Assert.NotEqual(cand.VigId, vigenteNuevo!.IdPrecio);
            Assert.Equal(cand.RefValor + 1000m, vigenteNuevo.Valor);
            var efectivas = bundle.OfertasVigentes
                .Where(o => o.IdPresProd == cand.IdPresProd && esVig(o)
                    && vigenteNuevo != null
                    && PublicCatalogSpecs.OfertaEfectiva(o, vigenteNuevo.IdPrecio, o.PrecioReferencia?.Valor))
                .ToList();
            Assert.Empty(efectivas);
        }
        finally
        {
            await tx.RollbackAsync(Xunit.TestContext.Current.CancellationToken);
        }

        Assert.Equal(nPreciosAntes, ctx.Precios.Count());
    }
}