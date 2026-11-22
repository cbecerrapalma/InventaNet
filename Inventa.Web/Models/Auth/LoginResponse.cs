using System.Text.Json.Serialization;

namespace Inventa.Web.Models.Auth;

public sealed class LoginResponse
{
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = "";

    [JsonPropertyName("refreshToken")]
    public string? RefreshToken { get; set; }

    [JsonPropertyName("expiresAt")]
    public DateTime? ExpiresAt { get; set; }

    [JsonPropertyName("idUsuario")]
    public int? IdUsuario { get; set; }

    [JsonPropertyName("idTenant")]
    public int? IdTenant { get; set; }

    [JsonPropertyName("nomUsuario")]
    public string? NomUsuario { get; set; }
}
