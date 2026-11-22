using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Inventa.Store.Services;

// S68 pieza 3: parseo JWT solo para display (nombre, exp).
// Nunca valida firma, nunca confía en claims para authz.
// Devuelve null si token invalido/nulo.
public static class StoreJwt
{
    public static ClaimsPrincipal? Parse(string? token)
    {
        if (string.IsNullOrWhiteSpace(token)) return null;
        try
        {
            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token)) return null;
            var jwt = handler.ReadJwtToken(token);
            var identity = new ClaimsIdentity(jwt.Claims, "jwt-display");
            return new ClaimsPrincipal(identity);
        }
        catch
        {
            return null;
        }
    }

    public static string? GetName(this ClaimsPrincipal? principal)
    {
        if (principal == null) return null;
        // PassPlat emite NameIdentifier (sub) y Name (opcional, en S68 H1 no viene).
        // Prioridad: claim "name" > "NameIdentifier" > "sub" > "preferred_username" > "email".
        // S96.1 D5: se ignoran los valores solo numéricos (el Id de usuario no es un nombre);
        // sin candidato, la cabecera muestra "Mi cuenta".
        foreach (var type in new[] { "name", ClaimTypes.NameIdentifier, "sub", "preferred_username", ClaimTypes.Email })
        {
            var value = principal.FindFirst(type)?.Value;
            if (!string.IsNullOrWhiteSpace(value) && !value.All(char.IsDigit))
                return value;
        }
        return null;
    }

    public static DateTimeOffset? GetExpiration(this ClaimsPrincipal? principal)
    {
        if (principal == null) return null;
        var exp = principal.FindFirst("exp")?.Value;
        if (long.TryParse(exp, out var unix)) return DateTimeOffset.FromUnixTimeSeconds(unix);
        return null;
    }

    public static bool IsExpired(this ClaimsPrincipal? principal, TimeSpan? skew = null)
    {
        var exp = GetExpiration(principal);
        if (!exp.HasValue) return true;
        var now = DateTimeOffset.UtcNow + (skew ?? TimeSpan.Zero);
        return exp.Value <= now;
    }

    public static string? GetClaim(this ClaimsPrincipal? principal, string type)
        => principal?.FindFirst(type)?.Value;
}