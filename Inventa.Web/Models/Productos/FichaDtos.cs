// S92: DTOs de lectura de la administración de productos (GET api/productos/{id}/ficha, /listado, /resumen).
namespace Inventa.Web.Models.Productos;

public sealed class FichaPrecioDto
{
    public long Id { get; set; }
    public decimal Valor { get; set; }
    public DateOnly Desde { get; set; }
}

public sealed class FichaOfertaDto
{
    public decimal Precio { get; set; }
    public DateOnly Desde { get; set; }
    public DateOnly? Hasta { get; set; }
    public int? Pct { get; set; }
}

public sealed class FichaCodigoDto
{
    public string Codigo { get; set; } = "";
    public string Tipo { get; set; } = "";
    public bool Principal { get; set; }
    public long IdCodBar { get; set; }   // S94
    public byte TipoCod { get; set; }    // S94: 1 Normal, 3 Interno, 4 Proveedor
}

public sealed class FichaPresentacionDto
{
    public long IdPresProd { get; set; }
    public string Descrip { get; set; } = "";
    public bool Activo { get; set; }
    public bool EsVenta { get; set; }
}


public sealed class FichaVarianteDto
{
    public long IdVarProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
    public decimal CostoProm { get; set; }
    public FichaPresentacionDto? Presentacion { get; set; }
    public List<FichaCodigoDto> Codigos { get; set; } = [];
    public FichaPrecioDto? PrecioVigente { get; set; }
    public FichaOfertaDto? Oferta { get; set; }
    public decimal StockDisponible { get; set; }
}

public sealed class FichaProductoDto
{
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Marca { get; set; }
    public string Unidad { get; set; } = "";
    public bool EsPesable { get; set; }
    public bool ManejaLote { get; set; }
    public int? DiasVencimiento { get; set; }
    public bool Activo { get; set; }
    public bool VisibleStore { get; set; }
    public int Imagenes { get; set; }
    public string? ImagenPrincipal { get; set; }
    public string Estado { get; set; } = "";
    public List<string> Razones { get; set; } = [];
    public List<FichaVarianteDto> Variantes { get; set; } = [];
}

public sealed class ProductoListadoItemDto
{
    public long IdProd { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Marca { get; set; }
    public bool Activo { get; set; }
    public bool VisibleStore { get; set; }
    public string Estado { get; set; } = "";
    public List<string> Razones { get; set; } = [];
    public bool TieneMarca { get; set; }
    public bool TieneImagen { get; set; }
    public bool Vendible { get; set; }
    public bool TienePrecio { get; set; }
    public int Variantes { get; set; }
    public decimal? PrecioMin { get; set; }
    public decimal? PrecioMax { get; set; }
}

public sealed class ProductoResumenDto
{
    public int Total { get; set; }
    public int Publicados { get; set; }
    public int ConAdvertencia { get; set; }
    public int Listos { get; set; }
    public int Pendientes { get; set; }
    public int Inactivos { get; set; }
    public int SinPrecio { get; set; }
    public int SinImagen { get; set; }
    public int SinMarca { get; set; }
    public int SinVarianteVendible { get; set; }
}

/// <summary>S92: textos y colores del estado en tienda (códigos del backend, PublicCatalogSpecs.EstadosTienda).</summary>
public static class EstadoTiendaUi
{
    public static string Texto(string estado) => estado switch
    {
        "Publicado" => "Publicado",
        "PublicadoConAdvertencia" => "Publicado con advertencia",
        "Listo" => "Listo para publicar",
        "Pendiente" => "Pendiente",
        "Inactivo" => "Inactivo",
        _ => estado,
    };

    public static MudBlazor.Color Color(string estado) => estado switch
    {
        "Publicado" => MudBlazor.Color.Success,
        "PublicadoConAdvertencia" => MudBlazor.Color.Warning,
        "Listo" => MudBlazor.Color.Info,
        "Pendiente" => MudBlazor.Color.Default,
        _ => MudBlazor.Color.Error,
    };
}
