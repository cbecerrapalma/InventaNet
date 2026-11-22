using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/categorias")]
[Authorize]
public class CategoriasController : ControllerBase
{
    private readonly ICatalogoServiceAsync _svc;
    public CategoriasController(ICatalogoServiceAsync svc) => _svc = svc;

    [HttpGet]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> List(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetCategoriasAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_CREAR")]
    public async Task<IActionResult> Create([FromBody] CreateCategoriaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.CreateCategoriaAsync(idTenant, dto.Codigo, dto.Nombre, dto.IdPadre, dto.Orden, dto.ImagenUrl, ct);
        return r.IsSuccess ? CreatedAtAction(nameof(List), new { id = r.Value.IdCategoria }, r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:long}")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_VER", "INVENTA_CATALOGOS_MARCAS_VER", "INVENTA_CATALOGOS_UNIDADES_VER", "INVENTA_CATALOGOS_METODOS_PAGO_VER", "INVENTA_CATALOGOS_ATRIBUTOS_VER", "INVENTA_VENTAS_PRINCIPAL_VER")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetCategoriaByIdAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    [HttpPut("{id:long}")]
    [RequierePermiso("INVENTA_CATALOGOS_CATEGORIAS_EDITAR")]
    public async Task<IActionResult> Update(long id, [FromBody] CreateCategoriaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.UpdateCategoriaAsync(id, idTenant, dto.Codigo, dto.Nombre, dto.IdPadre, dto.Orden, dto.ImagenUrl, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
// S69: hierarchy fields appended optional (backoffice compat: old clients send none => root).
public record CreateCategoriaDto(string Codigo, string Nombre, long? IdPadre = null, short Orden = 0, string? ImagenUrl = null);


