using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/catalogos")]
[Authorize]
public class CatalogosController : ControllerBase
{
    private readonly ICatalogoServiceAsync _svc;
    public CatalogosController(ICatalogoServiceAsync svc) => _svc = svc;

    [HttpGet("marcas")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> GetMarcas(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetMarcasAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("categorias")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> GetCategorias(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetCategoriasAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("unidades")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> GetUnidades(CancellationToken ct)
    {
        var r = await _svc.GetUnidadesAsync(ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("metodos-pago")]
    public async Task<IActionResult> GetMetodosPago(CancellationToken ct)
    {
        var r = await _svc.GetMetodosPagoAsync(ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("atributos")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> GetAtributos([FromQuery] int idCategoria, CancellationToken ct)
    {
        var r = await _svc.GetAtriDefsAsync(idCategoria, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost("marcas")]
    [RequierePermiso("INVENTA_CATALOGOS_MARCAS_CREAR")]
    public async Task<IActionResult> CreateMarca([FromBody] CreateMarcaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.CreateMarcaAsync(idTenant, dto.Codigo, dto.Nombre, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPut("marcas/{id:int}")]
    [RequierePermiso("INVENTA_CATALOGOS_MARCAS_EDITAR")]
    public async Task<IActionResult> UpdateMarca(int id, [FromBody] CreateMarcaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.UpdateMarcaAsync(id, idTenant, dto.Codigo, dto.Nombre, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
public record CreateMarcaDto(string Codigo, string Nombre);


