using CBP.Acceso;
using CBP.Blazor;
using CBP.Blazor.Acceso;
using CBP.Blazor.Formatting;
using CBP.Blazor.Mud;
using Inventa.Web;
using Inventa.Web.Models;
using Inventa.Web.Services.Auth;
using Inventa.Web.Services.Http;
using Inventa.Web.Services.Importacion;
using Inventa.Web.Services.Storage;
using Inventa.Web.Services.Tenant;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using MudBlazor.Services;
using System.Globalization;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

// S93.2: cultura es-CL determinista sin ICU (se complementa con
// applicationCulture en wwwroot/index.html).
CultureInfo.DefaultThreadCurrentCulture = FormatoCl.Cultura;
CultureInfo.DefaultThreadCurrentUICulture = FormatoCl.Cultura;
builder.RootComponents.Add<Inventa.Web.App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddCbpBlazor();
builder.Services.AddAccesoDinamico(AccesoGenerado.Manifiesto);

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
    throw new InvalidOperationException("ApiBaseUrl is not configured. Add \"ApiBaseUrl\" to wwwroot/appsettings.json.");

if (!Uri.TryCreate(apiBaseUrl, UriKind.Absolute, out var apiUri))
    throw new InvalidOperationException($"ApiBaseUrl is not a valid absolute URI: '{apiBaseUrl}'");

var appSettings = builder.Configuration.GetSection("AppSettings").Get<AppSettings>() ?? new AppSettings { ApiBaseUrl = apiBaseUrl };
appSettings.ApiBaseUrl = apiBaseUrl;
if (string.IsNullOrWhiteSpace(appSettings.AppCodigo))
    throw new InvalidOperationException("AppSettings:AppCodigo is not configured. Add \"AppCodigo\" to wwwroot/appsettings.json (S84.4: app desde configuración en solo lectura).");
builder.Services.AddSingleton(appSettings);

builder.Services.AddMudServices();
builder.Services.AddMudEsCl(); // S93.2: textos MudBlazor en espanol
builder.Services.AddAuthorizationCore();

builder.Services.AddScoped<JwtClaimsReader>();
builder.Services.AddScoped<ITokenStorage, TokenStorage>();
builder.Services.AddScoped<IAccessTokenProvider, TokenStorageAccessTokenProvider>();
builder.Services.AddScoped<TenantContextService>();
builder.Services.AddScoped<ITenantContextProvider>(sp => sp.GetRequiredService<TenantContextService>());
builder.Services.AddScoped<TenantService>();
builder.Services.AddScoped<PermissionService>();
builder.Services.AddScoped<CustomAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<CustomAuthenticationStateProvider>());
builder.Services.AddScoped<AuthService>();

builder.Services.AddScoped<CorrelationHandler>();
builder.Services.AddScoped<AuthenticatedHttpHandler>();
builder.Services.AddScoped<TenantHttpHandler>();
builder.Services.AddScoped<ErrorHandler>();

builder.Services.AddScoped(sp =>
{
    var correlation = sp.GetRequiredService<CorrelationHandler>();
    var authenticated = sp.GetRequiredService<AuthenticatedHttpHandler>();
    var tenant = sp.GetRequiredService<TenantHttpHandler>();
    var error = sp.GetRequiredService<ErrorHandler>();
    error.InnerHandler = new HttpClientHandler();
    tenant.InnerHandler = error;
    authenticated.InnerHandler = tenant;
    correlation.InnerHandler = authenticated;
    return new HttpClient(correlation) { BaseAddress = apiUri };
});

builder.Services.AddScoped<ApiClient>();
// S104: subida directa al storage; HttpClient propio, sin el token de la app. Las URL relativas (storage local) van contra el API.
builder.Services.AddCbpDirectUpload(apiUri);
builder.Services.AddAccesoApiCliente("api/admin/acceso"); // S95 K5: panel /configuracion/acceso
builder.Services.AddMenuAccesoCliente("api/acceso/menu"); // S100 D7: menú que entrega PassPlat
builder.Services.AddScoped<Inventa.Web.Services.Importacion.ImportacionApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Productos.ProductosApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Precios.PreciosApiService>(); // S93.1
builder.Services.AddScoped<Inventa.Web.Services.Catalogos.CatalogosApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Clientes.ClientesApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Proveedores.ProveedoresApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Bodegas.BodegasApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Inventario.InventarioApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Compras.ComprasApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Ventas.VentasApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Configuracion.ConfiguracionApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Storefront.StorefrontApiService>();
builder.Services.AddScoped<Inventa.Web.Services.Dashboard.DashboardApiService>();

var host = builder.Build();
var authProvider = host.Services.GetRequiredService<CustomAuthenticationStateProvider>();
await authProvider.InitializeAsync();
await host.RunAsync();
