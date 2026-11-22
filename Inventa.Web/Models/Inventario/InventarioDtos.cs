namespace Inventa.Web.Models.Inventario;

public sealed class StockDto
{
    public long IdStock { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public long IdVarProd { get; set; }
    public long? IdLote { get; set; }
    public decimal CantAct { get; set; }
    public decimal CostoProm { get; set; }
    public DateTime FecAct { get; set; }
    // S114: producto y variante (listado paginado de la API)
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarCodigo { get; set; } = "";
    public string VarNombre { get; set; } = "";
}

public sealed class StockDispDto
{
    public long IdStockDisp { get; set; }
    public int IdTenant { get; set; }
    public long IdPresProd { get; set; }
    public int IdBodega { get; set; }
    public decimal CantDisp { get; set; }
    // S114: producto y variante (listado paginado de la API)
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarCodigo { get; set; } = "";
    public string VarNombre { get; set; } = "";
    public string PresDescrip { get; set; } = "";
}

public sealed class MovInvDto
{
    public long IdMovInv { get; set; }
    public int IdTenant { get; set; }
    public int IdBodega { get; set; }
    public long IdPresProd { get; set; }
    public long? IdLote { get; set; }
    public DateTime FecMov { get; set; }
    public byte TipoMov { get; set; }
    public decimal CantCom { get; set; }
    public decimal CantBase { get; set; }
    public decimal CostoUni { get; set; }
    public int? UsuarioCrea { get; set; }
    // S114: producto y variante (listado paginado de la API)
    public string ProdCodigo { get; set; } = "";
    public string ProdNombre { get; set; } = "";
    public string VarCodigo { get; set; } = "";
    public string VarNombre { get; set; } = "";
    public string PresDescrip { get; set; } = "";
}

public sealed class LoteDto
{
    public long IdLote { get; set; }
    public int IdTenant { get; set; }
    public long IdVarProd { get; set; }
    public string Codigo { get; set; } = "";
    public DateOnly? FecVen { get; set; }
    public bool Activo { get; set; }
}

// SP request items (must match SpContracts serialization camelCase)
public sealed class TransferenciaItemSp
{
    public long IdPresProd { get; set; }
    public long? IdLote { get; set; }
    public decimal Cantidad { get; set; }
}

public sealed class AjusteItemSp
{
    public long IdPresProd { get; set; }
    public long? IdLote { get; set; }
    public decimal CantAjuste { get; set; }
    public decimal CostoUnitario { get; set; }
}

public sealed class TransferirDto
{
    public int IdBodegaOrig { get; set; }
    public int IdBodegaDest { get; set; }
    public List<TransferenciaItemSp> Items { get; set; } = [];
}

public sealed class AjustarDto
{
    public int IdBodega { get; set; }
    public List<AjusteItemSp> Items { get; set; } = [];
}
