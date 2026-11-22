using System;
using System.Collections.Generic;

namespace Inventa.Domain.Entities;

/// <summary>
/// Espejo local de PassPlat Usuarios.Id — PK no IDENTITY.
/// No contiene Password, Hash, MFA, Session ni RefreshToken.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string NomUsuario { get; set; } = null!;
    public string? Email { get; set; }
    public string? Nombre { get; set; }
    public string? Apellido { get; set; }
    public bool Activo { get; set; } = true;
    public bool EsSistema { get; set; }
    public bool EmailVerificado { get; set; }
    public DateTime FechaSync { get; set; } = DateTime.UtcNow;
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    public DateTime? FecMod { get; set; }

    // Navigation
    public ICollection<UsuarioTenant> UsuarioTenants { get; set; } = [];
}
