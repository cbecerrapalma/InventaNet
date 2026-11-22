using System.Text.Json;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Importacion;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S91.4: GET api/importaciones/{id}/cambios (paginación/filtro/disponible, 404 de otro tenant) y mapeo de los 409 nuevos.
public sealed class S914_ImportacionesCambiosControllerTests
{
    private sealed class FakeAplicador : IProductoImportacionAplicadorAsync
    {
        public Error? Falla;
        public Task<Result<AplicarImportacionResult>> AplicarAsync(long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct = default) =>
            Task.FromResult(Result<AplicarImportacionResult>.Failure(Falla!));
    }

    private sealed class FakeSvc : IImportacionServiceAsync
    {
        public (long Id, int Tenant, string? Accion, int Page, int PageSize)? Recibido;
        public Result<CambiosPaginaResult>? Respuesta;
        public Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<LoteCreadoResult>> ValidarProductosAsync(int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<CambiosPaginaResult>> GetCambiosAsync(long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default)
        {
            Recibido = (id, idTenant, accion, page, pageSize);
            return Task.FromResult(Respuesta!);
        }
    }

    private static ImportacionesController NewController(FakeSvc svc, FakeAplicador? aplicador = null)
    {
        var c = new ImportacionesController(svc, aplicador ?? new FakeAplicador());
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = 4;
        http.Items["IdUsuario"] = 3;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return c;
    }

    [Fact]
    public async Task GetCambios_PasaFiltroYPaginaDelTenant_YDevuelveDisponible()
    {
        var svc = new FakeSvc
        {
            Respuesta = Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(true,
                [new CambioFila(3, "P1", "V1", AccionCambio.Modificado, [new CambioCampo("Precio de venta", "$1.000", "$1.100")])], 2, 10, 11)),
        };
        var r = await NewController(svc).GetCambios(15, "Modificado", 2, 10, CancellationToken.None);
        Assert.Equal((15L, 4, "Modificado", 2, 10), svc.Recibido);
        var json = JsonSerializer.Serialize(Assert.IsType<OkObjectResult>(r).Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        using var doc = JsonDocument.Parse(json);
        Assert.True(doc.RootElement.GetProperty("disponible").GetBoolean());
        Assert.Equal(11, doc.RootElement.GetProperty("total").GetInt32());
        Assert.Equal(2, doc.RootElement.GetProperty("page").GetInt32());
        Assert.Equal("Precio de venta", doc.RootElement.GetProperty("items")[0].GetProperty("campos")[0].GetProperty("campo").GetString());
    }

    [Fact]
    public async Task GetCambios_LoteAnterior_DevuelveDisponibleFalse()
    {
        var svc = new FakeSvc { Respuesta = Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(false, [], 1, 25, 0)) };
        var r = await NewController(svc).GetCambios(1, null, 1, 25, CancellationToken.None);
        var json = JsonSerializer.Serialize(Assert.IsType<OkObjectResult>(r).Value, new JsonSerializerOptions(JsonSerializerDefaults.Web));
        Assert.False(JsonDocument.Parse(json).RootElement.GetProperty("disponible").GetBoolean());
    }

    [Fact]
    public async Task GetCambios_LoteDeOtroTenant_Retorna404()
    {
        var svc = new FakeSvc
        {
            Respuesta = Result<CambiosPaginaResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404)),
        };
        var r = await NewController(svc).GetCambios(99, null, 1, 25, CancellationToken.None);
        Assert.Equal(404, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Theory]
    [InlineData("SIN_CAMBIOS")]
    [InlineData("LOTE_DESACTUALIZADO")]
    public async Task Aplicar_ReglasNuevas_Retornan409ConDetalle(string codigo)
    {
        var aplicador = new FakeAplicador { Falla = new Error(codigo, "detalle " + codigo).WithType(ErrorType.Conflict).WithHttpStatus(409) };
        var r = await NewController(new FakeSvc(), aplicador).Aplicar(1, CancellationToken.None);
        var obj = Assert.IsType<ObjectResult>(r);
        Assert.Equal(409, obj.StatusCode);
        Assert.Equal("detalle " + codigo, Assert.IsType<ProblemDetails>(obj.Value, exactMatch: false).Detail);
    }
}

