using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/configuracion")]
public class ConfiguracionController : ControllerBase
{
    private readonly IConfiguracionServiceAsync _cfgSvc;
    public ConfiguracionController(IConfiguracionServiceAsync cfgSvc) => _cfgSvc = cfgSvc;

    [HttpGet]
    [RequierePermiso("INVENTA_CONFIGURACION_PRINCIPAL_VER", "INVENTA_CONFIGURACION_TENANT_VER")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var r = await _cfgSvc.GetAsync(ct);
        return Ok(r.Value);
    }

    [HttpPut]
    [RequierePermiso("INVENTA_CONFIGURACION_PRINCIPAL_EDITAR", "INVENTA_CONFIGURACION_TENANT_EDITAR")]
    public async Task<IActionResult> Update([FromBody] UpdateConfigDto dto, CancellationToken ct)
    {
        var r = await _cfgSvc.GetAsync(ct);
        var cfg = r.Value;
        if (cfg == null) return NotFound();
        cfg.PassPlatApiBaseUrl = dto.PassPlatApiBaseUrl;
        cfg.PassPlatTimeoutMs = dto.PassPlatTimeoutMs;
        cfg.PassPlatAmbiente = dto.PassPlatAmbiente;
        cfg.Activo = dto.Activo;
        cfg.FecMod = DateTime.UtcNow;
        if (HttpContext.Items.TryGetValue("IdUsuario", out var u)) cfg.UsuarioMod = (int?)u;
        var up = await _cfgSvc.UpdateAsync(cfg, ct);
        return Ok(up.Value);
    }
}

// S57.24: validación explícita derivada de contrato (nvarchar(500)/varchar(20)) + Blazor UpdateConfiguracionDto
// (Required/Url/Range 1000-120000 ya existentes en UI). Sin regex ni límites nuevos.
public record UpdateConfigDto(
    [Required, MaxLength(500), Url] string PassPlatApiBaseUrl,
    [Range(1000, 120000)] int PassPlatTimeoutMs,
    [Required, MaxLength(20)] string PassPlatAmbiente,
    bool Activo);

