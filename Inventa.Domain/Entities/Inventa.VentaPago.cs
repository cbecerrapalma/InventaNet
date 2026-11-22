using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// PAGO DE VENTA
// ============================================
public class VentaPago
{
    public long IdVentaPago { get; set; }
    public long IdVenta { get; set; }
    public byte IdMetodoPago { get; set; }
    public decimal Monto { get; set; }
    public string? Referencia { get; set; }

    // Navigation
    public Venta Venta { get; set; } = null!;
    public MetodoPago MetodoPago { get; set; } = null!;
}