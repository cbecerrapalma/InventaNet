using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using CBP.Excel.Parsing;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>S91.4: catálogo base del tenant que ya carga el validador (una consulta por catálogo).</summary>
public sealed record CatalogoImportacion(
    IReadOnlyList<Prod> Productos,
    IReadOnlyList<VarProd> Variantes,
    IReadOnlyList<CodBar> CodigosBarra,
    IReadOnlyList<Categoria> Categorias,
    IReadOnlyList<Marca> Marcas,
    IReadOnlyList<Unidad> Unidades)
{
    public static readonly CatalogoImportacion Vacio = new([], [], [], [], [], []);

    private HashSet<(string, string)>? _clavesVariante;

    /// <summary>¿La variante (CodigoProducto, CodigoVariante) ya existe en el tenant? O(1).</summary>
    public bool ExisteVariante(string codigoProducto, string codigoVariante)
    {
        _clavesVariante ??= Variantes.Where(v => v.Prod is not null)
            .Select(v => (ExcelValueParser.NormalizeCode(v.Prod!.Codigo), ExcelValueParser.NormalizeCode(v.Codigo)))
            .ToHashSet();
        return _clavesVariante.Contains((ExcelValueParser.NormalizeCode(codigoProducto), ExcelValueParser.NormalizeCode(codigoVariante)));
    }
}

public static class AccionCambio
{
    public const string Nuevo = "Nuevo";
    public const string Modificado = "Modificado";
    public const string SinCambios = "SinCambios";
}

public sealed record CambioCampo(string Campo, string Actual, string Nuevo);

public sealed record CambioFila(int Fila, string CodigoProducto, string CodigoVariante, string Accion, IReadOnlyList<CambioCampo> Campos);

public sealed record ResumenCambios(int Nuevos, int Modificados, int SinCambios, IReadOnlyDictionary<string, int> PorCampo)
{
    public bool HayCambios => Nuevos + Modificados > 0;
}

public sealed record CambiosImportacion(IReadOnlyList<CambioFila> Filas, ResumenCambios Resumen, string Sha256);

/// <summary>
/// S91.4: compara las filas que recibirá dbo.sp_ImportarProductos v3 (ya mapeadas a TVP) contra el catálogo
/// actual, con las MISMAS reglas que el SP, para mostrar antes de aplicar qué se crea y qué se sobrescribe.
/// Puro (sin acceso a datos); O(n) con diccionarios.
/// Reglas espejo del SP v3: categorías/marcas por Codigo = UPPER(nombre); celda vacía conserva (Marca,
/// DiasVencimiento, Pesable, ManejaLote, PublicarEnTienda, Activo); precios crean vigencia
/// solo si difieren de la última fila (FecVig DESC, Id DESC); oferta nueva si no existe una activa igual
/// (presentación, precio, desde); código principal nuevo o promovido; adicionales solo si no existen.
/// Los campos de producto se informan en la primera fila del producto dentro del archivo.
/// </summary>
public static class ProductoImportacionComparador
{
    public const string CampoNombreProducto = "Nombre del producto";
    public const string CampoCategoria = "Categoría";
    public const string CampoMarca = "Marca";
    public const string CampoPesable = "Pesable";
    public const string CampoManejaLote = "Maneja lote";
    public const string CampoDiasVencimiento = "Días de vencimiento";
    public const string CampoPublicarEnTienda = "Publicar en tienda";
    public const string CampoActivo = "Activo";
    public const string CampoNombreVariante = "Nombre de la variante";
    public const string CampoCosto = "Costo";
    public const string CampoPresentacion = "Presentación";
    public const string CampoPrecioVenta = "Precio de venta";
    public const string CampoOferta = "Oferta";
    public const string CampoCodigoBarras = "Código de barras";
    public const string CampoTipoCodigo = "Tipo de código";
    public const string CampoCodigoAdicional = "Código adicional";

    private static readonly CultureInfo EsCl = new("es-CL");

    public static CambiosImportacion Comparar(
        IReadOnlyList<(int Fila, ProductoImportTvpItem Item)> filas,
        CatalogoImportacion catalogo,
        EstadoComercialImportacion comercial,
        DateOnly hoy)
    {
        var idUnidadPorCodigo = catalogo.Unidades
            .GroupBy(u => u.Codigo.Trim())
            .ToDictionary(g => g.Key, g => g.First().IdUnidad, StringComparer.OrdinalIgnoreCase);
        var prods = catalogo.Productos
            .GroupBy(p => ExcelValueParser.NormalizeCode(p.Codigo))
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var vars = catalogo.Variantes.Where(v => v.Prod is not null)
            .GroupBy(v => (ExcelValueParser.NormalizeCode(v.Prod!.Codigo), ExcelValueParser.NormalizeCode(v.Codigo)))
            .ToDictionary(g => g.Key, g => g.First());
        var marcasPorId = catalogo.Marcas.ToDictionary(m => m.IdMarca);
        var marcasPorCodigo = catalogo.Marcas
            .GroupBy(m => m.Codigo.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);
        var catsPorId = catalogo.Categorias.ToDictionary(c => c.IdCategoria);
        var catsPorCodigoPadre = catalogo.Categorias
            .GroupBy(c => (c.Codigo.Trim().ToUpperInvariant(), c.IdPadre))
            .ToDictionary(g => g.Key, g => g.First());
        var principalPorPres = catalogo.CodigosBarra.Where(c => c.Principal)
            .GroupBy(c => c.IdPresProd)
            .ToDictionary(g => g.Key, g => g.First().Codigo);
        var codsPorCodigo = catalogo.CodigosBarra
            .GroupBy(c => c.Codigo.Trim().ToUpperInvariant())
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.Ordinal);

        var productosVistos = new HashSet<string>(StringComparer.Ordinal);
        var salida = new List<CambioFila>(filas.Count);

        foreach (var (fila, it) in filas.OrderBy(f => f.Fila))
        {
            var codProd = ExcelValueParser.NormalizeCode(it.CodigoProducto);
            var codVar = ExcelValueParser.NormalizeCode(it.CodigoVariante);
            var campos = new List<CambioCampo>();
            var primeraDelProducto = productosVistos.Add(codProd);

            if (!prods.TryGetValue(codProd, out var prod) || !vars.TryGetValue((codProd, codVar), out var variante))
            {
                salida.Add(new CambioFila(fila, it.CodigoProducto, it.CodigoVariante, AccionCambio.Nuevo, []));
                continue;
            }

            // ── Producto (solo en su primera fila del archivo) ──
            if (primeraDelProducto)
            {
                if (!string.Equals(it.NombreProducto, prod.Nombre, StringComparison.Ordinal))
                    campos.Add(new(CampoNombreProducto, prod.Nombre, it.NombreProducto));

                var idCatNueva = ResolverCategoria(it.CategoriaPadre, it.Categoria, catsPorCodigoPadre);
                if (idCatNueva != prod.IdCategoria)
                    campos.Add(new(CampoCategoria, RutaCategoria(prod.IdCategoria, catsPorId), RutaArchivo(it.CategoriaPadre, it.Categoria)
                        + (idCatNueva is null ? " (se creará)" : "")));

                if (!string.IsNullOrWhiteSpace(it.Marca))
                {
                    marcasPorCodigo.TryGetValue(it.Marca.Trim().ToUpperInvariant(), out var marcaNueva);
                    if (marcaNueva?.IdMarca != prod.IdMarca)
                        campos.Add(new(CampoMarca,
                            prod.IdMarca is { } im && marcasPorId.TryGetValue(im, out var ma) ? ma.Nombre : "",
                            it.Marca.Trim() + (marcaNueva is null ? " (se creará)" : "")));
                }

                Bandera(campos, CampoPesable, it.Pesable, prod.EsPesable);
                Bandera(campos, CampoManejaLote, it.ManejaLote, prod.ManejaLot);
                if (it.DiasVencimiento is { } dv && dv != prod.DiasVen)
                    campos.Add(new(CampoDiasVencimiento, prod.DiasVen?.ToString(CultureInfo.InvariantCulture) ?? "", dv.ToString(CultureInfo.InvariantCulture)));
                Bandera(campos, CampoPublicarEnTienda, it.PublicarEnTienda, prod.VisibleStore);
                Bandera(campos, CampoActivo, it.Activo, prod.Activo);
            }

            // ── Variante ──
            if (!string.Equals(it.NombreVariante, variante.Nombre, StringComparison.Ordinal))
                campos.Add(new(CampoNombreVariante, variante.Nombre, it.NombreVariante));
            if (it.CostoCompra is { } costo && costo != variante.CostoProm)
                campos.Add(new(CampoCosto, Monto(variante.CostoProm), Monto(costo)));

            // ── Presentación (1 por variante, unidad base) ──
            if (!idUnidadPorCodigo.TryGetValue(it.UnidadBase.Trim(), out var idUnidad)
                || !comercial.Presentaciones.TryGetValue((variante.IdVarProd, idUnidad), out var idPres))
            {
                campos.Add(new(CampoPresentacion, "", "Se creará"));
                salida.Add(new CambioFila(fila, it.CodigoProducto, it.CodigoVariante, AccionCambio.Modificado, campos));
                continue;
            }

            if (it.PrecioVenta > 0 && (!comercial.UltimoPrecio.TryGetValue(idPres, out var pv) || pv != it.PrecioVenta))
                campos.Add(new(CampoPrecioVenta, comercial.UltimoPrecio.TryGetValue(idPres, out var pva) ? Monto(pva) : "", Monto(it.PrecioVenta)));

            if (it.PrecioOferta is { } po && po > 0)
            {
                var desde = it.OfertaDesde is { } od ? DateOnly.FromDateTime(od) : hoy;
                if (!comercial.OfertasActivas.Contains((idPres, po, desde)))
                    campos.Add(new(CampoOferta, "", $"{Monto(po)} desde {desde:dd-MM-yyyy}"));
            }

            if (!string.IsNullOrWhiteSpace(it.CodigoBarras))
            {
                var cod = it.CodigoBarras.Trim();
                var principalActual = principalPorPres.TryGetValue(idPres, out var pa) ? pa : "";
                if (!codsPorCodigo.TryGetValue(cod.ToUpperInvariant(), out var existente))
                    campos.Add(new(CampoCodigoBarras, principalActual, cod + (principalActual.Length > 0 ? " (el anterior queda como adicional)" : "")));
                else if (existente.IdPresProd == idPres)
                {
                    if (!existente.Principal)
                        campos.Add(new(CampoCodigoBarras, principalActual, cod + " (pasa a principal)"));
                    else if (it.TipoCod is { } tc && tc != existente.TipoCod)
                        campos.Add(new(CampoTipoCodigo, TipoCodTexto(existente.TipoCod), TipoCodTexto(tc)));
                }
            }

            if (!string.IsNullOrWhiteSpace(it.CodigosAdicionales))
            {
                foreach (var ad in it.CodigosAdicionales.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                             .Distinct(StringComparer.OrdinalIgnoreCase))
                {
                    if (!codsPorCodigo.ContainsKey(ad.ToUpperInvariant()))
                        campos.Add(new(CampoCodigoAdicional, "", ad));
                }
            }

            salida.Add(new CambioFila(fila, it.CodigoProducto, it.CodigoVariante,
                campos.Count > 0 ? AccionCambio.Modificado : AccionCambio.SinCambios, campos));
        }

        var porCampo = salida.SelectMany(f => f.Campos)
            .GroupBy(c => c.Campo)
            .OrderByDescending(g => g.Count())
            .ToDictionary(g => g.Key, g => g.Count());
        var resumen = new ResumenCambios(
            salida.Count(f => f.Accion == AccionCambio.Nuevo),
            salida.Count(f => f.Accion == AccionCambio.Modificado),
            salida.Count(f => f.Accion == AccionCambio.SinCambios),
            porCampo);
        return new CambiosImportacion(salida, resumen, Huella(salida));
    }

    /// <summary>Huella determinista: mismas filas y cambios ⇒ mismo SHA-256.</summary>
    public static string Huella(IReadOnlyList<CambioFila> filas)
    {
        var sb = new StringBuilder();
        foreach (var f in filas.OrderBy(f => f.Fila))
        {
            sb.Append(f.Fila).Append('|').Append(f.CodigoProducto).Append('|').Append(f.CodigoVariante).Append('|').Append(f.Accion);
            foreach (var c in f.Campos)
                sb.Append('|').Append(c.Campo).Append('=').Append(c.Actual).Append('>').Append(c.Nuevo);
            sb.Append('\n');
        }
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(sb.ToString())));
    }

    private static void Bandera(List<CambioCampo> campos, string campo, string? valorArchivo, bool actual)
    {
        if (string.IsNullOrWhiteSpace(valorArchivo)) return; // vacío conserva
        var nuevo = valorArchivo.Trim().Equals("S", StringComparison.OrdinalIgnoreCase);
        if (nuevo != actual) campos.Add(new(campo, actual ? "Sí" : "No", nuevo ? "Sí" : "No"));
    }

    private static long? ResolverCategoria(string? padre, string hija,
        IReadOnlyDictionary<(string, long?), Categoria> catsPorCodigoPadre)
    {
        long? idPadre = null;
        if (!string.IsNullOrWhiteSpace(padre))
        {
            if (!catsPorCodigoPadre.TryGetValue((padre.Trim().ToUpperInvariant(), null), out var p)) return null;
            idPadre = p.IdCategoria;
        }
        return catsPorCodigoPadre.TryGetValue((hija.Trim().ToUpperInvariant(), idPadre), out var h) ? h.IdCategoria : null;
    }

    private static string RutaCategoria(long idCategoria, IReadOnlyDictionary<long, Categoria> porId)
    {
        if (!porId.TryGetValue(idCategoria, out var c)) return "";
        return c.IdPadre is { } ip && porId.TryGetValue(ip, out var p) ? $"{p.Nombre.Trim()} > {c.Nombre.Trim()}" : c.Nombre.Trim();
    }

    private static string RutaArchivo(string? padre, string hija)
        => string.IsNullOrWhiteSpace(padre) ? hija.Trim() : $"{padre.Trim()} > {hija.Trim()}";

    private static string Monto(decimal v) => "$" + v.ToString(v == decimal.Truncate(v) ? "N0" : "N2", EsCl);

    private static string TipoCodTexto(byte t) => t switch
    {
        1 => "Normal",
        3 => "Interno",
        4 => "Proveedor",
        _ => t.ToString(CultureInfo.InvariantCulture),
    };
}
