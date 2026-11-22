namespace Inventa.Domain.Entities;

// ============================================
// OFERTA CON REFERENCIA OBLIGATORIA (S57.52)
// IdPrecioReferencia NOT NULL -> Precio TipoPrec=1 misma PresProd (P11/P13/P14).
// PrecioOferta es fuente de verdad; referencia congelada (P16).
// Sin contraparte sync (como ProductoImagen/Storefront, solo Async).
// ============================================
public class Oferta
{
    public long IdOferta { get; set; }
    public long IdPresProd { get; set; }
    public long IdPrecioReferencia { get; set; }
    public DateOnly FechaDesde { get; set; }
    public DateOnly? FechaHasta { get; set; }
    public decimal PrecioOferta { get; set; }
    public decimal? PctReferencial { get; set; }
    public short Prioridad { get; set; } = 100;
    public bool Activo { get; set; } = true;
    public bool Publicado { get; set; } = false;
    /// <summary>0=manual, 1=migración Fase 5 (reservado; S57.52 no migra).</summary>
    public byte OrigenMigracion { get; set; } = 0;
    public long? SourceIdPrecio { get; set; }
    public Guid? MigrationBatchId { get; set; }
    public DateTime FecCrea { get; set; }
    public DateTime? FecMod { get; set; }

    // Navigation
    public PresProd PresProd { get; set; } = null!;
    public Precio PrecioReferencia { get; set; } = null!;
}
