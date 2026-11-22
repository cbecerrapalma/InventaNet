using Inventa.Application.Async;
using Inventa.Application.Async.Services;
using Inventa.Data.Async;
using Inventa.Data.Async.Configuration;
using Inventa.WebAPI.Authentication;
using Inventa.WebAPI.Clients;
using Inventa.WebAPI.Configuration;
using Inventa.WebAPI.Controllers;
using Inventa.WebAPI.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using CBP.Excel.ClosedXML.Extensions;
using CBP.Acceso;
using CBP.Storage;
using CBP.Storage.Imaging;
using CBP.Storage.Local;
using CBP.Storage.S3;
using CBP.WebApi.Extensions;
using CBP.Events.DependencyInjection;
using CBP.Outbox;
using Inventa.Data.Async.Repositories;
using PassPlat.Client;
using PassPlat.Client.Services;
using Inventa.WebAPI;

var builder = WebApplication.CreateBuilder(args);

// S57.45-R1: registro explicito de UserSecrets en Development. Observado 2026-09-24:
// CreateBuilder no incorpora el provider en este linaje de host aun con env=Development
// y [UserSecretsId] visible (causa: proveedor ausente => fail-fast "ausente"). Fuente #1
// S57.40 (dev). En prod rige secrets.json machine-local (AddMachineSecretsFile, linea inf).
if (builder.Environment.IsDevelopment())
    builder.Configuration.AddUserSecrets<Program>(optional: true);

// S57.38: conexion propiedad de Inventa.Data.Async (ConnectionStrings:InventaDb, sin env SQL).
builder.Services.AddInventaDatabase(builder.Configuration);

// Data Async — all 27 async repositories
builder.Services.AddDataAsync();

// Application Async — all aggregate async services
builder.Services.AddApplicationAsync();

// S57.27: PassPlat runtime options — contrato único tipado + validado, fail-fast (ValidateOnStart).
// Fuente: IConfiguration sección "PassPlat". JAMÁS dbo.Configuracion. Sin fallback.
builder.Services.AddOptions<PassPlatOptions>()
    .Bind(builder.Configuration.GetSection(PassPlatOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

// S79: PassPlatService options (usuario técnico) — validación solo al USAR (fail-fast en endpoint, no en arranque).
// S79.2: SIN ValidateDataAnnotations. Con esa validación, IOptions.Value lanza OptionsValidationException
// en el constructor del controlador/proveedor y el endpoint responde 500 en vez del 503
// CONTACTO_NO_CONFIGURADO diseñado (el controlador valida con Validator.TryValidateObject al USAR).
builder.Services.AddOptions<PassPlatServiceOptions>()
    .Bind(builder.Configuration.GetSection(PassPlatServiceOptions.SectionName));

// S79: token provider (singleton) + email sender + servicio de contacto
builder.Services.AddSingleton<IPassPlatServiceTokenProvider, PassPlatServiceTokenProvider>();
builder.Services.AddScoped<IContactoEmailSender, ContactoEmailSender>();
builder.Services.AddScoped<ContactoServiceAsync>();

// S87: ClosedXML Excel engine for import templates/validation
builder.Services.AddClosedXmlExcel();

// PassPlat typed HttpClient — dev localhost self-signed bypass (Development only)
builder.Services.AddHttpClient<IPassPlatClient, PassPlatClient>((sp, client) =>
{
    var opts = sp.GetRequiredService<IOptions<PassPlatOptions>>().Value;
    client.BaseAddress = new Uri(opts.ApiBaseUrl);
    client.Timeout = TimeSpan.FromMilliseconds(opts.TimeoutMs);
}).ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler();
    var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");
    if (string.Equals(env, "Development", StringComparison.OrdinalIgnoreCase))
        handler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
    return handler;
});

// S57.40: JWT HS256 — secret NUNCA por env (UserSecrets en dev, secrets.json machine-local
// en prod). Fail-fast accionable si falta (mirror PassPlat.WebAPI/Program.cs:52-69).
JwtBearerConfiguration.AddMachineSecretsFile(builder.Configuration);
JwtBearerConfiguration.RejectEnvironmentSecret(builder.Configuration);
var jwtOptions = JwtBearerConfiguration.ReadAndValidate(builder.Configuration);
builder.Services.AddOptions<JwtOptions>()
    .Bind(builder.Configuration.GetSection(JwtOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();
// S56.26: sanitized startup log — never log secret material
Console.WriteLine($"[JWT CONFIG] Configured=True Issuer={jwtOptions.Issuer} Audiences=[{string.Join(",", jwtOptions.ValidAudiences)}] Env={builder.Environment.EnvironmentName}");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = JwtBearerConfiguration.BuildValidationParameters(jwtOptions);
        opt.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = ctx =>
            {
                Console.WriteLine($"[JWT FAIL] {ctx.Exception.GetType().Name}: {ctx.Exception.Message}");
                return Task.CompletedTask;
            },
            OnTokenValidated = async ctx =>
            {
                try
                {
                    var sync = ctx.HttpContext.RequestServices.GetRequiredService<TenantUserSyncMiddlewareHandler>();
                    await sync.HandleAsync(ctx);
                    Console.WriteLine($"[JWT SYNC OK] tid={ctx.Principal?.FindFirstValue("tid")} sub={ctx.Principal?.FindFirstValue("sub")}");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[JWT SYNC FAIL] {ex.GetType().Name}: {ex.Message} {ex.StackTrace}");
                    ctx.Fail(ex.Message);
                }
            },
            OnForbidden = ctx =>
            {
                Console.WriteLine($"[JWT FORBIDDEN] {ctx.Result}");
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddScoped<TenantUserSyncMiddlewareHandler>();

// S95 D2: políticas INVENTA_* descubiertas de los [Authorize(Policy)] de los controladores (sin lista escrita a mano).
// Misma regla que antes: usuario autenticado con el claim "permissions" = nombre de la política. Default DENY.
var politicasInventa = DescubridorPoliticas.Descubrir(typeof(Program).Assembly, "INVENTA_");
builder.Services.AddAuthorization(opt =>
{
    opt.AgregarPoliticasDePermiso(politicasInventa, "permissions");
    // S95 K5.1 (decisión del dueño 2026-10-19): el catálogo de acceso SOLO lo publica un usuario maestro de PassPlat
    // (Usuarios.EsSistema → claim is_system). Ningún permiso INVENTA_* lo habilita.
    opt.AgregarPoliticaClaim(PoliticaUsuarioMaestro, PassPlatClaims.EsSistema, PassPlatClaims.Verdadero);
});
// S100 D2: [RequierePermiso(…)] = «cualquiera de estos permisos de ventana»; mismo claim que las políticas de arriba.
builder.Services.AddRequierePermiso("permissions");

// S95 D1/D6: catálogo de acceso generado en el servidor (mismas páginas que Inventa.Web) + permisos de la API,
// y SDK oficial de PassPlat con el token del usuario de la petición (PassPlat autoriza con sus propios permisos).
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton(AccesoGenerado.Manifiesto.ConPermisosApi(politicasInventa));
var passPlatAcceso = builder.Configuration.GetSection(PassPlatOptions.SectionName).Get<PassPlatOptions>() ?? new PassPlatOptions();
builder.Services.AddPassPlatAcceso(o =>
{
    o.BaseUrl = passPlatAcceso.ApiBaseUrl;
    o.TimeoutMs = passPlatAcceso.TimeoutMs;
    o.ObtenerToken = DescubridorPoliticas.TokenDeLaPeticion;
});
if (builder.Environment.IsDevelopment())
{
    builder.Services.AddHttpClient<IInyectorCatalogoRemoto, PassPlatCatalogoCliente>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            // Igual que PassPlatClient: certificado local autofirmado solo en Development.
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    // S100 D6: mismo handler para el menú de PassPlat.
    builder.Services.AddHttpClient<IProveedorMenuRemoto, PassPlatMenuCliente>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
    // S127 D2: mismo handler para el login externo de PassPlat.
    builder.Services.AddHttpClient<IPassPlatLoginExterno, PassPlatLoginExternoCliente>()
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
        });
}

var storeOrigins = builder.Configuration.GetSection("Cors:StorePublicAllowedOrigins").Get<string[]>();
builder.Services.AddCors(opt =>
{
    opt.AddPolicy("StorePublic", p =>
    {
        if (builder.Environment.IsDevelopment() && (storeOrigins == null || storeOrigins.Length == 0))
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Retry-After");
        else if (storeOrigins != null && storeOrigins.Length > 0)
            p.WithOrigins(storeOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithExposedHeaders("Retry-After");
        else if (builder.Environment.IsDevelopment())
            p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Retry-After");
        else
            p.WithOrigins(Array.Empty<string>()).AllowAnyHeader().AllowAnyMethod().WithExposedHeaders("Retry-After"); // production requires explicit origins -> NEEDS_INFRASTRUCTURE_INPUT
    });
});
builder.Services.AddRateLimiter(opt =>
{
    opt.RejectionStatusCode = 429;
    opt.AddFixedWindowLimiter("Public", o => { o.PermitLimit = 20; o.Window = TimeSpan.FromSeconds(10); o.QueueLimit = 0; });
    opt.AddFixedWindowLimiter("Authenticated", o => { o.PermitLimit = 300; o.Window = TimeSpan.FromSeconds(10); o.QueueLimit = 0; });
    // S79: rate limiter particionado por IP para contacto público (5 req / 10 min, configurable)
    var contactLimit = builder.Configuration.GetValue("RateLimiter:PublicContact:PermitLimit", 5);
    var contactWindowMinutes = builder.Configuration.GetValue("RateLimiter:PublicContact:WindowMinutes", 10);
    opt.AddPolicy("PublicContact", context =>
    {
        var ip = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        return RateLimitPartition.GetFixedWindowLimiter(ip, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = contactLimit,
            Window = TimeSpan.FromMinutes(contactWindowMinutes),
            QueueLimit = 0
        });
    });
    opt.OnRejected = async (ctx, ct) =>
    {
        ctx.HttpContext.Response.Headers["Retry-After"] = "10";
        ctx.HttpContext.Response.Headers["X-Correlation-Id"] = ctx.HttpContext.TraceIdentifier;
        await ctx.HttpContext.Response.WriteAsJsonAsync(new { status = 429, title = "Too Many Requests", detail = "Rate limit exceeded", traceId = ctx.HttpContext.TraceIdentifier }, cancellationToken: ct);
    };
});

builder.Services.AddControllers().AddJsonOptions(o => o.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHealthChecks();

// S57.36 §13/§14: probe propiedad de Data.Async — el registro debe ocurrir ANTES de Build()
// (registrar después de Build no tiene efecto sobre la app construida).
builder.Services.AddInventaDatabaseProbeHostedService();

// S104: storage de archivos por CBP.Storage. La API autoriza y firma; los bytes viajan directo al storage.
// Storage:Provider elige el proveedor: Local (disco + URL firmadas, sin internet) o S3 (R2, B2, MinIO, AWS),
// cuyas opciones se validan al arrancar (fail-fast) solo cuando es el elegido.
builder.Services.AddCbpStorage();
// S106: variantes de imagen (CBP.Storage.Imaging) sobre el proveedor elegido.
builder.Services.AddCbpStorageImaging();
// S107 (P10): importación de imágenes por URL (RemoteUploadService de CBP.Storage), solo desde Storage:Remote:AllowedHosts.
builder.Services.Configure<RemoteUploadOptions>(builder.Configuration.GetSection("Storage:Remote"));
builder.Services.AddHttpClient<RemoteUploadService>(c => c.DefaultRequestHeaders.UserAgent.ParseAdd("InventaNet/1.0"));
var storageProviderName = builder.Configuration["Storage:Provider"] ?? "Local";
var storageLocal = string.Equals(storageProviderName, "Local", StringComparison.OrdinalIgnoreCase);
if (storageLocal)
    builder.Services.AddCbpStorageLocal(builder.Configuration.GetSection("Storage:Local"));
else if (string.Equals(storageProviderName, "S3", StringComparison.OrdinalIgnoreCase))
    builder.Services.AddCbpStorageS3(o => builder.Configuration.GetSection("Storage:S3").Bind(o));
else
    throw new InvalidOperationException("Storage:Provider debe ser 'Local' o 'S3'.");

// S105 (P9): barrido periódico del storage (purga pendientes vencidos, rechazados y eliminados; repara la ubicación
// público/privado). Sección opcional Storage:Barrido; sin ella rigen los valores de ArchivoBarridoOptions.
var barridoOptions = builder.Configuration.GetSection(ArchivoBarridoOptions.SectionName).Get<ArchivoBarridoOptions>() ?? new ArchivoBarridoOptions();
builder.Services.AddSingleton(barridoOptions);
builder.Services.AddHostedService<ArchivoBarridoHostedService>();

// PassPlat S136 (D10): Outbox genérico de CBP sobre dbo.Outbox de Inventa (sin eventos registrados todavía).
// Sección opcional Outbox; sin ella rigen los valores de OutboxOptions. El procesador exige la tabla al arrancar.
builder.Services.AddCBPEvents();
builder.Services.AddCbpOutbox<OutboxRepositoryAsync>();
builder.Services.AddCbpOutboxProcesador<OutboxProcessor>();

var app = builder.Build();

// S57.27: resolución temprana de PassPlatOptions — fail-fast + log sanitizado (sin secretos).
var passPlatOptions = app.Services.GetRequiredService<IOptions<PassPlatOptions>>().Value;
Console.WriteLine($"[PASSPLAT CONFIG] Validated=True BaseAddress={passPlatOptions.ApiBaseUrl} TimeoutMs={passPlatOptions.TimeoutMs}");

// S57.82 / S104: resolución temprana del storage (valida contenedores y opciones; fail-fast si es inválido).
var storage = app.Services.GetRequiredService<IStorageProvider>();
Console.WriteLine($"[STORAGE] Provider={storage.Name}");

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// S57.83-G3: el namespace privado nunca se sirve estáticamente. Desde S104 el contenedor privado
// vive fuera de wwwroot y se lee solo con URL firmada; esta guarda se conserva por si quedara algo
// bajo wwwroot/uploads/private. ANTES de UseStaticFiles porque el static short-circuita.
app.Use(async (ctx, next) =>
{
    if (ctx.Request.Path.StartsWithSegments("/uploads/private", StringComparison.OrdinalIgnoreCase))
    {
        ctx.Response.StatusCode = StatusCodes.Status404NotFound;
        return;
    }
    await next();
});
// S106: CORS ANTES de UseStaticFiles. El static cortocircuita el pipeline y, si va primero, los objetos
// públicos de /uploads (original y variantes) salen sin cabeceras CORS y el navegador no puede leerlos
// desde otra origen (tienda en :5003, consola de la web en :57186).
app.UseCors("StorePublic");
app.UseStaticFiles();
// S107: los contenedores públicos del proveedor Local viven fuera del repo (Storage:Local:Containers:*:Root);
// se publican aquí bajo su PublicBasePath (/uploads), con el mismo CORS de arriba.
if (storageLocal) app.UseCbpStorageLocalPublicFiles();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseCrossTenantValidation();
// S67: "Authenticated" es el default, pero no pisa controladores con politica
// explicita (StoreAuthController -> "Public"): la convencion global se agrega
// ultima a la metadata y ganaria por orden si se usara RequireRateLimiting.
app.MapControllers().Add(e =>
{
    if (!e.Metadata.OfType<EnableRateLimitingAttribute>().Any())
        e.Metadata.Add(new EnableRateLimitingAttribute("Authenticated"));
});
// S104: endpoints PUT/GET firmados del proveedor local (la URL firmada es la credencial; sin JWT de la app).
if (storageLocal) app.MapCbpStorageLocal();
app.MapHealthChecks("/health").DisableRateLimiting();
// S95 D4 / K5.1: publicación del catálogo de acceso en PassPlat, solo usuario maestro (ver y publicar).
app.MapAccesoEndpoints("api/admin/acceso", PoliticaUsuarioMaestro, PoliticaUsuarioMaestro).RequireRateLimiting("Authenticated");
// S100 D6: el menú de la app para el usuario, tal como lo entrega PassPlat.
app.MapMenuAcceso("api/acceso/menu").RequireRateLimiting("Authenticated");
// S105: barrido manual del storage (mismo servicio que el periódico), solo usuario maestro.
app.MapPost("api/admin/storage/barrido", ArchivoBarridoHostedService.BarrerManualAsync)
    .RequireAuthorization(PoliticaUsuarioMaestro)
    .RequireRateLimiting("Authenticated");
if (app.Environment.IsDevelopment())
{
    app.MapGet("/debug/jwt", () => Results.Json(new { Configured = jwtOptions.SecretKey.Length >= 32, Issuer = jwtOptions.Issuer, Audiences = jwtOptions.ValidAudiences, Env = builder.Environment.EnvironmentName }));
    app.MapGet("/debug/validate", (string token) =>
{
    try
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parms = JwtBearerConfiguration.BuildValidationParameters(jwtOptions);
        var principal = handler.ValidateToken(token, parms, out var vt);
        return Results.Json(new { ok = true, claims = principal.Claims.Select(c => new { c.Type, c.Value }) });
    }
    catch (Exception ex)
    {
        return Results.Json(new { ok = false, error = "Token validation failed", type = ex.GetType().Name });
    }
    });
    app.MapGet("/debug/sync", async (string token, ITenantSyncServiceAsync tSync, IUsuarioSyncService uSync, IUsuarioTenantSyncService utSync) =>
{
    try
    {
        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
        var parms = JwtBearerConfiguration.BuildValidationParameters(jwtOptions);
        var principal = handler.ValidateToken(token, parms, out var vt);
        var sub = principal.FindFirst("sub")?.Value ?? principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        var tid = principal.FindFirst("tid")?.Value;
        var email = principal.FindFirst("email")?.Value;
        var name = principal.FindFirst("name")?.Value;
        if (!int.TryParse(sub, out var idUsuario)) return Results.Json(new { ok=false, step="parse sub", sub });
        if (!int.TryParse(tid, out var idTenantPassPlat)) return Results.Json(new { ok=false, step="parse tid", tid });
        var tenantResult = await tSync.ResolveLocalTenantIdAsync(idTenantPassPlat);
        int idTenantLocal;
        if (!tenantResult.IsSuccess)
        {
            var create = await tSync.EnsureTenantAsync(idTenantPassPlat, $"T{idTenantPassPlat}", $"Tenant {idTenantPassPlat}");
            if (!create.IsSuccess) return Results.Json(new { ok=false, step="ensure tenant", error="Tenant sync failed" });
            idTenantLocal = create.Value.IdTenant;
        }
        else idTenantLocal = tenantResult.Value;
        var parts = (name ?? "").Split(' ', 2);
        var uRes = await uSync.EnsureUsuarioAsync(idUsuario, $"user_{sub}", email, parts.ElementAtOrDefault(0), parts.ElementAtOrDefault(1), true, false);
        if (!uRes.IsSuccess) return Results.Json(new { ok=false, step="ensure usuario", error="Usuario sync failed" });
        var utRes = await utSync.EnsureAsync(idUsuario, idTenantLocal, true, false);
        if (!utRes.IsSuccess) return Results.Json(new { ok=false, step="ensure ut", error="Sync failed" });
        return Results.Json(new { ok=true, idTenantLocal, idUsuario, tid, sub });
    }
    catch (Exception ex)
    {
        return Results.Json(new { ok = false, error = "Sync validation failed", type = ex.GetType().Name });
    }
    });
}

app.Run();

public partial class Program
{
    /// <summary>S95 K5.1: política del usuario maestro de PassPlat (claim is_system).</summary>
    public const string PoliticaUsuarioMaestro = "USUARIO_MAESTRO";
}
