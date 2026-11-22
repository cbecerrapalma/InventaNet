using System.Security.Claims;
using System.Text;
using System.Text.Json;

namespace Inventa.Web.Services.Auth;

public sealed class JwtClaimsReader
{
    private string? _token;
    private JsonElement _payload;
    private bool _parsed;

    public void SetToken(string? token)
    {
        _token = token;
        _parsed = false;
        _payload = default;
    }

    private bool TryParse()
    {
        if (_parsed) return _payload.ValueKind == JsonValueKind.Object;
        _parsed = true;
        if (string.IsNullOrWhiteSpace(_token)) return false;
        try
        {
            var parts = _token.Split('.');
            if (parts.Length < 2) return false;
            var payload = parts[1].Replace('-', '+').Replace('_', '/');
            switch (payload.Length % 4)
            {
                case 2: payload += "=="; break;
                case 3: payload += "="; break;
            }
            var bytes = Convert.FromBase64String(payload);
            var json = Encoding.UTF8.GetString(bytes);
            using var doc = JsonDocument.Parse(json);
            _payload = doc.RootElement.Clone();
            return true;
        }
        catch { return false; }
    }

    private string? TryGetString(string key)
    {
        if (!TryParse()) return null;
        if (_payload.TryGetProperty(key, out var v))
        {
            if (v.ValueKind == JsonValueKind.String) return v.GetString();
            if (v.ValueKind == JsonValueKind.Number) return v.GetRawText();
        }
        return null;
    }

    public string? GetUserId()
        => TryGetString("sub") ?? TryGetString(ClaimTypes.NameIdentifier) ?? TryGetString("nameid");

    public string? GetUserName()
        => TryGetString("unique_name") ?? TryGetString("name") ?? TryGetString(ClaimTypes.Name) ?? TryGetString("email");

    public int? GetTenantId()
    {
        var raw = TryGetString("tid") ?? TryGetString("TenantId") ?? TryGetString("tenantid") ?? TryGetString("tenant_id");
        if (int.TryParse(raw, out var id)) return id;
        return null;
    }

    public int? GetIdApp()
    {
        var raw = TryGetString("IdApp") ?? TryGetString("idApp") ?? TryGetString("aud");
        if (int.TryParse(raw, out var id)) return id;
        return null;
    }

    /// <summary>
    /// S95 K5.1: usuario maestro de PassPlat (Usuarios.EsSistema) = claim <c>is_system</c> = "true" emitido por PassPlat.
    /// Única autoridad para la ventana de acceso; la tabla espejo Usuario.EsSistema de Inventa no decide (AGENTS §6c/§6d).
    /// </summary>
    public bool EsUsuarioMaestro() => string.Equals(TryGetString("is_system"), "true", StringComparison.OrdinalIgnoreCase);

    public IReadOnlySet<string> GetPermissions()
    {
        if (!TryParse()) return new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        void AddClaim(string key)
        {
            if (_payload.TryGetProperty(key, out var v))
            {
                if (v.ValueKind == JsonValueKind.Array)
                    foreach (var el in v.EnumerateArray())
                        if (el.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(el.GetString()))
                            set.Add(el.GetString()!);
                else if (v.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(v.GetString()))
                    set.Add(v.GetString()!);
            }
        }
        AddClaim("permissions");
        AddClaim("permiso");
        AddClaim("permission");
        return set;
    }

    public DateTime? GetExpiration()
    {
        var raw = TryGetString("exp");
        if (long.TryParse(raw, out var sec))
            return DateTimeOffset.FromUnixTimeSeconds(sec).UtcDateTime;
        return null;
    }

    public bool IsExpired(int clockSkewSeconds = 0)
    {
        var exp = GetExpiration();
        if (exp is null) return false;
        return DateTime.UtcNow.AddSeconds(clockSkewSeconds) >= exp.Value;
    }

    public IReadOnlyList<Claim> ToClaims()
    {
        var claims = new List<Claim>();
        var uid = GetUserId();
        if (!string.IsNullOrWhiteSpace(uid))
        {
            claims.Add(new Claim(ClaimTypes.NameIdentifier, uid));
            claims.Add(new Claim("sub", uid));
        }
        var name = GetUserName();
        if (!string.IsNullOrWhiteSpace(name))
            claims.Add(new Claim(ClaimTypes.Name, name));
        var tid = GetTenantId();
        if (tid.HasValue)
        {
            claims.Add(new Claim("tid", tid.Value.ToString()));
            claims.Add(new Claim("TenantId", tid.Value.ToString()));
            claims.Add(new Claim("tenantid", tid.Value.ToString()));
        }
        var appId = GetIdApp();
        if (appId.HasValue)
            claims.Add(new Claim("IdApp", appId.Value.ToString()));
        foreach (var p in GetPermissions())
        {
            claims.Add(new Claim("permissions", p));
            claims.Add(new Claim("permiso", p));
        }
        var exp = GetExpiration();
        if (exp.HasValue)
            claims.Add(new Claim("exp", new DateTimeOffset(exp.Value).ToUnixTimeSeconds().ToString()));
        return claims;
    }
}
