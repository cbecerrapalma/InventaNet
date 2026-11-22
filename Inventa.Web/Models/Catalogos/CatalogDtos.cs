using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Catalogos;

public sealed class CategoriaDto
{
    public long IdCategoria { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}
public sealed class MarcaDto
{
    public int IdMarca { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}
public sealed class UnidadDto
{
    public int IdUnidad { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
}
public sealed class MetodoPagoDto
{
    public byte IdMetodoPago { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}
public sealed class AtriDefDto
{
    public long IdAtriDef { get; set; }
    public long IdCategoria { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool EsOblig { get; set; }
    public bool Activo { get; set; }
    public List<AtriValDto> Valores { get; set; } = [];
}
public sealed class AtriValDto
{
    public long IdAtriVal { get; set; }
    public long IdAtriDef { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public int Orden { get; set; }
    public bool Activo { get; set; }
}
public sealed class CreateCategoriaDto
{
    [Required, MaxLength(20)] public string Codigo { get; set; } = "";
    [Required, MaxLength(80)] public string Nombre { get; set; } = "";
}
public sealed class CreateMarcaDto
{
    [Required, MaxLength(20)] public string Codigo { get; set; } = "";
    [Required, MaxLength(80)] public string Nombre { get; set; } = "";
}
