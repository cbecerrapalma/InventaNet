using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// VARIANTE ATRIBUTO (pivot)
// ============================================
public class VarAtri
{
    public long IdVarAtri { get; set; }
    public long IdVarProd { get; set; }
    public long IdAtriVal { get; set; }

    // Navigation
    public VarProd VarProd { get; set; } = null!;
    public AtriVal AtriVal { get; set; } = null!;
}