using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public class TenantsController : ControllerBase
{
    private readonly ITenantServiceAsync _tenantSvc;
    private readonly IConfiguracionServiceAsync _cfgSvc;
    public TenantsController(ITenantServiceAsync tenantSvc, IConfiguracionServiceAsync cfgSvc)
    {
        _tenantSvc = tenantSvc;
        _cfgSvc = cfgSvc;
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        // tenant-aware: return only caller tenants via ITenantServiceAsync filtered internally
        var r = await _tenantSvc.GetAllAsync(ct);
        return Ok(r.Value);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var r = await _tenantSvc.GetByIdAsync(id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpGet("{id:int}/config")]
    [RequierePermiso("INVENTA_CONFIGURACION_PRINCIPAL_VER", "INVENTA_CONFIGURACION_TENANT_VER")]
    public async Task<IActionResult> GetConfig(int id, CancellationToken ct)
    {
        // S57.24 hardening: sin lectura cross-tenant (misma regla que UpdateConfig).
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var local) || (int)local! != id)
            return StatusCode(403, new { codigo = "CROSS_TENANT", mensaje = "No puede leer configuración de otro tenant" });
        var r = await _cfgSvc.GetTenantConfigAsync(id, ct);
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPut("{id:int}/config")]
    [RequierePermiso("INVENTA_CONFIGURACION_PRINCIPAL_EDITAR", "INVENTA_CONFIGURACION_TENANT_EDITAR")]
    public async Task<IActionResult> UpdateConfig(int id, [FromBody] UpdateTenantConfigDto dto, CancellationToken ct)
    {
        // tenant isolation: route id must equal current tenant from X-Tenant-Id / JWT
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var local) || (int)local! != id)
            return StatusCode(403, new { codigo = "CROSS_TENANT", mensaje = "No puede editar configuración de otro tenant" });
        if (dto.MaxBodegas < 1 || dto.MaxBodegas > 100)
            return StatusCode(400, new CBP.Results.Errors.Error("VALIDATION", "MaxBodegas debe estar entre 1 y 100").WithHttpStatus(400).ToProblemDetails(HttpContext));
        var moneda = (dto.Moneda ?? "").Trim().ToUpperInvariant();
        if (string.IsNullOrWhiteSpace(moneda) || moneda.Length != 3)
            return StatusCode(400, new CBP.Results.Errors.Error("VALIDATION", "Moneda debe ser código de 3 letras").WithHttpStatus(400).ToProblemDetails(HttpContext));
        if (dto.TasaImpuesto < 0m || dto.TasaImpuesto > 1m)
            return StatusCode(400, new CBP.Results.Errors.Error("VALIDATION", "TasaImpuesto debe estar entre 0 y 1").WithHttpStatus(400).ToProblemDetails(HttpContext));
        var r = await _cfgSvc.GetTenantConfigAsync(id, ct);
        var cfg = r.Value;
        if (cfg == null) return NotFound();
        cfg.MaxBodegas = dto.MaxBodegas;
        cfg.PermiteStockNegativo = dto.PermiteStockNegativo;
        cfg.Moneda = moneda;
        cfg.TasaImpuesto = dto.TasaImpuesto;
        if (dto.PermiteVentaSinCliente.HasValue) cfg.PermiteVentaSinCliente = dto.PermiteVentaSinCliente.Value;
        // DiasRetencionMov intentionally NOT updated — readonly by design (see S55.2)
        if (HttpContext.Items.TryGetValue("IdUsuario", out var u)) cfg.UsuarioMod = (int?)u;
        cfg.FecMod = DateTime.UtcNow;
        var up = await _cfgSvc.UpsertTenantConfigAsync(cfg, ct);
        return Ok(up.Value);
    }
}
public record UpdateTenantConfigDto(short MaxBodegas, bool PermiteStockNegativo, string Moneda, decimal TasaImpuesto, bool? PermiteVentaSinCliente);

