// S79: endpoint público de contacto (anónimo, guest-first).
// POST /api/public/contacto
// Rate limit particionado por IP (PublicContact). Honeypot. Destinatario fijado por servidor.
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.ComponentModel.DataAnnotations;
using System.Net;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/public")]
[AllowAnonymous]
[EnableRateLimiting("PublicContact")]
public sealed class PublicContactController : ControllerBase
{
    private readonly ContactoServiceAsync _contactoService;
    private readonly IContactoEmailSender _emailSender;
    private readonly IPassPlatClient _passPlat;
    private readonly IPassPlatServiceTokenProvider _tokenProvider;
    private readonly PassPlatServiceOptions _serviceOpts;
    private readonly IConfiguration _cfg;
    private readonly ILogger<PublicContactController> _logger;

    public PublicContactController(
        ContactoServiceAsync contactoService,
        IContactoEmailSender emailSender,
        IPassPlatClient passPlat,
        IPassPlatServiceTokenProvider tokenProvider,
        IOptions<PassPlatServiceOptions> serviceOpts,
        IConfiguration cfg,
        ILogger<PublicContactController> logger)
    {
        _contactoService = contactoService ?? throw new ArgumentNullException(nameof(contactoService));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _passPlat = passPlat ?? throw new ArgumentNullException(nameof(passPlat));
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _serviceOpts = serviceOpts?.Value ?? throw new ArgumentNullException(nameof(serviceOpts));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>Envía formulario de contacto a la tienda.</summary>
    /// <remarks>
    /// Guest-first: no requiere autenticación.
    /// Rate limit: particionado por IP (5 req / 10 min).
    /// Honeypot: campo Website debe venir vacío.
    /// Destinatario: email del perfil publicado de la tienda (nunca del cliente).
    /// </remarks>
    [HttpPost("contacto")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> EnviarContacto(
        [FromBody] Inventa.Application.Async.Services.PublicContactRequest request,
        CancellationToken ct = default)
    {
        Response.Headers.CacheControl = "no-store";

        // 1. Validar credenciales del usuario técnico (solo al USAR, no en arranque)
        var validationResults = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
        if (!Validator.TryValidateObject(_serviceOpts, new ValidationContext(_serviceOpts), validationResults, true))
        {
            _logger.LogWarning("PassPlatService no configurado trace {Trace}", HttpContext.TraceIdentifier);
            return Problem(
                statusCode: 503,
                title: "Contacto no configurado",
                detail: "CONTACTO_NO_CONFIGURADO",
                instance: HttpContext.TraceIdentifier,
                extensions: new Dictionary<string, object?> { ["code"] = "CONTACTO_NO_CONFIGURADO" });
        }

        // 2. Resolver tenant PassPlat desde config (igual que PublicCatalogController)
        var rawTenant = _cfg["PublicCatalog:PassPlatTenantId"];
        if (!int.TryParse(rawTenant, out var idTenantPassPlat) || idTenantPassPlat <= 0)
        {
            _logger.LogWarning("Contacto sin PassPlatTenantId trace {Trace}", HttpContext.TraceIdentifier);
            return Problem(
                statusCode: 503,
                title: "Contacto no configurado",
                detail: "CONTACTO_NO_CONFIGURADO",
                instance: HttpContext.TraceIdentifier,
                extensions: new Dictionary<string, object?> { ["code"] = "CONTACTO_NO_CONFIGURADO" });
        }

        // 3. Referer para OrigenUrl (saneado en servicio)
        var referer = Request.Headers.Referer.ToString();

        // 4. Ejecutar servicio (validaciones, honeypot, perfil, variables)
        var serviceRequest = new Inventa.Application.Async.Services.PublicContactRequest(
            request.Nombre,
            request.Email,
            request.Telefono,
            request.Mensaje,
            request.Consentimiento,
            request.ProductoId,
            request.Website);
        var result = await _contactoService.EnviarAsync(idTenantPassPlat, serviceRequest, referer, ct);

        if (!result.IsSuccess)
        {
            var error = result.Error!;
            var status = error.HttpStatusCode ?? 500;
            var code = error.Code;

            // Mapear errores específicos
            if (code == "CONTACTO_RATE_LIMITED" && error.Details?.TryGetValue("retryAfter", out var ra) == true && ra is int retry)
            {
                Response.Headers.RetryAfter = retry.ToString();
                return StatusCode(429, new ProblemDetails
                {
                    Status = 429,
                    Title = "Demasiadas solicitudes",
                    Detail = error.Message,
                    Instance = HttpContext.TraceIdentifier,
                    Extensions = { ["code"] = code, ["correlationId"] = HttpContext.TraceIdentifier, ["retryAfter"] = retry }
                });
            }

            if (code == "CONTACTO_VALIDACION")
                return BadRequest(new ProblemDetails
                {
                    Status = 400,
                    Title = "Solicitud inválida",
                    Detail = error.Message,
                    Instance = HttpContext.TraceIdentifier,
                    Extensions = { ["code"] = code, ["correlationId"] = HttpContext.TraceIdentifier }
                });

            // 503: no configurado / upstream
            return Problem(
                statusCode: 503,
                title: "Servicio no disponible",
                detail: code,
                instance: HttpContext.TraceIdentifier,
                extensions: new Dictionary<string, object?> { ["code"] = code, ["correlationId"] = HttpContext.TraceIdentifier });
        }

        // 5. Éxito: 202 Accepted con trackingId
        var data = result.Value!;
        _logger.LogInformation("Contacto enviado trace {Trace} tracking {Tracking}", HttpContext.TraceIdentifier, data.TrackingId ?? "-");
        return Accepted(new { code = "CONTACTO_ENVIADO", correlationId = HttpContext.TraceIdentifier, trackingId = data.TrackingId });
    }
}

// PublicContactRequest está definido en Inventa.Application.Async.Services

/// <summary>Implementación de IContactoEmailSender usando PassPlatClient + token provider.</summary>
internal sealed class ContactoEmailSender : IContactoEmailSender
{
    private readonly IPassPlatClient _passPlat;
    private readonly IPassPlatServiceTokenProvider _tokenProvider;
    private readonly IConfiguration _cfg;
    private readonly ILogger<ContactoEmailSender> _logger;

    public ContactoEmailSender(
        IPassPlatClient passPlat,
        IPassPlatServiceTokenProvider tokenProvider,
        IConfiguration cfg,
        ILogger<ContactoEmailSender> logger)
    {
        _passPlat = passPlat ?? throw new ArgumentNullException(nameof(passPlat));
        _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ContactoEmailResult>> SendAsync(
        string codigoPlantilla,
        string destinatario,
        string nombreDestinatario,
        Dictionary<string, string> variables,
        string cultura,
        CancellationToken ct = default)
    {
        // Resolver tenant PassPlat desde config (el mismo que usa PublicCatalog)
        var rawTenant = _cfg["PublicCatalog:PassPlatTenantId"];
        if (!int.TryParse(rawTenant, out var idTenantPassPlat) || idTenantPassPlat <= 0)
            return Result<ContactoEmailResult>.Failure(new Error("CONTACTO_NO_CONFIGURADO", "Tenant PassPlat no configurado.").WithType(ErrorType.ExternalService).WithHttpStatus(503));

        // Obtener token del usuario técnico para este tenant
        var token = await _tokenProvider.GetAccessTokenAsync(idTenantPassPlat, ct);
        if (string.IsNullOrEmpty(token))
            return Result<ContactoEmailResult>.Failure(new Error("CONTACTO_UPSTREAM", "No se pudo autenticar con PassPlat.").WithType(ErrorType.ExternalService).WithHttpStatus(503));

        // Preparar request para PassPlat
        var req = new EnviarEmailPlantillaRequest(
            codigoPlantilla,
            destinatario,
            nombreDestinatario,
            variables,
            cultura);

        // Enviar con reintento único ante 401 (invalida caché y renueva token)
        for (int attempt = 0; attempt < 2; attempt++)
        {
            var result = await _passPlat.EnviarEmailPlantillaAsync(token, req, ct);
            if (result.Success && result.Data != null)
            {
                return Result<ContactoEmailResult>.Success(new ContactoEmailResult(result.Data.TrackingId, result.Data.IdEmailLog));
            }

            if (result.Error != null && result.Error.Contains("401") && attempt == 0)
            {
                // Invalidar caché y reintentar una vez con token fresco (S79 §4.1.2)
                _logger.LogWarning("PassPlat 401 en intento {Attempt}, invalidando caché y reintentando...", attempt + 1);
                _tokenProvider.Invalidate(idTenantPassPlat);
                var fresh = await _tokenProvider.GetAccessTokenAsync(idTenantPassPlat, ct);
                if (string.IsNullOrEmpty(fresh))
                    return Result<ContactoEmailResult>.Failure(new Error("CONTACTO_UPSTREAM", "No se pudo renovar autenticación con PassPlat.").WithType(ErrorType.ExternalService).WithHttpStatus(503));
                token = fresh;
                continue;
            }

            // Mapear error upstream
            var statusCode = 503;
            var code = "CONTACTO_UPSTREAM";
            Error error;
            if (result.Error != null && result.Error.Contains("429"))
            {
                // 429 → 429 con retryAfter para que el controlador emita Retry-After (S79 §4.1.2).
                // Propaga Retry-After de PassPlat cuando existe; fallback 60s solo si falta (S79.2).
                var retryAfter = result.RetryAfterSeconds ?? 60;
                return Result<ContactoEmailResult>.Failure(new Error("CONTACTO_RATE_LIMITED", result.Error).WithType(ErrorType.ExternalService).WithHttpStatus(429).WithDetails(("retryAfter", retryAfter)));
            }
            else if (result.Error != null && (result.Error.Contains("403") || result.Error.Contains("404")))
            {
                statusCode = 503;
                code = "CONTACTO_NO_CONFIGURADO";
            }

            error = new Error(code, result.Error ?? "Error enviando email").WithType(ErrorType.ExternalService).WithHttpStatus(statusCode);
            return Result<ContactoEmailResult>.Failure(error);
        }

        return Result<ContactoEmailResult>.Failure(new Error("CONTACTO_UPSTREAM", "Error enviando email tras reintentos").WithType(ErrorType.ExternalService).WithHttpStatus(503));
    }
}