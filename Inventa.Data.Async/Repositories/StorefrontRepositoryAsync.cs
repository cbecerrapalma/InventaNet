using CBP.Data.Abstractions;
using CBP.Results;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S57.34: storefront (TenantStorefront/SocialLinks/BusinessHours) + catálogo público.
/// No existe repositorio Sync contraparte (acceso vía DbContext directo) — se documenta
/// como gap S57 cubierto solo en Async. Todo Async real con CancellationToken.
/// </summary>
public sealed record StoreTenantResolution(int IdTenant, bool Activo);
public sealed record ActiveTenantItem(int IdTenant, string Codigo, string Nombre, int? IdTenantPassPlat);
public sealed record PublicCardItem(
    long IdProd, string Codigo, string Nombre, long IdCategoria, string CatNombre,
    int? IdMarca, string? MarcaNombre, string? UnidadNombre,
    string? ImagenUrl, decimal? PrecioDesde, bool TieneOferta,
    // S63: precio efectivo (R1–R5). PrecioDesde conserva su semántica legacy (H2).
    decimal? PrecioFinalDesde, decimal? PrecioReferencia, int? PctDescuento, DateOnly? OfertaHasta);
// S69: hierarchy fields appended (repo returns raw data; sanitization lives in WebAPI/Store).
public sealed record PublicCategoryItem(long IdCategoria, string Codigo, string Nombre, int Cantidad, long? IdPadre = null, short Orden = 0, string? ImagenUrl = null);
public sealed record PublicBrandItem(int IdMarca, string Codigo, string Nombre, int Cantidad);
public sealed record StorefrontImageItem(string Url, bool EsPrincipal, int Orden);
public sealed record PublicDetailBundle(
    long IdProd, string Codigo, string Nombre, long IdCategoria, string CatNombre,
    int? IdMarca, string? MarcaNombre,
    IReadOnlyList<VarProd> Variantes,
    IReadOnlyList<PresProd> Presentaciones,
    IReadOnlyList<Precio> Precios,
    // S57.52: ofertas tabla nueva vigentes + referencia (aditivo, catálogo read-model mínimo).
    IReadOnlyList<Oferta> OfertasVigentes,
    IReadOnlyDictionary<int, string> Unidades,
    string? ImagenPrincipalUrl,
    IReadOnlyList<StorefrontImageItem> Imagenes);
public sealed record PublicDetailOfertaView(long IdOferta, decimal PrecioOferta, DateOnly FechaDesde, DateOnly? FechaHasta, decimal? PrecioReferenciaValor, bool ReferenciaEsVigente, int? PctDescuento);
public sealed record PublicDetailPresView(long IdPresProd, long IdVarProd, string Descrip, string Unidad, long? IdPrecioVigente, decimal? PrecioVigente, DateOnly? FecVig, decimal? PrecioFinal, int? PctDescuento, IReadOnlyList<PublicDetailOfertaView> Ofertas);
public sealed record PublicDetailView(long IdProd, string Codigo, string Nombre, long IdCategoria, string CatNombre, int? IdMarca, string? MarcaNombre, IReadOnlyList<VarProd> Variantes, IReadOnlyList<PublicDetailPresView> Presentaciones, IReadOnlyDictionary<int, string> Unidades, string? ImagenPrincipalUrl, IReadOnlyList<StorefrontImageItem> Imagenes);
public sealed record StorefrontProfileBundle(
    string TenantNombre,
    TenantStorefront Profile,
    IReadOnlyList<TenantSocialLink> Links,
    IReadOnlyList<TenantBusinessHour> Hours,
    // S60.1: Moneda cruda de TenantConfigInventa (null = sin fila). La sanitización
    // vive en Application (StorefrontBranding); el repositorio devuelve datos.
    string? Moneda);

public interface IStorefrontRepositoryAsync
{
    Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default);
    Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default);
    // S63: parámetros nuevos opcionales (retrocompatibles). Firmas documentadas en S63.1-CATALOG-CONTRACT.
    Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default);
    Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default);
    Task<IReadOnlyList<PublicCategoryItem>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<PublicBrandItem>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default);
    Task<StorefrontProfileBundle?> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default);
    Task<TenantStorefront?> GetProfileAsync(int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<TenantSocialLink>> GetLinksAsync(int idTenant, bool soloActivas, CancellationToken ct = default);
    Task<IReadOnlyList<TenantBusinessHour>> GetHoursAsync(int idTenant, bool soloActivas, CancellationToken ct = default);
    Task<TenantStorefront> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default);
    Task<TenantSocialLink> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default);
    Task<bool> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default);
    Task<IReadOnlyList<TenantBusinessHour>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default);
}

public sealed class StorefrontRepositoryAsync : IStorefrontRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    private readonly IUnitOfWorkAsync _uow;
    public StorefrontRepositoryAsync(InventaDbContext ctx, IUnitOfWorkAsync uow)
    {
        _ctx = ctx;
        _uow = uow;
    }

    public async Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default)
    {
        var t = await _ctx.Tenants.AsNoTracking()
            .Where(x => x.IdTenantPassPlat == idTenantPassPlat)
            .Select(x => new { x.IdTenant, x.Activo })
            .FirstOrDefaultAsync(ct);
        return t == null ? null : new StoreTenantResolution(t.IdTenant, t.Activo);
    }

    public async Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default)
        => await _ctx.Tenants.AsNoTracking().Where(t => t.Activo)
            .OrderBy(t => t.Codigo)
            .Select(t => new ActiveTenantItem(t.IdTenant, t.Codigo, t.Nombre, t.IdTenantPassPlat))
            .ToListAsync(ct);

    // S63/S73: catálogo público via SP dbo.sp_Store_CatalogoCards.
    // S73: reemplaza la consulta T-SQL en texto (S72) por invocación a SP desplegado
    // vía CBP IUnitOfWorkAsync.RawQuery.QuerySPWithOutputAsync<T>.
    // Semántica = EF original S63/S64/S70 (oráculo: S72_BaselineSqlTests.cs / PublicCatalogSpecs.cs).
    // S81: el SP no usa SNAPSHOT (revertido S80; ver Docs\Sprints\S81).
    public async Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default)
    {
        // S74 B5: idMarca fuera de rango int → vacío sin llamar al SP (comportamiento original).
        if (idMarca.HasValue && (idMarca.Value < int.MinValue || idMarca.Value > int.MaxValue))
            return (0, Array.Empty<PublicCardItem>());

        var hoy = PublicCatalogSpecs.Hoy();

        var parameters = new List<RawParameter>
        {
            RawParameter.Int("IdTenant", idTenant),
            RawParameter.Date("Hoy", hoy.ToDateTime(TimeOnly.MinValue)),
            RawParameter.NVarChar("Q", q, 400),
            RawParameter.BigInt("IdCategoria", idCategoria),
            RawParameter.Int("IdMarca", idMarca.HasValue ? (int)idMarca.Value : null),
            RawParameter.Bit("SoloOfertas", soloOfertas),
            RawParameter.Decimal("PrecioMin", precioMin),
            RawParameter.Decimal("PrecioMax", precioMax),
            // S74 B4: sin normalización local; el SP normaliza por lista blanca.
            // AnsiString porque el SP declara @Orden varchar(20).
            RawParameter.In("Orden", orden ?? "defecto", DbType.AnsiString, 20),
            RawParameter.Int("Page", page),
            RawParameter.Int("PageSize", pageSize),
            RawParameter.Out("TotalCount", DbType.Int32)
        };

        var spResult = await _uow.RawQuery.QuerySPWithOutputAsync<SpPublicCardRow>(
            "dbo.sp_Store_CatalogoCards",
            parameters,
            ct);

        // S74 B6: no incluir Error.Message (puede llevar texto SQL).
        if (spResult.IsFailure)
        {
            // S81: error de SP → CatalogoNoDisponibleException → 503 en controlador/middleware.
            throw new CatalogoNoDisponibleException(spResult.Error!.Code, $"Error SP catálogo: {spResult.Error.Code}");
        }

        // S74 B3: el total es el OUTPUT siempre, haya filas o no.
        var total = spResult.Value.GetOutput<int>("TotalCount");
        var rows = spResult.Value.Rows;

        if (rows.Count == 0) return (total, Array.Empty<PublicCardItem>());

        // S74 B1/B2: mapeo idéntico al EF original (R3–R5).
        // PrecioReferencia SOLO si hay oferta efectiva; OfertaHasta solo entonces.
        var items = rows.Select(MapRow).ToList();

        // S106: la grilla usa la variante 'listado' cuando existe. La Url del original es única (CBP.Storage), así que
        // se reemplaza la misma imagen que eligió el SP sin tocar su orden.
        var urls = items.Where(i => i.ImagenUrl is not null).Select(i => i.ImagenUrl!).Distinct().ToList();
        if (urls.Count > 0)
        {
            var miniaturas = await _ctx.ProductoImagenes.AsNoTracking()
                .Where(pi => pi.UrlMiniatura != null && urls.Contains(pi.Url))
                .Select(pi => new { pi.Url, pi.UrlMiniatura })
                .ToDictionaryAsync(x => x.Url, x => x.UrlMiniatura!, ct);
            if (miniaturas.Count > 0)
                items = items
                    .Select(i => i.ImagenUrl is not null && miniaturas.TryGetValue(i.ImagenUrl, out var mini) ? i with { ImagenUrl = mini } : i)
                    .ToList();
        }

        return (total, items);
    }

    // S74: mapeo fila del SP → PublicCardItem extraído para hacerlo testeable
    // (cubre la rama OfertaHasta no nula que los datos actuales no ejercitan).
    // S77 R2'/R4': PrecioReferencia y % contra la referencia congelada del SP.
    internal static PublicCardItem MapRow(SpPublicCardRow r) => new(
        r.IdProd, r.Codigo, r.Nombre, r.IdCategoria, r.CatNombre, r.IdMarca, r.MarcaNombre,
        r.UnidadNombre, r.ImagenUrl, r.PrecioDesde, r.TieneOferta,
        r.PrecioFinal,
        r.OfertaPrecio.HasValue ? r.PrecioReferencia : null,
        r.PrecioReferencia.HasValue
            ? PublicCatalogSpecs.PctDescuento(r.PrecioReferencia.Value, r.OfertaPrecio)
            : null,
        r.OfertaPrecio.HasValue && r.OfertaHasta.HasValue
            ? r.OfertaHasta
            : null);

    // Record que coincide EXACTAMENTE con el result set del SP dbo.sp_Store_CatalogoCards
    // (orden y tipos de columnas deben coincidir para el mapeo automático de CBP).
    // Debe tener constructor sin parámetros para satisfacer constraint 'where T : class, new()'.
    // S74: internal para la prueba unitaria del mapeo (MapRow).
    internal sealed class SpPublicCardRow
    {
        public long IdProd { get; set; }
        public string Codigo { get; set; } = null!;
        public string Nombre { get; set; } = null!;
        public long IdCategoria { get; set; }
        public string CatNombre { get; set; } = null!;
        public int? IdMarca { get; set; }
        public string? MarcaNombre { get; set; }
        public string? UnidadNombre { get; set; }
        public string? ImagenUrl { get; set; }
        public decimal? PrecioDesde { get; set; }
        public bool TieneOferta { get; set; }
        public decimal? PrecioFinal { get; set; }
        public decimal? PrecioVigente { get; set; }
        public decimal? OfertaPrecio { get; set; }
        // S77: columna nueva del SP (entre OfertaPrecio y OfertaHasta).
        public decimal? PrecioReferencia { get; set; }
        // S75: DateOnly directo (CBP DbValueConverter ya soporta DateOnly).
        public DateOnly? OfertaHasta { get; set; }
    }

    public async Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default)
    {
        var prod = await _ctx.Productos.AsNoTracking()
            .Where(p => p.IdProd == idProd && p.IdTenant == idTenant && p.Activo && p.VisibleStore
                && _ctx.Variantes.Any(v => v.IdProd == p.IdProd && v.Activo
                    && _ctx.Presentaciones.Any(pp => pp.IdVarProd == v.IdVarProd && pp.Activo && pp.EsVenta)))
            .Select(p => new { p.IdProd, p.Codigo, p.Nombre, p.IdCategoria, CatNombre = p.Categoria.Nombre, p.IdMarca, MarcaNombre = p.Marca != null ? p.Marca.Nombre : null })
            .FirstOrDefaultAsync(ct);
        if (prod == null) return null;
        var variantes = await _ctx.Variantes.AsNoTracking().Where(v => v.IdProd == idProd && v.Activo).ToListAsync(ct);
        var varIds = variantes.Select(v => v.IdVarProd).ToList();
        var presentaciones = await _ctx.Presentaciones.AsNoTracking().Where(pr => varIds.Contains(pr.IdVarProd) && pr.Activo && pr.EsVenta).ToListAsync(ct);
        var presIds = presentaciones.Select(pr => pr.IdPresProd).ToList();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var precios = await _ctx.Precios.AsNoTracking()
            // S57.69: precio normal único (sin discriminator).
            .Where(pc => presIds.Contains(pc.IdPresProd) && pc.Activo && pc.FecVig <= today)
            .OrderByDescending(pc => pc.FecVig).ToListAsync(ct);
        // S57.52: ofertas vigentes (tabla nueva) con referencia para el read-model mínimo.
        // S63 R2 (D1): Publicado = 1 obligatorio.
        var ofertasVigentes = await _ctx.Ofertas.AsNoTracking()
            .Include(o => o.PrecioReferencia)
            .Where(o => presIds.Contains(o.IdPresProd) && o.Activo && o.Publicado
                && o.FechaDesde <= today && (o.FechaHasta == null || o.FechaHasta >= today))
            .OrderByDescending(o => o.FechaDesde).ToListAsync(ct);
        var unidades = await _ctx.Unidades.AsNoTracking().ToDictionaryAsync(u => u.IdUnidad, u => u.Nombre, ct);
        var imgPrincipal = await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.IdProd == idProd && pi.Activo)
            .OrderByDescending(pi => pi.EsPrincipal).ThenBy(pi => pi.Orden)
            .Select(pi => pi.UrlFicha ?? pi.Url).FirstOrDefaultAsync(ct);
        var imagenes = await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.IdProd == idProd && pi.Activo)
            .OrderByDescending(pi => pi.EsPrincipal).ThenBy(pi => pi.Orden)
            .Select(pi => new StorefrontImageItem(pi.UrlFicha ?? pi.Url, pi.EsPrincipal, pi.Orden))
            .ToListAsync(ct);
        return new PublicDetailBundle(prod.IdProd, prod.Codigo, prod.Nombre, prod.IdCategoria, prod.CatNombre, prod.IdMarca, prod.MarcaNombre, variantes, presentaciones, precios, ofertasVigentes, unidades, imgPrincipal, imagenes);
    }

    // S63 §4.4: Cantidad = productos publicables (misma regla EsPublicable).
    // S70 (H1): UNA consulta trae todas las activas con sus publicables propios;
    // PublicCategoryHierarchy.Build agrega (padre = propios + Σ hijas) y ordena
    // (Orden, Nombre, IdCategoria). ImagenUrl cruda: el controlador sanitiza.
    public async Task<IReadOnlyList<PublicCategoryItem>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default)
    {
        var pub = PublicCatalogSpecs.EsPublicable;
        var rows = await _ctx.Categorias.AsNoTracking().Where(c => c.IdTenant == idTenant && c.Activo)
            .Select(c => new CategoryRow(c.IdCategoria, c.Codigo, c.Nombre,
                c.IdPadre, c.Orden, c.ImagenUrl,
                _ctx.Productos.Where(pr => pr.IdTenant == idTenant && pr.IdCategoria == c.IdCategoria).Where(pub).Count()))
            .ToListAsync(ct);
        return PublicCategoryHierarchy.Build(rows);
    }

    // S63 §4.5: marcas activas con >= 1 producto publicable. Sin marca (null) no se lista.
    public async Task<IReadOnlyList<PublicBrandItem>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default)
    {
        var pub = PublicCatalogSpecs.EsPublicable;
        return await _ctx.Marcas.AsNoTracking().Where(m => m.IdTenant == idTenant && m.Activo)
            .Where(m => _ctx.Productos.Where(pr => pr.IdTenant == idTenant && pr.IdMarca == m.IdMarca).Where(pub).Any())
            .OrderBy(m => m.Nombre)
            .Select(m => new PublicBrandItem(m.IdMarca, m.Codigo, m.Nombre,
                _ctx.Productos.Where(pr => pr.IdTenant == idTenant && pr.IdMarca == m.IdMarca).Where(pub).Count()))
            .ToListAsync(ct);
    }

    public async Task<StorefrontProfileBundle?> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default)
    {
        var p = await _ctx.TenantStorefronts.AsNoTracking().FirstOrDefaultAsync(x => x.IdTenant == idTenant && x.Publicado, ct);
        if (p == null) return null;
        // S60.1: nombre + moneda en UNA sola consulta (subselect correlacionado).
        var tenant = await _ctx.Tenants.AsNoTracking()
            .Where(t => t.IdTenant == idTenant)
            .Select(t => new
            {
                t.Nombre,
                Moneda = _ctx.TenantConfigs.AsNoTracking()
                    .Where(c => c.IdTenant == t.IdTenant)
                    .Select(c => c.Moneda).FirstOrDefault()
            })
            .FirstOrDefaultAsync(ct);
        var links = await _ctx.TenantSocialLinks.AsNoTracking().Where(s => s.IdTenant == idTenant && s.Activo).OrderBy(s => s.Orden).ToListAsync(ct);
        var hours = await _ctx.TenantBusinessHours.AsNoTracking().Where(h => h.IdTenant == idTenant && h.Activo).OrderBy(h => h.DiaSemana).ToListAsync(ct);
        return new StorefrontProfileBundle(tenant?.Nombre ?? "", p, links, hours, tenant?.Moneda);
    }

    public Task<TenantStorefront?> GetProfileAsync(int idTenant, CancellationToken ct = default)
        => _ctx.TenantStorefronts.FirstOrDefaultAsync(x => x.IdTenant == idTenant, ct);

    public async Task<IReadOnlyList<TenantSocialLink>> GetLinksAsync(int idTenant, bool soloActivas, CancellationToken ct = default)
    {
        var q = _ctx.TenantSocialLinks.AsNoTracking().Where(s => s.IdTenant == idTenant);
        if (soloActivas) q = q.Where(s => s.Activo);
        return await q.OrderBy(s => s.Orden).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<TenantBusinessHour>> GetHoursAsync(int idTenant, bool soloActivas, CancellationToken ct = default)
    {
        var q = _ctx.TenantBusinessHours.AsNoTracking().Where(h => h.IdTenant == idTenant);
        if (soloActivas) q = q.Where(h => h.Activo);
        return await q.OrderBy(h => h.DiaSemana).ToListAsync(ct);
    }

    public async Task<TenantStorefront> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default)
    {
        var existing = await _ctx.TenantStorefronts.FirstOrDefaultAsync(x => x.IdTenant == profile.IdTenant, ct);
        if (existing == null) _ctx.TenantStorefronts.Add(profile);
        else _ctx.Entry(existing).CurrentValues.SetValues(profile);
        await _ctx.SaveChangesAsync(ct);
        return existing ?? profile;
    }

    public async Task<TenantSocialLink> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default)
    {
        _ctx.TenantSocialLinks.Add(link);
        await _ctx.SaveChangesAsync(ct);
        return link;
    }

    public async Task<bool> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default)
    {
        var e = await _ctx.TenantSocialLinks.FirstOrDefaultAsync(x => x.IdTenantSocialLink == idLink && x.IdTenant == idTenant, ct);
        if (e == null) return false;
        _ctx.TenantSocialLinks.Remove(e);
        await _ctx.SaveChangesAsync(ct);
        return true;
    }

    public async Task<IReadOnlyList<TenantBusinessHour>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default)
    {
        var existing = await _ctx.TenantBusinessHours.Where(x => x.IdTenant == idTenant).ToListAsync(ct);
        _ctx.TenantBusinessHours.RemoveRange(existing);
        foreach (var h in hours) _ctx.TenantBusinessHours.Add(h);
        await _ctx.SaveChangesAsync(ct);
        return await _ctx.TenantBusinessHours.AsNoTracking().Where(x => x.IdTenant == idTenant).OrderBy(x => x.DiaSemana).ToListAsync(ct);
    }
}
