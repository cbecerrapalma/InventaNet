using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

// ── S92: DTOs de la administración de productos (nunca entidades) ──
public sealed record FichaPrecioDto(long Id, decimal Valor, DateOnly Desde);
public sealed record FichaOfertaDto(decimal Precio, DateOnly Desde, DateOnly? Hasta, int? Pct);
public sealed record FichaCodigoDto(string Codigo, string Tipo, bool Principal, long IdCodBar = 0, byte TipoCod = 0); // S94: +IdCodBar/TipoCod (gestión de códigos)
public sealed record FichaPresentacionDto(long IdPresProd, string Descrip, bool Activo, bool EsVenta);

public sealed record FichaVarianteDto(
    long IdVarProd, string Codigo, string Nombre, bool Activo, decimal CostoProm,
    FichaPresentacionDto? Presentacion, IReadOnlyList<FichaCodigoDto> Codigos,
    FichaPrecioDto? PrecioVigente, FichaOfertaDto? Oferta,
    decimal StockDisponible);

public sealed record FichaProductoDto(
    long IdProd, string Codigo, string Nombre, string Categoria, string? Marca, string Unidad,
    bool EsPesable, bool ManejaLote, int? DiasVencimiento, bool Activo, bool VisibleStore,
    int Imagenes, string? ImagenPrincipal, string Estado, IReadOnlyList<string> Razones,
    IReadOnlyList<FichaVarianteDto> Variantes);

public sealed record ProductoListadoItemDto(
    long IdProd, string Codigo, string Nombre, string Categoria, string? Marca, bool Activo, bool VisibleStore,
    string Estado, IReadOnlyList<string> Razones, bool TieneMarca, bool TieneImagen, bool Vendible, bool TienePrecio,
    int Variantes, decimal? PrecioMin, decimal? PrecioMax);

public sealed record ProductoResumenDto(
    int Total, int Publicados, int ConAdvertencia, int Listos, int Pendientes, int Inactivos,
    int SinPrecio, int SinImagen, int SinMarca, int SinVarianteVendible);

/// <summary>S92: filtros del listado en términos de Application (el controlador no conoce Data.Async).</summary>
public sealed record ProductoListadoConsulta(
    string? Texto = null, string? Estado = null, bool? ConPrecio = null, bool? ConImagen = null,
    bool? ConMarca = null, long? IdCategoria = null, bool? Activo = null);

/// <summary>Contrato que Inventa.Web espera de GET api/productos/publishability (S57.2) + Estado (S92).</summary>
public sealed record PublishabilityRowDto(
    long IdProd, string Codigo, string Nombre, bool Activo, bool VisibleStore, bool TienePresVenta, bool TienePrecio,
    bool TieneImagen, bool TieneMarca, bool TieneCategoria, bool Publicable, IReadOnlyList<string> Razones, string Estado);

public interface IProductoFichaServiceAsync
{
    Task<Result<FichaProductoDto?>> GetFichaAsync(int idTenant, long idProd, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<ProductoListadoItemDto> Items, int Total)>> ListarAsync(int idTenant, ProductoListadoConsulta consulta, int page, int pageSize, CancellationToken ct = default);
    Task<Result<ProductoResumenDto>> GetResumenAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PublishabilityRowDto>>> GetPublishabilityAsync(int idTenant, CancellationToken ct = default);
}

/// <summary>
/// S92: lectura de la administración de productos. Estado, precio vigente (R1), oferta efectiva (R2/R2'/R4)
/// y publicación se calculan SOLO con <see cref="PublicCatalogSpecs"/>, como la ficha pública de la tienda.
/// </summary>
public sealed class ProductoFichaServiceAsync : IProductoFichaServiceAsync
{
    private readonly IProductoFichaRepositoryAsync _repo;

    public ProductoFichaServiceAsync(IProductoFichaRepositoryAsync repo) => _repo = repo;

    public async Task<Result<FichaProductoDto?>> GetFichaAsync(int idTenant, long idProd, CancellationToken ct = default)
    {
        var b = await _repo.GetFichaAsync(idTenant, idProd, ct);
        return b is null
            ? Result<FichaProductoDto?>.Success(null, allowNull: true)
            : Result<FichaProductoDto?>.Success(ArmarFicha(b), allowNull: true);
    }

    public async Task<Result<(IReadOnlyList<ProductoListadoItemDto> Items, int Total)>> ListarAsync(
        int idTenant, ProductoListadoConsulta consulta, int page, int pageSize, CancellationToken ct = default)
    {
        var filtro = new ProductoListadoFiltro(consulta.Texto, consulta.Estado, consulta.ConPrecio, consulta.ConImagen,
            consulta.ConMarca, consulta.IdCategoria, consulta.Activo);
        page = page < 1 ? 1 : page;
        pageSize = pageSize is 20 or 50 or 100 ? pageSize : 20;
        var (rows, total) = await _repo.SearchListadoAsync(idTenant, filtro, page, pageSize, ct);
        var rangos = await _repo.GetRangoPrecioVigenteAsync(rows.Select(r => r.IdProd).ToList(), ct);
        IReadOnlyList<ProductoListadoItemDto> items = rows.Select(r =>
        {
            var tiene = rangos.TryGetValue(r.IdProd, out var rg);
            return new ProductoListadoItemDto(r.IdProd, r.Codigo, r.Nombre, r.Categoria, r.Marca, r.Activo, r.VisibleStore,
                Estado(r), PublicCatalogSpecs.RazonesTienda(r), r.TieneMarca, r.TieneImagen, r.Vendible, r.TienePrecio,
                r.Variantes, tiene ? rg.Min : null, tiene ? rg.Max : null);
        }).ToList();
        return Result<(IReadOnlyList<ProductoListadoItemDto>, int)>.Success((items, total));
    }

    public async Task<Result<ProductoResumenDto>> GetResumenAsync(int idTenant, CancellationToken ct = default)
    {
        var d = await _repo.GetDiagnosticosAsync(idTenant, ct);
        var estados = d.Select(Estado).ToList();
        return Result<ProductoResumenDto>.Success(new ProductoResumenDto(
            d.Count,
            estados.Count(e => e == EstadosTienda.Publicado),
            estados.Count(e => e == EstadosTienda.PublicadoConAdvertencia),
            estados.Count(e => e == EstadosTienda.Listo),
            estados.Count(e => e == EstadosTienda.Pendiente),
            estados.Count(e => e == EstadosTienda.Inactivo),
            d.Count(x => !x.TienePrecio),
            d.Count(x => !x.TieneImagen),
            d.Count(x => !x.TieneMarca),
            d.Count(x => !x.Vendible)));
    }

    public async Task<Result<IReadOnlyList<PublishabilityRowDto>>> GetPublishabilityAsync(int idTenant, CancellationToken ct = default)
    {
        var d = await _repo.GetDiagnosticosAsync(idTenant, ct);
        IReadOnlyList<PublishabilityRowDto> rows = d.Select(x => new PublishabilityRowDto(
            x.IdProd, x.Codigo, x.Nombre, x.Activo, x.VisibleStore, x.Vendible, x.TienePrecio, x.TieneImagen, x.TieneMarca,
            x.IdCategoria > 0, x.Activo && x.Vendible, PublicCatalogSpecs.RazonesTienda(x), Estado(x))).ToList();
        return Result<IReadOnlyList<PublishabilityRowDto>>.Success(rows);
    }

    private static string Estado(ProductoDiagnosticoRow r) =>
        PublicCatalogSpecs.EstadoTienda(r.Activo, r.VisibleStore, r.Vendible, r.TienePrecio);

    /// <summary>Arma la ficha desde el bundle (puro; probado sin BD).</summary>
    public static FichaProductoDto ArmarFicha(FichaProductoBundle b)
    {
        var esCand = PublicCatalogSpecs.PrecioCandidato(b.Hoy).Compile();
        var esVig = PublicCatalogSpecs.OfertaVigente(b.Hoy).Compile();
        var c = b.Cabecera;
        var variantes = b.Variantes.Select(v =>
        {
            var pres = b.Presentaciones.Where(p => p.IdVarProd == v.IdVarProd).OrderBy(p => p.IdPresProd).FirstOrDefault();
            if (pres is null)
                return new FichaVarianteDto(v.IdVarProd, v.Codigo, v.Nombre, v.Activo, v.CostoProm, null, [], null, null, 0m);
            var vigente = b.PreciosCandidatos
                .Where(p => p.IdPresProd == pres.IdPresProd && esCand(p))
                .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio)
                .FirstOrDefault();
            var oferta = vigente is null ? null : b.OfertasVigentes
                .Where(o => o.IdPresProd == pres.IdPresProd && esVig(o)
                    && PublicCatalogSpecs.OfertaEfectiva(o, vigente.IdPrecio, o.PrecioReferencia?.Valor))
                .OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta)
                .FirstOrDefault();
            return new FichaVarianteDto(
                v.IdVarProd, v.Codigo, v.Nombre, v.Activo, v.CostoProm,
                new FichaPresentacionDto(pres.IdPresProd, pres.Descrip, pres.Activo, pres.EsVenta),
                b.Codigos.Where(x => x.IdPresProd == pres.IdPresProd)
                    .OrderByDescending(x => x.Principal).ThenBy(x => x.Codigo)
                    .Select(x => new FichaCodigoDto(x.Codigo, TipoCodigo(x.TipoCod), x.Principal, x.IdCodBar, x.TipoCod)).ToList(),
                vigente is null ? null : new FichaPrecioDto(vigente.IdPrecio, vigente.Valor, vigente.FecVig),
                oferta is null ? null : new FichaOfertaDto(oferta.PrecioOferta, oferta.FechaDesde, oferta.FechaHasta,
                    PublicCatalogSpecs.PctDescuento(oferta.PrecioReferencia?.Valor ?? vigente!.Valor, oferta.PrecioOferta)),
                b.StockPorPresentacion.TryGetValue(pres.IdPresProd, out var st) ? st : 0m);
        }).ToList();
        var categoria = c.CategoriaPadre is null ? c.Categoria : $"{c.CategoriaPadre} > {c.Categoria}";
        return new FichaProductoDto(
            c.IdProd, c.Codigo, c.Nombre, categoria, c.Marca, $"{c.UnidadNombre} ({c.UnidadCodigo})",
            c.EsPesable, c.ManejaLote, c.DiasVen, c.Activo, c.VisibleStore, b.Imagenes, b.ImagenPrincipal,
            Estado(b.Diagnostico), PublicCatalogSpecs.RazonesTienda(b.Diagnostico), variantes);
    }

    public static string TipoCodigo(byte tipo) => tipo switch
    {
        1 => "Normal",
        2 => "Pesable",
        3 => "Interno",
        4 => "Proveedor",
        _ => $"Tipo {tipo}",
    };
}
