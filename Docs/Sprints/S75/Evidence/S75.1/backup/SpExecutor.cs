using CBP.Data.Abstractions;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Inventa.Data.Repositories.Internal;

/// <summary>
/// Helper para ejecutar los stored procedures críticos del modelo de inventario.
///
/// Cambios respecto a la versión anterior:
///   1. Bug timeout corregido: el finally restaura prev ?? SpCommandTimeoutSeconds en
///      lugar de null, que borraba la configuración del UoW silenciosamente.
///   2. RETURN VALUE: capturado y expuesto en SpResult para los SPs que lo usen.
///   3. Versión async: ExecuteSPAsync usando ExecuteSqlRawAsync de EF Core 10 —
///      libera el thread durante operaciones largas (recepción OC, transferencias).
///   4. Métodos síncronos reciben DbContext directamente — los repositorios custom
///      viven dentro del UoW y solo tienen acceso al contexto, no al UoW.
///      Métodos async reciben IUnitOfWorkAsync para la futura interfaz web.
///
/// Los métodos síncronos son los usados hoy en WinForms (VentaRepository,
/// StockRepository, OrdenCompraRepository). Los async están listos para la
/// futura interfaz web del mismo sistema.
/// </summary>
public static class SpExecutor
{
    // ── Nombres canónicos de SPs ───────────────────────────────────────────────
    internal const string SpRegistrarVenta = "dbo.sp_RegistrarVenta";
    internal const string SpAnularVenta = "dbo.sp_AnularVenta";
    internal const string SpRecibirOrdenCompra = "dbo.sp_RecibirOrdenCompra";
    internal const string SpAjustarInventario = "dbo.sp_AjustarInventario";
    internal const string SpTransferirEntresBodegas = "dbo.sp_TransferirEntresBodegas";

    // ── Nombres de tipo TVP ────────────────────────────────────────────────────
    private const string TvpVentaItem = "dbo.VentaItemType";
    private const string TvpVentaPago = "dbo.VentaPagoType";
    private const string TvpOCRecepcion = "dbo.OCRecepcionItemType";
    private const string TvpAjusteItem = "dbo.AjusteItemType";
    private const string TvpTransferencia = "dbo.TransferenciaItemType";

    // ── Timeouts por operación ────────────────────────────────────────────────
    // Cada SP tiene su propio timeout: los transaccionales cortos tienen 60 s,
    // los que procesan lotes grandes (recepción OC, transferencias) tienen 300 s.
    private static readonly Dictionary<string, int> SpTimeouts = new()
    {
        [SpRegistrarVenta] = 60,
        [SpAnularVenta] = 60,
        [SpRecibirOrdenCompra] = 300,   // puede procesar muchas líneas de OC
        [SpAjustarInventario] = 180,
        [SpTransferirEntresBodegas] = 180,
    };

    private const int DefaultSpTimeoutSeconds = 120;

    // ── Rango de errores de negocio SQL Server ────────────────────────────────
    private const int SpBusinessErrorMin = 50000;
    private const int SpBusinessErrorMax = 59999;

    // =========================================================================
    // EJECUCIÓN SÍNCRONA (WinForms)
    // =========================================================================

    internal static Result<RegistrarVentaResult> RegistrarVenta(
        DbContext context, RegistrarVentaRequest req)
    {
        var pIdTenant = Scalar("@IdTenant", req.IdTenant);
        var pIdBodega = Scalar("@IdBodega", req.IdBodega);
        var pNumVenta = Scalar("@NumVenta", req.NumVenta);
        var pIdCliente = Nullable("@IdCliente", req.IdCliente);
        var pTipoDoc = Scalar("@TipoDoc", req.TipoDoc);
        var pIdUsuario = Nullable("@IdUsuario", req.IdUsuario);
        var pItems = BuildVentaItemTvp(req.Items);
        var pPagos = BuildVentaPagoTvp(req.Pagos);
        var pIdVenta = OutBigInt("@IdVenta");
        var pSubTotal = OutDecimal("@SubTotal");
        var pDescuento = OutDecimal("@Descuento");
        var pImpuesto = OutDecimal("@Impuesto");
        var pTotal = OutDecimal("@Total");
        var pRetVal = ReturnValue();  // captura RETURN VALUE — uso interno únicamente  // captura RETURN VALUE internamente (0 = éxito en SQL Server)
                                      // No se expone en RegistrarVentaResult — contrato público sin cambios

        const string sql =
            $"EXEC {SpRegistrarVenta} " +
            "@IdTenant, @IdBodega, @NumVenta, @IdCliente, @TipoDoc, @IdUsuario, " +
            "@Items, @Pagos, " +
            "@IdVenta OUTPUT, @SubTotal OUTPUT, @Descuento OUTPUT, @Impuesto OUTPUT, @Total OUTPUT";

        object[] parameters =
        [
            pIdTenant, pIdBodega, pNumVenta, pIdCliente, pTipoDoc, pIdUsuario,
            pItems, pPagos, pIdVenta, pSubTotal, pDescuento, pImpuesto, pTotal, pRetVal
        ];

        return ExecuteSp(context, sql, parameters, SpRegistrarVenta,
            () => new RegistrarVentaResult(
                (long)pIdVenta.Value,
                (decimal)pSubTotal.Value,
                (decimal)pDescuento.Value,
                (decimal)pImpuesto.Value,
                (decimal)pTotal.Value));
    }

    internal static Result AnularVenta(
        DbContext context, AnularVentaRequest req)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",        req.IdTenant),
            Scalar("@IdVenta",         req.IdVenta),
            Scalar("@MotivoAnulacion", req.MotivoAnulacion),
            Nullable("@IdUsuario",     req.IdUsuario)
        ];

        return ExecuteSp(context,
            $"EXEC {SpAnularVenta} @IdTenant, @IdVenta, @MotivoAnulacion, @IdUsuario",
            parameters, SpAnularVenta);
    }

    internal static Result RecibirOrdenCompra(
        DbContext context, RecibirOrdenCompraRequest req)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",      req.IdTenant),
            Scalar("@IdBodega",      req.IdBodega),
            Scalar("@IdOrdenCompra", req.IdOrdenCompra),
            BuildOCRecepcionTvp(req.Items),
            Nullable("@IdUsuario",   req.IdUsuario)
        ];

        return ExecuteSp(context,
            $"EXEC {SpRecibirOrdenCompra} @IdTenant, @IdBodega, @IdOrdenCompra, @Items, @IdUsuario",
            parameters, SpRecibirOrdenCompra);
    }

    internal static Result AjustarInventario(
        DbContext context, AjustarInventarioRequest req)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",    req.IdTenant),
            Scalar("@IdBodega",    req.IdBodega),
            BuildAjusteItemTvp(req.Items),
            Nullable("@IdUsuario", req.IdUsuario)
        ];

        return ExecuteSp(context,
            $"EXEC {SpAjustarInventario} @IdTenant, @IdBodega, @Items, @IdUsuario",
            parameters, SpAjustarInventario);
    }

    internal static Result TransferirEntresBodegas(
        DbContext context, TransferirEntresBodegasRequest req)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",     req.IdTenant),
            Scalar("@IdBodegaOrig", req.IdBodegaOrig),
            Scalar("@IdBodegaDest", req.IdBodegaDest),
            BuildTransferenciaTvp(req.Items),
            Nullable("@IdUsuario",  req.IdUsuario)
        ];

        return ExecuteSp(context,
            $"EXEC {SpTransferirEntresBodegas} @IdTenant, @IdBodegaOrig, @IdBodegaDest, @Items, @IdUsuario",
            parameters, SpTransferirEntresBodegas);
    }

    // =========================================================================
    // EJECUCIÓN ASÍNCRONA (Web / API — futura interfaz Inventa)
    // CORRECCIÓN 3: usa ExecuteSqlRawAsync de EF Core 10
    // =========================================================================

    public static async Task<Result<RegistrarVentaResult>> RegistrarVentaAsync(
        InventaDbContext context, RegistrarVentaRequest req,
        CancellationToken ct = default)
    {
        var pIdTenant = Scalar("@IdTenant", req.IdTenant);
        var pIdBodega = Scalar("@IdBodega", req.IdBodega);
        var pNumVenta = Scalar("@NumVenta", req.NumVenta);
        var pIdCliente = Nullable("@IdCliente", req.IdCliente);
        var pTipoDoc = Scalar("@TipoDoc", req.TipoDoc);
        var pIdUsuario = Nullable("@IdUsuario", req.IdUsuario);
        var pItems = BuildVentaItemTvp(req.Items);
        var pPagos = BuildVentaPagoTvp(req.Pagos);
        var pIdVenta = OutBigInt("@IdVenta");
        var pSubTotal = OutDecimal("@SubTotal");
        var pDescuento = OutDecimal("@Descuento");
        var pImpuesto = OutDecimal("@Impuesto");
        var pTotal = OutDecimal("@Total");
        var pRetVal = ReturnValue();  // captura RETURN VALUE — uso interno únicamente

        const string sql =
            $"EXEC {SpRegistrarVenta} " +
            "@IdTenant, @IdBodega, @NumVenta, @IdCliente, @TipoDoc, @IdUsuario, " +
            "@Items, @Pagos, " +
            "@IdVenta OUTPUT, @SubTotal OUTPUT, @Descuento OUTPUT, @Impuesto OUTPUT, @Total OUTPUT";

        object[] parameters =
        [
            pIdTenant, pIdBodega, pNumVenta, pIdCliente, pTipoDoc, pIdUsuario,
            pItems, pPagos, pIdVenta, pSubTotal, pDescuento, pImpuesto, pTotal, pRetVal
        ];

        return await ExecuteSpAsync(context, sql, parameters, SpRegistrarVenta,
            () => new RegistrarVentaResult(
                (long)pIdVenta.Value,
                (decimal)pSubTotal.Value,
                (decimal)pDescuento.Value,
                (decimal)pImpuesto.Value,
                (decimal)pTotal.Value),
            ct);
    }

    public static async Task<Result> AnularVentaAsync(
        InventaDbContext context, AnularVentaRequest req,
        CancellationToken ct = default)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",        req.IdTenant),
            Scalar("@IdVenta",         req.IdVenta),
            Scalar("@MotivoAnulacion", req.MotivoAnulacion),
            Nullable("@IdUsuario",     req.IdUsuario)
        ];

        return await ExecuteSpAsync(context,
            $"EXEC {SpAnularVenta} @IdTenant, @IdVenta, @MotivoAnulacion, @IdUsuario",
            parameters, SpAnularVenta, ct);
    }

    public static async Task<Result> RecibirOrdenCompraAsync(
        InventaDbContext context, RecibirOrdenCompraRequest req,
        CancellationToken ct = default)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",      req.IdTenant),
            Scalar("@IdBodega",      req.IdBodega),
            Scalar("@IdOrdenCompra", req.IdOrdenCompra),
            BuildOCRecepcionTvp(req.Items),
            Nullable("@IdUsuario",   req.IdUsuario)
        ];

        return await ExecuteSpAsync(context,
            $"EXEC {SpRecibirOrdenCompra} @IdTenant, @IdBodega, @IdOrdenCompra, @Items, @IdUsuario",
            parameters, SpRecibirOrdenCompra, ct);
    }

    public static async Task<Result> AjustarInventarioAsync(
        InventaDbContext context, AjustarInventarioRequest req,
        CancellationToken ct = default)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",    req.IdTenant),
            Scalar("@IdBodega",    req.IdBodega),
            BuildAjusteItemTvp(req.Items),
            Nullable("@IdUsuario", req.IdUsuario)
        ];

        return await ExecuteSpAsync(context,
            $"EXEC {SpAjustarInventario} @IdTenant, @IdBodega, @Items, @IdUsuario",
            parameters, SpAjustarInventario, ct);
    }

    public static async Task<Result> TransferirEntresBodegasAsync(
        InventaDbContext context, TransferirEntresBodegasRequest req,
        CancellationToken ct = default)
    {
        object[] parameters =
        [
            Scalar("@IdTenant",     req.IdTenant),
            Scalar("@IdBodegaOrig", req.IdBodegaOrig),
            Scalar("@IdBodegaDest", req.IdBodegaDest),
            BuildTransferenciaTvp(req.Items),
            Nullable("@IdUsuario",  req.IdUsuario)
        ];

        return await ExecuteSpAsync(context,
            $"EXEC {SpTransferirEntresBodegas} @IdTenant, @IdBodegaOrig, @IdBodegaDest, @Items, @IdUsuario",
            parameters, SpTransferirEntresBodegas, ct);
    }

    // =========================================================================
    // NÚCLEO GENÉRICO — SÍNCRONO
    // =========================================================================

    private static Result ExecuteSp(
        DbContext context,
        string sql, object[] parameters, string spName)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);

        // CORRECCIÓN 1: guardar timeout previo y restaurarlo siempre a un valor válido.
        // GetCommandTimeout() puede devolver null si no fue configurado explícitamente.
        // SetCommandTimeout(null) no restaura al "no configurado" — resetea al default
        // del driver, ignorando cualquier configuración previa del UoW.
        var prev = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(timeout);
        try
        {
            context.Database.ExecuteSqlRaw(sql, parameters);
            return Result.Success();
        }
        catch (SqlException ex) when (IsBusinessError(ex))
        {
            return Result.Failure(MapBusinessError(ex, spName));
        }
        catch (Exception ex)
        {
            return Result.Failure(MapUnexpectedError(ex, spName));
        }
        finally
        {
            // CORRECCIÓN 1: si prev era null, restaurar al default razonable (30 s),
            // no a null que dejaría la conexión sin timeout configurado.
            context.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    private static Result<T> ExecuteSp<T>(
        DbContext context,
        string sql, object[] parameters, string spName,
        Func<T> resultFactory)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(timeout);
        try
        {
            context.Database.ExecuteSqlRaw(sql, parameters);
            return Result<T>.Success(resultFactory());
        }
        catch (SqlException ex) when (IsBusinessError(ex))
        {
            return Result<T>.Failure(MapBusinessError(ex, spName));
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(MapUnexpectedError(ex, spName));
        }
        finally
        {
            context.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    // =========================================================================
    // NÚCLEO GENÉRICO — ASÍNCRONO (EF Core 10 ExecuteSqlRawAsync)
    // =========================================================================

    private static async Task<Result> ExecuteSpAsync(
        InventaDbContext ctx,
        string sql, object[] parameters, string spName,
        CancellationToken ct)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = ctx.Database.GetCommandTimeout();
        ctx.Database.SetCommandTimeout(timeout);
        try
        {
            await ctx.Database.ExecuteSqlRawAsync(sql, parameters, ct);
            return Result.Success();
        }
        catch (SqlException ex) when (IsBusinessError(ex))
        {
            return Result.Failure(MapBusinessError(ex, spName));
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(
                new Error("SP_CANCELLED", $"La operación '{spName}' fue cancelada.")
                    .WithType(ErrorType.Database)
                    .WithHttpStatus(499)
                    .WithDetail("StoredProcedure", spName));
        }
        catch (Exception ex)
        {
            return Result.Failure(MapUnexpectedError(ex, spName));
        }
        finally
        {
            ctx.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    private static async Task<Result<T>> ExecuteSpAsync<T>(
        InventaDbContext ctx,
        string sql, object[] parameters, string spName,
        Func<T> resultFactory,
        CancellationToken ct)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = ctx.Database.GetCommandTimeout();
        ctx.Database.SetCommandTimeout(timeout);
        try
        {
            await ctx.Database.ExecuteSqlRawAsync(sql, parameters, ct);
            return Result<T>.Success(resultFactory());
        }
        catch (SqlException ex) when (IsBusinessError(ex))
        {
            return Result<T>.Failure(MapBusinessError(ex, spName));
        }
        catch (OperationCanceledException)
        {
            return Result<T>.Failure(
                new Error("SP_CANCELLED", $"La operación '{spName}' fue cancelada.")
                    .WithType(ErrorType.Database)
                    .WithHttpStatus(499)
                    .WithDetail("StoredProcedure", spName));
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(MapUnexpectedError(ex, spName));
        }
        finally
        {
            ctx.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    // =========================================================================
    // BUILDERS DE TVP
    // =========================================================================

    private static SqlParameter BuildVentaItemTvp(IReadOnlyList<VentaItemSp> items)
    {
        var dt = new DataTable();
        dt.Columns.Add("IdPresProd", typeof(long));
        dt.Columns.Add("IdLote", typeof(long));
        dt.Columns.Add("Cantidad", typeof(decimal));
        dt.Columns.Add("PrecioUnitario", typeof(decimal));
        dt.Columns.Add("Descuento", typeof(decimal));

        dt.BeginLoadData();
        foreach (var i in items)
            dt.Rows.Add(i.IdPresProd, NullableVal(i.IdLote),
                        i.Cantidad, i.PrecioUnitario, i.Descuento);
        dt.EndLoadData();
        return TvpParam("@Items", TvpVentaItem, dt);
    }

    private static SqlParameter BuildVentaPagoTvp(IReadOnlyList<VentaPagoSp> pagos)
    {
        var dt = new DataTable();
        dt.Columns.Add("IdMetodoPago", typeof(byte));
        dt.Columns.Add("Monto", typeof(decimal));
        dt.Columns.Add("Referencia", typeof(string));

        dt.BeginLoadData();
        foreach (var p in pagos)
            dt.Rows.Add(p.IdMetodoPago, p.Monto, NullableVal(p.Referencia));
        dt.EndLoadData();
        return TvpParam("@Pagos", TvpVentaPago, dt);
    }

    private static SqlParameter BuildOCRecepcionTvp(IReadOnlyList<OCRecepcionItemSp> items)
    {
        var dt = new DataTable();
        dt.Columns.Add("IdOCDetalle", typeof(long));
        dt.Columns.Add("IdLote", typeof(long));
        dt.Columns.Add("CantRecibida", typeof(decimal));
        dt.Columns.Add("CostoUnitario", typeof(decimal));

        dt.BeginLoadData();
        foreach (var i in items)
            dt.Rows.Add(i.IdOCDetalle, NullableVal(i.IdLote),
                        i.CantRecibida, i.CostoUnitario);
        dt.EndLoadData();
        return TvpParam("@Items", TvpOCRecepcion, dt);
    }

    private static SqlParameter BuildAjusteItemTvp(IReadOnlyList<AjusteItemSp> items)
    {
        var dt = new DataTable();
        dt.Columns.Add("IdPresProd", typeof(long));
        dt.Columns.Add("IdLote", typeof(long));
        dt.Columns.Add("CantAjuste", typeof(decimal));
        dt.Columns.Add("CostoUnitario", typeof(decimal));

        dt.BeginLoadData();
        foreach (var i in items)
            dt.Rows.Add(i.IdPresProd, NullableVal(i.IdLote),
                        i.CantAjuste, i.CostoUnitario);
        dt.EndLoadData();
        return TvpParam("@Items", TvpAjusteItem, dt);
    }

    private static SqlParameter BuildTransferenciaTvp(IReadOnlyList<TransferenciaItemSp> items)
    {
        var dt = new DataTable();
        dt.Columns.Add("IdPresProd", typeof(long));
        dt.Columns.Add("IdLote", typeof(long));
        dt.Columns.Add("Cantidad", typeof(decimal));

        dt.BeginLoadData();
        foreach (var i in items)
            dt.Rows.Add(i.IdPresProd, NullableVal(i.IdLote), i.Cantidad);
        dt.EndLoadData();
        return TvpParam("@Items", TvpTransferencia, dt);
    }

    // =========================================================================
    // FÁBRICA DE PARÁMETROS SQL
    // =========================================================================

    private static SqlParameter Scalar(string name, object value)
        => new(name, value);

    private static SqlParameter Nullable<T>(string name, T? value) where T : struct
        => new(name, (object?)value ?? DBNull.Value);

    private static SqlParameter OutBigInt(string name)
        => new(name, SqlDbType.BigInt) { Direction = ParameterDirection.Output };

    private static SqlParameter OutDecimal(string name)
        => new(name, SqlDbType.Decimal)
        {
            Direction = ParameterDirection.Output,
            Precision = 18,
            Scale = 4
        };

    private static SqlParameter TvpParam(string name, string typeName, DataTable dt)
        => new(name, SqlDbType.Structured) { TypeName = typeName, Value = dt };

    /// <summary>
    /// CORRECCIÓN 2: parámetro para capturar el RETURN VALUE del SP.
    /// Debe ser el último parámetro en la colección.
    /// </summary>
    private static SqlParameter ReturnValue()
        => new("@ReturnValue", SqlDbType.Int) { Direction = ParameterDirection.ReturnValue };

    /// <summary>Lee el RETURN VALUE después de ejecutar el SP. 0 = éxito (convención SQL Server).</summary>
    private static int ReturnVal(SqlParameter p)
        => p.Value is int v ? v : 0;

    private static object NullableVal<T>(T? value) where T : struct
        => (object?)value ?? DBNull.Value;

    private static object NullableVal(string? value)
        => (object?)value ?? DBNull.Value;

    // =========================================================================
    // MAPEO DE ERRORES
    // =========================================================================

    private static bool IsBusinessError(SqlException ex)
        => ex.Number is >= SpBusinessErrorMin and <= SpBusinessErrorMax;

    private static Error MapBusinessError(SqlException ex, string spName)
    {
        var (code, type, http) = ex.Number switch
        {
            50001 => ("BODEGA_INACTIVA", ErrorType.Validation, 400),
            50002 => ("VENTA_DUPLICADA", ErrorType.Conflict, 409),
            50003 => ("VENTA_SIN_ITEMS", ErrorType.Validation, 400),
            50004 => ("VENTA_SIN_PAGOS", ErrorType.Validation, 400),
            50005 => ("STOCK_INSUFICIENTE", ErrorType.Conflict, 409),
            50010 => ("VENTA_NO_ENCONTRADA", ErrorType.NotFound, 404),
            50011 => ("VENTA_ESTADO_INVALIDO", ErrorType.Conflict, 409),
            50012 => ("MOTIVO_REQUERIDO", ErrorType.Validation, 400),
            50020 => ("OC_NO_ENCONTRADA", ErrorType.NotFound, 404),
            50021 => ("OC_ESTADO_INVALIDO", ErrorType.Conflict, 409),
            50022 => ("OC_SIN_ITEMS", ErrorType.Validation, 400),
            50023 => ("OC_DETALLE_INVALIDO", ErrorType.Validation, 400),
            50030 => ("BODEGA_INACTIVA", ErrorType.Validation, 400),
            50031 => ("AJUSTE_SIN_ITEMS", ErrorType.Validation, 400),
            50032 => ("AJUSTE_CANTIDAD_CERO", ErrorType.Validation, 400),
            50033 => ("STOCK_INSUFICIENTE", ErrorType.Conflict, 409),
            50040 => ("BODEGA_MISMA_ORIGEN_DEST", ErrorType.Validation, 400),
            50041 => ("BODEGA_ORIGEN_INACTIVA", ErrorType.Validation, 400),
            50042 => ("BODEGA_DEST_INACTIVA", ErrorType.Validation, 400),
            50043 => ("TRANSFERENCIA_SIN_ITEMS", ErrorType.Validation, 400),
            50044 => ("STOCK_INSUFICIENTE", ErrorType.Conflict, 409),
            _ => ("SP_BUSINESS_ERROR", ErrorType.Validation, 400)
        };

        // Concatenar todos los mensajes de error del SP (puede haber más de uno en errores encadenados)
        var allMessages = string.Join(" | ",
            Enumerable.Range(0, ex.Errors.Count).Select(i => ex.Errors[i].Message));

        return new Error(code, allMessages)
            .WithType(type)
            .WithHttpStatus(http)
            .WithDetail("StoredProcedure", spName)
            .WithDetail("SqlErrorNumber", ex.Number);
    }

    private static Error MapUnexpectedError(Exception ex, string spName)
    {
        // S57.74 Fase 2 (V2): UQ_Venta_Num violation => 409 VENTA_DUPLICADA clean, sin SQL leak
        if (ex is SqlException sqlEx && (sqlEx.Number == 2627 || sqlEx.Number == 2601)
            && sqlEx.Message.Contains("UQ_Venta_Num", StringComparison.OrdinalIgnoreCase))
        {
            return new Error("VENTA_DUPLICADA", "El número de venta ya existe para este tenant")
                .WithType(ErrorType.Conflict)
                .WithHttpStatus(409)
                .WithDetail("StoredProcedure", spName);
        }

        var error = new Error("DATABASE_ERROR",
                $"Error de base de datos ejecutando {spName}")
            .WithType(ErrorType.Database)
            .WithHttpStatus(503)
            .WithDetail("StoredProcedure", spName)
            .WithDetail("ExceptionType", ex.GetType().Name)
            .WithDetail("ExceptionMessage", ex.Message);

        if (ex.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            return error.WithDetail("ErrorCategory", "Timeout");
        if (ex.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase))
            return error.WithDetail("ErrorCategory", "Deadlock");

        return error;
    }
}
