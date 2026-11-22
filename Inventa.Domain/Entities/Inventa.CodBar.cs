using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// CODIGO DE BARRA
// ============================================
public class CodBar
{
    public long IdCodBar { get; set; }
    public long IdPresProd { get; set; }
    public string Codigo { get; set; } = null!;
    /// <summary>1=Normal, 2=Pesable, 3=Interno, 4=Proveedor</summary>
    public byte TipoCod { get; set; } = 1;
    public bool Principal { get; set; } = false;

    // Navigation
    public PresProd PresProd { get; set; } = null!;
}
