using Inventa.Web.Models.Tenant;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;

namespace Inventa.Web.Services.Tenant;

public sealed class TenantContextService : ITenantContextProvider
{
    private readonly ITokenStorage _storage;
    private int? _cachedLocalId;
    private int? _cachedPassPlatId;
    private bool _loaded;
    public event Action<int?, int?>? OnTenantChanged;

    public TenantContextService(ITokenStorage storage) => _storage = storage;

    public async Task<int?> GetCurrentTenantIdAsync(CancellationToken ct = default)
    {
        // S92.1: si la caché está vacía se relee el storage. AuthService guarda el tenant del login directo en
        // el storage; con un null cacheado de antes del login, TenantSelector "fijaba" el tenant de nuevo y
        // disparaba OnTenantChanged (el dashboard cancelaba su petición recién iniciada).
        if (!_loaded || _cachedLocalId is null)
        {
            _cachedLocalId = await _storage.GetTenantIdAsync(ct);
            _cachedPassPlatId = await _storage.GetTenantPassPlatIdAsync(ct);
            _loaded = true;
        }
        return _cachedLocalId;
    }

    public async Task<int?> GetCurrentTenantPassPlatIdAsync(CancellationToken ct = default)
    {
        if (!_loaded)
        {
            await GetCurrentTenantIdAsync(ct);
        }
        return _cachedPassPlatId;
    }

    public async Task SetCurrentTenantAsync(int localId, int passPlatId, CancellationToken ct = default)
    {
        // S92.1: notificar solo si el tenant realmente cambia. Antes se notificaba siempre (p. ej. al
        // re-confirmar el mismo tenant tras el login) y las páginas cancelaban y repetían sus peticiones
        // (dashboard abortado → TaskCanceledException en la API, mis-tenants duplicado).
        var anterior = await GetCurrentTenantIdAsync(ct);
        _cachedLocalId = localId;
        _cachedPassPlatId = passPlatId;
        _loaded = true;
        await _storage.SetTenantIdsAsync(localId, passPlatId, ct);
        if (anterior != localId)
            OnTenantChanged?.Invoke(localId, passPlatId);
    }

    // Backward compatibility
    public async Task SetCurrentTenantAsync(int tenantId, CancellationToken ct = default)
    {
        await SetCurrentTenantAsync(tenantId, 0, ct);
    }

    public int? CurrentTenantId => _cachedLocalId;
    public int? CurrentTenantPassPlatId => _cachedPassPlatId;

    public async Task ClearAsync(CancellationToken ct = default)
    {
        _cachedLocalId = null;
        _cachedPassPlatId = null;
        _loaded = true;
        OnTenantChanged?.Invoke(null, null);
        await _storage.ClearTenantIdsAsync(ct);
    }

    public async Task ValidateAndFixAsync(IReadOnlyList<TenantItemDto> allowed, CancellationToken ct = default)
    {
        var current = await GetCurrentTenantIdAsync(ct);
        if (current is null) return;
        var exists = allowed.Any(t => t.IdTenant == current.Value);
        if (!exists)
        {
            // invalid tenant -> clear or pick first valid
            if (allowed.Count > 0)
                await SetCurrentTenantAsync(allowed[0].IdTenant, allowed[0].IdTenantPassPlat, ct);
            else
            {
                _cachedLocalId = null;
                _cachedPassPlatId = null;
                OnTenantChanged?.Invoke(null, null);
                await _storage.ClearTenantIdsAsync(ct);
            }
        }
    }
}
