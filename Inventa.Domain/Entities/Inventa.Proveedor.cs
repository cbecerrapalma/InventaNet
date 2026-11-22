using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Proveedor
{
    public int IdProveedor { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string RazonSocial { get; set; } = null!;
    public string? NombreComercial { get; set; }
    public string? RUT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Contacto { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = [];
}
