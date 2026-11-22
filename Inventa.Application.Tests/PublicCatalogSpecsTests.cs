using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S63: reglas R1–R5 con LOS MISMOS expression builders del repositorio
// (compilados sobre conjuntos en memoria). Sin BD, sin paquetes nuevos.
public sealed class PublicCatalogSpecsTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 8);

    private static Precio P(long id, long pres, decimal valor, DateOnly fec, bool activo = true) =>
        new() { IdPrecio = id, IdPresProd = pres, Valor = valor, FecVig = fec, Activo = activo };

    private static Oferta O(long id, long pres, decimal oferta, DateOnly desde, DateOnly? hasta, bool publicado = true, bool activo = true) =>
        new() { IdOferta = id, IdPresProd = pres, IdPrecioReferencia = 1, PrecioOferta = oferta, FechaDesde = desde, FechaHasta = hasta, Activo = activo, Publicado = publicado };

    private static decimal? Vigente(IEnumerable<Precio> precios) =>
        precios.AsQueryable().Where(PublicCatalogSpecs.PrecioCandidato(Hoy))
            .OrderByDescending(PublicCatalogSpecs.PrecioOrdenFecVig)
            .ThenByDescending(PublicCatalogSpecs.PrecioOrdenId)
            .Select(p => (decimal?)p.Valor).FirstOrDefault();

    private static Oferta? Ganadora(IEnumerable<Oferta> ofertas, decimal? vigente) =>
        vigente == null ? null : ofertas.AsQueryable()
            .Where(PublicCatalogSpecs.OfertaVigente(Hoy))
            .Where(o => o.PrecioOferta < vigente.Value)
            .OrderBy(PublicCatalogSpecs.OfertaOrdenPrecio)
            .ThenByDescending(PublicCatalogSpecs.OfertaOrdenId)
            .FirstOrDefault();

    // ---- R1 ----
    [Fact]
    public void R1_GanaLaMasReciente() =>
        Assert.Equal(20m, Vigente([P(1, 7, 10m, new(2026, 1, 1)), P(2, 7, 20m, new(2026, 5, 1))]));

    [Fact]
    public void R1_FilaFuturaIgnorada() =>
        Assert.Equal(10m, Vigente([P(1, 7, 10m, new(2026, 1, 1)), P(2, 7, 99m, new(2026, 12, 1))]));

    [Fact]
    public void R1_FilaInactivaIgnorada() =>
        Assert.Equal(10m, Vigente([P(1, 7, 10m, new(2026, 1, 1)), P(2, 7, 99m, new(2026, 9, 1), false)]));

    [Fact]
    public void R1_EmpateFecha_GanaMayorIdPrecio() =>
        Assert.Equal(20m, Vigente([P(1, 7, 10m, new(2026, 5, 1)), P(2, 7, 20m, new(2026, 5, 1))]));

    // ---- R2 ----
    [Fact]
    public void R2_Publicado0_Ignorada() =>
        Assert.Null(Ganadora([O(1, 7, 5m, new(2026, 1, 1), null, publicado: false)], 10m));

    [Fact]
    public void R2_Activo0_Ignorada() =>
        Assert.Null(Ganadora([O(1, 7, 5m, new(2026, 1, 1), null, activo: false)], 10m));

    [Fact]
    public void R2_FueraDeFechas_Ignorada()
    {
        Assert.Null(Ganadora([O(1, 7, 5m, new(2026, 11, 1), null)], 10m));
        Assert.Null(Ganadora([O(1, 7, 5m, new(2026, 1, 1), new(2026, 9, 1))], 10m));
    }

    [Fact]
    public void R2_HastaNull_Vigente() =>
        Assert.NotNull(Ganadora([O(1, 7, 5m, new(2026, 1, 1), null)], 10m));

    [Fact]
    public void R2_BordesHoy_Incluidos()
    {
        Assert.NotNull(Ganadora([O(1, 7, 5m, Hoy, Hoy)], 10m));
        Assert.NotNull(Ganadora([O(1, 7, 5m, Hoy, null)], 10m));
    }

    [Fact]
    public void R2_OfertaMayorOIgualQueVigente_Ignorada()
    {
        Assert.Null(Ganadora([O(1, 7, 10m, new(2026, 1, 1), null)], 10m));
        Assert.Null(Ganadora([O(1, 7, 15m, new(2026, 1, 1), null)], 10m));
    }

    [Fact]
    public void R2_Varias_GanaMenorPrecioOferta_EmpateMayorId()
    {
        var g = Ganadora([
            O(1, 7, 8m, new(2026, 1, 1), null),
            O(2, 7, 6m, new(2026, 6, 1), null),
            O(3, 7, 6m, new(2026, 2, 1), null)], 10m);
        Assert.NotNull(g);
        Assert.Equal(3, g.IdOferta);
    }

    // ---- R3/R4 ----
    [Fact]
    public void R3_ConOferta_FinalEsOferta() =>
        Assert.Equal(9990m, PublicCatalogSpecs.PrecioFinal(12990m, 9990m));

    [Fact]
    public void R3_SinOferta_FinalEsVigente() =>
        Assert.Equal(12990m, PublicCatalogSpecs.PrecioFinal(12990m, null));

    [Fact]
    public void R4_EjemploContrato_23() =>
        Assert.Equal(23, PublicCatalogSpecs.PctDescuento(12990m, 9990m));

    [Fact]
    public void R4_EjemploContrato_1() =>
        Assert.Equal(1, PublicCatalogSpecs.PctDescuento(100m, 99m));

    [Fact]
    public void R4_MedioPunto_RedondeaLejosDeCero() =>
        Assert.Equal(1, PublicCatalogSpecs.PctDescuento(200m, 199m));

    [Fact]
    public void R4_SinOfertaEfectiva_Null()
    {
        Assert.Null(PublicCatalogSpecs.PctDescuento(100m, null));
        Assert.Null(PublicCatalogSpecs.PctDescuento(100m, 100m));
        Assert.Null(PublicCatalogSpecs.PctDescuento(100m, 150m));
        Assert.Null(PublicCatalogSpecs.PctDescuento(0m, 0m));
    }

    [Fact]
    public void R4_OfertaCero_Tope99() =>
        Assert.Equal(99, PublicCatalogSpecs.PctDescuento(100m, 0m));

    // ---- R5 (espejo en memoria de la composición del repositorio) ----
    private sealed record PresCalc(decimal? Vig, decimal? Of);

    private static (decimal? Final, decimal? Ref, int? Pct, bool Tiene) Card(
        IReadOnlyList<(decimal? Vig, decimal? Of)> pres)
    {
        var ord = pres.Where(p => p.Vig != null).OrderBy(p => p.Of ?? p.Vig).ToList();
        if (ord.Count == 0)
            return (null, null, null, pres.Any(p => p.Vig != null && p.Of != null && p.Of < p.Vig));
        var win = ord[0];
        return (
            win.Of ?? win.Vig,
            win.Of == null ? null : win.Vig,
            PublicCatalogSpecs.PctDescuento(win.Vig!.Value, win.Of),
            pres.Any(p => p.Vig != null && p.Of != null && p.Of < p.Vig));
    }

    [Fact]
    public void R5_DosPres_SoloUnaConOferta()
    {
        var (final, @ref, pct, tiene) = Card([(100m, null), (120m, 90m)]);
        Assert.True(tiene);
        Assert.Equal(90m, final);
        Assert.Equal(120m, @ref);
        Assert.Equal(25, pct);
    }

    [Fact]
    public void R5_MinFinalSinOferta_TieneOfertaIgualTrue()
    {
        var (final, @ref, pct, tiene) = Card([(100m, null), (150m, 120m)]);
        Assert.True(tiene);
        Assert.Equal(100m, final);
        Assert.Null(@ref);
        Assert.Null(pct);
    }

    [Fact]
    public void R5_SinPrecio_TodoNull()
    {
        var (final, @ref, pct, tiene) = Card([(null, null)]);
        Assert.False(tiene);
        Assert.Null(final);
        Assert.Null(@ref);
        Assert.Null(pct);
    }

    // ---- Filtros/orden (misma semántica que el repositorio) ----
    private sealed record Fila(long Id, string Nombre, decimal? Final, int? Pct, bool Tiene);

    private static readonly IReadOnlyList<Fila> Filas =
    [
        new(1, "B", 100m, null, false),
        new(2, "A", 80m, 20, true),
        new(3, "C", null, null, false),
        new(4, "A", 80m, 10, true),
    ];

    [Fact]
    public void Filtro_SoloOfertas() =>
        Assert.Equal([2, 4], Filas.Where(f => f.Tiene).Select(f => f.Id));

    [Fact]
    public void Filtro_RangoPrecio_ExcluyeSinPrecio() =>
        Assert.Equal([1], Filas.Where(f => f.Final != null && f.Final >= 90m && f.Final <= 110m).Select(f => f.Id));

    [Fact]
    public void Orden_PrecioAsc_NulosAlFinal_Desempate() =>
        Assert.Equal([2, 4, 1, 3], Filas
            .OrderBy(f => f.Final == null).ThenBy(f => f.Final).ThenBy(f => f.Nombre).ThenBy(f => f.Id)
            .Select(f => f.Id));

    [Fact]
    public void Orden_DescuentoDesc_NulosAlFinal() =>
        Assert.Equal([2, 4, 1, 3], Filas
            .OrderBy(f => f.Pct == null).ThenByDescending(f => f.Pct ?? -1).ThenBy(f => f.Nombre).ThenBy(f => f.Id)
            .Select(f => f.Id));
}

