using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace Inventa.Store.Services;

// S68 pieza 5: AuthenticationStateProvider para el Store.
// No protege rutas (nada AuthorizeRouteView), solo expone
// estado de sesion para header/botones. Refresh solo bajo
// demanda (GetAuthenticationStateAsync o notificación manual).
public sealed class StoreAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly IStoreAuthService _auth;
    private readonly ILogger<StoreAuthenticationStateProvider> _logger;
    private AuthenticationState _cached = new(new ClaimsPrincipal(new ClaimsIdentity()));

    public StoreAuthenticationStateProvider(IStoreAuthService auth, ILogger<StoreAuthenticationStateProvider> logger)
    {
        _auth = auth;
        _logger = logger;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        var principal = await _auth.GetUserAsync();
        var state = principal != null
            ? new AuthenticationState(principal)
            : new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
        if (!Equivalent(state, _cached))
        {
            _cached = state;
            NotifyAuthenticationStateChanged(Task.FromResult(state));
        }
        return _cached;
    }

    public void NotifyUserChanged() => _ = GetAuthenticationStateAsync();

    private static bool Equivalent(AuthenticationState a, AuthenticationState b)
    {
        var ua = a.User.Identity?.IsAuthenticated ?? false;
        var ub = b.User.Identity?.IsAuthenticated ?? false;
        if (ua != ub) return false;
        if (!ua) return true;
        var na = a.User.GetName();
        var nb = b.User.GetName();
        return string.Equals(na, nb, StringComparison.Ordinal);
    }
}