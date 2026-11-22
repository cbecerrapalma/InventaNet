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

namespace CBP.Data.Asynchronous;

/// <summary>
/// Implementación asíncrona de IRawQueryRepositoryAsyncSqlServer.
/// Misma arquitectura de convergencia que el equivalente sync:
///   · Superficie base (RawParameter) → ToSqlParams() → núcleo ADO.NET.
///   · Superficie Raw (SqlParameter[]) → núcleo ADO.NET directamente.
///
/// SpResult incluye ReturnValue capturado automáticamente.
/// QuerySPMultiple lee múltiples result sets con reader.NextResultAsync().
/// </summary>
internal sealed class RawQueryRepositoryAsync : IRawQueryRepositoryAsyncSqlServer
{
    private readonly DbContext _context;
    private readonly ILoggerService? _logger;
    private readonly LoggingOptions _options;

    public RawQueryRepositoryAsync(
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

    public async Task<Result<IReadOnlyList<T>>> QuerySPAsync<T>(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<IReadOnlyList<T>>(spName);
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                await ExecuteReaderAsync<T>(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>(spName, ex); }
    }

    public async Task<Result<SpResult<T>>> QuerySPWithOutputAsync<T>(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpResult<T>>(spName);
        try
        {
            return Result<SpResult<T>>.Success(
                await ExecuteReaderWithOutputAsync<T>(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<SpResult<T>>(spName, ex); }
    }

    public async Task<Result<int>> ExecuteSPAsync(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<int>(spName);
        try
        {
            return Result<int>.Success(
                await ExecuteNonQueryAsync(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<int>(spName, ex); }
    }

    public async Task<Result<T?>> ScalarSPAsync<T>(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<T?>(spName);
        try
        {
            return Result<T?>.Success(
                ConvertScalar<T>(await ExecuteScalarAsync(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct)));
        }
        catch (Exception ex) { return Fail<T?>(spName, ex); }
    }

    public async Task<Result<SpMultiResult<T1, T2>>> QuerySPMultipleAsync<T1, T2>(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
        where T1 : class, new()
        where T2 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2>>.Success(
                await ExecuteMultiReaderAsync<T1, T2>(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2>>(spName, ex); }
    }

    public async Task<Result<SpMultiResult<T1, T2, T3>>> QuerySPMultipleAsync<T1, T2, T3>(
        string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2, T3>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2, T3>>.Success(
                await ExecuteMultiReaderAsync<T1, T2, T3>(spName, CommandType.StoredProcedure, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2, T3>>(spName, ex); }
    }

    public async Task<Result<IReadOnlyList<T>>> QuerySqlAsync<T>(
        string sql, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<IReadOnlyList<T>>("QuerySql");
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                await ExecuteReaderAsync<T>(sql, CommandType.Text, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>("QuerySql", ex); }
    }

    public async Task<Result<int>> ExecuteSqlAsync(
        string sql, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<int>("ExecuteSql");
        try
        {
            return Result<int>.Success(
                await ExecuteNonQueryAsync(sql, CommandType.Text, ToSqlParams(parameters), ct));
        }
        catch (Exception ex) { return Fail<int>("ExecuteSql", ex); }
    }

    // =========================================================================
    // SUPERFICIE RAW — SqlParameter[]
    // =========================================================================

    public async Task<Result<IReadOnlyList<T>>> QuerySPRawAsync<T>(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<IReadOnlyList<T>>(spName);
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                await ExecuteReaderAsync<T>(spName, CommandType.StoredProcedure, parameters, ct));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>(spName, ex); }
    }

    public async Task<Result<SpResult<T>>> QuerySPRawWithOutputAsync<T>(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpResult<T>>(spName);
        try
        {
            return Result<SpResult<T>>.Success(
                await ExecuteReaderWithOutputAsync<T>(spName, CommandType.StoredProcedure, parameters, ct));
        }
        catch (Exception ex) { return Fail<SpResult<T>>(spName, ex); }
    }

    public async Task<Result<int>> ExecuteSPRawAsync(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<int>(spName);
        try
        {
            return Result<int>.Success(
                await ExecuteNonQueryAsync(spName, CommandType.StoredProcedure, parameters, ct));
        }
        catch (Exception ex) { return Fail<int>(spName, ex); }
    }

    public async Task<Result<T?>> ScalarSPRawAsync<T>(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<T?>(spName);
        try
        {
            return Result<T?>.Success(
                ConvertScalar<T>(await ExecuteScalarAsync(spName, CommandType.StoredProcedure, parameters, ct)));
        }
        catch (Exception ex) { return Fail<T?>(spName, ex); }
    }

    public async Task<Result<SpMultiResult<T1, T2>>> QuerySPMultipleRawAsync<T1, T2>(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2>>.Success(
                await ExecuteMultiReaderAsync<T1, T2>(spName, CommandType.StoredProcedure, parameters, ct));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2>>(spName, ex); }
    }

    public async Task<Result<SpMultiResult<T1, T2, T3>>> QuerySPMultipleRawAsync<T1, T2, T3>(
        string spName, CancellationToken ct = default, params SqlParameter[] parameters)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        if (string.IsNullOrWhiteSpace(spName)) return Fail<SpMultiResult<T1, T2, T3>>(spName);
        try
        {
            return Result<SpMultiResult<T1, T2, T3>>.Success(
                await ExecuteMultiReaderAsync<T1, T2, T3>(spName, CommandType.StoredProcedure, parameters, ct));
        }
        catch (Exception ex) { return Fail<SpMultiResult<T1, T2, T3>>(spName, ex); }
    }

    public async Task<Result<IReadOnlyList<T>>> QuerySqlRawAsync<T>(
        string sql, CancellationToken ct = default, params SqlParameter[] parameters)
        where T : class, new()
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<IReadOnlyList<T>>("QuerySqlRaw");
        try
        {
            return Result<IReadOnlyList<T>>.Success(
                await ExecuteReaderAsync<T>(sql, CommandType.Text, parameters, ct));
        }
        catch (Exception ex) { return Fail<IReadOnlyList<T>>("QuerySqlRaw", ex); }
    }

    public async Task<Result<int>> ExecuteSqlRawAsync(
        string sql, CancellationToken ct = default, params SqlParameter[] parameters)
    {
        if (string.IsNullOrWhiteSpace(sql)) return Fail<int>("ExecuteSqlRaw");
        try
        {
            return Result<int>.Success(
                await ExecuteNonQueryAsync(sql, CommandType.Text, parameters, ct));
        }
        catch (Exception ex) { return Fail<int>("ExecuteSqlRaw", ex); }
    }

    // =========================================================================
    // NÚCLEO ADO.NET ASYNC
    // =========================================================================

    private async Task<IReadOnlyList<T>> ExecuteReaderAsync<T>(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
        where T : class, new()
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);
            await using var reader = await cmd.ExecuteReaderAsync(ct);
            return await MapReaderAsync<T>(reader, ct);
        });
    }

    private async Task<SpResult<T>> ExecuteReaderWithOutputAsync<T>(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
        where T : class, new()
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            var rows = await MapReaderAsync<T>(reader, ct);
            await reader.CloseAsync();

            return new SpResult<T>(
                rows,
                CollectOutputs(cmd.Parameters),
                reader.RecordsAffected,
                ReadReturnValue(cmd.Parameters));
        });
    }

    private async Task<SpMultiResult<T1, T2>> ExecuteMultiReaderAsync<T1, T2>(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
        where T1 : class, new()
        where T2 : class, new()
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            var set1 = await MapReaderAsync<T1>(reader, ct);

            if (!await reader.NextResultAsync(ct))
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 2 result sets.");

            var set2 = await MapReaderAsync<T2>(reader, ct);
            await reader.CloseAsync();

            return new SpMultiResult<T1, T2>(
                set1, set2,
                CollectOutputs(cmd.Parameters),
                ReadReturnValue(cmd.Parameters));
        });
    }

    private async Task<SpMultiResult<T1, T2, T3>> ExecuteMultiReaderAsync<T1, T2, T3>(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new()
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);

            await using var reader = await cmd.ExecuteReaderAsync(ct);

            var set1 = await MapReaderAsync<T1>(reader, ct);

            if (!await reader.NextResultAsync(ct))
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 2 result sets.");

            var set2 = await MapReaderAsync<T2>(reader, ct);

            if (!await reader.NextResultAsync(ct))
                throw new InvalidOperationException(
                    $"El SP '{commandText}' devolvió menos de 3 result sets.");

            var set3 = await MapReaderAsync<T3>(reader, ct);
            await reader.CloseAsync();

            return new SpMultiResult<T1, T2, T3>(
                set1, set2, set3,
                CollectOutputs(cmd.Parameters),
                ReadReturnValue(cmd.Parameters));
        });
    }

    private async Task<int> ExecuteNonQueryAsync(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);
            return await cmd.ExecuteNonQueryAsync(ct);
        });
    }

    private async Task<object?> ExecuteScalarAsync(
        string commandText, CommandType commandType, SqlParameter[] parameters, CancellationToken ct)
    {
        return await MeasureAsync(commandText, commandType, async () =>
        {
            await using var cmd = BuildCommand(commandText, commandType, parameters);
            await OpenIfClosedAsync(ct);
            return await cmd.ExecuteScalarAsync(ct);
        });
    }

    // =========================================================================
    // MEDICIÓN SQL LENTO (S37) — canal RawQuery ADO.NET (disjunto del interceptor EF)
    // =========================================================================

    private async Task<TResult> MeasureAsync<TResult>(
        string commandText, CommandType commandType, Func<Task<TResult>> operation)
    {
        if (_logger == null)
            return await operation();

        var sw = Stopwatch.StartNew();
        try
        {
            return await operation();
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

        // RETURN VALUE siempre presente — debe ser el primero (convención ADO.NET)
        cmd.Parameters.Add(new SqlParameter("@RETURN_VALUE", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        });

        foreach (var p in parameters)
            cmd.Parameters.Add(p);

        return cmd;
    }

    private async Task OpenIfClosedAsync(CancellationToken ct)
    {
        var conn = _context.Database.GetDbConnection();
        if (conn.State == ConnectionState.Closed)
            await ((SqlConnection)conn).OpenAsync(ct);
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

    private static async Task<IReadOnlyList<T>> MapReaderAsync<T>(
        System.Data.Common.DbDataReader reader, CancellationToken ct)
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

        while (await reader.ReadAsync(ct))
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
