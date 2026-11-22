using System.Security.Claims;
using Bunit;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas puras: StoreAuthenticationStateProvider estado sesion.</summary>
public sealed class StoreAuthStateProviderTests : IDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IStoreAuthService> _authMock = new();
    private readonly Mock<ILogger<StoreAuthenticationStateProvider>> _loggerMock = new();

    public StoreAuthStateProviderTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton(_authMock.Object);
        _ctx.Services.AddSingleton(_loggerMock.Object);
        _ctx.Services.AddScoped<StoreAuthenticationStateProvider>();
        _ctx.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StoreAuthenticationStateProvider>());
        _ctx.Services.AddAuthorizationCore();
    }

    public void Dispose() => _ctx.Dispose();

    private StoreAuthenticationStateProvider GetProvider() => _ctx.Services.GetRequiredService<StoreAuthenticationStateProvider>();

    [Fact]
    public async Task GetAuthenticationStateAsync_NoSession_ReturnsAnonymous()
    {
        _authMock.Setup(x => x.GetUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimsPrincipal?)null);

        var provider = GetProvider();
        var state = await provider.GetAuthenticationStateAsync();

        Assert.False(state.User.Identity?.IsAuthenticated ?? false);
    }

    [Fact]
    public async Task GetAuthenticationStateAsync_ValidSession_ReturnsAuthenticated()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("name", "Juan") }, "jwt-display"));
        _authMock.Setup(x => x.GetUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);

        var provider = GetProvider();
        var state = await provider.GetAuthenticationStateAsync();

        Assert.True(state.User.Identity?.IsAuthenticated);
        Assert.Equal("Juan", state.User.GetName());
    }

    [Fact]
    public async Task NotifyUserChanged_TriggersStateChanged()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[] { new Claim("name", "Juan") }, "jwt-display"));
        _authMock.Setup(x => x.GetUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(principal);

        var provider = GetProvider();
        var state1 = await provider.GetAuthenticationStateAsync();
        Assert.True(state1.User.Identity?.IsAuthenticated);

        _authMock.Setup(x => x.GetUserAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync((ClaimsPrincipal?)null);

        provider.NotifyUserChanged();

        var state2 = await provider.GetAuthenticationStateAsync();
        Assert.False(state2.User.Identity?.IsAuthenticated);
    }

    [Fact]
    public void Constructor_RegistersAsAuthenticationStateProvider()
    {
        var provider = _ctx.Services.GetRequiredService<AuthenticationStateProvider>();
        Assert.IsType<StoreAuthenticationStateProvider>(provider);
    }
}
