using System;

namespace Inventa.Domain.Entities;

/// <summary>
/// Configuración de negocio por tenant (solo negocio, no identidad).
/// </summary>
public class TenantConfigInventa
{
    public int IdTenant { get; set; }
    public short MaxBodegas { get; set; } = 10;
    public bool PermiteStockNegativo { get; set; }
    public string Moneda { get; set; } = "CLP";
    public decimal TasaImpuesto { get; set; }
    public int DiasRetencionMov { get; set; } = 365;
    public bool PermiteVentaSinCliente { get; set; } = true;
    public DateTime? FecMod { get; set; }
    public int? UsuarioMod { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Usuario? UsuarioModNav { get; set; }
}
