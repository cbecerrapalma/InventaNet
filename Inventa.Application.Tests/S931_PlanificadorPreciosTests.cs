using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

/// <summary>
/// S93.1 C3/C9: reglas de K3 en el planificador puro (sin BD). Cada prueba arma la instantánea de una
/// presentación y verifica errores, conflictos, omisiones, operaciones y el «después» de hoy.
/// </summary>
public sealed class S931_PlanificadorPreciosTests
{
    private const int T = 4;
    private const long P = 100;
    private static readonly DateOnly Hoy = new(2026, 10, 15);

    private static Precio Pr(long id, decimal valor, DateOnly fecVig, bool activo = true) =>
        new() { IdPrecio = id, IdPresProd = P, Valor = valor, FecVig = fecVig, Activo = activo };

    private static Oferta Of(long id, long refId, decimal precio, DateOnly desde, DateOnly? hasta = null, bool publicado = true) =>
        new() { IdOferta = id, IdPresProd = P, IdPrecioReferencia = refId, PrecioOferta = precio, FechaDesde = desde, FechaHasta = hasta, Activo = true, Publicado = publicado };

    private static Dictionary<long, SnapshotPrecios> Snap(IEnumerable<Precio> precios, IEnumerable<Oferta>? ofertas = null, int tenant = T) =>
        new() { [P] = new SnapshotPrecios(P, tenant, "V-1", "Prod / Var", precios.ToList(), (ofertas ?? []).ToList()) };

    private static PlanCambiosPrecios Plan(Dictionary<long, SnapshotPrecios> s, params CambioPrecio[] c) =>
        PlanificadorPrecios.Planificar(c, s, T, Hoy, decimales: 0);

    private static CambioPrecio Oficial(decimal? valor = null, long? visto = 1, DateOnly? desde = null, OpcionOferta? con = null, decimal? pct = null, RedondeoPrecioModo red = RedondeoPrecioModo.Ninguno) =>
        new(P, TipoCambioPrecio.PrecioOficial, Valor: valor, PctAjuste: pct, Redondeo: red, Desde: desde, IdPrecioVigenteVisto: visto, ConOferta: con);

    private static CambioPrecio Oferta(decimal? precio = null, decimal? pct = null, long? visto = 1, long? ofVista = null, DateOnly? desde = null, DateOnly? hasta = null) =>
        new(P, TipoCambioPrecio.Oferta, PrecioOferta: precio, PctDescuento: pct, Desde: desde, Hasta: hasta, IdPrecioVigenteVisto: visto, IdOfertaVista: ofVista);

    // ── Precio oficial ────────────────────────────────────────────────────────────

    [Fact]
    public void PrecioOficial_DesdeHoy_InsertaVigencia_YCambiaElEfectivo()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1200));
        Assert.True(plan.Valido);
        var op = Assert.IsType<OpInsertarPrecio>(Assert.Single(plan.Operaciones));
        Assert.Equal((1200m, Hoy), (op.Valor, op.FecVig));
        var r = Assert.Single(plan.Presentaciones);
        Assert.Equal((1000m, 1200m), (r.EfectivoAntes!.Value, r.EfectivoDespues!.Value));
    }

    [Fact]
    public void PrecioOficial_FechaPasada_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1200, desde: Hoy.AddDays(-1)));
        Assert.Equal(CodigosPrecio.FechaPasada, Assert.Single(plan.Errores).Codigo);
        Assert.Empty(plan.Operaciones);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1200.5)]
    public void PrecioOficial_Invalido_CeroNegativoODecimalesEnClp(decimal valor)
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(valor));
        Assert.Equal(CodigosPrecio.PrecioInvalido, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void PrecioOficial_Decimales_PermitidosEnOtraMoneda()
    {
        var plan = PlanificadorPrecios.Planificar([Oficial(1200.5m)], Snap([Pr(1, 1000, Hoy.AddDays(-10))]), T, Hoy, decimales: 2);
        Assert.True(plan.Valido);
    }

    [Fact]
    public void PrecioOficial_IgualAlVigente_SeOmiteSinCambio()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1000));
        Assert.True(plan.Valido);
        Assert.Equal(CodigosPrecio.SinCambio, Assert.Single(plan.Omitidos).Codigo);
        Assert.Empty(plan.Operaciones);
        Assert.Empty(plan.Presentaciones);
    }

    [Theory]
    [InlineData(10, RedondeoPrecioModo.Ninguno, 11990)]   // 10.900 × 1,10 = 11.990
    [InlineData(10, RedondeoPrecioModo.Termina990, 11990)]
    [InlineData(7, RedondeoPrecioModo.Termina990, 11990)] // 11.663 → 11.990
    [InlineData(7, RedondeoPrecioModo.A100, 11700)]
    [InlineData(-5, RedondeoPrecioModo.A10, 10360)]       // 10.355 → 10.360
    public void PrecioOficial_PorPorcentaje_ConRedondeo(decimal pct, RedondeoPrecioModo modo, decimal esperado)
    {
        var plan = Plan(Snap([Pr(1, 10900, Hoy.AddDays(-10))]), Oficial(pct: pct, red: modo));
        Assert.Equal(esperado, Assert.IsType<OpInsertarPrecio>(Assert.Single(plan.Operaciones)).Valor);
    }

    [Fact]
    public void PrecioOficial_PorPorcentaje_SinPrecio_Error()
    {
        var plan = Plan(Snap([]), Oficial(pct: 10, visto: null));
        Assert.Equal(CodigosPrecio.SinPrecioOficial, Assert.Single(plan.Errores).Codigo);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(0)]
    [InlineData(501)]
    public void PrecioOficial_PorcentajeFueraDeRango_Error(decimal pct)
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(pct: pct));
        Assert.Equal(CodigosPrecio.PctInvalido, Assert.Single(plan.Errores).Codigo);
    }

    [Theory]
    [InlineData(0, 990)]
    [InlineData(990, 990)]
    [InlineData(991, 1990)]
    [InlineData(12345, 12990)]
    [InlineData(12990, 12990)]
    [InlineData(13000, 13990)]
    public void Redondeo_Termina990(decimal v, decimal esperado) =>
        Assert.Equal(esperado, RedondeoPrecio.Aplicar(v, RedondeoPrecioModo.Termina990));

    [Fact]
    public void PrecioOficial_Programado_NoCambiaElEfectivoDeHoy()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1500, desde: Hoy.AddDays(3)));
        Assert.True(plan.Valido);
        var r = Assert.Single(plan.Presentaciones);
        Assert.Equal(1000m, r.EfectivoDespues);
        Assert.Contains(r.Acciones, a => a.Contains("programado"));
    }

    // ── D3: precio con oferta ─────────────────────────────────────────────────────

    [Fact]
    public void D3_PorDefecto_MantieneLaOferta_SiSigueSiendoMenor()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-2), Hoy.AddDays(20))]);
        var plan = Plan(s, Oficial(1200));
        Assert.True(plan.Valido);
        // la oferta vieja termina ayer; se crea una igual que apunta al precio nuevo (por clave)
        var cerrar = Assert.Single(plan.Operaciones.OfType<OpCerrarOferta>());
        Assert.Equal(Hoy.AddDays(-1), cerrar.FechaHasta);
        var nueva = Assert.Single(plan.Operaciones.OfType<OpInsertarOferta>());
        var precio = Assert.Single(plan.Operaciones.OfType<OpInsertarPrecio>());
        Assert.Equal(precio.Clave, nueva.ClavePrecioNuevo);
        Assert.Null(nueva.IdPrecioReferencia);
        Assert.Equal((800m, Hoy, (DateOnly?)Hoy.AddDays(20)), (nueva.PrecioOferta, nueva.Desde, nueva.Hasta));
        Assert.Equal(33.33m, nueva.PctReferencial);
        var r = Assert.Single(plan.Presentaciones);
        Assert.Equal((800m, 800m), (r.EfectivoAntes!.Value, r.EfectivoDespues!.Value));
        Assert.Equal(1200m, r.OficialDespues);
    }

    [Fact]
    public void D3_Terminar_CierraLaOferta_SinCrearOtra()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-2))]);
        var plan = Plan(s, Oficial(1200, con: OpcionOferta.Terminar));
        Assert.True(plan.Valido);
        Assert.Single(plan.Operaciones.OfType<OpCerrarOferta>());
        Assert.Empty(plan.Operaciones.OfType<OpInsertarOferta>());
        Assert.Equal(1200m, Assert.Single(plan.Presentaciones).EfectivoDespues);
    }

    [Fact]
    public void D3_PorDefecto_TerminaSiLaOfertaQuedaSobreElNuevoPrecio()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-2))]);
        var plan = Plan(s, Oficial(700));
        Assert.True(plan.Valido);
        Assert.Empty(plan.Operaciones.OfType<OpInsertarOferta>());
        Assert.Contains(Assert.Single(plan.Presentaciones).Avisos, a => a.Contains("termina"));
    }

    [Fact]
    public void D3_MantenerExplicito_SobreElNuevoPrecio_Error()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-2))]);
        var plan = Plan(s, Oficial(700, con: OpcionOferta.Mantener));
        Assert.Equal(CodigosPrecio.OfertaQuedariaSobrePrecio, Assert.Single(plan.Errores).Codigo);
        Assert.Empty(plan.Operaciones);
    }

    [Fact]
    public void D3_OfertaQueEmpiezaHoy_SeAnulaYSeRecrea()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy)]);
        var plan = Plan(s, Oficial(1200));
        Assert.Single(plan.Operaciones.OfType<OpAnularOferta>());
        Assert.Single(plan.Operaciones.OfType<OpInsertarOferta>());
    }

    // ── Oferta (D2) ───────────────────────────────────────────────────────────────

    [Fact]
    public void Oferta_PorPorcentaje_CalculaPrecioYPct()
    {
        var plan = Plan(Snap([Pr(1, 12990, Hoy.AddDays(-10))]), Oferta(pct: 20, hasta: Hoy.AddDays(7)));
        Assert.True(plan.Valido);
        var op = Assert.Single(plan.Operaciones.OfType<OpInsertarOferta>());
        Assert.Equal((10392m, 1L, 20m), (op.PrecioOferta, op.IdPrecioReferencia!.Value, op.PctReferencial));
        Assert.Equal(10392m, Assert.Single(plan.Presentaciones).EfectivoDespues);
    }

    [Fact]
    public void Oferta_SobreElPrecio_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oferta(precio: 1000));
        Assert.Equal(CodigosPrecio.OfertaSobrePrecio, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void Oferta_SinPrecioOficial_Error()
    {
        var plan = Plan(Snap([]), Oferta(precio: 500, visto: null));
        Assert.Equal(CodigosPrecio.SinPrecioOficial, Assert.Single(plan.Errores).Codigo);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(100)]
    public void Oferta_PctFueraDeRango_Error(decimal pct)
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oferta(pct: pct));
        Assert.Equal(CodigosPrecio.PctInvalido, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void Oferta_RangoInvalido_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oferta(precio: 800, desde: Hoy.AddDays(5), hasta: Hoy.AddDays(2)));
        Assert.Equal(CodigosPrecio.OfertaRangoInvalido, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void Oferta_FechaPasada_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oferta(precio: 800, desde: Hoy.AddDays(-1)));
        Assert.Equal(CodigosPrecio.FechaPasada, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void D2_OfertaNueva_ReemplazaALaVigente_YALaProgramada()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))],
            [Of(7, 1, 900, Hoy.AddDays(-3)), Of(8, 1, 850, Hoy.AddDays(5), Hoy.AddDays(9))]);
        var plan = Plan(s, Oferta(precio: 700, ofVista: 7));
        Assert.True(plan.Valido);
        Assert.Equal(Hoy.AddDays(-1), Assert.Single(plan.Operaciones.OfType<OpCerrarOferta>()).FechaHasta); // 7 termina ayer
        Assert.Equal(8, Assert.Single(plan.Operaciones.OfType<OpAnularOferta>()).IdOferta);                  // 8 se anula
        Assert.Equal(700m, Assert.Single(plan.Operaciones.OfType<OpInsertarOferta>()).PrecioOferta);
    }

    [Fact]
    public void Oferta_YPrecio_EnElMismoLote_LaOfertaApuntaAlPrecioNuevo()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1500), Oferta(pct: 10));
        Assert.True(plan.Valido);
        var precio = Assert.Single(plan.Operaciones.OfType<OpInsertarPrecio>());
        var oferta = Assert.Single(plan.Operaciones.OfType<OpInsertarOferta>());
        Assert.Equal(precio.Clave, oferta.ClavePrecioNuevo);
        Assert.Equal(1350m, oferta.PrecioOferta);
        Assert.Equal(1350m, Assert.Single(plan.Presentaciones).EfectivoDespues);
    }

    [Fact]
    public void Oferta_ConPrecioProgramadoDentroDelPeriodo_Avisa()
    {
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10)), Pr(2, 1100, Hoy.AddDays(4))]);
        var plan = Plan(s, Oferta(precio: 800, hasta: Hoy.AddDays(10)));
        Assert.True(plan.Valido);
        Assert.Contains(Assert.Single(plan.Presentaciones).Avisos, a => a.Contains("dejará sin efecto"));
    }

    // ── Terminar / anular ─────────────────────────────────────────────────────────

    [Fact]
    public void TerminarOferta_Empezada_FechaHastaAyer()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-3))]),
            new CambioPrecio(P, TipoCambioPrecio.TerminarOferta, IdOfertaVista: 7));
        Assert.Equal(Hoy.AddDays(-1), Assert.IsType<OpCerrarOferta>(Assert.Single(plan.Operaciones)).FechaHasta);
        Assert.Equal(1000m, Assert.Single(plan.Presentaciones).EfectivoDespues);
    }

    [Fact]
    public void TerminarOferta_QueEmpiezaHoy_SeAnula()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy)]),
            new CambioPrecio(P, TipoCambioPrecio.TerminarOferta, IdOfertaVista: 7));
        Assert.IsType<OpAnularOferta>(Assert.Single(plan.Operaciones));
    }

    [Fact]
    public void TerminarOferta_YPrecio_EnElMismoLote_NoMantieneLaOfertaTerminada()
    {
        // Regresión (navegador S93.1): antes daba un falso 409 porque D3 recreaba la oferta antes de terminarla.
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy)]);
        var plan = Plan(s, Oficial(1200), new CambioPrecio(P, TipoCambioPrecio.TerminarOferta, IdOfertaVista: 7));
        Assert.True(plan.Valido);
        Assert.Empty(plan.Desactualizados);
        Assert.Equal(7, Assert.Single(plan.Operaciones.OfType<OpAnularOferta>()).IdOferta);
        Assert.Empty(plan.Operaciones.OfType<OpInsertarOferta>());
        Assert.Equal(1200m, Assert.Single(plan.Presentaciones).EfectivoDespues);
    }

    [Fact]
    public void TerminarOferta_Inexistente_EsConflicto()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), new CambioPrecio(P, TipoCambioPrecio.TerminarOferta, IdOfertaVista: 99));
        Assert.True(plan.HayConflicto);
        Assert.Empty(plan.Operaciones);
    }

    [Fact]
    public void AnularProgramado_Futuro_SeAnula()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10)), Pr(2, 1200, Hoy.AddDays(2))]),
            new CambioPrecio(P, TipoCambioPrecio.AnularProgramado, IdPrecio: 2));
        Assert.Equal(2, Assert.IsType<OpAnularPrecio>(Assert.Single(plan.Operaciones)).IdPrecio);
    }

    [Fact]
    public void AnularProgramado_QueYaRige_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), new CambioPrecio(P, TipoCambioPrecio.AnularProgramado, IdPrecio: 1));
        Assert.Equal(CodigosPrecio.ProgramadoNoAnulable, Assert.Single(plan.Errores).Codigo);
    }

    [Fact]
    public void AnularProgramado_ReferenciaDeOferta_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10)), Pr(2, 1200, Hoy.AddDays(2))], [Of(7, 2, 900, Hoy.AddDays(2))]),
            new CambioPrecio(P, TipoCambioPrecio.AnularProgramado, IdPrecio: 2));
        Assert.Equal(CodigosPrecio.ProgramadoNoAnulable, Assert.Single(plan.Errores).Codigo);
    }

    // ── Concurrencia, tenant, atomicidad ──────────────────────────────────────────

    [Fact]
    public void PrecioVistoDistinto_EsConflicto_SinOperaciones()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10)), Pr(2, 1100, Hoy.AddDays(-1))]), Oficial(1200, visto: 1));
        Assert.Equal([P], plan.Desactualizados);
        Assert.False(plan.Valido);
        Assert.Empty(plan.Operaciones);
    }

    [Fact]
    public void OfertaVistaDistinta_EsConflicto()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))], [Of(7, 1, 800, Hoy.AddDays(-3))]), Oferta(precio: 700, ofVista: null));
        Assert.True(plan.HayConflicto);
    }

    [Fact]
    public void SinVerificarVistos_NoHayConflicto()
    {
        var plan = PlanificadorPrecios.Planificar(
            [new CambioPrecio(P, TipoCambioPrecio.PrecioOficial, Valor: 1200, VerificarVistos: false)],
            Snap([Pr(1, 1000, Hoy.AddDays(-10))]), T, Hoy, 0);
        Assert.True(plan.Valido);
    }

    [Fact]
    public void OtroTenant_PresentacionNoEncontrada()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))], tenant: 5), Oficial(1200));
        Assert.Equal(CodigosPrecio.PresentacionNoEncontrada, Assert.Single(plan.Errores).Codigo);
        Assert.Empty(plan.Operaciones);
    }

    [Fact]
    public void Duplicado_MismoTipo_Error()
    {
        var plan = Plan(Snap([Pr(1, 1000, Hoy.AddDays(-10))]), Oficial(1200), Oficial(1300));
        Assert.Contains(plan.Errores, e => e.Codigo == CodigosPrecio.CambioDuplicado && e.Indice == 1);
        Assert.False(plan.Valido);
    }

    [Fact]
    public void Lote_ConUnaFilaMala_NoEsValido_YElServicioNoEscribe()
    {
        // Dos presentaciones: una válida y otra con error. El plan entero queda inválido.
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-10))]);
        s[200] = new SnapshotPrecios(200, T, "V-2", "Otro", [new Precio { IdPrecio = 9, IdPresProd = 200, Valor = 500, FecVig = Hoy.AddDays(-5), Activo = true }], []);
        var plan = PlanificadorPrecios.Planificar(
            [Oficial(1200), new CambioPrecio(200, TipoCambioPrecio.PrecioOficial, Valor: -1, IdPrecioVigenteVisto: 9)], s, T, Hoy, 0);
        Assert.False(plan.Valido);
        Assert.Equal(1, Assert.Single(plan.Errores).Indice);
    }

    [Fact]
    public void Referencia_SeleccionDeOfertaEfectiva_IgualQueLaTienda()
    {
        // La oferta más barata apunta a una referencia vieja: la tienda toma la siguiente efectiva.
        var s = Snap([Pr(1, 1000, Hoy.AddDays(-20), activo: false), Pr(2, 900, Hoy.AddDays(-10))],
            [Of(5, 1, 500, Hoy.AddDays(-15)), Of(6, 2, 800, Hoy.AddDays(-5))]);
        var vig = PublicCatalogSpecs.PrecioVigente(s[P].Precios, Hoy);
        var ef = PublicCatalogSpecs.SeleccionarOfertaEfectiva(s[P].OfertasActivas, vig, Hoy);
        Assert.Equal(6, ef!.IdOferta);
        var (estado, motivo) = PublicCatalogSpecs.CalcularEstado(s[P].OfertasActivas[0], vig!.IdPrecio, vig.Valor, Hoy, esLaEfectiva: false);
        Assert.Equal((EstadoOferta.SinEfecto, MotivoSinEfecto.ReferenciaDesactualizada), (estado, motivo));
    }
}
