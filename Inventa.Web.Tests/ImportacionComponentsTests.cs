using System.Net;
using System.Text;
using Bunit;
using Inventa.Web.Models.Importacion;
using Inventa.Web.Models.Productos;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Importacion;
using Inventa.Web.Shared.Importacion;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>S92.2-K5: componentes de S92 (bUnit) + helper EstadoTiendaUi (xUnit).</summary>
public sealed class ImportacionComponentsTests
{
    private readonly BunitContext _ctx = new();
    private const string CambiosJson = """{"disponible":true,"items":[{"fila":2,"codigoProducto":"YERBA-500","codigoVariante":"UN","accion":"Modificado","campos":[{"campo":"Precio","actual":"100","nuevo":"120"}]}],"total":1,"page":1,"pageSize":10}""";

    private sealed class StubHandler : HttpMessageHandler
    {
        public int Hits { get; private set; }
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage req, CancellationToken ct)
        {
            Hits++;
            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            { Content = new StringContent(CambiosJson, Encoding.UTF8, "application/json") });
        }
    }

    public ImportacionComponentsTests()
    {
        _ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        _ctx.Services.AddMudServices();
        var stub = new StubHandler();
        _ctx.Services.AddSingleton(_ => new ApiClient(new HttpClient(stub) { BaseAddress = new Uri("https://localhost:5002/") }));
        _ctx.Services.AddSingleton<ImportacionApiService>();
    }

    [Fact]
    public void TarjetaCambios_MuestraConteos_YPorCampo()
    {
        var resumen = new ResumenCambiosDto
        {
            Nuevos = 2,
            Modificados = 1,
            SinCambios = 3,
            PorCampo = new Dictionary<string, int> { ["Precio"] = 1 }
        };

        var cut = _ctx.Render<TarjetaCambios>(p => p.Add(r => r.Resumen, resumen));

        Assert.Equal("2", cut.Find("[data-testid='cambios-nuevos']").TextContent.Trim());
        Assert.Equal("1", cut.Find("[data-testid='cambios-modificados']").TextContent.Trim());
        Assert.Equal("3", cut.Find("[data-testid='cambios-sin-cambios']").TextContent.Trim());
        Assert.Contains("Precio: 1", cut.Find("[data-testid='cambios-por-campo']").TextContent);
    }

    [Fact]
    public void TablaCambios_CargaPagina_YMuestraFila()
    {
        var resumen = new ResumenCambiosDto { Nuevos = 0, Modificados = 1, SinCambios = 0 };

        var cut = _ctx.Render<TablaCambios>(p => p
            .Add(r => r.LoteId, 7)
            .Add(r => r.Resumen, resumen));

        cut.WaitForAssertion(() => Assert.Contains("YERBA-500", cut.Find("[data-testid='tabla-cambios']").TextContent));
        Assert.Contains("Modificada", cut.Find("[data-testid='tabla-cambios']").TextContent);
    }

    [Theory]
    [InlineData("Publicado", "Publicado", Color.Success)]
    [InlineData("PublicadoConAdvertencia", "Publicado con advertencia", Color.Warning)]
    [InlineData("Listo", "Listo para publicar", Color.Info)]
    [InlineData("Pendiente", "Pendiente", Color.Default)]
    [InlineData("Inactivo", "Inactivo", Color.Error)]
    public void EstadoTiendaUi_TextoYColor_PorEstado(string estado, string texto, Color color)
    {
        Assert.Equal(texto, EstadoTiendaUi.Texto(estado));
        Assert.Equal(color, EstadoTiendaUi.Color(estado));
    }

    [Fact]
    public void EstadoTiendaUi_Desconocido_Passthrough()
    {
        Assert.Equal("Raro", EstadoTiendaUi.Texto("Raro"));
    }
}
