using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Catalogo;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S94 K7: el escritor único del catálogo contra SQL real (localhost/Inventa), con el escritor de precios de S93.1
/// en la MISMA transacción. Cada prueba abre una transacción y la revierte: no deja filas.
/// Invariantes ADR-PROD-001 rev. 2: 1 presentación por variante, ≥ 1 código y exactamente 1 principal, Descrip y código
/// interno con la regla de dbo.sp_ImportarProductos v5.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S94_CatalogoEdicionDataTests(OfficialLocalFixture fx)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private static int Abarrotes(InventaDbContext ctx) =>
        ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == 2).Select(t => t.IdTenant).Single();

    private static CatalogoEdicionServiceAsync Servicio(InventaDbContext ctx)
    {
        var precios = new PreciosGrillaRepositoryAsync(ctx);
        var cfg = new ConfiguracionServiceAsync(new ConfiguracionRepositoryAsync(ctx), new TenantConfigInventaRepositoryAsync(ctx));
        var gestion = new GestionPreciosServiceAsync(precios, new PrecioGrillaServiceAsync(precios, cfg));
        return new CatalogoEdicionServiceAsync(new CatalogoEdicionRepositoryAsync(ctx), gestion);
    }

    private static async Task<(long IdCategoria, int IdUnidad)> Referencias(InventaDbContext ctx, int tenant) =>
        (await ctx.Categorias.AsNoTracking().Where(c => c.IdTenant == tenant && c.Activo).Select(c => c.IdCategoria).FirstAsync(Ct),
         await ctx.Unidades.AsNoTracking().OrderBy(u => u.IdUnidad).Select(u => u.IdUnidad).FirstAsync(Ct));

    private static string Unico(string prefijo) => $"{prefijo}-{Guid.NewGuid():N}"[..20];

    /// <summary>EAN-13 válido que no existe en la BD (prefijo 780, fuera del interno 2…).</summary>
    private static async Task<string> EanLibre(InventaDbContext ctx)
    {
        for (var i = 0; ; i++)
        {
            var base12 = "780" + Random.Shared.NextInt64(100_000_000, 999_999_999).ToString();
            var ean = base12 + CodigoBarraReglas.CalcularDvEan13(base12);
            if (!await ctx.CodigosBarra.AsNoTracking().AnyAsync(c => c.Codigo == ean, Ct)) return ean;
        }
    }

    private static async Task AssertInvariantes(InventaDbContext ctx, long idProd)
    {
        var vars = await ctx.Variantes.AsNoTracking().Where(v => v.IdProd == idProd)
            .Select(v => new { v.IdVarProd, v.Nombre, Prod = v.Prod.Nombre, v.Prod.IdUniBase,
                Pres = v.Presentaciones.Select(pp => new { pp.IdUnidad, pp.Descrip, pp.EsVenta, pp.EsCompra, pp.Activo, VarActivo = v.Activo,
                    Codigos = pp.CodigosBarra.Select(c => new { c.Codigo, c.Principal }).ToList() }).ToList() })
            .ToListAsync(Ct);
        Assert.NotEmpty(vars);
        foreach (var v in vars)
        {
            var pres = Assert.Single(v.Pres);
            Assert.Equal(v.IdUniBase, pres.IdUnidad);
            Assert.Equal(CatalogoEdicionReglas.Descripcion(v.Prod, v.Nombre), pres.Descrip);
            Assert.True(pres.EsVenta && pres.EsCompra);
            Assert.Equal(pres.VarActivo, pres.Activo);
            Assert.NotEmpty(pres.Codigos);
            Assert.Single(pres.Codigos, c => c.Principal);
        }
    }

    [Fact]
    public async Task CrearProducto_SinCodigo_GeneraInterno_Presentacion1a1_YPrecioInicial_YRevertir()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var (cat, uni) = await Referencias(ctx, tenant);
        var prefijo = CatalogoEdicionReglas.PrefijoInterno(tenant);
        var mayorAntes = await ctx.CodigosBarra.AsNoTracking().Where(c => c.TipoCod == 3 && c.Codigo.StartsWith(prefijo)).MaxAsync(c => (string?)c.Codigo, Ct);
        var nProd = await ctx.Productos.CountAsync(Ct);
        var codigo = Unico("S94P");

        await using (var tx = await ctx.Database.BeginTransactionAsync(Ct))
        {
            var r = await Servicio(ctx).CrearProductoAsync(tenant,
                new NuevoProductoDatos(codigo, "Producto S94", cat, null, uni, new NuevaVarianteDatos("UN", "Unidad", 12990m)), Ct);
            Assert.True(r.IsSuccess, r.Error?.Message);
            ctx.ChangeTracker.Clear();

            await AssertInvariantes(ctx, r.Value.IdProd);
            var cod = await ctx.CodigosBarra.AsNoTracking().SingleAsync(c => c.IdCodBar == r.Value.IdCodBar, Ct);
            Assert.Equal((byte)3, cod.TipoCod);
            Assert.True(cod.Principal);
            Assert.Equal(CatalogoEdicionReglas.SiguienteInterno(tenant, mayorAntes), cod.Codigo);
            Assert.True(EanCodigoValidator.EsEstandar(cod.Codigo, out _));

            var precio = await ctx.Precios.AsNoTracking().SingleAsync(p => p.IdPresProd == r.Value.IdPresProd, Ct);
            Assert.Equal((12990m, PublicCatalogSpecs.Hoy(), true), (precio.Valor, precio.FecVig, precio.Activo));
            Assert.Equal(r.Value.IdPrecio, precio.IdPrecio);
            var prod = await ctx.Productos.AsNoTracking().SingleAsync(p => p.IdProd == r.Value.IdProd, Ct);
            Assert.False(prod.VisibleStore);
            await tx.RollbackAsync(Ct);
        }
        ctx.ChangeTracker.Clear();
        Assert.Equal(nProd, await ctx.Productos.CountAsync(Ct));
        Assert.False(await ctx.Productos.AnyAsync(p => p.Codigo == codigo, Ct));
    }

    [Fact]
    public async Task CrearVariante_ConCodigoDeOtroTenant_409SinRevelarDuenio_YNoEscribeNada()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var (cat, uni) = await Referencias(ctx, tenant);
        // Un código que ya usa VESTUARIO (otro tenant): la unicidad es global.
        var ajeno = await ctx.CodigosBarra.AsNoTracking().Where(c => c.PresProd.VarProd.Prod.IdTenant != tenant && c.TipoCod == 1)
            .Select(c => c.Codigo).FirstAsync(Ct);

        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var svc = Servicio(ctx);
        var p = await svc.CrearProductoAsync(tenant, new NuevoProductoDatos(Unico("S94V"), "Producto S94", cat, null, uni, new NuevaVarianteDatos("A", "Talla A", 5000m)), Ct);
        Assert.True(p.IsSuccess, p.Error?.Message);
        var nVar = await ctx.Variantes.CountAsync(v => v.IdProd == p.Value.IdProd, Ct);

        var r = await svc.CrearVarianteAsync(tenant, p.Value.IdProd, new NuevaVarianteDatos("B", "Talla B", 5000m, ajeno, TipoCodigoAlta.Normal), Ct);
        Assert.False(r.IsSuccess);
        Assert.Equal((CodigosCatalogo.CodigoBarrasDuplicado, 409), (r.Error!.Code, r.Error.HttpStatusCode));
        Assert.DoesNotContain("VESTUARIO", r.Error.Message, StringComparison.OrdinalIgnoreCase);
        ctx.ChangeTracker.Clear();
        Assert.Equal(nVar, await ctx.Variantes.CountAsync(v => v.IdProd == p.Value.IdProd, Ct));

        var ok = await svc.CrearVarianteAsync(tenant, p.Value.IdProd, new NuevaVarianteDatos("B", "Talla B", 6000m, await EanLibre(ctx), TipoCodigoAlta.Normal), Ct);
        Assert.True(ok.IsSuccess, ok.Error?.Message);
        ctx.ChangeTracker.Clear();
        await AssertInvariantes(ctx, p.Value.IdProd);
        await tx.RollbackAsync(Ct);
    }

    [Fact]
    public async Task Codigos_AgregarMarcarPrincipalYQuitar_MantienenUnSoloPrincipal()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var (cat, uni) = await Referencias(ctx, tenant);
        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var svc = Servicio(ctx);
        var p = await svc.CrearProductoAsync(tenant, new NuevoProductoDatos(Unico("S94C"), "Producto S94", cat, null, uni, new NuevaVarianteDatos("UN", "Unidad", 1990m)), Ct);
        Assert.True(p.IsSuccess, p.Error?.Message);
        var idVar = p.Value.IdVarProd!.Value;
        var interno = p.Value.IdCodBar!.Value;

        var ean = await svc.AgregarCodigoAsync(tenant, idVar, new NuevoCodigoDatos(await EanLibre(ctx), TipoCodigoAlta.Normal, Principal: false), Ct);
        Assert.True(ean.IsSuccess, ean.Error?.Message);
        var prov = await svc.AgregarCodigoAsync(tenant, idVar, new NuevoCodigoDatos("12345678901", TipoCodigoAlta.Proveedor, Principal: true), Ct);
        Assert.True(prov.IsSuccess, prov.Error?.Message);
        ctx.ChangeTracker.Clear();
        await AssertInvariantes(ctx, p.Value.IdProd);
        Assert.True((await ctx.CodigosBarra.AsNoTracking().SingleAsync(c => c.IdCodBar == prov.Value.IdCodBar, Ct)).Principal);

        var quitarPrincipal = await svc.QuitarCodigoAsync(tenant, prov.Value.IdCodBar!.Value, Ct);
        Assert.Equal((CodigosCatalogo.PrincipalNoQuitable, 409), (quitarPrincipal.Error!.Code, quitarPrincipal.Error.HttpStatusCode));

        Assert.True((await svc.MarcarPrincipalAsync(tenant, interno, Ct)).IsSuccess);
        Assert.True((await svc.QuitarCodigoAsync(tenant, prov.Value.IdCodBar!.Value, Ct)).IsSuccess);
        ctx.ChangeTracker.Clear();
        await AssertInvariantes(ctx, p.Value.IdProd);
        var restantes = await ctx.CodigosBarra.AsNoTracking().Where(c => c.IdPresProd == p.Value.IdPresProd).OrderBy(c => c.IdCodBar).ToListAsync(Ct);
        Assert.Equal(2, restantes.Count);
        Assert.Equal(interno, Assert.Single(restantes, c => c.Principal).IdCodBar);

        // Otro tenant no ve ni toca el código.
        var ajeno = await svc.MarcarPrincipalAsync(OfficialLocalFixture.OtroTenant, interno, Ct);
        Assert.Equal(404, ajeno.Error!.HttpStatusCode);
        await tx.RollbackAsync(Ct);
    }

    [Fact]
    public async Task EditarProductoYVariante_RecalculaDescrip_ConservaIdPresProd()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var (cat, uni) = await Referencias(ctx, tenant);
        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var svc = Servicio(ctx);
        var codigo = Unico("S94E");
        var p = await svc.CrearProductoAsync(tenant, new NuevoProductoDatos(codigo, "Polera", cat, null, uni, new NuevaVarianteDatos("4", "Talla 4", 9990m)), Ct);
        Assert.True(p.IsSuccess, p.Error?.Message);

        var otraUnidad = await ctx.Unidades.AsNoTracking().Where(u => u.IdUnidad != uni).Select(u => u.IdUnidad).FirstAsync(Ct);
        var e = await svc.EditarProductoAsync(tenant, p.Value.IdProd, new EdicionProductoDatos(codigo, "Polera Liberty", cat, null, otraUnidad), Ct);
        Assert.True(e.IsSuccess, e.Error?.Message); // sin historia: la unidad sí cambia
        var v = await svc.EditarVarianteAsync(tenant, p.Value.IdVarProd!.Value, new EdicionVarianteDatos("Talla 4 años", Activo: false), Ct);
        Assert.True(v.IsSuccess, v.Error?.Message);
        ctx.ChangeTracker.Clear();

        var pres = await ctx.Presentaciones.AsNoTracking().SingleAsync(pp => pp.IdVarProd == p.Value.IdVarProd, Ct);
        Assert.Equal(p.Value.IdPresProd, pres.IdPresProd);
        Assert.Equal("Polera Liberty / Talla 4 años", pres.Descrip);
        Assert.Equal(otraUnidad, pres.IdUnidad);
        Assert.False(pres.Activo);
        await AssertInvariantes(ctx, p.Value.IdProd);
        await tx.RollbackAsync(Ct);
    }

    [Fact]
    public async Task CambiarUnidadBase_ConHistoria_409_SinCambios()
    {
        using var ctx = fx.NewContext();
        var tenant = Abarrotes(ctx);
        var master = await ctx.Productos.AsNoTracking().SingleAsync(p => p.IdTenant == tenant && p.Codigo == OfficialLocalFixture.MasterCodProd, Ct);
        var otraUnidad = await ctx.Unidades.AsNoTracking().Where(u => u.IdUnidad != master.IdUniBase).Select(u => u.IdUnidad).FirstAsync(Ct);

        await using var tx = await ctx.Database.BeginTransactionAsync(Ct);
        var r = await Servicio(ctx).EditarProductoAsync(tenant, master.IdProd,
            new EdicionProductoDatos(master.Codigo, master.Nombre + " (S94)", master.IdCategoria, master.IdMarca, otraUnidad), Ct);
        Assert.Equal((CodigosCatalogo.UnidadConHistoria, 409), (r.Error!.Code, r.Error.HttpStatusCode));
        ctx.ChangeTracker.Clear();
        var despues = await ctx.Productos.AsNoTracking().SingleAsync(p => p.IdProd == master.IdProd, Ct);
        Assert.Equal((master.Nombre, master.IdUniBase), (despues.Nombre, despues.IdUniBase));
        await tx.RollbackAsync(Ct);
    }

    [Fact]
    public async Task Reglas_IgualesAlSpImportarProductosV5_EnTodosLosDatosImportados()
    {
        using var ctx = fx.NewContext();
        // Descrip: todas las presentaciones (las crea el SP) siguen la regla C#.
        var filas = await ctx.Presentaciones.AsNoTracking()
            .Select(pp => new { pp.Descrip, Prod = pp.VarProd.Prod.Nombre, Var = pp.VarProd.Nombre }).ToListAsync(Ct);
        Assert.True(filas.Count > 1000, "Se esperan los datos importados de VESTUARIO.");
        Assert.Empty(filas.Where(f => f.Descrip != CatalogoEdicionReglas.Descripcion(f.Prod, f.Var)).Take(5));

        // Código interno: cada TipoCod 3 generado por el SP se reproduce con el formato C# (prefijo, secuencia y DV).
        var internos = await ctx.CodigosBarra.AsNoTracking().Where(c => c.TipoCod == 3)
            .Select(c => new { c.Codigo, c.PresProd.VarProd.Prod.IdTenant }).ToListAsync(Ct);
        Assert.NotEmpty(internos);
        foreach (var i in internos)
        {
            Assert.StartsWith(CatalogoEdicionReglas.PrefijoInterno(i.IdTenant), i.Codigo);
            Assert.Equal(i.Codigo, CodigoBarraReglas.FormatearInterno(i.IdTenant, long.Parse(i.Codigo.Substring(4, 8))));
        }
    }
}
