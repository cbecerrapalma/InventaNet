using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// CONVERSION DE PRODUCTO
// ============================================
public class ProdConv
{
    public long IdProdConv { get; set; }
    public long IdVarProd { get; set; }
    public int IdUnidad { get; set; }
    public decimal Factor { get; set; }

    // Navigation
    public VarProd VarProd { get; set; } = null!;
    public Unidad Unidad { get; set; } = null!;
}
