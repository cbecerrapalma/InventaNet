using CBP.Results.Errors;

namespace Inventa.Data.Repositories.Internal;

/// <summary>
/// S75: traduce el <see cref="Error"/> devuelto por CBP RawQuery al contrato de
/// errores previo de <see cref="SpExecutor"/> (mismos códigos, tipos y HTTP).
/// Puro y testeable. Fuente: <c>Details["SqlErrorNumber"]</c> /
/// <c>Details["SqlErrorMessage"]</c> agregados por CBP (S75), más código/mensaje.
/// Gate 0 §3: el único UK que <c>sp_RegistrarVenta</c> puede violar es
/// <c>UQ_Venta_Num</c> (carrera pre-check → INSERT).
/// </summary>
internal static class SpErrorMapper
{
    public static Error Map(Error error, string spName)
    {
        int? number = null;
        if (error.Details != null
            && error.Details.TryGetValue("SqlErrorNumber", out var n)
            && n is int i)
            number = i;
        var sqlMessage = error.Details != null
            && error.Details.TryGetValue("SqlErrorMessage", out var m) ? m as string : null;

        // 1. Errores de negocio del SP (50001–50044): mensaje crudo, sin HTML.
        if (number is >= 50000 and <= 59999)
            return Business(number.Value, sqlMessage ?? error.Message, spName);

        // 2. Cancelación (antes: catch OperationCanceledException).
        if (error.Code == "OPERATION_CANCELLED")
            return new Error("SP_CANCELLED", $"La operación '{spName}' fue cancelada.")
                .WithType(ErrorType.Database)
                .WithHttpStatus(499)
                .WithDetail("StoredProcedure", spName);

        // 3. Duplicada por carrera (antes: 2627/2601 + UQ_Venta_Num en mensaje).
        if ((number == 2627 || number == 2601)
            && error.Message.Contains("UQ_Venta_Num", StringComparison.OrdinalIgnoreCase))
            return new Error("VENTA_DUPLICADA", "El número de venta ya existe para este tenant")
                .WithType(ErrorType.Conflict)
                .WithHttpStatus(409)
                .WithDetail("StoredProcedure", spName);

        // 4-5. Timeout / deadlock: por código CBP (más fiable); fallback al
        // mensaje como antes.
        if (error.Code == "DATABASE_TIMEOUT"
            || error.Message.Contains("timeout", StringComparison.OrdinalIgnoreCase))
            return Database(spName, error, "Timeout");
        if (error.Code == "DEADLOCK_VICTIM"
            || error.Message.Contains("deadlock", StringComparison.OrdinalIgnoreCase))
            return Database(spName, error, "Deadlock");

        // 6. Genérico.
        return Database(spName, error, null);
    }

    private static Error Business(int number, string message, string spName)
    {
        var (code, type, http) = number switch
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

        return new Error(code, message)
            .WithType(type)
            .WithHttpStatus(http)
            .WithDetail("StoredProcedure", spName)
            .WithDetail("SqlErrorNumber", number);
    }

    private static Error Database(string spName, Error src, string? category)
    {
        // Desviación documentada: CBP no expone ExceptionType (antes
        // ex.GetType().Name) ni el mensaje crudo (saneado a 200 + HTML).
        // Se conserva el mensaje saneado de CBP como equivalente.
        var error = new Error("DATABASE_ERROR",
                $"Error de base de datos ejecutando {spName}")
            .WithType(ErrorType.Database)
            .WithHttpStatus(503)
            .WithDetail("StoredProcedure", spName)
            .WithDetail("ExceptionType", src.Code)
            .WithDetail("ExceptionMessage", src.Message);
        return category == null ? error : error.WithDetail("ErrorCategory", category);
    }
}
