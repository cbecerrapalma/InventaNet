using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Cliente
{
    public int IdCliente { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public string? RUT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    /// <summary>1=Público General, 2=Frecuente, 3=Mayorista, 4=Corporativo</summary>
    public byte TipoCliente { get; set; } = 1;
    public bool Activo { get; set; } = true;
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<Venta> Ventas { get; set; } = [];
}
