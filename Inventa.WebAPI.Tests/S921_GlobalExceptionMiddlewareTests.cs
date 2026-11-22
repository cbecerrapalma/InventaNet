using Inventa.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S92.1: una petición cancelada por el cliente no es un error del servidor (antes salía sin manejar del pipeline).
public sealed class S921_GlobalExceptionMiddlewareTests
{
    private static GlobalExceptionMiddleware Mw(RequestDelegate next) =>
        new(next, NullLogger<GlobalExceptionMiddleware>.Instance);

    [Fact]
    public async Task CancelacionDelCliente_SeManeja_Con499()
    {
        using var cts = new CancellationTokenSource();
        var ctx = new DefaultHttpContext { RequestAborted = cts.Token };
        await cts.CancelAsync();
        await Mw(_ => throw new TaskCanceledException()).InvokeAsync(ctx); // no debe propagarse
        Assert.Equal(499, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task CancelacionSinAbortoDelCliente_SePropaga()
    {
        // Un timeout interno (cliente sigue conectado) no se oculta como 499.
        var ctx = new DefaultHttpContext();
        await Assert.ThrowsAsync<TaskCanceledException>(() => Mw(_ => throw new TaskCanceledException()).InvokeAsync(ctx));
    }

    [Fact]
    public async Task OtraExcepcion_Sigue500ProblemDetails()
    {
        var ctx = new DefaultHttpContext();
        ctx.Response.Body = new MemoryStream();
        await Mw(_ => throw new InvalidOperationException("x")).InvokeAsync(ctx);
        Assert.Equal(500, ctx.Response.StatusCode);
    }
}

