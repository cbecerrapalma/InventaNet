using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Tenant
{
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    public int? IdTenantPassPlat { get; set; }
    public bool EsSistema { get; set; }
    public DateTime? FechaSync { get; set; }
    public DateTime? FecMod { get; set; }

    // Navigation
    public ICollection<Marca> Marcas { get; set; } = [];
    public ICollection<Categoria> Categorias { get; set; } = [];
    public ICollection<Prod> Productos { get; set; } = [];
    public ICollection<Bodega> Bodegas { get; set; } = [];
    public ICollection<Lote> Lotes { get; set; } = [];
    public ICollection<Proveedor> Proveedores { get; set; } = [];
    public ICollection<OrdenCompra> OrdenesCompra { get; set; } = [];
    public ICollection<Cliente> Clientes { get; set; } = [];
    public ICollection<Venta> Ventas { get; set; } = [];
    public ICollection<UsuarioTenant> UsuarioTenants { get; set; } = [];
    public TenantConfigInventa? TenantConfig { get; set; }
    public ICollection<TenantSocialLink> SocialLinks { get; set; } = [];
    public ICollection<TenantBusinessHour> BusinessHours { get; set; } = [];
}