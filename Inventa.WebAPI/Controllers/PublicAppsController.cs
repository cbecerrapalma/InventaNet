using Inventa.WebAPI.Clients;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace Inventa.WebAPI.Controllers;

/// <summary>
/// Metadatos públicos de aplicación por código (S84.4: app desde configuración en solo lectura).
/// No expone IdApp numérico ni secretos. Solo lectura sobre el catálogo de apps activas de PassPlat.
/// </summary>
[ApiController]
[Route("api/public/apps")]
public class PublicAppsController : ControllerBase
{
    private readonly IPassPlatClient _passPlat;
    public PublicAppsController(IPassPlatClient passPlat) => _passPlat = passPlat;

    [HttpGet("{codigo}")]
    [AllowAnonymous]
    [EnableRateLimiting("Public")]
    public async Task<IActionResult> GetByCodigo(string codigo, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(codigo))
            return BadRequest(new { error = "Codigo requerido." });
        var result = await _passPlat.GetActivasAsync(ct);
        if (!result.Success)
            return StatusCode(503, new { error = "AUTH_UPSTREAM", detail = result.Error });
        var app = result.Data?.FirstOrDefault(a =>
            a.Activa && string.Equals(a.Codigo, codigo, StringComparison.OrdinalIgnoreCase));
        if (app is null)
            return NotFound(new { error = "APP_DESCONOCIDA" });
        return Ok(new { codigo = app.Codigo, nombre = app.Nombre });
    }
}
