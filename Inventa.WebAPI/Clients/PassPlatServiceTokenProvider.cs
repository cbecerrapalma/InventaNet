// S79: proveedor de token del usuario técnico de PassPlat.
// Cachea en memoria por tenant PassPlat (clave: IdTenantPassPlat), single-flight con SemaphoreSlim,
// renovación 60s antes de expiración. Resuelve IdApp por código (cache 10 min). Nunca loguea secretos/tokens.
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Microsoft.Extensions.Options;

namespace Inventa.WebAPI.Clients;

public interface IPassPlatServiceTokenProvider
{
    Task<string?> GetAccessTokenAsync(int idTenantPassPlat, CancellationToken ct = default);
    void Invalidate(int idTenantPassPlat);
}

public sealed class PassPlatServiceTokenProvider : IPassPlatServiceTokenProvider
{
    private readonly IPassPlatClient _passPlat;
    private readonly PassPlatServiceOptions _opts;
    private readonly ILogger<PassPlatServiceTokenProvider> _logger;

    private readonly ConcurrentDictionary<int, (string Token, DateTime ExpiresAt)> _tokenCache = new();
    private readonly ConcurrentDictionary<int, SemaphoreSlim> _locks = new();
    private (int IdApp, DateTime ExpiresAt)? _appIdCache;
    private readonly SemaphoreSlim _appLock = new(1, 1);

    public PassPlatServiceTokenProvider(IPassPlatClient passPlat, IOptions<PassPlatServiceOptions> opts, ILogger<PassPlatServiceTokenProvider> logger)
    {
        _passPlat = passPlat ?? throw new ArgumentNullException(nameof(passPlat));
        _opts = opts?.Value ?? throw new ArgumentNullException(nameof(opts));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public void Invalidate(int idTenantPassPlat)
    {
        _tokenCache.TryRemove(idTenantPassPlat, out _);
    }

    public async Task<string?> GetAccessTokenAsync(int idTenantPassPlat, CancellationToken ct = default)
    {
        if (idTenantPassPlat <= 0) return null;

        if (_tokenCache.TryGetValue(idTenantPassPlat, out var cached) && cached.ExpiresAt > DateTime.UtcNow.AddSeconds(60))
            return cached.Token;

        var sem = _locks.GetOrAdd(idTenantPassPlat, _ => new SemaphoreSlim(1, 1));
        await sem.WaitAsync(ct);
        try
        {
            if (_tokenCache.TryGetValue(idTenantPassPlat, out cached) && cached.ExpiresAt > DateTime.UtcNow.AddSeconds(60))
                return cached.Token;

            var idApp = await GetOrResolveAppIdAsync(ct);
            if (idApp <= 0) return null;

            var loginReq = new LoginRequest(_opts.NomUsuario, null, idApp, _opts.Password, idTenantPassPlat, null, null, null);
            var result = await _passPlat.LoginAsync(loginReq, ct);
            if (!result.Success || result.Data == null || string.IsNullOrEmpty(result.Data.AccessToken))
            {
                _logger.LogWarning("PassPlatService login falló para tenant {Tenant}: {Error}", idTenantPassPlat, result.Error ?? "sin error");
                return null;
            }

            var token = result.Data.AccessToken;
            var exp = result.Data.ExpiresAt;
            _tokenCache[idTenantPassPlat] = (token, exp);
            return token;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error obteniendo token PassPlatService para tenant {Tenant}", idTenantPassPlat);
            return null;
        }
        finally
        {
            sem.Release();
        }
    }

    private async Task<int> GetOrResolveAppIdAsync(CancellationToken ct)
    {
        if (_appIdCache.HasValue && _appIdCache.Value.ExpiresAt > DateTime.UtcNow)
            return _appIdCache.Value.IdApp;

        await _appLock.WaitAsync(ct);
        try
        {
            if (_appIdCache.HasValue && _appIdCache.Value.ExpiresAt > DateTime.UtcNow)
                return _appIdCache.Value.IdApp;

            var appsResult = await _passPlat.GetActivasAsync(ct);
            if (!appsResult.Success || appsResult.Data == null)
            {
                _logger.LogWarning("No se pudo obtener apps activas de PassPlat");
                return 0;
            }

            var app = appsResult.Data.FirstOrDefault(a => string.Equals(a.Codigo, _opts.AppCodigo, StringComparison.OrdinalIgnoreCase) && a.Activa);
            if (app == null)
            {
                _logger.LogWarning("App {Codigo} no encontrada o inactiva en PassPlat", _opts.AppCodigo);
                return 0;
            }

            _appIdCache = (app.Id, DateTime.UtcNow.AddMinutes(10));
            return app.Id;
        }
        finally
        {
            _appLock.Release();
        }
    }
}