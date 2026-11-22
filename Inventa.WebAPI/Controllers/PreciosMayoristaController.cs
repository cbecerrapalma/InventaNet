using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S57.69 B-7: superficie admin para PrecioMayorista (relación dedicada, sin discriminator).
// Consume SOLO Application.Async (nunca repositorios). Mayorista NO se expone en catálogo público.
[ApiController]
[Route("api/precios-mayorista")]
[RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_VER")]
public class PreciosMayoristaController : ControllerBase
{
    private readonly IPrecioMayoristaServiceAsync _svc;

    public PreciosMayoristaController(IPrecioMayoristaServiceAsync svc) => _svc = svc;

    [HttpGet]
    public async Task<IActionResult> GetByPresentacion([FromQuery] long idPresProd, [FromQuery] bool historial = false, CancellationToken ct = default)
    {
        if (idPresProd <= 0) return BadRequest(new { error = "idPresProd debe ser >0" });
        var r = historial
            ? await _svc.GetHistorialAsync(idPresProd, ct)
            : await _svc.GetByPresentacionAsync(idPresProd, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value.Select(PrecioMayoristaDto.FromEntity));
    }

    [HttpGet("vigente")]
    public async Task<IActionResult> GetVigente([FromQuery] long idPresProd, CancellationToken ct = default)
    {
        if (idPresProd <= 0) return BadRequest(new { error = "idPresProd debe ser >0" });
        var r = await _svc.GetVigenteAsync(idPresProd, null, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value is null ? NotFound() : Ok(PrecioMayoristaDto.FromEntity(r.Value));
    }

    // Escritura vigencia: INSERT nueva FecVig, no UPDATE histórico (IX_PrecioMayorista_Fast por FecVig DESC).
    [HttpPost]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> Create([FromBody] UpsertPrecioMayoristaDto dto, CancellationToken ct = default)
    {
        var r = await _svc.CreateAsync(dto.IdPresProd, dto.Valor, dto.FecVig, true, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return CreatedAtAction(nameof(GetByPresentacion), new { idPresProd = r.Value.IdPresProd }, PrecioMayoristaDto.FromResult(r.Value));
    }

    [HttpPut("{id:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> Update(long id, [FromBody] UpsertPrecioMayoristaDto dto, CancellationToken ct = default)
    {
        var r = await _svc.UpdateAsync(id, dto.Valor, dto.FecVig, true, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(PrecioMayoristaDto.FromResult(r.Value));
    }

    [HttpDelete("{id:long}")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> Delete(long id, CancellationToken ct = default)
    {
        var r = await _svc.DeleteAsync(id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return NoContent();
    }

    [HttpPost("{id:long}/deactivate")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> Deactivate(long id, CancellationToken ct = default)
    {
        var r = await _svc.DeactivateAsync(id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(PrecioMayoristaDto.FromResult(r.Value));
    }

    [HttpPost("{id:long}/reactivate")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> Reactivate(long id, CancellationToken ct = default)
    {
        var r = await _svc.ReactivateAsync(id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(PrecioMayoristaDto.FromResult(r.Value));
    }
}

public sealed record PrecioMayoristaDto(long IdPrecioMayorista, long IdPresProd, decimal Valor, DateOnly FecVig, bool Activo)
{
    public static PrecioMayoristaDto FromResult(PrecioMayoristaResult r) =>
        new(r.IdPrecioMayorista, r.IdPresProd, r.Valor, r.FecVig, r.Activo);
    public static PrecioMayoristaDto FromEntity(Inventa.Domain.Entities.PrecioMayorista e) =>
        new(e.IdPrecioMayorista, e.IdPresProd, e.Valor, e.FecVig, e.Activo);
}

public sealed record UpsertPrecioMayoristaDto(long IdPresProd, decimal Valor, DateOnly FecVig);
