using System.Net;
using System.Text.Json;
using Inventa.Web.Models.Api;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Storage;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Logging;

namespace Inventa.Web.Services.Http;

/// <summary>
/// Centralizes ProblemDetails parsing + G-401 auto-logout (S54.16).
/// On 401: clears localStorage, resets AuthenticationState, navigates /login once per session without loop.
/// </summary>
public sealed class ErrorHandler : DelegatingHandler
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IServiceProvider _sp;
    private static int _handling401; // 0/1 interlocked to avoid duplicate navigation
    private readonly ILogger<ErrorHandler> _logger;

    public ErrorHandler(IServiceProvider sp, ILogger<ErrorHandler> logger)
    {
        _sp = sp;
        _logger = logger;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var response = await base.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Avoid logging token/url with secret, keep correlation for diagnostics
            var corr = TryGetCorrelation(response, string.Empty);
            _logger.LogWarning("HTTP 401 {Method} {Uri} correlation {Corr} — triggering auto-logout", request.Method, request.RequestUri?.AbsolutePath, corr);
            // S125 D3: sin Task.Run (AGENTS.md §6); mismo cuerpo, sin esperar a que termine.
            _ = CerrarSesionAsync();

            async Task CerrarSesionAsync()
            {
                if (System.Threading.Interlocked.Exchange(ref _handling401, 1) == 1) return;
                try
                {
                    using var scope = _sp.CreateScope();
                    var storage = scope.ServiceProvider.GetRequiredService<ITokenStorage>();
                    var auth = scope.ServiceProvider.GetRequiredService<CustomAuthenticationStateProvider>();
                    var nav = scope.ServiceProvider.GetRequiredService<NavigationManager>();
                    await storage.ClearAsync();
                    await auth.ClearAuthenticationAsync();
                    var curr = nav.Uri;
                    // avoid loop if already on /login
                    if (!curr.Contains("/login", StringComparison.OrdinalIgnoreCase))
                    {
                        await Task.Delay(50);
                        nav.NavigateTo("/login", forceLoad: false);
                    }
                }
                finally { System.Threading.Interlocked.Exchange(ref _handling401, 0); }
            }
        }

        return response;
    }

    public static async Task<ApiError> ParseErrorAsync(HttpResponseMessage response, CancellationToken ct)
    {
        var raw = await response.Content.ReadAsStringAsync(ct);
        var correlation = TryGetCorrelation(response, raw);

        try
        {
            var pd = JsonSerializer.Deserialize<ProblemDetailsResponse>(raw, JsonOpts);
            if (pd is not null && (pd.Title is not null || pd.Detail is not null || pd.Status.HasValue))
            {
                return new ApiError
                {
                    StatusCode = pd.Status ?? (int)response.StatusCode,
                    Title = pd.Title ?? response.ReasonPhrase,
                    Detail = pd.Detail ?? raw,
                    Errors = pd.Errors,
                    CorrelationId = pd.CorrelationId ?? correlation,
                    RawBody = raw
                };
            }
        }
        catch
        {
            // fall through to generic
        }

        return new ApiError
        {
            StatusCode = (int)response.StatusCode,
            Title = response.ReasonPhrase,
            Detail = string.IsNullOrWhiteSpace(raw) ? response.ReasonPhrase : raw,
            CorrelationId = correlation,
            RawBody = raw
        };
    }

    private static string? TryGetCorrelation(HttpResponseMessage response, string body)
    {
        if (response.Headers.TryGetValues(CorrelationHandler.HeaderName, out var vals))
            return vals.FirstOrDefault();
        if (response.Headers.TryGetValues("X-CorrelationId", out var vals2))
            return vals2.FirstOrDefault();
        try
        {
            using var doc = JsonDocument.Parse(body);
            if (doc.RootElement.TryGetProperty("correlationId", out var c))
                return c.GetString();
            if (doc.RootElement.TryGetProperty("correlation_id", out var c2))
                return c2.GetString();
        }
        catch { }
        return null;
    }
}
