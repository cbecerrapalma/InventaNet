using System.Text.Json.Serialization;

namespace Inventa.Web.Models.Tenant;

public sealed class TenantItemDto
{
    [JsonPropertyName("idTenant")]
    public int IdTenant { get; set; }

    [JsonPropertyName("idTenantPassPlat")]
    public int IdTenantPassPlat { get; set; }

    [JsonPropertyName("codigo")]
    public string Codigo { get; set; } = "";

    [JsonPropertyName("nombre")]
    public string Nombre { get; set; } = "";
}
