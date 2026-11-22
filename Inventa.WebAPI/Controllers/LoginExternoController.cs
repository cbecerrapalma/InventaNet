using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PassPlat.Client.Models;
using PassPlat.Client.Services;

namespace Inventa.WebAPI.Controllers;

/// <summary>
/// S127 D2 — puente del login externo del backoffice hacia PassPlat (SDK <see cref="IPassPlatLoginExterno"/>).
/// La App se resuelve por código, como en <see cref="AuthController"/>; PassPlat vuelve a <c>Apps.UrlBase</c> + <c>/signin-callback</c>.
/// </summary>
[ApiController]
[Route("api/auth/externo")]
[AllowAnonymous]
[EnableRateLimiting("Public")]
public class LoginExternoController : ControllerBase
{
    private readonly IPassPlatClient _passPlat;
    private readonly IPassPlatLoginExterno _loginExterno;

    public LoginExternoController(IPassPlatClient passPlat, IPassPlatLoginExterno loginExterno)
        => (_passPlat, _loginExterno) = (passPlat, loginExterno);

    [HttpGet("proveedores")]
    public async Task<IActionResult> Proveedores([FromQuery] string? appCodigo, [FromQuery] int idTenantPassPlat, CancellationToken ct)
    {
        if (idTenantPassPlat <= 0)
            return Ok(Array.Empty<ProveedorLoginExternoDto>());

        var idApp = await ResolverIdAppAsync(appCodigo, ct);
        if (idApp is null)
            return AppNoConfigurada();

        var r = await _loginExterno.ObtenerProveedoresLoginAsync(idTenantPassPlat, idApp.Value, ct);
        return Ok(r.IsSuccess ? r.Value : Array.Empty<ProveedorLoginExternoDto>());
    }

    [HttpGet("{proveedor}/authorize")]
    public async Task<IActionResult> Autorizar(string proveedor, [FromQuery] string? appCodigo, [FromQuery] int idTenantPassPlat, CancellationToken ct)
    {
        var idApp = await ResolverIdAppAsync(appCodigo, ct);
        if (idApp is null)
            return AppNoConfigurada();

        var r = await _loginExterno.IniciarLoginExternoAsync(proveedor, idTenantPassPlat, idApp.Value, null, ct);
        return r.IsSuccess ? Ok(new { authorizationUrl = r.Value }) : Fallo(r.Error!.Code, r.Error.Message);
    }

    [HttpPost("canje")]
    public async Task<IActionResult> Canjear([FromBody] CanjeLoginExternoRequest req, CancellationToken ct)
    {
        var idApp = await ResolverIdAppAsync(req.AppCodigo, ct);
        if (idApp is null)
            return AppNoConfigurada();

        var r = await _loginExterno.CanjearCodigoAsync(req.Codigo, idApp.Value, ct);
        return r.IsSuccess ? Ok(r.Value) : Fallo(r.Error!.Code, r.Error.Message);
    }

    private async Task<int?> ResolverIdAppAsync(string? appCodigo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(appCodigo))
            return null;

        var apps = await _passPlat.GetActivasAsync(ct);
        var app = apps.Success
            ? apps.Data?.FirstOrDefault(a => a.Activa && string.Equals(a.Codigo, appCodigo, StringComparison.OrdinalIgnoreCase))
            : null;
        return app?.Id;
    }

    private ObjectResult AppNoConfigurada() => StatusCode(503, new { error = "AUTH_APP_NO_CONFIGURADA" });

    private IActionResult Fallo(string codigo, string mensaje)
        => codigo == CodigosLoginExterno.NoDisponible
            ? StatusCode(503, new { codigo, mensaje })
            : BadRequest(new { codigo, mensaje });

    public sealed class CanjeLoginExternoRequest
    {
        public string Codigo { get; set; } = "";
        public string? AppCodigo { get; set; }
    }
}
