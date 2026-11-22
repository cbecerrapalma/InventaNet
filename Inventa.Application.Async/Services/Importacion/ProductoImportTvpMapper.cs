using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// S88.2: convierte <see cref="ProductoImportRow"/> (DTO de Excel, texto crudo)
/// en <see cref="ProductoImportTvpItem"/> (TVP ya resuelto para el SP).
/// Reglas C#: partición de ruta de categoría (K4) y TipoCod por formato (K3).
/// </summary>
public static class ProductoImportTvpMapper
{
    public static ProductoImportTvpItem Map(ProductoImportRow f)
    {
        var (padre, hija) = PartirCategoria(f.Categoria);
        return new ProductoImportTvpItem
        {
            CodigoProducto = f.CodigoProducto,
            NombreProducto = f.NombreProducto,
            CategoriaPadre = string.IsNullOrWhiteSpace(padre) ? null : padre.Trim(),
            Categoria = hija,
            Marca = string.IsNullOrWhiteSpace(f.Marca) ? null : f.Marca,
            UnidadBase = f.UnidadBase,
            // S91.4 (TVP v3): vacío => null = conservar (existente) / valor por defecto del SP (nuevo).
            Pesable = SiNo(f.Pesable),
            ManejaLote = SiNo(f.ManejaLote),
            DiasVencimiento = int.TryParse(f.DiasVencimiento, out var d) ? d : null,
            PublicarEnTienda = SiNo(f.PublicarEnTienda),
            CodigoVariante = f.CodigoVariante,
            NombreVariante = f.NombreVariante,
            CodigoBarras = string.IsNullOrWhiteSpace(f.CodigoBarras) ? null : f.CodigoBarras,
            TipoCod = ResolverTipoCod(f),
            CodigosAdicionales = string.IsNullOrWhiteSpace(f.CodigosAdicionales) ? null : f.CodigosAdicionales,
            PrecioVenta = decimal.TryParse(f.PrecioVenta, out var pv) ? pv : 0m,
            PrecioOferta = decimal.TryParse(f.PrecioOferta, out var po) ? po : null,
            OfertaDesde = DateTime.TryParseExact(f.OfertaDesde, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var od) ? od : null,
            OfertaHasta = DateTime.TryParseExact(f.OfertaHasta, "dd-MM-yyyy", null, System.Globalization.DateTimeStyles.None, out var oh) ? oh : null,
            PrecioMayorista = null, // S93.1 D5: mayorista congelado (TVP v3 sin cambios)
            CostoCompra = decimal.TryParse(f.CostoCompra, out var cc) ? cc : null,
            Proveedor = null, // S93.1 D6: sin PrecioCompra
            StockInicial = int.TryParse(f.StockInicial, out var si) ? si : 0,
            Bodega = string.IsNullOrWhiteSpace(f.Bodega) ? null : f.Bodega,
            Activo = SiNo(f.Activo)
        };
    }

    /// <summary>S91.4: "S"/"N" normalizado; vacío o no reconocido => null (el validador ya rechaza valores inválidos).</summary>
    public static string? SiNo(string? valor)
        => string.IsNullOrWhiteSpace(valor) ? null
            : CBP.Excel.Parsing.ExcelValueParser.ParseYesNo(valor.Trim()) switch { true => "S", false => "N", null => null };

    /// <summary>
    /// K4: "Padre > Hija" → (padre, hija); un nivel → (null, nivel).
    /// </summary>
    public static (string? Padre, string Hija) PartirCategoria(string? ruta)
    {
        var niveles = (ruta ?? "").Split('>', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return niveles.Length switch
        {
            0 => (null, ""),
            1 => (null, niveles[0]),
            _ => (niveles[0], niveles[1])
        };
    }

    /// <summary>
    /// K3: TipoCod decidido por C#.
    /// Vacío o generado por C# → null (el SP genera interno y marca 3).
    /// EAN-13/8/UPC-A válido → 1 (Normal). Cualquier otro (11-dígitos
    /// Ficcus sin DV) → 4 (Proveedor).
    /// </summary>
    public static byte? ResolverTipoCod(ProductoImportRow f)
    {
        if (string.IsNullOrWhiteSpace(f.CodigoBarras))
            return null;
        var codigo = f.CodigoBarras.Trim();
        if (EanCodigoValidator.EsEstandar(codigo, out _))
            return 1;
        return 4;
    }
}
