using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/ventas")]
[Authorize]
public class VentasController : ControllerBase
{
    private readonly IVentaServiceAsync _ventaSvc;
    public VentasController(IVentaServiceAsync ventaSvc) => _ventaSvc = ventaSvc;

    [HttpPost]
    [RequierePermiso("INVENTA_VENTAS_PRINCIPAL_CREAR")]
    public async Task<IActionResult> Registrar([FromBody] RegistrarVentaDto dto, CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var t) || !HttpContext.Items.TryGetValue("IdUsuario", out var u))
            return Unauthorized();
        var req = new RegistrarVentaInput((int)t!, dto.IdBodega, dto.NumVenta, dto.IdCliente, dto.TipoDoc, (int)u!,
            dto.Items.Select(i => new VentaItemInput(i.IdPresProd, i.IdLote, i.Cantidad, i.PrecioUnitario, i.Descuento)).ToList(),
            dto.Pagos.Select(p => new VentaPagoInput(p.IdMetodoPago, p.Monto, p.Referencia)).ToList());
        var result = await _ventaSvc.RegistrarVentaAsync(req, ct);
        if (!result.IsSuccess) return StatusCode(result.Error!.HttpStatusCode ?? 400, result.Error);
        return Ok(result.Value);
    }

    [HttpPost("{id}/anular")]
    [RequierePermiso("INVENTA_VENTAS_DETALLE_ANULAR")]
    public async Task<IActionResult> Anular(long id, [FromBody] AnularDto dto, CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var t) || !HttpContext.Items.TryGetValue("IdUsuario", out var u))
            return Unauthorized();
        var req = new AnularVentaInput((int)t!, id, dto.Motivo, (int)u!);
        var result = await _ventaSvc.AnularVentaAsync(req, ct);
        if (!result.IsSuccess) return StatusCode(result.Error!.HttpStatusCode ?? 400, result.Error);
        return Ok();
    }

    [HttpGet]
    [RequierePermiso("INVENTA_VENTAS_PRINCIPAL_VER", "INVENTA_VENTAS_DETALLE_VER")]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _ventaSvc.SearchPagedAsync(idTenant, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Data.Async.Repositories.VentaListadoRow>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _ventaSvc.GetRecentAsync(idTenant, 100, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:long}")]
    [RequierePermiso("INVENTA_VENTAS_PRINCIPAL_VER", "INVENTA_VENTAS_DETALLE_VER")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ventaSvc.GetFichaAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }
}

public record VentaItemDto(long IdPresProd, long? IdLote, decimal Cantidad, decimal PrecioUnitario, decimal Descuento = 0m);
public record VentaPagoDto(byte IdMetodoPago, decimal Monto, string? Referencia = null);
public record RegistrarVentaDto(int IdBodega, string NumVenta, int? IdCliente, byte TipoDoc, List<VentaItemDto> Items, List<VentaPagoDto> Pagos);
public record AnularDto(string Motivo);


