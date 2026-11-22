using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Prod
{
    public long IdProd { get; set; }
    public int IdTenant { get; set; }
    public long IdCategoria { get; set; }
    public int? IdMarca { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public int IdUniBase { get; set; }
    public int? DiasVen { get; set; }
    public bool ManejaLot { get; set; } = false;
    public bool EsPesable { get; set; } = false;
    public bool Activo { get; set; } = true;
    /// <summary>S57.2 publishability: commercial opt-in for Inventa.Store. Activo=operational, VisibleStore=public. Default false (fail-closed).</summary>
    public bool VisibleStore { get; set; } = false;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Categoria Categoria { get; set; } = null!;
    public Unidad UnidadBase { get; set; } = null!;
    public Marca? Marca { get; set; }
    public ICollection<VarProd> Variantes { get; set; } = [];
}
