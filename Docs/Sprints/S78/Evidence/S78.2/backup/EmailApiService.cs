using CBP.Logging.Interfaces;
using CBP.MultiTenant.Abstractions;
using CBP.Results;
using CBP.Results.Errors;
using Microsoft.Extensions.Configuration;
using PassPlat.Aplicacion.Dtos.Core;
using PassPlat.Aplicacion.Services.Email;
using PassPlat.Datos.Repositories;
using System.Text.Encodings.Web;

namespace PassPlat.Aplicacion.Services.Email;

// S78: servicio de envío de email por plantilla para aplicaciones externas
// - Tenant siempre del JWT (ITenantContext), nunca del cuerpo
// - Solo plantillas publicadas de categorías permitidas (config EmailApi:CategoriasPermitidas, default ["app-externa"])
// - Valores HTML-codificados explícitamente (Fluid no codifica por defecto v2.40)
// - IdApp = null en el job (prioridad: App → Tenant → Global; queremos Tenant/Global)
// - TemplateCode en Extra para override
public interface IEmailApiService
{
    Task<Result<EnviarEmailPlantillaResponse>> EnviarAsync(EnviarEmailPlantillaRequest request, CancellationToken ct = default);
}

public sealed class EmailApiService : IEmailApiService
{
    private readonly ITenantContext _tenantContext;
    private readonly IEmailTemplateRepository _templateRepo;
    private readonly IEmailQueue _emailQueue;
    private readonly IConfiguration _cfg;
    private readonly ILoggerService? _logger;

    public EmailApiService(
        ITenantContext tenantContext,
        IEmailTemplateRepository templateRepo,
        IEmailQueue emailQueue,
        IConfiguration cfg,
        ILoggerService? logger = null)
    {
        _tenantContext = tenantContext ?? throw new ArgumentNullException(nameof(tenantContext));
        _templateRepo = templateRepo ?? throw new ArgumentNullException(nameof(templateRepo));
        _emailQueue = emailQueue ?? throw new ArgumentNullException(nameof(emailQueue));
        _cfg = cfg ?? throw new ArgumentNullException(nameof(cfg));
        _logger = logger;
    }

    public async Task<Result<EnviarEmailPlantillaResponse>> EnviarAsync(EnviarEmailPlantillaRequest request, CancellationToken ct = default)
    {
        // 1. Tenant del JWT (ITenantContext)
        if (!_tenantContext.HasTenant)
            return Result<EnviarEmailPlantillaResponse>.Failure("TENANT_REQUIRED", "Tenant no resuelto desde JWT");

        var idTenant = _tenantContext.CurrentId;

        // 2. Categoría permitida (config, default ["app-externa"])
        var allowedCategories = _cfg.GetSection("EmailApi:CategoriasPermitidas").Get<string[]>() ?? ["app-externa"];

        // 3. Obtener plantilla por código (con fallback tenant → global)
        var templateResult = await _templateRepo.ObtenerPorNombreCulturaAsync(request.CodigoPlantilla, request.Cultura, idTenant, ct);
        if (templateResult.IsFailure || templateResult.Value == null)
            return Result<EnviarEmailPlantillaResponse>.Failure("EMAIL_PLANTILLA_NO_ENCONTRADA", $"Plantilla no encontrada: {request.CodigoPlantilla}");

        var template = templateResult.Value;

        // 4. Verificar estado publicado
        if (!string.Equals(template.Estado, "publicado", StringComparison.OrdinalIgnoreCase))
            return Result<EnviarEmailPlantillaResponse>.Failure("EMAIL_PLANTILLA_NO_PUBLICADA", "La plantilla no está publicada");

        // 5. Verificar categoría permitida (evita plantillas sensibles como reset pwd, MFA)
        if (!allowedCategories.Contains(template.Categoria, StringComparer.OrdinalIgnoreCase))
            return Result<EnviarEmailPlantillaResponse>.Failure("EMAIL_PLANTILLA_NO_PERMITIDA", $"Categoría '{template.Categoria}' no permitida para envío por API");

        // 6. Codificar valores HTML (Fluid no codifica por defecto v2.40)
        var encoder = HtmlEncoder.Default;
        var safeVariables = new Dictionary<string, object?>();
        if (request.Variables is not null)
        {
            foreach (var kv in request.Variables)
                safeVariables[kv.Key] = encoder.Encode(kv.Value ?? "");
        }

        // 7. Preparar job (IdApp = null para que resuelva Tenant → Global)
        var job = new EmailJob(
            Kind: EmailJobKind.ApiPlantilla,
            ToEmail: request.Destinatario,
            UserName: request.NombreDestinatario ?? "",
            Extra: new Dictionary<string, object?>
            {
                ["TemplateCode"] = request.CodigoPlantilla,
                ["Variables"] = safeVariables
            },
            IdTenant: idTenant,
            IdUsuario: null,
            IdApp: null, // prioridad Tenant → Global (no App)
            CorrelationId: Guid.NewGuid().ToString("N"));

        // 8. Encolar
        await _emailQueue.EnqueueAsync(job, ct);

        // 9. Log estructurado sin PII del mensaje
        _logger?.LogInformation("EmailApi.Enviar: template={Template}, tenant={Tenant}, destinatario={Destinatario}, correlationId={CorrelationId}",
            request.CodigoPlantilla, idTenant, MaskEmail(request.Destinatario), job.CorrelationId);

        return Result<EnviarEmailPlantillaResponse>.Success(new EnviarEmailPlantillaResponse(
            IdEmailLog: null, // EmailLogId se asigna en background
            TrackingId: job.CorrelationId));
    }

    private static string MaskEmail(string email)
    {
        var at = email.IndexOf('@');
        if (at <= 1) return "***";
        return email[..1] + new string('*', at - 1) + email[at..];
    }
}