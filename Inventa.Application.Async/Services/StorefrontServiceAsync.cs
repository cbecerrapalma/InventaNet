using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S57.34: storefront público (INVENTA_STORE, anónimo) + administración backoffice.
/// Usa exclusivamente IStorefrontRepositoryAsync. Sin DbContext.
/// </summary>
public interface IStorefrontServiceAsync
{
    Task<Result<int>> ResolveStoreTenantAsync(int idTenantPassPlat, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ActiveTenantItem>>> GetActiveTenantsAsync(CancellationToken ct = default);
    // S63: parámetros nuevos opcionales (documentados en S63.1-CATALOG-CONTRACT).
    Task<Result<(int Total, IReadOnlyList<PublicCardItem> Items)>> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default);
    // S63: reemplaza a GetPublicDetailAsync (bundle crudo) por la vista calculada R1–R4.
    Task<Result<PublicDetailView?>> GetPublicDetailViewAsync(int idTenant, long idProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PublicCategoryItem>>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PublicBrandItem>>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default);
    Task<Result<StorefrontProfileBundle?>> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default);
    Task<Result<TenantStorefront?>> GetProfileAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TenantSocialLink>>> GetLinksAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TenantBusinessHour>>> GetHoursAsync(int idTenant, CancellationToken ct = default);
    Task<Result<TenantStorefront>> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default);
    Task<Result<TenantSocialLink>> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default);
    Task<Result<bool>> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default);
    Task<Result<IReadOnlyList<TenantBusinessHour>>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default);
}

public sealed class StorefrontServiceAsync : IStorefrontServiceAsync
{
    private readonly IStorefrontRepositoryAsync _repo;
    private readonly ILogger<StorefrontServiceAsync> _logger;
    public StorefrontServiceAsync(IStorefrontRepositoryAsync repo, ILogger<StorefrontServiceAsync> logger)
    {
        _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<int>> ResolveStoreTenantAsync(int idTenantPassPlat, CancellationToken ct = default)
    {
        var r = await _repo.ResolveTenantByPassPlatAsync(idTenantPassPlat, ct);
        if (r == null)
            return Result<int>.Failure(new Error("STORE_TENANT_NOT_FOUND", "Tenant PassPlat sin mapping Inventa.").WithType(ErrorType.NotFound).WithHttpStatus(503));
        if (!r.Activo)
            return Result<int>.Failure(new Error("STORE_TENANT_INACTIVE", "Tenant Inventa inactivo.").WithType(ErrorType.Validation).WithHttpStatus(503));
        return Result<int>.Success(r.IdTenant);
    }

    public async Task<Result<IReadOnlyList<ActiveTenantItem>>> GetActiveTenantsAsync(CancellationToken ct = default)
        => Result<IReadOnlyList<ActiveTenantItem>>.Success(await _repo.GetActiveTenantsAsync(ct));

    // S63: normalización con el patrón existente (inválido => default, nunca 400):
    // orden desconocido => defecto; precios negativos => se ignoran; min>max => swap;
    // idMarca <= 0 => se ignora.
    public async Task<Result<(int Total, IReadOnlyList<PublicCardItem> Items)>> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default)
    {
        page = Math.Clamp(page, 1, 1000);
        pageSize = pageSize switch { 20 => 20, 50 => 50, 100 => 100, _ => 20 };
        q = q?.Trim();
        if (q is not null && q.Length > 100) q = q[..100];
        if (precioMin.HasValue && precioMin.Value < 0) precioMin = null;
        if (precioMax.HasValue && precioMax.Value < 0) precioMax = null;
        if (precioMin.HasValue && precioMax.HasValue && precioMin.Value > precioMax.Value)
            (precioMin, precioMax) = (precioMax, precioMin);
        if (idMarca.HasValue && idMarca.Value <= 0) idMarca = null;
        orden = NormalizeOrden(orden);
        try
        {
            var r = await _repo.GetPublicCardsAsync(idTenant, q, page, pageSize, idCategoria, soloOfertas, precioMin, precioMax, idMarca, orden, ct);
            return Result<(int, IReadOnlyList<PublicCardItem>)>.Success(r);
        }
        catch (CatalogoNoDisponibleException ex)
        {
            _logger.LogWarning("Catálogo no disponible: {Code}", ex.Code);
            return Result<(int, IReadOnlyList<PublicCardItem>)>.Failure(new Error("CATALOGO_NO_DISPONIBLE", "El catálogo no está disponible en este momento.").WithType(ErrorType.Database).WithHttpStatus(503));
        }
    }

    internal static string NormalizeOrden(string? orden) => orden switch
    {
        "precio_asc" => "precio_asc",
        "precio_desc" => "precio_desc",
        "descuento_desc" => "descuento_desc",
        "nombre_asc" => "nombre_asc",
        _ => "defecto",
    };

    // S63 §4.3: vista calculada R1–R4 desde el bundle (mismas expresiones compiladas).
    public async Task<Result<PublicDetailView?>> GetPublicDetailViewAsync(int idTenant, long idProd, CancellationToken ct = default)
    {
        var b = await _repo.GetPublicDetailAsync(idTenant, idProd, ct);
        if (b == null) return Result<PublicDetailView?>.Success(null, allowNull: true);
        var hoy = PublicCatalogSpecs.Hoy();
        var esCand = PublicCatalogSpecs.PrecioCandidato(hoy).Compile();
        var esVig = PublicCatalogSpecs.OfertaVigente(hoy).Compile();
        var presViews = b.Presentaciones.Select(pr =>
        {
            var vigente = b.Precios
                .Where(p => p.IdPresProd == pr.IdPresProd && esCand(p))
                .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio)
                .FirstOrDefault();
            // S77 R2': efectiva solo si la referencia sigue siendo el vigente
            // y la oferta mejora la referencia; % y referencia contra ella.
            var candidatas = b.OfertasVigentes
                .Where(o => o.IdPresProd == pr.IdPresProd && esVig(o)
                    && vigente != null
                    && PublicCatalogSpecs.OfertaEfectiva(o, vigente.IdPrecio, o.PrecioReferencia?.Valor))
                .OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta)
                .ToList();
            var ganadora = candidatas.FirstOrDefault();
            var refGanadora = ganadora?.PrecioReferencia?.Valor;
            var final = ganadora != null ? ganadora.PrecioOferta : vigente?.Valor;
            b.Unidades.TryGetValue(pr.IdUnidad, out var uni);
            return new PublicDetailPresView(
                pr.IdPresProd, pr.IdVarProd, pr.Descrip, uni ?? pr.IdUnidad.ToString(),
                vigente?.IdPrecio, vigente?.Valor, vigente?.FecVig, final,
                vigente == null ? null : PublicCatalogSpecs.PctDescuento(refGanadora ?? vigente.Valor, ganadora?.PrecioOferta),
                candidatas.Select(o =>
                {
                    var refValor = o.PrecioReferencia?.Valor ?? vigente!.Valor;
                    return new PublicDetailOfertaView(
                        o.IdOferta, o.PrecioOferta, o.FechaDesde, o.FechaHasta,
                        o.PrecioReferencia?.Valor,
                        vigente != null && vigente.IdPrecio == o.IdPrecioReferencia,
                        PublicCatalogSpecs.PctDescuento(refValor, o.PrecioOferta));
                }).ToList());
        }).ToList();
        return Result<PublicDetailView?>.Success(new PublicDetailView(
            b.IdProd, b.Codigo, b.Nombre, b.IdCategoria, b.CatNombre, b.IdMarca, b.MarcaNombre,
            b.Variantes, presViews, b.Unidades, b.ImagenPrincipalUrl, b.Imagenes), allowNull: true);
    }

    public async Task<Result<IReadOnlyList<PublicCategoryItem>>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default)
        => Result<IReadOnlyList<PublicCategoryItem>>.Success(await _repo.GetPublicCategoriesAsync(idTenant, ct));

    public async Task<Result<IReadOnlyList<PublicBrandItem>>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default)
        => Result<IReadOnlyList<PublicBrandItem>>.Success(await _repo.GetPublicBrandsAsync(idTenant, ct));

    // S60.1: el perfil publicado sale ya sanitizado (branding + moneda).
    // Punto único de sanitización: StorefrontBranding.
    public async Task<Result<StorefrontProfileBundle?>> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default)
    {
        var b = await _repo.GetPublishedProfileAsync(idTenant, ct);
        if (b != null)
        {
            StorefrontBranding.SanitizePublished(b.Profile);
            b = b with { Moneda = StorefrontBranding.SanitizeMoneda(b.Moneda) };
        }
        return Result<StorefrontProfileBundle?>.Success(b, allowNull: true);
    }

    public async Task<Result<TenantStorefront?>> GetProfileAsync(int idTenant, CancellationToken ct = default)
        => Result<TenantStorefront?>.Success(await _repo.GetProfileAsync(idTenant, ct), allowNull: true);

    public async Task<Result<IReadOnlyList<TenantSocialLink>>> GetLinksAsync(int idTenant, CancellationToken ct = default)
        => Result<IReadOnlyList<TenantSocialLink>>.Success(await _repo.GetLinksAsync(idTenant, false, ct));

    public async Task<Result<IReadOnlyList<TenantBusinessHour>>> GetHoursAsync(int idTenant, CancellationToken ct = default)
        => Result<IReadOnlyList<TenantBusinessHour>>.Success(await _repo.GetHoursAsync(idTenant, false, ct));

    public async Task<Result<TenantStorefront>> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default)
        => Result<TenantStorefront>.Success(await _repo.UpsertProfileAsync(profile, ct));

    public async Task<Result<TenantSocialLink>> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default)
        => Result<TenantSocialLink>.Success(await _repo.AddLinkAsync(link, ct));

    public async Task<Result<bool>> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default)
        => Result<bool>.Success(await _repo.RemoveLinkAsync(idTenant, idLink, ct));

    public async Task<Result<IReadOnlyList<TenantBusinessHour>>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default)
        => Result<IReadOnlyList<TenantBusinessHour>>.Success(await _repo.ReplaceHoursAsync(idTenant, hours, ct));
}
