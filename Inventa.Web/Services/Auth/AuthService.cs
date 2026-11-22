using Inventa.Web.Models.Api;
using Inventa.Web.Models.Auth;
using Inventa.Web.Models.Tenant;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;
using Inventa.Web.Models;

namespace Inventa.Web.Services.Auth;

public sealed class AuthService
{
    private readonly ApiClient _api;
    private readonly ITokenStorage _storage;
    private readonly CustomAuthenticationStateProvider _authState;
    private readonly JwtClaimsReader _reader;
    private readonly AppSettings _settings;
    private readonly Tenant.TenantService _tenants;

    public AuthService(ApiClient api, ITokenStorage storage, CustomAuthenticationStateProvider authState, JwtClaimsReader reader, AppSettings settings, Tenant.TenantService tenants)
    {
        _api = api;
        _storage = storage;
        _authState = authState;
        _reader = reader;
        _settings = settings;
        _tenants = tenants;
    }

    public bool IsAuthenticated => _authState.IsAuthenticated;

    public async Task<LoginResponse> LoginAsync(string nomUsuario, string email, string password, int? idTenantPassPlat, CancellationToken ct = default)
    {
        var req = new
        {
            NomUsuario = string.IsNullOrWhiteSpace(nomUsuario) ? null : nomUsuario,
            Email = string.IsNullOrWhiteSpace(email) ? null : email,
            AppCodigo = _settings.AppCodigo,
            Password = password,
            IdTenant = idTenantPassPlat ?? 0,
            IdDisp = (string?)null,
            IdIP = (string?)null,
            IdAgente = (string?)null
        };

        // ApiClient throws ApiException on non-success -> caller handles ProblemDetails
        var resp = await _api.PostAsync<object, LoginResponse>("api/auth/login", req, ct);
        if (resp is null || string.IsNullOrWhiteSpace(resp.AccessToken))
            throw new InvalidOperationException("Respuesta de login inválida");

        return await CompletarSesionAsync(resp, idTenantPassPlat, ct);
    }

    /// <summary>
    /// S127 D3 — canjea el código de un solo uso del login externo (PassPlat vuelve a /signin-callback?codigo=…)
    /// y abre la misma sesión que el login con clave.
    /// </summary>
    public async Task<LoginResponse> CompletarLoginExternoAsync(string codigo, CancellationToken ct = default)
    {
        var req = new { Codigo = codigo, AppCodigo = _settings.AppCodigo };
        var resp = await _api.PostAsync<object, LoginResponse>("api/auth/externo/canje", req, ct);
        if (resp is null || string.IsNullOrWhiteSpace(resp.AccessToken))
            throw new InvalidOperationException("Respuesta de login inválida");

        return await CompletarSesionAsync(resp, null, ct);
    }

    private async Task<LoginResponse> CompletarSesionAsync(LoginResponse resp, int? idTenantPassPlat, CancellationToken ct)
    {
        // S87.2-H1: persistir la sesión PRIMERO para que las llamadas autenticadas
        // (mis-tenants) lleven bearer vía AuthenticatedHttpHandler. Antes, mis-tenants
        // se llamaba sin token → 401 → el auto-logout de ErrorHandler borraba la
        // sesión recién creada (carrera que dejaba sesión a medias / rebote a /login).
        _reader.SetToken(resp.AccessToken);
        var tid = _reader.GetTenantId(); // tid = IdTenantPassPlat
        var provisionalLocal = resp.IdTenant ?? tid ?? idTenantPassPlat ?? 0;
        var provisionalPP = tid ?? idTenantPassPlat ?? 0;
        await _storage.SetSessionAsync(
            resp.AccessToken,
            resp.RefreshToken,
            provisionalLocal > 0 ? provisionalLocal : null,
            provisionalPP > 0 ? provisionalPP : null,
            ct);

        // S85: obtener tenant LOCAL (IdTenant) via mis-tenants usando tid del JWT (IdTenantPassPlat)
        int effectiveLocalTenant = provisionalLocal;
        int effectivePassPlatTenant = provisionalPP;
        List<TenantItemDto>? loginTenants = null;
        try
        {
            loginTenants = await _api.GetAsync<List<TenantItemDto>>("api/auth/mis-tenants", ct);
            if (loginTenants != null && tid.HasValue)
            {
                var match = loginTenants.FirstOrDefault(t => t.IdTenantPassPlat == tid.Value);
                if (match != null)
                {
                    effectiveLocalTenant = match.IdTenant; // idTenant LOCAL (nunca 0 si existe espejo)
                    effectivePassPlatTenant = tid.Value;
                }
            }
        }
        catch (ApiException ex)
        {
            // La sesión queda intacta (token ya persistido): mensaje claro, sin sesión a medias.
            throw new InvalidOperationException(
                "Sesión iniciada, pero no se pudo resolver el tenant local (mis-tenants no respondió). Reintente el ingreso.", ex);
        }

        if (effectiveLocalTenant > 0 && effectivePassPlatTenant > 0)
        {
            await _storage.SetTenantIdsAsync(effectiveLocalTenant, effectivePassPlatTenant, ct);
        }
        else
        {
            await _storage.ClearTenantIdsAsync(ct);
        }
        await _authState.SetAuthenticationAsync(resp.AccessToken, ct);
        // S92.2-K7: la lista autoritativa recién pedida (1 petición) se deja en
        // caché bajo la sesión actual; TenantSelector/Dashboard ya no repiten.
        // El login sigue resolviendo el tenant local con llamada directa, no con la caché.
        if (loginTenants is not null)
            await _tenants.PrimeCacheAsync(loginTenants, ct);
        else
            _tenants.InvalidateCache();
        return resp;
    }

    public async Task RestoreSessionAsync(CancellationToken ct = default)
        => await _authState.InitializeAsync(ct);

    public async Task LogoutAsync(CancellationToken ct = default)
    {
        await _storage.ClearAsync(ct);
        // S92.2-K7: sin sesión no hay caché válida.
        _tenants.InvalidateCache();
        await _authState.ClearAuthenticationAsync();
    }

    public async Task<string?> GetCurrentUserAsync(CancellationToken ct = default)
    {
        var token = await _storage.GetAccessTokenAsync(ct);
        _reader.SetToken(token);
        return _reader.GetUserName();
    }

    public async Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken ct = default)
    {
        var token = await _storage.GetAccessTokenAsync(ct);
        _reader.SetToken(token);
        return _reader.GetPermissions();
    }
}
