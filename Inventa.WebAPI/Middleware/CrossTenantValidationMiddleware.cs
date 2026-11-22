using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Inventa.WebAPI.Middleware;

/// <summary>
/// Validates X-Tenant-Id header matches the tenant claim (tid/TenantId) from the JWT.
/// Returns 403 if mismatch. This is the shared contract with Inventa.Store.
/// </summary>
public class CrossTenantValidationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<CrossTenantValidationMiddleware> _logger;

    public CrossTenantValidationMiddleware(RequestDelegate next, ILogger<CrossTenantValidationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only validate on authenticated requests with Bearer token
        if (context.User?.Identity?.IsAuthenticated == true)
        {
            var headerTenant = context.Request.Headers["X-Tenant-Id"].FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(headerTenant) && int.TryParse(headerTenant, out var headerTid))
            {
                // Get tenant claim from JWT (tid / TenantId / tenantid)
                var tidClaim = context.User?.FindFirst("tid")?.Value
                            ?? context.User?.FindFirst("TenantId")?.Value
                            ?? context.User?.FindFirst("tenantid")?.Value;
                if (int.TryParse(tidClaim, out var jwtTid))
                {
                    if (headerTid != jwtTid)
                    {
                        _logger.LogWarning("Cross-tenant violation: X-Tenant-Id={Header} != JWT tid={JwtTid}", headerTid, jwtTid);
                        var cid = context.Items["CorrelationId"]?.ToString() ?? context.TraceIdentifier;
                        context.Response.StatusCode = 403;
                        context.Response.ContentType = "application/problem+json";
                        await context.Response.WriteAsJsonAsync(new
                        {
                            type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                            title = "Forbidden",
                            status = 403,
                            detail = "X-Tenant-Id header does not match authenticated tenant",
                            instance = context.Request.Path.Value,
                            errorCode = "CROSS_TENANT_VIOLATION",
                            traceId = cid,
                            correlationId = cid
                        });
                        return;
                    }
                }
                else
                {
                    _logger.LogWarning("Cross-tenant validation skipped: no tid claim in token");
                }
            }
        }
        await _next(context);
    }
}

public static class CrossTenantValidationMiddlewareExtensions
{
    public static IApplicationBuilder UseCrossTenantValidation(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CrossTenantValidationMiddleware>();
    }
}