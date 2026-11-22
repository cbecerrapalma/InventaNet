namespace Inventa.Web.Services.Http;

/// <summary>
/// Injects X-Tenant-Id header with the PassPlat tenant ID (tid) for API calls.
/// Routes with tenant ID in path (api/tenants/{id}/config) use local IdTenant via the URL.
/// </summary>
public sealed class TenantHttpHandler : DelegatingHandler
{
    private readonly ITenantContextProvider _tenantProvider;

    public TenantHttpHandler(ITenantContextProvider tenantProvider)
    {
        _tenantProvider = tenantProvider;
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var tenantId = await _tenantProvider.GetCurrentTenantPassPlatIdAsync(cancellationToken);
        if (tenantId.HasValue && tenantId.Value > 0)
        {
            request.Headers.Remove("X-Tenant-Id");
            request.Headers.Add("X-Tenant-Id", tenantId.Value.ToString());
        }

        return await base.SendAsync(request, cancellationToken);
    }
}
