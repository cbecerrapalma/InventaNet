using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Inventa.WebAPI.Authentication;

/// <summary>
/// Validates JWT tid -> Tenant.IdTenantPassPlat -> local IdTenant, syncs Usuario/UsuarioTenant.
/// No local passwords/roles/MFA.
/// </summary>
public class TenantUserSyncMiddlewareHandler
{
    private readonly ITenantSyncServiceAsync _tenantSync;
    private readonly IUsuarioSyncService _usuarioSync;
    private readonly IUsuarioTenantSyncService _utSync;
    private readonly ILogger<TenantUserSyncMiddlewareHandler> _logger;

    public TenantUserSyncMiddlewareHandler(
        ITenantSyncServiceAsync tenantSync,
        IUsuarioSyncService usuarioSync,
        IUsuarioTenantSyncService utSync,
        ILogger<TenantUserSyncMiddlewareHandler> logger)
    {
        _tenantSync = tenantSync;
        _usuarioSync = usuarioSync;
        _utSync = utSync;
        _logger = logger;
    }

    public async Task HandleAsync(TokenValidatedContext ctx)
    {
        var principal = ctx.Principal;
        if (principal == null) { ctx.Fail("No principal"); return; }

        var sub = principal.FindFirstValue("sub") ?? principal.FindFirstValue(ClaimTypes.NameIdentifier);
        var tid = principal.FindFirstValue("tid") ?? principal.FindFirstValue("TenantId") ?? principal.FindFirstValue("http://schemas.microsoft.com/identity/claims/tenantid");
        var email = principal.FindFirstValue("email") ?? principal.FindFirstValue(ClaimTypes.Email);
        var name = principal.FindFirstValue("name") ?? principal.FindFirstValue(ClaimTypes.Name);
        var nomUsuario = principal.FindFirstValue("preferred_username") ?? name ?? $"user_{sub}";

        if (!int.TryParse(sub, out var idUsuario))
        {
            _logger.LogWarning("JWT sub not int: {Sub}", sub);
            ctx.Fail("Invalid sub");
            return;
        }
        if (!int.TryParse(tid, out var idTenantPassPlat))
        {
            _logger.LogWarning("JWT tid not int: {Tid}", tid);
            ctx.Fail("Invalid tid");
            return;
        }

        // Tenant sync: JWT tid -> local IdTenant
        var tenantResult = await _tenantSync.ResolveLocalTenantIdAsync(idTenantPassPlat);
        int idTenantLocal;
        if (!tenantResult.IsSuccess)
        {
            // create mirror if not exists
            var create = await _tenantSync.EnsureTenantAsync(idTenantPassPlat, $"T{idTenantPassPlat}", $"Tenant {idTenantPassPlat}");
            if (!create.IsSuccess) { ctx.Fail("Tenant sync failed"); return; }
            idTenantLocal = create.Value.IdTenant;
        }
        else idTenantLocal = tenantResult.Value;

        // Usuario mirror
        var parts = (name ?? "").Split(' ', 2);
        await _usuarioSync.EnsureUsuarioAsync(idUsuario, nomUsuario, email, parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1), true, false);

        // UsuarioTenant mirror idempotent
        await _utSync.EnsureAsync(idUsuario, idTenantLocal, true, false);

        // enrich HttpContext with local tenant id for controllers
        ctx.HttpContext.Items["IdTenantLocal"] = idTenantLocal;
        ctx.HttpContext.Items["IdUsuario"] = idUsuario;
    }
}
