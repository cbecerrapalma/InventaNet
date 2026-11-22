using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>S92.2-K5: regresión S92.1 de TenantContextService (sin evento si el
/// tenant no cambia; con evento si cambia; relee el storage con caché vacía).</summary>
public sealed class TenantContextServiceTests
{
    private sealed class StubStorage : ITokenStorage
    {
        public string? Token { get; set; } = "tok";
        public int? TenantId { get; set; }
        public int? TenantPassPlatId { get; set; }
        public int SetTenantIdsCalls { get; private set; }

        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult(Token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult(TenantId);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult(TenantPassPlatId);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default)
        {
            Token = accessToken; TenantId = tenantId; TenantPassPlatId = tenantPassPlatId;
            return Task.CompletedTask;
        }
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default)
        {
            TenantId = tenantId; TenantPassPlatId = tenantPassPlatId; SetTenantIdsCalls++;
            return Task.CompletedTask;
        }
        public Task ClearTenantIdsAsync(CancellationToken ct = default)
        {
            TenantId = null; TenantPassPlatId = null;
            return Task.CompletedTask;
        }
        public Task ClearAsync(CancellationToken ct = default)
        {
            Token = null; TenantId = null; TenantPassPlatId = null;
            return Task.CompletedTask;
        }
    }

    [Fact]
    public async Task SetMismoTenant_NoDisparaEvento()
    {
        var storage = new StubStorage { TenantId = 4, TenantPassPlatId = 40 };
        var svc = new TenantContextService(storage);
        var eventos = 0;
        svc.OnTenantChanged += (_, _) => eventos++;

        await svc.SetCurrentTenantAsync(4, 40, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(0, eventos);
        Assert.Equal(1, storage.SetTenantIdsCalls);
    }

    [Fact]
    public async Task SetTenantDistinto_DisparaEventoConIds()
    {
        var storage = new StubStorage { TenantId = 4, TenantPassPlatId = 40 };
        var svc = new TenantContextService(storage);
        (int?, int?)? recibido = null;
        svc.OnTenantChanged += (l, p) => recibido = (l, p);

        await svc.SetCurrentTenantAsync(6, 60, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal((6, 60), recibido);
        Assert.Equal(6, svc.CurrentTenantId);
        Assert.Equal(60, svc.CurrentTenantPassPlatId);
    }

    [Fact]
    public async Task CacheVacia_ReleeStorage_EnVezDeFijarNull()
    {
        // S92.1: AuthService guarda el tenant del login directo en el storage;
        // con un null cacheado de antes del login, el selector "fijaba" el
        // tenant de nuevo y disparaba OnTenantChanged espurio.
        var storage = new StubStorage { TenantId = null, TenantPassPlatId = null };
        var svc = new TenantContextService(storage);
        var eventos = 0;
        svc.OnTenantChanged += (_, _) => eventos++;
        await svc.ClearAsync(Xunit.TestContext.Current.CancellationToken); // caché vacía pero marcada como cargada

        storage.TenantId = 4;
        storage.TenantPassPlatId = 40;
        var actual = await svc.GetCurrentTenantIdAsync(Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(4, actual);
        Assert.Equal(40, await svc.GetCurrentTenantPassPlatIdAsync(Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(1, eventos); // solo el ClearAsync, ningún refijado espurio
    }
}
