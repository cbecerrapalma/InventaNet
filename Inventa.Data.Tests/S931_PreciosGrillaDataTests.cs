using System.Diagnostics;
using CBP.Data.Abstractions;
using CBP.Data.Asynchronous;
using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S93.1 C2: grilla «Precios y ofertas» contra SQL real (solo lectura).
/// - Equivalencia con la tienda: por producto publicable, el mejor precio de la grilla
///   (misma regla que sp_Store_CatalogoCards v4) coincide con la card pública.
/// - Filtros en SQL antes de paginar: cada filtro cuadra con el conteo independiente.
/// - Rendimiento (VESTUARIO): se informa el p95 de 5 mediciones por consulta.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S931_PreciosGrillaDataTests : IDisposable
{
    private readonly OfficialLocalFixture _fx;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    public S931_PreciosGrillaDataTests(OfficialLocalFixture fx)
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

    private static int TenantPorPassPlat(InventaDbContext ctx, int tid) =>
        ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == tid).Select(t => t.IdTenant).Single();

    [Theory]
    [InlineData(2)] // ABARROTES (local 4)
    [InlineData(3)] // VESTUARIO (local 5): catálogo completo
    public async Task Grilla_EquivaleALaTienda_PorProductoPublicable(int tidPassPlat)
    {
        var ctx = _scope.ServiceProvider.GetRequiredService<InventaDbContext>();
        var uow = _scope.ServiceProvider.GetRequiredService<IUnitOfWorkAsync>();
        var tenant = TenantPorPassPlat(ctx, tidPassPlat);
        var store = new StorefrontRepositoryAsync(ctx, uow);
        var grilla = new PreciosGrillaRepositoryAsync(ctx);
        var hoy = PublicCatalogSpecs.Hoy();

        var cards = new List<PublicCardItem>();
        for (var page = 1; ; page++)
        {
            var (total, items) = await store.GetPublicCardsAsync(tenant, null, page, 100, null, ct: Ct);
            cards.AddRange(items);
            if (cards.Count >= total || items.Count == 0) break;
        }
        Assert.NotEmpty(cards);

        var idsPres = await ctx.Presentaciones.AsNoTracking()
            .Where(pp => pp.VarProd.Prod.IdTenant == tenant).Select(pp => pp.IdPresProd).ToListAsync(Ct);
        var filas = (await grilla.GetPorIdsAsync(tenant, idsPres, hoy, Ct)).Where(f => f.Publicable).ToLookup(f => f.IdProd);

        var comparados = 0;
        foreach (var card in cards)
        {
            var conPrecio = filas[card.IdProd].Where(f => f.PrecioValor != null).ToList();
            var best = conPrecio.OrderBy(f => f.OfertaPrecio ?? f.PrecioValor).ThenBy(f => f.IdPresProd).FirstOrDefault();
            Assert.Equal(card.PrecioFinalDesde, best is null ? null : best.OfertaPrecio ?? best.PrecioValor);
            Assert.Equal(card.TieneOferta, conPrecio.Any(f => f.IdOferta != null));
            Assert.Equal(card.PrecioReferencia, best?.IdOferta != null ? best.PrecioValor : null);
            Assert.Equal(card.OfertaHasta, best?.IdOferta != null ? best.OfertaHasta : null);
            Assert.Equal(card.PctDescuento, best?.IdOferta != null ? PublicCatalogSpecs.PctDescuento(best.PrecioValor!.Value, best.OfertaPrecio) : null);
            comparados++;
        }
        Assert.Equal(cards.Count, comparados);
    }

    [Fact]
    public async Task Filtros_SeAplicanAntesDePaginar_YCuadranConElResumen()
    {
        using var ctx = _fx.NewContext();
        var tenant = TenantPorPassPlat(ctx, 2);
        var repo = new PreciosGrillaRepositoryAsync(ctx);
        var hoy = PublicCatalogSpecs.Hoy();
        var resumen = await repo.GetResumenAsync(tenant, hoy, Ct);

        async Task<(int Total, List<PrecioGrillaRow> Todas)> Todas(FiltroEstadoPrecio e)
        {
            var (p1, total) = await repo.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Estado: e), hoy, 1, 5, Ct);
            var todas = new List<PrecioGrillaRow>(p1);
            for (var page = 2; todas.Count < total; page++)
                todas.AddRange((await repo.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Estado: e), hoy, page, 5, Ct)).Items);
            // La primera página está llena si hay datos suficientes (los filtros no recortan páginas).
            Assert.Equal(Math.Min(5, total), p1.Count);
            Assert.Equal(total, todas.Select(r => r.IdPresProd).Distinct().Count());
            return (total, todas);
        }

        var (vendibles, filas) = await Todas(FiltroEstadoPrecio.Todos);
        Assert.Equal(resumen.Vendibles, vendibles);
        Assert.All(filas, f => Assert.True(f.Vendible));

        var (sinPrecio, sp) = await Todas(FiltroEstadoPrecio.SinPrecio);
        Assert.Equal(resumen.SinPrecio, sinPrecio);
        Assert.All(sp, f => Assert.Null(f.IdPrecio));
        Assert.Equal(filas.Count(f => f.IdPrecio == null), sinPrecio);

        var (conOferta, co) = await Todas(FiltroEstadoPrecio.ConOferta);
        Assert.Equal(resumen.ConOferta, conOferta);
        Assert.All(co, f => Assert.True(f.IdOferta != null && f.OfertaPrecio < f.PrecioValor));

        var (sinOferta, _) = await Todas(FiltroEstadoPrecio.SinOferta);
        Assert.Equal(vendibles, conOferta + sinOferta);

        var (sinEfecto, se) = await Todas(FiltroEstadoPrecio.OfertaSinEfecto);
        Assert.Equal(resumen.OfertasSinEfecto, sinEfecto);
        Assert.All(se, f => Assert.True(f.OfertasActivasHoy > (f.IdOferta != null ? 1 : 0)));

        var (programados, _) = await Todas(FiltroEstadoPrecio.Programado);
        Assert.Equal(resumen.Programados, programados);
    }

    [Fact]
    public async Task OfertaEfectiva_DeLaGrilla_ApuntaAlPrecioVigente_YEsLaMasBaja()
    {
        using var ctx = _fx.NewContext();
        var tenant = TenantPorPassPlat(ctx, 2);
        var hoy = PublicCatalogSpecs.Hoy();
        var repo = new PreciosGrillaRepositoryAsync(ctx);
        var ids = await ctx.Presentaciones.AsNoTracking().Where(pp => pp.VarProd.Prod.IdTenant == tenant)
            .Select(pp => pp.IdPresProd).ToListAsync(Ct);
        var filas = await repo.GetPorIdsAsync(tenant, ids, hoy, Ct);
        var snaps = await repo.GetSnapshotsAsync(ids, Ct);
        foreach (var f in filas)
        {
            var s = snaps[f.IdPresProd];
            var vig = PublicCatalogSpecs.PrecioVigente(s.Precios, hoy);
            var ef = PublicCatalogSpecs.SeleccionarOfertaEfectiva(s.OfertasActivas, vig, hoy);
            Assert.Equal(vig?.IdPrecio, f.IdPrecio);
            Assert.Equal(ef?.IdOferta, f.IdOferta);
        }
    }

    [Fact]
    public async Task Busqueda_PorCodigoDeBarras_DevuelveSoloEsaPresentacion()
    {
        using var ctx = _fx.NewContext();
        var tenant = TenantPorPassPlat(ctx, 2);
        var cb = await ctx.CodigosBarra.AsNoTracking()
            .Where(c => c.PresProd.VarProd.Prod.IdTenant == tenant && c.Principal && c.PresProd.Activo && c.PresProd.EsVenta && c.PresProd.VarProd.Activo)
            .OrderBy(c => c.IdCodBar).FirstAsync(Ct);
        var (items, total) = await new PreciosGrillaRepositoryAsync(ctx)
            .GetPaginaAsync(tenant, new PreciosGrillaFiltro(Texto: cb.Codigo), PublicCatalogSpecs.Hoy(), 1, 20, Ct);
        Assert.Equal(1, total);
        Assert.Equal(cb.IdPresProd, items[0].IdPresProd);
    }

    [Fact]
    public async Task Rendimiento_Vestuario_InformaP95()
    {
        var tenant = TenantPorPassPlat(_fx.NewContext(), 3);
        var hoy = PublicCatalogSpecs.Hoy();
        var casos = new (string Nombre, Func<PreciosGrillaRepositoryAsync, Task> Run)[]
        {
            ("pagina1_sin_filtro", r => r.GetPaginaAsync(tenant, new PreciosGrillaFiltro(), hoy, 1, 50, Ct)),
            ("con_oferta", r => r.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Estado: FiltroEstadoPrecio.ConOferta), hoy, 1, 50, Ct)),
            ("oferta_sin_efecto", r => r.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Estado: FiltroEstadoPrecio.OfertaSinEfecto), hoy, 1, 50, Ct)),
            ("texto_chaqueta", r => r.GetPaginaAsync(tenant, new PreciosGrillaFiltro(Texto: "chaqueta"), hoy, 1, 50, Ct)),
            ("resumen", r => r.GetResumenAsync(tenant, hoy, Ct)),
        };
        var lineas = new List<string>();
        foreach (var (nombre, run) in casos)
        {
            var tiempos = new List<double>();
            for (var i = 0; i < 6; i++)
            {
                using var ctx = _fx.NewContext();
                var sw = Stopwatch.StartNew();
                await run(new PreciosGrillaRepositoryAsync(ctx));
                sw.Stop();
                if (i > 0) tiempos.Add(sw.Elapsed.TotalMilliseconds); // la primera calienta caché de planes
            }
            tiempos.Sort();
            var p95 = tiempos[^1]; // con 5 muestras el p95 es el máximo
            lineas.Add($"{nombre}: p95={p95:F0} ms; muestras=[{string.Join(", ", tiempos.Select(t => t.ToString("F0")))}]");
            Assert.True(p95 < 5000, $"{nombre} demasiado lento: {p95:F0} ms");
        }
        TestContext.Current.TestOutputHelper?.WriteLine(string.Join(Environment.NewLine, lineas));
        var dir = Path.Combine(AppContext.BaseDirectory, "S931");
        Directory.CreateDirectory(dir);
        await File.WriteAllLinesAsync(Path.Combine(dir, "C2-rendimiento-vestuario.txt"), lineas, Ct);
    }
}
