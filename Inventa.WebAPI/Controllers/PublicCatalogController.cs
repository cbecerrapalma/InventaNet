using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/public/catalog")]
[AllowAnonymous]
public class PublicCatalogController : ControllerBase
{
    private readonly IStorefrontServiceAsync _svc;
    private readonly IConfiguration _cfg;
    public PublicCatalogController(IStorefrontServiceAsync svc, IConfiguration cfg) { _svc = svc; _cfg = cfg; }

    // S57.0: PassPlat tenant is authoritative. Deployment configures
    // PublicCatalog:PassPlatTenantId (EXTERNAL id); resolved server-side
    // via Tenant.IdTenantPassPlat -> Tenant.IdTenant. No client input,
    // no first-active fallback, fail closed on any misconfiguration.
    async Task<int?> ResolveTenantIdAsync(CancellationToken ct)
    {
        var raw = _cfg["PublicCatalog:PassPlatTenantId"];
        if (!int.TryParse(raw, out var ppTenant) || ppTenant <= 0)
            return null;
        var r = await _svc.ResolveStoreTenantAsync(ppTenant, ct);
        return r.IsSuccess ? r.Value : null;
    }

    IActionResult StoreUnavailable(string code)
        => Problem(statusCode: 503, title: "Catalogo no disponible", detail: code, instance: HttpContext.TraceIdentifier);

    // S63.1: filtros/orden en servidor antes de paginar (soloOfertas, precioMin/Max,
    // idMarca, orden). Normalización en el servicio; totalCount refleja los filtros.
    [HttpGet("products")]
    public async Task<IActionResult> GetProducts([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] long? idCategoria, [FromQuery] bool soloOfertas = false, [FromQuery] decimal? precioMin = null, [FromQuery] decimal? precioMax = null, [FromQuery] long? idMarca = null, [FromQuery] string? orden = null, CancellationToken ct = default)
    {
        var idTenant = await ResolveTenantIdAsync(ct);
        if (idTenant == null)
        {
            var raw = _cfg["PublicCatalog:PassPlatTenantId"];
            if (!int.TryParse(raw, out var pp) || pp <= 0) return StoreUnavailable("STORE_TENANT_NOT_CONFIGURED");
            var rr = await _svc.ResolveStoreTenantAsync(pp, ct);
            return StoreUnavailable(rr.Error?.Code ?? "STORE_TENANT_NOT_FOUND");
        }
        var p = Math.Clamp(page ?? 1, 1, 1000);
        var ps = pageSize switch { 20 => 20, 50 => 50, 100 => 100, _ => 20 };
        var r = await _svc.GetPublicCardsAsync(idTenant.Value, q, p, ps, idCategoria, soloOfertas, precioMin, precioMax, idMarca, orden, ct);
        if (!r.IsSuccess)
        {
            if (r.Error?.Code == "CATALOGO_NO_DISPONIBLE")
            {
                Response.Headers.RetryAfter = "5";
                return StoreUnavailable(r.Error.Code);
            }
            return StatusCode(500);
        }
        var (total, cards) = r.Value;
        var items = cards.Select(pr => new PublicProductDto(pr.IdProd, pr.Codigo, pr.Nombre, pr.IdCategoria, pr.CatNombre, pr.IdMarca, pr.MarcaNombre, pr.UnidadNombre, pr.ImagenUrl, pr.PrecioDesde, pr.TieneOferta, pr.PrecioFinalDesde, pr.PrecioReferencia, pr.PctDescuento, pr.OfertaHasta)).ToList();
        var totalPages = (int)Math.Ceiling(total / (double)ps);
        return Ok(new { items, page = p, pageSize = ps, totalCount = total, totalPages });
    }

    [HttpGet("products/{id:long}")]
    public async Task<IActionResult> GetProduct(long id, CancellationToken ct)
    {
        var idTenant = await ResolveTenantIdAsync(ct);
        if (idTenant == null) return StoreUnavailable("STORE_TENANT_NOT_CONFIGURED");
        // S57.2: private/unpublished/products without valid sale presentation are indistinguishable from missing (404, no leakage).
        var r = await _svc.GetPublicDetailViewAsync(idTenant.Value, id, ct);
        if (!r.IsSuccess) return StatusCode(500);
        var detail = r.Value;
        if (detail == null) return NotFound();
        // S63.1: vista ya calculada (R1–R4) por el servicio; el controlador solo mapea.
        // S57.69: Mayorista NO se expone (relación no leída) = invisibilidad preservada.
        var presByVar = detail.Presentaciones.GroupBy(pr => pr.IdVarProd)
            .ToDictionary(g => g.Key, g => g.ToList());
        var varDtos = detail.Variantes.Select(v =>
        {
            presByVar.TryGetValue(v.IdVarProd, out var presForVar);
            var presDtos = (presForVar ?? new List<Inventa.Data.Async.Repositories.PublicDetailPresView>()).Select(pr =>
            {
                var priceDtos = pr.IdPrecioVigente == null || pr.PrecioVigente == null || pr.FecVig == null
                    ? new List<PublicPriceDto>()
                    : new List<PublicPriceDto> { new(pr.IdPrecioVigente.Value, pr.PrecioVigente.Value, pr.FecVig.Value, true) };
                var ofertaDtos = pr.Ofertas.Select(o => new PublicOfertaDto(o.IdOferta, o.PrecioOferta, o.FechaDesde, o.FechaHasta, o.PrecioReferenciaValor, o.ReferenciaEsVigente, o.PctDescuento)).ToList();
                return new PublicPresentacionDto(pr.IdPresProd, pr.Descrip, pr.Unidad, priceDtos, ofertaDtos, pr.PrecioVigente, pr.PrecioFinal, pr.PctDescuento);
            }).ToList();
            return new PublicVarianteDto(v.IdVarProd, v.Codigo, v.Nombre, presDtos);
        }).ToList();
        var imagenes = detail.Imagenes.Select(i => new PublicImageDto(i.Url, i.EsPrincipal, i.Orden)).ToList();
        return Ok(new PublicProductDetailDto(detail.IdProd, detail.Codigo, detail.Nombre, detail.IdCategoria, detail.CatNombre, detail.IdMarca, detail.MarcaNombre, varDtos, new List<PublicPriceDto>(), detail.ImagenPrincipalUrl, imagenes));
    }

    // S57.1 public storefront profile (server-resolved tenant, no internal IDs).
    [HttpGet("storefront")]
    public async Task<IActionResult> GetStorefront(CancellationToken ct)
    {
        var idTenant = await ResolveTenantIdAsync(ct);
        if (idTenant == null) return StoreUnavailable("STORE_TENANT_NOT_CONFIGURED");
        var r = await _svc.GetPublishedProfileAsync(idTenant.Value, ct);
        if (!r.IsSuccess) return StatusCode(500);
        var profile = r.Value;
        if (profile == null) return NoContent();
        var p = profile.Profile;
        // S60.1: branding ya sanitizado por el servicio; el controlador solo mapea.
        return Ok(new PublicStorefrontDto(
            profile.TenantNombre,
            p.NombreFantasia, p.DescripcionCorta, p.Descripcion, p.LogoUrl,
            new PublicContactDto(p.Telefono, p.WhatsApp, p.Email, p.Direccion, p.Comuna, p.Ciudad, p.Region, p.SitioWeb),
            profile.Links.Select(s => new PublicSocialDto(s.Tipo, s.Url)).ToList(),
            profile.Hours.Select(h => new PublicHourDto(h.DiaSemana, StorefrontFmt.Fmt(h.Apertura1), StorefrontFmt.Fmt(h.Cierre1), StorefrontFmt.Fmt(h.Apertura2), StorefrontFmt.Fmt(h.Cierre2), h.Cerrado)).ToList(),
            new PublicBrandingDto(p.ColorAcento, p.FaviconUrl, p.AnuncioTexto, p.PerfilStore, profile.Moneda ?? "CLP")));
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var idTenant = await ResolveTenantIdAsync(ct);
        if (idTenant == null) return StoreUnavailable("STORE_TENANT_NOT_CONFIGURED");
        // S57.5 P4.5: only categories having at least one PUBLICABLE product (same rule, tenant-scoped). No empty categories.
        var r = await _svc.GetPublicCategoriesAsync(idTenant.Value, ct);
        if (!r.IsSuccess) return StatusCode(500);
        return Ok(r.Value.Select(c => new PublicCategoryDto(c.IdCategoria, c.Codigo, c.Nombre, c.Cantidad, c.IdPadre, c.Orden, StorefrontFmt.SanitizeStoreImagenUrl(c.ImagenUrl))).ToList());
    }

    // S63.1 brands: marcas activas con >= 1 producto publicable. Mismo resolvedor
    // de tenant y mismos errores que el resto ([AllowAnonymous] de clase).
    [HttpGet("brands")]
    public async Task<IActionResult> GetBrands(CancellationToken ct)
    {
        var idTenant = await ResolveTenantIdAsync(ct);
        if (idTenant == null) return StoreUnavailable("STORE_TENANT_NOT_CONFIGURED");
        var r = await _svc.GetPublicBrandsAsync(idTenant.Value, ct);
        if (!r.IsSuccess) return StatusCode(500);
        return Ok(r.Value.Select(m => new PublicBrandDto(m.IdMarca, m.Codigo, m.Nombre, m.Cantidad)).ToList());
    }
}

// S63.1: campos nuevos appended (existentes intactos para el Store vigente).
public sealed record PublicProductDto(long IdProd, string Codigo, string Nombre, long IdCategoria, string CategoriaNombre, int? IdMarca, string? MarcaNombre, string? UnidadNombre, string? ImagenUrl, decimal? PrecioDesde, bool TieneOferta, decimal? PrecioFinalDesde = null, decimal? PrecioReferencia = null, int? PctDescuento = null, DateOnly? OfertaHasta = null);
public sealed record PublicProductDetailDto(long IdProd, string Codigo, string Nombre, long IdCategoria, string CategoriaNombre, int? IdMarca, string? MarcaNombre, IReadOnlyList<PublicVarianteDto> Variantes, IReadOnlyList<PublicPriceDto> PreciosVigentes, string? ImagenUrl, IReadOnlyList<PublicImageDto> Imagenes);
public sealed record PublicImageDto(string Url, bool EsPrincipal, int Orden);
public sealed record PublicVarianteDto(long IdVarProd, string Codigo, string Nombre, IReadOnlyList<PublicPresentacionDto> Presentaciones);
public sealed record PublicPresentacionDto(long IdPresProd, string Descrip, string Unidad, IReadOnlyList<PublicPriceDto> Precios, IReadOnlyList<PublicOfertaDto> Ofertas, decimal? PrecioVigente = null, decimal? PrecioFinal = null, int? PctDescuento = null);
public sealed record PublicOfertaDto(long IdOferta, decimal PrecioOferta, DateOnly FechaDesde, DateOnly? FechaHasta, decimal? PrecioReferenciaValor, bool ReferenciaEsVigente, int? PctDescuento = null);
public sealed record PublicPriceDto(long IdPrecio, decimal Valor, DateOnly FecVig, bool Vigente);
public sealed record PublicCategoryDto(long IdCategoria, string Codigo, string Nombre, int Cantidad = 0, long? IdPadre = null, short Orden = 0, string? ImagenUrl = null);
public sealed record PublicBrandDto(int IdMarca, string Codigo, string Nombre, int Cantidad);
public sealed record PublicStorefrontDto(string NombreLegal, string? NombreFantasia, string? DescripcionCorta, string? Descripcion, string? LogoUrl, PublicContactDto Contacto, IReadOnlyList<PublicSocialDto> Redes, IReadOnlyList<PublicHourDto> Horarios, PublicBrandingDto Branding);
// S60.1: branding público (valores ya sanitizados por el servicio).
public sealed record PublicBrandingDto(string? ColorAcento, string? FaviconUrl, string? AnuncioTexto, string PerfilStore, string Moneda);
public sealed record PublicContactDto(string? Telefono, string? WhatsApp, string? Email, string? Direccion, string? Comuna, string? Ciudad, string? Region, string? SitioWeb);
public sealed record PublicSocialDto(string Tipo, string Url);
public sealed record PublicHourDto(byte DiaSemana, string? Apertura1, string? Cierre1, string? Apertura2, string? Cierre2, bool Cerrado);

// S69: helper público para tests (sin DB). Sin estado, sin secretos.
public static partial class StorefrontFmt
{
    public static string? Fmt(TimeSpan? t) => t.HasValue ? t.Value.ToString(@"hh\:mm") : null;

    // S69: el catálogo público no rechaza, solo omite la imagen (null).
    // S70 (H5): delega en la regla única CategoriaImagenUrlRule.
    public static string? SanitizeStoreImagenUrl(string? url) =>
        CategoriaImagenUrlRule.Sanitizar(url);
}

/// <summary>S57.0 fail-closed store tenant failure. Message is safe for clients; Code is logged.</summary>
public sealed class StoreTenantException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
