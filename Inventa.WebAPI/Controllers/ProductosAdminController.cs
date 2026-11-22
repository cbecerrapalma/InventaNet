using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

/// <summary>
/// S92: lectura para la administración de productos (ficha, listado con filtros en el servidor,
/// resumen y diagnóstico de publicación). Capa HTTP delgada: todo se delega en IProductoFichaServiceAsync.
/// </summary>
[ApiController]
[Route("api/productos")]
[RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER")]
public class ProductosAdminController : ControllerBase
{
    private readonly IProductoFichaServiceAsync _ficha;

    public ProductosAdminController(IProductoFichaServiceAsync ficha) => _ficha = ficha;

    // GET api/productos/{id}/ficha — una sola llamada con variantes, códigos, precios, oferta, mayorista y stock.
    [HttpGet("{id:long}/ficha")]
    public async Task<IActionResult> GetFicha(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ficha.GetFichaAsync(idTenant, id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value is null ? NotFound() : Ok(r.Value);
    }

    // GET api/productos/listado?q=&estado=&precio=con|sin&imagen=con|sin&marca=con|sin&idCategoria=&activo=&page=&pageSize=
    [HttpGet("listado")]
    public async Task<IActionResult> Listado(
        [FromQuery] string? q, [FromQuery] string? estado, [FromQuery] string? precio, [FromQuery] string? imagen,
        [FromQuery] string? marca, [FromQuery] long? idCategoria, [FromQuery] bool? activo,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var filtro = new ProductoListadoConsulta(q, string.IsNullOrWhiteSpace(estado) ? null : estado,
            ConSin(precio), ConSin(imagen), ConSin(marca), idCategoria, activo);
        var r = await _ficha.ListarAsync(idTenant, filtro, page, pageSize, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        var ps = pageSize is 20 or 50 or 100 ? pageSize : 20;
        return Ok(PagedResponse<ProductoListadoItemDto>.From(r.Value.Items, page < 1 ? 1 : page, ps, r.Value.Total));
    }

    // GET api/productos/resumen — contadores del tenant completo.
    [HttpGet("resumen")]
    public async Task<IActionResult> Resumen(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ficha.GetResumenAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    // GET api/productos/publishability — S57.2; S92: devuelve el contrato completo (antes solo datos base).
    [HttpGet("publishability")]
    public async Task<IActionResult> GetPublishability(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ficha.GetPublishabilityAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    private static bool? ConSin(string? v) => v?.Trim().ToLowerInvariant() switch
    {
        "con" => true,
        "sin" => false,
        _ => null,
    };
}
