using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Importacion;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S91: 409 idempotencia (Aplicado/Fallido), listado extendido, 413 archivo grande.
public sealed class S91_ImportacionesControllerTests
{
    private sealed class FakeAplicador : IProductoImportacionAplicadorAsync
    {
        public Func<long, int, int, CancellationToken, Task<Result<AplicarImportacionResult>>>? OnAplicar;
        public Task<Result<AplicarImportacionResult>> AplicarAsync(long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct = default) =>
            OnAplicar is null ? throw new NotImplementedException() : OnAplicar(idImportacionLote, idTenant, idUsuario, ct);
    }

    private sealed class FakeSvc : IImportacionServiceAsync
    {
        public Func<int, int, string, byte[], CancellationToken, Task<Result<LoteCreadoResult>>>? OnValidar;
        public Func<int, int, int, CancellationToken, Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>>>? OnListar;
        public Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<LoteCreadoResult>> ValidarProductosAsync(int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default) =>
            OnValidar is null ? throw new NotImplementedException() : OnValidar(idTenant, idUsuario, fileName, contenido, ct);
        public Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<CambiosPaginaResult>> GetCambiosAsync(long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default) => Task.FromResult(Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(true, [new CambioFila(2, "P1", "V1", AccionCambio.Modificado, [new CambioCampo("Precio de venta", "$1.000", "$1.100")])], page, pageSize, 1)));
        public Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) =>
            OnListar is null ? throw new NotImplementedException() : OnListar(idTenant, page, pageSize, ct);
    }

    private static ImportacionesController NewController(FakeSvc svc, FakeAplicador aplicador)
    {
        var c = new ImportacionesController(svc, aplicador);
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = 4;
        http.Items["IdUsuario"] = 3;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return c;
    }

    [Fact]
    public async Task Aplicar_LoteYaAplicado_Retorna409()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_YA_APLICADO", "Ya aplicado").WithType(ErrorType.Conflict).WithHttpStatus(409)))
        };
        var r = await NewController(new FakeSvc(), aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(409, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Aplicar_LoteFallido_Retorna409()
    {
        var aplicador = new FakeAplicador
        {
            OnAplicar = (_, _, _, _) => Task.FromResult(
                Result<AplicarImportacionResult>.Failure(new Error("LOTE_FALLIDO", "Fallido").WithType(ErrorType.Conflict).WithHttpStatus(409)))
        };
        var r = await NewController(new FakeSvc(), aplicador).Aplicar(1, CancellationToken.None);
        Assert.Equal(409, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Listar_IncluyeNombreArchivoAdvertenciasFecAplicada()
    {
        var fec = new DateTime(2026, 10, 1, 12, 0, 0, DateTimeKind.Utc);
        IReadOnlyList<LoteItemResult> items = [new(5, "PRODUCTOS", "Aplicado", 10, 0, fec, "lote-5.xlsx", 3, fec)];
        var svc = new FakeSvc
        {
            OnListar = (_, _, _, _) => Task.FromResult(Result<(IReadOnlyList<LoteItemResult> Items, int Total)>.Success((items, 1)))
        };
        var r = await NewController(svc, new FakeAplicador()).Listar(1, 20, Xunit.TestContext.Current.CancellationToken);
        var ok = Assert.IsType<OkObjectResult>(r);
        var devueltos = Assert.IsAssignableFrom<IReadOnlyList<LoteItemResult>>(
            ok.Value!.GetType().GetProperty("items")!.GetValue(ok.Value));
        var item = Assert.Single(devueltos);
        Assert.Equal("lote-5.xlsx", item.NombreArchivo);
        Assert.Equal(3, item.Advertencias);
        Assert.Equal(fec, item.FecAplicada);
    }

    [Fact]
    public async Task Validar_ArchivoMuyGrande_Retorna413()
    {
        var svc = new FakeSvc
        {
            OnValidar = (_, _, _, _, _) => Task.FromResult(
                Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_MUY_GRANDE", "Supera 10 MB").WithType(ErrorType.Validation).WithHttpStatus(413)))
        };
        var contenido = new byte[10];
        var file = new FormFile(new MemoryStream(contenido), 0, contenido.Length, "file", "grande.xlsx");
        var r = await NewController(svc, new FakeAplicador()).ValidarProductos(file, CancellationToken.None);
        Assert.Equal(413, Assert.IsType<ObjectResult>(r).StatusCode);
    }
}

