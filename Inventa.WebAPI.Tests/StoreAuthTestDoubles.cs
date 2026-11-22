using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using CBP.Results;

namespace Inventa.WebAPI.Tests;

// S67: dobles + factories del contrato api/public/auth (sin red, sin DB).
// Cada clase de pruebas usa su factory (= host propio = presupuesto
// "Public" 20/10s independiente). El probe SQL se retira solo en el host
// de test; producto intacto.
public sealed class FakePassPlatClient : IPassPlatClient
{
    public Func<LoginRequest, StoreAuthResult<StoreLoginData>>? OnLoginStore;
    public Func<string, StoreAuthResult<LoginResponse>>? OnRefreshStore;
    public Func<string, StoreAuthResult<bool>>? OnLogoutStore;
    public List<AppDto>? Activas;
    public bool ActivasUnreachable;
    public LoginRequest? LastLogin;

    public void Reset()
    {
        OnLoginStore = req => StoreAuthResult<StoreLoginData>.Ok(
            new StoreLoginData("AT-" + req.IdApp, "RT-" + req.IdTenant, false, false));
        OnRefreshStore = rt => StoreAuthResult<LoginResponse>.Ok(
            new LoginResponse("AT2", "RT2", DateTime.UtcNow.AddMinutes(60), 7, 3, "u"));
        OnLogoutStore = _ => StoreAuthResult<bool>.Ok(true);
        Activas = new List<AppDto>
        {
            new(13, "INVENTA_STORE", "Inventa Store", null, true),
            new(12, "INVENTA", "Inventa POS", null, true),
        };
        ActivasUnreachable = false;
        LastLogin = null;
    }

    public Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
        => throw new NotImplementedException("backoffice fuera de S67");
    public Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default)
        => throw new NotImplementedException("backoffice fuera de S67");
    public Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
        => throw new NotImplementedException("backoffice fuera de S67");
    public Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
        => throw new NotImplementedException("S67 usa activas anonimo");

    public Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
    {
        LastLogin = req;
        var fn = OnLoginStore ?? (_ => StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE")));
        return Task.FromResult(fn(req));
    }
    public Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
    {
        var fn = OnRefreshStore ?? (_ => StoreAuthResult<LoginResponse>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE")));
        return Task.FromResult(fn(refreshToken));
    }
    public Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
    {
        var fn = OnLogoutStore ?? (_ => StoreAuthResult<bool>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE")));
        return Task.FromResult(fn(accessToken));
    }
    public Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default) =>
        Task.FromResult(ActivasUnreachable || Activas == null
            ? new ResultDto<List<AppDto>>(false, default, "PassPlat unavailable")
            : new ResultDto<List<AppDto>>(true, Activas, null));

    public Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest request, CancellationToken ct = default)
        => Task.FromResult(new ResultDto<EnviarEmailPlantillaResponse>(true, new EnviarEmailPlantillaResponse(1, "enviado"), null));
}

public class StoreAuthFactory : WebApplicationFactory<Program>
{
    public readonly FakePassPlatClient Fake = new();
    public StoreAuthFactory() => Fake.Reset();
    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((ctx, c) =>
        {
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PassPlatService:NomUsuario"] = "test-user",
                ["PassPlatService:Password"] = "test-pass",
                ["PassPlatService:AppCodigo"] = "INVENTA_STORE",
            });
        })
        .ConfigureServices(s =>
        {
            // Override options configuration to provide test values
            s.Configure<PassPlatServiceOptions>(opts =>
            {
                opts.NomUsuario = "test-user";
                opts.Password = TestDummies.Password;
                opts.AppCodigo = "INVENTA_STORE";
            });
            foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService) || d.ServiceType == typeof(IPassPlatClient)).ToList())
                s.Remove(d);
            s.AddSingleton<IPassPlatClient>(Fake);
        });
}

// Factory con config rota (tenant 0 + app inexistente) para NOT_CONFIGURED.
public sealed class StoreAuthMisconfiguredFactory : StoreAuthFactory
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);
        builder.ConfigureAppConfiguration((ctx, c) => c.AddInMemoryCollection(new Dictionary<string, string?>
        {
            ["PublicCatalog:PassPlatTenantId"] = "0",
            ["StoreAuth:AppCodigo"] = "NO_EXISTE",
        }));
    }
}

public sealed class FakePassPlatClientForContact : IPassPlatClient
{
    public List<AppDto> Activas { get; set; } = new()
    {
        new(13, "INVENTA_STORE", "Inventa Store", null, true),
        new(12, "INVENTA", "Inventa POS", null, true),
    };
    public bool ActivasUnreachable { get; set; }
    public bool LoginFail { get; set; }
    public bool EmailFail { get; set; }
    public bool EmailFailOnce { get; set; }
    public string EmailErrorCode { get; set; } = "";
    public int? EmailRetryAfterSeconds { get; set; }
    public int LoginCallCount { get; private set; }
    public int EmailCallCount { get; private set; }
    public LoginRequest? LastLogin { get; private set; }
    public EnviarEmailPlantillaRequest? LastEmailRequest { get; private set; }

    public void Reset()
    {
        ActivasUnreachable = false;
        LoginFail = false;
        EmailFail = false;
        EmailFailOnce = false;
        EmailErrorCode = "";
        EmailRetryAfterSeconds = null;
        LoginCallCount = 0;
        EmailCallCount = 0;
        LastLogin = null;
        LastEmailRequest = null;
    }

    public Task<ResultDto<LoginResponse>> LoginAsync(LoginRequest req, CancellationToken ct = default)
    {
        LoginCallCount++;
        LastLogin = req;
        if (LoginFail)
            return Task.FromResult(new ResultDto<LoginResponse>(false, default, "UPSTREAM_UNREACHABLE"));

        return Task.FromResult(new ResultDto<LoginResponse>(true,
            new LoginResponse("AT-" + req.IdApp, "RT-" + req.IdTenant, DateTime.UtcNow.AddMinutes(60), req.IdApp, req.IdTenant, "user"), null));
    }

    public Task<ResultDto<List<TenantDto>>> GetMisTenantsAsync(string token, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ResultDto<List<string>>> GetPermisosEfectivosAsync(int idUsuario, int idTenant, string idApp, string token, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ResultDto<AppDto>> GetAppByCodigoAsync(string codigo, string token, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<StoreAuthResult<StoreLoginData>> LoginStoreAsync(LoginRequest req, CancellationToken ct = default)
    {
        LoginCallCount++;
        LastLogin = req;
        if (LoginFail)
            return Task.FromResult(StoreAuthResult<StoreLoginData>.Fail(new StoreAuthError(0, "UPSTREAM_UNREACHABLE")));

        return Task.FromResult(StoreAuthResult<StoreLoginData>.Ok(
            new StoreLoginData("AT-" + req.IdApp, "RT-" + req.IdTenant, false, false)));
    }

    public Task<StoreAuthResult<LoginResponse>> RefreshStoreAsync(string refreshToken, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<StoreAuthResult<bool>> LogoutStoreAsync(string accessToken, CancellationToken ct = default)
        => throw new NotImplementedException();

    public Task<ResultDto<List<AppDto>>> GetActivasAsync(CancellationToken ct = default)
        => Task.FromResult(ActivasUnreachable || Activas == null
            ? new ResultDto<List<AppDto>>(false, default, "PassPlat unavailable")
            : new ResultDto<List<AppDto>>(true, Activas, null));

    public Task<ResultDto<EnviarEmailPlantillaResponse>> EnviarEmailPlantillaAsync(string token, EnviarEmailPlantillaRequest request, CancellationToken ct = default)
    {
        EmailCallCount++;
        LastEmailRequest = request;

        // EmailFailOnce: falla solo la primera llamada (para probar reintento 401), luego éxito.
        var shouldFail = EmailFail || (EmailFailOnce && EmailCallCount == 1);
        if (shouldFail)
        {
            var error = EmailErrorCode switch
            {
                "401" => "401 Unauthorized",
                "403" => "403 Forbidden",
                "404" => "404 Not Found",
                "429" => "429 Too Many Requests",
                _ => "500 Internal Server Error"
            };
            return Task.FromResult(new ResultDto<EnviarEmailPlantillaResponse>(false, default, error, EmailRetryAfterSeconds));
        }

        return Task.FromResult(new ResultDto<EnviarEmailPlantillaResponse>(true, new EnviarEmailPlantillaResponse(1, "enviado"), null));
    }
}

// Factory para pruebas de contacto (configura PassPlatService + PublicCatalog tenant)
public sealed class ContactTestFactory : WebApplicationFactory<Program>
{
    public readonly FakePassPlatClientForContact Fake = new();
    public ContactTestFactory() => Fake.Activas = new List<AppDto>
    {
        new(13, "INVENTA_STORE", "Inventa Store", null, true),
        new(12, "INVENTA", "Inventa POS", null, true),
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((ctx, c) =>
        {
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PassPlatService:NomUsuario"] = "test-user",
                ["PassPlatService:Password"] = "test-pass",
                ["PassPlatService:AppCodigo"] = "INVENTA_STORE",
                ["PublicCatalog:PassPlatTenantId"] = "3",
                ["RateLimiter:PublicContact:PermitLimit"] = "10000",
                ["RateLimiter:PublicContact:WindowMinutes"] = "1",
            });
        })
        .ConfigureServices(s =>
        {
            s.Configure<PassPlatServiceOptions>(opts =>
            {
                opts.NomUsuario = "test-user";
                opts.Password = TestDummies.Password;
                opts.AppCodigo = "INVENTA_STORE";
            });
            foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService) || d.ServiceType == typeof(IPassPlatClient) || d.ServiceType == typeof(IStorefrontServiceAsync)).ToList())
                s.Remove(d);
            s.AddSingleton<IPassPlatClient>(Fake);
            s.AddSingleton<IStorefrontServiceAsync>(new FakeStorefrontSvcForContact());
        });
}

// Factory sin credenciales PassPlatService (reproduce runtime sin provisión S78).
// NO inyecta PassPlatService:NomUsuario/Password ni s.Configure<PassPlatServiceOptions>:
// con ValidateDataAnnotations eager esto daba 500; el diseño S79 exige 503 CONTACTO_NO_CONFIGURADO.
public sealed class ContactUnconfiguredServiceFactory : WebApplicationFactory<Program>
{
    public readonly FakePassPlatClientForContact Fake = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((ctx, c) =>
        {
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PublicCatalog:PassPlatTenantId"] = "3",
                ["RateLimiter:PublicContact:PermitLimit"] = "10000",
                ["RateLimiter:PublicContact:WindowMinutes"] = "1",
                // Explícitamente nulos para anular posibles user-secrets
                ["PassPlatService:NomUsuario"] = null,
                ["PassPlatService:Password"] = null,
                ["PassPlatService:AppCodigo"] = null,
            });
        })
        .ConfigureServices(s =>
        {
            foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService) || d.ServiceType == typeof(IPassPlatClient) || d.ServiceType == typeof(IStorefrontServiceAsync)).ToList())
                s.Remove(d);
            s.AddSingleton<IPassPlatClient>(Fake);
            s.AddSingleton<IStorefrontServiceAsync>(new FakeStorefrontSvcForContact());
        });
}

// Factory para prueba de rate limit (usa config por defecto: 5 req / 10 min)
public sealed class ContactRateLimitTestFactory : WebApplicationFactory<Program>
{
    public readonly FakePassPlatClientForContact Fake = new();
    public ContactRateLimitTestFactory() => Fake.Activas = new List<AppDto>
    {
        new(13, "INVENTA_STORE", "Inventa Store", null, true),
        new(12, "INVENTA", "Inventa POS", null, true),
    };

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder.ConfigureAppConfiguration((ctx, c) =>
        {
            c.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["PassPlatService:NomUsuario"] = "test-user",
                ["PassPlatService:Password"] = "test-pass",
                ["PassPlatService:AppCodigo"] = "INVENTA_STORE",
                ["PublicCatalog:PassPlatTenantId"] = "3",
            });
        })
        .ConfigureServices(s =>
        {
            s.Configure<PassPlatServiceOptions>(opts =>
            {
                opts.NomUsuario = "test-user";
                opts.Password = TestDummies.Password;
                opts.AppCodigo = "INVENTA_STORE";
            });
            foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService) || d.ServiceType == typeof(IPassPlatClient) || d.ServiceType == typeof(IStorefrontServiceAsync)).ToList())
                s.Remove(d);
            s.AddSingleton<IPassPlatClient>(Fake);
            s.AddSingleton<IStorefrontServiceAsync>(new FakeStorefrontSvcForContact());
        });
}

internal sealed class FakeStorefrontSvcForContact : IStorefrontServiceAsync
{
    public Task<Result<int>> ResolveStoreTenantAsync(int idTenantPassPlat, CancellationToken ct = default)
        => Task.FromResult(Result<int>.Success(1));

    public Task<Result<IReadOnlyList<ActiveTenantItem>>> GetActiveTenantsAsync(CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<ActiveTenantItem>>.Success([]));

    public Task<Result<(int Total, IReadOnlyList<PublicCardItem> Items)>> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default)
        => Task.FromResult(Result<(int, IReadOnlyList<PublicCardItem>)>.Success((0, [])));

    public Task<Result<PublicDetailView?>> GetPublicDetailViewAsync(int idTenant, long idProd, CancellationToken ct = default)
        => Task.FromResult(Result<PublicDetailView?>.Success(null, allowNull: true));

    public Task<Result<IReadOnlyList<PublicCategoryItem>>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<PublicCategoryItem>>.Success([]));

    public Task<Result<IReadOnlyList<PublicBrandItem>>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<PublicBrandItem>>.Success([]));

    public Task<Result<StorefrontProfileBundle?>> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default)
    {
        var tenant = new TenantStorefront
        {
            IdTenant = 1,
            NombreFantasia = "Mi Tienda",
            Email = "tienda@ejemplo.cl",
            Publicado = true
        };
        var bundle = new StorefrontProfileBundle("Tenant Legal", tenant, [], [], "CLP");
        return Task.FromResult(Result<StorefrontProfileBundle?>.Success(bundle));
    }

    public Task<Result<TenantStorefront?>> GetProfileAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult(Result<TenantStorefront?>.Success(null, allowNull: true));

    public Task<Result<IReadOnlyList<TenantSocialLink>>> GetLinksAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<TenantSocialLink>>.Success([]));

    public Task<Result<IReadOnlyList<TenantBusinessHour>>> GetHoursAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<TenantBusinessHour>>.Success([]));

    public Task<Result<TenantStorefront>> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default)
        => Task.FromResult(Result<TenantStorefront>.Success(profile));

    public Task<Result<TenantSocialLink>> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default)
        => Task.FromResult(Result<TenantSocialLink>.Success(link));

    public Task<Result<bool>> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default)
        => Task.FromResult(Result<bool>.Success(true));

    public Task<Result<IReadOnlyList<TenantBusinessHour>>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default)
        => Task.FromResult(Result<IReadOnlyList<TenantBusinessHour>>.Success(hours));
}
