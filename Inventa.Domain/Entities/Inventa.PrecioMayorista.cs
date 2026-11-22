using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// PRECIO MAYORISTA (S57.69)
// Relación dedicada con semántica explícita: existencia aquí = mayorista.
// Sin discriminator (prohibido EsMayorista/Tipo/CategoriaPrecio).
// ============================================
public class PrecioMayorista
{
    public long IdPrecioMayorista { get; set; }
    public long IdPresProd { get; set; }
    public decimal Valor { get; set; }
    public DateOnly FecVig { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public PresProd PresProd { get; set; } = null!;
}
