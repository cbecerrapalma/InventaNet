using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class VarProd
{
    public long IdVarProd { get; set; }
    public long IdProd { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public decimal CostoProm { get; set; } = 0;
    public bool Activo { get; set; } = true;

    // Navigation
    public Prod Prod { get; set; } = null!;
    public ICollection<VarAtri> Atributos { get; set; } = [];
    public ICollection<PresProd> Presentaciones { get; set; } = [];
    public ICollection<Lote> Lotes { get; set; } = [];
    public ICollection<Stock> Stocks { get; set; } = [];
    public ICollection<ProdConv> Conversiones { get; set; } = [];
}
