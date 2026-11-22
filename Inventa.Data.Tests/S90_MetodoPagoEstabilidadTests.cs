using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

// S90 paso 0: las fixtures de venta/anulación insertaban y borraban un MetodoPago por
// corrida y agotaron la identidad TINYINT (255). Ahora usan el método sembrado EFE por
// código. Estas pruebas fijan ese contrato: setup + cleanup no altera MetodoPago.
// (La identidad en sí se verifica en la evidencia de S90 con IDENT_CURRENT antes/después
// de una corrida completa; leerla aquí exigiría SQL en texto.)
[Collection("OfficialLocal")]
public sealed class S90_MetodoPagoEstabilidadTests(OfficialLocalFixture fx)
{
    private async Task<List<(byte Id, string Codigo)>> SnapshotAsync()
    {
        using var ctx = fx.NewContext();
        var rows = await ctx.MetodosPago.AsNoTracking()
            .OrderBy(m => m.IdMetodoPago)
            .Select(m => new { m.IdMetodoPago, m.Codigo })
            .ToListAsync();
        return rows.Select(r => (r.IdMetodoPago, r.Codigo)).ToList();
    }

    private async Task<byte> IdSembradoAsync(string codigo)
    {
        using var ctx = fx.NewContext();
        return await ctx.MetodosPago.Where(m => m.Codigo == codigo).Select(m => m.IdMetodoPago).SingleAsync();
    }

    [Fact]
    public async Task SaleFixture_UsaMetodoSembrado_YNoAlteraMetodoPago()
    {
        var antes = await SnapshotAsync();
        var f = new SaleFixture(fx);
        await f.SetupAsync();
        try
        {
            Assert.Equal("EFE", SaleFixture.CodMetodo);
            Assert.Equal(await IdSembradoAsync("EFE"), f.IdMetodoPago);
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
        Assert.Equal(antes, await SnapshotAsync());
    }

    [Fact]
    public async Task CancellationFixture_UsaMetodoSembrado_YNoAlteraMetodoPago()
    {
        var antes = await SnapshotAsync();
        var f = new CancellationFixture(fx);
        await f.SetupAsync();
        try
        {
            Assert.Equal("EFE", CancellationFixture.CodMetodo);
            Assert.Equal(await IdSembradoAsync("EFE"), f.IdMetodoPago);
        }
        finally
        {
            await f.CleanupAsync();
        }
        Assert.Equal(0, await f.ResidueCountAsync());
        Assert.Equal(antes, await SnapshotAsync());
    }
}

