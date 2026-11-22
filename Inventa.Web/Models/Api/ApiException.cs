namespace Inventa.Web.Models.Api;

/// <summary>
/// Thrown when WebAPI returns non-success with ProblemDetails.
/// Carries StatusCode + CorrelationId for ErrorState.
/// </summary>
public sealed class ApiException : Exception
{
    public int StatusCode { get; }
    public string? Title { get; }
    public string? CorrelationId { get; }
    public Dictionary<string, string[]>? Errors { get; }
    public string? RawBody { get; }

    public ApiException(ApiError error)
        : base(error.Detail ?? error.Title ?? $"API error {error.StatusCode}")
    {
        StatusCode = error.StatusCode;
        Title = error.Title;
        CorrelationId = error.CorrelationId;
        Errors = error.Errors;
        RawBody = error.RawBody;
    }

    public ApiError ToApiError() => new()
    {
        StatusCode = StatusCode,
        Title = Title,
        Detail = Message,
        Errors = Errors,
        CorrelationId = CorrelationId,
        RawBody = RawBody
    };
}
