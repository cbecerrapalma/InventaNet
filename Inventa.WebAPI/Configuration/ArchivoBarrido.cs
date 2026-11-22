using Inventa.Application.Async.Services;

namespace Inventa.WebAPI.Configuration;

/// <summary>
/// S105: opciones del barrido del storage. Sección opcional <c>Storage:Barrido</c>; sin ella rigen estos valores.
/// </summary>
public sealed class ArchivoBarridoOptions
{
    public const string SectionName = "Storage:Barrido";

    /// <summary>La URL de subida dura 10 min (<c>DirectUploadService.UploadTtl</c>): la gracia nunca baja de 15.</summary>
    public const int GraciaMinimaMinutos = 15;

    public bool Habilitado { get; set; } = true;
    public int RetrasoInicialMinutos { get; set; } = 1;
    public int IntervaloMinutos { get; set; } = 60;
    public int GraciaMinutos { get; set; } = 60;
    public int Lote { get; set; } = 200;

    /// <summary>Lo creado o cerrado antes de este instante se puede purgar.</summary>
    public DateTime Limite(DateTime ahora) => ahora.AddMinutes(-Math.Max(GraciaMinimaMinutos, GraciaMinutos));

    public int LoteEfectivo => Math.Clamp(Lote, 1, 1000);
}

/// <summary>
/// S105 (P9): corre <see cref="IArchivoBarridoServiceAsync"/> al arrancar (tras el retraso) y cada intervalo, con su propio scope.
/// </summary>
public sealed class ArchivoBarridoHostedService : BackgroundService
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ArchivoBarridoOptions _opciones;
    private readonly ILogger<ArchivoBarridoHostedService> _log;

    public ArchivoBarridoHostedService(IServiceScopeFactory scopes, ArchivoBarridoOptions opciones, ILogger<ArchivoBarridoHostedService> log)
    {
        _scopes = scopes;
        _opciones = opciones;
        _log = log;
    }

    /// <summary>Barrido manual (endpoint del usuario maestro): una vuelta con las mismas opciones.</summary>
    public static async Task<IResult> BarrerManualAsync(IArchivoBarridoServiceAsync barrido, ArchivoBarridoOptions opciones, CancellationToken ct)
    {
        var r = await barrido.BarrerAsync(opciones.Limite(DateTime.Now), opciones.LoteEfectivo, ct);
        return r.IsSuccess
            ? Results.Ok(r.Value)
            : Results.Problem(r.Error!.Message, statusCode: r.Error.HttpStatusCode ?? StatusCodes.Status500InternalServerError);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (!_opciones.Habilitado) return;
        try
        {
            await Task.Delay(TimeSpan.FromMinutes(Math.Max(0, _opciones.RetrasoInicialMinutos)), stoppingToken);
            using var timer = new PeriodicTimer(TimeSpan.FromMinutes(Math.Max(1, _opciones.IntervaloMinutos)));
            do
            {
                await BarrerUnaVezAsync(stoppingToken);
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Apagado normal del host.
        }
    }

    private async Task BarrerUnaVezAsync(CancellationToken ct)
    {
        try
        {
            await using var scope = _scopes.CreateAsyncScope();
            var barrido = scope.ServiceProvider.GetRequiredService<IArchivoBarridoServiceAsync>();
            var r = await barrido.BarrerAsync(_opciones.Limite(DateTime.Now), _opciones.LoteEfectivo, ct);
            if (r.IsSuccess)
                _log.LogInformation("ARCHIVO_BARRIDO: {Purgados} purgados, {Fallidos} fallidos, {Reubicados} reubicados", r.Value.Purgados, r.Value.Fallidos, r.Value.Reubicados);
            else
                _log.LogWarning("ARCHIVO_BARRIDO_ERROR: {Code}", r.Error!.Code);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            // Estrategia: una vuelta fallida (BD o storage caídos) no detiene el host ni el servicio; la próxima reintenta.
            _log.LogWarning(ex, "ARCHIVO_BARRIDO_ERROR: vuelta fallida; se reintenta en la próxima");
        }
    }
}
