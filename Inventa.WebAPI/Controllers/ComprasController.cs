using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/compras/ordenes")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly IOrdenCompraServiceAsync _svc;
    public ComprasController(IOrdenCompraServiceAsync svc) => _svc = svc;

    [HttpGet]
    [RequierePermiso("INVENTA_COMPRAS_ORDENES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER")]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.SearchPagedAsync(idTenant, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Data.Async.Repositories.OrdenCompraListadoRow>.From(paged.Items, p, ps, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetRecentAsync(idTenant, 100, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:long}")]
    [RequierePermiso("INVENTA_COMPRAS_ORDENES_VER", "INVENTA_COMPRAS_ORDENES_DETALLE_VER")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetFichaAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPost]
    [RequierePermiso("INVENTA_COMPRAS_ORDENES_CREAR")]
    public async Task<IActionResult> Create([FromBody] CreateOrdenDto dto, CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var t) || !HttpContext.Items.TryGetValue("IdUsuario", out var u))
            return Unauthorized();
        var detalles = dto.Detalles?.Select(d => (d.IdPresProd, d.Cantidad, d.PrecioUnitario)).ToList() ?? new List<(long IdPresProd, decimal Cantidad, decimal PrecioUnitario)>();
        var r = await _svc.CreateWithDetailsAsync((int)t!, dto.IdProveedor, dto.Fecha, dto.Observacion, (int)u!, dto.NumOrden ?? string.Empty, detalles, ct);
        return r.IsSuccess ? CreatedAtAction(nameof(Get), new { id = r.Value.IdOrdenCompra }, r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost("{id}/recepcion")]
    [RequierePermiso("INVENTA_COMPRAS_ORDENES_DETALLE_RECIBIR")]
    public async Task<IActionResult> Recepcion(long id, [FromBody] RecepcionDto dto, CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var t) || !HttpContext.Items.TryGetValue("IdUsuario", out var u))
            return Unauthorized();
        var items = dto.Items.Select(i => new Inventa.Data.Async.Repositories.OCRecepcionItemCmd(i.IdOCDetalle, i.IdLote, i.CantRecibida, i.CostoUnitario)).ToList();
        var req = new Inventa.Data.Async.Repositories.RecibirOrdenCompraCmd((int)t!, dto.IdBodega, id, items, (int)u!);
        var r = await _svc.RecibirOrdenCompraAsync(req, ct);
        return r.IsSuccess ? Ok() : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}

public record OCRecepcionItemDto(long IdOCDetalle, long? IdLote, decimal CantRecibida, decimal CostoUnitario);
public record RecepcionDto(int IdBodega, List<OCRecepcionItemDto> Items);
public record CreateOrdenDetalleRequestDto(long IdPresProd, decimal Cantidad, decimal PrecioUnitario);
public record CreateOrdenDto(int IdProveedor, DateOnly Fecha, string? Observacion, string? NumOrden, List<CreateOrdenDetalleRequestDto>? Detalles);


