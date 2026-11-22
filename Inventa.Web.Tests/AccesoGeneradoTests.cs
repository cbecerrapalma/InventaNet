using System.Text;
using System.Text.Json;
using Bunit;
using CBP.Acceso;
using Inventa.Web.Layout;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Storage;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using MudBlazor.Services;
using Xunit;

namespace Inventa.Web.Tests;

/// <summary>
/// S95 (corrección 2): el manifiesto que CBP.Acceso.Generador produce al compilar Inventa.Web coincide,
/// ventana por ventana, con la tabla esperada verificada en el código
/// (Docs/Sprints/S95/Evidence/K0_TablaEsperada.md = §2.4 de S95-CORRECCION-K1K2-PLAN-OpenCode.md).
/// </summary>
public sealed class AccesoGeneradoTests
{
    private static readonly ManifiestoAcceso M = AccesoGenerado.Manifiesto;

    // S100: con AccesoMenuDinamico el nombre, el ícono y el orden salen de la página (no del menú) y el permiso de la página es de ventana.
    // Orden | Ruta | Código | Área | Nombre | Ícono | Permiso (null = solo sesión) | Visible en el menú (ruta sin parámetros)
    public static TheoryData<int, string, string, string, string, string, string?, bool> Ventanas => new()
    {
        { 1, "/", "INVENTA.INICIO", "INVENTA", "Dashboard", "Dashboard", "INVENTA_INICIO_VER", true },
        { 2, "/bodegas", "INVENTA.BODEGAS.PRINCIPAL", "INVENTA.BODEGAS", "Bodegas", "Warehouse", "INVENTA_BODEGAS_PRINCIPAL_VER", true },
        { 3, "/bodegas/{Id}", "INVENTA.BODEGAS.DETALLE", "INVENTA.BODEGAS", "Detalle Bodega", "Warehouse", "INVENTA_BODEGAS_DETALLE_VER", false },
        { 4, "/catalogos/atributos", "INVENTA.CATALOGOS.ATRIBUTOS", "INVENTA.CATALOGOS", "Atributos", "Tune", "INVENTA_CATALOGOS_ATRIBUTOS_VER", true },
        { 5, "/catalogos/categorias", "INVENTA.CATALOGOS.CATEGORIAS", "INVENTA.CATALOGOS", "Categorías", "Label", "INVENTA_CATALOGOS_CATEGORIAS_VER", true },
        { 6, "/catalogos/marcas", "INVENTA.CATALOGOS.MARCAS", "INVENTA.CATALOGOS", "Marcas", "BrandingWatermark", "INVENTA_CATALOGOS_MARCAS_VER", true },
        { 7, "/catalogos/metodos-pago", "INVENTA.CATALOGOS.METODOS_PAGO", "INVENTA.CATALOGOS", "Métodos de Pago", "Payment", "INVENTA_CATALOGOS_METODOS_PAGO_VER", true },
        { 8, "/catalogos/unidades", "INVENTA.CATALOGOS.UNIDADES", "INVENTA.CATALOGOS", "Unidades", "Straighten", "INVENTA_CATALOGOS_UNIDADES_VER", true },
        { 9, "/clientes", "INVENTA.CLIENTES.PRINCIPAL", "INVENTA.CLIENTES", "Clientes", "People", "INVENTA_CLIENTES_PRINCIPAL_VER", true },
        { 10, "/clientes/{Id}", "INVENTA.CLIENTES.DETALLE", "INVENTA.CLIENTES", "Detalle Cliente", "People", "INVENTA_CLIENTES_DETALLE_VER", false },
        { 11, "/compras/ordenes", "INVENTA.COMPRAS.ORDENES", "INVENTA.COMPRAS", "Órdenes de Compra", "ShoppingBag", "INVENTA_COMPRAS_ORDENES_VER", true },
        { 12, "/compras/ordenes/{Id}", "INVENTA.COMPRAS.ORDENES_DETALLE", "INVENTA.COMPRAS", "Detalle Orden", "ListAlt", "INVENTA_COMPRAS_ORDENES_DETALLE_VER", false },
        { 13, "/configuracion", "INVENTA.CONFIGURACION.PRINCIPAL", "INVENTA.CONFIGURACION", "Configuración", "Settings", "INVENTA_CONFIGURACION_PRINCIPAL_VER", true },
        { 14, "/configuracion/storefront", "INVENTA.CONFIGURACION.STOREFRONT", "INVENTA.CONFIGURACION", "Tienda pública", "Store", "INVENTA_CONFIGURACION_STOREFRONT_VER", true },
        { 15, "/configuracion/tenant", "INVENTA.CONFIGURACION.TENANT", "INVENTA.CONFIGURACION", "Configuración del tenant", "AdminPanelSettings", "INVENTA_CONFIGURACION_TENANT_VER", true },
        { 16, "/importaciones", "INVENTA.IMPORTACIONES.PRINCIPAL", "INVENTA.IMPORTACIONES", "Importar productos", "CloudUpload", "INVENTA_IMPORTACIONES_PRINCIPAL_VER", true },
        { 17, "/importaciones/nueva", "INVENTA.IMPORTACIONES.NUEVA", "INVENTA.IMPORTACIONES", "Nueva importación de productos", "CloudUpload", "INVENTA_IMPORTACIONES_NUEVA_VER", true },
        { 18, "/importaciones/{id:long}", "INVENTA.IMPORTACIONES.DETALLE", "INVENTA.IMPORTACIONES", "Detalle de Importar productos", "Description", "INVENTA_IMPORTACIONES_DETALLE_VER", false },
        { 19, "/inventario/ajustes", "INVENTA.INVENTARIO.AJUSTES", "INVENTA.INVENTARIO", "Ajustes", "Build", "INVENTA_INVENTARIO_AJUSTES_VER", true },
        { 20, "/inventario/lotes", "INVENTA.INVENTARIO.LOTES", "INVENTA.INVENTARIO", "Lotes", "BatchPrediction", "INVENTA_INVENTARIO_LOTES_VER", true },
        { 21, "/inventario/movimientos", "INVENTA.INVENTARIO.MOVIMIENTOS", "INVENTA.INVENTARIO", "Movimientos", "SyncAlt", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", true },
        { 22, "/inventario/stock", "INVENTA.INVENTARIO.STOCK", "INVENTA.INVENTARIO", "Stock", "Inventory2", "INVENTA_INVENTARIO_STOCK_VER", true },
        { 23, "/inventario/stock-disponible", "INVENTA.INVENTARIO.STOCK_DISPONIBLE", "INVENTA.INVENTARIO", "Stock Disponible", "Visibility", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", true },
        { 24, "/inventario/transferencias", "INVENTA.INVENTARIO.TRANSFERENCIAS", "INVENTA.INVENTARIO", "Transferencias", "SyncAlt", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", true },
        { 25, "/precios", "INVENTA.PRECIOS.PRINCIPAL", "INVENTA.PRECIOS", "Precios y ofertas", "Sell", "INVENTA_PRECIOS_PRINCIPAL_VER", true },
        { 26, "/productos", "INVENTA.PRODUCTOS.PRINCIPAL", "INVENTA.PRODUCTOS", "Productos", "Inventory", "INVENTA_PRODUCTOS_PRINCIPAL_VER", true },
        { 27, "/productos/{Id}", "INVENTA.PRODUCTOS.DETALLE", "INVENTA.PRODUCTOS", "Detalle de Productos", "Inventory", "INVENTA_PRODUCTOS_DETALLE_VER", false },
        { 28, "/proveedores", "INVENTA.PROVEEDORES.PRINCIPAL", "INVENTA.PROVEEDORES", "Proveedores", "LocalShipping", "INVENTA_PROVEEDORES_PRINCIPAL_VER", true },
        { 29, "/proveedores/{Id}", "INVENTA.PROVEEDORES.DETALLE", "INVENTA.PROVEEDORES", "Detalle Proveedor", "LocalShipping", "INVENTA_PROVEEDORES_DETALLE_VER", false },
        { 30, "/ventas", "INVENTA.VENTAS.PRINCIPAL", "INVENTA.VENTAS", "Ventas", "PointOfSale", "INVENTA_VENTAS_PRINCIPAL_VER", true },
        { 31, "/ventas/{Id}", "INVENTA.VENTAS.DETALLE", "INVENTA.VENTAS", "Detalle Venta", "Receipt", "INVENTA_VENTAS_DETALLE_VER", false },
    };

    [Theory]
    [MemberData(nameof(Ventanas))]
    public void Ventana_coincide_con_la_tabla_esperada(int orden, string ruta, string codigo, string area, string nombre, string icono, string? permiso, bool enMenu)
    {
        var a = M.Areas.Single(x => x.Codigo == area);
        var v = a.Modulos.Single(x => x.Ruta == ruta);
        Assert.Equal(codigo, v.Codigo);
        Assert.Equal(nombre, v.Nombre);
        Assert.Equal(icono, v.Icono);
        Assert.Equal(orden, v.Orden);
        Assert.Equal(enMenu, v.EsVisibleMenu);
        Assert.Equal(area, v.CodigoPadre);
        if (permiso is null) Assert.Empty(v.Permisos);
        else Assert.Equal(permiso, Assert.Single(v.Permisos).Codigo);
    }

    [Fact]
    public void Manifiesto_tiene_31_ventanas_y_ninguna_fuera_de_la_tabla()
    {
        Assert.Equal("INVENTA", M.AppCodigo);
        Assert.Equal(31, M.TotalVentanas);
        var esperadas = Ventanas.Select(f => f.Data.Item2).ToHashSet();
        Assert.All(M.Areas.SelectMany(a => a.Modulos), v => Assert.Contains(v.Ruta!, esperadas));
    }

    [Fact]
    public void Areas_son_12_en_el_orden_de_las_rutas_con_la_raiz_primero()
    {
        // S100: sin archivo de menú, las áreas siguen el orden de las rutas de las páginas.
        string[] esperadas =
        [
            "INVENTA", "INVENTA.BODEGAS", "INVENTA.CATALOGOS", "INVENTA.CLIENTES", "INVENTA.COMPRAS", "INVENTA.CONFIGURACION",
            "INVENTA.IMPORTACIONES", "INVENTA.INVENTARIO", "INVENTA.PRECIOS", "INVENTA.PRODUCTOS", "INVENTA.PROVEEDORES",
            "INVENTA.VENTAS"
        ];
        Assert.Equal(esperadas, M.Areas.OrderBy(a => a.Orden).Select(a => a.Codigo));
        Assert.Equal(Enumerable.Range(0, 12), M.Areas.Select(a => a.Orden).OrderBy(o => o));
    }

    [Fact]
    public void Diagnosticos_son_35_publicas2_permisoRaiz1_excluida1_permisosVentana31()
    {
        // S100 (antes → después): 45 → 35. Sin menú dinámico había ACC002 (4), ACC003 (6) y ACC004 (1) por leer Layout/NavMenu.razor;
        // ahora no se emiten y aparece ACC006 (1): INVENTA_INICIO_VER no tiene área y queda en la raíz.
        // S127: /signin-callback es pública y suma un ACC001 (35 → 36; públicas 2 → 3).
        Assert.Equal(36, M.Diagnosticos.Count);
        Assert.Single(M.Diagnosticos, d => d.Codigo == "ACC025" && d.Mensaje.Contains("/configuracion/acceso")); // K5.1: excluida
        Assert.Equal(31, M.Diagnosticos.Count(d => d.Codigo == "ACC030" && d.Nivel == NivelDiagnostico.Informativo));
        Assert.Equal(3, M.Diagnosticos.Count(d => d.Codigo == "ACC001" && d.Nivel == NivelDiagnostico.Informativo));
        Assert.Equal(1, M.Diagnosticos.Count(d => d.Codigo == "ACC006" && d.Nivel == NivelDiagnostico.Informativo));
        foreach (var codigo in new[] { "ACC002", "ACC003", "ACC004", "ACC008" })
            Assert.DoesNotContain(M.Diagnosticos, d => d.Codigo == codigo);
        Assert.Equal(0, M.TotalDiagnosticosError);
        Assert.Equal(0, M.TotalDiagnosticosAdvertencia);
    }

    [Fact]
    public void Ningun_nombre_ni_icono_contiene_codigo_Razor()
    {
        foreach (var v in M.Areas.SelectMany(a => a.Modulos))
        {
            Assert.DoesNotContain("@", v.Nombre);
            Assert.DoesNotContain("@", v.Icono ?? "");
            Assert.DoesNotContain(".", v.Icono ?? "");
        }
    }

    [Fact]
    public void Permisos_cuelgan_del_area_D5()
    {
        var permisos = M.Areas.SelectMany(a => a.Modulos).SelectMany(m => m.Permisos)
            .GroupBy(p => p.Codigo).ToDictionary(g => g.Key, g => g.Select(p => p.CodigoModulo).Distinct().Single());
        Assert.Equal("INVENTA.INVENTARIO", permisos["INVENTA_INVENTARIO_STOCK_VER"]);
        Assert.Equal("INVENTA.COMPRAS", permisos["INVENTA_COMPRAS_ORDENES_VER"]);
        Assert.Equal("INVENTA.CONFIGURACION", permisos["INVENTA_CONFIGURACION_PRINCIPAL_VER"]);
        Assert.Equal("INVENTA.PRODUCTOS", permisos["INVENTA_PRODUCTOS_PRINCIPAL_VER"]);
        Assert.Equal("INVENTA", permisos["INVENTA_INICIO_VER"]); // ACC006: sin área, queda en la raíz
    }

    /// <summary>
    /// S95 K3–K4 (D1): huella del catálogo de páginas. Inventa.WebAPI.Tests exige el MISMO literal (el WebAPI genera el
    /// manifiesto en el servidor con estas mismas páginas). Si cambia una página o el menú, actualizar ambos literales.
    /// </summary>
    public const string HuellaEsperada = "9D9D5BBE00109290B3495E7D738C5A57C594C5CE9FFF89DFDF52CFB43E90F5AE";

    [Fact]
    public void Huella_del_catalogo_es_la_esperada_y_coincide_con_la_recalculada()
    {
        Assert.Equal(HuellaEsperada, M.Huella);
        Assert.Equal(M.CalcularHuella(), M.Huella);
        // S100 (antes → después): 67 → 58. Se fueron los 9 permisos por entidad de las páginas; el permiso de cada página es el VER
        // de su ventana (cuelga del área, no de la ventana) y no queda ninguno de los 20 por entidad.
        var entidad = new System.Text.RegularExpressions.Regex("^INVENTA_(PRODUCTO|CLIENTE|PROVEEDOR|BODEGA|STOCK|VENTA|OC|COMPRA|CONFIG)_");
        Assert.DoesNotContain(M.PermisosCatalogo, p => entidad.IsMatch(p.Codigo));
        Assert.Equal(58, M.PermisosCatalogo.Count);
    }

    [Fact]
    public void Inventa_Web_usa_AccesoPagina_solo_en_la_ventana_tecnica_de_acceso_y_en_Inicio()
    {
        var tipos = typeof(AccesoGenerado).Assembly.GetTypes()
            .Where(t => t.GetCustomAttributes(typeof(AccesoPaginaAttribute), false).Length > 0).ToList();
        Assert.Equal(2, tipos.Count);
        var acceso = Assert.Single(tipos, t => t == typeof(Inventa.Web.Pages.Configuracion.Acceso));
        var attr = (AccesoPaginaAttribute)acceso.GetCustomAttributes(typeof(AccesoPaginaAttribute), false)[0];
        Assert.True(attr.Excluir);
        Assert.DoesNotContain(M.Areas.SelectMany(a => a.Modulos), v => v.Ruta == "/configuracion/acceso");

        // S100: Inicio solo exige sesión; el atributo declara el permiso con que PassPlat lo muestra en el menú.
        var inicio = Assert.Single(tipos, t => t != acceso);
        var attrInicio = (AccesoPaginaAttribute)inicio.GetCustomAttributes(typeof(AccesoPaginaAttribute), false)[0];
        Assert.Equal("INVENTA_INICIO_VER", attrInicio.Permiso);
        Assert.False(attrInicio.Excluir);
    }

    // ---------- S95 K5.1 (D5): permisos propios por ventana = VER + acciones que la página chequea con Has* ----------

    public static TheoryData<string, string> AccionesPorVentana => new()
    {
        { "/", "VER" }, { "/productos", "VER,CREAR,EDITAR" }, { "/productos/{Id}", "VER,CREAR,EDITAR" }, { "/precios", "VER,EDITAR" },
        { "/clientes", "VER,EDITAR" }, { "/clientes/{Id}", "VER,EDITAR" }, { "/ventas", "VER,CREAR" }, { "/ventas/{Id}", "VER,ANULAR" },
        { "/bodegas", "VER,ADMIN" }, { "/bodegas/{Id}", "VER,ADMIN" }, { "/inventario/stock", "VER" }, { "/inventario/stock-disponible", "VER" },
        { "/inventario/movimientos", "VER" }, { "/inventario/lotes", "VER" }, { "/inventario/transferencias", "VER,TRANSFERIR" },
        { "/inventario/ajustes", "VER,AJUSTAR" }, { "/compras/ordenes", "VER,CREAR" }, { "/compras/ordenes/{Id}", "VER,RECIBIR" },
        { "/catalogos/categorias", "VER,CREAR,EDITAR" }, { "/catalogos/marcas", "VER,CREAR,EDITAR" }, { "/catalogos/unidades", "VER" },
        { "/catalogos/metodos-pago", "VER" }, { "/catalogos/atributos", "VER" }, { "/importaciones", "VER,CREAR" },
        { "/importaciones/nueva", "VER,CREAR" }, { "/importaciones/{id:long}", "VER,CREAR" }, { "/proveedores", "VER,EDITAR" },
        { "/proveedores/{Id}", "VER,EDITAR" }, { "/configuracion", "VER,EDITAR" }, { "/configuracion/tenant", "VER,EDITAR" },
        { "/configuracion/storefront", "VER,EDITAR" },
    };

    [Theory]
    [MemberData(nameof(AccionesPorVentana))]
    public void Ventana_exporta_un_permiso_por_accion_que_chequea(string ruta, string acciones)
    {
        var v = M.Areas.SelectMany(a => a.Modulos).Single(x => x.Ruta == ruta);
        var esperado = acciones.Split(',').Select(a => v.Codigo.Replace('.', '_') + "_" + a).ToArray();
        Assert.Equal(esperado, v.PermisosVentana.Select(p => p.Codigo));
        Assert.All(v.PermisosVentana, p => Assert.Equal(v.Codigo, p.CodigoModulo));
        Assert.Equal($"{v.Nombre}: ver", v.PermisosVentana[0].Nombre);
    }

    [Fact]
    public void Permisos_por_ventana_son_58()
    {
        Assert.Equal(58, M.Areas.SelectMany(a => a.Modulos).Sum(v => v.PermisosVentana.Count));
        Assert.Equal(31, AccionesPorVentana.Count());
    }

    // ---------- S100: el menú ya no es de este archivo (ver S100_NavMenuTests) ----------

    private sealed class StubStorage(string token) : ITokenStorage
    {
        public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(token);
        public Task<string?> GetRefreshTokenAsync(CancellationToken ct = default) => Task.FromResult<string?>(null);
        public Task<int?> GetTenantIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(4);
        public Task<int?> GetTenantPassPlatIdAsync(CancellationToken ct = default) => Task.FromResult<int?>(2);
        public Task SetSessionAsync(string accessToken, string? refreshToken, int? tenantId, int? tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task SetTenantIdsAsync(int tenantId, int tenantPassPlatId, CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearTenantIdsAsync(CancellationToken ct = default) => Task.CompletedTask;
        public Task ClearAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private static string Token(bool maestro, string[] permisos)
    {
        object payload = maestro ? new { permissions = permisos, is_system = "true" } : new { permissions = permisos };
        return "e30." + Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonSerializer.Serialize(payload))) + ".";
    }

    private static BunitContext Contexto(string token)
    {
        var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        ctx.Services.AddSingleton<ITokenStorage>(new StubStorage(token));
        ctx.Services.AddSingleton(_ => new JwtClaimsReader());
        ctx.Services.AddSingleton<PermissionService>();
        ctx.AddAuthorization().SetAuthorized("admin_abarrotes");
        return ctx;
    }

    [Fact]
    public async Task Ventana_de_acceso_sin_claim_de_maestro_muestra_sin_permiso_aunque_tenga_todos_los_INVENTA()
    {
        await using var ctx = Contexto(Token(false, ["INVENTA_CONFIG_VER", "INVENTA_CONFIG_EDITAR", "INVENTA_PRODUCTO_VER"]));
        var cut = ctx.Render<Inventa.Web.Pages.Configuracion.Acceso>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindComponents<Inventa.Web.Shared.Forbidden>()), TimeSpan.FromSeconds(5));
        Assert.Empty(cut.FindComponents<CBP.Blazor.Mud.Acceso.PanelAccesoAdmin>());
        Assert.Empty(cut.FindComponents<Inventa.Web.Shared.PageHeader>()); // ni el encabezado: la operación no se ve
    }

    [Fact]
    public async Task Ventana_de_acceso_con_claim_de_maestro_muestra_el_panel()
    {
        await using var ctx = Contexto(Token(true, ["INVENTA_CONFIG_VER"]));
        ctx.Services.AddSingleton(AccesoGenerado.Manifiesto);
        ctx.Services.AddSingleton(new CBP.Blazor.Acceso.AccesoApiCliente(new HttpClient { BaseAddress = new Uri("https://api.prueba/") }, "api/admin/acceso"));
        var cut = ctx.Render<Inventa.Web.Pages.Configuracion.Acceso>();
        cut.WaitForAssertion(() => Assert.Single(cut.FindComponents<CBP.Blazor.Mud.Acceso.PanelAccesoAdmin>()), TimeSpan.FromSeconds(5));
        Assert.Empty(cut.FindComponents<Inventa.Web.Shared.Forbidden>());
    }

    // ---------- S95 K5 (D5): miga de pan desde el catálogo generado ----------

    [Theory]
    [InlineData("/configuracion/acceso", "Inicio|Configuración|Acceso y permisos")]
    [InlineData("/configuracion/storefront", "Inicio|Configuración|Tienda pública")]
    [InlineData("/inventario/lotes", "Inicio|Inventario|Lotes")]
    public async Task Miga_de_pan_toma_los_nombres_del_catalogo(string ruta, string esperado)
    {
        await using var ctx = new BunitContext();
        ctx.JSInterop.Mode = JSRuntimeMode.Loose;
        ctx.Services.AddMudServices();
        ctx.Services.AddSingleton(AccesoGenerado.Manifiesto);
        ctx.AddAuthorization().SetAuthorized("admin_abarrotes");
        ctx.Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>().NavigateTo(ruta);

        var cut = ctx.Render<Inventa.Web.Shared.AppBreadcrumbs>();
        cut.WaitForAssertion(() => Assert.Equal(esperado,
            string.Join("|", cut.FindAll(".mud-breadcrumb-item").Select(i => i.TextContent.Trim()))), TimeSpan.FromSeconds(5));
    }
}
