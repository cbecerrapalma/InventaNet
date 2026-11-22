using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Bunit;
using Inventa.Store.Models;
using Inventa.Store.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.Infrastructure;
using Moq;
using MudBlazor.Services;
using RichardSzalay.MockHttp;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas puras: StoreAuthService login/refresh/logout con HttpMessageHandler falso.</summary>
public sealed class StoreAuthServiceTests : IDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly MockHttpMessageHandler _httpMock = new();
    private readonly Mock<ILogger<StoreAuthService>> _logger = new();
    private readonly Mock<ILogger<StoreSessionStorage>> _storageLogger = new();
    private readonly TestJSRuntime _jsRuntime = new();

    public StoreAuthServiceTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton(_logger.Object);
        _ctx.Services.AddSingleton(_storageLogger.Object);
        _ctx.Services.AddSingleton<IJSRuntime>(_jsRuntime);
        _ctx.Services.AddScoped<StoreSessionStorage>();
        _ctx.Services.AddScoped(sp => new HttpClient(_httpMock) { BaseAddress = new Uri("https://api.test/") });
        _ctx.Services.AddScoped<StoreAuthService>();
    }

    // Implementación simple de IJSRuntime para testing
    private sealed class TestJSRuntime : IJSRuntime
    {
        private readonly Dictionary<string, string> _storage = new();

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            return InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "sessionStorage.setItem")
            {
                var key = args?[0]?.ToString();
                var value = args?[1]?.ToString();
                if (key != null && value != null) _storage[key] = value;
                if (typeof(TValue) == typeof(IJSVoidResult))
                    return ValueTask.FromResult((TValue)(object)Mock.Of<IJSVoidResult>());
                return ValueTask.FromResult(default(TValue)!);
            }
            if (identifier == "sessionStorage.getItem")
            {
                var key = args?[0]?.ToString();
                if (key != null && _storage.TryGetValue(key, out var value))
                    return ValueTask.FromResult((TValue)(object)value);
                return ValueTask.FromResult(default(TValue)!);
            }
            if (identifier == "sessionStorage.removeItem")
            {
                var key = args?[0]?.ToString();
                if (key != null) _storage.Remove(key);
                if (typeof(TValue) == typeof(IJSVoidResult))
                    return ValueTask.FromResult((TValue)(object)Mock.Of<IJSVoidResult>());
                return ValueTask.FromResult(default(TValue)!);
            }
            return ValueTask.FromResult(default(TValue)!);
        }
    }

    public void Dispose()
    {
        _ctx.Dispose();
        _httpMock.Dispose();
    }

    private StoreAuthService GetService() => _ctx.Services.GetRequiredService<StoreAuthService>();

    [Fact]
    public async Task Login_Success_SavesSessionAndReturnsTokens()
    {
        var tokenPair = new StoreTokenPair("access-123", "refresh-456");
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/login")
            .Respond(req => new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(tokenPair)
            });
        // Fallback para ver qué URL se está llamando
        _httpMock.When(HttpMethod.Post, "*")
            .Respond(req => new HttpResponseMessage(HttpStatusCode.NotFound));

        var svc = GetService();
        var result = await svc.LoginAsync("user@test.cl", "password123", Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.NotNull(result.Data);
        Assert.Equal("access-123", result.Data!.AccessToken);
        Assert.Equal("refresh-456", result.Data.RefreshToken);

        // verificar sessionStorage llamado
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        var session = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(session);
        Assert.Equal("access-123", session!.AccessToken);
        Assert.Equal("refresh-456", session.RefreshToken);
    }

    [Fact]
    public async Task Login_InvalidCredentials_ReturnsFailWithCode()
    {
        var problem = new { status = 401, title = "No autenticado", detail = "Usuario o contraseña incorrectos.", code = "STORE_AUTH_INVALID_CREDENTIALS", correlationId = "trace-123" };
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/login")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = JsonContent.Create(problem)
            }));

        var svc = GetService();
        var result = await svc.LoginAsync("user@test.cl", "wrong", Xunit.TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.NotNull(result.Problem);
        Assert.Equal("STORE_AUTH_INVALID_CREDENTIALS", result.Problem!.Code);
        Assert.Equal(401, result.Problem.Status);
    }

    [Fact]
    public async Task Login_ValidationError_ReturnsFail()
    {
        var problem = new { status = 400, title = "Solicitud inválida", detail = "Identificador requerido.", code = "STORE_AUTH_VALIDATION", correlationId = "trace-123" };
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/login")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = JsonContent.Create(problem)
            }));

        var svc = GetService();
        var result = await svc.LoginAsync("", "password", Xunit.TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("STORE_AUTH_VALIDATION", result.Problem!.Code);
    }

    [Fact]
    public async Task Login_UpstreamError_ReturnsFail()
    {
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/login")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)));

        var svc = GetService();
        var result = await svc.LoginAsync("user@test.cl", "password", Xunit.TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("STORE_AUTH_UPSTREAM", result.Problem!.Code);
    }

    [Fact]
    public async Task Refresh_Success_UpdatesSession()
    {
        // setup: existing session
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        await storage.SaveAsync(new StoreSession("old-access", "old-refresh"), Xunit.TestContext.Current.CancellationToken);

        var newPair = new StoreTokenPair("new-access", "new-refresh");
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/refresh")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(newPair)
            }));

        var svc = GetService();
        var result = await svc.RefreshAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.True(result.Success);
        Assert.Equal("new-access", result.Data!.AccessToken);

        var session = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("new-access", session!.AccessToken);
        Assert.Equal("new-refresh", session.RefreshToken);
    }

    [Fact]
    public async Task Refresh_NoRefreshToken_ClearsAndFails()
    {
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        await storage.SaveAsync(new StoreSession("access-only", null), Xunit.TestContext.Current.CancellationToken);

        var svc = GetService();
        var result = await svc.RefreshAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.False(result.Success);
        Assert.Equal("STORE_AUTH_REFRESH_INVALID", result.Problem!.Code);

        var session = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.Null(session);
    }

    [Fact]
    public async Task Refresh_Unauthorized_ClearsSession()
    {
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        await storage.SaveAsync(new StoreSession("access", "refresh"), Xunit.TestContext.Current.CancellationToken);

        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/refresh")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Unauthorized)
            {
                Content = JsonContent.Create(new { status = 401, code = "STORE_AUTH_REFRESH_INVALID" })
            }));

        var svc = GetService();
        await svc.RefreshAsync(Xunit.TestContext.Current.CancellationToken);

        var session = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.Null(session);
    }

[Fact]
    public async Task Logout_CallsUpstreamAndClearsLocal()
    {
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        await storage.SaveAsync(new StoreSession("access-token", "refresh-token"), Xunit.TestContext.Current.CancellationToken);

        // Verificar que la sesión se carga correctamente
        var sessionBefore = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(sessionBefore);
        Assert.Equal("access-token", sessionBefore!.AccessToken);

        // Mock único para logout - usar URL exacta
        var logoutMock = _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/logout")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.NoContent)));

        var svc = GetService();
        await svc.LogoutAsync(Xunit.TestContext.Current.CancellationToken);

        var session = await storage.LoadAsync<StoreSession>(Xunit.TestContext.Current.CancellationToken);
        Assert.Null(session);

        // Verificar llamada en el mismo mock
        Assert.Equal(1, _httpMock.GetMatchCount(logoutMock));
    }

    [Fact]
    public async Task Logout_NoSession_NoUpstreamCall()
    {
        var svc = GetService();
        await svc.LogoutAsync(Xunit.TestContext.Current.CancellationToken);

        var logoutRequest = _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/logout");
        Assert.Equal(0, _httpMock.GetMatchCount(logoutRequest));
    }

    [Fact]
    public async Task GetUserAsync_NoSession_ReturnsNull()
    {
        var svc = GetService();
        var user = await svc.GetUserAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Null(user);
    }

    [Fact]
    public async Task GetUserAsync_ExpiredToken_TriggersRefresh()
    {
        var storage = _ctx.Services.GetRequiredService<StoreSessionStorage>();
        // token expirado (exp en pasado)
        var exp = DateTimeOffset.UtcNow.AddMinutes(-10).ToUnixTimeSeconds();
        var expiredToken = CreateJwt(exp);
        await storage.SaveAsync(new StoreSession(expiredToken, "refresh-token"), Xunit.TestContext.Current.CancellationToken);

        // refresh devuelve nuevo token valido
        var newExp = DateTimeOffset.UtcNow.AddHours(1).ToUnixTimeSeconds();
        var newToken = CreateJwt(newExp);
        _httpMock.When(HttpMethod.Post, "https://api.test/api/public/auth/refresh")
            .Respond(req => Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = JsonContent.Create(new StoreTokenPair(newToken, "new-refresh"))
            }));

        var svc = GetService();
        var user = await svc.GetUserAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.NotNull(user);
        Assert.Equal("test-user", user.GetName());
    }

    private static string CreateJwt(long exp)
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
        {
            Claims = new System.Collections.Generic.Dictionary<string, object> { { "name", "test-user" }, { "exp", exp } },
            SigningCredentials = null!
        };
        var token = handler.CreateToken(descriptor);
        return handler.WriteToken(token);
    }
}
