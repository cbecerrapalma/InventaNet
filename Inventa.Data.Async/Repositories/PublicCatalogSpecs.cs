using System.Linq.Expressions;
using Inventa.Domain.Entities;

namespace Inventa.Data.Async.Repositories;

// S63: reglas R1–R5 del catálogo público como expression builders.
// S77: R2' (referencia congelada) vive en OfertaEfectiva; el SP la replica.
// Única fuente de verdad: el repositorio las compone en LINQ-to-SQL
// (se evalúan en construcción de la query) y las pruebas las compilan
// sobre listas en memoria. Prohibido duplicar la regla fuera de aquí.
public static class PublicCatalogSpecs
{
    // "hoy" tal como lo calcula hoy el repositorio (UTC). No cambiar (H1).
    public static DateOnly Hoy() => DateOnly.FromDateTime(DateTime.UtcNow);

    // Visibilidad de producto (regla actual, sin cambios):
    // Activo + VisibleStore + variante activa con presentación Activa+EsVenta.
    public static Expression<Func<Prod, bool>> EsPublicable =>
        pr => pr.Activo && pr.VisibleStore
            && pr.Variantes.Any(v => v.Activo
                && v.Presentaciones.Any(pp => pp.Activo && pp.EsVenta));

    // S92 K3 — listo para publicar: la MISMA regla de EsPublicable sin la condición VisibleStore.
    // Invariante (probado): EsPublicable ≡ VisibleStore && ListoParaPublicar.
    public static Expression<Func<Prod, bool>> ListoParaPublicar =>
        pr => pr.Activo
            && pr.Variantes.Any(v => v.Activo
                && v.Presentaciones.Any(pp => pp.Activo && pp.EsVenta));

    // S92 K3 — diagnóstico por producto para la administración (lista, resumen, ficha). Proyección única
    // con las reglas de este archivo: Vendible = cuerpo de ListoParaPublicar sin Activo; TienePrecio = R1
    // (Activo && FecVig <= hoy) en alguna presentación vendible. TieneImagen usa la tabla de imágenes
    // (Prod no tiene navegación). EF la traduce a un SELECT con EXISTS (sin OR en WHERE).
    public static Expression<Func<Prod, ProductoDiagnosticoRow>> Diagnostico(DateOnly hoy, IQueryable<ProductoImagen> imagenes) =>
        pr => new ProductoDiagnosticoRow
        {
            IdProd = pr.IdProd,
            Codigo = pr.Codigo,
            Nombre = pr.Nombre,
            IdCategoria = pr.IdCategoria,
            Categoria = pr.Categoria.Nombre,
            Marca = pr.Marca != null ? pr.Marca.Nombre : null,
            Activo = pr.Activo,
            VisibleStore = pr.VisibleStore,
            TieneMarca = pr.IdMarca != null,
            TieneImagen = imagenes.Any(i => i.IdProd == pr.IdProd && i.Activo),
            Vendible = pr.Variantes.Any(v => v.Activo && v.Presentaciones.Any(pp => pp.Activo && pp.EsVenta)),
            TienePrecio = pr.Variantes.Any(v => v.Activo && v.Presentaciones.Any(pp => pp.Activo && pp.EsVenta
                && pp.Precios.Any(pc => pc.Activo && pc.FecVig <= hoy))),
            Variantes = pr.Variantes.Count(v => v.Activo),
        };

    // S92 K3 — estado en tienda (derivado; sin columna en BD).
    public static string EstadoTienda(bool activo, bool visibleStore, bool vendible, bool tienePrecio) =>
        visibleStore
            ? (activo && vendible && tienePrecio ? EstadosTienda.Publicado : EstadosTienda.PublicadoConAdvertencia)
            : !activo ? EstadosTienda.Inactivo
            : vendible ? EstadosTienda.Listo
            : EstadosTienda.Pendiente;

    public static IReadOnlyList<string> RazonesTienda(ProductoDiagnosticoRow d)
    {
        var r = new List<string>();
        if (!d.Activo) r.Add("Producto inactivo");
        if (!d.Vendible) r.Add("Sin variante activa a la venta");
        if (d.Vendible && !d.TienePrecio) r.Add("Sin precio vigente: la tienda muestra «precio a consultar»");
        if (!d.TieneImagen) r.Add("Sin imagen");
        if (!d.TieneMarca) r.Add("Sin marca");
        return r;
    }

    // R1 — filas candidatas a precio vigente.
    public static Expression<Func<Precio, bool>> PrecioCandidato(DateOnly hoy) =>
        pc => pc.Activo && pc.FecVig <= hoy;

    // R1 — desempate: mayor FecVig, luego mayor IdPrecio.
    public static Expression<Func<Precio, DateOnly>> PrecioOrdenFecVig =>
        pc => pc.FecVig;
    public static Expression<Func<Precio, long>> PrecioOrdenId =>
        pc => pc.IdPrecio;

    // R2 — oferta vigente (D1: Publicado = 1 obligatorio).
    public static Expression<Func<Oferta, bool>> OfertaVigente(DateOnly hoy) =>
        o => o.Activo && o.Publicado
            && o.FechaDesde <= hoy && (o.FechaHasta == null || o.FechaHasta >= hoy);

    // R2' (S77) — oferta EFECTIVA en memoria: la referencia congelada
    // (o.IdPrecioReferencia) sigue siendo el precio vigente de la presentación
    // y la oferta mejora el valor de la referencia. El SP
    // dbo.sp_Store_CatalogoCards la replica (script Inventa_S77_001_*).

    // R2 — desempate sin convención de Prioridad evidenciada (Gate 0 §5):
    // menor PrecioOferta, luego mayor IdOferta.
    public static Expression<Func<Oferta, decimal>> OfertaOrdenPrecio =>
        o => o.PrecioOferta;
    public static Expression<Func<Oferta, long>> OfertaOrdenId =>
        o => o.IdOferta;

    public static bool OfertaEfectiva(Oferta o, long? idPrecioVigente, decimal? valorReferencia) =>
        idPrecioVigente.HasValue && o.IdPrecioReferencia == idPrecioVigente.Value
        && valorReferencia.HasValue && o.PrecioOferta < valorReferencia.Value;

    // R3 — precio final (oferta efectiva solo si < vigente).
    public static decimal PrecioFinal(decimal vigente, decimal? oferta) =>
        oferta.HasValue && oferta.Value < vigente ? oferta.Value : vigente;

    // R4 — % entero 1..99, away-from-zero; sin oferta efectiva => null.
    // No usa Oferta.PctReferencial (columna informativa).
    public static int? PctDescuento(decimal vigente, decimal? oferta)
    {
        if (vigente <= 0 || !oferta.HasValue || oferta.Value >= vigente) return null;
        var pct = Math.Round((vigente - oferta.Value) / vigente * 100, 0, MidpointRounding.AwayFromZero);
        return (int)Math.Clamp(pct, 1, 99);
    }

    // S93 (D1/D2) — estado de una oferta respecto del precio oficial vigente.
    // Función pura: la única que clasifica ofertas en todo el sistema.
    // esLaEfectiva: la oferta es la que devuelve SeleccionarOfertaEfectiva (S93.1:
    // primero se filtran las efectivas R2+R2' y luego se desempata, como
    // dbo.sp_Store_CatalogoCards v4). Una elegible que no gana es Superpuesta.
    public static (EstadoOferta Estado, MotivoSinEfecto Motivo) CalcularEstado(
        Oferta o, long? idPrecioVigente, decimal? valorReferencia, DateOnly hoy, bool esLaEfectiva)
    {
        if (!o.Activo) return (EstadoOferta.Terminada, MotivoSinEfecto.Ninguno);
        if (o.FechaHasta.HasValue && o.FechaHasta.Value < hoy) return (EstadoOferta.Vencida, MotivoSinEfecto.Ninguno);
        if (o.FechaDesde > hoy) return (EstadoOferta.Programada, MotivoSinEfecto.Ninguno);
        if (!o.Publicado) return (EstadoOferta.SinEfecto, MotivoSinEfecto.NoPublicada);
        if (idPrecioVigente != o.IdPrecioReferencia)
            return (EstadoOferta.SinEfecto, MotivoSinEfecto.ReferenciaDesactualizada);
        if (!OfertaEfectiva(o, idPrecioVigente, valorReferencia))
            return (EstadoOferta.SinEfecto, MotivoSinEfecto.NoMejoraPrecio);
        if (!esLaEfectiva) return (EstadoOferta.SinEfecto, MotivoSinEfecto.Superpuesta);
        return (EstadoOferta.Efectiva, MotivoSinEfecto.Ninguno);
    }

    // S93.1 — R1 en memoria: precio oficial vigente a `hoy` (Activo, FecVig <= hoy,
    // desempate FecVig DESC, IdPrecio DESC). Igual que PrecioCandidato + orden.
    public static Precio? PrecioVigente(IEnumerable<Precio> precios, DateOnly hoy) =>
        precios.Where(p => p.Activo && p.FecVig <= hoy)
            .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio)
            .FirstOrDefault();

    // S93.1 — R2 + R2' + desempate en memoria, idéntico a sp_Store_CatalogoCards v4:
    // se filtran las ofertas vigentes y publicadas cuya referencia es el precio
    // vigente y que lo mejoran; gana la de menor PrecioOferta y luego mayor IdOferta.
    public static Oferta? SeleccionarOfertaEfectiva(IEnumerable<Oferta> ofertas, Precio? vigente, DateOnly hoy)
    {
        if (vigente is null) return null;
        return ofertas
            .Where(o => o.Activo && o.Publicado && o.FechaDesde <= hoy && (o.FechaHasta == null || o.FechaHasta >= hoy)
                && OfertaEfectiva(o, vigente.IdPrecio, vigente.Valor))
            .OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta)
            .FirstOrDefault();
    }

    // S93 — predicado compilable de OfertaVigente (R2) para uso en memoria.
    public static Func<Oferta, bool> OfertaVigenteEnMemoria(DateOnly hoy)
    {
        var expr = OfertaVigente(hoy);
        return expr.Compile();
    }
}

/// <summary>S93 (D1/D2): estado calculado de una oferta. No se persiste.</summary>
public enum EstadoOferta
{
    Efectiva = 0,
    SinEfecto = 1,
    Programada = 2,
    Vencida = 3,
    Terminada = 4
}

/// <summary>S93: motivo por el que una oferta vigente no tiene efecto.</summary>
public enum MotivoSinEfecto
{
    Ninguno = 0,
    ReferenciaDesactualizada = 1,
    NoMejoraPrecio = 2,
    NoPublicada = 3,
    Superpuesta = 4
}

/// <summary>S92 K3: códigos del estado en tienda (derivado de las reglas públicas).</summary>
public static class EstadosTienda
{
    public const string Publicado = "Publicado";
    public const string PublicadoConAdvertencia = "PublicadoConAdvertencia";
    public const string Listo = "Listo";
    public const string Pendiente = "Pendiente";
    public const string Inactivo = "Inactivo";
}

/// <summary>S92 K3: fila de diagnóstico (clase con setters para que EF pueda filtrar sobre la proyección).</summary>
public sealed class ProductoDiagnosticoRow
{
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public long IdCategoria { get; set; }
    public string Categoria { get; set; } = "";
    public string? Marca { get; set; }
    public bool Activo { get; set; }
    public bool VisibleStore { get; set; }
    public bool TieneMarca { get; set; }
    public bool TieneImagen { get; set; }
    public bool Vendible { get; set; }
    public bool TienePrecio { get; set; }
    public int Variantes { get; set; }
}
