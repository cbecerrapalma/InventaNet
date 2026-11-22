using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Venta
{
    public long IdVenta { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public string NumVenta { get; set; } = null!;
    public DateTime FecVenta { get; set; } = DateTime.UtcNow;
    public int? IdCliente { get; set; }
    /// <summary>1=Boleta, 2=Factura, 3=Nota de Venta</summary>
    public byte TipoDoc { get; set; } = 1;
    public int? IdUsuario { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Descuento { get; set; } = 0;
    public decimal Impuesto { get; set; } = 0;
    public decimal Total { get; set; }
    /// <summary>1=Completada, 2=Anulada</summary>
    public byte Estado { get; set; } = 1;
    public DateTime? FecAnulacion { get; set; }
    public string? MotivoAnulacion { get; set; }
    public int? UsuarioAnula { get; set; }

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public Bodega Bodega { get; set; } = null!;
    public Cliente? Cliente { get; set; }
    public Usuario? Usuario { get; set; }
    public Usuario? UsuarioAnulaNav { get; set; }
    public ICollection<VentaDetalle> Detalles { get; set; } = [];
    public ICollection<VentaPago> Pagos { get; set; } = [];
}
