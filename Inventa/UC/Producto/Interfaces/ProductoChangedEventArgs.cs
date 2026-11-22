using Inventa.Domain.Entities;

namespace Inventa.UC.Producto.Interfaces;

public class ProductoChangedEventArgs : EventArgs
{
    public Prod? ProductoAnterior { get; }
    public Prod? ProductoNuevo { get; }
    public bool EsNuevo { get; }

    public ProductoChangedEventArgs(Prod? productoAnterior, Prod? productoNuevo, bool esNuevo)
    {
        ProductoAnterior = productoAnterior;
        ProductoNuevo = productoNuevo;
        EsNuevo = esNuevo;
    }
}