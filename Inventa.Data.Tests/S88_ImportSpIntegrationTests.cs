using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S88.2: tests de integración SP vía repositorio de Inventa.Data.Async,
/// armando el TVP en la prueba (sin referencias a Application/Excel).
/// Contra SQL real (tenant 4 = ABARROTES).
/// Cada test: setup fixture → try/op+asserts → finally cleanup → residuo 0.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S88_ImportSpIntegrationTests(OfficialLocalFixture fx)
{
    private ProductoImportTvpItem Fila(
        string cp, string cv, string? codBar = null, byte? tipoCod = 1,
        int stock = 0, string? bod = null, decimal precio = 9990m) =>
        new()
        {
            CodigoProducto = cp,
            NombreProducto = "S88 Producto " + cp,
            CategoriaPadre = null,
            Categoria = "S88-CAT",
            Marca = "S88-MAR",
            UnidadBase = "S88-UN",
            Pesable = "N",
            ManejaLote = "N",
            PublicarEnTienda = "S",
            CodigoVariante = cv,
            NombreVariante = "Variante " + cv,
            CodigoBarras = codBar,
            TipoCod = codBar is null ? null : tipoCod,
            CodigosAdicionales = null,
            PrecioVenta = precio,
            PrecioOferta = null,
            OfertaDesde = null,
            OfertaHasta = null,
            PrecioMayorista = null,
            CostoCompra = 5000m,
            Proveedor = null, // S93.1 D6: el importador ya no envía proveedor (sin PrecioCompra)
            StockInicial = stock,
            Bodega = bod
        };

    [Fact]
    public async Task SpRepo_Aplicar_NuevoProducto_CreaCatalogoYPrecios()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-01", "S88-PROD-01-UN", codBar: "5901234123457")
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess, r.Error?.Message);
            Assert.Equal(1, r.Value.ProductosNuevos);
            Assert.Equal(0, r.Value.ProductosExistentes);
            Assert.Equal(1, r.Value.VariantesNuevas);
            Assert.Equal(1, r.Value.PresentacionesNuevas);
            Assert.Equal(1, r.Value.PreciosCreados);
            Assert.Equal(1, r.Value.CodigosBarraCreados);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_ProductoExistente_ActualizaYCreaVariante()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            // Primera aplicación: crea producto
            var filas1 = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-02", "S88-PROD-02-UN", codBar: "5901234123458")
            };
            var r1 = await repo.AplicarAsync(S88ImportFixture.Tenant, f.IdUsuario, filas1, CancellationToken.None);
            Assert.True(r1.IsSuccess);
            Assert.Equal(1, r1.Value.ProductosNuevos);

            // Segunda aplicación: mismo producto, nueva variante
            var filas2 = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-02", "S88-PROD-02-KG", codBar: "5901234123459", precio: 19990m)
            };
            var r2 = await repo.AplicarAsync(S88ImportFixture.Tenant, f.IdUsuario, filas2, CancellationToken.None);
            Assert.True(r2.IsSuccess);
            Assert.Equal(0, r2.Value.ProductosNuevos);
            Assert.Equal(1, r2.Value.ProductosExistentes);
            Assert.Equal(1, r2.Value.VariantesNuevas);
            Assert.Equal(1, r2.Value.PresentacionesNuevas);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_SinCodigoBarras_GeneraInterno()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-03", "S88-PROD-03-UN", codBar: null) // vacío = se genera interno
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess);
            Assert.Equal(1, r.Value.CodigosBarraCreados);
            Assert.Contains("CodigosBarrasGenerados", r.Value.ResumenJson);
            // El código generado es EAN-13 interno (prefijo 2) y TipoCod=3
            using var ctx2 = f.NewContext();
            var presIds = await ctx2.Presentaciones
                .Where(pp => pp.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && pp.VarProd.Prod.Codigo == "S88-PROD-03")
                .Select(pp => pp.IdPresProd)
                .ToListAsync(Xunit.TestContext.Current.CancellationToken);
            var generado = await ctx2.CodigosBarra.FirstAsync(c => presIds.Contains(c.IdPresProd), Xunit.TestContext.Current.CancellationToken);
            Assert.StartsWith("2", generado.Codigo);
            Assert.Equal(3, generado.TipoCod);
            Assert.True(generado.Principal);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_VariasFilasMismoProducto_AgrupaVariantes()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-04", "S88-PROD-04-S", codBar: "5901234123460", precio: 5000m),
                Fila("S88-PROD-04", "S88-PROD-04-M", codBar: "5901234123461", precio: 6000m),
                Fila("S88-PROD-04", "S88-PROD-04-L", codBar: "5901234123462", precio: 7000m)
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess);
            Assert.Equal(1, r.Value.ProductosNuevos);
            Assert.Equal(3, r.Value.VariantesNuevas);
            Assert.Equal(3, r.Value.PresentacionesNuevas);
            Assert.Equal(3, r.Value.PreciosCreados);
            Assert.Equal(3, r.Value.CodigosBarraCreados);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_CodBarDuplicadoEnArchivo_Falla()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-05", "S88-PROD-05-A", codBar: "5901234123463"),
                Fila("S88-PROD-05", "S88-PROD-05-B", codBar: "5901234123463") // duplicado en archivo
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsFailure);
            Assert.Contains("CODBAR_DUPLICADO", r.Error?.Code ?? "");
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_CodBarExisteEnBBDD_Falla()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            // Crear un codbar existente en BBDD (producto maestro)
            var existingProd = await ctx.Productos.FirstOrDefaultAsync(p => p.IdTenant == S88ImportFixture.Tenant && p.Codigo == "ABA-T0001", Xunit.TestContext.Current.CancellationToken);
            if (existingProd is null)
                throw new InvalidOperationException("Producto maestro ABA-T0001 no existe en T4");

            var existingVar = await ctx.Variantes.FirstOrDefaultAsync(v => v.IdProd == existingProd.IdProd, Xunit.TestContext.Current.CancellationToken);
            var existingPres = await ctx.Presentaciones.FirstOrDefaultAsync(p => p.IdVarProd == existingVar!.IdVarProd, Xunit.TestContext.Current.CancellationToken);
            var existingCodBar = await ctx.CodigosBarra.FirstOrDefaultAsync(c => c.IdPresProd == existingPres!.IdPresProd, Xunit.TestContext.Current.CancellationToken);
            var existingCodBarCode = existingCodBar!.Codigo;

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-06", "S88-PROD-06-UN", codBar: existingCodBarCode)
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsFailure);
            Assert.Contains("CODBAR_EXISTE", r.Error?.Code ?? "");
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_CategoriaNueva_CreaJerarquia()
    {
        // S88.2 K2/K4: categoría inexistente ya no falla; se crea jerarquía
        // desde CategoriaPadre/Categoria del TVP.
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-07", "S88-PROD-07-UN", codBar: "5901234123468");
            fila.CategoriaPadre = "S88-Padre";
            fila.Categoria = "S88-Hija";
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess, r.Error?.Message);
            using var ctx2 = f.NewContext();
            var hija = await ctx2.Categorias
                .Include(c => c.Padre)
                .FirstOrDefaultAsync(c => c.IdTenant == S88ImportFixture.Tenant && c.Codigo == "S88-HIJA", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(hija);
            Assert.NotNull(hija.Padre);
            Assert.Equal("S88-PADRE", hija.Padre.Codigo);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_MarcaNueva_CreaMarca()
    {
        // S88.2 K2: marca inexistente ya no falla; se crea.
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-08", "S88-PROD-08-UN", codBar: "5901234123469");
            fila.Marca = "S88-Mar-Nueva";
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess, r.Error?.Message);
            using var ctx2 = f.NewContext();
            var marca = await ctx2.Marcas
                .FirstOrDefaultAsync(m => m.IdTenant == S88ImportFixture.Tenant && m.Codigo == "S88-MAR-NUEVA", Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(marca);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_UnidadInexistente_Falla()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-09", "S88-PROD-09-UN", codBar: "5901234123470");
            fila.UnidadBase = "S88-UN-INEXISTENTE";
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsFailure);
            Assert.Contains("UNIDAD_NO_EXISTE", r.Error?.Code ?? "");
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_BodegaInexistenteParaStock_Falla()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-11", "S88-PROD-11-UN", codBar: "5901234123472", stock: 10, bod: "S88-BOD-INEXISTENTE")
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            // S88.2: el SP valida bodega por adelantado (BODEGA_NO_EXISTE), sin escribir.
            Assert.True(r.IsFailure);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_MultiplesProductosDistintos_TodosCreados()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-12", "S88-PROD-12-A", codBar: "5901234123473"),
                Fila("S88-PROD-13", "S88-PROD-13-A", codBar: "5901234123474"),
                Fila("S88-PROD-14", "S88-PROD-14-A", codBar: "5901234123475")
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess);
            Assert.Equal(3, r.Value.ProductosNuevos);
            Assert.Equal(3, r.Value.VariantesNuevas);
            Assert.Equal(3, r.Value.PresentacionesNuevas);
            Assert.Equal(3, r.Value.PreciosCreados);
            Assert.Equal(3, r.Value.CodigosBarraCreados);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_PrecioOfertaValido_CreaPrecioOferta()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-15", "S88-PROD-15-UN", codBar: "5901234123476", precio: 10000m);
            fila.PrecioOferta = 8000m;
            fila.OfertaDesde = new DateTime(2026, 1, 1);
            fila.OfertaHasta = new DateTime(2026, 12, 31);
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess);
            Assert.Equal(1, r.Value.PreciosCreados); // solo Precio; la oferta va a Oferta
            // S88.2 K2: oferta creada con referencia al precio vigente
            using var ctx2 = f.NewContext();
            var presId = await ctx2.Presentaciones
                .Where(pp => pp.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && pp.VarProd.Prod.Codigo == "S88-PROD-15"
                    && pp.VarProd.Codigo == "S88-PROD-15-UN")
                .Select(pp => pp.IdPresProd)
                .FirstAsync(Xunit.TestContext.Current.CancellationToken);
            var oferta = await ctx2.Ofertas
                .FirstOrDefaultAsync(o => o.IdPresProd == presId && o.Activo, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(oferta);
            Assert.Equal(8000m, oferta.PrecioOferta);
            Assert.Equal(new DateOnly(2026, 1, 1), oferta.FechaDesde);
            Assert.Equal(new DateOnly(2026, 12, 31), oferta.FechaHasta);
            var vigente = await ctx2.Precios
                .Where(p => p.IdPresProd == presId)
                .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio)
                .FirstAsync(Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(vigente.IdPrecio, oferta.IdPrecioReferencia);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    [Fact]
    public async Task SpRepo_Aplicar_PesoYManejaLote_Validos()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-16", "S88-PROD-16-UN", codBar: "5901234123477");
            fila.Pesable = "S";
            fila.ManejaLote = "S";
            fila.DiasVencimiento = 365;
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);

            Assert.True(r.IsSuccess);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    // ── S88.2 K1: vigencia, nunca UPDATE ──────────────────────────────────
    [Fact]
    public async Task SpRepo_Aplicar_PrecioCambia_NuevaVigenciaSinUpdate()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var r1 = await repo.AplicarAsync(S88ImportFixture.Tenant, 8,
                new List<ProductoImportTvpItem> { Fila("S88-PROD-20", "S88-PROD-20-UN", codBar: "5901234123481", precio: 9990m) },
                CancellationToken.None);
            Assert.True(r1.IsSuccess, r1.Error?.Message);
            Assert.Equal(1, r1.Value.PreciosCreados);

            // Re-aplicar igual: 0 precios nuevos (idempotente)
            var r2 = await repo.AplicarAsync(S88ImportFixture.Tenant, 8,
                new List<ProductoImportTvpItem> { Fila("S88-PROD-20", "S88-PROD-20-UN", codBar: "5901234123481", precio: 9990m) },
                CancellationToken.None);
            Assert.True(r2.IsSuccess, r2.Error?.Message);
            Assert.Equal(0, r2.Value.PreciosCreados);

            // Cambiar precio: nueva vigencia, historial preservado
            var r3 = await repo.AplicarAsync(S88ImportFixture.Tenant, 8,
                new List<ProductoImportTvpItem> { Fila("S88-PROD-20", "S88-PROD-20-UN", codBar: "5901234123481", precio: 10990m) },
                CancellationToken.None);
            Assert.True(r3.IsSuccess, r3.Error?.Message);
            Assert.Equal(1, r3.Value.PreciosCreados);

            using var ctx2 = f.NewContext();
            var precios = await ctx2.Precios
                .Where(p => p.PresProd.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && p.PresProd.VarProd.Prod.Codigo == "S88-PROD-20")
                .OrderBy(p => p.IdPrecio)
                .ToListAsync(Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(2, precios.Count);
            Assert.Equal(9990m, precios[0].Valor);
            Assert.Equal(10990m, precios[1].Valor);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    // ── S88.2 K3: TipoCod informado + Principal=1 ─────────────────────────
    [Fact]
    public async Task SpRepo_Aplicar_TipoCodYPrincipal_Correctos()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var filas = new List<ProductoImportTvpItem>
            {
                Fila("S88-PROD-21", "S88-PROD-21-A", codBar: "99988877701", tipoCod: 4), // 11-dígitos ficticio
                Fila("S88-PROD-21", "S88-PROD-21-B", codBar: "9999999999994", tipoCod: 1), // EAN-13 válido ficticio
                Fila("S88-PROD-21", "S88-PROD-21-C", codBar: null, tipoCod: null), // generado interno
            };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);
            Assert.True(r.IsSuccess, r.Error?.Message);
            Assert.Equal(3, r.Value.CodigosBarraCreados);

            using var ctx2 = f.NewContext();
            var prov = await ctx2.CodigosBarra.FirstAsync(c => c.Codigo == "99988877701", Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(4, prov.TipoCod);
            Assert.True(prov.Principal);
            var normal = await ctx2.CodigosBarra.FirstAsync(c => c.Codigo == "9999999999994", Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(1, normal.TipoCod);
            Assert.True(normal.Principal);
            var presIds = await ctx2.Presentaciones
                .Where(pp => pp.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && pp.VarProd.Prod.Codigo == "S88-PROD-21"
                    && pp.VarProd.Codigo == "S88-PROD-21-C")
                .Select(pp => pp.IdPresProd)
                .ToListAsync(Xunit.TestContext.Current.CancellationToken);
            var generado = await ctx2.CodigosBarra.FirstAsync(c => presIds.Contains(c.IdPresProd), Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(3, generado.TipoCod);
            Assert.True(generado.Principal);
            Assert.StartsWith("2", generado.Codigo);
            // 3 presentaciones, 3 principales (uno por presentación)
            var princCount = await ctx2.CodigosBarra
                .CountAsync(c => c.PresProd.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && c.PresProd.VarProd.Prod.Codigo == "S88-PROD-21"
                    && c.Principal, Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(3, princCount);
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    // ── S88.2 K2/K6: oferta sin fechas usa fecha de carga ─────────────────
    [Fact]
    public async Task SpRepo_Aplicar_OfertaSinFechas_UsaFechaCarga()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-22", "S88-PROD-22-UN", codBar: "5901234123482", precio: 10000m);
            fila.PrecioOferta = 8000m;
            var filas = new List<ProductoImportTvpItem> { fila };

            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, filas, CancellationToken.None);
            Assert.True(r.IsSuccess, r.Error?.Message);

            using var ctx2 = f.NewContext();
            var presId = await ctx2.Presentaciones
                .Where(pp => pp.VarProd.Prod.IdTenant == S88ImportFixture.Tenant
                    && pp.VarProd.Prod.Codigo == "S88-PROD-22")
                .Select(pp => pp.IdPresProd)
                .FirstAsync(Xunit.TestContext.Current.CancellationToken);
            var oferta = await ctx2.Ofertas.FirstOrDefaultAsync(o => o.IdPresProd == presId && o.Activo, Xunit.TestContext.Current.CancellationToken);
            Assert.NotNull(oferta);
            Assert.Equal(DateOnly.FromDateTime(DateTime.Today), oferta.FechaDesde);
            Assert.Null(oferta.FechaHasta);
            Assert.Equal(20.00m, oferta.PctReferencial);

            // Re-aplicar: idempotente, sin duplicar oferta
            var fila2 = Fila("S88-PROD-22", "S88-PROD-22-UN", codBar: "5901234123482", precio: 10000m);
            fila2.PrecioOferta = 8000m;
            var r2 = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, new List<ProductoImportTvpItem> { fila2 }, CancellationToken.None);
            Assert.True(r2.IsSuccess, r2.Error?.Message);
            using var ctx3 = f.NewContext();
            Assert.Equal(1, await ctx3.Ofertas.CountAsync(o => o.IdPresProd == presId && o.Activo, Xunit.TestContext.Current.CancellationToken));
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    // ── S93.1 (SP v5, D6): la columna Proveedor del TVP se ignora; ya no falla ni crea precio de compra ──
    [Fact]
    public async Task SpRepo_Aplicar_ProveedorEnTvp_SeIgnora_V5()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);
            var fila = Fila("S88-PROD-10", "S88-PROD-10-UN", codBar: "5901234123471");
            fila.Proveedor = "S88-PROV-INEXISTENTE";
            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, new List<ProductoImportTvpItem> { fila }, CancellationToken.None);
            Assert.True(r.IsSuccess, r.Error?.Message);
            Assert.Contains("\"ComprasCreadas\":0", r.Value.ResumenJson.Replace(" ", ""));
        }
        finally
        {
            await f.DisposeAsync();
        }
    }

    // ── S88.2 K2 → S93.1: mayorista por vigencia (SP §10 intacto, D5) y CostoCompra = costo promedio (D6) ──
    [Fact]
    public async Task SpRepo_Aplicar_MayoristaCreaVigencia_YCostoCompraActualizaCostoPromedio()
    {
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        try
        {
            using var ctx = f.NewContext();
            var repo = f.NewSpRepo(ctx);

            var fila = Fila("S88-PROD-23", "S88-PROD-23-UN", codBar: "5901234123483", precio: 10000m);
            fila.PrecioMayorista = 9000m;
            var r = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, new List<ProductoImportTvpItem> { fila }, CancellationToken.None);
            Assert.True(r.IsSuccess, r.Error?.Message);
            Assert.Contains("MayoristasCreados", r.Value.ResumenJson);

            using var ctx2 = f.NewContext();
            var pres = await ctx2.Presentaciones
                .Where(pp => pp.VarProd.Prod.IdTenant == S88ImportFixture.Tenant && pp.VarProd.Prod.Codigo == "S88-PROD-23")
                .Select(pp => new { pp.IdPresProd, pp.VarProd.CostoProm })
                .FirstAsync(Xunit.TestContext.Current.CancellationToken);
            Assert.Equal(5000m, pres.CostoProm);
            Assert.Equal(1, await ctx2.PreciosMayorista.CountAsync(m => m.IdPresProd == pres.IdPresProd, Xunit.TestContext.Current.CancellationToken));
            Assert.Equal(9000m, (await ctx2.PreciosMayorista.FirstAsync(m => m.IdPresProd == pres.IdPresProd, Xunit.TestContext.Current.CancellationToken)).Valor);

            // Re-aplicar igual: sin nuevas vigencias
            var fila2 = Fila("S88-PROD-23", "S88-PROD-23-UN", codBar: "5901234123483", precio: 10000m);
            fila2.PrecioMayorista = 9000m;
            var r2 = await repo.AplicarAsync(S88ImportFixture.Tenant, 8, new List<ProductoImportTvpItem> { fila2 }, CancellationToken.None);
            Assert.True(r2.IsSuccess, r2.Error?.Message);
            using var ctx3 = f.NewContext();
            Assert.Equal(1, await ctx3.PreciosMayorista.CountAsync(m => m.IdPresProd == pres.IdPresProd, Xunit.TestContext.Current.CancellationToken));
        }
        finally
        {
            await f.DisposeAsync();
        }
    }
}

