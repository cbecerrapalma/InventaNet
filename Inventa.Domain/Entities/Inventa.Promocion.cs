namespace Inventa.Domain.Entities;

// ============================================
// PROMOCION — FASE 7 / S57.66 (ADMIN-ONLY, PERSISTENCE-READY)
// Diseño aprobado: S57.51 DDL/03-Promocion.sql + 04-Promocion-Items.sql.
// Promocion es aggregate root: owns Presentaciones, Reglas, Beneficios.
// Cabecera con 0 referencias a Precio (P12). Sin Pricing Engine, sin stacking,
// sin POS, sin catálogo público (S57.66 §§20-22,48).
// ============================================
public class Promocion
{
    public long IdPromocion { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? TextoComercial { get; set; }
    /// <summary>1=2x1 2=3x2 3=SegundoMitad 4=Regalo 5=Combo 6=MontoMin 7=CantMin (S57.50-03).</summary>
    public byte TipoMecanica { get; set; }
    /// <summary>1=A-congelado (default) 2=B-flotante (solo por mecánica) 3=C-republicar (P17).</summary>
    public byte PoliticaPrecio { get; set; } = 1;
    public DateOnly FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public short Prioridad { get; set; } = 100;
    public bool Exclusiva { get; set; } = false;
    public bool Activo { get; set; } = true;
    public bool Publicado { get; set; } = false;
    public DateTime FecCrea { get; set; }
    public DateTime? FecMod { get; set; }

    // Navigation (aggregate children)
    public Tenant Tenant { get; set; } = null!;
    public ICollection<PromocionPresentacion> Presentaciones { get; set; } = [];
    public ICollection<PromocionRegla> Reglas { get; set; } = [];
    public ICollection<PromocionBeneficio> Beneficios { get; set; } = [];
}

// PromocionPresentacion reemplaza PromocionItem(Alcance,IdAlcance) (P19).
// Cada participación VENDIBLE lleva su IdPrecioReferencia (P12).
public class PromocionPresentacion
{
    public long IdPart { get; set; }
    public long IdPromocion { get; set; }
    public long IdPresProd { get; set; }
    /// <summary>NULL solo si EsMuestra=1 (CHECK pareado, P18). Referencia congelada (P16).</summary>
    public long? IdPrecioReferencia { get; set; }
    /// <summary>1=Gatillo 2=Beneficio 3=Regalo.</summary>
    public byte Rol { get; set; }
    public decimal Cantidad { get; set; } = 1;
    public bool EsMuestra { get; set; } = false;

    // Navigation
    public Promocion Promocion { get; set; } = null!;
    public PresProd PresProd { get; set; } = null!;
    public Precio? PrecioReferencia { get; set; }
}

// Persistencia de definición de regla. NO es Pricing Engine (S57.66 §10).
public class PromocionRegla
{
    public long IdRegla { get; set; }
    public long IdPromocion { get; set; }
    public byte TipoRegla { get; set; }
    public byte Operador { get; set; } = 1;
    public string Valor { get; set; } = null!;

    // Navigation
    public Promocion Promocion { get; set; } = null!;
}

// Beneficio lleva IdPromocion de origen: jamás se confunde con Oferta.
public class PromocionBeneficio
{
    public long IdBeneficio { get; set; }
    public long IdPromocion { get; set; }
    /// <summary>1=Pct 2=Monto 3=PrecioFijo 4=UnidadGratis.</summary>
    public byte TipoBeneficio { get; set; }
    public decimal Valor { get; set; }

    // Navigation
    public Promocion Promocion { get; set; } = null!;
}
