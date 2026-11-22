using CBP.Data.Abstractions;
using Xunit;

namespace Inventa.Data.Tests;

// S75 §3/§7: pruebas unitarias puras de CBP DbValueConverter.ConvertTo
// (mapeo DateOnly/TimeOnly/enums + número de error SQL). Sin DB.
public sealed class S75DbValueConverterTests
{
    [Fact]
    public void Null_Y_DbNull_A_Tipos_Referencia_Dan_Null()
    {
        Assert.Null(DbValueConverter.ConvertTo(null, typeof(string)));
        Assert.Null(DbValueConverter.ConvertTo(DBNull.Value, typeof(string)));
    }

    [Fact]
    public void DbNull_Siempre_Da_Null_Los_Llamadores_Gestionan()
    {
        // MapReaderAsync salta DBNull; ConvertScalar devuelve default.
        Assert.Null(DbValueConverter.ConvertTo(DBNull.Value, typeof(int)));
        Assert.Null(DbValueConverter.ConvertTo(DBNull.Value, typeof(decimal)));
    }

    [Fact]
    public void Mismo_Tipo_Se_Devuelve_Tal_Cual()
    {
        var s = "abc";
        Assert.Same(s, DbValueConverter.ConvertTo(s, typeof(string)));
    }

    [Fact]
    public void DateOnly_Desde_DateTime_Y_DateTimeOffset()
    {
        var dt = new DateTime(2026, 12, 31, 15, 30, 0);
        Assert.Equal(new DateOnly(2026, 12, 31), DbValueConverter.ConvertTo(dt, typeof(DateOnly)));
        var dto = new DateTimeOffset(2027, 1, 15, 8, 0, 0, TimeSpan.Zero);
        Assert.Equal(new DateOnly(2027, 1, 15), DbValueConverter.ConvertTo(dto, typeof(DateOnly)));
    }

    [Fact]
    public void TimeOnly_Desde_TimeSpan_Y_DateTime()
    {
        var ts = new TimeSpan(13, 45, 10);
        Assert.Equal(TimeOnly.FromTimeSpan(ts), DbValueConverter.ConvertTo(ts, typeof(TimeOnly)));
        var dt = new DateTime(2026, 5, 1, 9, 20, 30);
        Assert.Equal(TimeOnly.FromDateTime(dt), DbValueConverter.ConvertTo(dt, typeof(TimeOnly)));
    }

    private enum ProbeEnum { Ninguno = 0, Activo = 1 }

    [Fact]
    public void Enum_Desde_String_CaseInsensitive_Y_Desde_Numerico()
    {
        Assert.Equal(ProbeEnum.Activo, DbValueConverter.ConvertTo("activo", typeof(ProbeEnum)));
        Assert.Equal(ProbeEnum.Activo, DbValueConverter.ConvertTo(1, typeof(ProbeEnum)));
    }

    [Fact]
    public void Numericos_Por_Convert_ChangeType()
    {
        Assert.Equal(42m, DbValueConverter.ConvertTo(42, typeof(decimal)));
        Assert.Equal(7, DbValueConverter.ConvertTo("7", typeof(int)));
        Assert.True(DbValueConverter.ConvertTo(1, typeof(bool)) is true);
    }

    [Fact]
    public void DateTime_Se_Mantiene()
    {
        var dt = new DateTime(2026, 10, 9, 12, 0, 0);
        Assert.Equal(dt, DbValueConverter.ConvertTo(dt, typeof(DateTime)));
    }
}

