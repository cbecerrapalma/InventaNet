using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services;

/// <summary>S93.1 (K2): filtros de estado de la grilla (espejo de la capa de datos; los controladores no ven Data.Async).</summary>
public enum EstadoFiltroPrecios { Todos = 0, SinPrecio, ConOferta, SinOferta, OfertaSinEfecto, VenceEn7Dias, Programado }

public sealed record GrillaPreciosConsulta(
    string? Texto = null, long? IdCategoria = null, int? IdMarca = null,
    EstadoFiltroPrecios Estado = EstadoFiltroPrecios.Todos, bool IncluirInactivos = false, int Page = 1, int PageSize = 20);

public sealed record PrecioVigenciaDto(long IdPrecio, decimal Valor, DateOnly Desde);
public sealed record OfertaFilaDto(long IdOferta, decimal Precio, int? Pct, DateOnly Desde, DateOnly? Hasta);

/// <summary>Una fila de «Precios y ofertas»: precio oficial, oferta efectiva (regla de la tienda) y lo que paga el cliente.</summary>
public sealed record FilaPrecioDto(
    long IdPresProd, long IdProd, string CodigoProducto, string CodigoVariante, string Producto, string Variante,
    string Categoria, string? Marca, string? CodigoBarras, bool Vendible, bool Publicable,
    PrecioVigenciaDto? Oficial, PrecioVigenciaDto? Programado, OfertaFilaDto? Oferta, OfertaFilaDto? OfertaProgramada,
    int OfertasSinEfecto, decimal? PrecioEfectivo, decimal CostoProm, decimal? MargenPct);

public sealed record ResumenPreciosDto(
    string Moneda, int Decimales, int Vendibles, int SinPrecio, int ConOferta, int VencenEn7Dias, int OfertasSinEfecto, int Programados, DateOnly Hoy);

public sealed record HistorialPrecioDto(long IdPrecio, decimal Valor, DateOnly Desde, string Estado);
public sealed record HistorialOfertaDto(
    long IdOferta, decimal Precio, decimal? PctReferencial, DateOnly Desde, DateOnly? Hasta,
    long IdPrecioReferencia, decimal? ValorReferencia, bool Publicado, string Estado, string? Motivo);
public sealed record HistorialPreciosDto(FilaPrecioDto Fila, IReadOnlyList<HistorialPrecioDto> Precios, IReadOnlyList<HistorialOfertaDto> Ofertas);

public sealed record PaginaPreciosDto(IReadOnlyList<FilaPrecioDto> Items, int Total, int Page, int PageSize, string Moneda, int Decimales, DateOnly Hoy);

public interface IPrecioGrillaServiceAsync
{
    Task<Result<PaginaPreciosDto>> GetPaginaAsync(int idTenant, GrillaPreciosConsulta consulta, CancellationToken ct = default);
    Task<Result<ResumenPreciosDto>> GetResumenAsync(int idTenant, CancellationToken ct = default);
    Task<Result<HistorialPreciosDto?>> GetHistorialAsync(int idTenant, long idPresProd, CancellationToken ct = default);
    Task<IReadOnlyList<FilaPrecioDto>> GetFilasAsync(int idTenant, IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default);
    Task<(string Moneda, int Decimales)> GetMonedaAsync(int idTenant, CancellationToken ct = default);
}

/// <summary>
/// S93.1 (K2): lectura de la página «Precios y ofertas». Filtros, paginación y conteos en SQL
/// (<see cref="IPreciosGrillaRepositoryAsync"/>); aquí solo se arma el DTO (R3, R4, margen).
/// </summary>
public sealed class PrecioGrillaServiceAsync : IPrecioGrillaServiceAsync
{
    private readonly IPreciosGrillaRepositoryAsync _repo;
    private readonly IConfiguracionServiceAsync _cfg;

    public PrecioGrillaServiceAsync(IPreciosGrillaRepositoryAsync repo, IConfiguracionServiceAsync cfg)
    {
        _repo = repo;
        _cfg = cfg;
    }

    public async Task<Result<PaginaPreciosDto>> GetPaginaAsync(int idTenant, GrillaPreciosConsulta consulta, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(consulta);
        if (idTenant <= 0) return Result<PaginaPreciosDto>.Failure(Validation("IdTenant debe ser >0"));
        if (consulta.Page < 1 || consulta.PageSize is < 1 or > 100)
            return Result<PaginaPreciosDto>.Failure(Validation("page ≥ 1 y 1 ≤ pageSize ≤ 100"));
        var filtro = new PreciosGrillaFiltro(consulta.Texto, consulta.IdCategoria, consulta.IdMarca,
            (FiltroEstadoPrecio)(int)consulta.Estado, consulta.IncluirInactivos);
        var hoy = PublicCatalogSpecs.Hoy();
        var (items, total) = await _repo.GetPaginaAsync(idTenant, filtro, hoy, consulta.Page, consulta.PageSize, ct);
        var (moneda, dec) = await GetMonedaAsync(idTenant, ct);
        return Result<PaginaPreciosDto>.Success(new PaginaPreciosDto(items.Select(AFila).ToList(), total, consulta.Page, consulta.PageSize, moneda, dec, hoy));
    }

    public async Task<Result<ResumenPreciosDto>> GetResumenAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Result<ResumenPreciosDto>.Failure(Validation("IdTenant debe ser >0"));
        var hoy = PublicCatalogSpecs.Hoy();
        var r = await _repo.GetResumenAsync(idTenant, hoy, ct);
        var (moneda, dec) = await GetMonedaAsync(idTenant, ct);
        return Result<ResumenPreciosDto>.Success(new ResumenPreciosDto(moneda, dec, r.Vendibles, r.SinPrecio, r.ConOferta, r.VencenEn7Dias, r.OfertasSinEfecto, r.Programados, hoy));
    }

    public async Task<Result<HistorialPreciosDto?>> GetHistorialAsync(int idTenant, long idPresProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Result<HistorialPreciosDto?>.Failure(Validation("IdTenant debe ser >0"));
        var hoy = PublicCatalogSpecs.Hoy();
        var h = await _repo.GetHistorialAsync(idTenant, idPresProd, hoy, ct);
        if (h is null) return Result<HistorialPreciosDto?>.Success(null, allowNull: true);
        var vig = PublicCatalogSpecs.PrecioVigente(h.Precios, hoy);
        var ef = PublicCatalogSpecs.SeleccionarOfertaEfectiva(h.Ofertas, vig, hoy);
        var valores = h.Precios.ToDictionary(p => p.IdPrecio, p => p.Valor);
        var precios = h.Precios.Select(p => new HistorialPrecioDto(p.IdPrecio, p.Valor, p.FecVig,
            !p.Activo ? "Anulado" : p.IdPrecio == vig?.IdPrecio ? "Vigente" : p.FecVig > hoy ? "Programado" : "Historico")).ToList();
        var ofertas = h.Ofertas.Select(o =>
        {
            var (estado, motivo) = PublicCatalogSpecs.CalcularEstado(o, vig?.IdPrecio, vig?.Valor, hoy, o.IdOferta == ef?.IdOferta);
            return new HistorialOfertaDto(o.IdOferta, o.PrecioOferta, o.PctReferencial, o.FechaDesde, o.FechaHasta,
                o.IdPrecioReferencia, valores.TryGetValue(o.IdPrecioReferencia, out var v) ? v : null, o.Publicado,
                estado.ToString(), motivo == MotivoSinEfecto.Ninguno ? null : motivo.ToString());
        }).ToList();
        return Result<HistorialPreciosDto?>.Success(new HistorialPreciosDto(AFila(h.Fila), precios, ofertas));
    }

    public async Task<IReadOnlyList<FilaPrecioDto>> GetFilasAsync(int idTenant, IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default) =>
        (await _repo.GetPorIdsAsync(idTenant, idsPresProd, PublicCatalogSpecs.Hoy(), ct)).Select(AFila).ToList();

    public async Task<(string Moneda, int Decimales)> GetMonedaAsync(int idTenant, CancellationToken ct = default)
    {
        var r = await _cfg.GetTenantConfigAsync(idTenant, ct);
        var moneda = r.IsSuccess && !string.IsNullOrWhiteSpace(r.Value?.Moneda) ? r.Value!.Moneda! : "CLP";
        return (moneda, ReglasMoneda.Decimales(moneda));
    }

    // R3 (precio efectivo), R4 (% contra la referencia = precio vigente) y margen sobre el costo promedio.
    internal static FilaPrecioDto AFila(PrecioGrillaRow r)
    {
        var oficial = r.IdPrecio is long ip ? new PrecioVigenciaDto(ip, r.PrecioValor!.Value, r.PrecioDesde!.Value) : null;
        var oferta = r.IdOferta is long io && oficial is not null
            ? new OfertaFilaDto(io, r.OfertaPrecio!.Value, PublicCatalogSpecs.PctDescuento(oficial.Valor, r.OfertaPrecio), r.OfertaDesde!.Value, r.OfertaHasta)
            : null;
        decimal? efectivo = oficial is null ? null : PublicCatalogSpecs.PrecioFinal(oficial.Valor, oferta?.Precio);
        decimal? margen = efectivo is > 0 && r.CostoProm > 0
            ? Math.Round((efectivo.Value - r.CostoProm) / efectivo.Value * 100m, 1, MidpointRounding.AwayFromZero)
            : null;
        return new FilaPrecioDto(
            r.IdPresProd, r.IdProd, r.CodigoProducto, r.CodigoVariante, r.Producto, r.Variante, r.Categoria, r.Marca, r.CodigoBarras,
            r.Vendible, r.Publicable, oficial,
            r.IdPrecioProgramado is long pp ? new PrecioVigenciaDto(pp, r.PrecioProgramadoValor!.Value, r.PrecioProgramadoDesde!.Value) : null,
            oferta,
            r.IdOfertaProgramada is long op ? new OfertaFilaDto(op, r.OfertaProgramadaPrecio!.Value, null, r.OfertaProgramadaDesde!.Value, r.OfertaProgramadaHasta) : null,
            Math.Max(0, r.OfertasActivasHoy - (oferta is null ? 0 : 1)),
            efectivo, r.CostoProm, margen);
    }

    private static Error Validation(string msg) =>
        new Error("VALIDATION", msg).WithType(ErrorType.Validation).WithHttpStatus(400);
}
