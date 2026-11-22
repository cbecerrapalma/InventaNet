using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using CBP.Acceso;
using CBP.Acceso.Dto;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.WebAPI.Tests;

/// <summary>
/// S100 D2/D6 — La API exige solo permisos de ventana ([RequierePermiso], «cualquiera de estos»), ninguno por entidad,
/// y reenvía el menú que entrega PassPlat. Host real (WebApplicationFactory), sin red ni BD.
/// </summary>
public sealed class S100_PermisosVentanaTests : IClassFixture<S95_AccesoEndpointsTests.Fabrica>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    /// <summary>Los 20 permisos por entidad que S100 retira.</summary>
    private static readonly Regex PorEntidad = new("INVENTA_(PRODUCTO|CLIENTE|PROVEEDOR|BODEGA|STOCK|VENTA|OC|COMPRA|CONFIG)_(VER|CREAR|EDITAR|ANULAR|AJUSTAR|TRANSFERIR|ADMIN|RECIBIR)\\b",
        RegexOptions.CultureInvariant, TimeSpan.FromSeconds(2));

    private readonly S95_AccesoEndpointsTests.Fabrica _f;

    public S100_PermisosVentanaTests(S95_AccesoEndpointsTests.Fabrica f)
    {
        _f = f;
        f.Remoto.Reiniciar();
    }

    private static string RaizDelRepo()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "InventaNet.slnx"))) return dir.FullName;
        throw new InvalidOperationException("No se encontró InventaNet.slnx subiendo desde " + AppContext.BaseDirectory);
    }

    private static IEnumerable<string> Fuentes(string proyecto) =>
        Directory.EnumerateFiles(Path.Combine(RaizDelRepo(), proyecto), "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(p => p is "obj" or "bin"));

    [Fact]
    public void Ningun_permiso_por_entidad_queda_en_el_codigo()
    {
        var restos = new List<string>();
        foreach (var proyecto in new[] { "Inventa.Web", "Inventa.WebAPI", "Inventa.Application.Async" })
            foreach (var archivo in Fuentes(proyecto))
            {
                var lineas = File.ReadAllLines(archivo);
                for (var i = 0; i < lineas.Length; i++)
                    if (PorEntidad.IsMatch(lineas[i])) restos.Add($"{Path.GetRelativePath(RaizDelRepo(), archivo)}:{i + 1}");
            }

        Assert.True(restos.Count == 0, "Permisos por entidad que siguen en el código:\n" + string.Join("\n", restos));
        var manifiesto = _f.Services.GetRequiredService<ManifiestoAcceso>();
        Assert.DoesNotContain(manifiesto.PermisosCatalogo.Concat(manifiesto.PermisosDetectados), p => PorEntidad.IsMatch(p.Codigo));
        Assert.DoesNotContain(manifiesto.Areas.SelectMany(a => a.Modulos).SelectMany(v => v.Permisos), p => PorEntidad.IsMatch(p.Codigo));
    }

    [Fact]
    public void Todo_permiso_exigido_es_de_una_ventana_del_catalogo()
    {
        var manifiesto = _f.Services.GetRequiredService<ManifiestoAcceso>();
        var exigidos = DescubridorPoliticas.Descubrir(typeof(Program).Assembly, "INVENTA_");
        Assert.NotEmpty(exigidos);

        // S103: cada código es un permiso que PassPlat tendrá activo: el VER de una ventana o una acción que el catálogo declara.
        var validos = CatalogoAcceso.Desde(manifiesto).Modulos.Where(m => !string.IsNullOrEmpty(m.Ruta))
            .SelectMany(v => v.Acciones.Append("VER").Select(a => ReglasAcceso.CodigoPermisoVentana(v.Codigo, a)))
            .ToHashSet(StringComparer.Ordinal);
        var fuera = exigidos.Where(c => !validos.Contains(c)).ToList();
        Assert.True(fuera.Count == 0, "Permisos exigidos que no son de una ventana del catálogo: " + string.Join(", ", fuera));

        // Y el catálogo que se le entrega a PassPlat no pide ninguno suelto por entidad.
        Assert.DoesNotContain(CatalogoAcceso.Desde(manifiesto).Permisos, p => PorEntidad.IsMatch(p.Codigo));
    }

    [Fact]
    public void Ningun_controlador_usa_Authorize_Policy_INVENTA()
    {
        var conPolitica = new List<string>();
        foreach (var tipo in typeof(Program).Assembly.GetTypes().Where(t => typeof(ControllerBase).IsAssignableFrom(t)))
        {
            var atributos = tipo.GetCustomAttributes<AuthorizeAttribute>(inherit: true)
                .Concat(tipo.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).SelectMany(m => m.GetCustomAttributes<AuthorizeAttribute>(inherit: true)));
            conPolitica.AddRange(atributos.Where(a => a is not RequierePermisoAttribute && a.Policy?.StartsWith("INVENTA_", StringComparison.Ordinal) == true)
                .Select(a => $"{tipo.Name}: {a.Policy}"));
        }
        Assert.Empty(conPolitica);

        var texto = Fuentes("Inventa.WebAPI").Where(f => f.Contains("Controllers", StringComparison.Ordinal))
            .Where(f => Regex.IsMatch(File.ReadAllText(f), "Authorize\\(Policy\\s*=\\s*\"INVENTA_")).ToList();
        Assert.Empty(texto);
    }

    private sealed class ProveedorMenuFalso(Func<IReadOnlyList<ItemMenuDto>> resultado) : IProveedorMenuRemoto
    {
        public Task<IReadOnlyList<ItemMenuDto>> ObtenerMenuAsync(CancellationToken ct = default) => Task.FromResult(resultado());
    }

    private HttpClient ClienteConMenu(Func<IReadOnlyList<ItemMenuDto>> resultado) =>
        _f.WithWebHostBuilder(b => b.ConfigureTestServices(s => s.AddScoped<IProveedorMenuRemoto>(_ => new ProveedorMenuFalso(resultado)))).CreateClient();

    private static async Task<HttpResponseMessage> Menu(HttpClient cliente, bool autenticado = true)
    {
        using var req = new HttpRequestMessage(HttpMethod.Get, "/api/acceso/menu");
        if (autenticado) req.Headers.Add("X-Prueba-Permisos", "INVENTA_PRODUCTOS_PRINCIPAL_VER");
        return await cliente.SendAsync(req, Ct);
    }

    [Fact]
    public async Task Menu_reenvia_el_arbol_del_proveedor_y_traduce_403()
    {
        ItemMenuDto[] arbol =
        [
            new() { Codigo = "INVENTA", Nombre = "INVENTA", EsVisibleMenu = true, Hijos =
                [new() { Codigo = "INVENTA.INICIO", Nombre = "Inicio", Ruta = "/", Icono = "Dashboard", Orden = 1, EsVisibleMenu = true }] }
        ];
        using (var ok = await Menu(ClienteConMenu(() => arbol)))
        {
            Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
            var recibido = (await ok.Content.ReadFromJsonAsync<List<ItemMenuDto>>(Json, Ct))!;
            Assert.Equal("INVENTA", Assert.Single(recibido).Codigo);
            Assert.Equal("/", Assert.Single(recibido[0].Hijos).Ruta);
        }

        using (var sinSesion = await Menu(ClienteConMenu(() => arbol), autenticado: false))
            Assert.Equal(HttpStatusCode.Unauthorized, sinSesion.StatusCode);

        // 401/403 del proveedor → 403 ProblemDetails; cualquier otro estado del proveedor → 502.
        foreach (var (estadoProveedor, esperado) in new[] { (401, HttpStatusCode.Forbidden), (403, HttpStatusCode.Forbidden), (503, HttpStatusCode.BadGateway) })
        {
            using var r = await Menu(ClienteConMenu(() => throw new ErrorProveedorAccesoException("PassPlat rechazó el menú", estadoProveedor)));
            Assert.Equal(esperado, r.StatusCode);
            var problema = await r.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
            Assert.Equal("ACCESO_PROVEEDOR", problema.GetProperty("errorCode").GetString());
        }
    }
}
