using System.Net;
using System.Security.Claims;
using System.Text.Json;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Domain.Entities;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Controllers;
using Inventa.WebAPI.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S85: cadena tenant (tid -> IdTenantPassPlat -> IdTenant local) + login 401/403 es-CL.
// Nivel controlador, sin host ni DB.
public sealed class TenantChainTests
{
    private static AuthController NewController(
        FakeS85PassPlat pp, FakeS85Sync sync, FakeS85Tenants tenants)
    {
        var c = new AuthController(pp, sync, tenants);
        c.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        c.Request.Headers.Authorization = "Bearer T";
        return c;
    }

    private static JsonElement Body(IActionResult r)
    {
        var ok = Assert.IsType<OkObjectResult>(r);
        return JsonSerializer.SerializeToElement(ok.Value);
    }

    private static (int Status, JsonElement Json) StatusBody(IActionResult r)
    {
        var o = Assert.IsType<ObjectResult>(r);
        return (o.StatusCode ?? 0, JsonSerializer.SerializeToElement(o.Value));
    }

    [Fact]
    public void TenantDto_Id_Alias_Mapea_A_IdTenant()
    {
        var dto = JsonSerializer.Deserialize<TenantDto>(
            """{"id":3,"codigo":"VESTUARIO","nombre":"Vestuario SA de CV"}""",
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        Assert.NotNull(dto);
        Assert.Equal(3, dto!.IdTenant);
    }

    [Fact]
    public async Task MisTenants_Enriquece_Con_Espejo_Local()
    {
        var pp = new FakeS85PassPlat();
        pp.Tenants.Add(new TenantDto(3, "VESTUARIO", "Vestuario SA de CV"));
        var c = NewController(pp, new FakeS85Sync(new() { [3] = 5 }),
            new FakeS85Tenants(new() { [5] = "Ropa Calzado" }));
        var el = Body(await c.MisTenants(CancellationToken.None));
        var item = el.EnumerateArray().First();
        Assert.Equal(5, item.GetProperty("idTenant").GetInt32());
        Assert.Equal(3, item.GetProperty("idTenantPassPlat").GetInt32());
        Assert.Equal(5, item.GetProperty("idTenantLocal").GetInt32());
        Assert.Equal("Ropa Calzado", item.GetProperty("nombreLocal").GetString());
    }

    [Fact]
    public async Task MisTenants_Sin_Espejo_Nunca_Cero()
    {
        var pp = new FakeS85PassPlat();
        pp.Tenants.Add(new TenantDto(9, "NUEVO", "Nuevo"));
        var c = NewController(pp, new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var el = Body(await c.MisTenants(CancellationToken.None));
        var item = el.EnumerateArray().First();
        Assert.Equal(9, item.GetProperty("idTenant").GetInt32()); // fallback ppId, nunca 0
        Assert.Equal(0, item.GetProperty("idTenantLocal").GetInt32());
        Assert.Equal(JsonValueKind.Null, item.GetProperty("nombreLocal").ValueKind);
    }

    [Fact]
    public async Task MisTenants_Upstream_Caido_503()
    {
        var pp = new FakeS85PassPlat { TenantsFail = "PassPlat down" };
        var c = NewController(pp, new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var (status, _) = StatusBody(await c.MisTenants(CancellationToken.None));
        Assert.Equal(503, status);
    }

    [Fact]
    public async Task Login_Unauthorized_401_EsCL_SinUpstream()
    {
        var pp = new FakeS85PassPlat
        { LoginFail = """PassPlat login Unauthorized {"codigo":"LOGIN_FAILED","mensaje":"Credenciales invalidas"}""" };
        var c = NewController(pp, new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var (status, el) = StatusBody(await c.Login(
            new AuthController.BackofficeLoginRequest { NomUsuario = "u", Password = TestDummies.Password, IdApp = 12, IdTenant = 2 }, CancellationToken.None));
        Assert.Equal(401, status);
        Assert.Equal("LOGIN_CREDENCIALES_INVALIDAS", el.GetProperty("codigo").GetString());
        Assert.DoesNotContain("PassPlat", el.GetRawText());
    }

    [Fact]
    public async Task Login_Forbidden_403_EsCL()
    {
        var pp = new FakeS85PassPlat { LoginFail = "PassPlat login Forbidden SUSPENDED" };
        var c = NewController(pp, new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var (status, el) = StatusBody(await c.Login(
            new AuthController.BackofficeLoginRequest { NomUsuario = "u", Password = TestDummies.Password, IdApp = 12, IdTenant = 2 }, CancellationToken.None));
        Assert.Equal(403, status);
        Assert.Equal("LOGIN_TENANT_BLOQUEADO", el.GetProperty("codigo").GetString());
    }

    [Fact]
    public async Task Login_Otro_Error_503_AuthUpstream()
    {
        var pp = new FakeS85PassPlat { LoginFail = "boom" };
        var c = NewController(pp, new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var (status, el) = StatusBody(await c.Login(
            new AuthController.BackofficeLoginRequest { NomUsuario = "u", Password = TestDummies.Password, IdApp = 12, IdTenant = 2 }, CancellationToken.None));
        Assert.Equal(503, status);
        Assert.Equal("AUTH_UPSTREAM", el.GetProperty("error").GetString());
    }

    [Fact]
    public async Task Login_OK_Passthrough()
    {
        var c = NewController(new FakeS85PassPlat(), new FakeS85Sync(new()), new FakeS85Tenants(new()));
        var r = await c.Login(
            new AuthController.BackofficeLoginRequest { NomUsuario = "u", Password = TestDummies.Password, IdApp = 12, IdTenant = 3 }, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(r);
        Assert.Equal("AT-12", ((LoginResponse)ok.Value!).AccessToken);
    }

    // C1: CrossTenantValidationMiddleware tests
    [Fact]
    public async Task CrossTenantMiddleware_HeaderEqualsTid_Allows()
    {
        var logger = new TestLogger<CrossTenantValidationMiddleware>();
        var middleware = new CrossTenantValidationMiddleware(_ => Task.CompletedTask, logger);
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tid", "3"),
            new Claim("sub", "4")
        }, "Bearer"));
        ctx.Request.Headers["X-Tenant-Id"] = "3";

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode); // passes through
    }

    [Fact]
    public async Task CrossTenantMiddleware_HeaderNotEqualsTid_Returns403()
    {
        var logger = new TestLogger<CrossTenantValidationMiddleware>();
        var middleware = new CrossTenantValidationMiddleware(_ => Task.CompletedTask, logger);
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tid", "3"),
            new Claim("sub", "4")
        }, "Bearer"));
        ctx.Request.Headers["X-Tenant-Id"] = "5"; // local IdTenant, not tid

        await middleware.InvokeAsync(ctx);

        Assert.Equal(403, ctx.Response.StatusCode);
    }

    [Fact]
    public async Task CrossTenantMiddleware_NoHeader_Allows()
    {
        var logger = new TestLogger<CrossTenantValidationMiddleware>();
        var middleware = new CrossTenantValidationMiddleware(_ => Task.CompletedTask, logger);
        var ctx = new DefaultHttpContext();
        ctx.User = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("tid", "3"),
            new Claim("sub", "4")
        }, "Bearer"));
        // no X-Tenant-Id header

        await middleware.InvokeAsync(ctx);

        Assert.Equal(200, ctx.Response.StatusCode); // passes through
    }
}

// Simple test logger
public class TestLogger<T> : ILogger<T> where T : class
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}

public sealed class FakeS85PassPlat : IPassPlatClient
{
    public readonly List<TenantDto> Tenants = new();
    public string? TenantsFail;
    public string? LoginFail;

    public Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default) =>
        Task.FromResult(LoginFail is null
            ? new ResultDto<LoginResponse>(true,
                new LoginResponse("AT-" + req.IdApp, "RT", DateTime.UtcNow.AddMinutes(60), 4, req.IdTenant, "u"), null)
            : new ResultDto<LoginResponse>(false, default, LoginFail));

    public Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default) =>
        Task.FromResult(TenantsFail is null
            ? new ResultDto<List<TenantDto>>(true, Tenants, null)
            : new ResultDto<List<TenantDto>>(false, default, TenantsFail));

    public Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
        => throw new NotImplementedException("S85 no cubre permisos");
    public Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
        => throw new NotImplementedException("S85 no cubre apps");
    public Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
        => throw new NotImplementedException("S85 es backoffice");
    public Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
        => throw new NotImplementedException("S85 es backoffice");
    public Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
        => throw new NotImplementedException("S85 es backoffice");
    public Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default)
        => throw new NotImplementedException("S85 usa IdApp legacy");
    public Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest request, CancellationToken ct = default)
        => throw new NotImplementedException("S85 no cubre email");
}

public sealed class FakeS85Sync : ITenantSyncServiceAsync
{
    private readonly Dictionary<int, int> _map;
    public FakeS85Sync(Dictionary<int, int> map) => _map = map;
    public Task<Result<int>> ResolveLocalTenantIdAsync(int idTenantPassPlat, CancellationToken ct = default) =>
        Task.FromResult(_map.TryGetValue(idTenantPassPlat, out var local)
            ? Result<int>.Success(local)
            : Result<int>.Failure(new Error("TENANT_NOT_SYNCED", "Sin espejo").WithType(ErrorType.NotFound).WithHttpStatus(404)));
    public Task<Result<Tenant>> EnsureTenantAsync(int idTenantPassPlat, string codigo, string nombre, CancellationToken ct = default)
        => throw new NotImplementedException("S85 no crea espejos");
}

public sealed class FakeS85Tenants : ITenantServiceAsync
{
    private readonly Dictionary<int, string> _nombres;
    public FakeS85Tenants(Dictionary<int, string> nombres) => _nombres = nombres;
    public Task<Result<Tenant?>> GetByIdAsync(int idTenant, CancellationToken ct = default) =>
        Task.FromResult(_nombres.TryGetValue(idTenant, out var n)
            ? Result<Tenant?>.Success(new Tenant { IdTenant = idTenant, Codigo = "C" + idTenant, Nombre = n })
            : Result<Tenant?>.Failure(new Error("NOT_FOUND", "No").WithType(ErrorType.NotFound).WithHttpStatus(404)));
    public Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<Tenant?>> GetByCodigoAsync(string codigo, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<Tenant>>> GetActivosAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<int>> CountActivosAsync(CancellationToken ct = default) => throw new NotImplementedException();
}

