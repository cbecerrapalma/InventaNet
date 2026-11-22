using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S57.66: promoción ADMIN-ONLY / PERSISTENCE-READY — consume SOLO Application.Async
// (nunca repositorios/DbContext). Sin Pricing Engine, sin POS, sin catálogo público.
[ApiController]
[Route("api/promociones")]
[RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_VER")]
public class PromocionesController : ControllerBase
{
    private readonly IPromocionServiceAsync _promoSvc;

    public PromocionesController(IPromocionServiceAsync promoSvc)
    {
        _promoSvc = promoSvc;
    }

    [HttpPost]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> CreatePromocion([FromBody] CreatePromocionDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.CreateAsync(idTenant, new CreatePromocionInput(
            dto.Codigo, dto.Nombre, dto.TextoComercial, dto.TipoMecanica, dto.PoliticaPrecio,
            dto.FechaDesde, dto.FechaHasta, dto.Prioridad, dto.Exclusiva, dto.Publicado), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return CreatedAtAction(nameof(GetPromocion), new { id = r.Value.IdPromocion }, r.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetPromociones(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.GetByTenantAsync(idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetPromocion(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.GetByIdAsync(idTenant, id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value == null) return NotFound();
        return Ok(r.Value);
    }

    [HttpPut("{id:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> UpdatePromocion(long id, [FromBody] UpdatePromocionDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.UpdateAsync(idTenant, id, new UpdatePromocionInput(
            dto.Nombre, dto.TextoComercial, dto.TipoMecanica, dto.PoliticaPrecio,
            dto.FechaDesde, dto.FechaHasta, dto.Prioridad, dto.Exclusiva, dto.Activo, dto.Publicado), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpDelete("{id:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> DeletePromocion(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.DeleteAsync(idTenant, id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return NoContent();
    }

    [HttpPost("{id:long}/presentaciones")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> AddPresentacion(long id, [FromBody] AddPresentacionDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.AddPresentacionAsync(idTenant, id, new AddPresentacionInput(
            dto.IdPresProd, dto.IdPrecioReferencia, dto.Rol, dto.Cantidad, dto.EsMuestra), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return StatusCode(201, r.Value);
    }

    [HttpPut("{id:long}/presentaciones/{idPart:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> UpdatePresentacion(long id, long idPart, [FromBody] UpdatePresentacionDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var check = await _promoSvc.GetByIdAsync(idTenant, id, ct);
        if (!check.IsSuccess) return StatusCode(check.Error!.HttpStatusCode ?? 400, check.Error!.ToProblemDetails(HttpContext));
        if (check.Value == null || check.Value.Presentaciones.All(p => p.IdPart != idPart)) return NotFound();
        var r = await _promoSvc.UpdatePresentacionAsync(idTenant, idPart, new UpdatePresentacionInput(dto.Rol, dto.Cantidad), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpDelete("{id:long}/presentaciones/{idPart:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> DeletePresentacion(long id, long idPart, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var check = await _promoSvc.GetByIdAsync(idTenant, id, ct);
        if (!check.IsSuccess) return StatusCode(check.Error!.HttpStatusCode ?? 400, check.Error!.ToProblemDetails(HttpContext));
        if (check.Value == null || check.Value.Presentaciones.All(p => p.IdPart != idPart)) return NotFound();
        var r = await _promoSvc.DeletePresentacionAsync(idTenant, idPart, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return NoContent();
    }

    [HttpPost("{id:long}/reglas")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> AddRegla(long id, [FromBody] AddReglaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.AddReglaAsync(idTenant, id, new AddReglaInput(dto.TipoRegla, dto.Operador, dto.Valor), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return StatusCode(201, r.Value);
    }

    [HttpPost("{id:long}/beneficios")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> AddBeneficio(long id, [FromBody] AddBeneficioDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _promoSvc.AddBeneficioAsync(idTenant, id, new AddBeneficioInput(dto.TipoBeneficio, dto.Valor), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return StatusCode(201, r.Value);
    }
}

public sealed record CreatePromocionDto(
    string Codigo,
    string Nombre,
    string? TextoComercial,
    byte TipoMecanica,
    byte PoliticaPrecio,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    short Prioridad = 100,
    bool Exclusiva = false,
    bool Publicado = false);

public sealed record UpdatePromocionDto(
    string Nombre,
    string? TextoComercial,
    byte TipoMecanica,
    byte PoliticaPrecio,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    short Prioridad,
    bool Exclusiva,
    bool Activo,
    bool Publicado);

public sealed record AddPresentacionDto(
    long IdPresProd,
    long? IdPrecioReferencia,
    byte Rol = 1,
    decimal Cantidad = 1,
    bool EsMuestra = false);

public sealed record UpdatePresentacionDto(byte Rol, decimal Cantidad);

public sealed record AddReglaDto(byte TipoRegla, byte Operador, string Valor);

public sealed record AddBeneficioDto(byte TipoBeneficio, decimal Valor);
