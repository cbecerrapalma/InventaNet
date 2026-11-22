using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S57.56: endpoint DIAGNÓSTICO (no comercial) para el CommercialOfferResolver.
// Solo lectura, tenant-scoped (404 cross-tenant), policy de lectura.
// Ningún path de producción lo usa; existe para equivalencia E2E (s57.56) y
// preparación de cutover. Estrategia y política M6 via query (contrato explícito,
// jamás variable de entorno). NO activar comportamiento comercial aquí.
[ApiController]
[Route("api/diagnostics")]
[RequierePermiso("INVENTA_PRECIOS_PRINCIPAL_VER")]
public class DiagnosticsController : ControllerBase
{
    private readonly ICommercialOfferResolverAsync _resolver;

    public DiagnosticsController(ICommercialOfferResolverAsync resolver) => _resolver = resolver;

    [HttpGet("commercial-offer")]
    public async Task<IActionResult> GetCommercialOffer(
        [FromQuery] long idPresProd,
        // S57.62 M8.2-B: default NewPreferred. LegacyOnly/DualDiagnostic
        // persisten SOLO como diagnóstico explícito (nunca default).
        [FromQuery] string strategy = "NewPreferred",
        [FromQuery] bool honorPublicado = false,
        [FromQuery] DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (!Enum.TryParse<CommercialOfferStrategy>(strategy, ignoreCase: true, out var strat))
            return BadRequest(new { code = "STRATEGY_INVALIDO", detail = "strategy: LegacyOnly|NewOnly|NewPreferred|DualDiagnostic" });
        var r = await _resolver.ResolveAsync(idTenant, idPresProd, strat, new CommercialOfferPolicy(honorPublicado), fecha, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
