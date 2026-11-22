using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Bodegas;

public sealed class BodegaDto
{
    public int IdBodega { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public bool Activo { get; set; }
}

public sealed class CreateBodegaDto
{
    [Required, MaxLength(20)] public string Codigo { get; set; } = "";
    [Required, MaxLength(100)] public string Nombre { get; set; } = "";
}
