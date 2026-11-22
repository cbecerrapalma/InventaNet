using System.Net.Http.Json;
using Inventa.Store.Models;

namespace Inventa.Store.Services;

public sealed class ContactSendRequest
{
    public string Nombre { get; set; } = "";
    public string Email { get; set; } = "";
    public string? Telefono { get; set; }
    public string Mensaje { get; set; } = "";
    public bool Consentimiento { get; set; }
    public long? ProductoId { get; set; }
    public string Website { get; set; } = "";
}

public sealed record ContactSendResponse(
    string Code,
    string CorrelationId,
    string? TrackingId);

public sealed record ContactErrorResponse(
    string Code,
    string? Detail,
    int? RetryAfter);

public interface IContactApiService
{
    Task<ContactSendResult> SendAsync(ContactSendRequest req, CancellationToken ct = default);
}

public sealed record ContactSendResult(bool Success, ContactSendResponse? Data, ContactErrorResponse? Error);

    public sealed class ContactApiService : IContactApiService
    {
        // Extrae code/title/detail de un ProblemDetails sin exponer el JSON crudo.
        // S96.1 D3: detail solo se muestra para CONTACTO_VALIDACION (mensaje pensado para el usuario).
        internal static (string? Code, string? Title, string? Detail) TryReadProblemDetails(string content)
        {
            try
            {
                using var doc = System.Text.Json.JsonDocument.Parse(content);
                var root = doc.RootElement;
                if (root.ValueKind != System.Text.Json.JsonValueKind.Object)
                    return (null, null, null);
                string? code = null, title = null, detail = null;
                if (root.TryGetProperty("code", out var c) && c.ValueKind == System.Text.Json.JsonValueKind.String)
                    code = c.GetString();
                if (root.TryGetProperty("title", out var t) && t.ValueKind == System.Text.Json.JsonValueKind.String)
                    title = t.GetString();
                if (root.TryGetProperty("detail", out var d) && d.ValueKind == System.Text.Json.JsonValueKind.String)
                    detail = d.GetString();
                if (string.IsNullOrWhiteSpace(title)) title = null;
                if (string.IsNullOrWhiteSpace(code)) code = null;
                if (string.IsNullOrWhiteSpace(detail)) detail = null;
                return (code, title, detail);
            }
            catch (System.Text.Json.JsonException)
            {
                return (null, null, null);
            }
        }

        // S96.1 D3: texto visible por código. Solo CONTACTO_VALIDACION usa el detail del servidor.
        internal static string MensajeAmable(string code, string? title, string? detail) => code switch
        {
            "CONTACTO_VALIDACION" => detail ?? title ?? "Revisa los datos del formulario.",
            "CONTACTO_RATE_LIMITED" => title ?? "Demasiadas solicitudes.",
            "CONTACTO_NO_CONFIGURADO" => "El formulario de contacto no está disponible por ahora. Intenta más tarde.",
            _ => "No se pudo enviar el mensaje. Intenta más tarde."
        };

        private readonly HttpClient _http;
        private readonly ILogger<ContactApiService> _logger;

    public ContactApiService(HttpClient http, ILogger<ContactApiService> logger)
    {
        _http = http ?? throw new ArgumentNullException(nameof(http));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ContactSendResult> SendAsync(ContactSendRequest req, CancellationToken ct = default)
    {
        try
        {
            var resp = await _http.PostAsJsonAsync("api/public/contacto", req, ct);
            if (resp.IsSuccessStatusCode)
            {
                var data = await resp.Content.ReadFromJsonAsync<ContactSendResponse>(cancellationToken: ct);
                return new ContactSendResult(true, data, null);
            }

            var errorContent = await resp.Content.ReadAsStringAsync(ct);
            int? retryAfter = null;
            if (resp.Headers.TryGetValues("Retry-After", out var values))
            {
                var val = values.FirstOrDefault();
                if (int.TryParse(val, out var ra)) retryAfter = ra;
            }

            // S79.2: nunca mostrar el cuerpo crudo (ProblemDetails JSON) ni mensajes
            // técnicos al usuario. Se extraen code/title/detail; el texto visible sale de
            // MensajeAmable según el código (S96.1 D3).
            var (bodyCode, bodyTitle, bodyDetail) = TryReadProblemDetails(errorContent);
            var code = bodyCode ?? resp.StatusCode switch
            {
                System.Net.HttpStatusCode.BadRequest => "CONTACTO_VALIDACION",
                System.Net.HttpStatusCode.TooManyRequests => "CONTACTO_RATE_LIMITED",
                System.Net.HttpStatusCode.ServiceUnavailable => "CONTACTO_NO_CONFIGURADO",
                _ => "CONTACTO_UPSTREAM"
            };
            var error = new ContactErrorResponse(
                Code: code,
                Detail: MensajeAmable(code, bodyTitle, bodyDetail),
                RetryAfter: retryAfter);

            return new ContactSendResult(false, null, error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error enviando contacto");
            return new ContactSendResult(false, null, new ContactErrorResponse("CONTACTO_UPSTREAM", "No se pudo enviar el mensaje. Intenta más tarde.", null));
        }
    }
}