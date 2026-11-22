using CBP.Storage.Imaging;

namespace Inventa.Application.Async.Services;

/// <summary>
/// S106: tamaños y nombres de las variantes de imagen de producto. El original queda como «detalle»;
/// la grilla de la tienda usa <see cref="Listado"/> y la ficha del producto usa <see cref="Ficha"/>.
/// </summary>
public static class ProductoImagenVariantes
{
    public const string ListadoNombre = "listado";
    public const string FichaNombre = "ficha";

    /// <summary>Variante de la grilla del catálogo (lado mayor 400 px).</summary>
    public static readonly ImageVariantSpec Listado = new(ListadoNombre, 400);

    /// <summary>Variante de la ficha del producto (lado mayor 1200 px).</summary>
    public static readonly ImageVariantSpec Ficha = new(FichaNombre, 1200);

    /// <summary>Las variantes que genera el servidor al confirmar una subida.</summary>
    public static readonly IReadOnlyList<ImageVariantSpec> Todas = [Listado, Ficha];
}
