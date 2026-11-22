using System.Net;
using System.Text;
using System.Security.Claims;
using Bunit;
using Inventa.Store.Models;
using Inventa.Store.Pages;
using Inventa.Store.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S68 pruebas UI: Login.razor + header (bUnit).</summary>
public sealed class StoreLoginPageTests : IDisposable
{
    private readonly BunitContext _ctx = new();
    private readonly Mock<IStoreAuthService> _authMock = new();
    private readonly Mock<ILogger<StoreAuthenticationStateProvider>> _loggerMock = new();

    public StoreLoginPageTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        _ctx.Services.AddSingleton(_authMock.Object);
        _ctx.Services.AddSingleton(_loggerMock.Object);
        _ctx.Services.AddScoped<StoreSessionStorage>();
        _ctx.Services.AddScoped<IStoreAuthService>(sp => _authMock.Object);
        _ctx.Services.AddScoped<StoreAuthenticationStateProvider>();
        _ctx.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<StoreAuthenticationStateProvider>());
        _ctx.Services.AddAuthorizationCore();

        // S83: Login.razor inyecta StorefrontState; registrar stub para perfil
        var http = new HttpClient(new StubStorefrontHandler()) { BaseAddress = new Uri("http://localhost/") };
        var cfg = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["ApiBaseUrl"] = "http://localhost/" })
            .Build();
        _ctx.Services.AddSingleton(new StorefrontApiService(http));
        _ctx.Services.AddSingleton(new StorefrontState(new StorefrontApiService(http), cfg));
    }

    sealed class StubStorefrontHandler : HttpMessageHandler
    {
        const string Json = """{"nombreLegal":"Tienda Demo","nombreFantasia":null,"descripcionCorta":null,"descripcion":null,"logoUrl":null,"contacto":{},"redes":[],"horarios":[],"branding":null}""";
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(Json, Encoding.UTF8, "application/json")
            });
        }
    }

    public void Dispose() => _ctx.Dispose();

    // S71: MudTextField no-Immediate confirma el valor en blur; el envío se
    // hace con form.Submit (el click en MudButton no dispara OnValidSubmit
    // en bUnit). Todo en pruebas, sin cambios de producto.
    static async Task FillLoginAsync(IRenderedComponent<Inventa.Store.Pages.Login> cut, string user, string pass)
    {
        await cut.Find("[data-testid='store-login-identificador']").ChangeAsync(user);
        await cut.Find("[data-testid='store-login-identificador']").BlurAsync();
        await cut.Find("[data-testid='store-login-password']").ChangeAsync(pass);
        await cut.Find("[data-testid='store-login-password']").BlurAsync();
    }

    static async Task SubmitLoginAsync(IRenderedComponent<Inventa.Store.Pages.Login> cut)
    {
        await cut.Find("form[data-testid='store-login-form']").SubmitAsync();
        await cut.InvokeAsync(() => { });
    }

    [Fact]
    public void LoginPage_RendersForm()
    {
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();
        Assert.NotNull(cut.Find("form[data-testid='store-login-form']"));
        Assert.NotNull(cut.Find("[data-testid='store-login-identificador']"));
        Assert.NotNull(cut.Find("[data-testid='store-login-password']"));
        Assert.NotNull(cut.Find("[data-testid='store-login-submit']"));
    }

    // S96.1 D2: la contraseña no se ve al escribir y el gestor de claves la reconoce.
    [Fact]
    public void LoginPage_Password_EsTipoPassword()
    {
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();
        var pwd = cut.Find("input[data-testid='store-login-password']");
        Assert.Equal("password", pwd.GetAttribute("type"));
        Assert.Equal("current-password", pwd.GetAttribute("autocomplete"));
        Assert.Equal("username", cut.Find("input[data-testid='store-login-identificador']").GetAttribute("autocomplete"));
    }

    [Fact]
    public async Task LoginPage_SubmitEmpty_ShowsValidationMessage()
    {
        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();
        await SubmitLoginAsync(cut);

        // El EditForm inválido bloquea el envío: sin llamada al login y con
        // marca de error de MudTextField (mensaje en inglés de DataAnnotations,
        // comportamiento vigente de producto; Login.razor no se toca en S71).
        _authMock.Verify(x => x.LoginAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal("http://localhost/", nav.Uri);
        cut.WaitForAssertion(() => Assert.Contains("mud-input-error",
            cut.Find("[data-testid='store-login-form']").OuterHtml));
    }

    [Fact]
    public async Task LoginPage_Success_NavigatesToReturnUrl()
    {
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Ok(new StoreTokenPair("at", "rt")));

        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        // ReturnUrl es SupplyParameterFromQuery: se fija navegando, no por parámetro.
        nav.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString("/productos/123"));
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Equal("http://localhost/productos/123", nav.Uri));
    }

    [Fact]
    public async Task LoginPage_Success_DefaultReturnUrl_Root()
    {
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Ok(new StoreTokenPair("at", "rt")));

        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Equal("http://localhost/", nav.Uri));
    }

    [Fact]
    public async Task LoginPage_InvalidCredentials_ShowsErrorStaysOnPage()
    {
        var problem = new StoreAuthProblem(401, StoreAuthCodes.InvalidCredentials, "trace-1", null);
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "wrong", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var nav = _ctx.Services.GetRequiredService<NavigationManager>();
        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "wrong");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Equal("http://localhost/", nav.Uri)); // no navega
        var msg = cut.Find("[data-testid='store-login-error']");
        Assert.Contains("incorrectos", msg.TextContent);
    }

    [Fact]
    public async Task LoginPage_Forbidden_ShowsContactMessage()
    {
        var problem = new StoreAuthProblem(403, StoreAuthCodes.Forbidden, "trace-2", null);
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Contains("Contacta a la tienda",
            cut.Find("[data-testid='store-login-error']").TextContent));
    }

    [Fact]
    public async Task LoginPage_MfaRequired_ShowsAdminPanelMessage()
    {
        var problem = new StoreAuthProblem(403, StoreAuthCodes.MfaRequired, "trace-3", null);
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Contains("panel de administración",
            cut.Find("[data-testid='store-login-error']").TextContent));
    }

    [Fact]
    public async Task LoginPage_RateLimited_ShowsWaitMessage()
    {
        var problem = new StoreAuthProblem(429, StoreAuthCodes.RateLimited, "trace-4", null);
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Contains("Demasiados intentos",
            cut.Find("[data-testid='store-login-error']").TextContent));
    }

    [Fact]
    public async Task LoginPage_UpstreamError_ShowsGeneric()
    {
        var problem = new StoreAuthProblem(503, StoreAuthCodes.Upstream, "trace-5", "PASSPLAT_DOWN");
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Contains("Inténtalo de nuevo en unos minutos",
            cut.Find("[data-testid='store-login-error']").TextContent));
    }

    // S96 K2: tienda PWD_EXPIRADA upstream → mismo mensaje que backoffice
    [Fact]
    public async Task LoginPage_PwdExpirada_ShowsSameMessage()
    {
        var problem = new StoreAuthProblem(403, StoreAuthCodes.Forbidden, "trace-6", "PWD_EXPIRADA");
        _authMock.Setup(x => x.LoginAsync("user@test.cl", "pass", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Fail(problem));

        var cut = _ctx.Render<Inventa.Store.Pages.Login>();

        await FillLoginAsync(cut, "user@test.cl", "pass");
        await SubmitLoginAsync(cut);

        cut.WaitForAssertion(() => Assert.Contains("venc",
            cut.Find("[data-testid='store-login-error']").TextContent, StringComparison.OrdinalIgnoreCase));
        cut.WaitForAssertion(() => Assert.Contains("Restabl", // Restablécela / restablece
            cut.Find("[data-testid='store-login-error']").TextContent, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task LoginPage_ReturnUrlSanitized_BlockedPatternsGoToRoot()
    {
        _authMock.Setup(x => x.LoginAsync("u", "p", It.IsAny<CancellationToken>()))
            .ReturnsAsync(StoreAuthResult<StoreTokenPair>.Ok(new StoreTokenPair("at", "rt")));

        var nav = _ctx.Services.GetRequiredService<NavigationManager>();

        foreach (var bad in new[] { "//evil.com", "/\\evil", "http://evil.com", "javascript:alert(1)" })
        {
            nav.NavigateTo("/");
            nav.NavigateTo("/login?returnUrl=" + Uri.EscapeDataString(bad));
            var cut = _ctx.Render<Inventa.Store.Pages.Login>();
            await FillLoginAsync(cut, "u", "p");
            await SubmitLoginAsync(cut);
            cut.WaitForAssertion(() => Assert.Equal("http://localhost/", nav.Uri));
        }
    }
}
