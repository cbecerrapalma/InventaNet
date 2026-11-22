using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S91.3: búsqueda de productos "contiene" por código/nombre de producto, código/nombre de variante
/// y código de barras, aislada por tenant. Solo lectura sobre la semilla ABA-T0001 (tenant 4).
/// </summary>
[Collection("OfficialLocal")]
public sealed class S913_ProductoBusquedaTests(OfficialLocalFixture fx)
{
    private async Task<(long IdProd, string NombreProd, string CodVar, string CodBar)> MasterAsync()
    {
        using var ctx = fx.NewContext();
        var idProd = await fx.MasterIdProdAsync();
        var datos = await ctx.CodigosBarra.AsNoTracking()
            .Where(cb => cb.PresProd.VarProd.IdProd == idProd)
            .OrderBy(cb => cb.IdCodBar)
            .Select(cb => new { Prod = cb.PresProd.VarProd.Prod.Nombre, Var = cb.PresProd.VarProd.Codigo, cb.Codigo })
            .FirstOrDefaultAsync();
        if (datos is null) throw new InvalidOperationException("S86: semilla ABA-T0001 sin código de barras. STOP, no skip.");
        return (idProd, datos.Prod, datos.Var, datos.Codigo);
    }

    private async Task<IReadOnlyList<long>> BuscarAsync(int idTenant, string texto)
    {
        using var ctx = fx.NewContext();
        var (items, _) = await new ProdRepositoryAsync(ctx).SearchPagedAsync(idTenant, texto, 1, 500);
        return items.Select(p => p.IdProd).ToList();
    }

    [Fact]
    public async Task Busca_PorParteDelNombre()
    {
        var m = await MasterAsync();
        var parte = m.NombreProd.Substring(1, Math.Min(6, m.NombreProd.Length - 1));
        Assert.Contains(m.IdProd, await BuscarAsync(OfficialLocalFixture.MasterIdTenant, parte));
    }

    [Fact]
    public async Task Busca_PorParteDelCodigoDeProducto()
    {
        var m = await MasterAsync();
        Assert.Contains(m.IdProd, await BuscarAsync(OfficialLocalFixture.MasterIdTenant, OfficialLocalFixture.MasterCodProd[2..]));
    }

    [Fact]
    public async Task Busca_PorCodigoDeVariante()
    {
        var m = await MasterAsync();
        Assert.Contains(m.IdProd, await BuscarAsync(OfficialLocalFixture.MasterIdTenant, m.CodVar));
    }

    [Fact]
    public async Task Busca_PorCodigoDeBarrasCompletoYParcial()
    {
        var m = await MasterAsync();
        Assert.Contains(m.IdProd, await BuscarAsync(OfficialLocalFixture.MasterIdTenant, m.CodBar));
        Assert.Contains(m.IdProd, await BuscarAsync(OfficialLocalFixture.MasterIdTenant, m.CodBar[^6..]));
    }

    [Fact]
    public async Task CodigoDeBarras_NoCruzaTenants()
    {
        var m = await MasterAsync();
        Assert.DoesNotContain(m.IdProd, await BuscarAsync(OfficialLocalFixture.OtroTenant, m.CodBar));
    }

    [Fact]
    public async Task Comodines_SeTratanComoTexto()
    {
        Assert.Empty(await BuscarAsync(OfficialLocalFixture.MasterIdTenant, "%_%"));
    }

    [Fact]
    public async Task SinTexto_DevuelveTodoElTenant()
    {
        using var ctx = fx.NewContext();
        var total = await ctx.Productos.CountAsync(p => p.IdTenant == OfficialLocalFixture.MasterIdTenant, Xunit.TestContext.Current.CancellationToken);
        var (_, t) = await new ProdRepositoryAsync(ctx).SearchPagedAsync(OfficialLocalFixture.MasterIdTenant, "  ", 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(total, t);
    }
}

