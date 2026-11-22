namespace Inventa.Web.Models.Precios;

// S93.1: contratos de «Precios y ofertas» (espejo de Inventa.Application.Async; enums con los mismos valores).

public enum EstadoFiltroPrecios { Todos = 0, SinPrecio, ConOferta, SinOferta, OfertaSinEfecto, VenceEn7Dias, Programado }
public enum TipoCambioPrecio { PrecioOficial = 0, Oferta = 1, TerminarOferta = 2, AnularProgramado = 3 }
public enum RedondeoPrecioModo { Ninguno = 0, A10 = 1, A100 = 2, Termina990 = 3 }
public enum OpcionOferta { Mantener = 0, Terminar = 1 }

public sealed record PrecioVigenciaDto(long IdPrecio, decimal Valor, DateOnly Desde);
public sealed record OfertaFilaDto(long IdOferta, decimal Precio, int? Pct, DateOnly Desde, DateOnly? Hasta);

public sealed record FilaPrecioDto(
    long IdPresProd, long IdProd, string CodigoProducto, string CodigoVariante, string Producto, string Variante,
    string Categoria, string? Marca, string? CodigoBarras, bool Vendible, bool Publicable,
    PrecioVigenciaDto? Oficial, PrecioVigenciaDto? Programado, OfertaFilaDto? Oferta, OfertaFilaDto? OfertaProgramada,
    int OfertasSinEfecto, decimal? PrecioEfectivo, decimal CostoProm, decimal? MargenPct)
{
    public string Nombre => string.IsNullOrWhiteSpace(Variante) ? Producto : $"{Producto} / {Variante}";
}

public sealed record PaginaPreciosDto(IReadOnlyList<FilaPrecioDto> Items, int Total, int Page, int PageSize, string Moneda, int Decimales, DateOnly Hoy);

public sealed record ResumenPreciosDto(
    string Moneda, int Decimales, int Vendibles, int SinPrecio, int ConOferta, int VencenEn7Dias, int OfertasSinEfecto, int Programados, DateOnly Hoy);

public sealed record HistorialPrecioDto(long IdPrecio, decimal Valor, DateOnly Desde, string Estado);
public sealed record HistorialOfertaDto(
    long IdOferta, decimal Precio, decimal? PctReferencial, DateOnly Desde, DateOnly? Hasta,
    long IdPrecioReferencia, decimal? ValorReferencia, bool Publicado, string Estado, string? Motivo);
public sealed record HistorialPreciosDto(FilaPrecioDto Fila, IReadOnlyList<HistorialPrecioDto> Precios, IReadOnlyList<HistorialOfertaDto> Ofertas);

/// <summary>Un cambio del lote (ver CambioPrecio en Application.Async). Desde = null significa «desde hoy».</summary>
public sealed record CambioPrecioDto(
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

public sealed record CambiosPreciosRequest(IReadOnlyList<CambioPrecioDto> Cambios);

public sealed record ErrorCambioPrecioDto(int Indice, long IdPresProd, string Codigo, string Mensaje);
public sealed record OmitidoCambioPrecioDto(int Indice, long IdPresProd, string Codigo, string Mensaje);
public sealed record CambioPresentacionResumenDto(
    long IdPresProd, string Codigo, string Nombre,
    decimal? OficialAntes, decimal? OficialDespues, decimal? OfertaAntes, decimal? OfertaDespues,
    decimal? EfectivoAntes, decimal? EfectivoDespues,
    IReadOnlyList<string> Acciones, IReadOnlyList<string> Avisos, bool AfectaOferta);

public sealed record ResultadoCambiosPreciosDto(
    bool Aplicado,
    IReadOnlyList<ErrorCambioPrecioDto> Errores,
    IReadOnlyList<long> Desactualizados,
    IReadOnlyList<OmitidoCambioPrecioDto> Omitidos,
    IReadOnlyList<CambioPresentacionResumenDto> Presentaciones,
    IReadOnlyList<FilaPrecioDto> Filas,
    IReadOnlyList<long> PreciosCreados);

/// <summary>Respuesta de simular/aplicar ya interpretada: 200 (Resultado), 422 (Errores) o 409 (Desactualizados).</summary>
public sealed record RespuestaCambiosPrecios(
    ResultadoCambiosPreciosDto? Resultado,
    IReadOnlyList<ErrorCambioPrecioDto> Errores,
    IReadOnlyList<long> Desactualizados,
    string? Mensaje)
{
    public bool Ok => Resultado is not null && Errores.Count == 0 && Desactualizados.Count == 0;
    public bool Conflicto => Desactualizados.Count > 0;
}
