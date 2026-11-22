using System.Net.Http.Json;
using System.Text.Json;
using Inventa.Web.Models.Api;

namespace Inventa.Web.Services.Http;

/// <summary>
/// Single HTTP abstraction for all Pages — Pages never use HttpClient directly.
/// Centralizes JSON, headers, ProblemDetails, Correlation, Cancellation.
/// </summary>
public sealed class ApiClient
{
    private readonly HttpClient _http;
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ApiClient(HttpClient http)
    {
        _http = http;
    }

    public async Task<T?> GetAsync<T>(string uri, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(uri, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string uri, TRequest body, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(uri, body, JsonOpts, ct);
        return await HandleResponseAsync<TResponse>(response, ct);
    }

    public async Task<TResponse?> PostAsync<TResponse>(string uri, object? body, CancellationToken ct = default)
    {
        HttpResponseMessage response = body is null
            ? await _http.PostAsync(uri, null, ct)
            : await _http.PostAsJsonAsync(uri, body, JsonOpts, ct);
        return await HandleResponseAsync<TResponse>(response, ct);
    }

    public async Task PostAsync<TRequest>(string uri, TRequest body, CancellationToken ct = default)
    {
        var response = await _http.PostAsJsonAsync(uri, body, JsonOpts, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string uri, TRequest body, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync(uri, body, JsonOpts, ct);
        return await HandleResponseAsync<TResponse>(response, ct);
    }

    public async Task PutAsync<TRequest>(string uri, TRequest body, CancellationToken ct = default)
    {
        var response = await _http.PutAsJsonAsync(uri, body, JsonOpts, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<TResponse?> PatchAsync<TRequest, TResponse>(string uri, TRequest body, CancellationToken ct = default)
    {
        var response = await _http.PatchAsJsonAsync(uri, body, JsonOpts, ct);
        return await HandleResponseAsync<TResponse>(response, ct);
    }

    public async Task DeleteAsync(string uri, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync(uri, ct);
        await EnsureSuccessAsync(response, ct);
    }

    public async Task<T?> DeleteAsync<T>(string uri, CancellationToken ct = default)
    {
        var response = await _http.DeleteAsync(uri, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    public async Task<T?> PostMultipartAsync<T>(string uri, MultipartFormDataContent multipart, CancellationToken ct = default)
    {
        var response = await _http.PostAsync(uri, multipart, ct);
        return await HandleResponseAsync<T>(response, ct);
    }

    // S57.83-G3: descarga binaria (namespace privado de imágenes; <img> no envía JWT).
    public async Task<(byte[] Bytes, string? ContentType)> GetBytesAsync(string uri, CancellationToken ct = default)
    {
        var response = await _http.GetAsync(uri, ct);
        if (!response.IsSuccessStatusCode)
        {
            var error = await ErrorHandler.ParseErrorAsync(response, ct);
            throw new ApiException(error);
        }
        var bytes = await response.Content.ReadAsByteArrayAsync(ct);
        return (bytes, response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task<T?> HandleResponseAsync<T>(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
        {
            if (response.StatusCode == System.Net.HttpStatusCode.NoContent)
                return default;
            if (response.Content.Headers.ContentLength == 0)
                return default;
            var content = await response.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(content))
                return default;
            return JsonSerializer.Deserialize<T>(content, JsonOpts);
        }

        var error = await ErrorHandler.ParseErrorAsync(response, ct);
        throw new ApiException(error);
    }

    private static async Task EnsureSuccessAsync(HttpResponseMessage response, CancellationToken ct)
    {
        if (response.IsSuccessStatusCode)
            return;
        var error = await ErrorHandler.ParseErrorAsync(response, ct);
        throw new ApiException(error);
    }
}
