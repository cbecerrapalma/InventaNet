using CBP.Blazor.Formatting;
using Inventa.Web.Models.Precios;

namespace Inventa.Web.Shared.Price;

/// <summary>
/// S93.1: cambios pendientes de una variante en «Precios y ofertas». A lo más uno por tipo
/// (anular programado, precio oficial, terminar oferta, nueva oferta), igual que exige el servidor.
/// </summary>
public sealed class PendientesPrecio(FilaPrecioDto fila)
{
    public FilaPrecioDto Fila { get; set; } = fila;
    public CambioPrecioDto? Anular { get; set; }
    public CambioPrecioDto? Precio { get; set; }
    public CambioPrecioDto? Terminar { get; set; }
    public CambioPrecioDto? Oferta { get; set; }

    public IEnumerable<CambioPrecioDto> Cambios =>
        new[] { Anular, Precio, Terminar, Oferta }.Where(c => c is not null).Select(c => c!);

    public int Cantidad => Cambios.Count();

    public void Aplicar(IEnumerable<CambioPrecioDto> cambios)
    {
        foreach (var c in cambios)
        {
            switch (c.Tipo)
            {
                case TipoCambioPrecio.AnularProgramado: Anular = c; break;
                case TipoCambioPrecio.PrecioOficial: Precio = c; break;
                case TipoCambioPrecio.TerminarOferta: Terminar = c; Oferta = null; break;
                case TipoCambioPrecio.Oferta: Oferta = c; Terminar = null; break;
            }
        }
    }
}

/// <summary>S93.1: armado de cambios y textos de la página. Funciones puras (probadas con bUnit/xUnit).</summary>
public static class PreciosUi
{
    public static CambioPrecioDto NuevoPrecio(FilaPrecioDto f, decimal valor, DateOnly? desde = null) =>
        new(f.IdPresProd, TipoCambioPrecio.PrecioOficial, Valor: valor, Desde: desde, IdPrecioVigenteVisto: f.Oficial?.IdPrecio);

    public static CambioPrecioDto AjustePrecio(FilaPrecioDto f, decimal pct, RedondeoPrecioModo redondeo, DateOnly? desde = null) =>
        new(f.IdPresProd, TipoCambioPrecio.PrecioOficial, PctAjuste: pct, Redondeo: redondeo, Desde: desde, IdPrecioVigenteVisto: f.Oficial?.IdPrecio);

    public static CambioPrecioDto NuevaOferta(FilaPrecioDto f, decimal? precio, decimal? pct, DateOnly? desde, DateOnly? hasta) =>
        new(f.IdPresProd, TipoCambioPrecio.Oferta, PrecioOferta: precio, PctDescuento: precio is null ? pct : null,
            Desde: desde, Hasta: hasta, IdPrecioVigenteVisto: f.Oficial?.IdPrecio, IdOfertaVista: f.Oferta?.IdOferta);

    public static CambioPrecioDto TerminarOferta(FilaPrecioDto f) =>
        new(f.IdPresProd, TipoCambioPrecio.TerminarOferta, IdOfertaVista: f.Oferta!.IdOferta);

    public static CambioPrecioDto AnularProgramado(FilaPrecioDto f) =>
        new(f.IdPresProd, TipoCambioPrecio.AnularProgramado, IdPrecio: f.Programado!.IdPrecio);

    /// <summary>Precio de oferta a partir del % sobre la referencia (mismo redondeo que el servidor).</summary>
    public static decimal PrecioDesdePct(decimal referencia, decimal pct, int decimales) =>
        Math.Round(referencia * (1 - pct / 100m), decimales, MidpointRounding.AwayFromZero);

    /// <summary>% de descuento (entero, como R4) de un precio de oferta sobre la referencia.</summary>
    public static int? PctDesdePrecio(decimal referencia, decimal precio)
    {
        if (referencia <= 0 || precio >= referencia || precio <= 0) return null;
        return (int)Math.Clamp(Math.Round((referencia - precio) / referencia * 100m, 0, MidpointRounding.AwayFromZero), 1, 99);
    }

    public static bool MontoValido(decimal? v, int decimales) => v is > 0 && v.Value == Math.Round(v.Value, decimales);

    public static DateOnly FinDeMes(DateOnly d) => new(d.Year, d.Month, DateTime.DaysInMonth(d.Year, d.Month));

    public static string TextoPrecioPendiente(CambioPrecioDto c) =>
        c.Valor is decimal v ? FormatoCl.Moneda(v)
        : c.PctAjuste is decimal p ? $"{(p > 0 ? "+" : "")}{FormatoCl.Porcentaje(p, 2)}{TextoRedondeo(c.Redondeo)}"
        : "—";

    public static string TextoOfertaPendiente(CambioPrecioDto c) =>
        c.Tipo == TipoCambioPrecio.TerminarOferta ? "Se termina"
        : (c.PrecioOferta is decimal v ? FormatoCl.Moneda(v) : $"−{FormatoCl.Porcentaje(c.PctDescuento, 2)}")
          + (c.Hasta is DateOnly h ? $" hasta {FormatoCl.Fecha(h)}" : "");

    public static string TextoRedondeo(RedondeoPrecioModo r) => r switch
    {
        RedondeoPrecioModo.A10 => " (a $10)",
        RedondeoPrecioModo.A100 => " (a $100)",
        RedondeoPrecioModo.Termina990 => " (termina en 990)",
        _ => ""
    };

    public static string TextoOferta(OfertaFilaDto o) =>
        $"{FormatoCl.Moneda(o.Precio)}{(o.Pct is int p ? $" −{FormatoCl.Porcentaje(p)}" : "")}{(o.Hasta is DateOnly h ? $" · hasta {FormatoCl.Fecha(h)}" : "")}";

    public static string TextoEstado(string estado) => estado switch
    {
        "Vigente" => "Vigente",
        "Historico" => "Histórico",
        "Programado" => "Programado",
        "Anulado" => "Anulado",
        "Efectiva" => "Efectiva",
        "SinEfecto" => "Sin efecto",
        "Programada" => "Programada",
        "Vencida" => "Vencida",
        "Terminada" => "Terminada",
        _ => estado
    };

    public static string TextoMotivo(string? motivo) => motivo switch
    {
        "ReferenciaDesactualizada" => "el precio oficial cambió después de crearla",
        "NoMejoraPrecio" => "no es menor que el precio oficial",
        "NoPublicada" => "no está publicada",
        "Superpuesta" => "hay otra oferta vigente más baja",
        _ => ""
    };
}
