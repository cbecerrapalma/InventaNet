using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/proveedores")]
[Authorize]
public class ProveedoresController : ControllerBase
{
    private readonly ICatalogoServiceAsync _svc;
    public ProveedoresController(ICatalogoServiceAsync svc) => _svc = svc;

    [HttpGet]
    [RequierePermiso("INVENTA_PROVEEDORES_PRINCIPAL_VER", "INVENTA_PROVEEDORES_DETALLE_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER")]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] bool? activo, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q) || activo.HasValue)
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetProveedoresPagedAsync(idTenant, q, activo, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Domain.Entities.Proveedor>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetProveedoresAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:int}")]
    [RequierePermiso("INVENTA_PROVEEDORES_PRINCIPAL_VER", "INVENTA_PROVEEDORES_DETALLE_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetProveedorByIdAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPost]
    [RequierePermiso("INVENTA_PROVEEDORES_PRINCIPAL_EDITAR", "INVENTA_PROVEEDORES_DETALLE_EDITAR")]
    public async Task<IActionResult> Create([FromBody] CreateProveedorDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.CreateProveedorAsync(idTenant, dto.Codigo, dto.RazonSocial, ct);
        return r.IsSuccess ? CreatedAtAction(nameof(List), new { id = r.Value.IdProveedor }, r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPut("{id:int}")]
    [RequierePermiso("INVENTA_PROVEEDORES_PRINCIPAL_EDITAR", "INVENTA_PROVEEDORES_DETALLE_EDITAR")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProveedorDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.UpdateProveedorAsync(id, idTenant, dto.Codigo, dto.RazonSocial, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
public record CreateProveedorDto(string Codigo, string RazonSocial);


