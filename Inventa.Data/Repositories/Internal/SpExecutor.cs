using CBP.Data.Abstractions;
using CBP.Data.Asynchronous;
using CBP.Data.Synchronous;
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
/// S75: los SP se ejecutan POR NOMBRE con CommandType.StoredProcedure vía CBP
/// RawQuery (ExecuteSPRaw/ExecuteSPRawAsync con SqlParameter[]: TVP y OUTPUT).
/// Cero T-SQL en texto (AGENTS.md §0). Mismo DbContext → misma conexión y
/// transacción. El mapeo de errores vive en SpErrorMapper (puro y testeable).
/// Firmas, TVP, timeouts por SP y contratos sin cambios.
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

        // S75: sin string EXEC (CBP agrega @RETURN_VALUE solo; duplicarlo falla).
        SqlParameter[] parameters =
        [
            pIdTenant, pIdBodega, pNumVenta, pIdCliente, pTipoDoc, pIdUsuario,
            pItems, pPagos, pIdVenta, pSubTotal, pDescuento, pImpuesto, pTotal
        ];

        return ExecuteSp(context, SpRegistrarVenta, parameters,
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
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",        req.IdTenant),
            Scalar("@IdVenta",         req.IdVenta),
            Scalar("@MotivoAnulacion", req.MotivoAnulacion),
            Nullable("@IdUsuario",     req.IdUsuario)
        ];

        return ExecuteSp(context, SpAnularVenta, parameters);
    }

    internal static Result RecibirOrdenCompra(
        DbContext context, RecibirOrdenCompraRequest req)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",      req.IdTenant),
            Scalar("@IdBodega",      req.IdBodega),
            Scalar("@IdOrdenCompra", req.IdOrdenCompra),
            BuildOCRecepcionTvp(req.Items),
            Nullable("@IdUsuario",   req.IdUsuario)
        ];

        return ExecuteSp(context, SpRecibirOrdenCompra, parameters);
    }

    internal static Result AjustarInventario(
        DbContext context, AjustarInventarioRequest req)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",    req.IdTenant),
            Scalar("@IdBodega",    req.IdBodega),
            BuildAjusteItemTvp(req.Items),
            Nullable("@IdUsuario", req.IdUsuario)
        ];

        return ExecuteSp(context, SpAjustarInventario, parameters);
    }

    internal static Result TransferirEntresBodegas(
        DbContext context, TransferirEntresBodegasRequest req)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",     req.IdTenant),
            Scalar("@IdBodegaOrig", req.IdBodegaOrig),
            Scalar("@IdBodegaDest", req.IdBodegaDest),
            BuildTransferenciaTvp(req.Items),
            Nullable("@IdUsuario",  req.IdUsuario)
        ];

        return ExecuteSp(context, SpTransferirEntresBodegas, parameters);
    }

    // =========================================================================
    // EJECUCIÓN ASÍNCRONA (Web / API — futura interfaz Inventa)
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

        SqlParameter[] parameters =
        [
            pIdTenant, pIdBodega, pNumVenta, pIdCliente, pTipoDoc, pIdUsuario,
            pItems, pPagos, pIdVenta, pSubTotal, pDescuento, pImpuesto, pTotal
        ];

        return await ExecuteSpAsync(context, SpRegistrarVenta, parameters,
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
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",        req.IdTenant),
            Scalar("@IdVenta",         req.IdVenta),
            Scalar("@MotivoAnulacion", req.MotivoAnulacion),
            Nullable("@IdUsuario",     req.IdUsuario)
        ];

        return await ExecuteSpAsync(context, SpAnularVenta, parameters, ct);
    }

    public static async Task<Result> RecibirOrdenCompraAsync(
        InventaDbContext context, RecibirOrdenCompraRequest req,
        CancellationToken ct = default)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",      req.IdTenant),
            Scalar("@IdBodega",      req.IdBodega),
            Scalar("@IdOrdenCompra", req.IdOrdenCompra),
            BuildOCRecepcionTvp(req.Items),
            Nullable("@IdUsuario",   req.IdUsuario)
        ];

        return await ExecuteSpAsync(context, SpRecibirOrdenCompra, parameters, ct);
    }

    public static async Task<Result> AjustarInventarioAsync(
        InventaDbContext context, AjustarInventarioRequest req,
        CancellationToken ct = default)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",    req.IdTenant),
            Scalar("@IdBodega",    req.IdBodega),
            BuildAjusteItemTvp(req.Items),
            Nullable("@IdUsuario", req.IdUsuario)
        ];

        return await ExecuteSpAsync(context, SpAjustarInventario, parameters, ct);
    }

    public static async Task<Result> TransferirEntresBodegasAsync(
        InventaDbContext context, TransferirEntresBodegasRequest req,
        CancellationToken ct = default)
    {
        SqlParameter[] parameters =
        [
            Scalar("@IdTenant",     req.IdTenant),
            Scalar("@IdBodegaOrig", req.IdBodegaOrig),
            Scalar("@IdBodegaDest", req.IdBodegaDest),
            BuildTransferenciaTvp(req.Items),
            Nullable("@IdUsuario",  req.IdUsuario)
        ];

        return await ExecuteSpAsync(context, SpTransferirEntresBodegas, parameters, ct);
    }

    // =========================================================================
    // NÚCLEO GENÉRICO — SÍNCRONO (S75: CBP ExecuteSPRaw por nombre)
    // =========================================================================

    private static Result ExecuteSp(
        DbContext context, string spName, SqlParameter[] parameters)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);

        var prev = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(timeout);
        try
        {
            // Mismo DbContext → misma conexión y transacción (CBP BuildCommand).
            var raw = RawQueryRepositoryFactorySync.Create(context);
            var r = raw.ExecuteSPRaw(spName, parameters);
            return r.IsSuccess
                ? Result.Success()
                : Result.Failure(SpErrorMapper.Map(r.Error!, spName));
        }
        catch (Exception ex)
        {
            // ExecuteSPRaw no lanza por diseño; red defensiva (contrato: nunca lanza).
            return Result.Failure(SpErrorMapper.Map(SyntheticError(ex), spName));
        }
        finally
        {
            context.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    private static Result<T> ExecuteSp<T>(
        DbContext context, string spName, SqlParameter[] parameters,
        Func<T> resultFactory)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = context.Database.GetCommandTimeout();
        context.Database.SetCommandTimeout(timeout);
        try
        {
            var raw = RawQueryRepositoryFactorySync.Create(context);
            var r = raw.ExecuteSPRaw(spName, parameters);
            return r.IsSuccess
                ? Result<T>.Success(resultFactory())
                : Result<T>.Failure(SpErrorMapper.Map(r.Error!, spName));
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(SpErrorMapper.Map(SyntheticError(ex), spName));
        }
        finally
        {
            context.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    // =========================================================================
    // NÚCLEO GENÉRICO — ASÍNCRONO (S75: CBP ExecuteSPRawAsync por nombre)
    // =========================================================================

    private static async Task<Result> ExecuteSpAsync(
        InventaDbContext ctx, string spName, SqlParameter[] parameters,
        CancellationToken ct)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = ctx.Database.GetCommandTimeout();
        ctx.Database.SetCommandTimeout(timeout);
        try
        {
            var raw = RawQueryRepositoryFactoryAsync.Create(ctx);
            var r = await raw.ExecuteSPRawAsync(spName, ct, parameters)
                .ConfigureAwait(false);
            return r.IsSuccess
                ? Result.Success()
                : Result.Failure(SpErrorMapper.Map(r.Error!, spName));
        }
        catch (OperationCanceledException)
        {
            return Result.Failure(SpErrorMapper.Map(
                new Error("OPERATION_CANCELLED", "La operación fue cancelada."),
                spName));
        }
        catch (Exception ex)
        {
            return Result.Failure(SpErrorMapper.Map(SyntheticError(ex), spName));
        }
        finally
        {
            ctx.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    private static async Task<Result<T>> ExecuteSpAsync<T>(
        InventaDbContext ctx, string spName, SqlParameter[] parameters,
        Func<T> resultFactory,
        CancellationToken ct)
    {
        var timeout = SpTimeouts.GetValueOrDefault(spName, DefaultSpTimeoutSeconds);
        var prev = ctx.Database.GetCommandTimeout();
        ctx.Database.SetCommandTimeout(timeout);
        try
        {
            var raw = RawQueryRepositoryFactoryAsync.Create(ctx);
            var r = await raw.ExecuteSPRawAsync(spName, ct, parameters)
                .ConfigureAwait(false);
            return r.IsSuccess
                ? Result<T>.Success(resultFactory())
                : Result<T>.Failure(SpErrorMapper.Map(r.Error!, spName));
        }
        catch (OperationCanceledException)
        {
            return Result<T>.Failure(SpErrorMapper.Map(
                new Error("OPERATION_CANCELLED", "La operación fue cancelada."),
                spName));
        }
        catch (Exception ex)
        {
            return Result<T>.Failure(SpErrorMapper.Map(SyntheticError(ex), spName));
        }
        finally
        {
            ctx.Database.SetCommandTimeout(prev ?? 30);
        }
    }

    /// <summary>
    /// Error sintético para la red defensiva catch-all (nunca debería ocurrir).
    /// Equivale al antiguo MapUnexpectedError genérico (DATABASE_ERROR/503).
    /// </summary>
    private static Error SyntheticError(Exception ex) =>
        new("SP_EXECUTION_ERROR",
            string.IsNullOrWhiteSpace(ex.Message) ? ex.GetType().Name : ex.Message);

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

    private static object NullableVal<T>(T? value) where T : struct
        => (object?)value ?? DBNull.Value;

    private static object NullableVal(string? value)
        => (object?)value ?? DBNull.Value;
}
