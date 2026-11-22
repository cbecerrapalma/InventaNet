using System.Text.Json;

namespace Inventa.WebAPI.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger) { _next = next; _logger = logger; }
    // S98 (P3): excepciones que provoca el aborto del cliente: la cancelación misma, la E/S cortada o el
    // comando de BD cancelado (la SqlException «Operación cancelada por el usuario» es DbException),
    // también envueltas (p. ej. DbUpdateException → SqlException).
    private static bool EsPorAbortoDelCliente(Exception ex)
    {
        for (Exception? e = ex; e is not null; e = e.InnerException)
            if (e is OperationCanceledException or IOException or System.Data.Common.DbException) return true;
        return false;
    }

    public async Task InvokeAsync(HttpContext ctx)
    {
        // S57.69: catch TipoPrecioBlockedException eliminado (policy M8.1 retirada).
        try { await _next(ctx); }
        catch (Exception ex) when (ctx.RequestAborted.IsCancellationRequested && EsPorAbortoDelCliente(ex))
        {
            // S92.1: el cliente canceló la petición (navegó o la reemplazó). No es un error del servidor:
            // se responde 499 (Client Closed Request) si aún se puede. S98: la cancelación queda en Debug;
            // la excepción de BD o de E/S que provoca ese aborto, en Information y sin stack (no es Error ni 500).
            var nivel = ex is OperationCanceledException ? LogLevel.Debug : LogLevel.Information;
            _logger.Log(nivel, "Request aborted by client {Method} {Path} type {Type}", ctx.Request.Method, ctx.Request.Path, ex.GetType().Name);
            if (!ctx.Response.HasStarted) ctx.Response.StatusCode = StatusCodes.Status499ClientClosedRequest;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            var cid = ctx.Items["CorrelationId"]?.ToString() ?? ctx.TraceIdentifier;
            _logger.LogError(ex, "Unhandled exception {Method} {Path} correlation {Cid} type {Type}", ctx.Request.Method, ctx.Request.Path, cid, ex.GetType().Name);
            if (ctx.Response.HasStarted) throw;
            ctx.Response.StatusCode = 500;
            ctx.Response.ContentType = "application/problem+json";
            ctx.Response.Headers["X-Correlation-Id"] = cid;
            var problem = new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
                title = "Internal Server Error",
                status = 500,
                detail = "An unexpected error occurred. Use correlationId for support.",
                instance = ctx.Request.Path.Value,
                correlationId = cid,
                traceId = cid
            };
            await ctx.Response.WriteAsJsonAsync(problem);
        }
    }
}
