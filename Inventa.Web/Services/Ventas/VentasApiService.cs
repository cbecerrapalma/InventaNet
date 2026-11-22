using Inventa.Web.Models.Common;
using Inventa.Web.Models.Ventas;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Ventas;

public sealed class VentasApiService
{
    private readonly ApiClient _api;
    public VentasApiService(ApiClient api) => _api = api;

    public Task<List<VentaDto>?> ListAsync(CancellationToken ct) => _api.GetAsync<List<VentaDto>>("api/ventas", ct);
    public Task<PagedResponse<VentaDto>?> ListPagedAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<VentaDto>>($"api/ventas?{string.Join("&", qs)}", ct);
    }
    public Task<VentaDto?> GetAsync(long id, CancellationToken ct) => _api.GetAsync<VentaDto>($"api/ventas/{id}", ct);
    public Task<RegistrarVentaResult?> RegistrarAsync(RegistrarVentaDto dto, CancellationToken ct) => _api.PostAsync<RegistrarVentaDto, RegistrarVentaResult>("api/ventas", dto, ct);
    public Task AnularAsync(long id, AnularDto dto, CancellationToken ct) => _api.PostAsync($"api/ventas/{id}/anular", dto, ct);
}
