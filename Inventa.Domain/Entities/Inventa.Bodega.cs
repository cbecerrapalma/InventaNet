using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;

namespace Inventa.Domain.Entities;

public class Bodega
{
    public int IdBodega { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = null!;
    public string Nombre { get; set; } = null!;
    public bool Activo { get; set; } = true;

    // Navigation
    public Tenant Tenant { get; set; } = null!;
    public ICollection<MovInv> Movimientos { get; set; } = [];
    public ICollection<Stock> Stocks { get; set; } = [];
    public ICollection<StockDisp> StocksDisp { get; set; } = [];
    public ICollection<Venta> Ventas { get; set; } = [];
}