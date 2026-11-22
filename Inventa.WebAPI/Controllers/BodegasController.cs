using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/bodegas")]
[Authorize]
public class BodegasController : ControllerBase
{
    private readonly IInventarioServiceAsync _svc;
    public BodegasController(IInventarioServiceAsync svc) => _svc = svc;

    [HttpGet]
    [RequierePermiso("INVENTA_BODEGAS_PRINCIPAL_VER", "INVENTA_BODEGAS_DETALLE_VER", "INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetBodegasPagedAsync(idTenant, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Domain.Entities.Bodega>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetBodegasAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost]
    [RequierePermiso("INVENTA_BODEGAS_PRINCIPAL_ADMIN", "INVENTA_BODEGAS_DETALLE_ADMIN")]
    public async Task<IActionResult> Create([FromBody] CreateBodegaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.CreateBodegaAsync(idTenant, dto.Codigo, dto.Nombre, ct);
        return r.IsSuccess ? CreatedAtAction(nameof(List), new { id = r.Value.IdBodega }, r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:int}")]
    [RequierePermiso("INVENTA_BODEGAS_PRINCIPAL_VER", "INVENTA_BODEGAS_DETALLE_VER", "INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetBodegaByIdAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPut("{id:int}")]
    [RequierePermiso("INVENTA_BODEGAS_PRINCIPAL_ADMIN", "INVENTA_BODEGAS_DETALLE_ADMIN")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateBodegaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.UpdateBodegaAsync(id, idTenant, dto.Codigo, dto.Nombre, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
public record CreateBodegaDto(string Codigo, string Nombre);


