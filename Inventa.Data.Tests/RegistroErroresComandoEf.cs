using System.Collections.Concurrent;
using System.Data.Common;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S95 (diagnóstico): registra cada comando SQL que falla en CUALQUIER DbContext de esta suite
/// (texto SQL, parámetros, número de error SQL y prueba en curso) en
/// <c>bin/…/Diagnostico/ef-command-errors.log</c> y en la salida de la prueba.
/// Motivo: <c>S931_PreciosGrillaDataTests.Rendimiento_Vestuario_InformaP95</c> falló una vez con
/// «Incorrect syntax near ')'» y no se pudo reproducir sin el SQL. Se engancha al DiagnosticListener
/// de EF Core, así no hace falta tocar cada AddDbContext/NewContext. Solo lee; no ejecuta SQL.
/// </summary>
internal static class RegistroErroresComandoEf
{
    public static readonly string Archivo = Path.Combine(AppContext.BaseDirectory, "Diagnostico", "ef-command-errors.log");

    private static readonly ConcurrentQueue<string> Recientes = new();
    private static readonly object Bloqueo = new();
    private static int _instalado;

    /// <summary>Últimas entradas registradas en este proceso (para pruebas).</summary>
    public static IReadOnlyCollection<string> EntradasRecientes => Recientes.ToArray();

    [ModuleInitializer]
    internal static void Instalar()
    {
        if (Interlocked.Exchange(ref _instalado, 1) == 1) return;
        DiagnosticListener.AllListeners.Subscribe(new ObservadorListeners());
    }

    /// <summary>Formato de una entrada: separado del enganche para poder probarlo.</summary>
    internal static string Formatear(string? prueba, DbCommand? comando, Exception error)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"==== {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff} | prueba: {prueba ?? "(desconocida)"}");
        sb.AppendLine($"Error SQL: {NumeroError(error)} | {error.GetType().Name}: {error.Message}");
        if (comando is not null)
        {
            foreach (DbParameter p in comando.Parameters)
                sb.AppendLine($"  {p.ParameterName} ({p.DbType}) = {(p.Value is null or DBNull ? "NULL" : p.Value)}");
            sb.AppendLine("SQL:");
            sb.AppendLine(comando.CommandText);
        }
        return sb.ToString();
    }

    internal static string NumeroError(Exception error)
    {
        for (var e = error; e is not null; e = e.InnerException)
            if (e is SqlException sql) return $"{sql.Number} (clase {sql.Class}, estado {sql.State})";
        return "n/d";
    }

    private static void Registrar(CommandErrorEventData datos)
    {
        var prueba = TestContext.Current.Test?.TestDisplayName;
        var entrada = Formatear(prueba, datos.Command, datos.Exception);
        Recientes.Enqueue(entrada);
        while (Recientes.Count > 50 && Recientes.TryDequeue(out _)) { }
        try
        {
            lock (Bloqueo)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(Archivo)!);
                File.AppendAllText(Archivo, entrada + Environment.NewLine);
            }
        }
        catch (IOException)
        {
            // El registro nunca debe tumbar la prueba; la salida de la prueba conserva la entrada.
        }
        TestContext.Current.TestOutputHelper?.WriteLine(entrada);
    }

    private sealed class ObservadorListeners : IObserver<DiagnosticListener>
    {
        public void OnNext(DiagnosticListener listener)
        {
            if (listener.Name == DbLoggerCategory.Name)
                listener.Subscribe(new ObservadorEventos(), nombre => nombre == RelationalEventId.CommandError.Name);
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }

    private sealed class ObservadorEventos : IObserver<KeyValuePair<string, object?>>
    {
        public void OnNext(KeyValuePair<string, object?> evento)
        {
            if (evento.Value is CommandErrorEventData datos) Registrar(datos);
        }

        public void OnCompleted() { }
        public void OnError(Exception error) { }
    }
}
