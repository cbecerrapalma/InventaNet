using CBP.Results.Errors;
using Inventa.Data.Repositories.Internal;
using Xunit;

namespace Inventa.Data.Tests;

// S75 §4: pruebas unitarias puras de SpErrorMapper (contrato de errores previo
// de SpExecutor: mismos códigos, tipos y HTTP). Sin DB.
public sealed class S75SpErrorMapperTests
{
    private static Error Cbp(string code, string message, int? sqlNumber = null, string? sqlMessage = null)
    {
        var e = new Error(code, message);
        if (sqlNumber.HasValue) e = e.WithDetail("SqlErrorNumber", sqlNumber.Value);
        if (sqlMessage != null) e = e.WithDetail("SqlErrorMessage", sqlMessage);
        return e;
    }

    [Fact]
    public void Negocio_50005_Mapea_StockInsuficiente_409_Mensaje_Crudo()
    {
        // El mensaje crudo puede llevar <html>; debe preservarse sin codificar.
        var mapped = SpErrorMapper.Map(
            Cbp("SP_EXECUTION_ERROR", "ignored", 50005, "Stock < 10 & restos"),
            "dbo.sp_RegistrarVenta");
        Assert.Equal("STOCK_INSUFICIENTE", mapped.Code);
        Assert.Equal("Stock < 10 & restos", mapped.Message);
        Assert.Equal(409, mapped.HttpStatusCode);
        Assert.Equal(50005, mapped.Details!["SqlErrorNumber"]);
    }

    [Fact]
    public void Negocio_50002_Mapea_VentaDuplicada_409()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("SP_EXECUTION_ERROR", "ignored", 50002, "duplicada"), "dbo.sp_RegistrarVenta");
        Assert.Equal("VENTA_DUPLICADA", mapped.Code);
        Assert.Equal(409, mapped.HttpStatusCode);
    }

    [Fact]
    public void Negocio_Fuera_De_Tabla_Mapea_Generico_400()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("SP_EXECUTION_ERROR", "ignored", 59999, "otro"), "dbo.sp_RegistrarVenta");
        Assert.Equal("SP_BUSINESS_ERROR", mapped.Code);
        Assert.Equal(400, mapped.HttpStatusCode);
    }

    [Fact]
    public void Carrera_UQ_Venta_Num_2627_Mapea_409()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("CONSTRAINT_VIOLATION",
                "Violation of UNIQUE KEY constraint 'UQ_Venta_Num'. Cannot insert duplicate key.",
                2627),
            "dbo.sp_RegistrarVenta");
        Assert.Equal("VENTA_DUPLICADA", mapped.Code);
        Assert.Equal(409, mapped.HttpStatusCode);
    }

    [Fact]
    public void UK_Distinta_2627_Mapea_DatabaseError_503()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("CONSTRAINT_VIOLATION", "Violation of UNIQUE KEY constraint 'UX_Otra'.", 2627),
            "dbo.sp_RegistrarVenta");
        Assert.Equal("DATABASE_ERROR", mapped.Code);
        Assert.Equal(503, mapped.HttpStatusCode);
    }

    [Fact]
    public void Timeout_Mapea_503_Con_Categoria()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("DATABASE_TIMEOUT", "Execution Timeout Expired."), "dbo.sp_RecibirOrdenCompra");
        Assert.Equal("DATABASE_ERROR", mapped.Code);
        Assert.Equal(503, mapped.HttpStatusCode);
        Assert.Equal("Timeout", mapped.Details!["ErrorCategory"]);
    }

    [Fact]
    public void Deadlock_Mapea_503_Con_Categoria()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("DEADLOCK_VICTIM", "Transaction was deadlocked."), "dbo.sp_AjustarInventario");
        Assert.Equal("DATABASE_ERROR", mapped.Code);
        Assert.Equal("Deadlock", mapped.Details!["ErrorCategory"]);
    }

    [Fact]
    public void Cancelacion_Mapea_499()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("OPERATION_CANCELLED", "La operación fue cancelada."), "dbo.sp_AnularVenta");
        Assert.Equal("SP_CANCELLED", mapped.Code);
        Assert.Equal(499, mapped.HttpStatusCode);
    }

    [Fact]
    public void Generico_Mapea_DatabaseError_503_Sin_Categoria()
    {
        var mapped = SpErrorMapper.Map(
            Cbp("SP_EXECUTION_ERROR", "Algo falló."), "dbo.sp_TransferirEntreBodegas");
        Assert.Equal("DATABASE_ERROR", mapped.Code);
        Assert.Equal(503, mapped.HttpStatusCode);
        Assert.False(mapped.Details!.ContainsKey("ErrorCategory"));
    }
}

