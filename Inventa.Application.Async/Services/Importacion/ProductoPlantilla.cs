using CBP.Excel.Abstractions;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// S87.2 C1: plantilla VACÍA de productos (0 filas de datos). Columnas exactas
/// de ADR-PROD-002 §2, una fila por variante. Listas del tenant para desplegables.
/// </summary>
public static class ProductoPlantilla
{
    public static ExcelTemplateOptions Build(
        IReadOnlyList<string> unidades,
        IReadOnlyList<string> bodegas,
        IReadOnlyList<string> categoriasSugerencia,
        IReadOnlyList<string> marcasSugerencia)
    {
        List<string> Sn() => ["S", "N"];

        return new ExcelTemplateOptions
        {
            SheetName = "Productos",
            Instructions =
                "Importación de productos: una fila por variante.\n" +
                "Las columnas de producto se repiten en cada variante; la API agrupa por CodigoProducto.\n" +
                "Los campos marcados con * son obligatorios.\n" +
                "Categoria admite ruta con > de hasta 2 niveles (ej. Vestuario > Poleras); si no existe se creará.\n" +
                "Marca inexistente se creará automáticamente (advertencia, no error).\n" +
                "CodigoBarras en formato texto para conservar ceros; vacío genera código interno.\n" +
                "Precios en pesos chilenos sin decimales (ej. 9990).\n" +
                "Fechas con formato dd-mm-aaaa (ej. 12-10-2026).\n" +
                "S/N en mayúscula (Pesable, ManejaLote, PublicarEnTienda, Activo).\n" +
                "StockInicial solo aplica a variantes nuevas y exige Bodega.\n" +
                "Las presentaciones vendibles las genera el sistema; no se digitan.\n" +
                "Al volver a cargar productos existentes: una celda con valor reemplaza el valor actual; una celda opcional vacía conserva el valor actual.\n" +
                "Quitar una fila del archivo no elimina el producto; para desactivarlo usa Activo = N.\n" +
                "El stock de variantes existentes se corrige con Ajustes de inventario.\n" +
                "Antes de aplicar verás la lista de cambios (nuevos, modificados y sin cambios).\n" +
                "Use los desplegables donde existan; respete largos máximos.",
            Columns =
            [
                Col("CodigoProducto", "CodigoProducto", "Código del producto; agrupa variantes; único por tenant.", true, ExcelDataType.Text, 50, "FIC-11150"),
                Col("NombreProducto", "NombreProducto", "Nombre genérico del producto.", true, ExcelDataType.Text, 150, "Polera Niño Esmeralda"),
                Col("Categoria", "Categoria", "Ruta con > (máx. 2 niveles). Se crea si no existe.", true, ExcelDataType.Text, null, "Vestuario > Poleras", listName: "Categorias"),
                Col("Marca", "Marca", "Se crea si no existe. Vacío conserva la marca actual.", false, ExcelDataType.Text, 100, "Ficcus", listName: "Marcas"),
                Col("UnidadBase", "UnidadBase", "Unidad base del producto.", true, ExcelDataType.Text, null, "UN", allowed: unidades),
                Col("Pesable", "Pesable (S/N)", "S = cantidades con decimales. Vacío: N si es nuevo; conserva si existe.", false, ExcelDataType.Text, null, "N", allowed: Sn()),
                Col("ManejaLote", "ManejaLote (S/N)", "S = exige lote en movimientos. Vacío: N si es nuevo; conserva si existe.", false, ExcelDataType.Text, null, "N", allowed: Sn()),
                Col("DiasVencimiento", "DiasVencimiento", "Entero ≥ 0. Vacío: sin control si es nuevo; conserva si existe.", false, ExcelDataType.Integer, null, null),
                Col("PublicarEnTienda", "PublicarEnTienda (S/N)", "S = visible en tienda. Vacío: N si es nuevo; conserva si existe.", false, ExcelDataType.Text, null, "S", allowed: Sn()),
                Col("CodigoVariante", "CodigoVariante", "Único dentro del producto.", true, ExcelDataType.Text, 50, "FIC-11150-T08"),
                Col("NombreVariante", "NombreVariante", "Ej. Talla 8.", true, ExcelDataType.Text, 100, "Talla 8"),
                Col("CodigoBarras", "CodigoBarras", "Texto (conserva ceros). Vacío: se genera interno si la variante es nueva; conserva el actual si existe.", false, ExcelDataType.Text, null, "26207240802"),
                Col("CodigosAdicionales", "CodigosAdicionales", "Lista separada por ;", false, ExcelDataType.Text, null, null),
                Col("PrecioVenta", "PrecioVenta", "Entero CLP > 0, sin decimales.", true, ExcelDataType.Integer, null, "9990"),
                Col("PrecioOferta", "PrecioOferta", "Entero < PrecioVenta.", false, ExcelDataType.Integer, null, "3996"),
                Col("OfertaDesde", "OfertaDesde (dd-mm-aaaa)", "Inicio de oferta.", false, ExcelDataType.Date, null, "12-10-2026"),
                Col("OfertaHasta", "OfertaHasta (dd-mm-aaaa)", "Fin de oferta (≥ desde).", false, ExcelDataType.Date, null, null),
                Col("CostoCompra", "CostoCompra", "Costo promedio de la variante (> 0). Vacío conserva el actual.", false, ExcelDataType.Integer, null, null),
                Col("StockInicial", "StockInicial", "Entero ≥ 0. Solo variantes nuevas; exige Bodega.", false, ExcelDataType.Integer, null, "10"),
                Col("Bodega", "Bodega", "Código de bodega (debe existir).", false, ExcelDataType.Text, null, "BOD-CENTRAL", listName: "Bodegas"),
                Col("Activo", "Activo (S/N)", "N = desactiva el producto. Vacío: S si es nuevo; conserva si existe.", false, ExcelDataType.Text, null, "S", allowed: Sn()),
            ],
            HiddenLists =
            [
                Lista("Unidades", unidades),
                Lista("SN", Sn()),
                Lista("Bodegas", bodegas),
                Lista("Categorias", categoriasSugerencia),
                Lista("Marcas", marcasSugerencia),
            ],
            SampleRows = FilasEjemplo(),
        };
    }

    private static ExcelTemplateColumn Col(
        string property, string header, string desc, bool required,
        ExcelDataType tipo, int? maxLen, string? ejemplo,
        IReadOnlyList<string>? allowed = null, string? listName = null)
        => new()
        {
            PropertyName = property,
            HeaderName = header,
            Description = desc,
            IsRequired = required,
            DataType = tipo,
            MaxLength = maxLen,
            ExampleValue = ejemplo,
            AllowedValues = allowed,
            ListName = listName,
        };

    private static ExcelTemplateList Lista(string nombre, IReadOnlyList<string> valores)
        => new() { Name = nombre, Values = valores };

    // 2 productos × 3 variantes genéricos, sin dependencia de datos del tenant.
    private static IReadOnlyList<ExcelTemplateRow> FilasEjemplo()
    {
        ExcelTemplateRow Fila(
            string cp, string np, string cat, string marca,
            string cv, string nv, string precio, string? oferta = null) => new()
            {
                Values = new Dictionary<string, object?>
                {
                    ["CodigoProducto"] = cp,
                    ["NombreProducto"] = np,
                    ["Categoria"] = cat,
                    ["Marca"] = marca,
                    ["UnidadBase"] = "UN",
                    ["Pesable"] = "N",
                    ["ManejaLote"] = "N",
                    ["DiasVencimiento"] = null,
                    ["PublicarEnTienda"] = "S",
                    ["CodigoVariante"] = cv,
                    ["NombreVariante"] = nv,
                    ["CodigoBarras"] = null,
                    ["CodigosAdicionales"] = null,
                    ["PrecioVenta"] = precio,
                    ["PrecioOferta"] = oferta,
                    ["OfertaDesde"] = oferta is null ? null : "12-10-2026",
                    ["OfertaHasta"] = null,
                    ["CostoCompra"] = null,
                    ["StockInicial"] = "10",
                    ["Bodega"] = "BOD-CENTRAL",
                    ["Activo"] = "S",
                }
            };

        return
        [
            Fila("EJE-100", "Polera Ejemplo", "Vestuario > Poleras", "Generica", "EJE-100-T08", "Talla 8", "9990", "7992"),
            Fila("EJE-100", "Polera Ejemplo", "Vestuario > Poleras", "Generica", "EJE-100-T10", "Talla 10", "9990"),
            Fila("EJE-100", "Polera Ejemplo", "Vestuario > Poleras", "Generica", "EJE-100-T12", "Talla 12", "9990"),
            Fila("EJE-200", "Pantalón Ejemplo", "Vestuario > Pantalones", "Generica", "EJE-200-32", "Talla 32", "19990"),
            Fila("EJE-200", "Pantalón Ejemplo", "Vestuario > Pantalones", "Generica", "EJE-200-34", "Talla 34", "19990", "15992"),
            Fila("EJE-200", "Pantalón Ejemplo", "Vestuario > Pantalones", "Generica", "EJE-200-36", "Talla 36", "19990"),
        ];
    }
}
