using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

// ============================================
// PRESENTACION DE PRODUCTO
// ============================================
public class PresProd
{
    public long IdPresProd { get; set; }
    public long IdVarProd { get; set; }
    public int IdUnidad { get; set; }
    public string Descrip { get; set; } = null!;
    public bool EsVenta { get; set; } = true;
    public bool EsCompra { get; set; } = true;
    public bool Activo { get; set; } = true;

    // Navigation
    public VarProd VarProd { get; set; } = null!;
    public Unidad Unidad { get; set; } = null!;
    public ICollection<CodBar> CodigosBarra { get; set; } = [];
    public ICollection<Precio> Precios { get; set; } = [];
    public ICollection<PrecioMayorista> PreciosMayorista { get; set; } = [];
    public ICollection<Oferta> Ofertas { get; set; } = [];
    public ICollection<PromocionPresentacion> PromocionPresentaciones { get; set; } = [];
    public ICollection<StockDisp> StocksDisp { get; set; } = [];
    public ICollection<MovInv> Movimientos { get; set; } = [];
    public ICollection<OrdenCompraDetalle> DetallesOC { get; set; } = [];
    public ICollection<VentaDetalle> DetallesVenta { get; set; } = [];
}
