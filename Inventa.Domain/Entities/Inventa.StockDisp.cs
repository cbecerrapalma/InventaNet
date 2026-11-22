using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// STOCK DISPONIBLE (ultra rápido POS)
// ============================================
public class StockDisp
{
    public long IdStockDisp { get; set; }
    public int IdTenant { get; set; }
    public long IdPresProd { get; set; }
    public int IdBodega { get; set; }
    public decimal CantDisp { get; set; }

    // Navigation
    public PresProd PresProd { get; set; } = null!;
    public Bodega Bodega { get; set; } = null!;
}