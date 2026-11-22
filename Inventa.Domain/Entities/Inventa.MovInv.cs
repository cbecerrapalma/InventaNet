using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// MOVIMIENTO DE INVENTARIO
// ============================================
public class MovInv
{
    public long IdMovInv { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public long IdPresProd { get; set; }
    public long? IdLote { get; set; }
    public DateTime FecMov { get; set; } = DateTime.UtcNow;
    /// <summary>1=Compra, 2=Venta, 3=Ajuste Entrada, 4=Ajuste Salida, 5=Trans.Entrada, 6=Trans.Salida</summary>
    public byte TipoMov { get; set; }
    public decimal CantCom { get; set; }
    public decimal CantBase { get; set; }
    public decimal CostoUni { get; set; }
    public int? UsuarioCrea { get; set; }

    // Navigation
    public Bodega Bodega { get; set; } = null!;
    public PresProd PresProd { get; set; } = null!;
    public Lote? Lote { get; set; }
    public VentaDetalle? VentaDetalle { get; set; }
    public Usuario? UsuarioCreaNav { get; set; }
}
