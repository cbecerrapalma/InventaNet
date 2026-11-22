using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Inventa.WebAPI.Clients;

public interface IPassPlatClient
{
    Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default);
    Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default);
    Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default);
    Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default);
    // S67: contrato Store (api/public/auth). App/tenant los fija el servidor; el cliente nunca los elige.
    Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default);
    Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default);
    Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default);
    Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default);
    // S79: envío de email por plantilla (usuario técnico).
    Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest req, CancellationToken ct = default);
}

public record EnviarEmailPlantillaRequest(
    string CodigoPlantilla,
    string Destinatario,
    string? NombreDestinatario = null,
    Dictionary<string, string>? Variables = null,
    string Cultura = "es");

public record EnviarEmailPlantillaResponse(
    long? IdEmailLog,
    string TrackingId);

public record LoginRequest(string? NomUsuario, string? Email, int IdApp, string Password, int IdTenant, string? IdDisp, string? IdIP, string? IdAgente);
public record LoginResponse(string AccessToken, string RefreshToken, DateTime ExpiresAt, int IdUsuario, int IdTenant, string NomUsuario);
public record TenantDto(int IdTenant, string Codigo, string Nombre)
{
    // PassPlat emite "id" (no "idTenant") en mis-tenants: alias real que converge a IdTenant.
    [JsonPropertyName("id")]
    public int Id { get => IdTenant; init => IdTenant = value; }
}
public record AppDto(int Id, string Codigo, string Nombre, string? UrlBase, bool Activa);
    public record ResultDto<T>(bool Success, T? Data, string? Error, int? RetryAfterSeconds = null);

// S67: resultado estructurado del proxy Store. StatusCode = HTTP upstream
// (0 = inalcanzable); Codigo = campo codigo/code de PassPlat o marcador
// UPSTREAM_* (nunca cuerpos ni secretos).
public record StoreAuthError(int StatusCode, string Codigo, int? RetryAfterSeconds = null);
public record StoreAuthResult<T>(bool Success, T? Data, StoreAuthError? Error)
{
    public static StoreAuthResult<T> Ok(T data) => new(true, data, null);
    public static StoreAuthResult<T> Fail(StoreAuthError error) => new(false, default, error);
}
// Tokens del Store + flags de flujo (MFA / cambio de password requerido).
public record StoreLoginData(string? AccessToken, string? RefreshToken, bool RequiereMfa, bool RequiereCambioPwd);

public class PassPlatClient : IPassPlatClient
{
    private readonly HttpClient _http;
    private readonly ILogger<PassPlatClient> _logger;
    public PassPlatClient(HttpClient http, ILogger<PassPlatClient> logger)
    {
        _http = http;
        _logger = logger;
    }

    public async Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("/api/auth/login", req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                var code = ExtractErrorCode(raw);
                return new ResultDto<LoginResponse>(false, default, $"PassPlat login {(int)resp.StatusCode} {code}");
            }
            // PassPlat returns direct LoginResponse, not wrapped ResultDto
            try
            {
                var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var direct = JsonSerializer.Deserialize<LoginResponse>(raw, opts);
                if (direct != null && !string.IsNullOrEmpty(direct.AccessToken))
                    return new ResultDto<LoginResponse>(true, direct, null);
            }
            catch { }
            var data = JsonSerializer.Deserialize<ResultDto<LoginResponse>>(raw, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            if (data != null && data.Success && data.Data != null) return data;
            if (data != null && data.Data != null) return new ResultDto<LoginResponse>(true, data.Data, null);
            return data ?? new ResultDto<LoginResponse>(false, default, "Empty response: " + raw.Substring(0, Math.Min(500, raw.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PassPlat unreachable");
            return new ResultDto<LoginResponse>(false, default, "PassPlat unavailable: " + ex.Message);
        }
    }

    private static string ExtractErrorCode(string raw)
    {
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            foreach (var name in new[] { "codigo", "code", "error" })
            {
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var v)
                    && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                    return v.GetString()!;
            }
        }
        catch (JsonException) { }
        return "UPSTREAM_ERROR";
    }

    public async Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, "/api/auth/mis-tenants");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                return new ResultDto<List<TenantDto>>(false, default, $"mis-tenants {resp.StatusCode} {err}");
            }
            var raw = await resp.Content.ReadAsStringAsync(ct);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            // PassPlat returns direct array, not wrapper
            try
            {
                var direct = JsonSerializer.Deserialize<List<TenantDto>>(raw, opts);
                if (direct != null) return new ResultDto<List<TenantDto>>(true, direct, null);
            }
            catch { }
            var wrapped = JsonSerializer.Deserialize<ResultDto<List<TenantDto>>>(raw, opts);
            if (wrapped != null && wrapped.Data != null) return new ResultDto<List<TenantDto>>(true, wrapped.Data, null);
            return wrapped ?? new ResultDto<List<TenantDto>>(false, default, "Empty: " + raw.Substring(0, Math.Min(500, raw.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PassPlat mis-tenants unreachable");
            return new ResultDto<List<TenantDto>>(false, default, "PassPlat unavailable: " + ex.Message);
        }
    }

    public async Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/permisosefectivos/usuario/{idUsuario}?idTenant={idTenant}&idApp={idApp}");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                return new ResultDto<List<string>>(false, default, $"permisos {resp.StatusCode} {err}");
            }
            var raw = await resp.Content.ReadAsStringAsync(ct);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var direct = JsonSerializer.Deserialize<List<string>>(raw, opts);
                if (direct != null) return new ResultDto<List<string>>(true, direct, null);
            }
            catch { }
            var wrapped = JsonSerializer.Deserialize<ResultDto<List<string>>>(raw, opts);
            if (wrapped != null && wrapped.Data != null) return new ResultDto<List<string>>(true, wrapped.Data, null);
            return wrapped ?? new ResultDto<List<string>>(false, default, "Empty: " + raw.Substring(0, Math.Min(500, raw.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PassPlat permisos unreachable");
            return new ResultDto<List<string>>(false, default, "PassPlat unavailable: " + ex.Message);
        }
    }

    // S67: login del Store. 200 con tokens -> Success; 200 con desafio MFA
    // (requiereMFA/idMFAPrincipal, sin access) -> Success con RequiereMfa;
    // 200 con ReqCambioPwd -> Success con flag; 4xx/5xx -> Failure tipado.
    // Nunca se registran cuerpos (pueden traer tokens); solo status + codigo.
    public async Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.PostAsJsonAsync("/api/auth/login", req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return StoreAuthResult<StoreLoginData>.Fail(ReadStoreError(resp, raw));
            var payload = JsonSerializer.Deserialize<LoginPayload>(raw, JsonOpts);
            if (payload == null)
                return StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError((int)resp.StatusCode, "UPSTREAM_MALFORMED"));
            var mfa = payload.RequiereMFA || (payload.IdMFAPrincipal.HasValue && string.IsNullOrEmpty(payload.AccessToken));
            if (mfa)
                return StoreAuthResult<StoreLoginData>.Ok(new StoreLoginData(null, null, RequiereMfa: true, RequiereCambioPwd: false));
            if (string.IsNullOrEmpty(payload.AccessToken) || string.IsNullOrEmpty(payload.RefreshToken))
                return StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError((int)resp.StatusCode, "UPSTREAM_MALFORMED"));
            return StoreAuthResult<StoreLoginData>.Ok(new StoreLoginData(payload.AccessToken, payload.RefreshToken, RequiereMfa: false, payload.ReqCambioPwd));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("StoreAuth login upstream inalcanzable: {Tipo}", ex.GetType().Name);
            return StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE"));
        }
    }

    // S67: refresh del Store. PassPlat 200 -> tokens nuevos; cualquier 4xx
    // (401 INVALID_REFRESH/REFRESH_EXPIRED/...) -> Failure con status + codigo.
    public async Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.PostAsJsonAsync("/api/auth/refresh", new { refreshToken }, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return StoreAuthResult<LoginResponse>.Fail(ReadStoreError(resp, raw));
            var data = JsonSerializer.Deserialize<LoginResponse>(raw, JsonOpts);
            if (data == null || string.IsNullOrEmpty(data.AccessToken) || string.IsNullOrEmpty(data.RefreshToken))
                return StoreAuthResult<LoginResponse>.Fail(new StoreAuthError((int)resp.StatusCode, "UPSTREAM_MALFORMED"));
            return StoreAuthResult<LoginResponse>.Ok(data);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("StoreAuth refresh upstream inalcanzable: {Tipo}", ex.GetType().Name);
            return StoreAuthResult<LoginResponse>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE"));
        }
    }

    // S67: logout del Store. Requiere Bearer (PassPlat revoca por jti del access).
    // 200/204 -> true; 401 (Bearer invalido/expirado) se informa como Failure
    // 401 y el controlador lo vuelve 204 idempotente.
    public async Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
    {
        try
        {
            using var req = new HttpRequestMessage(HttpMethod.Post, "/api/auth/logout");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);
            using var resp = await _http.SendAsync(req, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
                return StoreAuthResult<bool>.Fail(ReadStoreError(resp, raw));
            return StoreAuthResult<bool>.Ok(true);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("StoreAuth logout upstream inalcanzable: {Tipo}", ex.GetType().Name);
            return StoreAuthResult<bool>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE"));
        }
    }

    // S67: catalogo de apps activas (endpoint anonimo). Permite resolver
    // Codigo -> Id numerico pre-login sin token. Falla cerrado si ausente.
    public async Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default)
    {
        try
        {
            using var resp = await _http.GetAsync("/api/apps/activas", ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                return new ResultDto<List<AppDto>>(false, default, $"activas {resp.StatusCode}");
            }
            var raw = await resp.Content.ReadAsStringAsync(ct);
            var direct = JsonSerializer.Deserialize<List<AppDto>>(raw, JsonOpts);
            if (direct != null) return new ResultDto<List<AppDto>>(true, direct, null);
            return new ResultDto<List<AppDto>>(false, default, "activas: respuesta vacia");
        }
        catch (Exception ex)
        {
            _logger.LogWarning("StoreAuth activas upstream inalcanzable: {Tipo}", ex.GetType().Name);
            return new ResultDto<List<AppDto>>(false, default, "PassPlat unavailable");
        }
    }

    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    // Extrae solo el campo codigo/code del cuerpo de error (nunca el mensaje:
    // puede traer detalle interno). Sin cuerpos en logs.
    private static StoreAuthError ReadStoreError(HttpResponseMessage resp, string raw)
    {
        int? retry = null;
        try
        {
            var ra = resp.Headers.RetryAfter;
            if (ra?.Delta.HasValue == true) retry = Math.Max(1, (int)ra.Delta.Value.TotalSeconds);
        }
        catch { }
        try
        {
            using var doc = JsonDocument.Parse(raw);
            var root = doc.RootElement;
            foreach (var name in new[] { "codigo", "code", "error" })
            {
                if (root.ValueKind == JsonValueKind.Object && root.TryGetProperty(name, out var v)
                    && v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                    return new StoreAuthError((int)resp.StatusCode, v.GetString()!, retry);
            }
        }
        catch (JsonException) { }
        return new StoreAuthError((int)resp.StatusCode, "UPSTREAM_ERROR", retry);
    }

    private sealed record LoginPayload(string? AccessToken, string? RefreshToken, bool RequiereMFA, int? IdMFAPrincipal, bool ReqCambioPwd);

    public async Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
    {
        try
        {
            var req = new HttpRequestMessage(HttpMethod.Get, $"/api/apps/codigo/{codigo}");
            req.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            var resp = await _http.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = await resp.Content.ReadAsStringAsync(ct);
                return new ResultDto<AppDto>(false, default, $"app {resp.StatusCode} {err}");
            }
            var raw = await resp.Content.ReadAsStringAsync(ct);
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var direct = JsonSerializer.Deserialize<AppDto>(raw, opts);
                if (direct != null && direct.Id != 0) return new ResultDto<AppDto>(true, direct, null);
            }
            catch { }
            var wrapped = JsonSerializer.Deserialize<ResultDto<AppDto>>(raw, opts);
            if (wrapped != null && wrapped.Data != null) return new ResultDto<AppDto>(true, wrapped.Data, null);
            return wrapped ?? new ResultDto<AppDto>(false, default, "Empty: " + raw.Substring(0, Math.Min(500, raw.Length)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PassPlat app unreachable");
            return new ResultDto<AppDto>(false, default, "PassPlat unavailable: " + ex.Message);
        }
    }

    // S79: envío de email por plantilla con usuario técnico (Bearer token).
    // Retorna 202 Accepted -> Success con response; 4xx/5xx -> Failure con status + codigo.
    public async Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest req, CancellationToken ct = default)
    {
        try
        {
            using var httpReq = new HttpRequestMessage(HttpMethod.Post, "/api/email/enviar-plantilla");
            httpReq.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            httpReq.Content = JsonContent.Create(req);
            using var resp = await _http.SendAsync(httpReq, ct);
            var raw = await resp.Content.ReadAsStringAsync(ct);
            if (!resp.IsSuccessStatusCode)
            {
                var err = ReadStoreError(resp, raw);
                return new ResultDto<EnviarEmailPlantillaResponse>(false, default, $"enviar-plantilla {err.StatusCode} {err.Codigo}", err.RetryAfterSeconds);
            }
            var opts = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            try
            {
                var direct = JsonSerializer.Deserialize<EnviarEmailPlantillaResponse>(raw, opts);
                if (direct != null) return new ResultDto<EnviarEmailPlantillaResponse>(true, direct, null);
            }
            catch { }
            return new ResultDto<EnviarEmailPlantillaResponse>(false, default, "enviar-plantilla: respuesta vacía");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            _logger.LogWarning("PassPlat enviar-plantilla upstream inalcanzable: {Tipo}", ex.GetType().Name);
            return new ResultDto<EnviarEmailPlantillaResponse>(false, default, "PassPlat unavailable: " + ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando email plantilla");
            return new ResultDto<EnviarEmailPlantillaResponse>(false, default, "PassPlat error: " + ex.Message);
        }
    }
}
