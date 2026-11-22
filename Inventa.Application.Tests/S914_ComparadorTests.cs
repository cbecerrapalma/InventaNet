using System;
using System.Collections.Generic;
using System.Linq;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

/// <summary>
/// S91.4: comparador puro de la vista previa de cambios. Debe predecir exactamente lo que hará
/// dbo.sp_ImportarProductos v3 (última vigencia FecVig DESC/Id DESC, vacío conserva, Activo, códigos).
/// </summary>
public sealed class S914_ComparadorTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 14);

    // Catálogo: producto P1 (cat Abarrotes > Despensa, marca Marca X) con variante V1 (pres 100, principal 7800000000017).
    private static (CatalogoImportacion Cat, EstadoComercialImportacion Com) Base(
        decimal? ultimoPrecio = 1000m, bool activo = true, bool visible = false, int? idMarca = 1)
    {
        var padre = new Categoria { IdCategoria = 10, IdTenant = 4, Codigo = "ABARROTES", Nombre = "Abarrotes" };
        var hija = new Categoria { IdCategoria = 11, IdTenant = 4, Codigo = "DESPENSA", Nombre = "Despensa", IdPadre = 10 };
        var marca = new Marca { IdMarca = 1, IdTenant = 4, Codigo = "FIC", Nombre = "Ficcus" }; // código ≠ UPPER(nombre)
        var marcaX = new Marca { IdMarca = 2, IdTenant = 4, Codigo = "MARCA X", Nombre = "Marca X" };
        var prod = new Prod
        {
            IdProd = 1, IdTenant = 4, Codigo = "P1", Nombre = "Arroz", IdCategoria = 11, IdMarca = idMarca, IdUniBase = 1,
            EsPesable = false, ManejaLot = false, DiasVen = null, Activo = activo, VisibleStore = visible,
        };
        var variante = new VarProd { IdVarProd = 50, IdProd = 1, Codigo = "V1", Nombre = "1 kg", CostoProm = 0m, Prod = prod };
        var pres = new PresProd { IdPresProd = 100, IdVarProd = 50, IdUnidad = 1, Descrip = "Arroz / 1 kg", VarProd = variante };
        var cod = new CodBar { IdCodBar = 1, IdPresProd = 100, Codigo = "7800000000017", TipoCod = 1, Principal = true, PresProd = pres };
        var cat = new CatalogoImportacion([prod], [variante], [cod], [padre, hija], [marca, marcaX],
            [new Unidad { IdUnidad = 1, Codigo = "UN", Nombre = "Unidad" }]);
        var com = new EstadoComercialImportacion
        {
            Presentaciones = new Dictionary<(long, int), long> { [(50L, 1)] = 100L },
            UltimoPrecio = ultimoPrecio is { } u ? new Dictionary<long, decimal> { [100L] = u } : new Dictionary<long, decimal>(),
        };
        return (cat, com);
    }

    private static ProductoImportTvpItem Item(Action<ProductoImportTvpItem>? cambiar = null)
    {
        var it = new ProductoImportTvpItem
        {
            CodigoProducto = "P1", NombreProducto = "Arroz", CategoriaPadre = "Abarrotes", Categoria = "Despensa",
            UnidadBase = "UN", CodigoVariante = "V1", NombreVariante = "1 kg", CodigoBarras = "7800000000017", TipoCod = 1,
            PrecioVenta = 1000m,
        };
        cambiar?.Invoke(it);
        return it;
    }

    private static CambiosImportacion Comparar(ProductoImportTvpItem it, (CatalogoImportacion Cat, EstadoComercialImportacion Com) b)
        => ProductoImportacionComparador.Comparar([(2, it)], b.Cat, b.Com, Hoy);

    [Fact]
    public void SinCambios_CuandoTodoCoincide()
    {
        var r = Comparar(Item(), Base());
        Assert.False(r.Resumen.HayCambios);
        Assert.Equal(AccionCambio.SinCambios, r.Filas[0].Accion);
    }

    [Fact]
    public void NombreCambiado_EsModificado()
    {
        var r = Comparar(Item(i => i.NombreProducto = "Arroz grado 1"), Base());
        var c = Assert.Single(r.Filas[0].Campos);
        Assert.Equal(ProductoImportacionComparador.CampoNombreProducto, c.Campo);
        Assert.Equal(("Arroz", "Arroz grado 1"), (c.Actual, c.Nuevo));
        Assert.Equal(1, r.Resumen.Modificados);
    }

    [Fact]
    public void PrecioIgual_NoCambia_PrecioDistinto_Cambia()
    {
        Assert.False(Comparar(Item(i => i.PrecioVenta = 1000.0000m), Base()).Resumen.HayCambios);
        var r = Comparar(Item(i => i.PrecioVenta = 1100m), Base());
        var c = Assert.Single(r.Filas[0].Campos);
        Assert.Equal(ProductoImportacionComparador.CampoPrecioVenta, c.Campo);
        Assert.Equal("$1.100", c.Nuevo);
    }

    [Fact]
    public void SinPrecioPrevio_EsCambio()
    {
        var r = Comparar(Item(), Base(ultimoPrecio: null));
        Assert.Equal(ProductoImportacionComparador.CampoPrecioVenta, Assert.Single(r.Filas[0].Campos).Campo);
    }

    [Fact]
    public void CeldasVacias_EnExistente_Conservan()
    {
        // Marca vacía (actual "Ficcus" con código FIC), banderas vacías, días vacíos: nada cambia.
        var r = Comparar(Item(i => { i.Marca = null; i.Pesable = null; i.ManejaLote = null; i.PublicarEnTienda = null; i.Activo = null; i.DiasVencimiento = null; }),
            Base(visible: true));
        Assert.False(r.Resumen.HayCambios);
    }

    [Fact]
    public void ActivoN_DesactivaYSeInforma()
    {
        var r = Comparar(Item(i => i.Activo = "N"), Base());
        var c = Assert.Single(r.Filas[0].Campos);
        Assert.Equal(ProductoImportacionComparador.CampoActivo, c.Campo);
        Assert.Equal(("Sí", "No"), (c.Actual, c.Nuevo));
    }

    [Fact]
    public void Marca_SeResuelvePorCodigoUpperComoElSp()
    {
        // "Ficcus" => código FICCUS (no existe: la actual tiene código FIC) => el SP crearía otra marca.
        var r = Comparar(Item(i => i.Marca = "Ficcus"), Base());
        var c = Assert.Single(r.Filas[0].Campos);
        Assert.Equal(ProductoImportacionComparador.CampoMarca, c.Campo);
        Assert.EndsWith("(se creará)", c.Nuevo);
        // "marca x" => código MARCA X existente (id 2) ≠ actual (1) => cambio sin crear.
        var r2 = Comparar(Item(i => i.Marca = "marca x"), Base());
        Assert.DoesNotContain("se creará", Assert.Single(r2.Filas[0].Campos).Nuevo);
    }

    [Fact]
    public void ProductoOVarianteNuevos_SonNuevo()
    {
        var r = Comparar(Item(i => { i.CodigoProducto = "P2"; i.CodigoVariante = "V9"; }), Base());
        Assert.Equal(AccionCambio.Nuevo, r.Filas[0].Accion);
        var r2 = Comparar(Item(i => i.CodigoVariante = "V2"), Base());
        Assert.Equal(AccionCambio.Nuevo, r2.Filas[0].Accion);
        Assert.Equal(1, r2.Resumen.Nuevos);
    }

    [Fact]
    public void CodigoBarras_NuevoPrincipal_YVacioConserva()
    {
        var r = Comparar(Item(i => i.CodigoBarras = "7800000000024"), Base());
        var c = Assert.Single(r.Filas[0].Campos);
        Assert.Equal(ProductoImportacionComparador.CampoCodigoBarras, c.Campo);
        Assert.Equal("7800000000017", c.Actual);
        Assert.Contains("anterior queda como adicional", c.Nuevo);
        Assert.False(Comparar(Item(i => { i.CodigoBarras = null; i.TipoCod = null; }), Base()).Resumen.HayCambios);
    }

    [Fact]
    public void AdicionalExistente_NoCuenta_AdicionalNuevo_Cuenta()
    {
        Assert.False(Comparar(Item(i => i.CodigosAdicionales = "7800000000017"), Base()).Resumen.HayCambios);
        var r = Comparar(Item(i => i.CodigosAdicionales = "111; 111 ;222"), Base());
        Assert.Equal(2, r.Filas[0].Campos.Count(c => c.Campo == ProductoImportacionComparador.CampoCodigoAdicional));
    }

    [Fact]
    public void Oferta_IdenticaActiva_NoCuenta_Distinta_Cuenta()
    {
        var b = Base();
        b.Com = new EstadoComercialImportacion
        {
            Presentaciones = b.Com.Presentaciones, UltimoPrecio = b.Com.UltimoPrecio,
            OfertasActivas = new HashSet<(long, decimal, DateOnly)> { (100L, 800m, Hoy) },
        };
        Assert.False(Comparar(Item(i => i.PrecioOferta = 800m), b).Resumen.HayCambios); // desde vacío = hoy
        Assert.True(Comparar(Item(i => { i.PrecioOferta = 800m; i.OfertaDesde = new DateTime(2026, 11, 1); }), b).Resumen.HayCambios);
    }

    // S93.1 (D6): sin precio de compra por proveedor; CostoCompra solo actualiza el costo promedio.
    [Fact]
    public void CostoCompra_SeComparaConElCostoPromedio_SinPrecioDeCompra()
    {
        var b = Base();
        b.Cat.Variantes[0].CostoProm = 500m;
        Assert.False(Comparar(Item(i => { i.CostoCompra = 500m; i.Proveedor = "prov-1"; }), b).Resumen.HayCambios);
        var r = Comparar(Item(i => i.CostoCompra = 600m), b);
        Assert.Equal(ProductoImportacionComparador.CampoCosto, Assert.Single(r.Filas[0].Campos).Campo);
    }

    [Fact]
    public void CamposDeProducto_SeInformanUnaVezPorProducto()
    {
        var b = Base();
        var v2 = new VarProd { IdVarProd = 51, IdProd = 1, Codigo = "V2", Nombre = "5 kg", Prod = b.Cat.Productos[0] };
        var cat = b.Cat with { Variantes = [b.Cat.Variantes[0], v2] };
        var com = new EstadoComercialImportacion
        {
            Presentaciones = new Dictionary<(long, int), long> { [(50L, 1)] = 100L, [(51L, 1)] = 101L },
            UltimoPrecio = new Dictionary<long, decimal> { [100L] = 1000m, [101L] = 1000m },
        };
        var r = ProductoImportacionComparador.Comparar(
            [(2, Item(i => i.NombreProducto = "Otro")), (3, Item(i => { i.NombreProducto = "Otro"; i.CodigoVariante = "V2"; i.NombreVariante = "5 kg"; i.CodigoBarras = null; }))],
            cat, com, Hoy);
        Assert.Equal(1, r.Resumen.PorCampo[ProductoImportacionComparador.CampoNombreProducto]);
        Assert.Equal(AccionCambio.SinCambios, r.Filas[1].Accion);
    }

    [Fact]
    public void Huella_EsDeterminista_YDetectaDiferencias()
    {
        var a = Comparar(Item(i => i.PrecioVenta = 1100m), Base());
        var b = Comparar(Item(i => i.PrecioVenta = 1100m), Base());
        var c = Comparar(Item(i => i.PrecioVenta = 1200m), Base());
        Assert.Equal(a.Sha256, b.Sha256);
        Assert.NotEqual(a.Sha256, c.Sha256);
        Assert.Equal(64, a.Sha256.Length);
    }
}

