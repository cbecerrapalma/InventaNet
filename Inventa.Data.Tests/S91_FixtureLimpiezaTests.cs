using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S91: el fixture S88 no deja residuo — Setup+Dispose elimina todos los
/// maestros S88-* (incluye S88-CAT/S88-MAR del setup) y no toca los lotes de importación del tenant (S91.2).
/// Misma Collection que los tests S88 (secuencial, orden-independiente).
/// </summary>
[Collection("OfficialLocal")]
public sealed class S91_FixtureLimpiezaTests(OfficialLocalFixture fx)
{
    [Fact]
    public async Task Fixture_SetupDispose_NoDejaResiduoS88()
    {
        int lotesAntes;
        using (var c0 = fx.NewContext()) lotesAntes = await c0.ImportacionLotes.CountAsync(l => l.IdTenant == 4, Xunit.TestContext.Current.CancellationToken);
        var f = new S88ImportFixture(fx);
        await f.SetupAsync();
        await f.DisposeAsync();

        using var ctx = f.NewContext();
        Assert.Equal(0, await ctx.Categorias.CountAsync(c => c.IdTenant == 4 && c.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(0, await ctx.Marcas.CountAsync(m => m.IdTenant == 4 && m.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(0, await ctx.Unidades.CountAsync(u => u.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(0, await ctx.Bodegas.CountAsync(b => b.IdTenant == 4 && b.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(0, await ctx.Proveedores.CountAsync(p => p.IdTenant == 4 && p.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        Assert.Equal(0, await ctx.Productos.CountAsync(p => p.IdTenant == 4 && p.Codigo.StartsWith("S88-"), Xunit.TestContext.Current.CancellationToken));
        // S91.2: los lotes de importación del tenant (uso real de la UI) no se tocan.
        Assert.Equal(lotesAntes, await ctx.ImportacionLotes.CountAsync(l => l.IdTenant == 4, Xunit.TestContext.Current.CancellationToken));
    }
}

