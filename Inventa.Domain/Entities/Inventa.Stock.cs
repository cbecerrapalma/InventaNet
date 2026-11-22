using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// STOCK CONSOLIDADO
// ============================================
public class Stock
{
    public long IdStock { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public long IdVarProd { get; set; }
    public long? IdLote { get; set; }
    public decimal CantAct { get; set; }
    public decimal CostoProm { get; set; }
    public DateTime FecAct { get; set; }

    // Navigation
    public Bodega Bodega { get; set; } = null!;
    public VarProd VarProd { get; set; } = null!;
    public Lote? Lote { get; set; }
}
