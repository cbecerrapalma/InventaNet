namespace Inventa.Web.Shared;

/// <summary>
/// S92: permite que una página de detalle nombre el último tramo de la miga (p. ej. el nombre del
/// producto en vez de su id). MainLayout lo crea y lo cascadea; AppBreadcrumbs lo escucha. Sin estado global.
/// </summary>
public sealed class MigaContexto
{
    public string? Ruta { get; private set; }
    public string? Etiqueta { get; private set; }
    public event Action? Cambio;

    /// <summary>Define la etiqueta del último tramo para la ruta indicada (absoluta, sin query).</summary>
    public void Definir(string ruta, string? etiqueta)
    {
        Ruta = "/" + ruta.Trim('/');
        Etiqueta = etiqueta;
        Cambio?.Invoke();
    }
}
