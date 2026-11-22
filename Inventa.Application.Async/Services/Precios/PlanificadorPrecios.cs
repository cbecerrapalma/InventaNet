using System.Globalization;
using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services.Precios;

/// <summary>S93.1 (K3): tipos de cambio que acepta la página «Precios y ofertas».</summary>
public enum TipoCambioPrecio
{
    PrecioOficial = 0,
    Oferta = 1,
    TerminarOferta = 2,
    AnularProgramado = 3
}

/// <summary>S93.1 (D3): qué hacer con la oferta cuando cambia el precio oficial al que apunta.</summary>
public enum OpcionOferta
{
    Mantener = 0,
    Terminar = 1
}

/// <summary>
/// Un cambio del lote. Campos por tipo:
/// PrecioOficial: Valor o PctAjuste (+Redondeo), Desde, IdPrecioVigenteVisto, ConOferta.
/// Oferta: PrecioOferta o PctDescuento, Desde, Hasta, IdPrecioVigenteVisto, IdOfertaVista.
/// TerminarOferta: IdOfertaVista. AnularProgramado: IdPrecio.
/// VerificarVistos = false solo para los endpoints antiguos (sin control de concurrencia).
/// </summary>
public sealed record CambioPrecio(
    long IdPresProd,
    TipoCambioPrecio Tipo,
    decimal? Valor = null,
    decimal? PctAjuste = null,
    RedondeoPrecioModo Redondeo = RedondeoPrecioModo.Ninguno,
    DateOnly? Desde = null,
    DateOnly? Hasta = null,
    long? IdPrecioVigenteVisto = null,
    OpcionOferta? ConOferta = null,
    decimal? PrecioOferta = null,
    decimal? PctDescuento = null,
    long? IdOfertaVista = null,
    long? IdPrecio = null,
    bool VerificarVistos = true);

public sealed record ErrorCambioPrecio(int Indice, long IdPresProd, string Codigo, string Mensaje);
public sealed record OmitidoCambioPrecio(int Indice, long IdPresProd, string Codigo, string Mensaje);

/// <summary>Antes → después de una presentación (lo que paga el cliente hoy) y el detalle de lo que se hará.</summary>
public sealed record CambioPresentacionResumen(
    long IdPresProd, string Codigo, string Nombre,
    decimal? OficialAntes, decimal? OficialDespues,
    decimal? OfertaAntes, decimal? OfertaDespues,
    decimal? EfectivoAntes, decimal? EfectivoDespues,
    IReadOnlyList<string> Acciones, IReadOnlyList<string> Avisos,
    bool AfectaOferta = false);

public sealed record PlanCambiosPrecios(
    IReadOnlyList<ErrorCambioPrecio> Errores,
    IReadOnlyList<long> Desactualizados,
    IReadOnlyList<OmitidoCambioPrecio> Omitidos,
    IReadOnlyList<CambioPresentacionResumen> Presentaciones,
    IReadOnlyList<OperacionPrecio> Operaciones)
{
    public bool HayConflicto => Desactualizados.Count > 0;
    public bool Valido => Errores.Count == 0 && !HayConflicto;
}

/// <summary>Códigos de error y omisión de S93 K3 (mensajes en español de Chile).</summary>
public static class CodigosPrecio
{
    public const string PresentacionNoEncontrada = "PRESENTACION_NO_ENCONTRADA";
    public const string PrecioInvalido = "PRECIO_INVALIDO";
    public const string FechaPasada = "FECHA_PASADA";
    public const string SinPrecioOficial = "SIN_PRECIO_OFICIAL";
    public const string OfertaSobrePrecio = "OFERTA_SOBRE_PRECIO";
    public const string PctInvalido = "PCT_INVALIDO";
    public const string OfertaRangoInvalido = "OFERTA_RANGO_INVALIDO";
    public const string OfertaQuedariaSobrePrecio = "OFERTA_QUEDARIA_SOBRE_PRECIO";
    public const string ProgramadoNoAnulable = "PROGRAMADO_NO_ANULABLE";
    public const string LoteDemasiadoGrande = "LOTE_DEMASIADO_GRANDE";
    public const string CambioDuplicado = "CAMBIO_DUPLICADO";
    public const string PrecioDesactualizado = "PRECIO_DESACTUALIZADO";
    public const string SinCambio = "SIN_CAMBIO";
    public const int MaxCambios = 500;
}

/// <summary>
/// S93.1 (K3): planificador puro de cambios de precio. No accede a la BD: recibe las instantáneas y devuelve
/// el plan (errores, conflictos, omitidos, resumen y operaciones). Reglas: D2 (una oferta efectiva; la nueva
/// reemplaza a las que se solapan), D3 (mantener/terminar la oferta al cambiar el precio), D4 (sin fechas
/// pasadas; solo INSERT de vigencias; anular solo lo programado) y D7 (decimales por moneda). La oferta
/// efectiva se calcula igual que la tienda (<see cref="PublicCatalogSpecs.SeleccionarOfertaEfectiva"/>).
/// </summary>
public static class PlanificadorPrecios
{
    // Formato es-CL sin depender de ICU (igual criterio que CBP.Blazor FormatoCl): $12.990.
    private static readonly NumberFormatInfo Cl = new() { NumberGroupSeparator = ".", NumberDecimalSeparator = "," };

    private sealed class PrecioW
    {
        public long? Id;
        public int? Clave;
        public decimal Valor;
        public DateOnly FecVig;
        public bool Activo;
        public bool ActivoOriginal;
        // Orden de desempate R1: los nuevos son más recientes que cualquier existente.
        public long Orden => Id ?? (long.MaxValue / 2 + (Clave ?? 0));
    }

    private sealed class OfertaW
    {
        public long? Id;
        public long? RefId;
        public int? RefClave;
        public DateOnly Desde;
        public DateOnly? Hasta;
        public DateOnly? HastaOriginal;
        public decimal Precio;
        public decimal PctReferencial;
        public bool Activo;
        public bool ActivoOriginal;
        public bool Publicado;
        public long Orden => Id ?? long.MaxValue / 2;
    }

    public static PlanCambiosPrecios Planificar(
        IReadOnlyList<CambioPrecio> cambios,
        IReadOnlyDictionary<long, SnapshotPrecios> snapshots,
        int idTenant,
        DateOnly hoy,
        int decimales)
    {
        ArgumentNullException.ThrowIfNull(cambios);
        ArgumentNullException.ThrowIfNull(snapshots);
        var errores = new List<ErrorCambioPrecio>();
        var desactualizados = new SortedSet<long>();
        var omitidos = new List<OmitidoCambioPrecio>();
        var resumenes = new List<CambioPresentacionResumen>();
        var ops = new List<OperacionPrecio>();
        var clave = 0;

        var indexados = cambios.Select((c, i) => (Cambio: c, Indice: i)).ToList();
        foreach (var grupo in indexados.GroupBy(x => x.Cambio.IdPresProd))
        {
            var idPres = grupo.Key;
            if (!snapshots.TryGetValue(idPres, out var snap) || snap.IdTenant != idTenant)
            {
                foreach (var x in grupo)
                    errores.Add(new(x.Indice, idPres, CodigosPrecio.PresentacionNoEncontrada, "La presentación no existe en este tenant."));
                continue;
            }

            // Duplicados: un PrecioOficial y una Oferta por presentación; TerminarOferta/AnularProgramado por id distinto.
            foreach (var dup in grupo.GroupBy(x => (x.Cambio.Tipo,
                         Id: x.Cambio.Tipo switch
                         {
                             TipoCambioPrecio.TerminarOferta => x.Cambio.IdOfertaVista ?? 0,
                             TipoCambioPrecio.AnularProgramado => x.Cambio.IdPrecio ?? 0,
                             _ => 0
                         })).Where(g => g.Count() > 1))
                foreach (var x in dup.Skip(1))
                    errores.Add(new(x.Indice, idPres, CodigosPrecio.CambioDuplicado, "Hay más de un cambio del mismo tipo para esta variante."));

            var precios = snap.Precios.Select(p => new PrecioW { Id = p.IdPrecio, Valor = p.Valor, FecVig = p.FecVig, Activo = p.Activo, ActivoOriginal = p.Activo }).ToList();
            var ofertas = snap.OfertasActivas.Where(o => o.Activo).Select(o => new OfertaW
            {
                Id = o.IdOferta, RefId = o.IdPrecioReferencia, Desde = o.FechaDesde, Hasta = o.FechaHasta, HastaOriginal = o.FechaHasta,
                Precio = o.PrecioOferta, PctReferencial = o.PctReferencial ?? 0, Activo = true, ActivoOriginal = true, Publicado = o.Publicado
            }).ToList();

            // Estado visto por el usuario (antes de cualquier cambio): tokens de concurrencia.
            var vigAntes = PublicCatalogSpecs.PrecioVigente(snap.Precios, hoy);
            var ofAntes = PublicCatalogSpecs.SeleccionarOfertaEfectiva(snap.OfertasActivas, vigAntes, hoy);
            var acciones = new List<string>();
            var avisos = new List<string>();
            var huboCambio = false;
            var afectaOferta = false;

            foreach (var x in grupo.OrderBy(x => Prioridad(x.Cambio.Tipo)).ThenBy(x => x.Indice))
            {
                var c = x.Cambio;
                void Error(string codigo, string msg) => errores.Add(new(x.Indice, idPres, codigo, msg));

                if (c.VerificarVistos && c.Tipo is TipoCambioPrecio.PrecioOficial or TipoCambioPrecio.Oferta)
                {
                    if (c.IdPrecioVigenteVisto != vigAntes?.IdPrecio) { desactualizados.Add(idPres); continue; }
                    if (c.Tipo == TipoCambioPrecio.Oferta && c.IdOfertaVista != ofAntes?.IdOferta) { desactualizados.Add(idPres); continue; }
                }

                switch (c.Tipo)
                {
                    case TipoCambioPrecio.AnularProgramado:
                    {
                        var p = precios.FirstOrDefault(p => p.Id == c.IdPrecio);
                        if (p is null || !p.Activo) { desactualizados.Add(idPres); break; }
                        if (p.FecVig <= hoy) { Error(CodigosPrecio.ProgramadoNoAnulable, "Ese precio ya rige: no se puede anular, cree una nueva vigencia."); break; }
                        if (ofertas.Any(o => o.Activo && o.RefId == p.Id)) { Error(CodigosPrecio.ProgramadoNoAnulable, "Ese precio programado es referencia de una oferta."); break; }
                        p.Activo = false;
                        huboCambio = true;
                        acciones.Add($"Anula el precio programado de {Moneda(p.Valor)} del {Fecha(p.FecVig)}.");
                        break;
                    }
                    case TipoCambioPrecio.PrecioOficial:
                    {
                        var desde = c.Desde ?? hoy;
                        if (desde < hoy) { Error(CodigosPrecio.FechaPasada, "La vigencia debe comenzar hoy o en una fecha futura."); break; }
                        var baseP = PrecioEn(precios, desde);
                        decimal valor;
                        if (c.Valor is decimal v)
                        {
                            if (v <= 0 || !ReglasMoneda.RespetaDecimales(v, decimales))
                            { Error(CodigosPrecio.PrecioInvalido, decimales == 0 ? "El precio debe ser un monto entero mayor que 0." : "El precio debe ser mayor que 0."); break; }
                            valor = v;
                        }
                        else if (c.PctAjuste is decimal pct)
                        {
                            if (baseP is null) { Error(CodigosPrecio.SinPrecioOficial, "La variante no tiene precio oficial para ajustar."); break; }
                            if (pct < -90 || pct > 500 || pct == 0) { Error(CodigosPrecio.PctInvalido, "El ajuste debe estar entre −90 % y +500 % y ser distinto de 0."); break; }
                            valor = RedondeoPrecio.Aplicar(ReglasMoneda.Redondear(baseP.Valor * (1 + pct / 100m), decimales), c.Redondeo);
                            if (valor <= 0) { Error(CodigosPrecio.PrecioInvalido, "El precio resultante no es válido."); break; }
                        }
                        else { Error(CodigosPrecio.PrecioInvalido, "Indique el precio o el porcentaje de ajuste."); break; }

                        if (baseP is not null && baseP.Valor == valor)
                        {
                            omitidos.Add(new(x.Indice, idPres, CodigosPrecio.SinCambio, "El precio es igual al que ya rige en esa fecha."));
                            break;
                        }
                        var nuevo = new PrecioW { Clave = ++clave, Valor = valor, FecVig = desde, Activo = true, ActivoOriginal = false };
                        precios.Add(nuevo);
                        huboCambio = true;
                        acciones.Add(desde == hoy
                            ? $"Precio oficial {Moneda(valor)} desde hoy."
                            : $"Precio oficial {Moneda(valor)} programado para el {Fecha(desde)}.");

                        // D3: ofertas que apuntan al precio que deja de regir y siguen activas desde `desde`.
                        if (baseP?.Id is long idBase)
                        {
                            foreach (var o in ofertas.Where(o => o.Activo && o.RefId == idBase && (o.Hasta == null || o.Hasta >= desde)).ToList())
                            {
                                afectaOferta = true;
                                var opcion = c.ConOferta ?? (o.Precio < valor ? OpcionOferta.Mantener : OpcionOferta.Terminar);
                                if (opcion == OpcionOferta.Mantener && o.Precio >= valor)
                                {
                                    Error(CodigosPrecio.OfertaQuedariaSobrePrecio, $"La oferta de {Moneda(o.Precio)} no es menor que el nuevo precio: termínela.");
                                    continue;
                                }
                                var hastaOriginal = o.Hasta;
                                Cortar(o, desde);
                                if (opcion == OpcionOferta.Mantener)
                                {
                                    var desdeNueva = o.Desde > desde ? o.Desde : desde;
                                    ofertas.Add(new OfertaW
                                    {
                                        RefClave = nuevo.Clave, Desde = desdeNueva, Hasta = hastaOriginal, Precio = o.Precio,
                                        PctReferencial = PctReferencial(valor, o.Precio), Activo = true, Publicado = true
                                    });
                                    avisos.Add($"La oferta de {Moneda(o.Precio)} se mantiene con el nuevo precio desde el {Fecha(desdeNueva)}.");
                                }
                                else
                                {
                                    avisos.Add($"La oferta de {Moneda(o.Precio)} termina: dejaría de tener efecto con el nuevo precio.");
                                }
                            }
                        }
                        break;
                    }
                    case TipoCambioPrecio.TerminarOferta:
                    {
                        var o = ofertas.FirstOrDefault(o => o.Id == c.IdOfertaVista && o.Activo && (o.Hasta == null || o.Hasta >= hoy));
                        if (o is null) { desactualizados.Add(idPres); break; }
                        Cortar(o, hoy);
                        huboCambio = true;
                        acciones.Add($"Termina la oferta de {Moneda(o.Precio)}.");
                        break;
                    }
                    case TipoCambioPrecio.Oferta:
                    {
                        var desde = c.Desde ?? hoy;
                        if (desde < hoy) { Error(CodigosPrecio.FechaPasada, "La oferta debe comenzar hoy o en una fecha futura."); break; }
                        if (c.Hasta is DateOnly h && h < desde) { Error(CodigosPrecio.OfertaRangoInvalido, "La fecha de término no puede ser anterior al inicio."); break; }
                        var refP = PrecioEn(precios, desde);
                        if (refP is null) { Error(CodigosPrecio.SinPrecioOficial, "La variante no tiene precio oficial en esa fecha: la oferta necesita una referencia."); break; }
                        decimal precio;
                        if (c.PrecioOferta is decimal po)
                        {
                            if (po <= 0 || !ReglasMoneda.RespetaDecimales(po, decimales))
                            { Error(CodigosPrecio.PrecioInvalido, decimales == 0 ? "El precio de oferta debe ser un monto entero mayor que 0." : "El precio de oferta debe ser mayor que 0."); break; }
                            precio = po;
                        }
                        else if (c.PctDescuento is decimal pd)
                        {
                            if (pd < 1 || pd > 99) { Error(CodigosPrecio.PctInvalido, "El descuento debe estar entre 1 % y 99 %."); break; }
                            precio = ReglasMoneda.Redondear(refP.Valor * (1 - pd / 100m), decimales);
                            if (precio <= 0) { Error(CodigosPrecio.PrecioInvalido, "El precio de oferta resultante no es válido."); break; }
                        }
                        else { Error(CodigosPrecio.PrecioInvalido, "Indique el precio de oferta o el porcentaje de descuento."); break; }
                        if (precio >= refP.Valor) { Error(CodigosPrecio.OfertaSobrePrecio, $"La oferta debe ser menor que el precio oficial ({Moneda(refP.Valor)})."); break; }

                        // D2: la nueva reemplaza a las que se solapan con su periodo.
                        var finNueva = c.Hasta ?? DateOnly.MaxValue;
                        foreach (var o in ofertas.Where(o => o.Activo && o.Desde <= finNueva && (o.Hasta ?? DateOnly.MaxValue) >= desde).ToList())
                        {
                            Cortar(o, desde);
                            avisos.Add($"Reemplaza la oferta de {Moneda(o.Precio)}.");
                        }
                        ofertas.Add(new OfertaW
                        {
                            RefId = refP.Id, RefClave = refP.Id is null ? refP.Clave : null, Desde = desde, Hasta = c.Hasta,
                            Precio = precio, PctReferencial = PctReferencial(refP.Valor, precio), Activo = true, Publicado = true
                        });
                        huboCambio = true;
                        var periodo = c.Hasta is DateOnly hh ? $" hasta el {Fecha(hh)}" : " sin fecha de término";
                        acciones.Add($"Oferta {Moneda(precio)} (−{PublicCatalogSpecs.PctDescuento(refP.Valor, precio)} %) desde {(desde == hoy ? "hoy" : "el " + Fecha(desde))}{periodo}.");

                        var proximo = precios.Where(p => p.Activo && p.FecVig > desde && p.FecVig <= finNueva)
                            .OrderBy(p => p.FecVig).FirstOrDefault();
                        if (proximo is not null)
                            avisos.Add($"El precio programado del {Fecha(proximo.FecVig)} dejará sin efecto esta oferta desde esa fecha.");
                        break;
                    }
                }
            }

            if (!huboCambio) continue;

            // Después (hoy), con la misma regla que la tienda.
            var vigDespues = PrecioEn(precios, hoy);
            var ofDespues = vigDespues is null ? null : ofertas
                .Where(o => o.Activo && o.Publicado && o.Desde <= hoy && (o.Hasta == null || o.Hasta >= hoy)
                    && (vigDespues.Id is long vid ? o.RefId == vid : o.RefClave == vigDespues.Clave)
                    && o.Precio < vigDespues.Valor)
                .OrderBy(o => o.Precio).ThenByDescending(o => o.Orden).FirstOrDefault();
            resumenes.Add(new CambioPresentacionResumen(
                idPres, snap.CodigoVariante, snap.Nombre,
                vigAntes?.Valor, vigDespues?.Valor,
                ofAntes?.PrecioOferta, ofDespues?.Precio,
                vigAntes is null ? null : ofAntes?.PrecioOferta ?? vigAntes.Valor,
                vigDespues is null ? null : ofDespues?.Precio ?? vigDespues.Valor,
                acciones, avisos, afectaOferta));

            // Operaciones (diferencias con la instantánea).
            foreach (var p in precios.Where(p => p.Id is null && p.Activo))
                ops.Add(new OpInsertarPrecio(idPres, p.Clave!.Value, p.Valor, p.FecVig));
            foreach (var p in precios.Where(p => p.Id is long && p.ActivoOriginal && !p.Activo))
                ops.Add(new OpAnularPrecio(idPres, p.Id!.Value));
            foreach (var o in ofertas.Where(o => o.Id is long))
            {
                if (o.ActivoOriginal && !o.Activo) ops.Add(new OpAnularOferta(idPres, o.Id!.Value));
                else if (o.Activo && o.Hasta != o.HastaOriginal) ops.Add(new OpCerrarOferta(idPres, o.Id!.Value, o.Hasta!.Value));
            }
            foreach (var o in ofertas.Where(o => o.Id is null && o.Activo))
                ops.Add(new OpInsertarOferta(idPres, o.RefId, o.RefClave, o.Desde, o.Hasta, o.Precio, o.PctReferencial));
        }

        // Todo o nada: con un error o un conflicto en cualquier fila no hay operaciones.
        var valido = errores.Count == 0 && desactualizados.Count == 0;
        return new PlanCambiosPrecios(errores.OrderBy(e => e.Indice).ToList(), desactualizados.ToList(), omitidos, resumenes,
            valido ? ops : []);
    }

    // S93.1: TerminarOferta va ANTES que PrecioOficial (desviación del orden de K3). Si no, la regla D3 del
    // precio «mantiene» (corta y recrea) la oferta que el usuario pidió terminar y el lote cae en un falso 409.
    private static int Prioridad(TipoCambioPrecio t) => t switch
    {
        TipoCambioPrecio.AnularProgramado => 0,
        TipoCambioPrecio.TerminarOferta => 1,
        TipoCambioPrecio.PrecioOficial => 2,
        _ => 3
    };

    // R1 sobre el estado de trabajo (incluye los precios nuevos del lote).
    private static PrecioW? PrecioEn(List<PrecioW> precios, DateOnly fecha) =>
        precios.Where(p => p.Activo && p.FecVig <= fecha)
            .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.Orden).FirstOrDefault();

    // Termina una oferta para que no rija desde `desde`: si ya empezó, FechaHasta = desde − 1; si no, se anula.
    // Una oferta nueva del lote que se corta antes de empezar simplemente no se inserta.
    private static void Cortar(OfertaW o, DateOnly desde)
    {
        var corte = desde.AddDays(-1);
        if (o.Desde <= corte)
        {
            if (o.Hasta is null || o.Hasta > corte) o.Hasta = corte;
        }
        else
        {
            o.Activo = false;
        }
    }

    private static decimal PctReferencial(decimal referencia, decimal oferta) =>
        referencia <= 0 ? 0 : Math.Round((referencia - oferta) / referencia * 100m, 2, MidpointRounding.AwayFromZero);

    private static string Moneda(decimal v) => "$" + Math.Round(v, 0, MidpointRounding.AwayFromZero).ToString("#,0", Cl);
    private static string Fecha(DateOnly d) => d.ToString("dd-MM-yyyy", CultureInfo.InvariantCulture);
}
