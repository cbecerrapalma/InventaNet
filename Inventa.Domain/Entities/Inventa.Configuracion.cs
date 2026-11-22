using System;

namespace Inventa.Domain.Entities;

/// <summary>
/// Configuración global singleton (Id = 1). No almacena SecretKey, Password ni RefreshToken.
/// </summary>
public class Configuracion
{
    public int Id { get; set; } = 1;
    public string PassPlatApiBaseUrl { get; set; } = "https://passplat.local";
    public int PassPlatTimeoutMs { get; set; } = 10000;
    public string PassPlatAmbiente { get; set; } = "Production";
    public bool Activo { get; set; } = true;
    public DateTime? FechaSync { get; set; }
    public DateTime? FecMod { get; set; }
    public int? UsuarioMod { get; set; }

    // Navigation
    public Usuario? UsuarioModNav { get; set; }
}
