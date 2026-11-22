using Inventa.Application.Async.Services;
using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IPassPlatClient _passPlat;
    private readonly ITenantSyncServiceAsync _sync;
    private readonly ITenantServiceAsync _tenants;
    public AuthController(IPassPlatClient passPlat, ITenantSyncServiceAsync sync, ITenantServiceAsync tenants)
        => (_passPlat, _sync, _tenants) = (passPlat, sync, tenants);

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] BackofficeLoginRequest req, CancellationToken ct)
    {
        var idApp = req.IdApp;
        if (!string.IsNullOrWhiteSpace(req.AppCodigo))
        {
            // S84.4: la app llega por código desde configuración en solo lectura; se resuelve a IdApp numérico.
            var apps = await _passPlat.GetActivasAsync(ct);
            var app = apps.Success
                ? apps.Data?.FirstOrDefault(a => a.Activa && string.Equals(a.Codigo, req.AppCodigo, StringComparison.OrdinalIgnoreCase))
                : null;
            if (app is null)
                return StatusCode(503, new { error = "AUTH_APP_NO_CONFIGURADA" });
            idApp = app.Id;
        }
        var result = await _passPlat.LoginAsync(new LoginRequest(
            req.NomUsuario, req.Email, idApp, req.Password,
            req.IdTenant, req.IdDisp, req.IdIP, req.IdAgente), ct);
        if (!result.Success)
        {
        // S96 D2: PassPlatClient devuelve "PassPlat login <status> <codigo>"
        var msg = result.Error ?? "";
        int status = 0;
        string code = "";
        var parts = msg.Split(' ', 4);
        if (parts.Length >= 4 && int.TryParse(parts[2], out var st))
        {
            status = st;
            code = parts[3];
        }
        else
        {
            // fallback: legacy format contained "Unauthorized"/"Forbidden"
            if (msg.Contains("Unauthorized", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("LOGIN_FAILED", StringComparison.OrdinalIgnoreCase))
                return StatusCode(401, new { codigo = "LOGIN_CREDENCIALES_INVALIDAS", mensaje = "Credenciales inválidas. Verifique usuario, contraseña y tenant." });
            if (msg.Contains("Forbidden", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("SUSPENDED", StringComparison.OrdinalIgnoreCase)
                || msg.Contains("BLOQUEADO", StringComparison.OrdinalIgnoreCase))
                return StatusCode(403, new { codigo = "LOGIN_TENANT_BLOQUEADO", mensaje = "Tenant bloqueado o sin acceso para este usuario." });
            return StatusCode(503, new { error = "AUTH_UPSTREAM" });
        }

        // S96 D2: PWD_EXPIRADA → 403 LOGIN_PWD_EXPIRADA
        if (string.Equals(code, "PWD_EXPIRADA", StringComparison.OrdinalIgnoreCase))
            return StatusCode(403, new { codigo = "LOGIN_PWD_EXPIRADA", mensaje = "Tu contraseña venció. Restablécela desde PassPlat o pide a un administrador que la cambie." });

        if (status == 401)
            return StatusCode(401, new { codigo = "LOGIN_CREDENCIALES_INVALIDAS", mensaje = "Credenciales inválidas. Verifique usuario, contraseña y tenant." });

        if (status == 403)
            return StatusCode(403, new { codigo = "LOGIN_TENANT_BLOQUEADO", mensaje = "Tenant bloqueado o sin acceso para este usuario." });

        return StatusCode(503, new { error = "AUTH_UPSTREAM" });
        }
        return Ok(result.Data);
    }

    [HttpGet("mis-tenants")]
    [Authorize]
    public async Task<IActionResult> MisTenants(CancellationToken ct)
    {
        var token = Request.Headers.Authorization.ToString().Replace("Bearer ", "");
        var result = await _passPlat.GetMisTenantsAsync(token, ct);
        if (!result.Success || result.Data is null) return StatusCode(503, new { error = result.Error });
        // S85: enriquece cada tenant PassPlat con su espejo local
        // (tid -> IdTenantPassPlat -> IdTenant). Nunca emite 0 como idTenant.
        var list = new List<object>(result.Data.Count);
        foreach (var t in result.Data)
        {
            var ppId = t.IdTenant;
            int localId = 0;
            string? nombreLocal = null;
            if (ppId > 0)
            {
                var resolved = await _sync.ResolveLocalTenantIdAsync(ppId, ct);
                if (resolved.IsSuccess && resolved.Value > 0)
                {
                    localId = resolved.Value;
                    var full = await _tenants.GetByIdAsync(localId, ct);
                    if (full.IsSuccess && full.Value is not null)
                        nombreLocal = full.Value.Nombre;
                }
            }
            list.Add(new
            {
                idTenant = localId > 0 ? localId : ppId,
                idTenantPassPlat = ppId,
                idTenantLocal = localId,
                codigo = t.Codigo,
                nombre = t.Nombre,
                nombreLocal
            });
        }
        return Ok(list);
    }

    /// <summary>
    /// Login backoffice. Retrocompatible: acepta IdApp numérico (legacy) o AppCodigo (S84.4).
    /// </summary>
    public sealed class BackofficeLoginRequest
    {
        public string? NomUsuario { get; set; }
        public string? Email { get; set; }
        public int IdApp { get; set; }
        public string? AppCodigo { get; set; }
        public string Password { get; set; } = "";
        public int IdTenant { get; set; }
        public string? IdDisp { get; set; }
        public string? IdIP { get; set; }
        public string? IdAgente { get; set; }
    }
}

