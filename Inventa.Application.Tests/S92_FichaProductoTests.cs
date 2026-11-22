using System;
using System.Collections.Generic;
using System.Linq;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

/// <summary>
/// S92: armado puro de la ficha de producto (sin BD) y estados de tienda. Reglas desde PublicCatalogSpecs:
/// R1 precio vigente (Activo, FecVig ≤ hoy, mayor FecVig y luego mayor IdPrecio), R2/R2' oferta efectiva
/// solo si referencia al vigente y lo mejora, R4 %.
/// </summary>
public sealed class S92_FichaProductoTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 14);

    private static FichaProductoBundle Bundle(
        IEnumerable<Precio>? precios = null, IEnumerable<Oferta>? ofertas = null,
        IEnumerable<CodBar>? codigos = null, bool varActiva = true, bool presVenta = true, bool visible = true, bool imagen = false)
    {
        var v = new VarProd { IdVarProd = 50, IdProd = 1, Codigo = "P1-01", Nombre = "Talla 2", Activo = varActiva };
        var pres = new PresProd { IdPresProd = 100, IdVarProd = 50, Descrip = "Polera / Talla 2", Activo = true, EsVenta = presVenta };
        var diag = new ProductoDiagnosticoRow
        {
            IdProd = 1, Codigo = "P1", Nombre = "Polera", Activo = true, VisibleStore = visible, TieneMarca = true,
            TieneImagen = imagen, Vendible = varActiva && presVenta, TienePrecio = (precios ?? []).Any(), Variantes = 1,
        };
        return new FichaProductoBundle(
            new FichaCabecera(1, "P1", "Polera", "Poleras", "Vestuario", "Ficcus", "UN", "Unidad", false, false, null, true, visible),
            diag, [v], [pres], (codigos ?? []).ToList(), (precios ?? []).ToList(), (ofertas ?? []).ToList(),
            new Dictionary<long, decimal> { [100L] = 7m }, imagen ? 1 : 0, imagen ? "img.jpg" : null, Hoy);
    }

    private static Precio P(long id, decimal valor, DateOnly fec, bool activo = true) =>
        new() { IdPrecio = id, IdPresProd = 100, Valor = valor, FecVig = fec, Activo = activo };

    [Fact]
    public void PrecioVigente_R1_DesempatePorIdYFuturoExcluido()
    {
        var f = ProductoFichaServiceAsync.ArmarFicha(Bundle(precios:
        [
            P(1, 1000m, Hoy.AddDays(-10)),
            P(2, 1100m, Hoy),            // mismo día que el 3: gana el Id mayor
            P(3, 1200m, Hoy),
            P(4, 9999m, Hoy.AddDays(5)), // futuro: no vigente
            P(5, 8888m, Hoy, activo: false),
        ]));
        var v = Assert.Single(f.Variantes);
        Assert.Equal(3, v.PrecioVigente!.Id);
        Assert.Equal(1200m, v.PrecioVigente.Valor);
    }

    [Fact]
    public void Oferta_SoloEfectivaSiReferenciaAlVigenteYLoMejora()
    {
        var viejo = P(1, 1000m, Hoy.AddDays(-30));
        var vigente = P(2, 1200m, Hoy.AddDays(-1));
        Oferta O(long id, decimal precio, Precio referencia) => new()
        {
            IdOferta = id, IdPresProd = 100, IdPrecioReferencia = referencia.IdPrecio, PrecioReferencia = referencia,
            PrecioOferta = precio, FechaDesde = Hoy.AddDays(-1), Activo = true, Publicado = true,
        };
        // Referencia al precio viejo: no efectiva (regla de referencia de oferta).
        var sinEfectiva = ProductoFichaServiceAsync.ArmarFicha(Bundle(precios: [viejo, vigente], ofertas: [O(10, 900m, viejo)]));
        Assert.Null(sinEfectiva.Variantes[0].Oferta);
        // Referencia al vigente y mejora: efectiva con % = 25 (1200 → 900).
        var efectiva = ProductoFichaServiceAsync.ArmarFicha(Bundle(precios: [viejo, vigente], ofertas: [O(11, 900m, vigente)]));
        Assert.Equal(900m, efectiva.Variantes[0].Oferta!.Precio);
        Assert.Equal(25, efectiva.Variantes[0].Oferta!.Pct);
    }

    [Fact]
    public void Codigos_PrincipalPrimero_TipoEnTexto_StockYCompras()
    {
        var f = ProductoFichaServiceAsync.ArmarFicha(Bundle(codigos:
        [
            new CodBar { IdCodBar = 1, IdPresProd = 100, Codigo = "111", TipoCod = 1, Principal = false },
            new CodBar { IdCodBar = 2, IdPresProd = 100, Codigo = "999", TipoCod = 4, Principal = true },
            new CodBar { IdCodBar = 3, IdPresProd = 100, Codigo = "2004000000129", TipoCod = 3, Principal = false },
        ]));
        var v = f.Variantes[0];
        Assert.Equal("999", v.Codigos[0].Codigo);
        Assert.True(v.Codigos[0].Principal);
        Assert.Equal("Proveedor", v.Codigos[0].Tipo);
        Assert.Contains(v.Codigos, c => c.Tipo == "Interno");
        Assert.Equal(7m, v.StockDisponible);
        Assert.Equal("Vestuario > Poleras", f.Categoria);
        Assert.Equal("Unidad (UN)", f.Unidad);
    }

    [Theory]
    [InlineData(true, true, true, true, EstadosTienda.Publicado)]
    [InlineData(true, true, true, false, EstadosTienda.PublicadoConAdvertencia)] // visible sin precio
    [InlineData(true, true, false, true, EstadosTienda.PublicadoConAdvertencia)] // visible sin variante a la venta
    [InlineData(false, true, true, true, EstadosTienda.PublicadoConAdvertencia)] // visible pero inactivo
    [InlineData(true, false, true, false, EstadosTienda.Listo)]                  // listo aunque falte precio (advertencia en razones)
    [InlineData(true, false, false, false, EstadosTienda.Pendiente)]
    [InlineData(false, false, true, true, EstadosTienda.Inactivo)]
    public void EstadoTienda(bool activo, bool visible, bool vendible, bool precio, string esperado)
        => Assert.Equal(esperado, PublicCatalogSpecs.EstadoTienda(activo, visible, vendible, precio));

    [Fact]
    public void Razones_Legibles()
    {
        var r = PublicCatalogSpecs.RazonesTienda(new ProductoDiagnosticoRow
        {
            Activo = true, Vendible = true, TienePrecio = false, TieneImagen = false, TieneMarca = false,
        });
        Assert.Contains(r, x => x.StartsWith("Sin precio vigente"));
        Assert.Contains("Sin imagen", r);
        Assert.Contains("Sin marca", r);
        Assert.DoesNotContain("Producto inactivo", r);
    }

    [Fact]
    public void Ficha_EstadoYRazonesDesdeDiagnostico()
    {
        var f = ProductoFichaServiceAsync.ArmarFicha(Bundle(precios: [P(1, 1000m, Hoy)], imagen: false));
        Assert.Equal(EstadosTienda.Publicado, f.Estado);
        Assert.Equal(["Sin imagen"], f.Razones);
    }
}

