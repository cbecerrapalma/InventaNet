using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Inventa.WebAPI.Clients;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S67: contrato POST api/public/auth/login (13 pruebas, host propio).
public sealed class StoreAuthLoginTests : IClassFixture<StoreAuthFactory>, IClassFixture<StoreAuthMisconfiguredFactory>
{
    private const string DummyPwd = "x";
    private const string WrongPwd = "mal";

    private readonly HttpClient _client;
    private readonly FakePassPlatClient _fake;
    private readonly HttpClient _badClient;

    public StoreAuthLoginTests(StoreAuthFactory factory, StoreAuthMisconfiguredFactory badFactory)
    {
        _client = factory.CreateClient();
        _fake = factory.Fake;
        _badClient = badFactory.CreateClient();
    }

    private static async Task<JsonDocument> Body(HttpResponseMessage r) =>
        JsonDocument.Parse(await r.Content.ReadAsStringAsync());

    private static string Code(JsonDocument b) => b.RootElement.GetProperty("code").GetString()!;
    private static string Corr(JsonDocument b) => b.RootElement.GetProperty("correlationId").GetString()!;

    [Fact]
    public async Task Login_200_ConTokens_YNoStore()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "cli@tienda.cl", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal("no-store", string.Join(",", r.Headers.CacheControl?.ToString() ?? ""));
        var b = await Body(r);
        Assert.Equal("AT-13", b.RootElement.GetProperty("accessToken").GetString());
        Assert.Equal("RT-3", b.RootElement.GetProperty("refreshToken").GetString());
        // En 200 la correlacion viaja en el header global (CorrelationIdMiddleware), no en el cuerpo.
        Assert.True(r.Headers.Contains("X-Correlation-Id"));
    }

    [Fact]
    public async Task Login_Ignora_IdApp_IdTenant_DelCuerpo()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login",
            new { identificador = "user1", password = DummyPwd, idApp = 99, idTenant = 1 }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        Assert.Equal(13, _fake.LastLogin!.IdApp);
        Assert.Equal(3, _fake.LastLogin!.IdTenant);
        Assert.Equal("user1", _fake.LastLogin!.NomUsuario);
        Assert.Null(_fake.LastLogin!.Email);
    }

    [Fact]
    public async Task Login_Email_Separa_NomUsuario()
    {
        _fake.Reset();
        await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "a@b.cl", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Null(_fake.LastLogin!.NomUsuario);
        Assert.Equal("a@b.cl", _fake.LastLogin!.Email);
    }

    [Fact]
    public async Task Login_SinIdentificador_400()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "  ", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("STORE_AUTH_VALIDATION", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_PasswordVacio_400()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = "" }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("STORE_AUTH_VALIDATION", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_IdentificadorLargo_400()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = new string('a', 257), password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("STORE_AUTH_VALIDATION", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_PasswordLargo_SinTrim_400()
    {
        _fake.Reset();
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = new string('p', 129) }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("STORE_AUTH_VALIDATION", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_Upstream401_401SinFiltrarSecreto()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(401, "CredencialesInvalidas"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = WrongPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Unauthorized, r.StatusCode);
        var b = await Body(r);
        Assert.Equal("STORE_AUTH_INVALID_CREDENTIALS", Code(b));
        Assert.Equal("CredencialesInvalidas", b.RootElement.GetProperty("upstream").GetString());
        Assert.False(string.IsNullOrWhiteSpace(Corr(b)));
        Assert.Equal("no-store", string.Join(",", r.Headers.CacheControl?.ToString() ?? ""));
    }

    [Fact]
    public async Task Login_Upstream403_403()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(403, "CUENTA_INACTIVA"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        Assert.Equal("STORE_AUTH_FORBIDDEN", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_Upstream400_503NotConfigured()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(400, "LOGIN_VALIDATION"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_NOT_CONFIGURED", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_Mfa_403Tipado()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Ok(new StoreLoginData(null, null, true, false));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        Assert.Equal("STORE_AUTH_MFA_REQUIRED", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_ReqCambioPwd_403Tipado()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Ok(new StoreLoginData("a", "r", false, true));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        Assert.Equal("STORE_AUTH_PASSWORD_CHANGE_REQUIRED", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_Upstream500_503()
    {
        _fake.Reset();
        _fake.OnLoginStore = _ => StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(500, "LOGIN_ERROR"));
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_UPSTREAM", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_AppAusenteDeActivas_503()
    {
        _fake.Reset();
        _fake.Activas = new List<AppDto> { new(12, "INVENTA", "Inventa POS", null, true) };
        var r = await _client.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_NOT_CONFIGURED", Code(await Body(r)));
    }

    [Fact]
    public async Task Login_SinTenantConfigurado_503()
    {
        var r = await _badClient.PostAsJsonAsync("/api/public/auth/login", new { identificador = "u", password = DummyPwd }, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.ServiceUnavailable, r.StatusCode);
        Assert.Equal("STORE_AUTH_NOT_CONFIGURED", Code(await Body(r)));
    }
}

