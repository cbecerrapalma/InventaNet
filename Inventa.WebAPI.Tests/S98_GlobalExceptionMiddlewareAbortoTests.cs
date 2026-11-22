using System.Data.Common;
using Inventa.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S98 (P3): la excepción de BD o de E/S que provoca el aborto del cliente (p. ej. SqlException
// «Operación cancelada por el usuario») no es un error del servidor: 499 y sin log Error.
// Una excepción ajena al aborto sigue siendo 500 + Error aunque el cliente se haya ido.
public sealed class S98_GlobalExceptionMiddlewareAbortoTests
{
    private sealed class DbExceptionDePrueba(string mensaje) : DbException(mensaje);

    private sealed class LogCaptura : ILogger<GlobalExceptionMiddleware>
    {
        public List<LogLevel> Niveles { get; } = [];
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter) => Niveles.Add(logLevel);
    }

    private static async Task<(int Estado, List<LogLevel> Niveles)> InvocarAsync(Exception ex, bool clienteAbortado)
    {
        using var cts = new CancellationTokenSource();
        if (clienteAbortado) await cts.CancelAsync();
        var ctx = new DefaultHttpContext { RequestAborted = cts.Token };
        ctx.Response.Body = new MemoryStream();
        var log = new LogCaptura();
        await new GlobalExceptionMiddleware(_ => throw ex, log).InvokeAsync(ctx);
        return (ctx.Response.StatusCode, log.Niveles);
    }

    [Fact]
    public async Task ExcepcionDeBd_ConClienteAbortado_499SinError()
    {
        var (estado, niveles) = await InvocarAsync(new DbExceptionDePrueba("Operación cancelada por el usuario."), clienteAbortado: true);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, estado);
        Assert.Equal(LogLevel.Information, Assert.Single(niveles));
    }

    [Fact]
    public async Task ExcepcionConBdInterna_ConClienteAbortado_499SinError()
    {
        // Como DbUpdateException → SqlException: la causa está en InnerException.
        var envuelta = new InvalidOperationException("envoltura", new DbExceptionDePrueba("Operación cancelada por el usuario."));
        var (estado, niveles) = await InvocarAsync(envuelta, clienteAbortado: true);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, estado);
        Assert.Equal(LogLevel.Information, Assert.Single(niveles));
    }

    [Fact]
    public async Task IOException_ConClienteAbortado_499SinError()
    {
        var (estado, niveles) = await InvocarAsync(new IOException("conexión cerrada"), clienteAbortado: true);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, estado);
        Assert.Equal(LogLevel.Information, Assert.Single(niveles));
    }

    [Fact]
    public async Task Cancelacion_ConClienteAbortado_499EnDebug()
    {
        var (estado, niveles) = await InvocarAsync(new TaskCanceledException(), clienteAbortado: true);
        Assert.Equal(StatusCodes.Status499ClientClosedRequest, estado);
        Assert.Equal(LogLevel.Debug, Assert.Single(niveles));
    }

    [Fact]
    public async Task ExcepcionDeBd_SinAborto_500ConError()
    {
        // Un fallo de BD con el cliente conectado sigue siendo un error del servidor.
        var (estado, niveles) = await InvocarAsync(new DbExceptionDePrueba("timeout"), clienteAbortado: false);
        Assert.Equal(StatusCodes.Status500InternalServerError, estado);
        Assert.Equal(LogLevel.Error, Assert.Single(niveles));
    }

    [Fact]
    public async Task ExcepcionAjena_ConClienteAbortado_500ConError()
    {
        // Un bug real no se esconde aunque coincida con un aborto del cliente.
        var (estado, niveles) = await InvocarAsync(new InvalidOperationException("bug"), clienteAbortado: true);
        Assert.Equal(StatusCodes.Status500InternalServerError, estado);
        Assert.Equal(LogLevel.Error, Assert.Single(niveles));
    }
}
