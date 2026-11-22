using CBP.Results;
using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Inventa.WebAPI.Controllers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace Inventa.WebAPI.Tests;

/// <summary>
/// S93.1 C4/C9: contrato HTTP de «Precios y ofertas». Usa el servicio real de escritura con un repositorio
/// falso en memoria (registra operaciones; no hay BD): 200/409/422, el POST antiguo con validación de
/// tenant (H5) y el límite del lote.
/// </summary>
public sealed class S931_PreciosControllerTests
{
    private const int Tenant = 4;
    private static readonly DateOnly Hoy = PublicCatalogSpecs.Hoy();

    private sealed class RepoFalso : IPreciosGrillaRepositoryAsync
    {
        public Dictionary<long, SnapshotPrecios> Snaps { get; } = [];
        public List<OperacionPrecio> Aplicadas { get; } = [];

        public Task<IReadOnlyDictionary<long, SnapshotPrecios>> GetSnapshotsAsync(IReadOnlyCollection<long> ids, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyDictionary<long, SnapshotPrecios>>(Snaps.Where(kv => ids.Contains(kv.Key)).ToDictionary());

        public async Task<(TResult Resultado, IReadOnlyDictionary<int, long> PreciosNuevos)> EjecutarAsync<TResult>(IReadOnlyCollection<long> ids,
            Func<IReadOnlyDictionary<long, SnapshotPrecios>, (IReadOnlyList<OperacionPrecio>? Ops, TResult Resultado)> planificar, CancellationToken ct = default)
        {
            var (ops, r) = planificar(await GetSnapshotsAsync(ids, ct));
            if (ops is null) return (r, new Dictionary<int, long>());
            Aplicadas.AddRange(ops);
            return (r, ops.OfType<OpInsertarPrecio>().ToDictionary(o => o.Clave, o => 1000L + o.Clave));
        }

        public Task<(IReadOnlyList<PrecioGrillaRow> Items, int Total)> GetPaginaAsync(int idTenant, PreciosGrillaFiltro filtro, DateOnly hoy, int page, int pageSize, CancellationToken ct = default) =>
            Task.FromResult<(IReadOnlyList<PrecioGrillaRow>, int)>(([new PrecioGrillaRow { IdPresProd = 1, Producto = filtro.Texto ?? "", PrecioValor = 1000, IdPrecio = 1, PrecioDesde = hoy }], 1));
        public Task<PreciosResumenRow> GetResumenAsync(int idTenant, DateOnly hoy, CancellationToken ct = default) => Task.FromResult(new PreciosResumenRow(1, 0, 0, 0, 0, 0));
        public Task<IReadOnlyList<PrecioGrillaRow>> GetPorIdsAsync(int idTenant, IReadOnlyCollection<long> ids, DateOnly hoy, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<PrecioGrillaRow>>(ids.Select(i => new PrecioGrillaRow { IdPresProd = i }).ToList());
        public Task<PreciosHistorial?> GetHistorialAsync(int idTenant, long idPresProd, DateOnly hoy, CancellationToken ct = default) => Task.FromResult<PreciosHistorial?>(null);
    }

    private sealed class ConfigFalsa : IConfiguracionServiceAsync
    {
        public Task<Result<TenantConfigInventa?>> GetTenantConfigAsync(int idTenant, CancellationToken ct = default) =>
            Task.FromResult(Result<TenantConfigInventa?>.Success(new TenantConfigInventa { IdTenant = idTenant, Moneda = "CLP" }, allowNull: true));
        public Task<Result<Configuracion?>> GetAsync(CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<Configuracion>> UpdateAsync(Configuracion cfg, CancellationToken ct = default) => throw new NotImplementedException();
        public Task<Result<TenantConfigInventa>> UpsertTenantConfigAsync(TenantConfigInventa cfg, CancellationToken ct = default) => throw new NotImplementedException();
    }

    private static (PreciosController C, RepoFalso Repo) Nuevo()
    {
        var repo = new RepoFalso();
        repo.Snaps[10] = new SnapshotPrecios(10, Tenant, "V-10", "Prod / Var",
            [new Precio { IdPrecio = 1, IdPresProd = 10, Valor = 1000, FecVig = Hoy.AddDays(-5), Activo = true }], []);
        repo.Snaps[20] = new SnapshotPrecios(20, 5, "V-20", "Otro tenant",
            [new Precio { IdPrecio = 2, IdPresProd = 20, Valor = 1000, FecVig = Hoy.AddDays(-5), Activo = true }], []);
        var grilla = new PrecioGrillaServiceAsync(repo, new ConfigFalsa());
        var c = new PreciosController(null!, grilla, new GestionPreciosServiceAsync(repo, grilla));
        var http = new DefaultHttpContext();
        http.Items["IdTenantLocal"] = Tenant;
        c.ControllerContext = new ControllerContext { HttpContext = http };
        return (c, repo);
    }

    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Cambios_Validos_200_YAplica()
    {
        var (c, repo) = Nuevo();
        var r = await c.PostCambios(new([new CambioPrecio(10, TipoCambioPrecio.PrecioOficial, Valor: 1200, IdPrecioVigenteVisto: 1)]), simular: false, Ct);
        var ok = Assert.IsType<OkObjectResult>(r);
        Assert.True(Assert.IsType<ResultadoCambiosPreciosDto>(ok.Value).Aplicado);
        Assert.Single(repo.Aplicadas.OfType<OpInsertarPrecio>());
    }

    [Fact]
    public async Task Cambios_Simular_NoAplica()
    {
        var (c, repo) = Nuevo();
        var r = await c.PostCambios(new([new CambioPrecio(10, TipoCambioPrecio.PrecioOficial, Valor: 1200, IdPrecioVigenteVisto: 1)]), simular: true, Ct);
        var dto = Assert.IsType<ResultadoCambiosPreciosDto>(Assert.IsType<OkObjectResult>(r).Value);
        Assert.False(dto.Aplicado);
        Assert.Single(dto.Presentaciones);
        Assert.Empty(repo.Aplicadas);
    }

    [Fact]
    public async Task Cambios_ConError_422_YNoAplicaNada()
    {
        var (c, repo) = Nuevo();
        var r = await c.PostCambios(new([new CambioPrecio(10, TipoCambioPrecio.PrecioOficial, Valor: 1200.5m, IdPrecioVigenteVisto: 1)]), simular: false, Ct);
        var obj = Assert.IsType<ObjectResult>(r);
        Assert.Equal(422, obj.StatusCode);
        var errores = Assert.IsAssignableFrom<IEnumerable<ErrorCambioPrecio>>(Assert.IsType<ProblemDetails>(obj.Value).Extensions["errores"]);
        Assert.Equal(CodigosPrecio.PrecioInvalido, Assert.Single(errores).Codigo);
        Assert.Empty(repo.Aplicadas);
    }

    [Fact]
    public async Task Cambios_Desactualizado_409()
    {
        var (c, repo) = Nuevo();
        var r = await c.PostCambios(new([new CambioPrecio(10, TipoCambioPrecio.PrecioOficial, Valor: 1200, IdPrecioVigenteVisto: 99)]), simular: false, Ct);
        var obj = Assert.IsType<ObjectResult>(r);
        Assert.Equal(409, obj.StatusCode);
        Assert.Equal(CodigosPrecio.PrecioDesactualizado, Assert.IsType<ProblemDetails>(obj.Value).Extensions["code"]);
        Assert.Empty(repo.Aplicadas);
    }

    [Fact]
    public async Task Cambios_LoteDemasiadoGrande_422()
    {
        var (c, _) = Nuevo();
        var lote = Enumerable.Range(0, CodigosPrecio.MaxCambios + 1)
            .Select(_ => new CambioPrecio(10, TipoCambioPrecio.PrecioOficial, Valor: 1200, IdPrecioVigenteVisto: 1)).ToList();
        var r = await c.PostCambios(new(lote), simular: false, Ct);
        Assert.Equal(422, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task PostAntiguo_OtroTenant_422_SinEscribir()
    {
        var (c, repo) = Nuevo();
        var r = await c.UpsertPrecio(new UpsertPrecioDto(20, 1500, Hoy), Ct);
        var obj = Assert.IsType<ObjectResult>(r);
        Assert.Equal(422, obj.StatusCode);
        var errores = Assert.IsAssignableFrom<IEnumerable<ErrorCambioPrecio>>(Assert.IsType<ProblemDetails>(obj.Value).Extensions["errores"]);
        Assert.Equal(CodigosPrecio.PresentacionNoEncontrada, Assert.Single(errores).Codigo);
        Assert.Empty(repo.Aplicadas);
    }

    [Fact]
    public async Task PostAntiguo_MismoTenant_201_ConElIdCreado()
    {
        var (c, _) = Nuevo();
        var r = await c.UpsertPrecio(new UpsertPrecioDto(10, 1500, Hoy), Ct);
        var created = Assert.IsType<CreatedAtActionResult>(r);
        Assert.Equal(1001L, Assert.IsType<Precio>(created.Value).IdPrecio);
    }

    [Fact]
    public async Task PostAntiguo_FechaPasada_422()
    {
        var (c, _) = Nuevo();
        var r = await c.UpsertPrecio(new UpsertPrecioDto(10, 1500, Hoy.AddDays(-1)), Ct);
        Assert.Equal(422, Assert.IsType<ObjectResult>(r).StatusCode);
    }

    [Fact]
    public async Task Grilla_200_ConMonedaYDecimales()
    {
        var (c, _) = Nuevo();
        var r = await c.GetGrilla("abc", null, null, EstadoFiltroPrecios.Todos, false, 1, 20, Ct);
        var pagina = Assert.IsType<PaginaPreciosDto>(Assert.IsType<OkObjectResult>(r).Value);
        Assert.Equal(("CLP", 0, "abc"), (pagina.Moneda, pagina.Decimales, pagina.Items[0].Producto));
    }

    [Fact]
    public void PreciosCompra_Retirados_410()
    {
        var (c, _) = Nuevo();
        Assert.Equal(410, Assert.IsType<ObjectResult>(c.GetPreciosCompra()).StatusCode);
        Assert.Equal(410, Assert.IsType<ObjectResult>(c.UpsertPrecioCompra()).StatusCode);
    }

    [Fact]
    public async Task Historial_Inexistente_404()
    {
        var (c, _) = Nuevo();
        Assert.IsType<NotFoundResult>(await c.GetHistorial(10, Ct));
    }
}
