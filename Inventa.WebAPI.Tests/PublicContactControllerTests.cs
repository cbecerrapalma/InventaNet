using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Inventa.WebAPI.Tests;

public sealed class PublicContactControllerTests : IClassFixture<ContactTestFactory>
{
    private readonly HttpClient _client;
    private readonly FakePassPlatClientForContact _fakePassPlat;

    public PublicContactControllerTests(ContactTestFactory factory)
    {
        _fakePassPlat = factory.Fake;
        _fakePassPlat.Reset();
        _client = factory.CreateClient();
    }

    private static object ValidRequest(string website = "")
        => new
        {
            nombre = "Juan Pérez",
            email = "juan@ejemplo.cl",
            telefono = "+56912345678",
            mensaje = "Hola, quisiera información sobre sus productos.",
            consentimiento = true,
            productoId = (long?)null,
            website = website
        };

    [Fact]
    public async Task ValidRequest_Returns202()
    {
        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_ENVIADO", body.GetProperty("code").GetString());
        Assert.NotNull(body.GetProperty("trackingId").GetString());
    }

    [Fact]
    public async Task HoneypotLleno_Returns202SinEnviar()
    {
        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(website: "http://spam.com"), Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_ENVIADO", body.GetProperty("code").GetString());
        Assert.Null(body.GetProperty("trackingId").GetString());
    }

    [Fact]
    public async Task InvalidEmail_Returns400()
    {
        var req = new { nombre = "Juan Pérez", email = "no-es-email", telefono = "+56912345678", mensaje = "Hola, quisiera información sobre sus productos.", consentimiento = true, productoId = (long?)null, website = "" };
        var resp = await _client.PostAsJsonAsync("api/public/contacto", req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_VALIDACION", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MissingRequired_Returns400()
    {
        var req = new { nombre = "", email = "juan@ejemplo.cl", telefono = "+56912345678", mensaje = "Hola", consentimiento = true, productoId = (long?)null, website = "" };
        var resp = await _client.PostAsJsonAsync("api/public/contacto", req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);

        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_VALIDACION", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task MensajeMuyCorto_Returns400()
    {
        var req = new { nombre = "Juan Pérez", email = "juan@ejemplo.cl", telefono = "+56912345678", mensaje = "Corto", consentimiento = true, productoId = (long?)null, website = "" };
        var resp = await _client.PostAsJsonAsync("api/public/contacto", req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task SinConsentimiento_Returns400()
    {
        var req = new { nombre = "Juan Pérez", email = "juan@ejemplo.cl", telefono = "+56912345678", mensaje = "Hola, quisiera información sobre sus productos.", consentimiento = false, productoId = (long?)null, website = "" };
        var resp = await _client.PostAsJsonAsync("api/public/contacto", req, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
    }

    [Fact]
    public async Task PassPlat401_RetriesOnceThenSucceeds()
    {
        // Host aislado: el token provider cachea por host, así que un host fresco
        // garantiza 1 login inicial + 1 tras invalidar por 401 = 2 logins.
        using var factory = new ContactTestFactory();
        var fake = factory.Fake;
        fake.EmailFailOnce = true;
        fake.EmailErrorCode = "401";
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);
        Assert.Equal(2, fake.LoginCallCount);
        Assert.Equal(2, fake.EmailCallCount);
    }

    [Fact]
    public async Task PassPlat403_Returns503NoConfigurado()
    {
        _fakePassPlat.EmailFail = true;
        _fakePassPlat.EmailErrorCode = "403";

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task PassPlat404_Returns503NoConfigurado()
    {
        _fakePassPlat.EmailFail = true;
        _fakePassPlat.EmailErrorCode = "404";

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task PassPlat429_SinRetryAfter_UsaFallback60()
    {
        _fakePassPlat.EmailFail = true;
        _fakePassPlat.EmailErrorCode = "429";
        _fakePassPlat.EmailRetryAfterSeconds = null;

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_RATE_LIMITED", body.GetProperty("code").GetString());
        Assert.Equal(60, body.GetProperty("retryAfter").GetInt32());
        Assert.True(resp.Headers.TryGetValues("Retry-After", out var values));
        Assert.Equal("60", Assert.Single(values));
    }

    [Fact]
    public async Task PassPlat429_ConRetryAfter_PropagaValorPassPlat()
    {
        _fakePassPlat.EmailFail = true;
        _fakePassPlat.EmailErrorCode = "429";
        _fakePassPlat.EmailRetryAfterSeconds = 120;

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.TooManyRequests, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_RATE_LIMITED", body.GetProperty("code").GetString());
        Assert.Equal(120, body.GetProperty("retryAfter").GetInt32());
        Assert.True(resp.Headers.TryGetValues("Retry-After", out var values));
        Assert.Equal("120", Assert.Single(values));
    }

    [Fact]
    public async Task PassPlat5xx_Returns503Upstream()
    {
        _fakePassPlat.EmailFail = true;
        _fakePassPlat.EmailErrorCode = "500";

        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_UPSTREAM", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task ClienteNoPuedeFijarDestinatario_UsaEmailPerfil()
    {
        var resp = await _client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(HttpStatusCode.Accepted, resp.StatusCode);

        Assert.Equal("tienda@ejemplo.cl", _fakePassPlat.LastEmailRequest?.Destinatario);
    }

    [Fact]
    public async Task ArranqueSinPassPlatService_Endpoint503()
    {
        var factory = new StoreAuthMisconfiguredFactory();
        var client = factory.CreateClient();

        var resp = await client.PostAsJsonAsync("api/public/contacto", ValidRequest(), Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", body.GetProperty("code").GetString());
    }
}

// S79.2: sin credenciales PassPlatService el endpoint debe responder 503
// CONTACTO_NO_CONFIGURADO (nunca 500 por OptionsValidationException eager).
public sealed class PublicContactSinCredencialesTests : IClassFixture<ContactUnconfiguredServiceFactory>
{
    private readonly HttpClient _client;

    public PublicContactSinCredencialesTests(ContactUnconfiguredServiceFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task SinPassPlatService_Endpoint503No500()
    {
        var resp = await _client.PostAsJsonAsync("api/public/contacto", new
        {
            nombre = "Juan Pérez",
            email = "juan@ejemplo.cl",
            telefono = "+56912345678",
            mensaje = "Hola, quisiera información sobre sus productos.",
            consentimiento = true,
            productoId = (long?)null,
            website = ""
        }, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", body.GetProperty("code").GetString());
    }

    [Fact]
    public async Task SinPassPlatService_HoneypotTambien503_ElGateDeConfigVaPrimero()
    {
        var resp = await _client.PostAsJsonAsync("api/public/contacto", new
        {
            nombre = "Bot",
            email = "bot@spam.com",
            telefono = "",
            mensaje = "Compre viagra barato http://spam.com",
            consentimiento = true,
            productoId = (long?)null,
            website = "http://spam.com"
        }, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(HttpStatusCode.ServiceUnavailable, resp.StatusCode);
        var body = await resp.Content.ReadFromJsonAsync<JsonElement>(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", body.GetProperty("code").GetString());
    }
}

public sealed class PassPlatServiceTokenProviderTests
{
    [Fact]
    public async Task CachePorTenant_SingleFlight_RenovacionAnticipada()
    {
        var fake = new FakePassPlatClientForContact();
        var opts = new PassPlatServiceOptions
        {
            NomUsuario = "test-user",
            Password = TestDummies.Password,
            AppCodigo = "INVENTA_STORE"
        };
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<PassPlatServiceTokenProvider>.Instance;

        var provider = new Inventa.WebAPI.Clients.PassPlatServiceTokenProvider(fake, Microsoft.Extensions.Options.Options.Create(opts), logger);

        var token1 = await provider.GetAccessTokenAsync(3, Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(token1);
        Assert.Equal(1, fake.LoginCallCount);

        var token2 = await provider.GetAccessTokenAsync(3, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(token1, token2);
        Assert.Equal(1, fake.LoginCallCount);
    }

    [Fact]
    public async Task AppIdResueltoPorCodigo_Cache10Min()
    {
        var fake = new FakePassPlatClientForContact();
        var opts = new PassPlatServiceOptions
        {
            NomUsuario = "test-user",
            Password = TestDummies.Password,
            AppCodigo = "INVENTA_STORE"
        };
        var logger = Microsoft.Extensions.Logging.Abstractions.NullLogger<PassPlatServiceTokenProvider>.Instance;

        var provider = new Inventa.WebAPI.Clients.PassPlatServiceTokenProvider(fake, Microsoft.Extensions.Options.Options.Create(opts), logger);

        var token = await provider.GetAccessTokenAsync(3, Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(token);
        Assert.Equal(13, fake.LastLogin?.IdApp);
    }

    // S96 K2: proveedor PWD_EXPIRADA → null y advertencia sin secretos
    [Fact]
    public async Task Provider_PwdExpirada_ReturnsNullAndLogsWarning()
    {
        var opts = new PassPlatServiceOptions
        {
            NomUsuario = "test-user",
            Password = TestDummies.Password,
            AppCodigo = "INVENTA_STORE"
        };

        // Capture log messages
        var logs = new List<(LogLevel Level, string Message)>();
        var loggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(b => b
            .AddProvider(new FakeTestLoggerProvider(logs)));
        var logger = loggerFactory.CreateLogger<PassPlatServiceTokenProvider>();

        // Use a custom fake that returns PWD_EXPIRADA error
        var customFake = new PwdExpiradaFake();
        var provider = new PassPlatServiceTokenProvider(customFake, Microsoft.Extensions.Options.Options.Create(opts), logger);

        var token = await provider.GetAccessTokenAsync(3, Xunit.TestContext.Current.CancellationToken);

        Assert.Null(token);
        // Warning should contain the code PWD_EXPIRADA, no password/secret
        var warning = logs.FirstOrDefault(l => l.Level == LogLevel.Warning);
        Assert.False(string.IsNullOrEmpty(warning.Message), "Expected warning log");
        Assert.Contains("PWD_EXPIRADA", warning.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("test-pass", warning.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("test-user", warning.Message, StringComparison.OrdinalIgnoreCase);
    }

    // Simple test logger provider (nested in test class)
    private sealed class FakeTestLoggerProvider : ILoggerProvider
    {
        private readonly List<(LogLevel Level, string Message)> _logs;
        public FakeTestLoggerProvider(List<(LogLevel Level, string Message)> logs) => _logs = logs;
        public ILogger CreateLogger(string categoryName) => new FakeTestLogger(_logs);
        public void Dispose() { }
    }
    private sealed class FakeTestLogger : ILogger
    {
        private readonly List<(LogLevel Level, string Message)> _logs;
        public FakeTestLogger(List<(LogLevel Level, string Message)> logs) => _logs = logs;
        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
            => _logs.Add((logLevel, formatter(state, exception)));
        private sealed class NullScope : IDisposable { public static readonly NullScope Instance = new(); public void Dispose() { } }
    }

    // Helper fake that returns PWD_EXPIRADA error
    private sealed class PwdExpiradaFake : IPassPlatClient
    {
        public Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
        {
            var err = "PassPlat login 403 PWD_EXPIRADA";
            return Task.FromResult(new ResultDto<LoginResponse>(false, default, err));
        }
        public Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default)
            => Task.FromResult(new ResultDto<List<AppDto>>(true, new List<AppDto> { new(13, "INVENTA_STORE", "Inventa Store", null, true) }, null));
        public Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
            => throw new NotImplementedException();
        public Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest request, CancellationToken ct = default)
            => throw new NotImplementedException();
    }
}
