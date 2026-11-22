using CBP.Excel.Abstractions;
using CBP.Excel.Parsing;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using System.Globalization;
using System.IO;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>S87.2 C2: validación de importación sin escribir catálogo (S87 §5.3).</summary>
public sealed record FilaImportacion(int RowNumber, ProductoImportRow Row);

public sealed record IncidenciaImportacion(int Fila, string Columna, string Codigo, string Mensaje, bool EsError);

public sealed record ResultadoValidacionImportacion(
    IReadOnlyList<FilaImportacion> Filas,
    IReadOnlyList<IncidenciaImportacion> Errores,
    IReadOnlyList<IncidenciaImportacion> Advertencias,
    int Productos,
    int Variantes,
    int Nuevos,
    int Existentes)
{
    /// <summary>S91.4: catálogo cargado durante la validación (para la vista previa de cambios y el aplicador).</summary>
    public CatalogoImportacion Catalogo { get; init; } = CatalogoImportacion.Vacio;
}

public sealed class ProductoImportacionValidadorAsync
{
    private static readonly CultureInfo EsCl = new("es-CL");

    private static readonly string[] ColumnasProducto =
    [
        nameof(ProductoImportRow.NombreProducto), nameof(ProductoImportRow.Categoria),
        nameof(ProductoImportRow.Marca), nameof(ProductoImportRow.UnidadBase),
        nameof(ProductoImportRow.Pesable),         nameof(ProductoImportRow.ManejaLote),
        nameof(ProductoImportRow.DiasVencimiento), nameof(ProductoImportRow.PublicarEnTienda),
        nameof(ProductoImportRow.Activo),
        nameof(ProductoImportRow.Bodega),
    ];

    // S93.1: columnas retiradas de la plantilla (D5 mayorista congelado, D6 PrecioCompra retirada).
    private static readonly Dictionary<string, string> ColumnasRetiradas = new(StringComparer.OrdinalIgnoreCase)
    {
        ["PrecioMayorista"] = "el precio mayorista no se usa por ahora.",
        ["Proveedor"] = "los precios de compra se retiraron; el costo se carga en CostoCompra (costo promedio de la variante).",
    };

    private readonly ICategoriaRepositoryAsync _categorias;
    private readonly IMarcaRepositoryAsync _marcas;
    private readonly IUnidadRepositoryAsync _unidades;
    private readonly IBodegaRepositoryAsync _bodegas;
    private readonly IProdRepositoryAsync _prods;
    private readonly IVarProdRepositoryAsync _vars;
    private readonly ICodBarRepositoryAsync _cods;
    private readonly IExcelReader _lector;

    public ProductoImportacionValidadorAsync(
        ICategoriaRepositoryAsync categorias,
        IMarcaRepositoryAsync marcas,
        IUnidadRepositoryAsync unidades,
        IBodegaRepositoryAsync bodegas,
        IProdRepositoryAsync prods,
        IVarProdRepositoryAsync vars,
        ICodBarRepositoryAsync cods,
        IExcelReader lector)
    {
        _categorias = categorias;
        _marcas = marcas;
        _unidades = unidades;
        _bodegas = bodegas;
        _prods = prods;
        _vars = vars;
        _cods = cods;
        _lector = lector;
    }

    public async Task<ResultadoValidacionImportacion> ValidarAsync(
        IReadOnlyList<ExcelRow> filasExcel, int idTenant, CancellationToken ct = default)
    {
        var errores = new List<IncidenciaImportacion>();
        var avisos = new List<IncidenciaImportacion>();
        void Error(int fila, string col, string cod, string msg) => errores.Add(new(fila, col, cod, msg, true));
        void Aviso(int fila, string col, string cod, string msg) => avisos.Add(new(fila, col, cod, msg, false));

        // 1. Mapeo por nombre de encabezado normalizado + columnas desconocidas.
        var setters = MapeoColumnas();
        var conocidas = new HashSet<string>(setters.Keys, StringComparer.Ordinal);
        var vistasDesconocidas = new HashSet<string>(StringComparer.Ordinal);
        var filas = new List<FilaImportacion>();
        foreach (var ef in filasExcel)
        {
            var row = new ProductoImportRow();
            foreach (var (clave, valor) in ef.Values)
            {
                if (setters.TryGetValue(clave, out var set))
                    set(row, Texto(valor));
                else if (vistasDesconocidas.Add(clave))
                {
                    if (ColumnasRetiradas.TryGetValue(clave, out var motivo))
                        Aviso(1, clave, "COLUMNA_IGNORADA", $"La columna '{clave}' ya no se usa y se ignora: {motivo}");
                    else
                        Aviso(1, clave, "COLUMNA_DESCONOCIDA", $"La columna '{clave}' no es parte de la plantilla y se ignora.");
                }
            }
            filas.Add(new(ef.RowNumber, row));
        }

        // 2. Por celda.
        var snVistos = new Dictionary<FilaImportacion, Dictionary<string, bool?>>();
        foreach (var f in filas)
        {
            var r = f.Row;
            Requerido(f.RowNumber, "CodigoProducto", r.CodigoProducto, 50, errores);
            Requerido(f.RowNumber, "NombreProducto", r.NombreProducto, 150, errores);
            Requerido(f.RowNumber, "Categoria", r.Categoria, 0, errores);
            if (r.Marca.Length > 100) Error(f.RowNumber, "Marca", "LARGO_MAX", "Marca excede 100 caracteres.");
            // S88.2: el SP deriva Codigo = UPPER(Nombre), varchar(50).
            if (r.Marca.Trim().Length > 50) Error(f.RowNumber, "Marca", "MARCA_LARGO", "Marca excede 50 caracteres para el código.");
            Requerido(f.RowNumber, "UnidadBase", r.UnidadBase, 0, errores);
            Requerido(f.RowNumber, "CodigoVariante", r.CodigoVariante, 50, errores);
            Requerido(f.RowNumber, "NombreVariante", r.NombreVariante, 100, errores);

            var sn = new Dictionary<string, bool?>();
            foreach (var col in new[] { "Pesable", "ManejaLote", "PublicarEnTienda", "Activo" })
            {
                var texto = (string?)typeof(ProductoImportRow).GetProperty(col)!.GetValue(r) ?? "";
                if (string.IsNullOrWhiteSpace(texto))
                    sn[col] = null;
                else
                {
                    var v = ExcelValueParser.ParseYesNo(texto.Trim());
                    if (v is null) Error(f.RowNumber, col, "SN_INVALIDO", $"{col} debe ser S o N.");
                    sn[col] = v;
                }
            }
            snVistos[f] = sn;

            if (!string.IsNullOrWhiteSpace(r.DiasVencimiento))
            {
                if (EnteroEstricto(r.DiasVencimiento) is not { } dv || dv < 0)
                    Error(f.RowNumber, "DiasVencimiento", "DIASVEN_INVALIDO", "DiasVencimiento debe ser entero ≥ 0.");
            }
            if (EnteroEstrictoONulo(r.PrecioVenta) is null)
                Error(f.RowNumber, "PrecioVenta", "PRECIO_VENTA_INVALIDO", "PrecioVenta es obligatorio, entero CLP > 0 sin decimales.");
            else if (EnteroEstricto(r.PrecioVenta)!.Value <= 0)
                Error(f.RowNumber, "PrecioVenta", "PRECIO_VENTA_INVALIDO", "PrecioVenta debe ser > 0.");
            var poTxt = r.PrecioOferta.Trim();
            if (poTxt.Length > 0 && (EnteroEstricto(poTxt) is not long poNum || poNum <= 0))
                Error(f.RowNumber, "PrecioOferta", "OFERTA_PRECIO_INVALIDO", "PrecioOferta debe ser entero > 0 y menor que PrecioVenta.");
            if (!string.IsNullOrWhiteSpace(r.CostoCompra))
            {
                if (ExcelValueParser.ParseDecimal(r.CostoCompra.Trim(), EsCl) is not { } cc || cc <= 0)
                    Error(f.RowNumber, "CostoCompra", "COSTO_INVALIDO", "CostoCompra debe ser > 0.");
            }
            if (!string.IsNullOrWhiteSpace(r.StockInicial))
            {
                if (EnteroEstricto(r.StockInicial) is not { } st || st < 0)
                    Error(f.RowNumber, "StockInicial", "STOCK_INVALIDO", "StockInicial debe ser entero ≥ 0.");
                else if (st > 0 && string.IsNullOrWhiteSpace(r.Bodega))
                    Error(f.RowNumber, "Bodega", "STOCK_BODEGA_REQUERIDA", "StockInicial > 0 exige Bodega.");
            }

            var desde = FechaONula(r.OfertaDesde);
            var hasta = FechaONula(r.OfertaHasta);
            if (!string.IsNullOrWhiteSpace(r.OfertaDesde) && desde is null)
                Error(f.RowNumber, "OfertaDesde", "OFERTA_FECHA_INVALIDA", "OfertaDesde debe ser fecha dd-mm-aaaa.");
            if (!string.IsNullOrWhiteSpace(r.OfertaHasta) && hasta is null)
                Error(f.RowNumber, "OfertaHasta", "OFERTA_FECHA_INVALIDA", "OfertaHasta debe ser fecha dd-mm-aaaa.");
            if (desde is not null && hasta is not null && desde > hasta)
                Error(f.RowNumber, "OfertaHasta", "OFERTA_RANGO_INVALIDO", "OfertaHasta no puede ser anterior a OfertaDesde.");
            if (!string.IsNullOrWhiteSpace(r.PrecioOferta) && !string.IsNullOrWhiteSpace(r.PrecioVenta)
                && EnteroEstricto(r.PrecioOferta) is { } po2 && EnteroEstricto(r.PrecioVenta) is { } pv2 && po2 >= pv2)
                Error(f.RowNumber, "PrecioOferta", "OFERTA_PRECIO_INVALIDO", "PrecioOferta debe ser menor que PrecioVenta.");
        }

        // 3. Por grupo CodigoProducto.
        foreach (var g in filas.GroupBy(f => ExcelValueParser.NormalizeCode(f.Row.CodigoProducto)))
        {
            if (string.IsNullOrEmpty(g.Key)) continue;
            var primera = g.First();
            foreach (var f in g.Skip(1))
            {
                foreach (var col in ColumnasProducto)
                {
                    var v0 = ValorCol(primera.Row, col);
                    var v1 = ValorCol(f.Row, col);
                    if (!IgualProducto(col, v0, v1))
                        Error(f.RowNumber, col, "GRUPO_INCONSISTENTE",
                            $"{col} difiere del valor de la primera fila del producto (fila {primera.RowNumber}: '{v0}').");
                }
            }
            var dupsVar = g.GroupBy(f => ExcelValueParser.NormalizeCode(f.Row.CodigoVariante))
                .Where(x => !string.IsNullOrEmpty(x.Key) && x.Count() > 1);
            foreach (var d in dupsVar)
                foreach (var f in d)
                    Error(f.RowNumber, "CodigoVariante", "VARIANTE_DUPLICADA",
                        $"CodigoVariante '{d.Key}' duplicado dentro del producto '{g.Key}'.");
            foreach (var f in g)
            {
                if (!string.IsNullOrWhiteSpace(f.Row.Categoria) && NivelesCategoria(f.Row.Categoria) > 2)
                    Error(f.RowNumber, "Categoria", "CATEGORIA_NIVELES", "Categoria admite máximo 2 niveles (Padre > Hija).");
                // S88.2: cada nivel es futuro Codigo (varchar 50) + Nombre (≤100).
                if (!string.IsNullOrWhiteSpace(f.Row.Categoria))
                    foreach (var nivel in f.Row.Categoria.Split('>', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                    {
                        if (nivel.Length > 50)
                            Error(f.RowNumber, "Categoria", "CATEGORIA_LARGO", $"Nivel '{nivel}' excede 50 caracteres para el código.");
                    }
            }
        }

        // 4. Cargas por lote: UNA consulta por catálogo, cero por fila.
        var categorias = await _categorias.GetByTenantAsync(idTenant, false, ct);
        var marcas = await _marcas.GetByTenantAsync(idTenant, false, ct);
        var unidades = await _unidades.GetAllOrdenadoAsync(ct);
        var bodegas = await _bodegas.GetByTenantAsync(idTenant, false, ct);
        var prods = await _prods.GetByTenantAsync(idTenant, false, ct);
        var vars = await _vars.GetByTenantAsync(idTenant, false, ct);
        var cods = await _cods.GetByTenantAsync(idTenant, ct);

        var rutasCategoria = RutasCategoria(categorias);
        var marcasSet = marcas.Select(m => m.Nombre.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var unidadesSet = unidades.Select(u => u.Codigo.Trim()).ToHashSet(StringComparer.OrdinalIgnoreCase);
        var bodegasSet = bodegas.Select(b => ExcelValueParser.NormalizeCode(b.Codigo)).ToHashSet(StringComparer.Ordinal);
        var prodsPorCodigo = prods
            .GroupBy(p => ExcelValueParser.NormalizeCode(p.Codigo))
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.Ordinal);
        var varsPorClave = vars
            .GroupBy(v => (Prod: ExcelValueParser.NormalizeCode(v.Prod!.Codigo), Var: ExcelValueParser.NormalizeCode(v.Codigo)))
            .ToDictionary(x => x.Key, x => x.First());
        var codsPorCodigo = cods
            .Where(c => c.PresProd?.VarProd?.Prod is not null)
            .GroupBy(c => c.Codigo.Trim())
            .ToDictionary(x => x.Key, x => x.First(), StringComparer.OrdinalIgnoreCase);

        var marcasNuevasAvisadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var categoriasNuevasAvisadas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var nuevos = 0;
        var existentes = 0;

        foreach (var f in filas)
        {
            var r = f.Row;
            var codProd = ExcelValueParser.NormalizeCode(r.CodigoProducto);
            var codVar = ExcelValueParser.NormalizeCode(r.CodigoVariante);

            if (!string.IsNullOrEmpty(codProd) && !string.IsNullOrEmpty(codVar))
            {
                if (varsPorClave.ContainsKey((codProd, codVar)))
                {
                    existentes++;
                    if (!string.IsNullOrWhiteSpace(r.StockInicial) && EnteroEstricto(r.StockInicial) is { } st2 && st2 > 0)
                        Aviso(f.RowNumber, "StockInicial", "STOCK_SE_IGNORA", "La variante ya existe: StockInicial se ignora (solo aplica a variantes nuevas).");
                }
                else nuevos++;
            }

            if (!string.IsNullOrWhiteSpace(r.UnidadBase) && !unidadesSet.Contains(r.UnidadBase.Trim()))
                Error(f.RowNumber, "UnidadBase", "UNIDAD_INVALIDA", $"UnidadBase '{r.UnidadBase.Trim()}' no existe.");
            if (!string.IsNullOrWhiteSpace(r.Bodega) && !bodegasSet.Contains(ExcelValueParser.NormalizeCode(r.Bodega)))
                Error(f.RowNumber, "Bodega", "BODEGA_NO_EXISTE", $"Bodega '{r.Bodega.Trim()}' no existe en el tenant.");
            if (!string.IsNullOrWhiteSpace(r.Marca) && !marcasSet.Contains(r.Marca.Trim()))
            {
                if (marcasNuevasAvisadas.Add(r.Marca.Trim()))
                    Aviso(f.RowNumber, "Marca", "MARCA_SE_CREARA", $"Marca '{r.Marca.Trim()}' no existe y se creará al aplicar.");
            }
            if (!string.IsNullOrWhiteSpace(r.Categoria) && NivelesCategoria(r.Categoria) <= 2)
            {
                var ruta = NormalizarRuta(r.Categoria);
                if (!rutasCategoria.Contains(ruta) && categoriasNuevasAvisadas.Add(ruta))
                    Aviso(f.RowNumber, "Categoria", "CATEGORIA_SE_CREARA", $"Categoría '{ruta}' no existe y se creará al aplicar.");
            }
        }

        // 4b. Grupos CodigoProducto contra BBDD: variante de otro producto, unidad base cambiada.
        // (Consistencia intra-grupo y VARIANTE_DUPLICADA ya cubiertos en fase 3.)
        var varsPorCodigoVar = vars
            .GroupBy(v => ExcelValueParser.NormalizeCode(v.Codigo))
            .ToDictionary(x => x.Key, x => x.ToList(), StringComparer.Ordinal);
        var unidadesPorCodigo = unidades
            .GroupBy(u => u.Codigo.Trim())
            .ToDictionary(x => x.Key, x => x.First().IdUnidad, StringComparer.OrdinalIgnoreCase);
        foreach (var g in filas.GroupBy(f => ExcelValueParser.NormalizeCode(f.Row.CodigoProducto)))
        {
            var lista = g.ToList();
            var primera = lista[0].Row;
            foreach (var f in lista)
            {
                var codVar = ExcelValueParser.NormalizeCode(f.Row.CodigoVariante);
                if (string.IsNullOrEmpty(codVar)) continue;
                if (varsPorCodigoVar.TryGetValue(codVar, out var candidatas)
                    && candidatas.All(v => !ExcelValueParser.NormalizeCode(v.Prod!.Codigo).Equals(g.Key, StringComparison.Ordinal)))
                    Error(f.RowNumber, "CodigoVariante", "CODVAR_MISMATCH", $"Variante '{codVar}' pertenece a otro producto en el catálogo.");
            }
            if (prodsPorCodigo.TryGetValue(g.Key, out var prodExistente))
            {
                var ubArchivo = (primera.UnidadBase ?? "").Trim();
                if (unidadesPorCodigo.TryGetValue(ubArchivo, out var idUnidadArchivo)
                    && idUnidadArchivo != prodExistente.IdUniBase)
                    Error(lista[0].RowNumber, "UnidadBase", "UNIDADBASE_MISMATCH", $"El producto '{g.Key}' ya existe con otra unidad base.");
            }
            // Precios (PrecioVenta/PrecioOferta) ya validados en fase por-fila.
        }

        // 5. Códigos de barra: duplicados en archivo, existentes en BBDD, formato.
        var todosCodigos = new List<(string Codigo, int Fila, string Columna)>();
        foreach (var f in filas)
        {
            if (!string.IsNullOrWhiteSpace(f.Row.CodigoBarras))
                todosCodigos.Add((f.Row.CodigoBarras.Trim(), f.RowNumber, "CodigoBarras"));
            foreach (var ad in f.Row.CodigosAdicionales.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (!string.IsNullOrEmpty(ad))
                    todosCodigos.Add((ad, f.RowNumber, "CodigosAdicionales"));
            }
            // S91.4: en variantes existentes el código vacío conserva el principal actual (no se genera otro).
            if (string.IsNullOrWhiteSpace(f.Row.CodigoBarras)
                && !varsPorClave.ContainsKey((ExcelValueParser.NormalizeCode(f.Row.CodigoProducto), ExcelValueParser.NormalizeCode(f.Row.CodigoVariante))))
                Aviso(f.RowNumber, "CodigoBarras", "CODBAR_SE_GENERARA", "Sin CodigoBarras: se generará un código interno al aplicar.");
        }
        foreach (var dup in todosCodigos.GroupBy(x => x.Codigo, StringComparer.OrdinalIgnoreCase).Where(x => x.Count() > 1))
            foreach (var item in dup)
                Error(item.Fila, item.Columna, "CODBAR_DUPLICADO_ARCHIVO", $"Código de barras '{dup.Key}' duplicado en el archivo.");
        var dupsEnArchivo = new HashSet<int>(errores.Where(e => e.Codigo == "CODBAR_DUPLICADO_ARCHIVO").Select(e => e.Fila));
        foreach (var item in todosCodigos)
        {
            if (dupsEnArchivo.Contains(item.Fila)) continue;
            if (codsPorCodigo.TryGetValue(item.Codigo, out var existente))
            {
                var fila = filas.First(f => f.RowNumber == item.Fila);
                var codProd = ExcelValueParser.NormalizeCode(fila.Row.CodigoProducto);
                var codVar = ExcelValueParser.NormalizeCode(fila.Row.CodigoVariante);
                var mismaVariante = existente.PresProd!.VarProd!.Codigo.Trim().Equals(codVar, StringComparison.OrdinalIgnoreCase)
                    && existente.PresProd.VarProd.Prod!.Codigo.Trim().Equals(codProd, StringComparison.OrdinalIgnoreCase);
                if (!mismaVariante)
                    Error(item.Fila, item.Columna, "CODBAR_EXISTE_BBDD", "El código de barras ya existe en el catálogo para otra variante.");
            }
            else if (!EanCodigoValidator.EsEstandar(item.Codigo, out _))
                Aviso(item.Fila, item.Columna, "CODBAR_FORMATO_NO_ESTANDAR", $"Código '{item.Codigo}' sin dígito verificador EAN-13/EAN-8/UPC-A válido: se acepta como código de proveedor.");
        }

        var grupos = filas
            .Where(f => !string.IsNullOrEmpty(ExcelValueParser.NormalizeCode(f.Row.CodigoProducto)))
            .Select(f => ExcelValueParser.NormalizeCode(f.Row.CodigoProducto))
            .Distinct(StringComparer.Ordinal)
            .Count();

        return new ResultadoValidacionImportacion(
            filas, errores, avisos, grupos, filas.Count, nuevos, existentes)
        {
            Catalogo = new CatalogoImportacion(prods, vars, cods, categorias, marcas, unidades),
        };
    }

    // ── Mapeo encabezado normalizado → setter ──
    private static Dictionary<string, Action<ProductoImportRow, string>> MapeoColumnas()
    {
        var props = typeof(ProductoImportRow).GetProperties();
        var map = new Dictionary<string, Action<ProductoImportRow, string>>(StringComparer.Ordinal);
        foreach (var p in props)
        {
            var attr = p.GetCustomAttributes(typeof(CBP.Excel.Attributes.ExcelColumnAttribute), true)
                .FirstOrDefault() as CBP.Excel.Attributes.ExcelColumnAttribute;
            var clave = ExcelValueParser.NormalizeHeader(attr?.Name ?? p.Name);
            map[clave] = (r, v) => p.SetValue(r, v);
        }
        return map;
    }

    private static string Texto(object? valor) => valor switch
    {
        null => "",
        double d => ExcelValueParser.NumericText(d),
        decimal m => m.ToString("0.#############################", CultureInfo.InvariantCulture),
        DateTime dt => dt.ToString("dd-MM-yyyy"),
        DateOnly dd => dd.ToString("dd-MM-yyyy"),
        bool b => b ? "S" : "N",
        _ => valor.ToString()?.Trim() ?? ""
    };

    private static void Requerido(int fila, string col, string valor, int maxLen, List<IncidenciaImportacion> errores)
    {
        if (string.IsNullOrWhiteSpace(valor))
            errores.Add(new(fila, col, "REQUERIDO", $"{col} es obligatorio.", true));
        else if (maxLen > 0 && valor.Trim().Length > maxLen)
            errores.Add(new(fila, col, "LARGO_MAX", $"{col} excede {maxLen} caracteres.", true));
    }

    // Entero estricto: rechaza decimales (9.5, 9990.5) en vez de redondear.
    private static long? EnteroEstricto(object? valor)
    {
        switch (valor)
        {
            case null: return null;
            case double d: return d == Math.Truncate(d) ? (long)d : null;
            case decimal m: return m == Math.Truncate(m) ? (long)m : null;
            case long l: return l;
            case int i: return i;
        }
        var texto = valor.ToString()?.Trim();
        if (string.IsNullOrEmpty(texto)) return null;
        var dec = ExcelValueParser.ParseDecimal(texto, EsCl)
            ?? ExcelValueParser.ParseDecimal(texto, CultureInfo.InvariantCulture);
        if (dec is null || dec.Value != Math.Truncate(dec.Value)) return null;
        try { return (long)dec.Value; } catch (OverflowException) { return null; }
    }

    private static long? EnteroEstrictoONulo(string texto)
        => string.IsNullOrWhiteSpace(texto) ? null : EnteroEstricto(texto.Trim());

    private static DateOnly? FechaONula(string texto)
        => string.IsNullOrWhiteSpace(texto) ? null : ExcelValueParser.ParseFecha(texto.Trim(), EsCl);

    private static string ValorCol(ProductoImportRow r, string col)
        => (string?)typeof(ProductoImportRow).GetProperty(col)!.GetValue(r) ?? "";

    private static bool IgualProducto(string col, string a, string b)
    {
        a = (a ?? "").Trim();
        b = (b ?? "").Trim();
        if (col == nameof(ProductoImportRow.Categoria))
            return NormalizarRuta(a).Equals(NormalizarRuta(b), StringComparison.OrdinalIgnoreCase);
        if (col is nameof(ProductoImportRow.Marca) or nameof(ProductoImportRow.UnidadBase))
            return a.Equals(b, StringComparison.OrdinalIgnoreCase);
        return a.Equals(b, StringComparison.Ordinal);
    }

    private static int NivelesCategoria(string ruta)
        => ruta.Split('>', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries).Length;

    private static string NormalizarRuta(string ruta)
        => string.Join(" > ", ruta.Split('>', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries));

    private static HashSet<string> RutasCategoria(IReadOnlyList<Categoria> categorias)
    {
        // Construye "Padre > Hija" vía IdPadre con comparación insensible.
        var porId = categorias.ToDictionary(c => c.IdCategoria);
        var rutas = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var c in categorias)
        {
            var nombre = c.Nombre.Trim();
            if (c.IdPadre is { } pid && porId.TryGetValue(pid, out var padre))
                rutas.Add(NormalizarRuta(padre.Nombre.Trim() + " > " + nombre));
            else
                rutas.Add(NormalizarRuta(nombre));
        }
        return rutas;
    }

    /// <summary>
    /// S88: Lee filas del archivo Excel para re-validación en aplicador.
    /// </summary>
    public async Task<IReadOnlyList<ExcelRow>> LeerFilasAsync(Stream stream, CancellationToken ct = default)
    {
        var lista = new List<ExcelRow>();
        await foreach (var row in _lector.ReadRowsAsync(stream,
            new ExcelReadOptions { SheetName = "Productos", CultureName = "es-CL", MaxRows = 20000, MaxBytes = 10L * 1024 * 1024 }, ct))
        {
            lista.Add(row);
        }
        return lista;
    }
}
