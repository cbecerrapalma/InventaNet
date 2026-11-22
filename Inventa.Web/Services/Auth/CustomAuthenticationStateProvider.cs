using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;
using Inventa.Web.Services.Storage;

namespace Inventa.Web.Services.Auth;

public sealed class CustomAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ITokenStorage _storage;
    private readonly JwtClaimsReader _reader;
    private ClaimsPrincipal _current = new(new ClaimsIdentity());

    public CustomAuthenticationStateProvider(ITokenStorage storage, JwtClaimsReader reader)
    {
        _storage = storage;
        _reader = reader;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        => new(_current);

    public async Task InitializeAsync(CancellationToken ct = default)
    {
        var token = await _storage.GetAccessTokenAsync(ct);
        if (string.IsNullOrWhiteSpace(token))
        {
            _current = new ClaimsPrincipal(new ClaimsIdentity());
            return;
        }
        _reader.SetToken(token);
        if (_reader.IsExpired())
        {
            await _storage.ClearAsync(ct);
            _current = new ClaimsPrincipal(new ClaimsIdentity());
            return;
        }
        var claims = _reader.ToClaims();
        var identity = new ClaimsIdentity(claims, "jwt");
        _current = new ClaimsPrincipal(identity);
    }

    public Task SetAuthenticationAsync(string token, CancellationToken ct = default)
    {
        _reader.SetToken(token);
        var claims = _reader.ToClaims();
        var identity = new ClaimsIdentity(claims, "jwt");
        _current = new ClaimsPrincipal(identity);
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_current)));
        return Task.CompletedTask;
    }

    public Task ClearAuthenticationAsync()
    {
        _current = new ClaimsPrincipal(new ClaimsIdentity());
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_current)));
        return Task.CompletedTask;
    }

    public ClaimsPrincipal CurrentPrincipal => _current;
    public bool IsAuthenticated => _current.Identity?.IsAuthenticated ?? false;
}
