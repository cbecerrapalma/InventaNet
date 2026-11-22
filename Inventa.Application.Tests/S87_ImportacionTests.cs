using CBP.Excel.Abstractions;
using CBP.Excel.Parsing;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S87.2 C5: EanCodigoValidator (puro) + ProductoImportacionValidadorAsync (fakes, sin BBDD).
public sealed class S87_ImportacionTests
{
    // ── EAN ──
    [Fact]
    public void Ean13_Valido_Acepta() =>
        Assert.True(EanCodigoValidator.EsEstandar("5901234123457", out var f) && f == EanCodigoValidator.FormatoCodigo.Ean13);

    [Fact]
    public void Ean13_DvMalo_Rechaza() =>
        Assert.False(EanCodigoValidator.EsEstandar("5901234123458", out _));

    [Fact]
    public void Ean8_Valido_Acepta() =>
        Assert.True(EanCodigoValidator.EsEstandar("96385074", out var f) && f == EanCodigoValidator.FormatoCodigo.Ean8);

    [Fact]
    public void UpcA_Valido_Acepta() =>
        Assert.True(EanCodigoValidator.EsEstandar("036000291452", out var f) && f == EanCodigoValidator.FormatoCodigo.UpcA);

    [Fact]
    public void CodigoProveedor_NoEsEstandar() =>
        Assert.False(EanCodigoValidator.EsEstandar("PROV-ABC-001", out var f) && f != EanCodigoValidator.FormatoCodigo.NoEstandar);

    [Fact]
    public void Vacio_NoEsEstandar() =>
        Assert.False(EanCodigoValidator.EsEstandar("  ", out _));

    // ── Validador ──
    private sealed class Ctx
    {
        public StubCategoriaImportRepo Categorias = new() { Datos = [new Categoria { IdCategoria = 1, IdTenant = 4, Codigo = "CAT-01", Nombre = "Abarrotes" }] };
        public StubMarcaImportRepo Marcas = new() { Datos = [new Marca { IdMarca = 1, IdTenant = 4, Codigo = "FIC", Nombre = "Ficcus" }] };
        public StubUnidadImportRepo Unidades = new() { Datos = [new Unidad { IdUnidad = 1, Codigo = "UN", Nombre = "Unidad" }, new Unidad { IdUnidad = 2, Codigo = "KG", Nombre = "Kilo" }] };
        public StubBodegaImportRepo Bodegas = new() { Datos = [new Bodega { IdBodega = 1, IdTenant = 4, Codigo = "BOD-01", Nombre = "Central" }] };
        public StubProveedorImportRepo Proveedores = new() { Datos = [new Proveedor { IdProveedor = 1, IdTenant = 4, Codigo = "PROV-01" }] };
        public StubProdImportRepo Prods = new();
        public StubVarProdImportRepo Vars = new();
        public StubCodBarImportRepo Cods = new();
        public StubExcelReader Lector = new();

        public ProductoImportacionValidadorAsync Validador() => new(
            Categorias, Marcas, Unidades, Bodegas, Prods, Vars, Cods, Lector);
    }

    private sealed class StubExcelReader : IExcelReader
    {
        public IReadOnlyList<ExcelRow> ReadRows(Stream stream, ExcelReadOptions options) => [];
        public IReadOnlyList<T> ReadRows<T>(Stream stream, ExcelReadOptions options) where T : class, new() => [];
        public IAsyncEnumerable<ExcelRow> ReadRowsAsync(Stream stream, ExcelReadOptions options, CancellationToken cancellationToken = default) => AsyncEnumerable.Empty<ExcelRow>();
        public IAsyncEnumerable<T> ReadRowsAsync<T>(Stream stream, ExcelReadOptions options, CancellationToken cancellationToken = default) where T : class, new() => AsyncEnumerable.Empty<T>();
    }

    private static ExcelRow Fila(int n, Dictionary<string, string> base_, params (string Col, string Val)[] cambios)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (k, v) in base_) dict[ExcelValueParser.NormalizeHeader(k)] = v;
        foreach (var (c, v) in cambios) dict[ExcelValueParser.NormalizeHeader(c)] = v;
        return new ExcelRow { RowNumber = n, Values = dict };
    }

    private static Dictionary<string, string> Base() => new()
    {
        ["CodigoProducto *"] = "PROD-001",
        ["NombreProducto *"] = "Arroz Grano Largo",
        ["Categoria *"] = "Abarrotes",
        ["Marca"] = "Ficcus",
        ["UnidadBase *"] = "UN",
        ["Pesable (S/N)"] = "N",
        ["ManejaLote (S/N)"] = "N",
        ["PublicarEnTienda (S/N)"] = "S",
        ["Activo (S/N)"] = "S",
        ["CodigoVariante *"] = "PROD-001-UN",
        ["NombreVariante *"] = "Unidad",
        ["CodigoBarras"] = "5901234123457",
        ["PrecioVenta *"] = "9990",
    };

    [Fact]
    public async Task FilaValida_EsValida()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base())], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(r.Errores);
        Assert.Empty(r.Advertencias);
        Assert.Equal(1, r.Productos);
        Assert.Equal(1, r.Nuevos);
    }

    [Fact]
    public async Task MarcaNueva_GeneraAvisoSinBloquear()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("Marca", "NuevaMarca"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(r.Errores);
        Assert.Contains(r.Advertencias, a => a.Codigo == "MARCA_SE_CREARA");
    }

    [Fact]
    public async Task CategoriaNueva_GeneraAvisoSinBloquear()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("Categoria *", "Limpieza"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(r.Errores);
        Assert.Contains(r.Advertencias, a => a.Codigo == "CATEGORIA_SE_CREARA");
    }

    [Fact]
    public async Task Categoria3Niveles_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("Categoria *", "A > B > C"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "CATEGORIA_NIVELES");
    }

    [Fact]
    public async Task UnidadInexistente_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("UnidadBase *", "XX"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "UNIDAD_INVALIDA");
    }

    [Fact]
    public async Task PrecioConDecimal_NoRedondea_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("PrecioVenta *", "9990.5"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "PRECIO_VENTA_INVALIDO");
    }

    [Fact]
    public async Task PrecioCero_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("PrecioVenta *", "0"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "PRECIO_VENTA_INVALIDO");
    }

    [Fact]
    public async Task SNInvalido_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("Pesable (S/N)", "TALVEZ"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "SN_INVALIDO");
    }

    [Fact]
    public async Task OfertaMayorIgualVenta_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("PrecioOferta", "9990"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "OFERTA_PRECIO_INVALIDO");
    }

    [Fact]
    public async Task StockSinBodega_EsError()
    {
        var ctx = new Ctx();
        var b = Base(); b["StockInicial"] = "5";
        var r = await ctx.Validador().ValidarAsync([Fila(2, b)], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "STOCK_BODEGA_REQUERIDA");
    }

    [Fact]
    public async Task GrupoInconsistente_EsError()
    {
        var ctx = new Ctx();
        var b2 = Base(); b2["CodigoVariante *"] = "PROD-001-KG"; b2["NombreVariante *"] = "Kilo"; b2["NombreProducto *"] = "Otro Nombre";
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base()), Fila(3, b2)], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "GRUPO_INCONSISTENTE");
    }

    [Fact]
    public async Task VarianteDuplicada_EsError()
    {
        var ctx = new Ctx();
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base()), Fila(3, Base())], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "VARIANTE_DUPLICADA");
    }

    [Fact]
    public async Task CodBarDuplicadoArchivo_EsError()
    {
        var ctx = new Ctx();
        var b2 = Base(); b2["CodigoProducto *"] = "PROD-002"; b2["NombreProducto *"] = "Azúcar"; b2["CodigoVariante *"] = "PROD-002-UN";
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base()), Fila(3, b2)], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "CODBAR_DUPLICADO_ARCHIVO");
    }

    [Fact]
    public async Task CodBarExisteOtraVariante_EsError()
    {
        var ctx = new Ctx();
        var otroProd = new Prod { IdProd = 9, IdTenant = 4, IdCategoria = 1, Codigo = "OTRO", Nombre = "Otro", IdUniBase = 1 };
        var otraVar = new VarProd { IdVarProd = 9, IdProd = 9, Codigo = "OTRA-VAR", Nombre = "Otra", Prod = otroProd };
        ctx.Cods.Datos = [new CodBar { Codigo = "5901234123457", PresProd = new PresProd { IdVarProd = 9, IdUnidad = 1, VarProd = otraVar } }];
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base())], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "CODBAR_EXISTE_BBDD");
    }

    [Fact]
    public async Task VarianteDeOtroProducto_EsError()
    {
        var ctx = new Ctx();
        var otroProd = new Prod { IdProd = 9, IdTenant = 4, IdCategoria = 1, Codigo = "OTRO", Nombre = "Otro", IdUniBase = 1 };
        ctx.Vars.Datos = [new VarProd { IdVarProd = 9, IdProd = 9, Codigo = "VAR-X", Nombre = "X", Prod = otroProd }];
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base(), ("CodigoVariante *", "VAR-X"))], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "CODVAR_MISMATCH");
    }

    [Fact]
    public async Task UnidadBaseDistintaExistente_EsError()
    {
        var ctx = new Ctx();
        ctx.Prods.Datos = [new Prod { IdProd = 7, IdTenant = 4, IdCategoria = 1, Codigo = "PROD-001", Nombre = "Arroz Grano Largo", IdUniBase = 2 }];
        var r = await ctx.Validador().ValidarAsync([Fila(2, Base())], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Contains(r.Errores, e => e.Codigo == "UNIDADBASE_MISMATCH");
    }

    [Fact]
    public async Task VarianteExistenteConStock_AvisaSinBloquear()
    {
        var ctx = new Ctx();
        var prod = new Prod { IdProd = 7, IdTenant = 4, IdCategoria = 1, Codigo = "PROD-001", Nombre = "Arroz Grano Largo", IdUniBase = 1 };
        ctx.Prods.Datos = [prod];
        ctx.Vars.Datos = [new VarProd { IdVarProd = 7, IdProd = 7, Codigo = "PROD-001-UN", Nombre = "Unidad", Prod = prod }];
        var b = Base(); b["StockInicial"] = "5"; b["Bodega"] = "BOD-01";
        var r = await ctx.Validador().ValidarAsync([Fila(2, b)], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(r.Errores);
        Assert.Equal(1, r.Existentes);
        Assert.Contains(r.Advertencias, a => a.Codigo == "STOCK_SE_IGNORA");
    }

    [Fact]
    public async Task SinCodigoBarras_AvisaGeneracion()
    {
        var ctx = new Ctx();
        var b = Base(); b.Remove("CodigoBarras");
        var r = await ctx.Validador().ValidarAsync([Fila(2, b)], 4, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(r.Errores);
        Assert.Contains(r.Advertencias, a => a.Codigo == "CODBAR_SE_GENERARA");
    }
}

