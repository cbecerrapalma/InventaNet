using System.ComponentModel;

namespace Inventa.UC.Producto;
public enum TipoDatos
{
    [Description("Datos generales")]
    General,

    [Description("Variantes")]
    Variantes,

    [Description("Presentaciones")]
    Presentaciones,

    [Description("Conversiones")]
    Conversiones,

    [Description("Códigos de barra")]
    Codigos,

    [Description("Precios de venta")]
    PreciosVenta,

    [Description("Stock")]
    Stock,

    [Description("Todos los datos")]
    Todo
}