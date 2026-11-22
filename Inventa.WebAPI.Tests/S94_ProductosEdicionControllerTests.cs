using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Catalogo;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

/// <summary>
/// S94 K7: contrato HTTP de la edición del catálogo. El controlador es delgado: delega en
/// <see cref="ICatalogoEdicionServiceAsync"/> (doble en memoria) y traduce el resultado (201/200/204/404/409/410/422).
/// </summary>
public sealed class S94_ProductosEdicionControllerTests
{
    private const int Tenant = 4;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private sealed class EdicionFalsa : ICatalogoEdicionServiceAsync
    {
        public Result<CatalogoEdicionResultadoDto> Respuesta { get; set; } =
            Result<CatalogoEdicionResultadoDto>.Success(new CatalogoEdicionResultadoDto(10, 20, 30, 40, "2004000000013", 50));
        public List<(string Op, int Tenant, object Datos)> Llamadas { get; } = [];
        private Task<Result<CatalogoEdicionResultadoDto>> R(string op, int t, object d) { Llamadas.Add((op, t, d)); return Task.FromResult(Respuesta); }
        public Task<Result<CatalogoEdicionResultadoDto>> CrearProductoAsync(int t, NuevoProductoDatos d, CancellationToken ct = default) => R("CrearProducto", t, d);
        public Task<Result<CatalogoEdicionResultadoDto>> CrearVarianteAsync(int t, long idProd, NuevaVarianteDatos d, CancellationToken ct = default) => R("CrearVariante", t, (idProd, d));
        public Task<Result<CatalogoEdicionResultadoDto>> EditarProductoAsync(int t, long idProd, EdicionProductoDatos d, CancellationToken ct = default) => R("EditarProducto", t, (idProd, d));
        public Task<Result<CatalogoEdicionResultadoDto>> EditarVarianteAsync(int t, long idVar, EdicionVarianteDatos d, CancellationToken ct = default) => R("EditarVariante", t, (idVar, d));
        public Task<Result<CatalogoEdicionResultadoDto>> AgregarCodigoAsync(int t, long idVar, NuevoCodigoDatos d, CancellationToken ct = default) => R("AgregarCodigo", t, (idVar, d));
        public Task<Result<CatalogoEdicionResultadoDto>> MarcarPrincipalAsync(int t, long idCodBar, CancellationToken ct = default) => R("MarcarPrincipal", t, idCodBar);
        public Task<Result<CatalogoEdicionResultadoDto>> QuitarCodigoAsync(int t, long idCodBar, CancellationToken ct = default) => R("QuitarCodigo", t, idCodBar);
    }

    private static (ProductosController C, EdicionFalsa Svc) Crear()
    {
        var svc = new EdicionFalsa();
        var c = new ProductosController(null!, null!, null!, svc);
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = Tenant;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return (c, svc);
    }

    private static int Status(IActionResult r) => r switch
    {
        ObjectResult o => o.StatusCode ?? 200,
        StatusCodeResult s => s.StatusCode,
        _ => throw new InvalidOperationException(r.GetType().Name)
    };

    [Fact]
    public async Task CrearProducto_ConVariante_201_YDelegaConElTenantLocal()
    {
        var (c, svc) = Crear();
        var r = await c.Create(new CrearProductoRequest("P1", "Polera", 1, null, 1, new CrearVarianteRequest("4", "Talla 4", 12990m)), Ct);
        var created = Assert.IsType<CreatedAtActionResult>(r);
        Assert.Equal(201, created.StatusCode);
        Assert.Equal(10L, Assert.IsType<CatalogoEdicionResultadoDto>(created.Value).IdProd);
        var (op, t, d) = Assert.Single(svc.Llamadas);
        Assert.Equal(("CrearProducto", Tenant), (op, t));
        var datos = Assert.IsType<NuevoProductoDatos>(d);
        Assert.Equal(("4", 12990m, TipoCodigoAlta.Normal), (datos.Variante.Codigo, datos.Variante.PrecioVenta, datos.Variante.TipoCodigo));
    }

    [Fact]
    public async Task CrearProducto_SinVariante_422_SinLlamarAlServicio()
    {
        var (c, svc) = Crear();
        var r = await c.Create(new CrearProductoRequest("P1", "Polera", 1, null, 1, null), Ct);
        Assert.Equal(422, Status(r));
        Assert.Empty(svc.Llamadas);
    }

    [Theory]
    [InlineData(409, ErrorType.Conflict)]
    [InlineData(404, ErrorType.NotFound)]
    [InlineData(422, ErrorType.Validation)]
    public async Task ErroresDelServicio_SeTraducenAProblemDetails(int http, ErrorType tipo)
    {
        var (c, svc) = Crear();
        svc.Respuesta = Result<CatalogoEdicionResultadoDto>.Failure(new Error(CodigosCatalogo.CodigoBarrasDuplicado, "Ese código de barras ya está en uso.").WithType(tipo).WithHttpStatus(http));
        var r = await c.CreateVariante(10, new CrearVarianteRequest("B", "Talla B", 1000m, "7801234567894"), Ct);
        var o = Assert.IsType<ObjectResult>(r);
        Assert.Equal(http, o.StatusCode);
        Assert.IsAssignableFrom<ProblemDetails>(o.Value);
    }

    [Fact]
    public void Presentaciones_YCodbarPorPresentacion_410()
    {
        var (c, svc) = Crear();
        var p = Assert.IsType<ObjectResult>(c.CreatePresentacion(1));
        Assert.Equal(StatusCodes.Status410Gone, p.StatusCode);
        Assert.Contains("genera el sistema", Assert.IsType<ProblemDetails>(p.Value).Detail);
        var cb = Assert.IsType<ObjectResult>(c.CreateCodBar(1));
        Assert.Equal(StatusCodes.Status410Gone, cb.StatusCode);
        Assert.Empty(svc.Llamadas);
    }

    [Fact]
    public async Task Codigos_Agregar201_Principal200_Quitar204()
    {
        var (c, svc) = Crear();
        Assert.Equal(201, Status(await c.AgregarCodigo(20, new AgregarCodigoRequest("123456789", TipoCodigoAlta.Proveedor, true), Ct)));
        Assert.Equal(200, Status(await c.MarcarPrincipal(40, Ct)));
        Assert.IsType<NoContentResult>(await c.QuitarCodigo(40, Ct));
        Assert.Equal(["AgregarCodigo", "MarcarPrincipal", "QuitarCodigo"], svc.Llamadas.Select(l => l.Op).ToArray());
        var (_, d) = ((long, NuevoCodigoDatos))svc.Llamadas[0].Datos;
        Assert.Equal((TipoCodigoAlta.Proveedor, true), (d.Tipo, d.Principal));
    }

    [Fact]
    public async Task EditarProductoYVariante_200()
    {
        var (c, svc) = Crear();
        Assert.Equal(200, Status(await c.Update(10, new CreateProdDto(1, null, "P1", "Polera Kids", 2), Ct)));
        Assert.Equal(200, Status(await c.UpdateVariante(20, new EditarVarianteRequest("Talla 4 años", false), Ct)));
        var (_, e) = ((long, EdicionProductoDatos))svc.Llamadas[0].Datos;
        Assert.Equal(2, e.IdUniBase);
    }
}
