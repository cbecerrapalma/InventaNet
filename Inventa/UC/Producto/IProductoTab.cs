using Inventa.Domain.Entities;

namespace Inventa.UC.Producto;

/// <summary>
/// Contrato que deben implementar todos los tabs del mantenedor de productos.
/// Permite la comunicación bidireccional con el formulario padre.
/// </summary>
public interface IProductoTab
{
    /// <summary>Indica si el control tiene cambios sin guardar</summary>
    bool HasChanges { get; }

    /// <summary>Indica si el control está cargando datos</summary>
    bool IsLoading { get; set; }

    /// <summary>Carga los datos del producto en el control</summary>
    void LoadData(Prod producto, IReadOnlyList<PresProd>? presentacionesCache);

    /// <summary>Limpia el control para nuevo producto</summary>
    void Clear();

    /// <summary>Valida los datos del control</summary>
    bool ValidateData();

    /// <summary>Obtiene los datos modificados del control</summary>
    void ApplyChanges(Prod producto);

    /// <summary>Evento que se dispara cuando hay cambios en el control</summary>
    event EventHandler? DataChanged;

    /// <summary>Evento que se dispara cuando se solicita refrescar datos</summary>
    event EventHandler? RefreshRequested;
}