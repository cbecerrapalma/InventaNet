using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Importacion;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S87.2 C5: ImportacionesController a nivel controlador, sin host ni DB.
public sealed class S87_ImportacionesControllerTests
{
    private sealed class FakeImportaciones : IImportacionServiceAsync
    {
        public Func<int, CancellationToken, Task<Result<PlantillaProductoResult>>>? OnPlantilla;
        public Func<int, int, string, byte[], CancellationToken, Task<Result<LoteCreadoResult>>>? OnValidar;
        public Func<long, int, CancellationToken, Task<Result<LoteDetalleResult>>>? OnLote;
        public Func<long, int, CancellationToken, Task<Result<ReporteLoteResult>>>? OnReporte;
        public Func<int, int, int, CancellationToken, Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>>>? OnListar;

        public Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default) =>
            OnPlantilla is null ? throw new NotImplementedException() : OnPlantilla(idTenant, ct);
        public Task<Result<LoteCreadoResult>> ValidarProductosAsync(int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default) =>
            OnValidar is null ? throw new NotImplementedException() : OnValidar(idTenant, idUsuario, fileName, contenido, ct);
        public Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default) =>
            OnLote is null ? throw new NotImplementedException() : OnLote(id, idTenant, ct);
        public Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default) =>
            OnReporte is null ? throw new NotImplementedException() : OnReporte(id, idTenant, ct);
        public Task<Result<CambiosPaginaResult>> GetCambiosAsync(long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default) => Task.FromResult(Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(true, [new CambioFila(2, "P1", "V1", AccionCambio.Modificado, [new CambioCampo("Precio de venta", "$1.000", "$1.100")])], page, pageSize, 1)));
        public Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) =>
            OnListar is null ? throw new NotImplementedException() : OnListar(idTenant, page, pageSize, ct);
    }

    private sealed class FakeAplicador : IProductoImportacionAplicadorAsync
    {
        public Func<long, int, int, CancellationToken, Task<Result<AplicarImportacionResult>>>? OnAplicar = null;
        public Task<Result<AplicarImportacionResult>> AplicarAsync(long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct = default) =>
            OnAplicar is null ? throw new NotImplementedException() : OnAplicar(idImportacionLote, idTenant, idUsuario, ct);
    }

    private static ImportacionesController NewController(FakeImportaciones svc, FakeAplicador? aplicador = null)
    {
        var c = new ImportacionesController(svc, aplicador ?? new FakeAplicador());
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = 4;
        http.Items["IdUsuario"] = 8;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return c;
    }

    private static IFormFile Archivo(string nombre = "lote.xlsx")
    {
        var bytes = new byte[] { 0x50, 0x4B, 0x03, 0x04 };
        return new FormFile(new MemoryStream(bytes), 0, bytes.Length, "file", nombre);
    }

    private static LoteCreadoResult LoteOk() => new(1, "Validado", 10, 5, 10, 10, 0, 0, 2, false, []);

    [Fact]
    public async Task Plantilla_Ok_RetornaFile()
    {
        var svc = new FakeImportaciones
        {
            OnPlantilla = (_, _) => Task.FromResult(Result<PlantillaProductoResult>.Success(new([1, 2], "plantilla.xlsx")))
        };
        var r = await NewController(svc).DescargarPlantillaProductos(CancellationToken.None);
        var f = Assert.IsType<FileContentResult>(r);
        Assert.Equal("plantilla.xlsx", f.FileDownloadName);
    }

    [Fact]
    public async Task Plantilla_Error_Retorna400()
    {
        var svc = new FakeImportaciones
        {
            OnPlantilla = (_, _) => Task.FromResult(
                Result<PlantillaProductoResult>.Failure(new Error("SIN_CATALOGO", "x").WithHttpStatus(400)))
        };
        var r = await NewController(svc).DescargarPlantillaProductos(CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Validar_SinArchivo_Retorna400()
    {
        var r = await NewController(new FakeImportaciones()).ValidarProductos(null!, CancellationToken.None);
        Assert.Equal(400, Assert.IsType<BadRequestObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Validar_Ok_Retorna201()
    {
        var svc = new FakeImportaciones { OnValidar = (_, _, _, _, _) => Task.FromResult(Result<LoteCreadoResult>.Success(LoteOk())) };
        var r = await NewController(svc).ValidarProductos(Archivo(), CancellationToken.None);
        var o = Assert.IsType<ObjectResult>(r);
        Assert.Equal(201, o.StatusCode);
        Assert.Equal(1L, ((LoteCreadoResult)o.Value!).Id);
    }

    [Fact]
    public async Task Validar_ErrorServicio_Retorna400()
    {
        var svc = new FakeImportaciones
        {
            OnValidar = (_, _, _, _, _) => Task.FromResult(
                Result<LoteCreadoResult>.Failure(new Error("FORMATO_INVALIDO", "x").WithHttpStatus(400)))
        };
        var r = await NewController(svc).ValidarProductos(Archivo(), CancellationToken.None);
        Assert.Equal(400, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task GetLote_Ok()
    {
        var det = new LoteDetalleResult(1, "PRODUCTOS", "lote.xlsx", "Validado", 10, 5, 10, 10, 0, 0, 2, DateTime.UtcNow, null, []);
        var svc = new FakeImportaciones { OnLote = (_, _, _) => Task.FromResult(Result<LoteDetalleResult>.Success(det)) };
        var r = await NewController(svc).GetLote(1, CancellationToken.None);
        Assert.Equal("Validado", ((LoteDetalleResult)Assert.IsType<OkObjectResult>(r).Value!).Estado);
    }

    [Fact]
    public async Task GetLote_OtroTenant_Retorna404()
    {
        var svc = new FakeImportaciones
        {
            OnLote = (_, _, _) => Task.FromResult(
                Result<LoteDetalleResult>.Failure(new Error("LOTE_NO_ENCONTRADO", "x").WithType(ErrorType.NotFound).WithHttpStatus(404)))
        };
        var r = await NewController(svc).GetLote(99, CancellationToken.None);
        Assert.Equal(404, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Reporte_Ok_RetornaFile()
    {
        var svc = new FakeImportaciones
        {
            OnReporte = (_, _, _) => Task.FromResult(Result<ReporteLoteResult>.Success(new([1], "reporte.xlsx")))
        };
        var r = await NewController(svc).GetReporte(1, CancellationToken.None);
        Assert.Equal("reporte.xlsx", Assert.IsType<FileContentResult>(r).FileDownloadName);
    }

    [Fact]
    public async Task Listar_Ok_RetornaPaginado()
    {
        IReadOnlyList<LoteItemResult> items = [new(1, "PRODUCTOS", "Validado", 10, 0, DateTime.UtcNow, "lote-1.xlsx", 0, null)];
        var svc = new FakeImportaciones
        {
            OnListar = (_, _, _, _) => Task.FromResult(Result<(IReadOnlyList<LoteItemResult> Items, int Total)>.Success((items, 1)))
        };
        var r = await NewController(svc).Listar(1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.IsType<OkObjectResult>(r);
    }
}

