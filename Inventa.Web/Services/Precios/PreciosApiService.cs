using System.Text.Json;
using Inventa.Web.Models.Api;
using Inventa.Web.Models.Precios;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Precios;

/// <summary>S93.1: cliente de «Precios y ofertas» (api/precios). Traduce 422/409 a <see cref="RespuestaCambiosPrecios"/>.</summary>
public sealed class PreciosApiService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private readonly ApiClient _api;

    public PreciosApiService(ApiClient api) => _api = api;

    public Task<PaginaPreciosDto?> GetGrillaAsync(string? q, long? idCategoria, int? idMarca, EstadoFiltroPrecios estado,
        bool incluirInactivos, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string> { $"page={page}", $"pageSize={pageSize}", $"estado={(int)estado}" };
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        if (idCategoria is long c) qs.Add($"idCategoria={c}");
        if (idMarca is int m) qs.Add($"idMarca={m}");
        if (incluirInactivos) qs.Add("incluirInactivos=true");
        return _api.GetAsync<PaginaPreciosDto>($"api/precios/grilla?{string.Join("&", qs)}", ct);
    }

    public Task<ResumenPreciosDto?> GetResumenAsync(CancellationToken ct) =>
        _api.GetAsync<ResumenPreciosDto>("api/precios/resumen", ct);

    public Task<HistorialPreciosDto?> GetHistorialAsync(long idPresProd, CancellationToken ct) =>
        _api.GetAsync<HistorialPreciosDto>($"api/precios/presentaciones/{idPresProd}/historial", ct);

    public Task<RespuestaCambiosPrecios> SimularAsync(IReadOnlyList<CambioPrecioDto> cambios, CancellationToken ct) =>
        EnviarAsync(cambios, simular: true, ct);

    public Task<RespuestaCambiosPrecios> AplicarAsync(IReadOnlyList<CambioPrecioDto> cambios, CancellationToken ct) =>
        EnviarAsync(cambios, simular: false, ct);

    private async Task<RespuestaCambiosPrecios> EnviarAsync(IReadOnlyList<CambioPrecioDto> cambios, bool simular, CancellationToken ct)
    {
        try
        {
            var r = await _api.PostAsync<CambiosPreciosRequest, ResultadoCambiosPreciosDto>(
                $"api/precios/cambios?simular={(simular ? "true" : "false")}", new CambiosPreciosRequest(cambios), ct);
            return new RespuestaCambiosPrecios(r, r?.Errores ?? [], r?.Desactualizados ?? [], null);
        }
        catch (ApiException ex) when (ex.StatusCode is 409 or 422)
        {
            var errores = new List<ErrorCambioPrecioDto>();
            var desact = new List<long>();
            if (!string.IsNullOrWhiteSpace(ex.RawBody))
            {
                try
                {
                    using var doc = JsonDocument.Parse(ex.RawBody);
                    if (doc.RootElement.TryGetProperty("errores", out var e) && e.ValueKind == JsonValueKind.Array)
                        errores.AddRange(e.Deserialize<List<ErrorCambioPrecioDto>>(Json) ?? []);
                    if (doc.RootElement.TryGetProperty("desactualizados", out var d) && d.ValueKind == JsonValueKind.Array)
                        desact.AddRange(d.Deserialize<List<long>>(Json) ?? []);
                }
                catch (JsonException) { /* cuerpo no estándar: se informa el mensaje */ }
            }
            if (ex.StatusCode == 422 && errores.Count == 0)
                errores.Add(new ErrorCambioPrecioDto(-1, 0, "VALIDACION", ex.Message));
            return new RespuestaCambiosPrecios(null, errores, desact, ex.Message);
        }
    }
}
