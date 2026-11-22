using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CBP.Excel.Abstractions;
using CBP.Excel.Parsing;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Moq;
using Xunit;

namespace Inventa.Application.Tests;

public sealed class S88_ImportacionAplicadorTests
{
    private const int Tenant = 4;
    private const int Usuario = 3;
    private static readonly byte[] FakeExcel = new byte[] { 1, 2, 3 };

    private sealed class Ctx
    {
        public StubLoteRepo Lotes = new();
        public StubStorageProvider Archivos = new() { ContenidoPorDefecto = FakeExcel };
        public StubProdImportRepo Prods = new();
        public StubVarProdImportRepo Vars = new();
        public StubPresProdImportRepo Press = new();
        public StubBodegaImportRepo Bodegas = new() { Datos = [new Bodega { IdBodega = 1, IdTenant = Tenant, Codigo = "S88-BOD", Nombre = "Bodega Test", Activo = true }] };
        public StubSpRepo SpRepo = new();
        public StubStockRepo StockRepo = new();
        public StubExcelReader Reader = new();

        /// <summary>S132: transacción simulada; corre el trabajo y anota si pidió confirmar (null = no se abrió).</summary>
        public bool? Confirmado;
        public Mock<ICatalogoEdicionRepositoryAsync> Transaccion { get; } = new();

        // S132: repos de la fase de stock del aplicador (los stubs compartidos no implementan esas lecturas).
        public IBodegaRepositoryAsync? BodegaAplicador;
        public IProdRepositoryAsync? ProdAplicador;
        public IVarProdRepositoryAsync? VarAplicador;
        public IPresProdRepositoryAsync? PresAplicador;

        public Ctx()
        {
            Transaccion
                .Setup(t => t.EnTransaccionAsync<Result<AplicarImportacionResult>>(
                    It.IsAny<Func<CancellationToken, Task<(Result<AplicarImportacionResult> Resultado, bool Confirmar)>>>(),
                    It.IsAny<CancellationToken>()))
                .Returns(async (Func<CancellationToken, Task<(Result<AplicarImportacionResult> Resultado, bool Confirmar)>> trabajo, CancellationToken ct) =>
                {
                    var (resultado, confirmar) = await trabajo(ct);
                    Confirmado = confirmar;
                    return resultado;
                });
        }

        /// <summary>S132: la fila por defecto carga stock inicial en S88-BOD; la presentación de la unidad base existe o no.</summary>
        public void ConStockInicial(bool conPresentacionBase)
        {
            Reader.Extra["StockInicial"] = "5";
            Reader.Extra["Bodega"] = "S88-BOD";

            var bodega = new Mock<IBodegaRepositoryAsync>();
            bodega.Setup(b => b.GetByCodigoAsync(Tenant, It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Bodega { IdBodega = 1, IdTenant = Tenant, Codigo = "S88-BOD", Nombre = "Bodega Test", Activo = true });
            BodegaAplicador = bodega.Object;

            var prod = new Prod { IdProd = 1, IdTenant = Tenant, Codigo = "S88-P1", Nombre = "Producto S88", IdCategoria = 5, IdUniBase = 1, Activo = true };
            var prods = new Mock<IProdRepositoryAsync>();
            prods.Setup(p => p.GetByCodigoAsync(Tenant, It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync(prod);
            ProdAplicador = prods.Object;

            var variante = new VarProd { IdVarProd = 50, IdProd = 1, Codigo = "S88-P1-01", Nombre = "Unica", Prod = prod };
            var vars = new Mock<IVarProdRepositoryAsync>();
            vars.Setup(v => v.GetByProductoAsync(1L, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<VarProd>)new List<VarProd> { variante });
            VarAplicador = vars.Object;

            var pres = new PresProd { IdPresProd = 100, IdVarProd = 50, IdUnidad = conPresentacionBase ? 1 : 2, Descrip = "x", VarProd = variante };
            var press = new Mock<IPresProdRepositoryAsync>();
            press.Setup(p => p.GetByVarianteAsync(50L, true, It.IsAny<CancellationToken>()))
                .ReturnsAsync((IReadOnlyList<PresProd>)new List<PresProd> { pres });
            PresAplicador = press.Object;
        }

        public ProductoImportacionAplicadorAsync Aplicador(ProductoImportacionValidadorAsync validador) => new(
            Lotes, Archivos, validador, SpRepo, StockRepo,
            ProdAplicador ?? Prods, VarAplicador ?? Vars, PresAplicador ?? Press, BodegaAplicador ?? Bodegas,
            Transaccion.Object);

        public StubCategoriaImportRepo Categorias = new();
        public StubCodBarImportRepo Cods = new();

        public ProductoImportacionValidadorAsync Validador() => new(
            Categorias, new StubMarcaImportRepo(),
            new StubUnidadImportRepo { Datos = [new Unidad { IdUnidad = 1, Codigo = "UN", Nombre = "Unidad" }] },
            Bodegas, Prods, Vars, Cods, Reader);

        /// <summary>S91.4: catálogo que coincide EXACTAMENTE con la fila por defecto del lector (S88-P1 / S88-P1-01).</summary>
        public void CatalogoIgualAlArchivo(string? codigoBarras = "5901234123457")
        {
            var prod = new Prod { IdProd = 1, IdTenant = Tenant, Codigo = "S88-P1", Nombre = "Producto S88", IdCategoria = 5, IdUniBase = 1, Activo = true };
            var variante = new VarProd { IdVarProd = 50, IdProd = 1, Codigo = "S88-P1-01", Nombre = "Unica", Prod = prod };
            var pres = new PresProd { IdPresProd = 100, IdVarProd = 50, IdUnidad = 1, Descrip = "x", VarProd = variante };
            Categorias.Datos = [new Categoria { IdCategoria = 5, IdTenant = Tenant, Codigo = "S88-CAT", Nombre = "S88-CAT" }];
            Prods.Datos = [prod];
            Vars.Datos = [variante];
            Cods.Datos = codigoBarras is null ? [] : [new CodBar { IdCodBar = 1, IdPresProd = 100, Codigo = codigoBarras, TipoCod = 1, Principal = true, PresProd = pres }];
            SpRepo.Comercial = new EstadoComercialImportacion
            {
                Presentaciones = new Dictionary<(long, int), long> { [(50L, 1)] = 100L },
                UltimoPrecio = new Dictionary<long, decimal> { [100L] = 1000m },
            };
        }
    }

    private static ProductoImportRow Fila(string prod, string variante, string? codBar = "5901234123457", string precio = "10000", string stock = "0")
    {
        return new ProductoImportRow
        {
            CodigoProducto = prod,
            NombreProducto = "Producto " + prod,
            Categoria = "S88-CAT",
            Marca = "S88-MAR",
            UnidadBase = "S88-UN",
            Pesable = "N",
            ManejaLote = "N",
            DiasVencimiento = "",
            PublicarEnTienda = "S",
            CodigoVariante = variante,
            NombreVariante = "Variante " + variante,
            CodigoBarras = codBar ?? "",
            CodigosAdicionales = "",
            PrecioVenta = precio,
            PrecioOferta = "",
            OfertaDesde = "",
            OfertaHasta = "",
            CostoCompra = "5000",
            StockInicial = stock,
            Bodega = "S88-BOD"
        };
    }

    private static ImportacionLote Lote(long id = 1) => new()
    {
        IdImportacionLote = id,
        IdTenant = Tenant,
        Tipo = ImportacionLoteTipos.Productos,
        Estado = ImportacionLoteEstados.Validado,
        RutaArchivo = "test.xlsx"
    };

    [Fact]
    public async Task Aplicador_LoteNoExisten_Retorna404()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = null;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal(404, r.Error!.HttpStatusCode);
    }

    [Fact]
    public async Task Aplicador_LoteEstadoInvalido_Retorna400()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote(); ctx.Lotes.Lote.Estado = "Borrador";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_ESTADO_INVALIDO", r.Error!.Code);
    }

    // ── S91: idempotencia (Aplicado/Fallido → 409) ──

    [Fact]
    public async Task Aplicador_LoteYaAplicado_Retorna409()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote(); ctx.Lotes.Lote.Estado = ImportacionLoteEstados.Aplicado;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_YA_APLICADO", r.Error!.Code);
        Assert.Equal(409, r.Error!.HttpStatusCode);
        Assert.False(ctx.SpRepo.AplicarCalled);
    }

    [Fact]
    public async Task Aplicador_LoteFallido_Retorna409()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote(); ctx.Lotes.Lote.Estado = ImportacionLoteEstados.Fallido;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_FALLIDO", r.Error!.Code);
        Assert.Equal(409, r.Error!.HttpStatusCode);
        Assert.False(ctx.SpRepo.AplicarCalled);
    }

    [Fact]
    public async Task Aplicador_ConErroresRevalidadoLimpio_Aplica()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote(); ctx.Lotes.Lote.Estado = ImportacionLoteEstados.ConErrores;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        Assert.True(ctx.SpRepo.AplicarCalled);
        Assert.True(ctx.Lotes.UpdateResultadoCalled);
    }

    [Fact]
    public async Task Aplicador_ValidacionConErrores_Retorna400YLoteFallido()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Reader.ReturnInvalidRow = true;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal(400, r.Error!.HttpStatusCode);
        Assert.True(ctx.Lotes.UpdateEstadoCalled);
    }

    [Fact]
    public async Task Aplicador_SPFalla_PropagaErrorYMarcaLoteFallido()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = true;
        ctx.SpRepo.FailError = new Error("CATEGORIA_NO_EXISTE", "Categoría no existe");
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("CATEGORIA_NO_EXISTE", r.Error!.Code);
        Assert.True(ctx.SpRepo.AplicarCalled);
        Assert.True(ctx.Lotes.UpdateEstadoCalled);
    }

    [Fact]
    public async Task Aplicador_StockFalla_RevierteYNoAplica()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        ctx.ConStockInicial(conPresentacionBase: true);
        ctx.StockRepo.ShouldFail = true;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("STOCK_INICIAL_FALLIDO", r.Error!.Code);
        Assert.Equal(409, r.Error.HttpStatusCode);
        Assert.True(ctx.StockRepo.AjustarCalled);
        Assert.False(ctx.Confirmado);
        Assert.False(ctx.Lotes.UpdateResultadoCalled);
        Assert.False(ctx.Lotes.UpdateEstadoCalled);
    }

    [Fact]
    public async Task Aplicador_StockSinDestino_RevierteYNoAplica()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        ctx.ConStockInicial(conPresentacionBase: false);
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        // D1 envuelve cualquier falla de la fase de stock en STOCK_INICIAL_FALLIDO; el detalle trae el motivo de D2.
        Assert.Equal("STOCK_INICIAL_FALLIDO", r.Error!.Code);
        Assert.Contains("no se encontró la presentación de la unidad base", r.Error.Message);
        Assert.Equal(409, r.Error.HttpStatusCode);
        Assert.False(ctx.StockRepo.AjustarCalled);
        Assert.False(ctx.Confirmado);
        Assert.False(ctx.Lotes.UpdateResultadoCalled);
        Assert.False(ctx.Lotes.UpdateEstadoCalled);
    }

    [Fact]
    public async Task Aplicador_Exito_ActualizaLoteAAplicado()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        Assert.Equal("Aplicado", r.Value.Estado);
        Assert.True(ctx.Confirmado);
        Assert.True(ctx.Confirmado);
        Assert.True(ctx.Lotes.UpdateResultadoCalled);
    }

    [Fact]
    public async Task Aplicador_ArchivoNoEncontrado_Retorna404()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Archivos.ContenidoPorDefecto = null;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_SIN_ARCHIVO", r.Error!.Code);
    }

    [Fact]
    public async Task Aplicador_LoteTipoInvalido_Retorna400()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote(); ctx.Lotes.Lote.Tipo = "OTROS";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_TIPO_INVALIDO", r.Error!.Code);
    }

    [Fact]
    public async Task Aplicador_ResumenJsonContieneResultadosSP()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        Assert.Contains("ProductosNuevos", r.Value.ResumenJson);
    }

    [Fact]
    public async Task S912_Aplicador_ConservaIncidenciasDeValidacionAlAplicar()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Lotes.Lote.ResumenJson = "{\"errores\":0,\"advertencias\":1,\"incidencias\":[{\"Fila\":2,\"Columna\":\"Marca\",\"Codigo\":\"MARCA_SE_CREARA\",\"Mensaje\":\"m\",\"Severidad\":\"Advertencia\"}]}";
        ctx.SpRepo.ShouldFail = false;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        using var doc = System.Text.Json.JsonDocument.Parse(ctx.Lotes.ResumenGuardado!);
        Assert.Equal(1, doc.RootElement.GetProperty("incidencias").GetArrayLength());
        Assert.Equal(1, doc.RootElement.GetProperty("advertencias").GetInt32());
        Assert.True(doc.RootElement.GetProperty("aplicacion").TryGetProperty("ProductosNuevos", out _));
    }

    [Fact]
    public async Task Aplicador_StockSoloSeEjecutaSiStockInicialPositivo()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        Assert.Equal(0, r.Value.StockAjustado);
        Assert.False(ctx.StockRepo.AjustarCalled);
    }

    [Fact]
    public async Task Aplicador_ConStockInicial_EjecutaFaseStock()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.SpRepo.ShouldFail = false;
        ctx.StockRepo.ShouldFail = false;
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
    }

    [Fact]
    public async Task S914_Aplicador_SinCambios_Retorna409YNoLlamaAlSp()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.CatalogoIgualAlArchivo();
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("SIN_CAMBIOS", r.Error!.Code);
        Assert.Equal(409, r.Error.HttpStatusCode);
        Assert.False(ctx.SpRepo.AplicarCalled);
    }

    [Fact]
    public async Task S914_Aplicador_HuellaDistinta_Retorna409Desactualizado()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Lotes.Lote.ResumenJson = "{\"cambiosSha256\":\"00\"}";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.False(r.IsSuccess);
        Assert.Equal("LOTE_DESACTUALIZADO", r.Error!.Code);
        Assert.Equal(409, r.Error.HttpStatusCode);
        Assert.False(ctx.SpRepo.AplicarCalled);
    }

    [Fact]
    public async Task S914_Aplicador_HuellaIgual_Aplica()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        var v = await ctx.Validador().ValidarAsync(await LeerAsync(ctx), Tenant, CancellationToken.None);
        var cambios = await ImportacionServiceAsync.CalcularCambiosAsync(v, ctx.SpRepo, Tenant, CancellationToken.None);
        ctx.Lotes.Lote.ResumenJson = "{\"cambiosSha256\":\"" + cambios.Sha256 + "\"}";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        Assert.True(ctx.SpRepo.AplicarCalled);
    }

    [Fact]
    public async Task S914_Aplicador_CodigoVacioEnVarianteExistente_NoGeneraInterno()
    {
        // Archivo igual al catálogo pero con precio distinto y SIN código: debe conservar el principal (TVP CodigoBarras null).
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.CatalogoIgualAlArchivo();
        ctx.Reader.SinCodigoYPrecio = "1500";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        var item = Assert.Single(ctx.SpRepo.Recibidas!);
        Assert.Null(item.CodigoBarras);
        Assert.Null(item.TipoCod);
    }

    [Fact]
    public async Task S92_Aplicador_VarianteNuevaSinCodigo_LlegaNullAlSp_ParaQueGenereInterno()
    {
        // S92 K7: el aplicador ya no genera códigos en C# (antes: secuencia estática y TipoCod 1).
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Reader.SinCodigoYPrecio = "1000";
        var r = await ctx.Aplicador(ctx.Validador()).AplicarAsync(1, Tenant, Usuario, CancellationToken.None);
        Assert.True(r.IsSuccess);
        var item = Assert.Single(ctx.SpRepo.Recibidas!);
        Assert.Null(item.CodigoBarras);
        Assert.Null(item.TipoCod);
    }

    // S93.1 (D5/D6): PrecioMayorista y Proveedor ya no son columnas: si vienen, aviso COLUMNA_IGNORADA (no error)
    // y el TVP viaja con NULL; CostoCompra sigue llegando (costo promedio de la variante).
    [Fact]
    public async Task S931_ColumnasRetiradas_AvisoColumnaIgnorada_YTvpEnNull()
    {
        var ctx = new Ctx();
        ctx.Lotes.Lote = Lote();
        ctx.Reader.Extra["PrecioMayorista"] = "800";
        ctx.Reader.Extra["Proveedor"] = "PROV-INEXISTENTE";
        ctx.Reader.Extra["CostoCompra"] = "500";
        var v = await ctx.Validador().ValidarAsync(await LeerAsync(ctx), Tenant, CancellationToken.None);
        Assert.Empty(v.Errores);
        Assert.Equal(2, v.Advertencias.Count(a => a.Codigo == "COLUMNA_IGNORADA"));
        Assert.Contains(v.Advertencias, a => a.Codigo == "COLUMNA_IGNORADA" && a.Columna == ExcelValueParser.NormalizeHeader("Proveedor"));
        var item = ProductoImportTvpMapper.Map(Assert.Single(v.Filas).Row);
        Assert.Null(item.PrecioMayorista);
        Assert.Null(item.Proveedor);
        Assert.Equal(500m, item.CostoCompra);
    }

    [Fact]
    public void S931_Plantilla_SinMayoristaNiProveedor()
    {
        var o = ProductoPlantilla.Build(["UN"], ["BOD"], ["Vestuario"], ["Ficcus"]);
        Assert.DoesNotContain(o.Columns, c => c.PropertyName is "PrecioMayorista" or "Proveedor");
        Assert.DoesNotContain(o.HiddenLists, l => l.Name == "Proveedores");
        Assert.Contains(o.Columns, c => c.PropertyName == "CostoCompra");
        Assert.All(o.SampleRows, r => Assert.False(r.Values.ContainsKey("PrecioMayorista") || r.Values.ContainsKey("Proveedor")));
    }

    private static async Task<List<ExcelRow>> LeerAsync(Ctx ctx)
    {
        var lista = new List<ExcelRow>();
        await foreach (var row in ctx.Reader.ReadRowsAsync(Stream.Null, new ExcelReadOptions())) lista.Add(row);
        return lista;
    }

    // ── Stubs ──

    private sealed class StubExcelReader : IExcelReader
    {
        public bool ReturnInvalidRow { get; set; }
        /// <summary>S91.4: fila por defecto sin CodigoBarras y con este PrecioVenta.</summary>
        public string? SinCodigoYPrecio { get; set; }
        /// <summary>S93.1: columnas adicionales de la fila por defecto (p. ej. columnas retiradas).</summary>
        public Dictionary<string, string> Extra { get; } = [];
        public IReadOnlyList<ExcelRow> ReadRows(Stream stream, ExcelReadOptions options) => [];
        public IReadOnlyList<T> ReadRows<T>(Stream stream, ExcelReadOptions options) where T : class, new() => [];
        public IAsyncEnumerable<ExcelRow> ReadRowsAsync(Stream stream, ExcelReadOptions options, CancellationToken ct = default) => Empty();
        public IAsyncEnumerable<T> ReadRowsAsync<T>(Stream stream, ExcelReadOptions options, CancellationToken ct = default) where T : class, new() => Empty<T>();

        private async IAsyncEnumerable<ExcelRow> Empty()
        {
            if (!ReturnInvalidRow)
            {
                // S91.4: una fila válida de un producto NUEVO (antes el archivo venía vacío y "se aplicaba";
                // hoy un archivo sin cambios responde 409 SIN_CAMBIOS).
                var ok = new Dictionary<string, object?>();
                foreach (var (k, v) in new[]
                {
                    ("CodigoProducto *", "S88-P1"), ("NombreProducto *", "Producto S88"), ("Categoria *", "S88-CAT"),
                    ("UnidadBase *", "UN"), ("CodigoVariante *", "S88-P1-01"), ("NombreVariante *", "Unica"),
                    ("CodigoBarras", SinCodigoYPrecio is null ? "5901234123457" : ""), ("PrecioVenta *", SinCodigoYPrecio ?? "1000"),
                })
                    ok[ExcelValueParser.NormalizeHeader(k)] = v;
                foreach (var (k, v) in Extra) ok[ExcelValueParser.NormalizeHeader(k)] = v;
                yield return new ExcelRow { RowNumber = 2, Values = ok };
                yield break;
            }
            var dict = new Dictionary<string, object?>();
            foreach (var col in new[] { "CodigoProducto", "NombreProducto", "Categoria", "UnidadBase", "CodigoVariante", "NombreVariante" })
                dict[col] = "";
            yield return new ExcelRow { RowNumber = 2, Values = dict };
        }

        private static async IAsyncEnumerable<T> Empty<T>() where T : class, new()
        {
            yield break;
        }
    }

    private sealed class StubLoteRepo : IImportacionLoteRepositoryAsync
    {
        public ImportacionLote? Lote { get; set; }
        public bool UpdateEstadoCalled { get; private set; }
        public bool UpdateResultadoCalled { get; private set; }
        public Task<ImportacionLote> CreateAsync(ImportacionLote entity, CancellationToken ct = default) => Task.FromResult(entity);
        public Task UpdateRutaAsync(long id, int idTenant, string rutaArchivo, CancellationToken ct = default) => Task.CompletedTask;
        public Task<ImportacionLote?> GetByIdInTenantAsync(long id, int idTenant, CancellationToken ct = default) => Task.FromResult(Lote);
        public Task<bool> ExistsAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default) => Task.FromResult(false);
        public Task<ImportacionLote?> GetUltimoAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default) => Task.FromResult<ImportacionLote?>(null);
        public Task<(IReadOnlyList<ImportacionLote> Items, int Total)> SearchPagedByTenantAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) => Task.FromResult<(IReadOnlyList<ImportacionLote>, int)>(([], 0));
        public Task UpdateEstadoAsync(long id, int idTenant, string estado, CancellationToken ct = default) { UpdateEstadoCalled = true; return Task.CompletedTask; }
        public Task UpdateResultadoAsync(long id, int idTenant, string estado, string resumenJson, CancellationToken ct = default) { UpdateResultadoCalled = true; ResumenGuardado = resumenJson; return Task.CompletedTask; }
        public string? ResumenGuardado { get; private set; }
    }

    private sealed class StubSpRepo : IProductoImportSpRepositoryAsync
    {
        public EstadoComercialImportacion Comercial { get; set; } = new();
        public Task<EstadoComercialImportacion> GetEstadoComercialAsync(int idTenant, CancellationToken ct) => Task.FromResult(Comercial);
        public bool AplicarCalled { get; private set; }
        public IReadOnlyList<ProductoImportTvpItem>? Recibidas { get; private set; }
        public bool ShouldFail { get; set; }
        public Error? FailError { get; set; }
        public Task<Result<ImportacionResultadoSp>> AplicarAsync(int idTenant, int idUsuario, IReadOnlyList<ProductoImportTvpItem> filas, CancellationToken ct)
        {
            AplicarCalled = true; Recibidas = filas;
            if (ShouldFail) return Task.FromResult(Result<ImportacionResultadoSp>.Failure(FailError!));
            return Task.FromResult(Result<ImportacionResultadoSp>.Success(new ImportacionResultadoSp
            {
                ProductosNuevos = 1, ProductosExistentes = 0, VariantesNuevas = 1, VariantesExistentes = 0,
                PresentacionesNuevas = 1, PreciosCreados = 1, CodigosBarraCreados = 1, Errores = 0,
                ResumenJson = "{\"CodigosBarrasGenerados\":[{\"CodigoBarras\":\"2004000000013\"}]}"
            }));
        }
    }

    private sealed class StubStockRepo : IStockRepositoryAsync
    {
        public bool AjustarCalled { get; private set; }
        public bool ShouldFail { get; set; }
        private static Task<TResult> NiM<TResult>([CallerMemberName] string m = "") => throw new NotImplementedException(m);
        public Task<IReadOnlyList<Stock>> GetByTenantBodegaAsync(int idTenant, int idBodega, bool incluirLote = false, CancellationToken ct = default) => NiM<IReadOnlyList<Stock>>();
        public Task<IReadOnlyList<Stock>> GetByVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default) => NiM<IReadOnlyList<Stock>>();
        public Task<Stock?> GetAsync(int idTenant, int idBodega, long idVarProd, long? idLote = null, CancellationToken ct = default) => NiM<Stock?>();
        public Task<IReadOnlyList<Stock>> GetConStockNegativoAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<Stock>>();
        public Task<decimal?> GetCantidadTotalAsync(int idTenant, long idVarProd, CancellationToken ct = default) => NiM<decimal?>();
        public Task<decimal?> GetCantidadEnBodegaAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default) => NiM<decimal?>();
        public Task<decimal?> GetCostoPromedioAsync(int idTenant, int idBodega, long idVarProd, CancellationToken ct = default) => NiM<decimal?>();
        public Task<(int Registros, decimal Cantidad, int Agotados)> GetSummaryAsync(int idTenant, CancellationToken ct = default) => NiM<(int, decimal, int)>();
        public Task<IReadOnlyList<Stock>> GetByTenantAsync(int idTenant, int? idBodega, int take, CancellationToken ct = default) => NiM<IReadOnlyList<Stock>>();
        public Task<(IReadOnlyList<StockListadoRow> Items, int Total)> SearchPagedAsync(int idTenant, int? idBodega, string? texto, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<StockListadoRow>, int)>();
        public Task<Result> AjustarInventarioAsync(AjustarInventarioCmd request, CancellationToken ct = default)
        {
            AjustarCalled = true;
            if (ShouldFail) return Task.FromResult(Result.Failure(new Error("STOCK_ERROR", "Error en ajuste de stock")));
            return Task.FromResult(Result.Success());
        }
        public Task<Result> TransferirEntresBodegasAsync(TransferirEntreBodegasCmd request, CancellationToken ct = default) => NiM<Result>();
    }

    private sealed class StubPresProdImportRepo : IPresProdRepositoryAsync
    {
        private static Task<TResult> NiM<TResult>([CallerMemberName] string m = "") => throw new NotImplementedException(m);
        public Task<IReadOnlyList<PresProd>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<IReadOnlyList<PresProd>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<PresProd?> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default) => NiM<PresProd?>();
        public Task<bool> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default) => NiM<bool>();
        public Task<IReadOnlyList<PresProd>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<IReadOnlyList<PresProd>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<int?> GetTenantIdAsync(long idPresProd, CancellationToken ct = default) => NiM<int?>();
        public Task<IReadOnlyDictionary<long, int>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default) => NiM<IReadOnlyDictionary<long, int>>();
        public Task<PresProd?> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default) => NiM<PresProd?>();
        public Task<PresProd?> GetByIdAsync(long idPresProd, CancellationToken ct = default) => NiM<PresProd?>();
        public Task<IReadOnlyList<PresProd>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<IReadOnlyList<PresProd>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
        public Task<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<PresentacionBusquedaRow>, int)>();
        public Task<PresProd> CreateAsync(PresProd entity, CancellationToken ct = default) => NiM<PresProd>();
    }
}


