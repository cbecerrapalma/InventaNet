using CBP.Data.Abstractions;
using CBP.Logging;
using CBP.Logging.Configuration;
using CBP.Logging.Interfaces;
using CBP.Logging.Models;
using CBP.Results;
using CBP.Results.Errors;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Options;
using System.Data;
using System.Diagnostics;
using System.Reflection;

namespace CBP.Data.Synchronous;

/// <summary>
/// Implementación sincrónica de IRawQueryRepositorySyncSqlServer.
/// Arquitectura de convergencia: toda la superficie pública (RawParameter y SqlParameter[])
/// converge en el mismo núcleo ADO.NET interno.
///
/// Novedades respecto a la versión anterior:
///   - SpResult&lt;T&gt; incluye ReturnValue capturado automáticamente.
///   - QuerySPMultiple: lee múltiples result sets con reader.NextResult().
///   - QuerySPMultipleRaw: ídem con SqlParameter[].
/// </summary>
internal sealed class RawQueryRepositorySync : IRawQueryRepositorySyncSqlServer
{
    private readonly DbContext _context;
    private readonly ILoggerService? _logger;
    private readonly LoggingOptions _options;

    public RawQueryRepositorySync(
        DbContext context,
        ILoggerService? logger = null,
        IOptions<LoggingOptions>? options = null)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _logger = logger;
        _options = options?.Value ?? new LoggingOptions();
    }

    // =========================================================================
    // SUPERFICIE BASE — RawParameter
    // =========================================================================

    public Result<IReadOnlyList<T>> QuerySP<T>(
        string spName, IEnumerable<RawParameter>? parameters = null)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<IReadOnlyList<T>>(spName);
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                ExecuteReader<T>(spName, CommandType.StoredProcedure, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>(spName, ex); }
    }

    public Result<SpResult<T>> QuerySPWithOutput<T>(
        string spName, IEnumerable<RawParameter>? parameters = null)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpResult<T>>(spName);
        try
        {
            return Result<SpResult<T>>.Success(
                ExecuteReaderWithOutput<T>(spName, CommandType.StoredProcedure, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<SpResult<T>>(spName, ex); }
    }

    public Result<int> ExecuteSP(string spName, IEnumerable<RawParameter>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<int>(spName);
        try
        {
            return Result<int>.Success(
                ExecuteNonQuery(spName, CommandType.StoredProcedure, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<int>(spName, ex); }
    }

    public Result<T?> ScalarSP<T>(string spName, IEnumerable<RawParameter>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<T?>(spName);
        try
        {
            return Result<T?>.Success(
                ConvertScalar<T>(ExecuteScalar(spName, CommandType.StoredProcedure, ToSqlParams(parameters))));
        }
        catch (Exception ex) { return Fail<T?>(spName, ex); }
    }

    public Result<SpMultiResult<T1, T2>> QuerySPMultiple<T1, T2>(
        string spName, IEnumerable<RawParameter>? parameters = null)
        where T1 : class, new()
        where T2 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2>>.Success(
                ExecuteMultiReader<T1, T2>(spName, CommandType.StoredProcedure, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2>>(spName, ex); }
    }

    public Result<SpMultiResult<T1, T2, T3>> QuerySPMultiple<T1, T2, T3>(
        string spName, IEnumerable<RawParameter>? parameters = null)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2, T3>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2, T3>>.Success(
                ExecuteMultiReader<T1, T2, T3>(spName, CommandType.StoredProcedure, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2, T3>>(spName, ex); }
    }

    public Result<IReadOnlyList<T>> QuerySql<T>(
        string sql, IEnumerable<RawParameter>? parameters = null)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<IReadOnlyList<T>>("QuerySql");
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                ExecuteReader<T>(sql, CommandType.Text, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>("QuerySql", ex); }
    }

    public Result<int> ExecuteSql(string sql, IEnumerable<RawParameter>? parameters = null)
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<int>("ExecuteSql");
        try
        {
            return Result<int>.Success(
                ExecuteNonQuery(sql, CommandType.Text, ToSqlParams(parameters)));
        }
        catch (Exception ex) { return Fail<int>("ExecuteSql", ex); }
    }

    // =========================================================================
    // SUPERFICIE RAW — SqlParameter[] (TVPs, UDTs, Decimal exacto, etc.)
    // =========================================================================

    public Result<IReadOnlyList<T>> QuerySPRaw<T>(string spName, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<IReadOnlyList<T>>(spName);
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                ExecuteReader<T>(spName, CommandType.StoredProcedure, parameters));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>(spName, ex); }
    }

    public Result<SpResult<T>> QuerySPRawWithOutput<T>(string spName, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpResult<T>>(spName);
        try
        {
            return Result<SpResult<T>>.Success(
                ExecuteReaderWithOutput<T>(spName, CommandType.StoredProcedure, parameters));
        }
        catch (Exception ex) { return Fail<SpResult<T>>(spName, ex); }
    }

    public Result<int> ExecuteSPRaw(string spName, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<int>(spName);
        try
        {
            return Result<int>.Success(
                ExecuteNonQuery(spName, CommandType.StoredProcedure, parameters));
        }
        catch (Exception ex) { return Fail<int>(spName, ex); }
    }

    public Result<T?> ScalarSPRaw<T>(string spName, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<T?>(spName);
        try
        {
            return Result<T?>.Success(
                ConvertScalar<T>(ExecuteScalar(spName, CommandType.StoredProcedure, parameters)));
        }
        catch (Exception ex) { return Fail<T?>(spName, ex); }
    }

    public Result<SpMultiResult<T1, T2>> QuerySPMultipleRaw<T1, T2>(
        string spName, params SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2>>.Success(
                ExecuteMultiReader<T1, T2>(spName, CommandType.StoredProcedure, parameters));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2>>(spName, ex); }
    }

    public Result<SpMultiResult<T1, T2, T3>> QuerySPMultipleRaw<T1, T2, T3>(
        string spName, params SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2, T3>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2, T3>>.Success(
                ExecuteMultiReader<T1, T2, T3>(spName, CommandType.StoredProcedure, parameters));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2, T3>>(spName, ex); }
    }

    public Result<IReadOnlyList<T>> QuerySqlRaw<T>(string sql, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<IReadOnlyList<T>>("QuerySqlRaw");
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                ExecuteReader<T>(sql, CommandType.Text, parameters));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>("QuerySqlRaw", ex); }
    }

    public Result<int> ExecuteSqlRaw(string sql, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<int>("ExecuteSqlRaw");
        try
        {
            return Result<int>.Success(
                ExecuteNonQuery(sql, CommandType.Text, parameters));
        }
        catch (Exception ex) { return Fail<int>("ExecuteSqlRaw", ex); }
    }

    // =========================================================================
    // NÚCLEO ADO.NET — todos los métodos públicos convergen aquí
    // =========================================================================

    private IReadOnlyList<T> ExecuteReader<T>(
        string commandText, CommandType commandType, SqlParameter[] parameters)
        where T : class, new()
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd    = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();
            using var reader = cmd.ExecuteReader();
            return MapReader<T>(reader);
        });
    }

    private SpResult<T> ExecuteReaderWithOutput<T>(
        string commandText, CommandType commandType, SqlParameter[] parameters)
        where T : class, new()
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();

            using var reader = cmd.ExecuteReader();
            var rows = MapReader<T>(reader);
            reader.Close();

            var outputs     = CollectOutputs(cmd.Parameters);
            var returnValue = ReadReturnValue(cmd.Parameters);

            return new SpResult<T>(rows, outputs, reader.RecordsAffected, returnValue);
        });
    }

    /// <summary>
    /// Lee dos result sets consecutivos con NextResult().
    /// El SP debe emitir exactamente dos SELECT en el mismo batch.
    /// </summary>
    private SpMultiResult<T1, T2> ExecuteMultiReader<T1, T2>(
        string commandText, CommandType commandType, SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();

            using var reader = cmd.ExecuteReader();

            var set1 = MapReader<T1>(reader);

            // Avanzar al segundo result set
            if (!reader.NextResult())
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 2 result sets. " +
                    "Verifique que el SP emita exactamente dos SELECT.");

            var set2 = MapReader<T2>(reader);
            reader.Close();

            return new SpMultiResult<T1, T2>(
                set1, set2,
                CollectOutputs(cmd.Parameters),
                ReadReturnValue(cmd.Parameters));
        });
    }

    private SpMultiResult<T1, T2, T3> ExecuteMultiReader<T1, T2, T3>(
        string commandText, CommandType commandType, SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();

            using var reader = cmd.ExecuteReader();

            var set1 = MapReader<T1>(reader);

            if (!reader.NextResult())
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 2 result sets.");

            var set2 = MapReader<T2>(reader);

            if (!reader.NextResult())
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 3 result sets.");

            var set3 = MapReader<T3>(reader);
            reader.Close();

            return new SpMultiResult<T1, T2, T3>(
                set1, set2, set3,
                CollectOutputs(cmd.Parameters),
                ReadReturnValue(cmd.Parameters));
        });
    }

    private int ExecuteNonQuery(string commandText, CommandType commandType, SqlParameter[] parameters)
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();
            return cmd.ExecuteNonQuery();
        });
    }

    private object? ExecuteScalar(string commandText, CommandType commandType, SqlParameter[] parameters)
    {
        return Measure(commandText, commandType, () =>
        {
            using var cmd = BuildCommand(commandText, commandType, parameters);
            OpenIfClosed();
            return cmd.ExecuteScalar();
        });
    }

    // =========================================================================
    // MEDICIÓN SQL LENTO (S37) — canal RawQuery ADO.NET (disjunto del interceptor EF)
    // =========================================================================

    private TResult Measure<TResult>(
        string commandText, CommandType commandType, Func<TResult> operation)
    {
        if (_logger == null)
            return operation();

        var sw = Stopwatch.StartNew();
        try
        {
            return operation();
        }
        finally
        {
            sw.Stop();
            EmitIfSlow(commandText, commandType, sw.Elapsed.TotalMilliseconds);
        }
    }

    internal void EmitIfSlow(string commandText, CommandType commandType, double elapsedMs)
    {
        var threshold = _options.SqlSlowQueryThresholdMs;
        if (threshold <= 0 || elapsedMs < threshold) return;

        var (procedureName, commandName) = SanitizeNames(commandText, commandType);

        _logger!.LogWarning(new LogEvent
        {
            EventName = LoggingEvents.SqlSlowQuery,
            Scope = LoggingScopes.Sql,
            Message = $"SQL lento detectado: {commandName} ({elapsedMs:F1} ms) — supera {threshold} ms",
            Properties = new Dictionary<string, object?>
            {
                [LoggingPropertyNames.Category] = LoggingCategories.DataSql,
                [LoggingPropertyNames.Operation] = LoggingOperations.Execute,
                [LoggingPropertyNames.Source] = LoggingSources.SqlServer,
                [LoggingPropertyNames.ElapsedMs] = Math.Round(elapsedMs, 1),
                [LoggingPropertyNames.CommandType] = MapCommandType(commandType),
                [LoggingPropertyNames.ProcedureName] = procedureName,
                [LoggingPropertyNames.CommandName] = commandName
            }
        });
    }

    internal static (string procedureName, string commandName) SanitizeNames(
        string commandText, CommandType commandType)
    {
        if (string.IsNullOrWhiteSpace(commandText))
            return (string.Empty, string.Empty);

        var text = commandText.Trim();
        var name = commandType == CommandType.StoredProcedure
            ? SanitizeIdentifier(text)
            : string.Concat(text.TakeWhile(char.IsLetterOrDigit));

        return (Truncate(name, 128), Truncate(name, 128));
    }

    private static string SanitizeIdentifier(string token)
    {
        var sb = new System.Text.StringBuilder(token.Length);
        foreach (var ch in token)
        {
            if (char.IsLetterOrDigit(ch) || ch is '_' or '.' or '[' or ']')
                sb.Append(ch);
            else
                break;
        }
        return sb.ToString();
    }

    internal static string MapCommandType(CommandType commandType) => commandType switch
    {
        CommandType.StoredProcedure => "storedProcedure",
        CommandType.Text => "text",
        CommandType.TableDirect => "tableDirect",
        _ => "unknown"
    };

    private static string Truncate(string value, int max) =>
        value.Length <= max ? value : value[..max];

    // =========================================================================
    // HELPERS
    // =========================================================================

    private SqlCommand BuildCommand(
        string commandText, CommandType commandType, SqlParameter[] parameters)
    {
        var connection  = (SqlConnection)_context.Database.GetDbConnection();
        var transaction = _context.Database.CurrentTransaction?.GetDbTransaction() as SqlTransaction;

        var cmd = new SqlCommand(commandText, connection, transaction)
        {
            CommandType    = commandType,
            CommandTimeout = _context.Database.GetCommandTimeout() ?? 30
        };

        // Agregar siempre el parámetro RETURN VALUE para capturarlo automáticamente.
        // Debe ser el primero en la colección (convención SQL Server / ADO.NET).
        var retVal = new SqlParameter("@RETURN_VALUE", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };
        cmd.Parameters.Add(retVal);

        foreach (var p in parameters)
            cmd.Parameters.Add(p);

        return cmd;
    }

    private void OpenIfClosed()
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State == ConnectionState.Closed)
            conn.Open();
    }

    private static SqlParameter[] ToSqlParams(IEnumerable<RawParameter>? parameters)
    {
        if (parameters == null) return [];

        return parameters.Select(p =>
        {
            var sp = new SqlParameter(p.Name, p.Value ?? DBNull.Value);
            if (p.ColumnDbType.HasValue) sp.DbType = p.ColumnDbType.Value;
            if (p.Size.HasValue)   sp.Size   = p.Size.Value;
            sp.Direction = p.IsOutput ? ParameterDirection.Output : ParameterDirection.Input;
            return sp;
        }).ToArray();
    }

    private static IReadOnlyDictionary<string, object?> CollectOutputs(
        SqlParameterCollection sqlParams)
    {
        var dict = new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        foreach (SqlParameter p in sqlParams)
        {
            if (p.Direction is ParameterDirection.Output or ParameterDirection.InputOutput)
                dict[p.ParameterName] = p.Value == DBNull.Value ? null : p.Value;
        }
        return dict;
    }

    private static int ReadReturnValue(SqlParameterCollection sqlParams)
    {
        foreach (SqlParameter p in sqlParams)
        {
            if (p.Direction == ParameterDirection.ReturnValue)
                return p.Value is int v ? v : 0;
        }
        return 0;
    }

    private static IReadOnlyList<T> MapReader<T>(IDataReader reader)
        where T : class, new()
    {
        var props     = GetCachedProperties<T>();
        var result    = new List<T>();
        var columnMap = new Dictionary<int, PropertyInfo>(reader.FieldCount);

        for (int i = 0; i < reader.FieldCount; i++)
        {
            if (props.TryGetValue(reader.GetName(i), out var prop))
                columnMap[i] = prop;
        }

        while (reader.Read())
        {
            var obj = new T();
            foreach (var (idx, prop) in columnMap)
            {
                var val = reader.GetValue(idx);
                if (val == DBNull.Value) continue;
                var targetType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                prop.SetValue(obj, Convert.ChangeType(val, targetType));
            }
            result.Add(obj);
        }

        return result.AsReadOnly();
    }

    private static T? ConvertScalar<T>(object? raw)
    {
        if (raw == null || raw == DBNull.Value) return default;
        return (T?)Convert.ChangeType(raw, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T));
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary
        <Type, Dictionary<string, PropertyInfo>> _propertyCache = new();

    private static Dictionary<string, PropertyInfo> GetCachedProperties<T>()
        => _propertyCache.GetOrAdd(typeof(T), t =>
            t.GetProperties(BindingFlags.Public | BindingFlags.Instance)
             .Where(p => p.CanWrite)
             .ToDictionary(p => p.Name, p => p, StringComparer.OrdinalIgnoreCase));

    private static Result<TResult> Fail<TResult>(string operation, Exception? ex = null)
    {
        if (ex == null)
            return Result<TResult>.Failure(CommonErrors.BadRequest.MissingParameter(operation));
        return Result<TResult>.Failure(Error.FromException(ex, "SP_EXECUTION_ERROR")
            .WithDetail("Operation", operation));
    }
}
