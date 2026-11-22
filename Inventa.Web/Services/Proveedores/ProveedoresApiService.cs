using Inventa.Web.Models.Common;
using Inventa.Web.Models.Proveedores;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Proveedores;

public sealed class ProveedoresApiService
{
    private readonly ApiClient _api;
    public ProveedoresApiService(ApiClient api) => _api = api;

    public Task<List<ProveedorDto>?> ListAsync(CancellationToken ct) =>
        _api.GetAsync<List<ProveedorDto>>("api/proveedores", ct);
    public Task<PagedResponse<ProveedorDto>?> ListPagedAsync(string? q, bool? activo, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        if (activo.HasValue) qs.Add(activo.Value ? "activo=true" : "activo=false");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<ProveedorDto>>($"api/proveedores?{string.Join("&", qs)}", ct);
    }

    // Backend does NOT expose GET /api/proveedores/{id} (see ProveedoresController.cs).
    // Keep for completeness — will 404; detail page falls back to List filter.
    public Task<ProveedorDto?> GetByIdAsync(int id, CancellationToken ct) =>
        _api.GetAsync<ProveedorDto>($"api/proveedores/{id}", ct);

    public Task<ProveedorDto?> CreateAsync(CreateProveedorDto dto, CancellationToken ct) =>
        _api.PostAsync<CreateProveedorDto, ProveedorDto>("api/proveedores", dto, ct);

    public Task<ProveedorDto?> UpdateAsync(int id, CreateProveedorDto dto, CancellationToken ct) =>
        _api.PutAsync<CreateProveedorDto, ProveedorDto>($"api/proveedores/{id}", dto, ct);
}
