using Inventa.Application.Async.Services.Importacion;
using Xunit;

namespace Inventa.Application.Tests;

/// <summary>
/// S88.2 (plan §4.10): partición de ruta de categoría (K4) y TipoCod (K3)
/// del mapper Row → TVP. Pruebas unitarias puras, sin dobles.
/// </summary>
public sealed class ProductoImportTvpMapperTests
{
    private static ProductoImportRow FilaBase() => new()
    {
        CodigoProducto = "T-MAP-01",
        NombreProducto = "Producto mapper",
        Categoria = "Vestuario > Poleras",
        Marca = "Ficcus",
        UnidadBase = "UN",
        CodigoVariante = "T-MAP-01-UN",
        NombreVariante = "Talla UN",
        CodigoBarras = "26207240802",
        PrecioVenta = "9990"
    };

    [Fact]
    public void PartirCategoria_PadreHija_Separa()
    {
        var (padre, hija) = ProductoImportTvpMapper.PartirCategoria("Vestuario > Poleras");
        Assert.Equal("Vestuario", padre);
        Assert.Equal("Poleras", hija);
    }

    [Fact]
    public void PartirCategoria_UnNivel_PadreNull()
    {
        var (padre, hija) = ProductoImportTvpMapper.PartirCategoria("Poleras");
        Assert.Null(padre);
        Assert.Equal("Poleras", hija);
    }

    [Fact]
    public void PartirCategoria_Vacia_HijaVacia()
    {
        var (padre, hija) = ProductoImportTvpMapper.PartirCategoria("");
        Assert.Null(padre);
        Assert.Equal("", hija);
    }

    [Fact]
    public void ResolverTipoCod_Ean13Valido_Uno()
    {
        var f = FilaBase();
        f.CodigoBarras = "5901234123457"; // EAN-13 con DV válido
        Assert.Equal((byte?)1, ProductoImportTvpMapper.ResolverTipoCod(f));
    }

    [Fact]
    public void ResolverTipoCod_OnceDigitosFiccus_Cuatro()
    {
        var f = FilaBase();
        f.CodigoBarras = "26207240802"; // 11 dígitos sin DV
        Assert.Equal((byte?)4, ProductoImportTvpMapper.ResolverTipoCod(f));
    }

    [Fact]
    public void ResolverTipoCod_Vacio_Null()
    {
        var f = FilaBase();
        f.CodigoBarras = "";
        Assert.Null(ProductoImportTvpMapper.ResolverTipoCod(f));
    }

    // S92 K7: C# ya no genera códigos internos; el formato puro sí se valida.
    [Theory]
    [InlineData(5, 1L, "2005000000010")]
    [InlineData(4, 12L, "2004000000129")]
    public void S92_FormatearInterno_EsEan13Valido(int tenant, long secuencia, string esperado)
    {
        var cod = CodigoBarraReglas.FormatearInterno(tenant, secuencia);
        Assert.Equal(esperado, cod);
        Assert.True(EanCodigoValidator.EsEstandar(cod, out _));
    }

    [Theory]
    [InlineData(0, 1L)]
    [InlineData(1000, 1L)]
    [InlineData(4, 0L)]
    [InlineData(4, 100_000_000L)]
    public void S92_FormatearInterno_FueraDeRango_Lanza(int tenant, long secuencia)
        => Assert.Throws<ArgumentOutOfRangeException>(() => CodigoBarraReglas.FormatearInterno(tenant, secuencia));

    [Fact]
    public void Map_FilaCompleta_ResuelveTodo()
    {
        var item = ProductoImportTvpMapper.Map(FilaBase());
        Assert.Equal("Vestuario", item.CategoriaPadre);
        Assert.Equal("Poleras", item.Categoria);
        Assert.Equal((byte?)4, item.TipoCod);
        Assert.Equal(9990m, item.PrecioVenta);
        Assert.Null(item.PrecioOferta);
        Assert.Null(item.CostoCompra);
        Assert.Equal(0, item.StockInicial);
        // S91.4 (TVP v3): vacío => null; el SP aplica N/N/N/S en productos nuevos y conserva en existentes.
        Assert.Null(item.Pesable);
        Assert.Null(item.ManejaLote);
        Assert.Null(item.PublicarEnTienda);
        Assert.Null(item.Activo);
    }

    [Theory]
    [InlineData("S", "S")]
    [InlineData("n", "N")]
    [InlineData("", null)]
    [InlineData("  ", null)]
    public void S914_Map_Banderas_NormalizaSNyVacioEsNull(string valor, string? esperado)
    {
        var f = FilaBase();
        f.Pesable = valor; f.ManejaLote = valor; f.PublicarEnTienda = valor; f.Activo = valor;
        var item = ProductoImportTvpMapper.Map(f);
        Assert.Equal(esperado, item.Pesable);
        Assert.Equal(esperado, item.ManejaLote);
        Assert.Equal(esperado, item.PublicarEnTienda);
        Assert.Equal(esperado, item.Activo);
    }
}

