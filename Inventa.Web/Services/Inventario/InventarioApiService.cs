using Inventa.Web.Models.Common;
using Inventa.Web.Models.Inventario;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Inventario;

public sealed class InventarioApiService
{
    private readonly ApiClient _api;
    public InventarioApiService(ApiClient api) => _api = api;

    public Task<List<StockDto>?> GetStockAsync(int? idBodega, CancellationToken ct)
    {
        var url = idBodega.HasValue ? $"api/inventario/stock?idBodega={idBodega.Value}" : "api/inventario/stock";
        return _api.GetAsync<List<StockDto>>(url, ct);
    }

    public Task<PagedResponse<StockDispDto>?> GetStockDispPagedAsync(int? idBodega, string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (idBodega.HasValue) qs.Add($"idBodega={idBodega.Value}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<StockDispDto>>($"api/inventario/stock-disp?{string.Join("&", qs)}", ct);
    }

    public Task<PagedResponse<StockDto>?> GetStockPagedAsync(int? idBodega, string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (idBodega.HasValue) qs.Add($"idBodega={idBodega.Value}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<StockDto>>($"api/inventario/stock?{string.Join("&", qs)}", ct);
    }
    public Task<List<MovInvDto>?> GetMovimientosAsync(CancellationToken ct) =>
        _api.GetAsync<List<MovInvDto>>("api/inventario/movimientos", ct);
    public Task<PagedResponse<MovInvDto>?> GetMovimientosPagedAsync(int? idBodega, byte? tipoMov, string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (idBodega.HasValue) qs.Add($"idBodega={idBodega.Value}");
        if (tipoMov.HasValue) qs.Add($"tipoMov={tipoMov.Value}");
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<MovInvDto>>($"api/inventario/movimientos?{string.Join("&", qs)}", ct);
    }
    public Task<List<LoteDto>?> GetLotesAsync(CancellationToken ct) =>
        _api.GetAsync<List<LoteDto>>("api/inventario/lotes", ct);
    public Task<PagedResponse<LoteDto>?> GetLotesPagedAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<LoteDto>>($"api/inventario/lotes?{string.Join("&", qs)}", ct);
    }

    public Task TransferirAsync(TransferirDto dto, CancellationToken ct) =>
        _api.PostAsync("api/inventario/transferir", dto, ct);

    public Task AjustarAsync(AjustarDto dto, CancellationToken ct) =>
        _api.PostAsync("api/inventario/ajustar", dto, ct);
}
