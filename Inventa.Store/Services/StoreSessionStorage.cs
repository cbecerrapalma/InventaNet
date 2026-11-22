using System.Text.Json;
using Microsoft.JSInterop;

namespace Inventa.Store.Services;

// S68 pieza 2: unico acceso a sessionStorage. Clave unica
// "inventa.store.session". Solo sessionStorage (nunca localStorage,
// cookies ni IndexedDB). Nunca lanza: prerender/desconexion JS
// retornan default y registran debug sin secretos.
public sealed class StoreSessionStorage
{
    public const string Key = "inventa.store.session";

    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;
    private readonly ILogger<StoreSessionStorage> _logger;

    public StoreSessionStorage(IJSRuntime js, ILogger<StoreSessionStorage> logger)
    {
        _js = js;
        _logger = logger;
    }

    public async Task SaveAsync<T>(T value, CancellationToken ct = default)
    {
        try
        {
            var json = JsonSerializer.Serialize(value, JsonOptions);
            await _js.InvokeVoidAsync("sessionStorage.setItem", ct, [Key, json]);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or InvalidOperationException or TaskCanceledException)
        {
            _logger.LogDebug("StoreSession save omitido ({Tipo})", ex.GetType().Name);
        }
    }

    public async Task<T?> LoadAsync<T>(CancellationToken ct = default)
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("sessionStorage.getItem", ct, [Key]);
            if (string.IsNullOrEmpty(json)) return default;
            return JsonSerializer.Deserialize<T>(json, JsonOptions);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or InvalidOperationException or TaskCanceledException or JsonException)
        {
            _logger.LogDebug("StoreSession load omitido ({Tipo})", ex.GetType().Name);
            return default;
        }
    }

    public async Task ClearAsync(CancellationToken ct = default)
    {
        try
        {
            await _js.InvokeVoidAsync("sessionStorage.removeItem", ct, [Key]);
        }
        catch (Exception ex) when (ex is JSDisconnectedException or JSException or InvalidOperationException or TaskCanceledException)
        {
            _logger.LogDebug("StoreSession clear omitido ({Tipo})", ex.GetType().Name);
        }
    }
}
