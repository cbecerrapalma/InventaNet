using System;

namespace Inventa.Domain.Entities;

/// <summary>
/// Espejo local de pertenencia PassPlat Usuario-Tenant. PK compuesta (IdUsuario, IdTenant).
/// </summary>
public class UsuarioTenant
{
    public int IdUsuario { get; set; }
    public int IdTenant { get; set; }
    public bool Activo { get; set; } = true;
    public bool EsPrincipal { get; set; }
    public DateTime FechaSync { get; set; } = DateTime.UtcNow;

    // Navigation
    public Usuario Usuario { get; set; } = null!;
    public Tenant Tenant { get; set; } = null!;
}
