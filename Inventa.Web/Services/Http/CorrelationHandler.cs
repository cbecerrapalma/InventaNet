namespace Inventa.Web.Services.Http;

/// <summary>
/// Generates X-Correlation-ID per request if absent.
/// ID is available to ErrorHandler via header.
/// </summary>
public sealed class CorrelationHandler : DelegatingHandler
{
    public const string HeaderName = "X-Correlation-ID";

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains(HeaderName))
        {
            request.Headers.Add(HeaderName, Guid.NewGuid().ToString());
        }

        return base.SendAsync(request, cancellationToken);
    }
}
