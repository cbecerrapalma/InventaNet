using Inventa.Store.Models;
using Microsoft.Extensions.Configuration;

namespace Inventa.Store.Services;

/// <summary>S57.1 scoped storefront profile loaded once per circuit (1 request).
/// U12: también comparte categorías cargadas una sola vez por el layout. *@
public sealed class StorefrontState
{
    private readonly StorefrontApiService _api;
    private readonly string _apiBase;
    private Task<PublicStorefrontDto?>? _task;
    private IReadOnlyList<CategoryNode>? _categorias;
    private IReadOnlyList<PublicCategoryDto>? _categoriasRaw;

    public StorefrontState(StorefrontApiService api, IConfiguration config)
    {
        _api = api;
        _apiBase = config["ApiBaseUrl"]?.TrimEnd('/') ?? "";
    }

    public Task<PublicStorefrontDto?> GetAsync(CancellationToken ct = default) => _task ??= LoadAsync(ct);
    public string DisplayName(PublicStorefrontDto? p) => !string.IsNullOrWhiteSpace(p?.NombreFantasia) ? p.NombreFantasia! : (!string.IsNullOrWhiteSpace(p?.NombreLegal) ? p.NombreLegal! : "Nuestra tienda");

    // S61.1 branding nunca null (defaults si el API no lo trae). Se fija al cargar el perfil.
    public PublicBrandingDto Branding { get; private set; } = new(null, null, null, "General", "CLP");
    public string Moneda => Branding.Moneda;

    // S61.1 misma regla que GetImageUrl inline (Catalog.razor:266, ProductDetail.razor:130):
    // relativa → prefijo con la base del API; absoluta http(s) → tal cual; vacía → null.
    // S104: separador '/' entre la base y la ruta (ProductoImagen.Url es «uploads/…», sin '/' inicial).
    public string? ResolveAssetUrl(string? url) => string.IsNullOrWhiteSpace(url) ? null
        : url.StartsWith("//", StringComparison.Ordinal) ? "https:" + url
        : url.StartsWith("http", StringComparison.OrdinalIgnoreCase) ? url
        : $"{_apiBase}/{url.TrimStart('/')}";

    // U12: categorías compartidas entre Layout y Catalog
    public bool CategoriasCargadas => _categorias is not null;
    public IReadOnlyList<CategoryNode>? Categorias => _categorias;
    public IReadOnlyList<PublicCategoryDto>? CategoriasRaw => _categoriasRaw;

    // S101: avisa a las páginas que ya renderizaron antes de que el layout cargara las categorías.
    public event Action? CategoriasChanged;

    public void SetCategorias(IReadOnlyList<CategoryNode> categorias, IReadOnlyList<PublicCategoryDto> categoriasRaw)
    {
        _categorias = categorias;
        _categoriasRaw = categoriasRaw;
        CategoriasChanged?.Invoke();
    }

    private async Task<PublicStorefrontDto?> LoadAsync(CancellationToken ct)
    {
        var p = await _api.GetStorefrontAsync(ct);
        var b = p?.Branding;
        Branding = b is null
            ? new(null, null, null, "General", "CLP")
            : new(b.ColorAcento, b.FaviconUrl, b.AnuncioTexto,
                string.IsNullOrWhiteSpace(b.PerfilStore) ? "General" : b.PerfilStore,
                string.IsNullOrWhiteSpace(b.Moneda) ? "CLP" : b.Moneda);
        return p;
    }
}