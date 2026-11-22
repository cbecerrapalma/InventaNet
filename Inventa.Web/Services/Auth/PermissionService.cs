using Inventa.Web.Services.Storage;

namespace Inventa.Web.Services.Auth;

public sealed class PermissionService
{
    private readonly JwtClaimsReader _reader;
    private readonly ITokenStorage _storage;

    public PermissionService(JwtClaimsReader reader, ITokenStorage storage)
    {
        _reader = reader;
        _storage = storage;
    }

    private async Task<IReadOnlySet<string>> GetCurrentPermissionsAsync(CancellationToken ct = default)
    {
        var token = await _storage.GetAccessTokenAsync(ct);
        _reader.SetToken(token);
        return _reader.GetPermissions();
    }

    public async Task<bool> HasPermissionAsync(string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(code)) return false;
        var perms = await GetCurrentPermissionsAsync(ct);
        return perms.Contains(code);
    }

    public async Task<bool> HasAnyAsync(CancellationToken ct = default, params string[] codes)
    {
        if (codes.Length == 0) return false;
        var perms = await GetCurrentPermissionsAsync(ct);
        return codes.Any(c => perms.Contains(c));
    }

    public async Task<bool> HasAllAsync(CancellationToken ct = default, params string[] codes)
    {
        if (codes.Length == 0) return false;
        var perms = await GetCurrentPermissionsAsync(ct);
        return codes.All(c => perms.Contains(c));
    }

    public Task<IReadOnlySet<string>> GetPermissionsAsync(CancellationToken ct = default) => GetCurrentPermissionsAsync(ct);

    /// <summary>S95 K5.1: el usuario de la sesión es maestro de PassPlat (claim is_system). Ver <see cref="JwtClaimsReader.EsUsuarioMaestro"/>.</summary>
    public async Task<bool> EsUsuarioMaestroAsync(CancellationToken ct = default)
    {
        var token = await _storage.GetAccessTokenAsync(ct);
        _reader.SetToken(token);
        return _reader.EsUsuarioMaestro();
    }
}
