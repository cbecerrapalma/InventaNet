using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Net.Http.Headers;

namespace Inventa.WebAPI.Controllers;

// S67: auth del Store (compradores). App y tenant los fija el SERVIDOR
// (StoreAuth:AppCodigo + PublicCatalog:PassPlatTenantId); lo que el cuerpo
// traiga fuera de {identificador,password} / {refreshToken} se ignora.
// Proxy fino a PassPlat: nunca emite JWT local ni toca espejos/DB.
// Errores: ProblemDetails + extensiones code/correlationId/upstream.
[ApiController]
[Route("api/public/auth")]
[AllowAnonymous]
[EnableRateLimiting("Public")]
public class StoreAuthController : ControllerBase
{
    public record StoreLoginRequest(string? Identificador, string? Password);
    public record StoreRefreshRequest(string? RefreshToken);
    public record StoreLogoutRequest(string? RefreshToken);

    private const string DefaultAppCodigo = "INVENTA_STORE";

    private readonly IPassPlatClient _passPlat;
    private readonly IConfiguration _cfg;
    private readonly ILogger<StoreAuthController> _logger;

    public StoreAuthController(IPassPlatClient passPlat, IConfiguration cfg, ILogger<StoreAuthController> logger)
    {
        _passPlat = passPlat;
        _cfg = cfg;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] StoreLoginRequest req, CancellationToken ct)
    {
        NoStore();
        var id = (req.Identificador ?? "").Trim();
        var pwd = req.Password ?? "";
        if (id.Length == 0) return Fail(400, "STORE_AUTH_VALIDATION", "Identificador requerido.", null);
        if (id.Length > 256) return Fail(400, "STORE_AUTH_VALIDATION", "Identificador demasiado largo.", null);
        if (pwd.Length == 0) return Fail(400, "STORE_AUTH_VALIDATION", "Password requerido.", null);
        if (pwd.Length > 128) return Fail(400, "STORE_AUTH_VALIDATION", "Password demasiado largo.", null);

        var resolved = await ResolveAppTenantAsync(ct);
        if (resolved.Failure != null) return resolved.Failure;
        var isEmail = id.Contains('@');
        var result = await _passPlat.LoginStoreAsync(
            new LoginRequest(isEmail ? null : id, isEmail ? id : null, resolved.IdApp, pwd, resolved.IdTenant, null, null, null), ct);
        if (result.Success)
        {
            var d = result.Data!;
            if (d.RequiereMfa) return Fail(403, "STORE_AUTH_MFA_REQUIRED", "La cuenta requiere verificación en un segundo factor. Complete el acceso en el canal habilitado.", null);
            if (d.RequiereCambioPwd) return Fail(403, "STORE_AUTH_PASSWORD_CHANGE_REQUIRED", "Debe actualizar su contraseña en el canal habilitado antes de continuar.", null);
            _logger.LogInformation("StoreAuth login ok trace {Trace}", HttpContext.TraceIdentifier);
            return Ok(new { accessToken = d.AccessToken, refreshToken = d.RefreshToken });
        }
        var e = result.Error!;
        return e.StatusCode switch
        {
            400 => Fail(503, "STORE_AUTH_NOT_CONFIGURED", "Autenticación del Store no configurada.", e.Codigo),
            401 => Fail(401, "STORE_AUTH_INVALID_CREDENTIALS", "Usuario o contraseña incorrectos.", e.Codigo),
            403 => Fail(403, "STORE_AUTH_FORBIDDEN", "Acceso denegado para esta aplicación.", e.Codigo),
            429 => Fail(429, "STORE_AUTH_RATE_LIMITED", "Demasiadas solicitudes. Reintente en unos segundos.", e.Codigo, e.RetryAfterSeconds ?? 10),
            _ => Fail(503, "STORE_AUTH_UPSTREAM", "Servicio de identidad no disponible. Reintente más tarde.", e.Codigo),
        };
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] StoreRefreshRequest req, CancellationToken ct)
    {
        NoStore();
        var rt = req.RefreshToken ?? "";
        if (rt.Length == 0) return Fail(400, "STORE_AUTH_VALIDATION", "RefreshToken requerido.", null);
        if (rt.Length > 2048) return Fail(400, "STORE_AUTH_VALIDATION", "RefreshToken demasiado largo.", null);

        var result = await _passPlat.RefreshStoreAsync(rt, ct);
        if (result.Success)
        {
            _logger.LogInformation("StoreAuth refresh ok trace {Trace}", HttpContext.TraceIdentifier);
            return Ok(new { accessToken = result.Data!.AccessToken, refreshToken = result.Data.RefreshToken });
        }
        var e = result.Error!;
        return e.StatusCode switch
        {
            400 or 401 or 404 => Fail(401, "STORE_AUTH_REFRESH_INVALID", "Sesión expirada o inválida. Inicie sesión nuevamente.", e.Codigo),
            429 => Fail(429, "STORE_AUTH_RATE_LIMITED", "Demasiadas solicitudes. Reintente en unos segundos.", e.Codigo, e.RetryAfterSeconds ?? 10),
            _ => Fail(503, "STORE_AUTH_UPSTREAM", "Servicio de identidad no disponible. Reintente más tarde.", e.Codigo),
        };
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] StoreLogoutRequest? req, CancellationToken ct)
    {
        NoStore();
        _ = req; // {refreshToken?} aceptado por compatibilidad de contrato; la revocación es por jti del Bearer.
        var bearer = BearerToken();
        if (bearer == null)
        {
            _logger.LogInformation("StoreAuth logout local trace {Trace}", HttpContext.TraceIdentifier);
            return NoContent();
        }
        var result = await _passPlat.LogoutStoreAsync(bearer, ct);
        if (result.Success)
        {
            _logger.LogInformation("StoreAuth logout ok trace {Trace}", HttpContext.TraceIdentifier);
            return NoContent();
        }
        var e = result.Error!;
        // Idempotente: Bearer inválido/expirado = estado final ya alcanzado.
        if (e.StatusCode is 400 or 401 or 404)
        {
            _logger.LogInformation("StoreAuth logout idempotente trace {Trace} upstream {Codigo}", HttpContext.TraceIdentifier, e.Codigo);
            return NoContent();
        }
        return Fail(503, "STORE_AUTH_UPSTREAM", "Servicio de identidad no disponible. Reintente más tarde.", e.Codigo);
    }

    // Resuelve IdApp por codigo (GET activas anonimo) + IdTenant por config.
    // Falla cerrado: sin config o sin match -> Failure 503 listo para retornar.
    private async Task<(int IdApp, int IdTenant, IActionResult? Failure)> ResolveAppTenantAsync(CancellationToken ct)
    {
        var codigo = (_cfg["StoreAuth:AppCodigo"] ?? "").Trim();
        if (codigo.Length == 0) codigo = DefaultAppCodigo;
        var rawTenant = _cfg["PublicCatalog:PassPlatTenantId"];
        if (!int.TryParse(rawTenant, out var idTenant) || idTenant <= 0)
        {
            _logger.LogWarning("StoreAuth sin tenant trace {Trace}", HttpContext.TraceIdentifier);
            return (0, 0, Fail(503, "STORE_AUTH_NOT_CONFIGURED", "Autenticación del Store no configurada.", null));
        }
        var apps = await _passPlat.GetActivasAsync(ct);
        if (!apps.Success || apps.Data == null)
        {
            _logger.LogWarning("StoreAuth activas inalcanzable trace {Trace}", HttpContext.TraceIdentifier);
            return (0, 0, Fail(503, "STORE_AUTH_UPSTREAM", "Servicio de identidad no disponible. Reintente más tarde.", null));
        }
        var app = apps.Data.FirstOrDefault(a => string.Equals(a.Codigo, codigo, StringComparison.OrdinalIgnoreCase) && a.Activa);
        if (app == null)
        {
            _logger.LogWarning("StoreAuth app {Codigo} no encontrada trace {Trace}", codigo, HttpContext.TraceIdentifier);
            return (0, 0, Fail(503, "STORE_AUTH_NOT_CONFIGURED", "Autenticación del Store no configurada.", null));
        }
        return (app.Id, idTenant, null);
    }

    private string? BearerToken()
    {
        var h = Request.Headers.Authorization.ToString();
        if (h.Length == 0) return null;
        const string prefix = "Bearer ";
        if (!h.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
        var token = h.Substring(prefix.Length).Trim();
        return token.Length == 0 ? null : token;
    }

    private void NoStore() => Response.Headers[HeaderNames.CacheControl] = "no-store";

    private IActionResult Fail(int status, string code, string detail, string? upstream, int? retryAfter = null)
    {
        _logger.LogWarning("StoreAuth {Code} trace {Trace} upstream {Upstream}",
            code, HttpContext.TraceIdentifier, upstream ?? "-");
        if (retryAfter.HasValue) Response.Headers.RetryAfter = retryAfter.Value.ToString();
        var problem = new ProblemDetails
        {
            Status = status,
            Title = status switch
            {
                400 => "Solicitud inválida",
                401 => "No autenticado",
                403 => "Acceso denegado",
                429 => "Demasiadas solicitudes",
                _ => "Servicio no disponible",
            },
            Detail = detail,
            Instance = HttpContext.TraceIdentifier,
        };
        problem.Extensions["code"] = code;
        problem.Extensions["correlationId"] = HttpContext.TraceIdentifier;
        if (!string.IsNullOrEmpty(upstream)) problem.Extensions["upstream"] = upstream;
        return StatusCode(status, problem);
    }
}
