using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Lote
{
    public long IdLote { get; set; }
    public int IdTenant { get; set; }
    public long IdVarProd { get; set; }
    public string Codigo { get; set; } = null!;
    public DateOnly? FecVen { get; set; }
    public bool Activo { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public VarProd VarProd { get; set; } = null!;
    public ICollection<MovInv> Movimientos { get; set; } = [];
    public ICollection<Stock> Stocks { get; set; } = [];
}
