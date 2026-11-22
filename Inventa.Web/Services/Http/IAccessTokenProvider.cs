namespace Inventa.Web.Services.Http;

/// <summary>
/// Skeleton for S54.1. S54.2 will inject TokenStorage-backed implementation.
/// Returns null until authentication is implemented — no hard-coded token.
/// </summary>
public interface IAccessTokenProvider
{
    Task<string?> GetAccessTokenAsync(CancellationToken ct = default);
}
