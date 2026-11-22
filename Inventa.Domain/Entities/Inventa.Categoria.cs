using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Categoria
{
    public long IdCategoria { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;

    // S69: jerarquía de 2 niveles (raíz + hijas). Reglas de negocio
    // (mismo tenant, máx. 2 niveles) en Application, no en SQL.
    public long? IdPadre { get; set; }
    public short Orden { get; set; }
    public string? ImagenUrl { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Categoria? Padre { get; set; }
    public ICollection<Categoria> Hijas { get; set; } = [];
    public ICollection<Prod> Productos { get; set; } = [];
    public ICollection<AtriDef> AtributosDef { get; set; } = [];
}
