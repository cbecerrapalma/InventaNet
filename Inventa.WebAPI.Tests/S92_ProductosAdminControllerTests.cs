using System.Text.Json;
using CBP.Results;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

// S92: GET api/productos/{id}/ficha, /listado, /resumen, /publishability (capa HTTP delgada, tenant del contexto).
public sealed class S92_ProductosAdminControllerTests
{
    private sealed class FakeFicha : IProductoFichaServiceAsync
    {
        public (int Tenant, ProductoListadoConsulta Filtro, int Page, int PageSize)? Listado;
        public FichaProductoDto? Ficha;
        public Task<Result<FichaProductoDto?>> GetFichaAsync(int idTenant, long idProd, CancellationToken ct = default)
            => Task.FromResult(Result<FichaProductoDto?>.Success(idTenant == 4 && idProd == 1 ? Ficha : null, allowNull: true));
        public Task<Result<(IReadOnlyList<ProductoListadoItemDto> Items, int Total)>> ListarAsync(int idTenant, ProductoListadoConsulta filtro, int page, int pageSize, CancellationToken ct = default)
        {
            Listado = (idTenant, filtro, page, pageSize);
            IReadOnlyList<ProductoListadoItemDto> items = [new(1, "P1", "Polera", "Poleras", "Ficcus", true, true, EstadosTienda.Publicado, ["Sin imagen"], true, false, true, true, 2, 990m, 1290m)];
            return Task.FromResult(Result<(IReadOnlyList<ProductoListadoItemDto>, int)>.Success((items, 41)));
        }
        public Task<Result<ProductoResumenDto>> GetResumenAsync(int idTenant, CancellationToken ct = default)
            => Task.FromResult(Result<ProductoResumenDto>.Success(new ProductoResumenDto(10, 6, 1, 2, 0, 1, 1, 9, 0, 1)));
        public Task<Result<IReadOnlyList<PublishabilityRowDto>>> GetPublishabilityAsync(int idTenant, CancellationToken ct = default)
            => Task.FromResult(Result<IReadOnlyList<PublishabilityRowDto>>.Success(
                [new PublishabilityRowDto(1, "P1", "Polera", true, true, true, true, false, true, true, true, ["Sin imagen"], EstadosTienda.Publicado)]));
    }

    private static ProductosAdminController Ctl(FakeFicha f)
    {
        var c = new ProductosAdminController(f);
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = 4;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return c;
    }

    private static JsonElement Json(object? v) =>
        JsonDocument.Parse(JsonSerializer.Serialize(v, new JsonSerializerOptions(JsonSerializerDefaults.Web))).RootElement;

    [Fact]
    public async Task Ficha_ExistenteOk_YDeOtroTenantOInexistente404()
    {
        var f = new FakeFicha { Ficha = new FichaProductoDto(1, "P1", "Polera", "Poleras", null, "Unidad (UN)", false, false, null, true, false, 0, null, EstadosTienda.Listo, [], []) };
        Assert.Equal("P1", Json(Assert.IsType<OkObjectResult>(await Ctl(f).GetFicha(1, CancellationToken.None)).Value).GetProperty("codigo").GetString());
        Assert.IsType<NotFoundResult>(await Ctl(f).GetFicha(2, CancellationToken.None));
    }

    [Fact]
    public async Task Listado_MapeaFiltrosConSin_YPagina()
    {
        var f = new FakeFicha();
        var r = await Ctl(f).Listado("polera", "Listo", "sin", "con", "x", 7, true, 3, 50, CancellationToken.None);
        Assert.Equal(4, f.Listado!.Value.Tenant);
        Assert.Equal(new ProductoListadoConsulta("polera", "Listo", false, true, null, 7, true), f.Listado.Value.Filtro);
        Assert.Equal((3, 50), (f.Listado.Value.Page, f.Listado.Value.PageSize));
        var j = Json(Assert.IsType<OkObjectResult>(r).Value);
        Assert.Equal(41, j.GetProperty("totalCount").GetInt32());
        Assert.Equal("Publicado", j.GetProperty("items")[0].GetProperty("estado").GetString());
    }

    [Fact]
    public async Task Resumen_YPublishability_DevuelvenContratoCompleto()
    {
        var f = new FakeFicha();
        Assert.Equal(9, Json(Assert.IsType<OkObjectResult>(await Ctl(f).Resumen(CancellationToken.None)).Value).GetProperty("sinImagen").GetInt32());
        var p = Json(Assert.IsType<OkObjectResult>(await Ctl(f).GetPublishability(CancellationToken.None)).Value)[0];
        foreach (var campo in new[] { "tienePresVenta", "tienePrecio", "tieneImagen", "tieneMarca", "tieneCategoria", "publicable", "razones", "estado" })
            Assert.True(p.TryGetProperty(campo, out _), campo);
    }
}

