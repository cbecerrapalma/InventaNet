using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S91.4: dbo.sp_ImportarProductos v3 (TVP v3) contra SQL real, tenant 4, datos S88-* del fixture
/// (setup → op → cleanup; el fixture no toca lotes). Reglas: celda vacía conserva en existentes,
/// valores por defecto en nuevos, Activo S/N, código vacío en variante existente no genera otro.
/// Y la instantánea comercial (solo lectura) con la regla del SP (FecVig DESC, Id DESC) y aislada por tenant.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S914_ImportSpV3Tests(OfficialLocalFixture fx)
{
    private const string Ean = "4006381333931";

    private static ProductoImportTvpItem Fila(Action<ProductoImportTvpItem>? cambiar = null)
    {
        var it = new ProductoImportTvpItem
        {
            CodigoProducto = "S88-V3-01", NombreProducto = "S88 V3", Categoria = "S88-CAT", Marca = "S88-MAR",
            UnidadBase = "S88-UN", Pesable = "N", ManejaLote = "N", DiasVencimiento = 30, PublicarEnTienda = "S",
            CodigoVariante = "S88-V3-01-UN", NombreVariante = "Unica", CodigoBarras = Ean, TipoCod = 1,
            PrecioVenta = 1000m, Activo = null,
        };
        cambiar?.Invoke(it);
        return it;
    }

    [Fact]
    public async Task V3_VacioConserva_ActivoNDesactiva_YCodigoVacioNoGeneraOtro()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using (var ctx = f.NewContext())
            {
                var r1 = await f.NewSpRepo(ctx).AplicarAsync(S88ImportFixture.Tenant, 8, [Fila()], CancellationToken.None);
                Assert.True(r1.IsSuccess, r1.Error?.Message);
            }
            using (var ctx = f.NewContext())
            {
                var p = await ctx.Productos.AsNoTracking().SingleAsync(x => x.IdTenant == S88ImportFixture.Tenant && x.Codigo == "S88-V3-01", Xunit.TestContext.Current.CancellationToken);
                Assert.True(p.Activo);
                Assert.True(p.VisibleStore);
                Assert.Equal(30, p.DiasVen);
                Assert.NotNull(p.IdMarca);
            }

            // Reimportación: celdas opcionales vacías + Activo = N + sin código de barras.
            using (var ctx = f.NewContext())
            {
                var r2 = await f.NewSpRepo(ctx).AplicarAsync(S88ImportFixture.Tenant, 8, [Fila(i =>
                {
                    i.Marca = null; i.Pesable = null; i.ManejaLote = null; i.PublicarEnTienda = null; i.DiasVencimiento = null;
                    i.CodigoBarras = null; i.TipoCod = null; i.Activo = "N";
                })], CancellationToken.None);
                Assert.True(r2.IsSuccess, r2.Error?.Message);
                Assert.Equal(0, r2.Value.PreciosCreados);
                Assert.Equal(0, r2.Value.CodigosBarraCreados);
            }
            using (var ctx = f.NewContext())
            {
                var p = await ctx.Productos.AsNoTracking().SingleAsync(x => x.IdTenant == S88ImportFixture.Tenant && x.Codigo == "S88-V3-01", Xunit.TestContext.Current.CancellationToken);
                Assert.False(p.Activo);         // Activo = N
                Assert.True(p.VisibleStore);    // vacío conserva (antes: vacío => S)
                Assert.Equal(30, p.DiasVen);    // vacío conserva (antes: NULL)
                Assert.NotNull(p.IdMarca);      // vacío conserva (antes: NULL)
                var codigos = await ctx.CodigosBarra.AsNoTracking()
                    .Where(c => c.PresProd.VarProd.IdProd == p.IdProd).ToListAsync(Xunit.TestContext.Current.CancellationToken);
                var c = Assert.Single(codigos);  // no se generó un interno nuevo
                Assert.Equal(Ean, c.Codigo);
                Assert.True(c.Principal);
            }

            using (var ctx = f.NewContext())
            {
                var r3 = await f.NewSpRepo(ctx).AplicarAsync(S88ImportFixture.Tenant, 8, [Fila(i => i.Activo = "S")], CancellationToken.None);
                Assert.True(r3.IsSuccess, r3.Error?.Message);
            }
            using (var ctx = f.NewContext())
                Assert.True((await ctx.Productos.AsNoTracking().SingleAsync(x => x.IdTenant == S88ImportFixture.Tenant && x.Codigo == "S88-V3-01", Xunit.TestContext.Current.CancellationToken)).Activo);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task V3_ProductoNuevo_BanderasVacias_UsaValoresPorDefecto_YGeneraInterno()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using (var ctx = f.NewContext())
            {
                var r = await f.NewSpRepo(ctx).AplicarAsync(S88ImportFixture.Tenant, 8, [Fila(i =>
                {
                    i.CodigoProducto = "S88-V3-02"; i.CodigoVariante = "S88-V3-02-UN";
                    i.Pesable = null; i.ManejaLote = null; i.PublicarEnTienda = null; i.Activo = null;
                    i.CodigoBarras = null; i.TipoCod = null;
                })], CancellationToken.None);
                Assert.True(r.IsSuccess, r.Error?.Message);
            }
            using (var ctx = f.NewContext())
            {
                var p = await ctx.Productos.AsNoTracking().SingleAsync(x => x.IdTenant == S88ImportFixture.Tenant && x.Codigo == "S88-V3-02", Xunit.TestContext.Current.CancellationToken);
                Assert.False(p.EsPesable);
                Assert.False(p.ManejaLot);
                Assert.False(p.VisibleStore);   // S91.4: nuevo sin valor => N (antes S)
                Assert.True(p.Activo);
                var c = Assert.Single(await ctx.CodigosBarra.AsNoTracking().Where(x => x.PresProd.VarProd.IdProd == p.IdProd).ToListAsync(Xunit.TestContext.Current.CancellationToken));
                Assert.Equal((byte)3, c.TipoCod); // interno generado para variante nueva
            }
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task EstadoComercial_UltimaVigenciaComoElSp_YAisladoPorTenant()
    {
        var idProd = await fx.MasterIdProdAsync();
        using var ctx = fx.NewContext();
        var estado = await new ProductoImportSpRepositoryAsync(ctx).GetEstadoComercialAsync(OfficialLocalFixture.MasterIdTenant, CancellationToken.None);

        var presMaster = await ctx.Presentaciones.AsNoTracking().Where(p => p.VarProd.IdProd == idProd).Select(p => p.IdPresProd).ToListAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.NotEmpty(presMaster);
        foreach (var idPres in presMaster)
        {
            var esperado = await ctx.Precios.AsNoTracking().Where(x => x.IdPresProd == idPres)
                .OrderByDescending(x => x.FecVig).ThenByDescending(x => x.IdPrecio).Select(x => (decimal?)x.Valor).FirstOrDefaultAsync(Xunit.TestContext.Current.CancellationToken);
            if (esperado is null) Assert.False(estado.UltimoPrecio.ContainsKey(idPres));
            else Assert.Equal(esperado, estado.UltimoPrecio[idPres]);
        }

        var presOtroTenant = await ctx.Presentaciones.AsNoTracking()
            .Where(p => p.VarProd.Prod.IdTenant == OfficialLocalFixture.OtroTenant).Select(p => p.IdPresProd).Take(200).ToListAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.DoesNotContain(estado.Presentaciones.Values, v => presOtroTenant.Contains(v));
        Assert.DoesNotContain(estado.UltimoPrecio.Keys, k => presOtroTenant.Contains(k));
    }
}

