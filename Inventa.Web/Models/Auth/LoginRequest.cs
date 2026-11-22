using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Auth;

public sealed class LoginRequest
{
    [Required(ErrorMessage = "Usuario es requerido")]
    public string NomUsuario { get; set; } = "";

    public string? Email { get; set; }

    [Required(ErrorMessage = "Password es requerido")]
    public string Password { get; set; } = "";

    public int IdApp { get; set; }

    public int? IdTenant { get; set; }
}
