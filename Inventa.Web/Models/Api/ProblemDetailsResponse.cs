using System.Text.Json.Serialization;

namespace Inventa.Web.Models.Api;

/// <summary>
/// RFC 7807 ProblemDetails as returned by Inventa.WebAPI.
/// </summary>
public sealed class ProblemDetailsResponse
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("status")]
    public int? Status { get; set; }

    [JsonPropertyName("detail")]
    public string? Detail { get; set; }

    [JsonPropertyName("instance")]
    public string? Instance { get; set; }

    [JsonPropertyName("correlationId")]
    public string? CorrelationId { get; set; }

    [JsonPropertyName("errors")]
    public Dictionary<string, string[]>? Errors { get; set; }

    // Allow extra fields without failing deserialization
    [JsonExtensionData]
    public Dictionary<string, System.Text.Json.JsonElement>? Extensions { get; set; }
}
