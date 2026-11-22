using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Unidad
{
    public int IdUnidad { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;

    // Navigation
    public ICollection<Prod> Productos { get; set; } = [];
    public ICollection<ProdConv> Conversiones { get; set; } = [];
    public ICollection<PresProd> Presentaciones { get; set; } = [];
}
