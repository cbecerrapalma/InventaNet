using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Importacion;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

public sealed class S88_ImportacionesControllerTests
{
    private sealed class FakeAplicador : IProductoImportacionAplicadorAsync
    {
        public Func<long, int, int, CancellationToken, Task<Result<AplicarImportacionResult>>>? OnAplicar;
        public Task<Result<AplicarImportacionResult>> AplicarAsync(long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct = default) =>
            OnAplicar is null ? throw new NotImplementedException() : OnAplicar(idImportacionLote, idTenant, idUsuario, ct);
    }

    private static ImportacionesController NewController(FakeAplicador aplicador)
    {
        var c = new ImportacionesController(new FakeImportacionesSvc(), aplicador);
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = 4;
        http.Items["IdUsuario"] = 3;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return c;
    }

    private sealed class FakeImportacionesSvc : IImportacionServiceAsync
    {
        public Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<LoteCreadoResult>> ValidarProductosAsync(int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<CambiosPaginaResult>> GetCambiosAsync(long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default) => Task.FromResult(Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(true, [new CambioFila(2, "P1", "V1", AccionCambio.Modificado, [new CambioCampo("Precio de venta", "$1.000", "$1.100")])], page, pageSize, 1)));
        public Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private static AplicarImportacionResult OkResult() => new()
    {
        IdImportacionLote = 1,
        Estado = "Aplicado",
        ProductosNuevos = 5,
        ProductosExistentes = 2,
        VariantesNuevas = 5,
        VariantesExistentes = 2,
        PresentacionesNuevas = 5,
        PreciosCreados = 5,
        CodigosBarraCreados = 5,
        StockAjustado = 0,
        ResumenJson = "{\"ProductosNuevos\":5}"
    };

    [Fact]
    public async Task Aplicar_Ok_Retorna200ConResultado()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(Result<AplicarImportacionResult>.Success(OkResult()))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(r);
        var res = Assert.IsType<AplicarImportacionResult>(ok.Value);
        Assert.Equal("Aplicado", res.Estado);
        Assert.Equal(5, res.ProductosNuevos);
    }

    [Fact]
    public async Task Aplicador_LoteNoExiste_Retorna404()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithHttpStatus(404)))
        };
        var r = await NewController(aplicador).Aplicar(999, CancellationToken.None);
        Assert.Equal(404, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_LoteEstadoInvalido_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_ESTADO_INVALIDO", "Estado inválido").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_ValidacionFallida_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("VALIDACION_FALLIDA", "Errores en lote").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_CategoriaNoExiste_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("CATEGORIA_NO_EXISTE", "Categoría no existe").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_CodBarDuplicado_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("CODBAR_DUPLICADO", "Código duplicado").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_ProveedorNoExiste_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("PROVEEDOR_NO_EXISTE", "Proveedor no existe").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_StockFalla_Retorna200ConStockCero()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Success(new AplicarImportacionResult
                {
                    IdImportacionLote = 1,
                    Estado = "Aplicado",
                    ProductosNuevos = 3,
                    StockAjustado = 0,
                    ResumenJson = "{\"StockAjustado\":0}"
                }))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(r);
        var res = Assert.IsType<AplicarImportacionResult>(ok.Value);
        Assert.Equal(0, res.StockAjustado);
    }

    [Fact]
    public async Task Aplicador_LoteTipoInvalido_Retorna400()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_TIPO_INVALIDO", "Tipo inválido").WithHttpStatus(400)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_ArchivoNoEncontrado_Retorna404()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_SIN_ARCHIVO", "Archivo no encontrado").WithHttpStatus(404)))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(404, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicador_ResumenJsonContieneResultados()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(Result<AplicarImportacionResult>.Success(OkResult()))
        };
        var r = await NewController(aplicador).Aplicar(1, CancellationToken.None);
        var ok = Assert.IsType<OkObjectResult>(r);
        var res = Assert.IsType<AplicarImportacionResult>(ok.Value);
        Assert.Contains("ProductosNuevos", res.ResumenJson);
    }
}
