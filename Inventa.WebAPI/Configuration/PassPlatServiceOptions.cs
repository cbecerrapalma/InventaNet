// S79: opciones del usuario técnico de PassPlat para envío de emails desde Inventa.Store.
// Sección "PassPlatService". Credenciales SOLO en UserSecrets (dev) o secrets.json machine-local (prod).
// Fallo al USAR (no en arranque) => endpoint responde 503 CONTACTO_NO_CONFIGURADO.
using System.ComponentModel.DataAnnotations;

namespace Inventa.WebAPI.Configuration;

public sealed class PassPlatServiceOptions : IValidatableObject
{
    public const string SectionName = "PassPlatService";

    public string AppCodigo { get; set; } = "INVENTA_STORE";

    [Required(ErrorMessage = "PassPlatService:NomUsuario es obligatorio.")]
    public string NomUsuario { get; set; } = string.Empty;

    [Required(ErrorMessage = "PassPlatService:Password es obligatorio.")]
    public string Password { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (string.IsNullOrWhiteSpace(AppCodigo))
            yield return new ValidationResult("PassPlatService:AppCodigo es obligatorio.", [nameof(AppCodigo)]);
        if (string.IsNullOrWhiteSpace(NomUsuario))
            yield return new ValidationResult("PassPlatService:NomUsuario es obligatorio.", [nameof(NomUsuario)]);
        if (string.IsNullOrWhiteSpace(Password))
            yield return new ValidationResult("PassPlatService:Password es obligatorio.", [nameof(Password)]);
    }
}