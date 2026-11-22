using System.ComponentModel.DataAnnotations;

namespace Inventa.Web.Models.Clientes;

public sealed class ClienteDto
{
    public int IdCliente { get; set; }
    public int IdTenant { get; set; }
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string? RUT { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? Email { get; set; }
    public byte TipoCliente { get; set; }
    public bool Activo { get; set; }
    public DateTime FecCrea { get; set; }
}

public sealed class CreateClienteDto
{
    [Required, MaxLength(20)]
    public string Codigo { get; set; } = "";
    [Required, MaxLength(120)]
    public string Nombre { get; set; } = "";
    [MaxLength(20)]
    public string? RUT { get; set; }
}
