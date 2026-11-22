using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S88.2: Fact que se omite (Skip) si VESTUARIO (tenant 5) no tiene la carga
/// Ficcus. La decisión se toma con EF/LINQ síncrono (sin SQL en texto, §0),
/// una sola vez por ejecución (caché estático).
/// S92.2-K4 (xUnit v3 xUnit3003): constructor con source information.
/// </summary>
public sealed class DatosFiccusFactAttribute : FactAttribute
{
    public DatosFiccusFactAttribute(
        [CallerFilePath] string? sourceFilePath = null,
        [CallerLineNumber] int sourceLineNumber = -1)
        : base(sourceFilePath, sourceLineNumber)
    {
        // El compilador suple caller info en cada uso [DatosFiccusFact]; aquí
        // corre el gate (Skip si VESTUARIO no tiene carga v2).
        if (!S88DatosFiccusGate.HayCargaV2())
            Skip = "VESTUARIO sin carga Ficcus v2 (jerárquica); se omite.";
    }
}

internal static class S88DatosFiccusGate
{
    private static bool? _cacheV2;

    /// <summary>
    /// Carga v2 presente = raíz VESTUARIO con hijas (la carga v1 plana
    /// no cuenta; está pendiente de limpieza S88.2).
    /// </summary>
    internal static bool HayCargaV2()
    {
        _cacheV2 ??= VerificarV2();
        return _cacheV2.Value;
    }

    private static bool VerificarV2()
    {
        try
        {
            var options = new DbContextOptionsBuilder<InventaDbContext>()
                .UseSqlServer(OfficialLocalFixture.ConnectionString)
                .Options;
            using var ctx = new InventaDbContext(options);
            var raiz = ctx.Categorias.FirstOrDefault(
                c => c.IdTenant == 5 && c.Codigo == "VESTUARIO" && c.IdPadre == null);
            return raiz != null && ctx.Categorias.Any(c => c.IdPadre == raiz.IdCategoria);
        }
        catch
        {
            return false;
        }
    }

    internal static bool HayCarga()
    {
        try
        {
            var options = new DbContextOptionsBuilder<InventaDbContext>()
                .UseSqlServer(OfficialLocalFixture.ConnectionString)
                .Options;
            using var ctx = new InventaDbContext(options);
            return ctx.Productos.Any(p => p.IdTenant == 5);
        }
        catch
        {
            return false;
        }
    }
}

