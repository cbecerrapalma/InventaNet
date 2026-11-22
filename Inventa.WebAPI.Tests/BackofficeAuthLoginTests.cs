using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S84.4: login backoffice por AppCodigo + GET api/public/apps/{codigo} (8 pruebas, host propio, sin red ni DB).
public sealed class BackofficeAuthLoginTests : IClassFixture<BackofficeAuthFactory>
{
    private readonly HttpClient _client;
    private readonly FakeBackofficePassPlat _fake;

    public BackofficeAuthLoginTests(BackofficeAuthFactory factory)
    {
        _client = factory.CreateClient();
        _fake = factory.Fake;
    }

    private static async Task<JsonDocument> Body(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private static string Err(JsonDocument b) => b.RootElement.GetProperty("error").GetString()!;

    [Fact]
    public async Task Login_AppCodigo_Resuelve_IdApp()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "admin_abarrotes", password = TestDummies.Password, idTenant = 2, appCodigo = "INVENTA" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(12, _fake.LastLogin!.IdApp);
        Assert.Equal("admin_abarrotes", _fake.LastLogin!.NomUsuario);
        var b = await Body(r);
        Assert.Equal("AT-12", b.RootElement.GetProperty("accessToken").GetString());
    }

    [Fact]
    public async Task Login_AppCodigo_CaseInsensitive()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = TestDummies.Password, idTenant = 1, appCodigo = "inventa" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(12, _fake.LastLogin!.IdApp);
    }

    [Fact]
    public async Task Login_Legacy_IdApp_SinCambios_NiResolucion()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = TestDummies.Password, idTenant = 1, idApp = 12 }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(12, _fake.LastLogin!.IdApp);
        Assert.Equal(0, _fake.ActivasCalls); // sin AppCodigo no se toca el catálogo
    }

    [Fact]
    public async Task Login_AppCodigo_Desconocido_503_SinLlamarLogin()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = TestDummies.Password, idTenant = 1, appCodigo = "NOEXISTE" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("AUTH_APP_NO_CONFIGURADA", Err(await Body(r)));
        Assert.Null(_fake.LastLogin);
    }

    [Fact]
    public async Task Login_AppCodigo_UpstreamCaido_503()
    {
        _fake.Reset();
        _fake.ActivasUnreachable = true;
        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = TestDummies.Password, idTenant = 1, appCodigo = "INVENTA" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("AUTH_APP_NO_CONFIGURADA", Err(await Body(r)));
        Assert.Null(_fake.LastLogin);
    }

    [Fact]
    public async Task Apps_GetByCodigo_200_SinId()
    {
        _fake.Reset();
        var r = await _client.GetAsync("/api/public/apps/INVENTA", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("INVENTA", b.RootElement.GetProperty("codigo").GetString());
        Assert.Equal("Inventa POS", b.RootElement.GetProperty("nombre").GetString());
        Assert.False(b.RootElement.TryGetProperty("id", out _));
        Assert.False(b.RootElement.TryGetProperty("Id", out _));
    }

    [Fact]
    public async Task Apps_GetByCodigo_404()
    {
        _fake.Reset();
        var r = await _client.GetAsync("/api/public/apps/NOEXISTE", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.NotFound, r.StatusCode);
        Assert.Equal("APP_DESCONOCIDA", Err(await Body(r)));
    }

    [Fact]
    public async Task Apps_GetByCodigo_UpstreamCaido_503()
    {
        _fake.Reset();
        _fake.ActivasUnreachable = true;
        var r = await _client.GetAsync("/api/public/apps/INVENTA", Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("AUTH_UPSTREAM", Err(await Body(r)));
    }

    // S96 K2: backoffice PWD_EXPIRADA → 403 LOGIN_PWD_EXPIRADA
    [Fact]
    public async Task Login_PwdExpirada_403_LoginPwdExpirada()
    {
        _fake.Reset();
        _fake.LoginErrorCode = "PWD_EXPIRADA";
        _fake.LoginErrorStatus = 403;

        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = "x", idTenant = 1, appCodigo = "INVENTA" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("LOGIN_PWD_EXPIRADA", b.RootElement.GetProperty("codigo").GetString());
        Assert.Contains("venc", b.RootElement.GetProperty("mensaje").GetString() ?? "", StringComparison.OrdinalIgnoreCase);
    }

    // S96 K2: 401 → 401 LOGIN_CREDENCIALES_INVALIDAS (sin cambio)
    [Fact]
    public async Task Login_Unauthorized_401_LoginCredencialesInvalidas()
    {
        _fake.Reset();
        _fake.LoginErrorCode = "LOGIN_FAILED";
        _fake.LoginErrorStatus = 401;

        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = "x", idTenant = 1, appCodigo = "INVENTA" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("LOGIN_CREDENCIALES_INVALIDAS", b.RootElement.GetProperty("codigo").GetString());
    }

    // S96 K2: 403 otro → 403 LOGIN_TENANT_BLOQUEADO (sin cambio)
    [Fact]
    public async Task Login_OtherForbidden_403_LoginTenantBloqueado()
    {
        _fake.Reset();
        _fake.LoginErrorCode = "SIN_ACCESO_TENANT";
        _fake.LoginErrorStatus = 403;

        var r = await _client.PostAsJsonAsync("/api/auth/login",
            new { nomUsuario = "u", password = "x", idTenant = 1, appCodigo = "INVENTA" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("LOGIN_TENANT_BLOQUEADO", b.RootElement.GetProperty("codigo").GetString());
    }
}

public sealed class FakeBackofficePassPlat : IPassPlatClient
{
    public List<AppDto>? Activas;
    public bool ActivasUnreachable;
    public LoginRequest? LastLogin;
    public int ActivasCalls;
    // S96: controles de error para LoginAsync
    public string? LoginErrorCode; // "PWD_EXPIRADA", "LOGIN_FAILED", "FORBIDDEN", etc.
    public int LoginErrorStatus = 403; // status HTTP a simular (401, 403, etc.)

    public void Reset()
    {
        Activas = new List<AppDto>
        {
            new(13, "INVENTA_STORE", "Inventa Store", null, true),
            new(12, "INVENTA", "Inventa POS", null, true),
        };
        ActivasUnreachable = false;
        LastLogin = null;
        ActivasCalls = 0;
        LoginErrorCode = null;
        LoginErrorStatus = 403;
    }

    public Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        LastLogin = req;
        if (!string.IsNullOrEmpty(LoginErrorCode))
        {
            // Formato que espera AuthController: "PassPlat login <status> <code>"
            var err = $"PassPlat login {LoginErrorStatus} {LoginErrorCode}";
            return Task.FromResult(new ResultDto<LoginResponse>(false, default, err));
        }
        return Task.FromResult(new ResultDto<LoginResponse>(true,
            new LoginResponse("AT-" + req.IdApp, "RT-" + req.IdTenant, DateTime.UtcNow.AddMinutes(60), 1, req.IdTenant, "u"), null));
    }

    public Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 no cubre mis-tenants");

    public Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 no cubre permisos");

    public Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 usa activas anonimo");

    public Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 es backoffice");

    public Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 es backoffice");

    public Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 es backoffice");

    public Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default)
    {
        ActivasCalls++;
        return Task.FromResult(ActivasUnreachable || Activas == null
            ? new ResultDto<List<AppDto>>(false, default, "PassPlat unavailable")
            : new ResultDto<List<AppDto>>(true, Activas, null));
    }

    public Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("S84.4 no cubre email");
}

public class BackofficeAuthFactory : WebApplicationFactory<Program>
{
    public readonly FakeBackofficePassPlat Fake = new();
    public BackofficeAuthFactory() => Fake.Reset();
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((ctx, c) =>
        {
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PassPlatService:NomUsuario"] = "test-user",
                ["PassPlatService:Password"] = "test-pass",
                ["PassPlatService:AppCodigo"] = "INVENTA",
            });
        })
        .ConfigureServices(s =>
        {
            s.Configure<PassPlatServiceOptions>(opts =>
            {
                opts.NomUsuario = "test-user";
                opts.Password = TestDummies.Password;
                opts.AppCodigo = "INVENTA";
            });
            foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService) || d.ServiceType == typeof(IPassPlatClient)).ToList())
                s.Remove(d);
            s.AddSingleton<IPassPlatClient>(Fake);
        });
}

