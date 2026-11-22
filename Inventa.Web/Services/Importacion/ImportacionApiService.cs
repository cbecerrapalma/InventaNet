using Inventa.Web.Models.Importacion;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Importacion;

// S91: único acceso de la Web al canal api/importaciones (la página nunca usa HttpClient directo).
public sealed class ImportacionApiService
{
    private const string Base = "api/importaciones";
    private readonly ApiClient _api;
    public ImportacionApiService(ApiClient api) => _api = api;

    public Task<(byte[] Bytes, string? ContentType)> DescargarPlantillaAsync(CancellationToken ct)
        => _api.GetBytesAsync($"{Base}/plantillas/productos", ct);

    public async Task<LoteCreadoDto?> ValidarProductosAsync(string fileName, string contentType, Stream stream, CancellationToken ct)
    {
        using var form = new MultipartFormDataContent();
        var sc = new StreamContent(stream);
        sc.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        form.Add(sc, "file", fileName);
        return await _api.PostMultipartAsync<LoteCreadoDto>($"{Base}/productos", form, ct);
    }

    public Task<LoteDetalleDto?> GetLoteAsync(long id, CancellationToken ct)
        => _api.GetAsync<LoteDetalleDto>($"{Base}/{id}", ct);

    public Task<(byte[] Bytes, string? ContentType)> DescargarReporteAsync(long id, CancellationToken ct)
        => _api.GetBytesAsync($"{Base}/{id}/reporte", ct);

    public Task<AplicarResultadoDto?> AplicarAsync(long id, CancellationToken ct)
        => _api.PostAsync<object, AplicarResultadoDto?>($"{Base}/{id}/aplicar", new { }, ct);

    // S91.4: vista previa / auditoría de cambios paginada en el servidor.
    public Task<CambiosPageDto?> GetCambiosAsync(long id, string? accion, int page, int pageSize, CancellationToken ct)
        => _api.GetAsync<CambiosPageDto>($"{Base}/{id}/cambios?page={page}&pageSize={pageSize}"
            + (string.IsNullOrEmpty(accion) ? "" : $"&accion={Uri.EscapeDataString(accion)}"), ct);

    public Task<LotesPageDto?> ListarAsync(int page, int pageSize, CancellationToken ct)
        => _api.GetAsync<LotesPageDto>($"{Base}?page={page}&pageSize={pageSize}", ct);
}
