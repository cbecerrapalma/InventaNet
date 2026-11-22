using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Inventa.Store.Models;

namespace Inventa.Store.Services;

// S68 pieza 4: logica de auth (login/refresh/logout) llamando
// a /api/public/auth (S67). Reutiliza el mismo HttpClient/BaseAddress
// que CatalogApiService (DI scoped). Sin idApp/idTenant en request.
// Un solo vuelo a la vez (SemaphoreSlim) para login/refresh.
// Password limpiado tras intento. Nunca loggea secretos.
public class StoreAuthService : IStoreAuthService
{
    private readonly HttpClient _http;
    private readonly StoreSessionStorage _storage;
    private readonly ILogger<StoreAuthService> _logger;
    private readonly SemaphoreSlim _gate = new(1, 1);

    public StoreAuthService(HttpClient http, StoreSessionStorage storage, ILogger<StoreAuthService> logger)
    {
        _http = http;
        _storage = storage;
        _logger = logger;
    }

    public async Task<StoreAuthResult<StoreTokenPair>> LoginAsync(string identificador, string password, CancellationToken ct = default)
    {
        var req = new StoreLoginRequest(identificador, password);
        try
        {
            await _gate.WaitAsync(ct);
            var resp = await _http.PostAsJsonAsync("api/public/auth/login", req, ct);
            if (resp.IsSuccessStatusCode)
            {
                var payload = await resp.Content.ReadFromJsonAsync<StoreTokenPair>(cancellationToken: ct);
                if (payload != null)
                {
                    await _storage.SaveAsync(new StoreSession(payload.AccessToken, payload.RefreshToken), ct);
                    _logger.LogInformation("StoreAuth login ok");
                    return StoreAuthResult<StoreTokenPair>.Ok(payload);
                }
            }
            var problem = await ReadProblemAsync(resp, ct);
            _logger.LogWarning("StoreAuth login {Code}", problem?.Code ?? "HTTP_" + (int)resp.StatusCode);
            return StoreAuthResult<StoreTokenPair>.Fail(problem ?? new StoreAuthProblem((int)resp.StatusCode, "STORE_AUTH_UPSTREAM", null, "empty"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "StoreAuth login upstream unreachable");
            return StoreAuthResult<StoreTokenPair>.Fail(new StoreAuthProblem(503, "STORE_AUTH_UPSTREAM", null, ex.GetType().Name));
        }
        finally
        {
            _gate.Release();
            // limpieza defensa
            password = string.Empty;
        }
    }

    public async Task<StoreAuthResult<StoreTokenPair>> RefreshAsync(CancellationToken ct = default)
    {
        var session = await _storage.LoadAsync<StoreSession>(ct);
        var rt = session?.RefreshToken;
        if (string.IsNullOrEmpty(rt))
        {
            await _storage.ClearAsync(ct);
            return StoreAuthResult<StoreTokenPair>.Fail(new StoreAuthProblem(401, "STORE_AUTH_REFRESH_INVALID", null, "no-refresh"));
        }

        try
        {
            await _gate.WaitAsync(ct);
            var req = new { refreshToken = rt };
            var resp = await _http.PostAsJsonAsync("api/public/auth/refresh", req, ct);
            if (resp.IsSuccessStatusCode)
            {
                var payload = await resp.Content.ReadFromJsonAsync<StoreTokenPair>(cancellationToken: ct);
                if (payload != null)
                {
                    await _storage.SaveAsync(new StoreSession(payload.AccessToken, payload.RefreshToken), ct);
                    _logger.LogInformation("StoreAuth refresh ok");
                    return StoreAuthResult<StoreTokenPair>.Ok(payload);
                }
            }
            var problem = await ReadProblemAsync(resp, ct);
            if (resp.StatusCode == System.Net.HttpStatusCode.Unauthorized || resp.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                await _storage.ClearAsync(ct);
            }
            _logger.LogWarning("StoreAuth refresh {Code}", problem?.Code ?? "HTTP_" + (int)resp.StatusCode);
            return StoreAuthResult<StoreTokenPair>.Fail(problem ?? new StoreAuthProblem((int)resp.StatusCode, "STORE_AUTH_UPSTREAM", null, "empty"));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
        {
            _logger.LogWarning(ex, "StoreAuth refresh upstream unreachable");
            return StoreAuthResult<StoreTokenPair>.Fail(new StoreAuthProblem(503, "STORE_AUTH_UPSTREAM", null, ex.GetType().Name));
        }
        finally
        {
            _gate.Release();
        }
    }

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        var session = await _storage.LoadAsync<StoreSession>(ct);
        var at = session?.AccessToken;

        if (!string.IsNullOrEmpty(at))
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, "api/public/auth/logout");
                req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", at);
                req.Content = JsonContent.Create(new { refreshToken = session?.RefreshToken });
                await _http.SendAsync(req, ct);
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                _logger.LogDebug(ex, "StoreAuth logout upstream error ignorado");
            }
        }
        await _storage.ClearAsync(ct);
        _logger.LogInformation("StoreAuth logout local");
    }

    public async Task<ClaimsPrincipal?> GetUserAsync(CancellationToken ct = default)
    {
        var session = await _storage.LoadAsync<StoreSession>(ct);
        if (session == null) return null;
        var principal = StoreJwt.Parse(session.AccessToken);
        if (principal == null || principal.IsExpired())
        {
            var refreshed = await RefreshAsync(ct);
            if (!refreshed.Success) return null;
            session = await _storage.LoadAsync<StoreSession>(ct);
            principal = session != null ? StoreJwt.Parse(session.AccessToken) : null;
        }
        return principal;
    }

    private static async Task<StoreAuthProblem?> ReadProblemAsync(HttpResponseMessage resp, CancellationToken ct)
    {
        try
        {
            var json = await resp.Content.ReadAsStringAsync(ct);
            if (string.IsNullOrWhiteSpace(json)) return null;
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var root = doc.RootElement;
            var code = root.TryGetProperty("code", out var c) ? c.GetString() : null;
            var corr = root.TryGetProperty("correlationId", out var ci) ? ci.GetString() : null;
            var up = root.TryGetProperty("upstream", out var u) ? u.GetString() : null;
            return new StoreAuthProblem((int)resp.StatusCode, code, corr, up);
        }
        catch
        {
            return null;
        }
    }
}