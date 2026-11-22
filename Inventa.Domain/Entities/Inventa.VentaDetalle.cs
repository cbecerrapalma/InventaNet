using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// DETALLE DE VENTA
// ============================================
public class VentaDetalle
{
    public long IdVentaDetalle { get; set; }
    public long IdVenta { get; set; }
    public int NumLinea { get; set; }
    public long IdPresProd { get; set; }
    public long IdMovInv { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal CostoUnitario { get; set; }
    public decimal Descuento { get; set; } = 0;
    public decimal Subtotal { get; set; }

    // Navigation
    public Venta Venta { get; set; } = null!;
    public PresProd PresProd { get; set; } = null!;
    public MovInv MovInv { get; set; } = null!;
}
