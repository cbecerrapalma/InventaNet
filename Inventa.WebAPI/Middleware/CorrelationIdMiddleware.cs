namespace Inventa.WebAPI.Middleware;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;
    public async Task InvokeAsync(HttpContext ctx)
    {
        var cid = ctx.Request.Headers["X-Correlation-Id"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(cid))
            cid = ctx.Request.Headers["X-Correlation-ID"].FirstOrDefault();
        if (string.IsNullOrWhiteSpace(cid))
            cid = Guid.NewGuid().ToString();
        // single correlation id per request, do not duplicate headers
        ctx.Items["CorrelationId"] = cid;
        ctx.Response.Headers["X-Correlation-Id"] = cid;
        ctx.Response.Headers["X-Correlation-ID"] = cid;
        using (ctx.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>().BeginScope(new Dictionary<string, object> { ["CorrelationId"] = cid }))
        {
            await _next(ctx);
        }
    }
}
