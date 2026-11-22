using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S92: lectura de la administración de productos contra SQL real (solo lectura; semilla ABA-T0001, tenants 4/5)
/// y SP v4 + generación de códigos internos con S88ImportFixture (datos S88-*, sin tocar lotes).
/// </summary>
[Collection("OfficialLocal")]
public sealed class S92_ProductoFichaDataTests(OfficialLocalFixture fx)
{
    private const int T = OfficialLocalFixture.MasterIdTenant;

    [Fact]
    public async Task EsPublicable_Equivale_VisibleStore_Y_ListoParaPublicar()
    {
        using var ctx = fx.NewContext();
        foreach (var tenant in new[] { T, OfficialLocalFixture.OtroTenant })
        {
            var pub = await ctx.Productos.Where(p => p.IdTenant == tenant).Where(PublicCatalogSpecs.EsPublicable)
                .Select(p => p.IdProd).OrderBy(x => x).ToListAsync(Xunit.TestContext.Current.CancellationToken);
            var listoYVisible = await ctx.Productos.Where(p => p.IdTenant == tenant && p.VisibleStore)
                .Where(PublicCatalogSpecs.ListoParaPublicar).Select(p => p.IdProd).OrderBy(x => x).ToListAsync(Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(pub, listoYVisible);
        }
    }

    [Fact]
    public async Task Diagnostico_CoincideConConsultasIndependientes()
    {
        using var ctx = fx.NewContext();
        var hoy = PublicCatalogSpecs.Hoy();
        var diag = await new ProductoFichaRepositoryAsync(ctx).GetDiagnosticosAsync(T, Xunit.TestContext.Current.CancellationToken);
        var listos = (await ctx.Productos.Where(p => p.IdTenant == T).Where(PublicCatalogSpecs.ListoParaPublicar)
            .Select(p => p.IdProd).ToListAsync(Xunit.TestContext.Current.CancellationToken)).ToHashSet();
        var conImagen = (await ctx.ProductoImagenes.Where(i => i.Activo && i.Prod.IdTenant == T).Select(i => i.IdProd).Distinct().ToListAsync(Xunit.TestContext.Current.CancellationToken)).ToHashSet();
        var conPrecio = (await ctx.Precios.Where(PublicCatalogSpecs.PrecioCandidato(hoy))
            .Where(pc => pc.PresProd.Activo && pc.PresProd.EsVenta && pc.PresProd.VarProd.Activo && pc.PresProd.VarProd.Prod.IdTenant == T)
            .Select(pc => pc.PresProd.VarProd.IdProd).Distinct().ToListAsync(Xunit.TestContext.Current.CancellationToken)).ToHashSet();
        Assert.Equal(await ctx.Productos.CountAsync(p => p.IdTenant == T, Xunit.TestContext.Current.CancellationToken), diag.Count);
        foreach (var d in diag)
        {
            Assert.Equal(listos.Contains(d.IdProd), d.Activo && d.Vendible);
            Assert.Equal(conImagen.Contains(d.IdProd), d.TieneImagen);
            Assert.Equal(conPrecio.Contains(d.IdProd), d.TienePrecio);
        }
    }

    [Fact]
    public async Task Listado_FiltrosEnServidor_CuadranConConteos()
    {
        using var ctx = fx.NewContext();
        var repo = new ProductoFichaRepositoryAsync(ctx);
        var diag = await repo.GetDiagnosticosAsync(T, Xunit.TestContext.Current.CancellationToken);
        var (pagina1, total) = await repo.SearchListadoAsync(T, new ProductoListadoFiltro(), 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(diag.Count, total);
        Assert.Equal(Math.Min(20, total), pagina1.Count);
        var (_, sinImagen) = await repo.SearchListadoAsync(T, new ProductoListadoFiltro(ConImagen: false), 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(diag.Count(d => !d.TieneImagen), sinImagen);
        var (_, conPrecio) = await repo.SearchListadoAsync(T, new ProductoListadoFiltro(ConPrecio: true), 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(diag.Count(d => d.TienePrecio), conPrecio);
        foreach (var estado in new[] { EstadosTienda.Publicado, EstadosTienda.PublicadoConAdvertencia, EstadosTienda.Listo, EstadosTienda.Pendiente, EstadosTienda.Inactivo })
        {
            var (_, n) = await repo.SearchListadoAsync(T, new ProductoListadoFiltro(Estado: estado), 1, 20, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(diag.Count(d => PublicCatalogSpecs.EstadoTienda(d.Activo, d.VisibleStore, d.Vendible, d.TienePrecio) == estado), n);
        }
        var (texto, nTexto) = await repo.SearchListadoAsync(T, new ProductoListadoFiltro(Texto: OfficialLocalFixture.MasterCodProd), 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.True(nTexto >= 1);
        Assert.Contains(texto, r => r.Codigo == OfficialLocalFixture.MasterCodProd);
    }

    [Fact]
    public async Task Ficha_PrecioVigenteR1_Stock_YAisladaPorTenant()
    {
        var idProd = await fx.MasterIdProdAsync();
        using var ctx = fx.NewContext();
        var repo = new ProductoFichaRepositoryAsync(ctx);
        var b = await repo.GetFichaAsync(T, idProd, Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(b);
        Assert.Null(await repo.GetFichaAsync(OfficialLocalFixture.OtroTenant, idProd, Xunit.TestContext.Current.CancellationToken));
        var hoy = PublicCatalogSpecs.Hoy();
        Assert.Equal(await ctx.Variantes.CountAsync(v => v.IdProd == idProd, Xunit.TestContext.Current.CancellationToken), b!.Variantes.Count);
        foreach (var pres in b.Presentaciones)
        {
            var idPres = pres.IdPresProd;
            var esperado = await ctx.Precios.Where(PublicCatalogSpecs.PrecioCandidato(hoy)).Where(pc => pc.IdPresProd == idPres)
                .OrderByDescending(pc => pc.FecVig).ThenByDescending(pc => pc.IdPrecio).Select(pc => (long?)pc.IdPrecio).FirstOrDefaultAsync(Xunit.TestContext.Current.CancellationToken);
            var vigenteBundle = b.PreciosCandidatos.Where(pc => pc.IdPresProd == idPres)
                .OrderByDescending(pc => pc.FecVig).ThenByDescending(pc => pc.IdPrecio).Select(pc => (long?)pc.IdPrecio).FirstOrDefault();
            Assert.Equal(esperado, vigenteBundle);
            var stock = await ctx.StockDisponible.Where(s => s.IdTenant == T && s.IdPresProd == idPres).SumAsync(s => (decimal?)s.CantDisp, Xunit.TestContext.Current.CancellationToken) ?? 0m;
            Assert.Equal(stock, b.StockPorPresentacion.TryGetValue(idPres, out var st) ? st : 0m);
            Assert.Equal(await ctx.CodigosBarra.CountAsync(c => c.IdPresProd == idPres, Xunit.TestContext.Current.CancellationToken), b.Codigos.Count(c => c.IdPresProd == idPres));
        }
    }

    [Fact]
    public async Task SpV4_DescripRecortada_Y_InternoTipo3ConSecuenciaDeLaBd()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            var prefijo = "2" + S88ImportFixture.Tenant.ToString("D3");
            long maxAntes;
            using (var ctx = f.NewContext())
            {
                var internos = await ctx.CodigosBarra.Where(c => c.TipoCod == 3 && c.Codigo.StartsWith(prefijo)).Select(c => c.Codigo).ToListAsync(Xunit.TestContext.Current.CancellationToken);
                maxAntes = internos.Select(c => long.Parse(c.Substring(4, 8))).DefaultIfEmpty(0).Max();
            }
            var item = new ProductoImportTvpItem
            {
                CodigoProducto = "S88-V4-01", NombreProducto = new string('N', 150), Categoria = "S88-CAT", Marca = "S88-MAR",
                UnidadBase = "S88-UN", CodigoVariante = "S88-V4-01-UN", NombreVariante = new string('V', 100),
                CodigoBarras = null, TipoCod = null, PrecioVenta = 1000m,
            };
            using (var ctx = f.NewContext())
            {
                var r = await f.NewSpRepo(ctx).AplicarAsync(S88ImportFixture.Tenant, 8, [item], CancellationToken.None);
                Assert.True(r.IsSuccess, r.Error?.Message);
            }
            using (var ctx = f.NewContext())
            {
                var pres = await ctx.Presentaciones.AsNoTracking().SingleAsync(p => p.VarProd.Codigo == "S88-V4-01-UN" && p.VarProd.Prod.IdTenant == S88ImportFixture.Tenant, Xunit.TestContext.Current.CancellationToken);
                Assert.Equal(100, pres.Descrip.Length);
                var cod = await ctx.CodigosBarra.AsNoTracking().SingleAsync(c => c.IdPresProd == pres.IdPresProd, Xunit.TestContext.Current.CancellationToken);
                Assert.Equal((byte)3, cod.TipoCod);
                Assert.True(cod.Principal);
                Assert.Equal(prefijo, cod.Codigo[..4]);
                Assert.Equal(maxAntes + 1, long.Parse(cod.Codigo.Substring(4, 8)));
            }
        }
        finally
        {
            await f.DisposeAsync();
        }
    }
}

