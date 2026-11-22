using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// METODO DE PAGO
// ============================================
public class MetodoPago
{
    public byte IdMetodoPago { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;

    // Navigation
    public ICollection<VentaPago> VentaPagos { get; set; } = [];
}
