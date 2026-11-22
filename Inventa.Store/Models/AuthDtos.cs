namespace Inventa.Store.Models;

// S68: modelo auth del Store (contrato S67). Los tipos con secretos
// redefinen ToString para no imprimirlos jamas (logs, asserts, dumps).

public static class StoreAuthCodes
{
    public const string Validation = "STORE_AUTH_VALIDATION";
    public const string InvalidCredentials = "STORE_AUTH_INVALID_CREDENTIALS";
    public const string Forbidden = "STORE_AUTH_FORBIDDEN";
    public const string MfaRequired = "STORE_AUTH_MFA_REQUIRED";
    public const string PasswordChangeRequired = "STORE_AUTH_PASSWORD_CHANGE_REQUIRED";
    public const string RateLimited = "STORE_AUTH_RATE_LIMITED";
    public const string NotConfigured = "STORE_AUTH_NOT_CONFIGURED";
    public const string Upstream = "STORE_AUTH_UPSTREAM";
    public const string RefreshInvalid = "STORE_AUTH_REFRESH_INVALID";
}

public sealed record StoreLoginRequest(string Identificador, string Password)
{
    public override string ToString() => $"StoreLoginRequest {{ Identificador = {Identificador}, Password = [REDACTED] }}";
}

public sealed record StoreTokenPair(string AccessToken, string RefreshToken)
{
    public override string ToString() => "StoreTokenPair { AccessToken = [REDACTED], RefreshToken = [REDACTED] }";
}

// Lo persistido en sessionStorage (clave unica "inventa.store.session").
public sealed record StoreSession(string AccessToken, string? RefreshToken)
{
    public override string ToString() => "StoreSession { AccessToken = [REDACTED], RefreshToken = [REDACTED] }";
}

public sealed record StoreAuthProblem(int Status, string? Code, string? CorrelationId, string? Upstream);

public sealed record StoreAuthResult<T>(bool Success, T? Data, StoreAuthProblem? Problem)
{
    public static StoreAuthResult<T> Ok(T data) => new(true, data, null);
    public static StoreAuthResult<T> Fail(StoreAuthProblem problem) => new(false, default, problem);
}

// S68 §3.3: tabla unica de mensajes (es-CL, sin jerga, sin revelar si el
// usuario existe). code desconocido / cuerpo no JSON / red -> generico.
public static class StoreAuthMessages
{
    public const string LocalValidation = "Ingresa tu correo o usuario y tu contraseña.";
    public const string Generic = "No pudimos procesar el ingreso. Inténtalo de nuevo en unos minutos.";

    public static string GetMessage(string? code) => code switch
    {
        StoreAuthCodes.Validation => LocalValidation,
        StoreAuthCodes.InvalidCredentials => "Usuario o contraseña incorrectos.",
        StoreAuthCodes.Forbidden => "Tu cuenta no tiene acceso a esta tienda o está bloqueada. Contacta a la tienda.",
        StoreAuthCodes.MfaRequired => "Tu cuenta requiere verificación en dos pasos, que aún no está disponible aquí. Puedes ingresar por el panel de administración.",
        StoreAuthCodes.PasswordChangeRequired => "Debes cambiar tu contraseña antes de ingresar. Hazlo desde el panel de administración.",
        StoreAuthCodes.RateLimited => "Demasiados intentos. Espera unos segundos e inténtalo de nuevo.",
        _ => Generic,
    };
}

// S68: returnUrl solo rutas relativas internas (anti open-redirect).
public static class StoreReturnUrl
{
    public static string Sanitize(string? url)
    {
        if (string.IsNullOrEmpty(url)) return "/";
        if (url.Length > 2048) return "/";
        if (!url.StartsWith('/')) return "/";
        if (url.Length > 1 && (url[1] == '/' || url[1] == '\\')) return "/";
        if (url.Contains('\\')) return "/";
        foreach (var c in url)
            if (char.IsWhiteSpace(c) || char.IsControl(c)) return "/";
        var colon = url.IndexOf(':');
        if (colon > 0)
        {
            var slash = url.IndexOf('/', 1);
            if (slash < 0 || colon < slash) return "/";
        }
        return url;
    }
}
