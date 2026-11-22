using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Precios;
using Inventa.Domain.Entities;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S57.36 §9/§11: controller aislado — consume SOLO Application.Async (nunca repositorios).
// S93.1: página «Precios y ofertas» (grilla, resumen, historial, cambios en lote). La escritura de
// Precio/Oferta pasa SIEMPRE por IGestionPreciosServiceAsync (también el POST antiguo).
[ApiController]
[Route("api/precios")]
[RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER")]
public class PreciosController : ControllerBase
{
    private readonly IPrecioServiceAsync _precioSvc;
    private readonly IPrecioGrillaServiceAsync _grillaSvc;
    private readonly IGestionPreciosServiceAsync _gestionSvc;

    public PreciosController(IPrecioServiceAsync precioSvc,
        IPrecioGrillaServiceAsync grillaSvc, IGestionPreciosServiceAsync gestionSvc)
    {
        _precioSvc = precioSvc;
        _grillaSvc = grillaSvc;
        _gestionSvc = gestionSvc;
    }

    private int IdTenant => (int)HttpContext.Items["IdTenantLocal"]!;

    [HttpGet]
    public async Task<IActionResult> GetPrecios([FromQuery] long? idPresProd, [FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = IdTenant;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _precioSvc.SearchPagedByTenantAsync(idTenant, idPresProd, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var resp = PagedResponse<Precio>.From(rPaged.Value.Items, p, ps, rPaged.Value.Total);
            return Ok(resp);
        }
        var r = await _precioSvc.GetByPresProdOrRecentAsync(idPresProd, 100, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    // S93.1 (D6): los precios de compra se retiraron. 410 para clientes antiguos.
    [HttpGet("compra")]
    public IActionResult GetPreciosCompra() => PreciosCompraRetirados();

    // S93.1 K4: grilla con filtros en el servidor.
    [HttpGet("grilla")]
    public async Task<IActionResult> GetGrilla(
        [FromQuery] string? q, [FromQuery] long? idCategoria, [FromQuery] int? idMarca,
        [FromQuery] EstadoFiltroPrecios estado = EstadoFiltroPrecios.Todos, [FromQuery] bool incluirInactivos = false,
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20, CancellationToken ct = default)
    {
        var r = await _grillaSvc.GetPaginaAsync(IdTenant, new GrillaPreciosConsulta(q, idCategoria, idMarca, estado, incluirInactivos, page, pageSize), ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpGet("resumen")]
    public async Task<IActionResult> GetResumen(CancellationToken ct)
    {
        var r = await _grillaSvc.GetResumenAsync(IdTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpGet("presentaciones/{idPresProd:long}/historial")]
    public async Task<IActionResult> GetHistorial(long idPresProd, CancellationToken ct)
    {
        var r = await _grillaSvc.GetHistorialAsync(IdTenant, idPresProd, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value is null) return NotFound();
        return Ok(r.Value);
    }

    // S93.1 K3/K4: lote de cambios. simular=true calcula sin escribir; simular=false aplica todo o nada.
    // 200 = aplicado o simulado; 422 = errores por fila (no se escribió nada); 409 = datos desactualizados.
    [HttpPost("cambios")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> PostCambios([FromBody] CambiosPreciosRequest body, [FromQuery] bool simular = false, CancellationToken ct = default)
    {
        var cambios = body?.Cambios ?? [];
        var r = simular
            ? await _gestionSvc.SimularAsync(IdTenant, cambios, ct)
            : await _gestionSvc.AplicarAsync(IdTenant, cambios, ct);
        return ResultadoPrecios.AResultado(this, r, simular);
    }

    // Escritura antigua de una vigencia (contrato S57.69). S93.1: delega en el único escritor y valida el tenant (H5).
    [HttpPost]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> UpsertPrecio([FromBody] UpsertPrecioDto dto, CancellationToken ct)
    {
        var r = await _gestionSvc.AplicarAsync(IdTenant,
            [new CambioPrecio(dto.IdPresProd, TipoCambioPrecio.PrecioOficial, Valor: dto.Valor, Desde: dto.FecVig, VerificarVistos: false)], ct);
        if (!r.IsSuccess || r.Value.HayErrores || r.Value.HayConflicto || r.Value.PreciosCreados.Count == 0)
            return ResultadoPrecios.AResultado(this, r, simular: false);
        var creado = new Precio { IdPrecio = r.Value.PreciosCreados[0], IdPresProd = dto.IdPresProd, Valor = dto.Valor, FecVig = dto.FecVig, Activo = true };
        return CreatedAtAction(nameof(GetPrecios), new { idPresProd = dto.IdPresProd }, creado);
    }

    [HttpPost("compra")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public IActionResult UpsertPrecioCompra() => PreciosCompraRetirados();

    private ObjectResult PreciosCompraRetirados()
    {
        var pd = new ProblemDetails
        {
            Status = StatusCodes.Status410Gone,
            Title = "Precios de compra retirados",
            Detail = "Los precios de compra se retiraron en S93; el costo es el costo promedio de la variante.",
            Instance = HttpContext.Request.Path
        };
        pd.Extensions["code"] = "PRECIOS_COMPRA_RETIRADOS";
        return StatusCode(StatusCodes.Status410Gone, pd);
    }
}

public record UpsertPrecioDto(long IdPresProd, decimal Valor, DateOnly FecVig);
public sealed record CambiosPreciosRequest(IReadOnlyList<CambioPrecio> Cambios);

/// <summary>S93.1: traduce el resultado de un lote de precios a HTTP (200/409/422) con ProblemDetails en español.</summary>
internal static class ResultadoPrecios
{
    public static IActionResult AResultado(ControllerBase c, CBP.Results.Result<ResultadoCambiosPreciosDto> r, bool simular)
    {
        if (!r.IsSuccess) return c.StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(c.HttpContext));
        var v = r.Value;
        if (v.HayConflicto)
        {
            var pd = new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Precios desactualizados",
                Detail = "Otra persona cambió el precio u oferta de algunas variantes. Recargue y revise antes de guardar.",
                Instance = c.HttpContext.Request.Path
            };
            pd.Extensions["code"] = CodigosPrecio.PrecioDesactualizado;
            pd.Extensions["desactualizados"] = v.Desactualizados;
            return c.StatusCode(StatusCodes.Status409Conflict, pd);
        }
        if (v.HayErrores && !simular)
        {
            var pd = new ProblemDetails
            {
                Status = StatusCodes.Status422UnprocessableEntity,
                Title = "Hay cambios con errores",
                Detail = "No se guardó ningún cambio. Corrija las filas marcadas.",
                Instance = c.HttpContext.Request.Path
            };
            pd.Extensions["errores"] = v.Errores;
            return c.StatusCode(StatusCodes.Status422UnprocessableEntity, pd);
        }
        return c.Ok(v);
    }
}
