using CBP.Results;
using CBP.Results.Errors;

namespace Inventa.Data.Repositories.Internal;

/// <summary>
/// Guards reutilizables para validación de argumentos en repositorios.
/// Centraliza la lógica repetida en todos los métodos de repositorio.
/// </summary>
internal static class Guard
{
    // ── IDs ───────────────────────────────────────────────────────────────────

    public static Error? IdInvalido(string nombre, int valor) =>
        valor <= 0 ? CommonErrors.Validation.OutOfRange(nombre, 1, int.MaxValue) : null;

    public static Error? IdInvalido(string nombre, long valor) =>
        valor <= 0 ? CommonErrors.Validation.OutOfRange(nombre, 1, long.MaxValue) : null;

    // ── Strings ───────────────────────────────────────────────────────────────

    public static Error? StringRequerido(string nombre, string? valor) =>
        string.IsNullOrWhiteSpace(valor) ? CommonErrors.Validation.Required(nombre) : null;

    // ── Rangos numéricos ──────────────────────────────────────────────────────

    public static Error? Rango(string nombre, int valor, int min, int max) =>
        (valor < min || valor > max) ? CommonErrors.Validation.OutOfRange(nombre, min, max) : null;

    public static Error? Rango(string nombre, decimal valor, decimal min, decimal max) =>
        (valor < min || valor > max) ? CommonErrors.Validation.OutOfRange(nombre, min, max) : null;

    /// <summary>
    /// Valida que un estado (byte) esté dentro del rango permitido.
    /// Uso típico: OrdenCompra 1-4, Venta 1-2, TipoDoc 1-3.
    /// </summary>
    public static Error? EstadoInvalido(string nombre, byte valor, byte min, byte max) =>
        (valor < min || valor > max) ? CommonErrors.Validation.OutOfRange(nombre, min, max) : null;

    // ── Fechas ────────────────────────────────────────────────────────────────

    public static Error? RangoFechas(DateTime desde, DateTime hasta) =>
        desde > hasta ? RangoFechasError(desde, hasta) : null;

    public static Error? RangoFechas(DateOnly desde, DateOnly hasta) =>
        desde > hasta ? RangoFechasError(desde, hasta) : null;

    private static Error RangoFechasError(object desde, object hasta) =>
        new Error("INVALID_DATE_RANGE", "La fecha 'desde' no puede ser mayor que la fecha 'hasta'")
            .WithType(ErrorType.Validation)
            .WithHttpStatus(400)
            .WithDetail("Desde", desde)
            .WithDetail("Hasta", hasta);

    // ── Cantidades especiales ─────────────────────────────────────────────────

    /// <summary>
    /// Valida que una cantidad de ajuste sea distinta de cero.
    /// CantAjuste puede ser positivo (entrada) o negativo (salida), pero nunca cero.
    /// El SP sp_AjustarInventario lanza error 50032 si recibe un ítem con CantAjuste=0.
    /// Esta validación previene el round-trip innecesario a SQL Server.
    /// </summary>
    public static Error? CantidadAjuste(string nombre, decimal valor) =>
        valor == 0m
            ? new Error("AJUSTE_CANTIDAD_CERO", $"La cantidad de ajuste '{nombre}' no puede ser cero.")
                  .WithType(ErrorType.Validation)
                  .WithHttpStatus(400)
                  .WithDetail("Campo", nombre)
            : null;

    /// <summary>
    /// Valida que una cantidad de movimiento (venta, transferencia) sea estrictamente positiva.
    /// A diferencia de CantidadAjuste, estos valores no pueden ser negativos.
    /// </summary>
    public static Error? CantidadPositiva(string nombre, decimal valor) =>
        valor <= 0m
            ? CommonErrors.Validation.OutOfRange(nombre, 0.0001m, decimal.MaxValue)
            : null;

    /// <summary>
    /// Valida que el TipoMov sea un valor reconocido por el modelo:
    /// 1=Compra, 2=Venta, 3=Ajuste Entrada, 4=Ajuste Salida,
    /// 5=Transferencia Entrada, 6=Transferencia Salida.
    /// </summary>
    public static Error? TipoMovInvalido(byte tipoMov)
    {
        const byte TipoMovMin = 1;
        const byte TipoMovMax = 6;
        return tipoMov is < TipoMovMin or > TipoMovMax
            ? new Error("TIPO_MOV_INVALIDO",
                  $"TipoMov '{tipoMov}' no es válido. Valores permitidos: 1=Compra, 2=Venta, " +
                  "3=Ajuste Entrada, 4=Ajuste Salida, 5=Trans.Entrada, 6=Trans.Salida.")
                  .WithType(ErrorType.Validation)
                  .WithHttpStatus(400)
                  .WithDetail("TipoMov", tipoMov)
            : null;
    }

    // ── Helpers para retorno inmediato ────────────────────────────────────────

    /// <summary>
    /// Retorna un Result fallido si el guard no es null, de lo contrario null para continuar.
    /// Uso: return Guard.Fail(Guard.IdInvalido(...)) ?? ...
    /// </summary>
    public static Result<T>? Fail<T>(Error? error) =>
        error is null ? null : Result<T>.Failure(error);

    /// <summary>
    /// Sobrecarga sin genérico para métodos que retornan Result (sin valor),
    /// típicamente operaciones de escritura como Remove.
    /// </summary>
    public static Result? Fail(Error? error) =>
        error is null ? null : Result.Failure(error);
}
