namespace Inventa.Web.Models.Storefront;

// S57.2 P1: DTOs for Backoffice storefront administration (api/storefront). Tenant from JWT, never shown.
public sealed class StorefrontProfileDto
{
    public string? NombreFantasia { get; set; }
    public string? DescripcionCorta { get; set; }
    public string? Descripcion { get; set; }
    public string? LogoUrl { get; set; }
    public string? Telefono { get; set; }
    public string? WhatsApp { get; set; }
    public string? Email { get; set; }
    public string? Direccion { get; set; }
    public string? Comuna { get; set; }
    public string? Ciudad { get; set; }
    public string? Region { get; set; }
    public string? SitioWeb { get; set; }
    public bool Publicado { get; set; }
}

public sealed class StorefrontGetResponse
{
    public StorefrontProfileDto? P { get; set; }
    public List<SocialLinkDto> Social { get; set; } = new();
    public List<BusinessHourDto> Hours { get; set; } = new();
}

public sealed class SocialLinkDto
{
    public int IdTenantSocialLink { get; set; }
    public string Tipo { get; set; } = "";
    public string Url { get; set; } = "";
    public int Orden { get; set; }
    public bool Activo { get; set; }
}

public sealed class BusinessHourDto
{
    public byte DiaSemana { get; set; }
    public TimeSpan? Apertura1 { get; set; }
    public TimeSpan? Cierre1 { get; set; }
    public TimeSpan? Apertura2 { get; set; }
    public TimeSpan? Cierre2 { get; set; }
    public bool Cerrado { get; set; }
}

public sealed class UpsertSocialDto
{
    public string Tipo { get; set; } = "";
    public string Url { get; set; } = "";
    public int Orden { get; set; }
}
