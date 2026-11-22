using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Configuracion;

// GET /api/configuracion response matches Configuracion entity (Id=1 singleton)
// No secrets exposed (SecretKey/Password never in DTO per AGENTS frozen contract)
public sealed class ConfiguracionDto
{
    public int Id { get; set; }
    public string PassPlatApiBaseUrl { get; set; } = "";
    public int PassPlatTimeoutMs { get; set; }
    public string PassPlatAmbiente { get; set; } = "";
    public bool Activo { get; set; }
    public DateTime? FechaSync { get; set; }
    public DateTime? FecMod { get; set; }
    public int? UsuarioMod { get; set; }
}

public sealed class UpdateConfiguracionDto
{
    [Required, MaxLength(500), Url] public string PassPlatApiBaseUrl { get; set; } = "";
    [Range(1000, 120000)] public int PassPlatTimeoutMs { get; set; }
    [Required, MaxLength(50)] public string PassPlatAmbiente { get; set; } = "";
    public bool Activo { get; set; } = true;
}

// GET /api/tenants/{id}/config response matches TenantConfigInventa entity
public sealed class TenantConfigDto
{
    public int IdTenant { get; set; }
    public short MaxBodegas { get; set; }
    public bool PermiteStockNegativo { get; set; }
    public string Moneda { get; set; } = "";
    public decimal TasaImpuesto { get; set; }
    public int DiasRetencionMov { get; set; }
    public bool PermiteVentaSinCliente { get; set; }
    public DateTime? FecMod { get; set; }
    public int? UsuarioMod { get; set; }
}

// PUT /api/tenants/{id}/config body — S55.2: PermiteVentaSinCliente editable, DiasRetencionMov remains readonly UNUSED/FUTURE
// S57.24: PermiteVentaSinCliente es bool? (PUT parcial: null = preservar; coherente con WebAPI UpdateTenantConfigDto).
public sealed class UpdateTenantConfigDto
{
    [Range(1, 100)] public short MaxBodegas { get; set; }
    public bool PermiteStockNegativo { get; set; }
    [Required, MaxLength(10)] public string Moneda { get; set; } = "";
    [Range(0, 1)] public decimal TasaImpuesto { get; set; }
    public bool? PermiteVentaSinCliente { get; set; } = true;
}
