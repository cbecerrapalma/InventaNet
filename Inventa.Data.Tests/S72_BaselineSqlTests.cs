using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Data.Tests;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Inventa.Data.Tests;

[Collection("OfficialLocal")]
public sealed class S72BaselineSqlTests
{
    private readonly OfficialLocalFixture _fx;
    public S72BaselineSqlTests(OfficialLocalFixture fx) => _fx = fx;

    // S74: reducida a oráculo documentado. La consulta original (abajo, intacta como
    // referencia) NO es traducible por EF Core 10 — ToQueryString lanza
    // InvalidOperationException ("could not be translated") — por lo que no puede
    // ejecutarse. El oráculo ejecutable es CatalogCardsOracle (S74, mismas reglas
    // R1–R5 compiladas en memoria); la equivalencia viva está en
    // S74_CatalogCardsEquivalenceTests.
    [Fact(Skip = "S74: oráculo documentado; ver CatalogCardsOracle + S74_CatalogCardsEquivalenceTests.")]
    public void CaptureBaselineSql()
    {
        using var loggerFactory = LoggerFactory.Create(b => b
            .AddConsole()
            .SetMinimumLevel(LogLevel.Information)
            .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information));

        var options = new DbContextOptionsBuilder<InventaDbContext>()
            .UseSqlServer(OfficialLocalFixture.ConnectionString)
            .UseLoggerFactory(loggerFactory)
            .EnableSensitiveDataLogging()
            .Options;

        using var ctx = new InventaDbContext(options);

        var hoy = PublicCatalogSpecs.Hoy();
        var pub = PublicCatalogSpecs.EsPublicable;
        var pc = PublicCatalogSpecs.PrecioCandidato(hoy);
        var of = PublicCatalogSpecs.OfertaVigente(hoy);

        var cards = ctx.Productos.AsNoTracking().Where(pr => pr.IdTenant == 5).Where(pub);
        var sel = cards.Select(pr => new
        {
            pr.IdProd, pr.Codigo, pr.Nombre, pr.IdCategoria,
            CatNombre = pr.Categoria.Nombre,
            pr.IdMarca,
            MarcaNombre = pr.Marca != null ? pr.Marca.Nombre : null,
            UnidadNombre = pr.UnidadBase.Nombre,
            ImagenUrl = ctx.ProductoImagenes.AsNoTracking()
                .Where(pi => pi.IdProd == pr.IdProd && pi.Activo)
                .OrderByDescending(pi => pi.EsPrincipal).ThenBy(pi => pi.Orden)
                .Select(pi => pi.Url).FirstOrDefault(),
            LegacyMin = ctx.Precios
                .Where(p => ctx.Presentaciones.Any(pp => pp.IdPresProd == p.IdPresProd
                    && pp.VarProd.IdProd == pr.IdProd && pp.VarProd.Activo && pp.Activo && pp.EsVenta))
                .Where(pc).Select(p => (decimal?)p.Valor).OrderBy(v => v).FirstOrDefault(),
            Pres = ctx.Presentaciones
                .Where(pp => pp.VarProd.IdProd == pr.IdProd && pp.VarProd.Activo && pp.Activo && pp.EsVenta)
                .Select(pp => new
                {
                    Vig = ctx.Precios.Where(p => p.IdPresProd == pp.IdPresProd).Where(pc)
                        .OrderByDescending(PublicCatalogSpecs.PrecioOrdenFecVig)
                        .ThenByDescending(PublicCatalogSpecs.PrecioOrdenId)
                        .Select(x => (decimal?)x.Valor).FirstOrDefault(),
                    Of = ctx.Ofertas.Where(o => o.IdPresProd == pp.IdPresProd).Where(of)
                        .Where(o => o.PrecioOferta < ctx.Precios.Where(p => p.IdPresProd == pp.IdPresProd).Where(pc)
                            .OrderByDescending(PublicCatalogSpecs.PrecioOrdenFecVig)
                            .ThenByDescending(PublicCatalogSpecs.PrecioOrdenId)
                            .Select(x => (decimal?)x.Valor).FirstOrDefault())
                        .OrderBy(PublicCatalogSpecs.OfertaOrdenPrecio)
                        .ThenByDescending(PublicCatalogSpecs.OfertaOrdenId)
                        .Select(o => new { o.PrecioOferta, o.FechaHasta }).FirstOrDefault()
                })
                .Where(x => x.Vig != null)
                .Select(x => new { Final = x.Of != null ? x.Of.PrecioOferta : x.Vig!.Value, Vig = x.Vig!.Value, x.Of })
                .OrderBy(x => x.Final).FirstOrDefault(),
            TieneOferta = ctx.Ofertas.Where(of)
                .Where(o => ctx.Presentaciones.Any(pp => pp.IdPresProd == o.IdPresProd
                    && pp.VarProd.IdProd == pr.IdProd && pp.VarProd.Activo && pp.Activo && pp.EsVenta))
                .Any(o => o.PrecioOferta < ctx.Precios.Where(p => p.IdPresProd == o.IdPresProd).Where(pc)
                    .OrderByDescending(PublicCatalogSpecs.PrecioOrdenFecVig)
                    .ThenByDescending(PublicCatalogSpecs.PrecioOrdenId)
                    .Select(x => (decimal?)x.Valor).FirstOrDefault())
        });

        Console.WriteLine("=== DEFAULT ===");
        Console.WriteLine(sel.OrderBy(x => x.Codigo).ThenBy(x => x.IdProd).ToQueryString());

        Console.WriteLine("=== SOLOOFERTAS ===");
        Console.WriteLine(sel.Where(x => x.TieneOferta).ToQueryString());

        Console.WriteLine("=== DESCUENTO_DESC ===");
        Console.WriteLine(sel.OrderBy(x => x.Pres == null || x.Pres.Of == null)
            .ThenBy(x => x.Pres!.Of!.PrecioOferta / x.Pres!.Vig)
            .ThenBy(x => x.Nombre).ThenBy(x => x.IdProd).ToQueryString());
    }
}
