namespace Inventa.Domain.Entities;

/// <summary>
/// S57.1 Commercial storefront profile per tenant (Inventa-owned).
/// Identity authority stays in PassPlat; everything visible here is
/// business content administered from Inventa. Null profile = unpublished.
/// </summary>
public class TenantStorefront
{
    public int IdTenant { get; set; }
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
    // S60.1 Branding público (columnas en dbo.TenantStorefront vía script S60).
    public string? ColorAcento { get; set; }
    public string? FaviconUrl { get; set; }
    public string? AnuncioTexto { get; set; }
    public string PerfilStore { get; set; } = "General";
    public bool Publicado { get; set; }
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    public DateTime? FecMod { get; set; }

    public Tenant Tenant { get; set; } = null!;
    public ICollection<TenantSocialLink> SocialLinks { get; set; } = [];
    public ICollection<TenantBusinessHour> BusinessHours { get; set; } = [];
}

/// <summary>S57.1 Normalized social links (extensible, no per-network columns).</summary>
public class TenantSocialLink
{
    public int IdTenantSocialLink { get; set; }
    public int IdTenant { get; set; }
    public string Tipo { get; set; } = null!;
    public string Url { get; set; } = null!;
    public int Orden { get; set; }
    public bool Activo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}

/// <summary>S57.1 Structured business hours (supports two daily ranges).</summary>
public class TenantBusinessHour
{
    public int IdTenantBusinessHour { get; set; }
    public int IdTenant { get; set; }
    public byte DiaSemana { get; set; }
    public TimeSpan? Apertura1 { get; set; }
    public TimeSpan? Cierre1 { get; set; }
    public TimeSpan? Apertura2 { get; set; }
    public TimeSpan? Cierre2 { get; set; }
    public bool Cerrado { get; set; }
    public bool Activo { get; set; } = true;

    public Tenant Tenant { get; set; } = null!;
}
