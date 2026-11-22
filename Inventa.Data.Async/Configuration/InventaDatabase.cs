// S57.38: fuente canonica unica ConnectionStrings:InventaDb (appsettings.json /
// appsettings.Development.json). Sin variables de entorno para BBDD, sin cadenas
// alternativas, sin fallback. Data.Async es el unico propietario
// de la resolucion (§19: una sola ruta). Secretos nunca en log.
using CBP.Data.Asynchronous;
using Inventa.Data.Context;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Inventa.Data.Async.Configuration;

public static class InventaDatabase
{
    public const string ConnectionName = "InventaDb";

    /// <summary>
    /// Registra DbContext + UoW async para el backend desde ConnectionStrings:InventaDb.
    /// Lanza si InventaDb falta, esta vacio o malformado (§11/§12).
    /// </summary>
    public static IServiceCollection AddInventaDatabase(this IServiceCollection services, IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(cfg);

        var cs = ResolveConnectionString(cfg);
        services.AddDbContext<InventaDbContext>(opt => opt.UseSqlServer(cs));
        services.AddUnitOfWorkAsync<InventaDbContext>();
        return services;
    }

    /// <summary>
    /// UNICA ruta de resolucion (§19). Lee Configuration.GetConnectionString("InventaDb").
    /// </summary>
    public static string ResolveConnectionString(IConfiguration cfg)
    {
        ArgumentNullException.ThrowIfNull(cfg);

        var cs = cfg.GetConnectionString(ConnectionName);
        if (string.IsNullOrWhiteSpace(cs))
            throw new InvalidOperationException("ConnectionStrings:InventaDb ausente o vacio en appsettings. Sin fallback (S57.38).");
        SqlConnectionStringBuilder b;
        try
        {
            b = new SqlConnectionStringBuilder(cs);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("ConnectionStrings:InventaDb malformado.", ex);
        }
        if (string.IsNullOrWhiteSpace(b.DataSource) || string.IsNullOrWhiteSpace(b.InitialCatalog))
            throw new InvalidOperationException("ConnectionStrings:InventaDb sin servidor/base utilizable.");
        Console.WriteLine($"[DB CONFIG] ConnectionName=InventaDb Server={b.DataSource} Database={b.InitialCatalog}");
        return cs;
    }

    public static async Task ProbeAsync(IConfiguration cfg, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(cfg);

        var cs = ResolveConnectionString(cfg);
        await using var c = new SqlConnection(cs);
        await c.OpenAsync(ct);
        // S125 D4: sin T-SQL en strings (AGENTS.md §6); la conexión abierta informa servidor y base.
        var server = c.DataSource;
        var db = c.Database;
        Console.WriteLine($"[DB PROBE] ConnectionName=InventaDb Server={server} Database={db} OK");
        if (!string.Equals(db, "Inventa", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Probe: Database={db} no es Inventa.");
    }

    /// <summary>
    /// §17: Encapsula ProbeAsync en Data.Async — registra IHostedService que ejecuta el probe al iniciar.
    /// Resuelve la conexion desde IConfiguration (ruta unica); sin targets ni env SQL.
    /// Program.cs llama a builder.Services.AddInventaDatabaseProbeHostedService() y NO llama a ProbeAsync directamente.
    /// </summary>
    public static IServiceCollection AddInventaDatabaseProbeHostedService(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddHostedService<DatabaseProbeHostedService>();
        return services;
    }

    private sealed class DatabaseProbeHostedService : Microsoft.Extensions.Hosting.IHostedService
    {
        private readonly IServiceProvider _sp;
        public DatabaseProbeHostedService(IServiceProvider sp) => _sp = sp;

        public async Task StartAsync(CancellationToken ct)
        {
            using var scope = _sp.CreateScope();
            var cfg = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            await ProbeAsync(cfg, ct);
        }

        public Task StopAsync(CancellationToken ct) => Task.CompletedTask;
    }
}
