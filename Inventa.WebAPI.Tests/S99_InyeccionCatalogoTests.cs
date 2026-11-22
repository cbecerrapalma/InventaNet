using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.RegularExpressions;
using CBP.Acceso;
using CBP.Acceso.Dto;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.WebAPI.Tests;

/// <summary>
/// S99 K5 — Detección autónoma de los permisos de INVENTA y contrato de inyección del catálogo. El manifiesto es el que la API
/// registra en DI (generado al compilar: páginas, menú, componentes, servicios y controladores) más las políticas de la API.
/// </summary>
public sealed class S99_InyeccionCatalogoTests : IClassFixture<S95_AccesoEndpointsTests.Fabrica>
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private static readonly string[] Crud = ["VER", "CREAR", "EDITAR", "ELIMINAR"];
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private readonly S95_AccesoEndpointsTests.Fabrica _f;

    public S99_InyeccionCatalogoTests(S95_AccesoEndpointsTests.Fabrica f)
    {
        _f = f;
        f.Remoto.Reiniciar();
    }

    private ManifiestoAcceso Manifiesto => _f.Services.GetRequiredService<ManifiestoAcceso>();

    private static string RaizDelRepo()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            if (File.Exists(Path.Combine(dir.FullName, "InventaNet.slnx"))) return dir.FullName;
        throw new InvalidOperationException("No se encontró InventaNet.slnx subiendo desde " + AppContext.BaseDirectory);
    }

    /// <summary>Permisos que PassPlat tendría con el catálogo: los sueltos más el VER y las acciones declaradas de cada ventana (S103).</summary>
    private static HashSet<string> CodigosDelCatalogo(CatalogoAccesoDto catalogo)
    {
        var codigos = catalogo.Permisos.Select(p => p.Codigo).ToHashSet(StringComparer.Ordinal);
        foreach (var v in catalogo.Modulos.Where(m => !string.IsNullOrEmpty(m.Ruta)))
            foreach (var accion in v.Acciones.Append("VER"))
                codigos.Add(ReglasAcceso.CodigoPermisoVentana(v.Codigo, accion));
        return codigos;
    }

    private static IEnumerable<string> ArchivosDeCodigo(string raiz, string proyecto) =>
        Directory.EnumerateFiles(Path.Combine(raiz, proyecto), "*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || f.EndsWith(".razor", StringComparison.OrdinalIgnoreCase))
            .Where(f => !f.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar).Any(p => p is "obj" or "bin"))
            .OrderBy(f => f, StringComparer.Ordinal);

    [Fact]
    public void Todo_literal_de_permiso_del_codigo_esta_en_el_catalogo()
    {
        var raiz = RaizDelRepo();
        var catalogo = CodigosDelCatalogo(CatalogoAcceso.Desde(Manifiesto));
        var literal = new Regex("\"(?<v>INVENTA_[A-Z0-9_]+)\"", RegexOptions.CultureInvariant);
        // «Miembro = "…"», «const string Miembro = "…"» y el inicializador de propiedad «Miembro { get; set; } = "…"».
        var asignacion = new Regex(@"(?<miembro>\w+)\s*(?:\{[^}]*\}\s*)?=\s*$", RegexOptions.CultureInvariant);

        var faltan = new List<string>();
        var revisados = 0;
        foreach (var proyecto in new[] { "Inventa.Web", "Inventa.WebAPI", "Inventa.Application.Async" })
            foreach (var archivo in ArchivosDeCodigo(raiz, proyecto))
            {
                var lineas = File.ReadAllLines(archivo);
                for (var i = 0; i < lineas.Length; i++)
                    foreach (Match m in literal.Matches(lineas[i]))
                    {
                        // Un literal asignado a un miembro «…App…» (AppCodigo = "INVENTA_STORE") es el código de una app, no un permiso.
                        var previo = asignacion.Match(lineas[i][..m.Index]);
                        if (previo.Success && previo.Groups["miembro"].Value.Contains("App", StringComparison.Ordinal)) continue;
                        revisados++;
                        if (!catalogo.Contains(m.Groups["v"].Value))
                            faltan.Add($"{Path.GetRelativePath(raiz, archivo)}:{i + 1} {m.Groups["v"].Value}");
                    }
            }

        Assert.True(revisados > 100, $"Se esperaban cientos de literales y se revisaron {revisados}: ¿cambió la ruta del repo?");
        Assert.True(faltan.Count == 0, "Literales de permiso que el catálogo de INVENTA no pide a PassPlat:\n" + string.Join("\n", faltan));
    }

    [Fact]
    public void Permisos_de_servicios_y_componentes_se_detectan_con_su_origen()
    {
        var raiz = RaizDelRepo();
        var detectados = Manifiesto.PermisosDetectados.ToDictionary(p => p.Codigo, StringComparer.Ordinal);

        // El componente ProductImagesSection.razor (Shared/Product) y el servicio DashboardServiceAsync.cs (Application.Async) exigen
        // estos permisos; las páginas y los controladores no son los primeros hallazgos (el origen es el primero en orden de ruta).
        var esperados = new[]
        {
            (Codigo: "INVENTA_PRODUCTOS_DETALLE_EDITAR", Origen: "Shared/Product/ProductImagesSection.razor", Archivo: Path.Combine("Inventa.Web", "Shared", "Product", "ProductImagesSection.razor")),
            (Codigo: "INVENTA_CLIENTES_PRINCIPAL_VER", Origen: "Application.Async/Services/DashboardServiceAsync.cs", Archivo: Path.Combine("Inventa.Application.Async", "Services", "DashboardServiceAsync.cs")),
        };
        foreach (var (codigo, origen, archivo) in esperados)
        {
            Assert.True(detectados.TryGetValue(codigo, out var permiso), $"{codigo} no se detectó");
            var partes = permiso!.Origen!.Split(':');
            Assert.Equal(origen, partes[0]);
            Assert.Equal(permiso.Codigo, codigo);
            var lineas = File.ReadAllLines(Path.Combine(raiz, archivo));
            Assert.Contains("\"" + codigo + "\"", lineas[int.Parse(partes[1]) - 1]); // el origen apunta a la línea que lo exige
        }

        // Todo lo detectado trae origen «ruta:línea», nombre y módulo del catálogo.
        Assert.NotEmpty(detectados);
        Assert.All(detectados.Values, p =>
        {
            Assert.Matches(@"^.+:\d+$", p.Origen ?? "");
            Assert.NotEmpty(p.Nombre);
            Assert.StartsWith("INVENTA", p.CodigoModulo, StringComparison.Ordinal);
        });
    }

    [Fact]
    public void Catalogo_de_Inventa_pide_31_ventanas_y_6_acciones_no_CRUD()
    {
        var catalogo = CatalogoAcceso.Desde(Manifiesto);

        var ventanas = catalogo.Modulos.Where(m => !string.IsNullOrEmpty(m.Ruta)).ToList();
        Assert.Equal(31, ventanas.Count);
        Assert.Equal(6, ventanas.SelectMany(v => v.Acciones).Count(a => !Crud.Contains(a)));
        Assert.Equal(["ADMIN", "ADMIN", "AJUSTAR", "ANULAR", "RECIBIR", "TRANSFERIR"], ventanas.SelectMany(v => v.Acciones).Where(a => !Crud.Contains(a)).OrderBy(a => a, StringComparer.Ordinal));
        // S103: lo que PassPlat crea por ventana son las acciones que el código exige (antes 31 × 4 + 6 = 130).
        Assert.Equal(58, CodigosDelCatalogo(catalogo).Count - catalogo.Permisos.Count);
        Assert.Equal(9, ventanas.Count(v => v.Acciones.Contains("CREAR")));
        Assert.Equal(12, ventanas.Count(v => v.Acciones.Contains("EDITAR")));
        Assert.Equal(0, ventanas.Count(v => v.Acciones.Contains("ELIMINAR")));
        Assert.All(catalogo.Modulos.Where(m => string.IsNullOrEmpty(m.Ruta)), m => Assert.Empty(m.Acciones)); // raíz y áreas sin CRUD
        Assert.Equal("INVENTA", catalogo.Modulos[0].Codigo);
        Assert.Null(catalogo.Modulos[0].CodigoPadre);
    }

    [Fact]
    public async Task Publicar_con_conflicto_del_proveedor_devuelve_409_con_plan_nuevo()
    {
        var plan = await PostAsync<PlanPublicacionDto>("/api/admin/acceso/plan", null);
        _f.Remoto.ConflictoAlInyectar = true; // entre el plan revisado y la inyección, alguien más cambia PassPlat

        var r = await Post("/api/admin/acceso/publicar", new { plan.HuellaRemota });

        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        var cuerpo = await r.Content.ReadFromJsonAsync<JsonElement>(Json, Ct);
        Assert.Equal("ACCESO_ESTADO_REMOTO_CAMBIO", cuerpo.GetProperty("errorCode").GetString());
        var nuevo = cuerpo.GetProperty("plan").Deserialize<PlanPublicacionDto>(Json)!;
        Assert.NotEqual(plan.HuellaRemota, nuevo.HuellaRemota);
        Assert.Contains("INVENTA.EXTRA", nuevo.Sobrantes);
        Assert.Equal(plan.TotalOperaciones, nuevo.TotalOperaciones); // el plan nuevo se calculó contra el estado nuevo
        Assert.Equal(0, _f.Remoto.Escrituras); // el conflicto no escribe nada
    }

    // ---------------- infraestructura ----------------

    private async Task<HttpResponseMessage> Post(string ruta, object? cuerpo)
    {
        var client = _f.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, ruta);
        req.Headers.Add("X-Prueba-Permisos", "MAESTRO");
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo);
        return await client.SendAsync(req, Ct);
    }

    private async Task<T> PostAsync<T>(string ruta, object? cuerpo)
    {
        var r = await Post(ruta, cuerpo);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return (await r.Content.ReadFromJsonAsync<T>(Json, Ct))!;
    }
}
