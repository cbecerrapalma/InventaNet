using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using CBP.Acceso;
using CBP.Acceso.Dto;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Xunit;

namespace Inventa.WebAPI.Tests;

/// <summary>
/// S95 K4 / S99 K5 — Endpoints api/admin/acceso en el host real (WebApplicationFactory), con un inyector de catálogo falso con el
/// estado verificado de PassPlat (H5/H6: raíz INVENTA + 9 áreas, 20 permisos, 0 asociaciones). Sin red, sin BD.
/// </summary>
public sealed class S95_AccesoEndpointsTests : IClassFixture<S95_AccesoEndpointsTests.Fabrica>
{
    /// <summary>Mismo literal que Inventa.Web.Tests/AccesoGeneradoTests.HuellaEsperada (D1).</summary>
    private const string HuellaEsperada = "9D9D5BBE00109290B3495E7D738C5A57C594C5CE9FFF89DFDF52CFB43E90F5AE";

    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web);
    private const string Maestro = "MAESTRO";
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private readonly Fabrica _f;

    public S95_AccesoEndpointsTests(Fabrica f)
    {
        _f = f;
        f.Remoto.Reiniciar();
    }

    // ---------------- infraestructura ----------------

    public sealed class Fabrica : WebApplicationFactory<Program>
    {
        public ProveedorFalso Remoto { get; } = new();

        protected override void ConfigureWebHost(IWebHostBuilder builder) =>
            builder.ConfigureTestServices(s =>
            {
                foreach (var d in s.Where(d => d.ServiceType == typeof(IHostedService)).ToList()) s.Remove(d);
                s.AddScoped<IInyectorCatalogoRemoto>(_ => Remoto);
                s.AddAuthentication(EsquemaPrueba.Nombre).AddScheme<AuthenticationSchemeOptions, EsquemaPrueba>(EsquemaPrueba.Nombre, _ => { });
                s.PostConfigure<AuthenticationOptions>(o =>
                {
                    o.DefaultAuthenticateScheme = EsquemaPrueba.Nombre;
                    o.DefaultChallengeScheme = EsquemaPrueba.Nombre;
                    o.DefaultForbidScheme = EsquemaPrueba.Nombre;
                    o.DefaultScheme = EsquemaPrueba.Nombre;
                });
            });
    }

    /// <summary>
    /// Autenticación de prueba: el encabezado X-Prueba-Permisos lleva los permisos (claim "permissions").
    /// El valor especial MAESTRO agrega el claim is_system=true (usuario maestro de PassPlat).
    /// </summary>
    private sealed class EsquemaPrueba(IOptionsMonitor<AuthenticationSchemeOptions> o, ILoggerFactory l, UrlEncoder e)
        : AuthenticationHandler<AuthenticationSchemeOptions>(o, l, e)
    {
        public const string Nombre = "Prueba";

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.TryGetValue("X-Prueba-Permisos", out var permisos)) return Task.FromResult(AuthenticateResult.NoResult());
            var lista = permisos.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries).Select(p => p.Trim()).ToList();
            var claims = lista.Where(p => p != Maestro).Select(p => new Claim("permissions", p))
                .Append(new Claim("sub", "3"))
                .Concat(lista.Contains(Maestro) ? [new Claim("is_system", "true")] : []);
            var id = new ClaimsIdentity(claims, Nombre);
            return Task.FromResult(AuthenticateResult.Success(new AuthenticationTicket(new ClaimsPrincipal(id), Nombre)));
        }
    }

    /// <summary>
    /// PassPlat simulado con el estado verificado (raíz INVENTA + 9 áreas, 20 permisos, 0 asociaciones): calcula el plan del contrato
    /// con las reglas de PassPlat (VER y acciones declaradas por ventana, mover, sobrantes) y lo aplica por completo o no aplica nada.
    /// </summary>
    public sealed class ProveedorFalso : IInyectorCatalogoRemoto
    {
        private static readonly string[] Fijas = ["VER"]; // S103: PassPlat crea siempre solo VER; lo demás, las acciones que el catálogo declara
        private Dictionary<string, (string? Padre, bool Activo)> _modulos = new();
        private Dictionary<string, string> _permisos = new();
        private HashSet<string> _asociados = new();
        public int Escrituras { get; private set; }

        /// <summary>true = en el próximo InyectarAsync alguien más cambia el estado de PassPlat: responde 409 sin escribir.</summary>
        public bool ConflictoAlInyectar { get; set; }

        public void Reiniciar()
        {
            Escrituras = 0;
            ConflictoAlInyectar = false;
            string[] areas = ["PRODUCTOS", "VENTAS", "INVENTARIO", "BODEGAS", "COMPRAS", "PRECIOS", "CLIENTES", "PROVEEDORES", "CONFIG"];
            (string Codigo, string Modulo)[] permisos =
            [
                ("INVENTA_PRODUCTO_VER", "PRODUCTOS"), ("INVENTA_PRODUCTO_CREAR", "PRODUCTOS"), ("INVENTA_PRODUCTO_EDITAR", "PRODUCTOS"),
                ("INVENTA_VENTA_VER", "VENTAS"), ("INVENTA_VENTA_CREAR", "VENTAS"), ("INVENTA_VENTA_ANULAR", "VENTAS"),
                ("INVENTA_STOCK_VER", "INVENTARIO"), ("INVENTA_STOCK_AJUSTAR", "INVENTARIO"), ("INVENTA_STOCK_TRANSFERIR", "INVENTARIO"),
                ("INVENTA_BODEGA_VER", "BODEGAS"), ("INVENTA_BODEGA_ADMIN", "BODEGAS"),
                ("INVENTA_OC_VER", "COMPRAS"), ("INVENTA_OC_CREAR", "COMPRAS"), ("INVENTA_COMPRA_RECIBIR", "COMPRAS"),
                ("INVENTA_CONFIG_VER", "CONFIG"), ("INVENTA_CONFIG_EDITAR", "CONFIG"),
                ("INVENTA_CLIENTE_VER", "CLIENTES"), ("INVENTA_CLIENTE_EDITAR", "CLIENTES"),
                ("INVENTA_PROVEEDOR_VER", "PROVEEDORES"), ("INVENTA_PROVEEDOR_EDITAR", "PROVEEDORES"),
            ];
            _modulos = new() { ["INVENTA"] = (null, true) };
            foreach (var a in areas) _modulos["INVENTA." + a] = ("INVENTA", true);
            _permisos = permisos.ToDictionary(p => p.Codigo, p => "INVENTA." + p.Modulo);
            _asociados = new();
        }

        private string Huella() => ReglasAcceso.Huella(
            _modulos.Select(m => $"M|{m.Key}|{m.Value.Padre}|{m.Value.Activo}")
                .Concat(_permisos.Select(p => $"P|{p.Key}|{p.Value}"))
                .Concat(_asociados.Select(a => "S|" + a)));

        /// <summary>El plan que PassPlat calcula para el catálogo del manifiesto (CatalogoAcceso.Desde) contra su estado actual.</summary>
        private PlanPublicacionDto Calcular(ManifiestoAcceso manifiesto)
        {
            var catalogo = CatalogoAcceso.Desde(manifiesto);
            var plan = new PlanPublicacionDto { CodigoApp = catalogo.CodigoApp, HuellaManifiesto = manifiesto.Huella, HuellaRemota = Huella() };
            foreach (var m in catalogo.Modulos.Where(m => !_modulos.ContainsKey(m.Codigo)))
                plan.Operaciones.Add(new OperacionAcceso { Tipo = TipoOperacionAcceso.CrearModulo, Codigo = m.Codigo, Modulo = m });
            foreach (var m in catalogo.Modulos.Where(m => !_asociados.Contains(m.Codigo)))
                plan.Operaciones.Add(new OperacionAcceso { Tipo = TipoOperacionAcceso.AsociarModulo, Codigo = m.Codigo, Modulo = m });

            // VER y acciones declaradas de cada ventana (con ruta) + permisos sueltos.
            var deseados = catalogo.Modulos.Where(m => !string.IsNullOrEmpty(m.Ruta))
                .SelectMany(v => Fijas.Concat(v.Acciones).Distinct().Select(a => (Codigo: ReglasAcceso.CodigoPermisoVentana(v.Codigo, a), Modulo: v.Codigo)))
                .Concat(catalogo.Permisos.Select(p => (p.Codigo, Modulo: p.CodigoModulo)))
                .OrderBy(p => p.Codigo, StringComparer.Ordinal).ToList();
            foreach (var p in deseados.Where(p => !_permisos.ContainsKey(p.Codigo)))
                plan.Operaciones.Add(new OperacionAcceso
                {
                    Tipo = TipoOperacionAcceso.CrearPermiso, Codigo = p.Codigo,
                    Permiso = new PermisoDto { Codigo = p.Codigo, Nombre = p.Codigo, CodigoModulo = p.Modulo }
                });
            foreach (var p in deseados.Where(p => _permisos.TryGetValue(p.Codigo, out var actual) && actual != p.Modulo))
                plan.Operaciones.Add(new OperacionAcceso
                {
                    Tipo = TipoOperacionAcceso.MoverPermiso, Codigo = p.Codigo,
                    Permiso = new PermisoDto { Codigo = p.Codigo, Nombre = p.Codigo, CodigoModulo = p.Modulo }
                });
            var codigos = catalogo.Modulos.Select(m => m.Codigo).ToHashSet();
            // S100 D5b: sobrantes = módulos activos que ya no están y, después, permisos activos que ya no se desean.
            var codigosPermisos = deseados.Select(p => p.Codigo).ToHashSet(StringComparer.Ordinal);
            plan.Sobrantes = _modulos.Where(m => m.Value.Activo && !codigos.Contains(m.Key)).Select(m => m.Key).OrderBy(c => c, StringComparer.Ordinal)
                .Concat(_permisos.Keys.Where(c => !codigosPermisos.Contains(c)).OrderBy(c => c, StringComparer.Ordinal)).ToList();
            return plan;
        }

        public Task<PlanPublicacionDto> SimularAsync(ManifiestoAcceso manifiesto, CancellationToken ct = default) => Task.FromResult(Calcular(manifiesto));

        public Task<ResultadoPublicacionDto> InyectarAsync(ManifiestoAcceso manifiesto, string huellaRemota, IReadOnlyCollection<string> desactivarSobrantes, CancellationToken ct = default)
        {
            if (ConflictoAlInyectar)
            {
                // Entre la simulación y la inyección alguien más cambió PassPlat: no se escribe nada.
                ConflictoAlInyectar = false;
                _modulos["INVENTA.EXTRA"] = ("INVENTA", true);
                throw new ErrorProveedorAccesoException("El estado de PassPlat cambió desde que se generó el plan.", 409);
            }
            var plan = Calcular(manifiesto);
            if (!string.Equals(huellaRemota, plan.HuellaRemota, StringComparison.Ordinal))
                throw new ErrorProveedorAccesoException("El estado de PassPlat cambió desde que se generó el plan.", 409);

            var resultado = new ResultadoPublicacionDto();
            foreach (var op in plan.Operaciones)
            {
                Escrituras++;
                resultado.Aplicadas.Add(op);
                switch (op.Tipo)
                {
                    case TipoOperacionAcceso.CrearModulo: _modulos[op.Codigo] = (op.Modulo!.CodigoPadre, true); break;
                    case TipoOperacionAcceso.AsociarModulo: _asociados.Add(op.Codigo); break;
                    case TipoOperacionAcceso.CrearPermiso:
                    case TipoOperacionAcceso.MoverPermiso: _permisos[op.Codigo] = op.Permiso!.CodigoModulo; break;
                }
            }
            foreach (var codigo in desactivarSobrantes.Distinct())
            {
                Escrituras++;
                if (_modulos.ContainsKey(codigo))
                {
                    _modulos[codigo] = (_modulos[codigo].Padre, false);
                    resultado.Aplicadas.Add(new OperacionAcceso { Tipo = TipoOperacionAcceso.DesactivarModulo, Codigo = codigo });
                }
                else
                {
                    _permisos.Remove(codigo); // inactivo = fuera del estado activo que PassPlat compara
                    resultado.Aplicadas.Add(new OperacionAcceso { Tipo = TipoOperacionAcceso.DesactivarPermiso, Codigo = codigo });
                }
            }
            return Task.FromResult(resultado);
        }
    }

    private async Task<HttpResponseMessage> Post(string ruta, object? cuerpo, string? permisos)
    {
        var client = _f.CreateClient();
        using var req = new HttpRequestMessage(HttpMethod.Post, ruta);
        if (permisos is not null) req.Headers.Add("X-Prueba-Permisos", permisos);
        if (cuerpo is not null) req.Content = JsonContent.Create(cuerpo);
        return await client.SendAsync(req, Ct);
    }

    private async Task<PlanPublicacionDto> Plan(string permisos = Maestro)
    {
        var r = await Post("/api/admin/acceso/plan", null, permisos);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        return (await r.Content.ReadFromJsonAsync<PlanPublicacionDto>(Json, Ct))!;
    }

    private static async Task<string?> CodigoError(HttpResponseMessage r) =>
        (await r.Content.ReadFromJsonAsync<JsonElement>(Json, Ct)).TryGetProperty("errorCode", out var c) ? c.GetString() : null;

    // ---------------- pruebas ----------------

    [Fact]
    public async Task Plan_sin_token_responde_401()
    {
        Assert.Equal(HttpStatusCode.Unauthorized, (await Post("/api/admin/acceso/plan", null, null)).StatusCode);
    }

    [Fact]
    public async Task Plan_sin_claim_de_maestro_responde_403_aunque_tenga_INVENTA_CONFIGURACION()
    {
        // K5.1: ningún permiso INVENTA_* habilita el catálogo de acceso; solo el usuario maestro (is_system).
        Assert.Equal(HttpStatusCode.Forbidden, (await Post("/api/admin/acceso/plan", null, "INVENTA_CONFIGURACION_PRINCIPAL_VER,INVENTA_CONFIGURACION_PRINCIPAL_EDITAR")).StatusCode);
    }

    [Fact]
    public async Task Plan_contra_el_estado_real_de_PassPlat_da_135_operaciones_y_21_sobrantes()
    {
        var plan = await Plan();
        Assert.Equal(34, plan.ModulosACrear);
        Assert.Equal(0, plan.ModulosAActualizar);
        Assert.Equal(43, plan.ModulosAAsociar);
        Assert.Equal(58, plan.PermisosACrear); // S103: 31 VER + 9 CREAR + 12 EDITAR + 6 acciones propias, las que el código exige (antes 130 = 31 × 4 + 6)
        Assert.Equal(0, plan.PermisosAMover); // S100 (antes → después): 2 → 0; ya no hay permisos por entidad sueltos que mover
        Assert.Equal(135, plan.TotalOperaciones); // S103 (antes → después): 207 → 135 (72 permisos de ventana menos)
        // S100 D5b (antes → después): 1 → 21 sobrantes: el módulo INVENTA.CONFIG y, después, los 20 permisos por entidad.
        Assert.Equal(21, plan.Sobrantes.Count);
        Assert.Equal("INVENTA.CONFIG", plan.Sobrantes[0]);
        Assert.All(plan.Sobrantes.Skip(1), c => Assert.Matches("^INVENTA_(PRODUCTO|CLIENTE|PROVEEDOR|BODEGA|STOCK|VENTA|OC|COMPRA|CONFIG)_", c));
        Assert.Equal(HuellaEsperada, plan.HuellaManifiesto);
        Assert.Equal(0, _f.Remoto.Escrituras);
    }

    [Fact]
    public async Task Plan_con_huella_de_manifiesto_del_cliente_distinta_responde_409()
    {
        var r = await Post("/api/admin/acceso/plan", new { HuellaManifiestoCliente = "OTRA" }, Maestro);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("ACCESO_MANIFIESTO_DISTINTO", await CodigoError(r));
        Assert.Equal(HttpStatusCode.OK, (await Post("/api/admin/acceso/plan", new { HuellaManifiestoCliente = HuellaEsperada }, Maestro)).StatusCode);
    }

    [Fact]
    public async Task Publicar_sin_claim_de_maestro_responde_403_aunque_tenga_INVENTA_CONFIGURACION_EDITAR()
    {
        var plan = await Plan();
        var r = await Post("/api/admin/acceso/publicar", new { plan.HuellaRemota }, "INVENTA_CONFIGURACION_PRINCIPAL_VER,INVENTA_CONFIGURACION_PRINCIPAL_EDITAR");
        Assert.Equal(HttpStatusCode.Forbidden, r.StatusCode);
        Assert.Equal(0, _f.Remoto.Escrituras);
    }

    [Fact]
    public async Task Publicar_con_huella_remota_vieja_responde_409_sin_escribir()
    {
        var r = await Post("/api/admin/acceso/publicar", new { HuellaRemota = "VIEJA" }, Maestro);
        Assert.Equal(HttpStatusCode.Conflict, r.StatusCode);
        Assert.Equal("ACCESO_ESTADO_REMOTO_CAMBIO", await CodigoError(r));
        Assert.Equal(0, _f.Remoto.Escrituras);
    }

    [Fact]
    public async Task Publicar_desactivando_algo_que_no_es_sobrante_responde_400()
    {
        var plan = await Plan();
        var r = await Post("/api/admin/acceso/publicar", new { plan.HuellaRemota, DesactivarSobrantes = new[] { "INVENTA.VENTAS" } }, Maestro);
        Assert.Equal(HttpStatusCode.BadRequest, r.StatusCode);
        Assert.Equal("ACCESO_SOBRANTE_INVALIDO", await CodigoError(r));
        Assert.Equal(0, _f.Remoto.Escrituras);
    }

    [Fact]
    public async Task Publicar_ignora_un_manifiesto_en_el_cuerpo_y_volver_a_planificar_da_cero()
    {
        var plan = await Plan();
        var cuerpo = new { plan.HuellaRemota, DesactivarSobrantes = plan.Sobrantes, Manifiesto = new { AppCodigo = "INVENTA", Areas = Array.Empty<object>() } };
        var r = await Post("/api/admin/acceso/publicar", cuerpo, Maestro);
        Assert.Equal(HttpStatusCode.OK, r.StatusCode);
        var resultado = (await r.Content.ReadFromJsonAsync<ResultadoPublicacionDto>(Json, Ct))!;
        Assert.True(resultado.Exito);
        Assert.Equal(156, resultado.Aplicadas.Count); // 135 + desactivar INVENTA.CONFIG y los 20 permisos por entidad
        Assert.Equal(20, resultado.Aplicadas.Count(o => o.Tipo == TipoOperacionAcceso.DesactivarPermiso));

        var otra = await Plan();
        Assert.Equal(0, otra.TotalOperaciones);
        Assert.Empty(otra.Sobrantes);
    }

    [Fact]
    public void Politicas_registradas_son_las_descubiertas_de_RequierePermiso_y_ninguna_por_entidad()
    {
        // S100 (antes → después): 20 políticas por entidad → solo permisos de ventana, descubiertos de [RequierePermiso].
        var descubiertas = CBP.WebApi.Extensions.DescubridorPoliticas.Descubrir(typeof(Program).Assembly, "INVENTA_");
        Assert.NotEmpty(descubiertas);
        Assert.DoesNotContain(descubiertas, c => System.Text.RegularExpressions.Regex.IsMatch(c, "^INVENTA_(PRODUCTO|CLIENTE|PROVEEDOR|BODEGA|STOCK|VENTA|OC|COMPRA|CONFIG)_"));
        var opciones = _f.Services.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        foreach (var p in descubiertas)
        {
            var politica = opciones.GetPolicy(p);
            Assert.NotNull(politica);
            var claim = Assert.Single(politica!.Requirements.OfType<ClaimsAuthorizationRequirement>());
            Assert.Equal("permissions", claim.ClaimType);
            Assert.Equal([p], claim.AllowedValues!);
            Assert.Single(politica.Requirements.OfType<DenyAnonymousAuthorizationRequirement>());
        }
        // K5.1: la política del catálogo de acceso exige el claim de usuario maestro de PassPlat.
        var maestro = opciones.GetPolicy(Program.PoliticaUsuarioMaestro);
        Assert.NotNull(maestro);
        var req = Assert.Single(maestro!.Requirements.OfType<ClaimsAuthorizationRequirement>());
        Assert.Equal("is_system", req.ClaimType);
        Assert.Equal(["true"], req.AllowedValues!);
    }

    [Fact]
    public void Manifiesto_del_servidor_tiene_la_huella_esperada_y_58_permisos()
    {
        var m = _f.Services.GetRequiredService<ManifiestoAcceso>();
        Assert.Equal(HuellaEsperada, m.Huella);
        Assert.Equal(31, m.TotalVentanas);
        Assert.Equal(58, m.PermisosCatalogo.Count); // S100 (antes → después): 78 → 58; ya no hay 20 por entidad, todo es de ventana
        var area = m.PermisosCatalogo.ToDictionary(p => p.Codigo, p => p.CodigoModulo);
        Assert.Equal("INVENTA.CONFIGURACION", area["INVENTA_CONFIGURACION_PRINCIPAL_VER"]);
        Assert.Equal("INVENTA.COMPRAS", area["INVENTA_COMPRAS_ORDENES_VER"]);
        Assert.Equal("INVENTA.INVENTARIO", area["INVENTA_INVENTARIO_STOCK_VER"]);
        Assert.Equal("INVENTA.PRODUCTOS", area["INVENTA_PRODUCTOS_PRINCIPAL_VER"]);
        Assert.Equal("INVENTA.VENTAS.DETALLE", area["INVENTA_VENTAS_DETALLE_ANULAR"]); // acción propia: cuelga de su ventana
        Assert.Equal("INVENTA.BODEGAS.PRINCIPAL", area["INVENTA_BODEGAS_PRINCIPAL_ADMIN"]);
    }}
