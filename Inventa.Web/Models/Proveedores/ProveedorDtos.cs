using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Proveedores;

public sealed class ProveedorDto
{
    public int IdProveedor { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string RazonSocial { get; set; } = "";
    public string? NombreComercial { get; set; }
    public string? RUT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public string? Contacto { get; set; }
    public bool Activo { get; set; }
    public DateTime FecCrea { get; set; }
}

public sealed class CreateProveedorDto
{
    [Required, MaxLength(20)]
    public string Codigo { get; set; } = "";
    [Required, MaxLength(200)]
    public string RazonSocial { get; set; } = "";
}
