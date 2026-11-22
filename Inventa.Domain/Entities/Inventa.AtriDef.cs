using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// ATRIBUTO DEFINICION
// ============================================
public class AtriDef
{
    public long IdAtriDef { get; set; }
    public long IdCategoria { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool EsOblig { get; set; } = false;
    public bool Activo { get; set; } = true;

    // Navigation
    public Categoria Categoria { get; set; } = null!;
    public ICollection<AtriVal> Valores { get; set; } = [];
}