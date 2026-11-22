using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// DETALLE ORDEN DE COMPRA
// ============================================
public class OrdenCompraDetalle
{
    public long IdOCDetalle { get; set; }
    public long IdOrdenCompra { get; set; }
    public long IdPresProd { get; set; }
    public decimal Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }

    // Navigation
    public OrdenCompra OrdenCompra { get; set; } = null!;
    public PresProd PresProd { get; set; } = null!;
}
