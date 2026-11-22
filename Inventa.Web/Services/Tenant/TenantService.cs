using Inventa.Web.Models.Tenant;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;

namespace Inventa.Web.Services.Tenant;

/// <summary>
/// S92.2-K7: caché de mis-tenants con clave de sesión (access token) y vuelo
/// único compartido (singleflight): tras el login hay 1 sola petición
/// GET api/auth/mis-tenants aunque varios componentes la pidan a la vez
/// (antes: 4). Sin cambio de contrato HTTP ni de firmas consumidas:
/// GetMyTenantsAsync conserva su firma.
/// </summary>
public sealed class TenantService
{
    private readonly ApiClient _api;
    private readonly ITokenStorage _storage;
    private readonly object _sync = new();
    private string? _cacheKey;
    private List<TenantItemDto>? _cached;
    private Task<List<TenantItemDto>>? _inflight;
    private string? _inflightKey;

    public TenantService(ApiClient api, ITokenStorage storage)
    {
        _api = api;
        _storage = storage;
    }

    public async Task<List<TenantItemDto>> GetMyTenantsAsync(CancellationToken ct = default)
    {
        // La clave es el token de la sesión: login/logout cambian de clave e
        // invalidan solos. El fetch compartido no usa el ct del llamador para
        // que una cancelación no aborte la petición de los demás; cada llamador
        // aplica su propio ct al esperar (WaitAsync).
        var key = await _storage.GetAccessTokenAsync(ct);
        Task<List<TenantItemDto>> task;
        lock (_sync)
        {
            if (_cached is not null && _cacheKey == key)
                return _cached;
            if (_inflight is not null && _inflightKey == key)
            {
                task = _inflight;
            }
            else
            {
                // El placeholder se registra ANTES de arrancar el fetch: si el
                // fetch falla de forma síncrona (stub/HTTP completado), el catch
                // lo limpia y no se "resucita" una tarea fallida como _inflight.
                var tcs = new TaskCompletionSource<List<TenantItemDto>>(
                    TaskCreationOptions.RunContinuationsAsynchronously);
                _inflight = tcs.Task;
                _inflightKey = key;
                task = CorrerFetchAsync(key, tcs);
            }
        }
        return await task.WaitAsync(ct);
    }

    public void InvalidateCache()
    {
        lock (_sync)
        {
            _cached = null;
            _cacheKey = null;
            _inflight = null;
            _inflightKey = null;
        }
    }

    /// <summary>
    /// S92.2-K7: el login ya trae la lista autoritativa de mis-tenants; se
    /// deja en caché bajo la sesión actual para que los componentes no pidan
    /// de nuevo (tras el login: 1 sola petición en total).
    /// </summary>
    public async Task PrimeCacheAsync(List<TenantItemDto> tenants, CancellationToken ct = default)
    {
        var key = await _storage.GetAccessTokenAsync(ct);
        lock (_sync)
        {
            _cached = tenants;
            _cacheKey = key;
            _inflight = null;
            _inflightKey = null;
        }
    }

    private async Task<List<TenantItemDto>> CorrerFetchAsync(
        string? key, TaskCompletionSource<List<TenantItemDto>> tcs)
    {
        try
        {
            var tenants = await _api.GetAsync<List<TenantItemDto>>("api/auth/mis-tenants");
            var list = tenants ?? [];
            lock (_sync)
            {
                // Si se invalidó durante el vuelo, no se pisa la (nueva) caché:
                // el resultado igual se entrega a quienes ya lo esperaban.
                if (ReferenceEquals(_inflight, tcs.Task))
                {
                    _cached = list;
                    _cacheKey = key;
                    _inflight = null;
                    _inflightKey = null;
                }
            }
            tcs.TrySetResult(list);
        }
        catch (Exception ex)
        {
            lock (_sync)
            {
                if (ReferenceEquals(_inflight, tcs.Task))
                {
                    _inflight = null;
                    _inflightKey = null;
                }
            }
            tcs.TrySetException(ex);
        }
        return await tcs.Task;
    }
}
