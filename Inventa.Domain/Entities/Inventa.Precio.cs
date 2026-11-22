using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// PRECIO DE VENTA
// ============================================
public class Precio
{
    public long IdPrecio { get; set; }
    public long IdPresProd { get; set; }
    // S57.69: precio normal único (sin discriminator). Mayorista vive en PrecioMayorista.
    public decimal Valor { get; set; }
    public DateOnly FecVig { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public PresProd PresProd { get; set; } = null!;
    public ICollection<Oferta> OfertasReferenciadas { get; set; } = [];
    public ICollection<PromocionPresentacion> PromocionPresentacionesReferenciadas { get; set; } = [];
}
