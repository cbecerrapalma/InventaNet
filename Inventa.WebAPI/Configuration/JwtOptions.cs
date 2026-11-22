// S57.40: contrato runtime tipado para JWT HS256 (sección "Jwt").
// El secret NUNCA proviene de environment: UserSecrets (dev) o secrets.json
// machine-local (prod). Configuración inválida => fail-fast en startup con
// mensaje accionable (mirror PassPlat.WebAPI/Program.cs:52-69). Sin secretos en mensajes.
using System.ComponentModel.DataAnnotations;

namespace Inventa.WebAPI.Configuration;

public sealed class JwtOptions : IValidatableObject
{
    public const string SectionName = "Jwt";
    public const int MinSecretLength = 32;

    public string SecretKey { get; set; } = string.Empty;
    public string Issuer { get; set; } = "PassPlat";
    public string Audience { get; set; } = "InventaNet";

    /// <summary>
    /// Audiencias adicionales aceptadas. Default ["PassPlat"]: cierra H-2 lado
    /// Inventa (tokens legacy emitidos con aud=PassPlat; mismo fix que S54.15 en PassPlat).
    /// </summary>
    public string[] AdditionalAudiences { get; set; } = ["PassPlat"];

    public int ClockSkewMinutes { get; set; } = 5;

    public IEnumerable<string> ValidAudiences =>
        new[] { Audience }.Concat(AdditionalAudiences ?? []).Distinct().ToArray();

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (SecretKey.Length < MinSecretLength)
            yield return new ValidationResult(
                "Jwt:SecretKey ausente o menor a 32 caracteres.",
                [nameof(SecretKey)]);
    }
}
