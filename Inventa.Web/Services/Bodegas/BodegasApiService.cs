using Inventa.Web.Models.Bodegas;
using Inventa.Web.Models.Common;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Bodegas;

public sealed class BodegasApiService
{
    private readonly ApiClient _api;
    public BodegasApiService(ApiClient api) => _api = api;

    public Task<List<BodegaDto>?> ListAsync(CancellationToken ct) =>
        _api.GetAsync<List<BodegaDto>>("api/bodegas", ct);
    public Task<PagedResponse<BodegaDto>?> ListPagedAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<BodegaDto>>($"api/bodegas?{string.Join("&", qs)}", ct);
    }

    public Task<BodegaDto?> GetByIdAsync(int id, CancellationToken ct) =>
        _api.GetAsync<BodegaDto>($"api/bodegas/{id}", ct);

    public Task<BodegaDto?> CreateAsync(CreateBodegaDto dto, CancellationToken ct) =>
        _api.PostAsync<CreateBodegaDto, BodegaDto>("api/bodegas", dto, ct);

    public Task<BodegaDto?> UpdateAsync(int id, CreateBodegaDto dto, CancellationToken ct) =>
        _api.PutAsync<CreateBodegaDto, BodegaDto>($"api/bodegas/{id}", dto, ct);
}
