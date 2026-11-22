// S79: servicio de contacto para Inventa.Store (público, anónimo).
// Orquesta: perfil publicado → email destino → variables → PassPlat (vía abstracción IContactoEmailSender).
// No persiste mensajes en Inventa (PassPlat registra EmailLog).
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Models;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging;
using System.Globalization;

namespace Inventa.Application.Async.Services;

/// <summary>Abstracción para envío de email por plantilla (implementada en WebAPI con PassPlatClient + token provider).</summary>
public interface IContactoEmailSender
{
    Task<Result<ContactoEmailResult>> SendAsync(
        string codigoPlantilla,
        string destinatario,
        string nombreDestinatario,
        Dictionary<string, string> variables,
        string cultura,
        CancellationToken ct = default);
}

/// <summary>Resultado del envío de email (opaco para la capa Application).</summary>
public sealed record ContactoEmailResult(string? TrackingId, long? IdEmailLog);

/// <summary>Request del formulario de contacto (validado en controlador y aquí).</summary>
public sealed record PublicContactRequest(
    string Nombre,
    string Email,
    string? Telefono,
    string Mensaje,
    bool Consentimiento,
    long? ProductoId,
    string Website); // honeypot

/// <summary>Errores tipados del contacto.</summary>
public static class ContactoErrors
{
    public static Error Validacion(string campo, string mensaje)
        => new Error("CONTACTO_VALIDACION", $"{campo}: {mensaje}").WithType(ErrorType.Validation).WithHttpStatus(400);

    public static Error NoConfigurado(string codigo)
        => new Error("CONTACTO_NO_CONFIGURADO", "El formulario de contacto no está configurado.").WithType(ErrorType.ExternalService).WithHttpStatus(503);

    public static Error Upstream(string codigoUpstream)
        => new Error("CONTACTO_UPSTREAM", "No se pudo enviar el mensaje. Intente más tarde.").WithType(ErrorType.ExternalService).WithHttpStatus(503);

    public static Error RateLimited(int retryAfter)
        => new Error("CONTACTO_RATE_LIMITED", $"Demasiadas solicitudes. Reintente en {retryAfter} segundos.").WithType(ErrorType.ExternalService).WithHttpStatus(429)
            .WithDetails(("retryAfter", retryAfter));
}

/// <summary>Servicio de contacto: valida, arma variables y delega envío a IContactoEmailSender.</summary>
public sealed class ContactoServiceAsync
{
    private readonly IStorefrontServiceAsync _storefront;
    private readonly IContactoEmailSender _emailSender;
    private readonly ILogger<ContactoServiceAsync> _logger;

    public ContactoServiceAsync(
        IStorefrontServiceAsync storefront,
        IContactoEmailSender emailSender,
        ILogger<ContactoServiceAsync> logger)
    {
        _storefront = storefront ?? throw new ArgumentNullException(nameof(storefront));
        _emailSender = emailSender ?? throw new ArgumentNullException(nameof(emailSender));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<Result<ContactoEmailResult>> EnviarAsync(
        int idTenantPassPlat,
        PublicContactRequest req,
        string? referer,
        CancellationToken ct = default)
    {
        // 1. Validaciones de servidor (duplican cliente; nunca confiar en cliente)
        var validationErrors = ValidateRequest(req);
        if (validationErrors.Count > 0)
            return Result<ContactoEmailResult>.Failure(validationErrors[0]); // primer error

        // 2. Honeypot: si viene lleno, 202 silencioso sin enviar (no revelar detección)
        if (!string.IsNullOrWhiteSpace(req.Website))
        {
            _logger.LogInformation("Contacto honeypot detectado trace {Trace}", "");
            return Result<ContactoEmailResult>.Success(new ContactoEmailResult(null, null));
        }

        // 3. Resolver tenant local desde PassPlatTenantId
        var tenantResult = await _storefront.ResolveStoreTenantAsync(idTenantPassPlat, ct);
        if (!tenantResult.IsSuccess)
            return Result<ContactoEmailResult>.Failure(tenantResult.Error!);

        var idTenant = tenantResult.Value;

        // 4. Obtener perfil publicado (para email destino + nombre tienda)
        var profileResult = await _storefront.GetPublishedProfileAsync(idTenant, ct);
        if (!profileResult.IsSuccess)
            return Result<ContactoEmailResult>.Failure(profileResult.Error!);

        var profile = profileResult.Value;
        if (profile == null || string.IsNullOrWhiteSpace(profile.Profile.Email))
        {
            _logger.LogWarning("Contacto: perfil no publicado o sin email tenant {Tenant}", idTenant);
            return Result<ContactoEmailResult>.Failure(ContactoErrors.NoConfigurado("STOREFRONT_NO_EMAIL"));
        }

        var emailDestino = profile.Profile.Email!;
        var nombreTienda = profile.Profile.NombreFantasia ?? profile.TenantNombre;

        // 5. Variables para la plantilla
        var variables = new Dictionary<string, string>
        {
            ["NombreTienda"] = nombreTienda,
            ["NombreContacto"] = req.Nombre,
            ["EmailContacto"] = req.Email,
            ["Telefono"] = req.Telefono ?? "",
            ["Mensaje"] = req.Mensaje,
            ["FechaHora"] = DateTime.Now.ToString("dd/MM/yyyy HH:mm", new CultureInfo("es-CL")),
            ["OrigenUrl"] = SanitizeReferer(referer),
            ["ProductoNombre"] = "",
            ["ProductoUrl"] = ""
        };

        // 6. Si viene ProductoId, intentar enriquecer con nombre y URL pública
        if (req.ProductoId.HasValue)
        {
            var detailResult = await _storefront.GetPublicDetailViewAsync(idTenant, req.ProductoId.Value, ct);
            if (detailResult.IsSuccess && detailResult.Value != null)
            {
                variables["ProductoNombre"] = detailResult.Value.Nombre;
                variables["ProductoUrl"] = $"/productos/{req.ProductoId.Value}";
            }
        }

        // 7. Enviar email (plantilla fija: inventa-store-contacto)
        const string codigoPlantilla = "inventa-store-contacto";
        var sendResult = await _emailSender.SendAsync(
            codigoPlantilla,
            emailDestino,
            req.Nombre,
            variables,
            "es",
            ct);

        if (!sendResult.IsSuccess)
        {
            _logger.LogWarning("Contacto email falló tenant {Tenant}: {Error}", idTenant, sendResult.Error?.Code);
            return Result<ContactoEmailResult>.Failure(sendResult.Error!);
        }

        _logger.LogInformation("Contacto enviado ok tenant {Tenant} trace {Trace}", idTenant, "");
        return sendResult;
    }

    private static List<Error> ValidateRequest(PublicContactRequest req)
    {
        var errors = new List<Error>();
        if (string.IsNullOrWhiteSpace(req.Nombre) || req.Nombre.Length < 2 || req.Nombre.Length > 80)
            errors.Add(ContactoErrors.Validacion("Nombre", "El nombre debe tener entre 2 y 80 caracteres."));
        if (string.IsNullOrWhiteSpace(req.Email) || req.Email.Length > 254 || !req.Email.Contains('@'))
            errors.Add(ContactoErrors.Validacion("Email", "Email inválido."));
        if (!string.IsNullOrWhiteSpace(req.Telefono) && (req.Telefono.Length > 30 || !System.Text.RegularExpressions.Regex.IsMatch(req.Telefono, @"^[\d\s\+\-\(\)]+$")))
            errors.Add(ContactoErrors.Validacion("Telefono", "Teléfono inválido (solo dígitos, espacios, +, -, (, ))."));
        if (string.IsNullOrWhiteSpace(req.Mensaje) || req.Mensaje.Length < 10 || req.Mensaje.Length > 2000)
            errors.Add(ContactoErrors.Validacion("Mensaje", "El mensaje debe tener entre 10 y 2000 caracteres."));
        if (!req.Consentimiento)
            errors.Add(ContactoErrors.Validacion("Consentimiento", "Debe aceptar el tratamiento de datos."));
        return errors;
    }

    private static string SanitizeReferer(string? referer)
    {
        if (string.IsNullOrWhiteSpace(referer)) return "";
        try
        {
            var uri = new Uri(referer);
            // Solo mismo origen (Store) para evitar fuga de referrer externo
            // En producción, el Store corre en https://localhost:5003 o dominio propio
            return uri.GetLeftPart(UriPartial.Path);
        }
        catch { return ""; }
    }
}