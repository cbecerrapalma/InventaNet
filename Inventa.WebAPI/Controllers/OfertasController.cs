using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Precios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S57.52: oferta con referencia obligatoria — consume SOLO Application.Async (nunca repositorios).
[ApiController]
[Route("api/ofertas")]
[RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_VER")]
public class OfertasController : ControllerBase
{
    private readonly IOfertaServiceAsync _ofertaSvc;
    private readonly IPrecioReferenceResolverAsync _resolver;
    private readonly IGestionPreciosServiceAsync _gestionSvc;

    public OfertasController(IOfertaServiceAsync ofertaSvc, IPrecioReferenceResolverAsync resolver, IGestionPreciosServiceAsync gestionSvc)
    {
        _gestionSvc = gestionSvc;
        _ofertaSvc = ofertaSvc;
        _resolver = resolver;
    }

    // S93.1: crear oferta delega en el único escritor (GestionPreciosServiceAsync). La referencia la fija el
    // servidor (precio oficial vigente en FechaDesde, R2'); IdPrecioReferencia/PctReferencial/Prioridad/Publicado
    // del cuerpo se ignoran (D2: nace publicada; el % lo calcula el servidor). Respuesta: resultado del lote.
    [HttpPost]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> CreateOferta([FromBody] CreateOfertaDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _gestionSvc.AplicarAsync(idTenant,
            [new CambioPrecio(dto.IdPresProd, TipoCambioPrecio.Oferta, PrecioOferta: dto.PrecioOferta,
                Desde: dto.FechaDesde, Hasta: dto.FechaHasta, VerificarVistos: false)], ct);
        if (r.IsSuccess && r.Value.Aplicado)
            return StatusCode(StatusCodes.Status201Created, r.Value);
        return ResultadoPrecios.AResultado(this, r, simular: false);
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetOferta(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ofertaSvc.GetByIdAsync(idTenant, id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value == null) return NotFound();
        return Ok(r.Value);
    }

    // S57.64 R8: aprobación editorial (Publicado=true, idempotente). 200/404, sin esquema nuevo.
    [HttpPost("{id:long}/aprobar")]
    [RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_EDITAR")]
    public async Task<IActionResult> AprobarOferta(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _ofertaSvc.AprobarAsync(idTenant, id, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    [HttpGet]
    public async Task<IActionResult> GetOfertas([FromQuery] long idPresProd, [FromQuery] bool vigente = false, [FromQuery] DateOnly? fecha = null, CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (vigente)
        {
            var rv = await _ofertaSvc.GetVigenteAsync(idTenant, idPresProd, fecha, ct);
            if (!rv.IsSuccess) return StatusCode(rv.Error!.HttpStatusCode ?? 400, rv.Error!.ToProblemDetails(HttpContext));
            if (rv.Value == null) return NotFound();
            return Ok(rv.Value);
        }
        var r = await _ofertaSvc.GetByPresProdAsync(idTenant, idPresProd, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    // S57.52: resolución normal→oferta→efectivo con referencia (solo lectura, S57.51-07).
    [HttpGet("resolucion")]
    public async Task<IActionResult> GetResolucion([FromQuery] long idPresProd, [FromQuery] DateOnly? fecha = null, CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _resolver.ResolveAsync(idTenant, idPresProd, fecha, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value == null) return NotFound();
        return Ok(r.Value);
    }
}

public sealed record CreateOfertaDto(
    long IdPresProd,
    long IdPrecioReferencia,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    decimal PrecioOferta,
    decimal? PctReferencial,
    short Prioridad = 100,
    bool Publicado = false);
