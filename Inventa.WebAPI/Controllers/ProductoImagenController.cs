using Inventa.Application.Async.Services;
using Inventa.Domain.Entities;
using CBP.Results.Errors;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/productos/{idProd:long}/imagenes")]
public class ProductoImagenController : ControllerBase
{
    private readonly IProductoImagenServiceAsync _svc;
    private readonly IProductoImagenStorageServiceAsync _store;
    private readonly IProductoServiceAsync _prods;
    public ProductoImagenController(IProductoImagenServiceAsync svc, IProductoImagenStorageServiceAsync store, IProductoServiceAsync prods) => (_svc, _store, _prods) = (svc, store, prods);

    // S57.85-F7-01: la fachada (_svc) solo recibe idProd; verificar producto-en-tenant
    // aquí para List/SetPrincipal/Reorder/Delete (las subidas ya llevan scoping en _store).
    // S57.85-F7-01 / S57.88: guard vía fachada ProductoServiceAsync.GetByIdInTenantAsync
    // (método pareado S57.35-1:1 existente; sin acceso directo a Data). Wire HTTP idéntico.
    private async Task<IActionResult?> RequireProductInTenantAsync(long idProd, int idTenant, CancellationToken ct)
    {
        var r = await _prods.GetByIdInTenantAsync(idProd, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value is null) return StatusCode(404, new Error("PRODUCTO_NO_ENCONTRADO", "Producto no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404).ToProblemDetails(HttpContext));
        return null;
    }

    public sealed record IniciarSubidaDto(string? NombreArchivo, string? ContentType, long TamanoBytes);

    // S104: la API solo autoriza. Devuelve la URL firmada; el navegador envía los bytes directo al storage.
    // El scoping producto→tenant vive en el servicio (producto ajeno => 404 sin registrar ni firmar).
    [HttpPost("subidas")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> IniciarSubida(long idProd, [FromBody] IniciarSubidaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _store.IniciarSubidaAsync(idProd, idTenant, dto.NombreArchivo, dto.ContentType, dto.TamanoBytes, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    // S104: verifica el objeto subido (tamaño y firma binaria, sin descargarlo) y crea la imagen.
    [HttpPost("subidas/{idArchivo:long}/confirmar")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> ConfirmarSubida(long idProd, long idArchivo, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _store.ConfirmarSubidaAsync(idProd, idTenant, idArchivo, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return CreatedAtAction(nameof(List), new { idProd }, r.Value);
    }

    [HttpGet]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER")]
    public async Task<IActionResult> List(long idProd, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var denied = await RequireProductInTenantAsync(idProd, idTenant, ct);
        if (denied is not null) return denied;
        // S104: Url lista para el cliente (pública, o firmada de descarga si la imagen es privada).
        var r = await _store.GetActivesAsync(idProd, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpPut("{idImagen:int}/principal")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> SetPrincipal(long idProd, int idImagen, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var denied = await RequireProductInTenantAsync(idProd, idTenant, ct);
        if (denied is not null) return denied;
        var r = await _svc.SetPrincipalAsync(idProd, idImagen, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return NoContent();
    }

    public sealed record ReorderDto(IReadOnlyList<int> Ids);

    [HttpPut("reorder")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> Reorder(long idProd, [FromBody] ReorderDto dto, CancellationToken ct)
    {
        if (dto?.Ids == null) return BadRequest(new { error = "Ids requeridos" });
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var denied = await RequireProductInTenantAsync(idProd, idTenant, ct);
        if (denied is not null) return denied;
        var r = await _svc.ReorderAsync(idProd, dto.Ids, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        // S104: se responde con la misma vista que List (el marcador de imagen privada no sale de la API).
        var list = await _store.GetActivesAsync(idProd, idTenant, ct);
        if (!list.IsSuccess) return StatusCode(list.Error!.HttpStatusCode ?? 400, list.Error!.ToProblemDetails(HttpContext));
        return Ok(list.Value);
    }

    [HttpDelete("{idImagen:int}")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> Delete(long idProd, int idImagen, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var denied = await RequireProductInTenantAsync(idProd, idTenant, ct);
        if (denied is not null) return denied;
        // S104: fila + objeto del storage (best-effort) + Archivo.Estado = eliminado.
        var r = await _store.DeleteAsync(idProd, idTenant, idImagen, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return NoContent();
    }

    // S57.83-G1 / S104: reporte solo-lectura de huérfanos storage-vs-BD. Nunca borra.
    [HttpGet("/api/productos/imagenes-orphans")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> Orphans(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _store.FindOrphanFilesAsync(idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }
}


