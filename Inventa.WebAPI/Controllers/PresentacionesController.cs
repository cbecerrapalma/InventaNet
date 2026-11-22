using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

/// <summary>S117 (P19): buscador de presentaciones de «Nueva venta» y «Nueva orden»; S131: también «Transferir» y «Ajustar inventario».</summary>
[ApiController]
[Route("api/productos/presentaciones")]
[Authorize]
public class PresentacionesController : ControllerBase
{
    private readonly IPresProdServiceAsync _svc;
    public PresentacionesController(IPresProdServiceAsync svc) => _svc = svc;

    /// <summary>Presentaciones activas del tenant por código o nombre de producto o variante, o código de barras.
    /// <c>uso=compra</c> trae las de compra (si no, las de venta); con <c>idBodega</c>, el disponible en esa bodega.</summary>
    [HttpGet("search")]
    [RequierePermiso("INVENTA_VENTAS_PRINCIPAL_CREAR", "INVENTA_COMPRAS_ORDENES_CREAR", "INVENTA_INVENTARIO_TRANSFERENCIAS_TRANSFERIR", "INVENTA_INVENTARIO_AJUSTES_AJUSTAR")]
    public async Task<IActionResult> Search([FromQuery] string? q, [FromQuery] string? uso, [FromQuery] int? idBodega, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var p = Math.Max(page ?? 1, 1);
        var ps = Math.Clamp(pageSize ?? 10, 1, 50);
        var paraVenta = !string.Equals(uso, "compra", StringComparison.OrdinalIgnoreCase);
        var r = await _svc.SearchPagedByTenantAsync(idTenant, q, paraVenta, idBodega, p, ps, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(PagedResponse<Inventa.Data.Async.Repositories.PresentacionBusquedaRow>.From(r.Value.Items, p, ps, r.Value.Total));
    }
}
