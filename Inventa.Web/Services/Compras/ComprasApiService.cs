using Inventa.Web.Models.Compras;
using Inventa.Web.Models.Common;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Compras;

public sealed class ComprasApiService
{
    private readonly ApiClient _api;
    public ComprasApiService(ApiClient api) => _api = api;

    public Task<List<OrdenCompraDto>?> GetOrdenesAsync(CancellationToken ct) =>
        _api.GetAsync<List<OrdenCompraDto>>("api/compras/ordenes", ct);
    public Task<PagedResponse<OrdenCompraDto>?> GetOrdenesPagedAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<OrdenCompraDto>>($"api/compras/ordenes?{string.Join("&", qs)}", ct);
    }

    public Task<OrdenCompraDto?> GetOrdenAsync(long id, CancellationToken ct) =>
        _api.GetAsync<OrdenCompraDto>($"api/compras/ordenes/{id}", ct);

    public Task<OrdenCompraDto?> CreateOrdenAsync(CreateOrdenDto dto, CancellationToken ct) =>
        _api.PostAsync<CreateOrdenDto, OrdenCompraDto>("api/compras/ordenes", dto, ct);

    public Task RecepcionAsync(long id, RecepcionDto dto, CancellationToken ct) =>
        _api.PostAsync($"api/compras/ordenes/{id}/recepcion", dto, ct);
}
