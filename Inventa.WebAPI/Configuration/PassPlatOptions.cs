// S57.27: contrato runtime tipado para PassPlatClient (sección "PassPlat").
// RUNTIME SOURCE ONLY vía IConfiguration. JAMÁS dbo.Configuracion (S57.25 opción B, S57.26).
// Configuración inválida => fail-fast en startup (ValidateOnStart). Mensajes sin secretos.
using System.ComponentModel.DataAnnotations;

namespace Inventa.WebAPI.Configuration;

public sealed class PassPlatOptions : IValidatableObject
{
    public const string SectionName = "PassPlat";
    public const int DefaultTimeoutMs = 10000;
    public const int MinTimeoutMs = 1000;
    public const int MaxTimeoutMs = 120000;

    [Required(ErrorMessage = "PassPlat:ApiBaseUrl es obligatorio.")]
    public string ApiBaseUrl { get; set; } = string.Empty;

    [Range(MinTimeoutMs, MaxTimeoutMs, ErrorMessage = "PassPlat:TimeoutMs debe estar entre 1000 y 120000 ms.")]
    public int TimeoutMs { get; set; } = DefaultTimeoutMs;

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrWhiteSpace(ApiBaseUrl))
        {
            yield return new ValidationResult(
                "PassPlat:ApiBaseUrl es obligatorio y debe ser una URI absoluta http/https.",
                [nameof(ApiBaseUrl)]);
            yield break;
        }
        if (!Uri.TryCreate(ApiBaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            yield return new ValidationResult(
                "PassPlat:ApiBaseUrl debe ser una URI absoluta con esquema http o https.",
                [nameof(ApiBaseUrl)]);
    }
}
