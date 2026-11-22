using CBP.Results;
using System.Data;

namespace CBP.Data.Abstractions;

/// <summary>
/// Parámetro tipado para SPs y SQL raw.
/// Evita depender de SqlParameter (vendor-specific) en Abstractions.
/// </summary>
public sealed class RawParameter
{
    public string  Name      { get; }
    public object? Value     { get; }
    public DbType? ColumnDbType { get; }
    public int?    Size      { get; }
    public bool    IsOutput  { get; }

    public RawParameter(string name, object? value, DbType? dbType = null,
        int? size = null, bool isOutput = false)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del parámetro no puede estar vacío.", nameof(name));

        Name     = name.StartsWith('@') ? name : $"@{name}";
        Value    = value;
        ColumnDbType = dbType;
        Size     = size;
        IsOutput = isOutput;
    }

    public static RawParameter In(string name, object? value, DbType? dbType = null, int? size = null)
        => new(name, value, dbType, size, isOutput: false);

    public static RawParameter Out(string name, DbType dbType, int? size = null)
        => new(name, null, dbType, size, isOutput: true);

    public static RawParameter NVarChar(string name, string? value, int size = -1)
        => new(name, value, DbType.String, size);

    public static RawParameter Int(string name, int? value)
        => new(name, value, DbType.Int32);

    public static RawParameter BigInt(string name, long? value)
        => new(name, value, DbType.Int64);

    public static RawParameter Bit(string name, bool? value)
        => new(name, value, DbType.Boolean);

    public static RawParameter Date(string name, DateTime? value)
        => new(name, value, DbType.DateTime2);

    public static RawParameter Decimal(string name, decimal? value)
        => new(name, value, DbType.Decimal);
}

/// <summary>
/// Resultado de un SP que devuelve un único result set, con parámetros OUTPUT
/// y RETURN VALUE capturados automáticamente.
///
/// ReturnValue: código entero devuelto por RETURN en el SP (0 = éxito por convención
/// SQL Server). Disponible siempre, aunque el SP no tenga RETURN explícito.
/// </summary>
public sealed class SpResult<T>
{
    public IReadOnlyList<T>                    Rows         { get; }
    public IReadOnlyDictionary<string, object?> OutputValues { get; }
    public int                                  RowsAffected { get; }

    /// <summary>
    /// Valor devuelto por RETURN en el SP. 0 = éxito (convención SQL Server).
    /// Útil para SPs que comunican estado vía RETURN en lugar de OUTPUT o excepciones.
    /// </summary>
    public int ReturnValue { get; }

    public SpResult(
        IReadOnlyList<T> rows,
        IReadOnlyDictionary<string, object?> outputValues,
        int rowsAffected,
        int returnValue = 0)
    {
        Rows         = rows;
        OutputValues = outputValues;
        RowsAffected = rowsAffected;
        ReturnValue  = returnValue;
    }

    public TOut? GetOutput<TOut>(string paramName)
    {
        var key = paramName.StartsWith('@') ? paramName : $"@{paramName}";
        return OutputValues.TryGetValue(key, out var val) && val is TOut typed
            ? typed
            : default;
    }
}

/// <summary>
/// Resultado de un SP que devuelve DOS result sets en un solo roundtrip.
/// Elimina la necesidad de dos llamadas separadas para reportes con cabecera+detalle.
/// </summary>
public sealed class SpMultiResult<T1, T2>
{
    public IReadOnlyList<T1>                    Set1         { get; }
    public IReadOnlyList<T2>                    Set2         { get; }
    public IReadOnlyDictionary<string, object?> OutputValues { get; }
    public int                                  ReturnValue  { get; }

    public SpMultiResult(
        IReadOnlyList<T1> set1,
        IReadOnlyList<T2> set2,
        IReadOnlyDictionary<string, object?> outputValues,
        int returnValue = 0)
    {
        Set1         = set1;
        Set2         = set2;
        OutputValues = outputValues;
        ReturnValue  = returnValue;
    }

    public TOut? GetOutput<TOut>(string paramName)
    {
        var key = paramName.StartsWith('@') ? paramName : $"@{paramName}";
        return OutputValues.TryGetValue(key, out var val) && val is TOut typed
            ? typed
            : default;
    }
}

/// <summary>
/// Resultado de un SP que devuelve TRES result sets en un solo roundtrip.
/// </summary>
public sealed class SpMultiResult<T1, T2, T3>
{
    public IReadOnlyList<T1>                    Set1         { get; }
    public IReadOnlyList<T2>                    Set2         { get; }
    public IReadOnlyList<T3>                    Set3         { get; }
    public IReadOnlyDictionary<string, object?> OutputValues { get; }
    public int                                  ReturnValue  { get; }

    public SpMultiResult(
        IReadOnlyList<T1> set1,
        IReadOnlyList<T2> set2,
        IReadOnlyList<T3> set3,
        IReadOnlyDictionary<string, object?> outputValues,
        int returnValue = 0)
    {
        Set1         = set1;
        Set2         = set2;
        Set3         = set3;
        OutputValues = outputValues;
        ReturnValue  = returnValue;
    }

    public TOut? GetOutput<TOut>(string paramName)
    {
        var key = paramName.StartsWith('@') ? paramName : $"@{paramName}";
        return OutputValues.TryGetValue(key, out var val) && val is TOut typed
            ? typed
            : default;
    }
}

// ─────────────────────────────────────────────────────────────────────────────
// CONTRATOS SYNC
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Acceso sincrónico a SPs y SQL raw dentro del mismo DbContext / transacción del UoW.
/// Todos los métodos retornan Result&lt;T&gt; — nunca lanzan excepciones en la superficie pública.
/// </summary>
public interface IRawQueryRepositorySync
{
    // ── SP — resultado único ──────────────────────────────────────────────────

    Result<IReadOnlyList<T>> QuerySP<T>(
        string spName,
        IEnumerable<RawParameter>? parameters = null)
        where T : class, new();

    /// <summary>SP con OUTPUT y RETURN VALUE capturados en SpResult&lt;T&gt;.</summary>
    Result<SpResult<T>> QuerySPWithOutput<T>(
        string spName,
        IEnumerable<RawParameter>? parameters = null)
        where T : class, new();

    Result<int>  ExecuteSP(string spName, IEnumerable<RawParameter>? parameters = null);
    Result<T?>   ScalarSP<T>(string spName, IEnumerable<RawParameter>? parameters = null);

    // ── SP — múltiples result sets ────────────────────────────────────────────

    /// <summary>
    /// SP que devuelve DOS SELECT en un solo roundtrip.
    /// Ideal para reportes cabecera+detalle o listas paralelas.
    /// </summary>
    Result<SpMultiResult<T1, T2>> QuerySPMultiple<T1, T2>(
        string spName,
        IEnumerable<RawParameter>? parameters = null)
        where T1 : class, new()
        where T2 : class, new();

    /// <summary>SP que devuelve TRES SELECT en un solo roundtrip.</summary>
    Result<SpMultiResult<T1, T2, T3>> QuerySPMultiple<T1, T2, T3>(
        string spName,
        IEnumerable<RawParameter>? parameters = null)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new();

    // ── SQL raw ───────────────────────────────────────────────────────────────

    Result<IReadOnlyList<T>> QuerySql<T>(
        string sql,
        IEnumerable<RawParameter>? parameters = null)
        where T : class, new();

    Result<int> ExecuteSql(string sql, IEnumerable<RawParameter>? parameters = null);
}

// ─────────────────────────────────────────────────────────────────────────────
// CONTRATOS ASYNC
// ─────────────────────────────────────────────────────────────────────────────

/// <summary>
/// Acceso asíncrono a SPs y SQL raw dentro del mismo DbContext / transacción del UoW.
/// </summary>
public interface IRawQueryRepositoryAsync
{
    Task<Result<IReadOnlyList<T>>> QuerySPAsync<T>(
        string spName,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T : class, new();

    Task<Result<SpResult<T>>> QuerySPWithOutputAsync<T>(
        string spName,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T : class, new();

    Task<Result<int>>  ExecuteSPAsync(string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default);
    Task<Result<T?>>   ScalarSPAsync<T>(string spName, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default);

    Task<Result<SpMultiResult<T1, T2>>> QuerySPMultipleAsync<T1, T2>(
        string spName,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T1 : class, new()
        where T2 : class, new();

    Task<Result<SpMultiResult<T1, T2, T3>>> QuerySPMultipleAsync<T1, T2, T3>(
        string spName,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T1 : class, new()
        where T2 : class, new()
        where T3 : class, new();

    Task<Result<IReadOnlyList<T>>> QuerySqlAsync<T>(
        string sql,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T : class, new();

    // S80: SNAPSHOT isolation en SP para lecturas públicas sin bloqueos.
    Task<Result<SpResult<T>>> QuerySPWithSnapshotAsync<T>(
        string spName,
        IEnumerable<RawParameter>? parameters = null,
        CancellationToken ct = default)
        where T : class, new();

    Task<Result<int>> ExecuteSqlAsync(string sql, IEnumerable<RawParameter>? parameters = null, CancellationToken ct = default);
}
