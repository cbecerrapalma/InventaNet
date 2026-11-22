namespace Inventa.Web.Models.Api;

/// <summary>
/// Normalized error for UI consumption. No stack/SQL/JWT/Secret.
/// </summary>
public sealed class ApiError
{
    public int StatusCode { get; init; }
    public string? Title { get; init; }
    public string? Detail { get; init; }
    public Dictionary<string, string[]>? Errors { get; init; }
    public string? CorrelationId { get; init; }
    public string? RawBody { get; init; }

    public bool IsValidation => StatusCode == 422 || (StatusCode == 400 && Errors is not null);
    public bool IsUnauthorized => StatusCode == 401;
    public bool IsForbidden => StatusCode == 403;
    public bool IsNotFound => StatusCode == 404;
    public bool IsConflict => StatusCode == 409;
    public bool IsUnavailable => StatusCode == 503;
}
