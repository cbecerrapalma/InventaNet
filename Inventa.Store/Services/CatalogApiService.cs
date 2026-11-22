using System.Globalization;
using System.Net.Http.Json;
using System.Text;
using Inventa.Store.Models;

namespace Inventa.Store.Services;

public sealed class CatalogApiService
{
    private readonly HttpClient _http;
    public CatalogApiService(HttpClient http) => _http = http;

    // S64: filtros/orden del contrato S63 viajan al servidor (pre-paginado).
    // Valores default se omiten para no ensuciar la URL.
    public static string BuildProductsUrl(string? q, int page, int pageSize, long? idCategoria,
        bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null,
        long? idMarca = null, string? orden = null)
    {
        var sb = new StringBuilder($"api/public/catalog/products?q={Uri.EscapeDataString(q ?? "")}&page={page}&pageSize={pageSize}");
        if (idCategoria.HasValue) sb.Append($"&idCategoria={idCategoria}");
        if (soloOfertas) sb.Append("&soloOfertas=true");
        if (precioMin.HasValue) sb.Append($"&precioMin={precioMin.Value.ToString(CultureInfo.InvariantCulture)}");
        if (precioMax.HasValue) sb.Append($"&precioMax={precioMax.Value.ToString(CultureInfo.InvariantCulture)}");
        if (idMarca is > 0) sb.Append($"&idMarca={idMarca}");
        if (!string.IsNullOrWhiteSpace(orden) && !string.Equals(orden, "defecto", StringComparison.OrdinalIgnoreCase))
            sb.Append($"&orden={Uri.EscapeDataString(orden)}");
        return sb.ToString();
    }

    public async Task<PagedResponse<PublicProductDto>?> GetProductsAsync(string? q, int page, int pageSize, long? idCategoria, CancellationToken ct = default)
        => await GetProductsAsync(q, page, pageSize, idCategoria, false, null, null, null, null, ct);

    public async Task<PagedResponse<PublicProductDto>?> GetProductsAsync(string? q, int page, int pageSize, long? idCategoria,
        bool soloOfertas, decimal? precioMin, decimal? precioMax, long? idMarca, string? orden, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<PagedResponse<PublicProductDto>>(
            BuildProductsUrl(q, page, pageSize, idCategoria, soloOfertas, precioMin, precioMax, idMarca, orden), ct);

    public async Task<PublicProductDetailDto?> GetProductAsync(long id, CancellationToken ct = default)
        => await _http.GetFromJsonAsync<PublicProductDetailDto>($"api/public/catalog/products/{id}", ct);

    public async Task<IReadOnlyList<PublicCategoryDto>?> GetCategoriesAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<PublicCategoryDto>>("api/public/catalog/categories", ct);

    // S64: marcas del contrato S63 (GET brands).
    public async Task<IReadOnlyList<PublicBrandDto>?> GetBrandsAsync(CancellationToken ct = default)
        => await _http.GetFromJsonAsync<IReadOnlyList<PublicBrandDto>>("api/public/catalog/brands", ct);
}
