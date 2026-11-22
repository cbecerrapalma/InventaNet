using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// ORDEN DE COMPRA
// ============================================
public class OrdenCompra
{
    public long IdOrdenCompra { get; set; }
    public int IdTenant { get; set; }
    public int IdProveedor { get; set; }
    public string NumOrden { get; set; } = null!;
    public DateOnly FecOrden { get; set; }
    public DateOnly? FecEntrega { get; set; }
    /// <summary>1=Pendiente, 2=Aprobada, 3=Recibida, 4=Cancelada</summary>
    public byte Estado { get; set; } = 1;
    public decimal Total { get; set; } = 0;
    public string? Observaciones { get; set; }
    public DateTime FecCrea { get; set; } = DateTime.UtcNow;
    public int? UsuarioCrea { get; set; }
    public int? UsuarioMod { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Proveedor Proveedor { get; set; } = null!;
    public Usuario? UsuarioCreaNav { get; set; }
    public Usuario? UsuarioModNav { get; set; }
    public ICollection<OrdenCompraDetalle> Detalles { get; set; } = [];
}
