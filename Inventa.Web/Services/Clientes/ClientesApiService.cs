using Inventa.Web.Models.Clientes;
using Inventa.Web.Models.Common;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Clientes;

public sealed class ClientesApiService
{
    private readonly ApiClient _api;
    public ClientesApiService(ApiClient api) => _api = api;

    public Task<List<ClienteDto>?> ListAsync(CancellationToken ct) =>
        _api.GetAsync<List<ClienteDto>>("api/clientes", ct);
    public Task<PagedResponse<ClienteDto>?> ListPagedAsync(string? q, bool? activo, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        if (activo.HasValue) qs.Add(activo.Value ? "activo=true" : "activo=false");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<ClienteDto>>($"api/clientes?{string.Join("&", qs)}", ct);
    }

    public Task<ClienteDto?> GetByIdAsync(int id, CancellationToken ct) =>
        _api.GetAsync<ClienteDto>($"api/clientes/{id}", ct);

    public Task<ClienteDto?> CreateAsync(CreateClienteDto dto, CancellationToken ct) =>
        _api.PostAsync<CreateClienteDto, ClienteDto>("api/clientes", dto, ct);

    public Task<ClienteDto?> UpdateAsync(int id, CreateClienteDto dto, CancellationToken ct) =>
        _api.PutAsync<CreateClienteDto, ClienteDto>($"api/clientes/{id}", dto, ct);
}
