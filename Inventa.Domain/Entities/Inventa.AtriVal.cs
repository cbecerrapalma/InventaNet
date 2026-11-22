using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// ATRIBUTO VALOR
// ============================================
public class AtriVal
{
    public long IdAtriVal { get; set; }
    public long IdAtriDef { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int Orden { get; set; } = 0;
    public bool Activo { get; set; } = true;

    // Navigation
    public AtriDef AtriDef { get; set; } = null!;
    public ICollection<VarAtri> VarAtributos { get; set; } = [];
}