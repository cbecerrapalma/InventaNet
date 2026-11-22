using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services.Importacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

/// <summary>S87.2 C3/C4: importación de productos (ADR-PROD-002 §3). Sin escritura de catálogo. S88: aplicación con escritura catálogo + stock.</summary>
[ApiController]
[Route("api/importaciones")]
[RequierePermiso("INVENTA_IMPORTACIONES_PRINCIPAL_CREAR", "INVENTA_IMPORTACIONES_NUEVA_CREAR", "INVENTA_IMPORTACIONES_DETALLE_CREAR")]
public class ImportacionesController : ControllerBase
{
    private const long MaxBytes = 10L * 1024 * 1024;
    private readonly IImportacionServiceAsync _svc;
    private readonly IProductoImportacionAplicadorAsync _aplicador;

    public ImportacionesController(IImportacionServiceAsync svc, IProductoImportacionAplicadorAsync aplicador)
    {
        _svc = svc;
        _aplicador = aplicador;
    }

    // GET api/importaciones/plantillas/productos
    [HttpGet("plantillas/productos")]
    public async Task<IActionResult> DescargarPlantillaProductos(CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetPlantillaProductosAsync(idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return File(r.Value.Contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", r.Value.NombreArchivo);
    }

    // POST api/importaciones/productos — valida y crea el lote (201).
    [HttpPost("productos")]
    [RequestSizeLimit(MaxBytes)]
    public async Task<IActionResult> ValidarProductos(IFormFile file, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var idUsuario = (int)HttpContext.Items["IdUsuario"]!;
        if (file == null || file.Length == 0)
            return BadRequest(new { status = 400, title = "Solicitud inválida", detail = "No se proporcionó archivo." });

        byte[] contenido;
        using (var ms = new MemoryStream())
        {
            await file.CopyToAsync(ms, ct);
            contenido = ms.ToArray();
        }
        var r = await _svc.ValidarProductosAsync(idTenant, idUsuario, file.FileName, contenido, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return StatusCode(201, r.Value);
    }

    // GET api/importaciones/{id}
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetLote(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetLoteAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    // GET api/importaciones/{id}/reporte
    [HttpGet("{id:long}/reporte")]
    public async Task<IActionResult> GetReporte(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetReporteAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return File(r.Value.Contenido, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", r.Value.NombreArchivo);
    }

    // GET api/importaciones/{id}/cambios?accion=&page=&pageSize= — vista previa / auditoría de cambios (S91.4)
    [HttpGet("{id:long}/cambios")]
    public async Task<IActionResult> GetCambios(long id, [FromQuery] string? accion, [FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetCambiosAsync(id, idTenant, accion, page, pageSize, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(new { disponible = r.Value.Disponible, items = r.Value.Items, total = r.Value.Total, page = r.Value.Page, pageSize = r.Value.PageSize });
    }

    // POST api/importaciones/{id}/aplicar — aplica lote validado (S88)
    [HttpPost("{id:long}/aplicar")]
    public async Task<IActionResult> Aplicar(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var idUsuario = (int)HttpContext.Items["IdUsuario"]!;
        var r = await _aplicador.AplicarAsync(id, idTenant, idUsuario, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    // GET api/importaciones?page=&pageSize=
    [HttpGet]
    public async Task<IActionResult> Listar([FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.ListarLotesAsync(idTenant, page, pageSize, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(new { items = r.Value.Items, total = r.Value.Total, page, pageSize });
    }
}
