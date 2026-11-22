using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize]
public class ClientesController : ControllerBase
{
    private readonly ICatalogoServiceAsync _svc;
    public ClientesController(ICatalogoServiceAsync svc) => _svc = svc;

    [HttpGet]
    [RequierePermiso("INVENTA_CLIENTES_PRINCIPAL_VER", "INVENTA_CLIENTES_DETALLE_VER", "INVENTA_VENTAS_PRINCIPAL_VER", "INVENTA_VENTAS_DETALLE_VER")]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] bool? activo, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q) || activo.HasValue)
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetClientesPagedAsync(idTenant, q, activo, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Domain.Entities.Cliente>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetClientesAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost]
    [RequierePermiso("INVENTA_CLIENTES_PRINCIPAL_EDITAR", "INVENTA_CLIENTES_DETALLE_EDITAR")]
    public async Task<IActionResult> Create([FromBody] CreateClienteDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.CreateClienteAsync(idTenant, dto.Codigo, dto.Nombre, dto.RUT, ct);
        return r.IsSuccess ? CreatedAtAction(nameof(List), new { id = r.Value.IdCliente }, r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:int}")]
    [RequierePermiso("INVENTA_CLIENTES_PRINCIPAL_VER", "INVENTA_CLIENTES_DETALLE_VER", "INVENTA_VENTAS_PRINCIPAL_VER", "INVENTA_VENTAS_DETALLE_VER")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetClienteByIdAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPut("{id:int}")]
    [RequierePermiso("INVENTA_CLIENTES_PRINCIPAL_EDITAR", "INVENTA_CLIENTES_DETALLE_EDITAR")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateClienteDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.UpdateClienteAsync(id, idTenant, dto.Codigo, dto.Nombre, dto.RUT, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
public record CreateClienteDto(string Codigo, string Nombre, string? RUT);


