using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S88.2: verificación de la carga Ficcus en VESTUARIO (tenant 5).
/// Conversión de los controles V1–V10 de Inventa_S88_110 (ese .sql queda
/// solo como evidencia de solo lectura). Consultas EF/LINQ, sin SQL en texto.
/// Se omiten (Skip) si VESTUARIO no tiene la carga (DatosFiccusFactAttribute).
/// </summary>
[Trait("Category", "DatosFiccus")]
[Collection("OfficialLocal")]
public sealed class S88_DatosFiccusTests(OfficialLocalFixture fx)
{
    private const int Tenant = 5;

    [DatosFiccusFact]
    public async Task Ficcus_ConteosCarga()
    {
        using var ctx = fx.NewContext();
        Assert.Equal(2754, await ctx.Productos.CountAsync(p => p.IdTenant == Tenant));
        Assert.Equal(12114, await ctx.Variantes.CountAsync(v => v.Prod.IdTenant == Tenant));
        Assert.Equal(12114, await ctx.Presentaciones.CountAsync(pp => pp.VarProd.Prod.IdTenant == Tenant));
        Assert.Equal(12114, await ctx.Precios.CountAsync(pr => pr.PresProd.VarProd.Prod.IdTenant == Tenant));
    }

    [DatosFiccusFact]
    public async Task Ficcus_Principales()
    {
        using var ctx = fx.NewContext();
        var princPres = await ctx.CodigosBarra
            .Where(c => c.Principal && c.PresProd.VarProd.Prod.IdTenant == Tenant)
            .Select(c => c.IdPresProd)
            .ToListAsync();
        Assert.Equal(12114, princPres.Count);
        Assert.Equal(12114, princPres.Distinct().Count());
        var todas = await ctx.Presentaciones
            .Where(pp => pp.VarProd.Prod.IdTenant == Tenant)
            .Select(pp => pp.IdPresProd)
            .ToListAsync();
        Assert.Empty(todas.Except(princPres));
    }

    [DatosFiccusFact]
    public async Task Ficcus_TipoCod()
    {
        using var ctx = fx.NewContext();
        var grupos = await ctx.CodigosBarra
            .Where(c => c.Principal && c.PresProd.VarProd.Prod.IdTenant == Tenant)
            .GroupBy(c => c.TipoCod)
            .Select(g => new { Tipo = g.Key, N = g.Count() })
            .ToListAsync();
        // Fuente: 12107 códigos de 11 dígitos (4=Proveedor) + 7 EAN-13 válidos (1).
        Assert.Equal(7, grupos.Where(g => g.Tipo == 1).Sum(g => g.N));
        Assert.Equal(12107, grupos.Where(g => g.Tipo == 4).Sum(g => g.N));
        Assert.Equal(12114, grupos.Sum(g => g.N));
    }

    [DatosFiccusFact]
    public async Task Ficcus_JerarquiaCategorias()
    {
        using var ctx = fx.NewContext();
        var cats = await ctx.Categorias
            .Where(c => c.IdTenant == Tenant)
            .Select(c => new { c.Codigo, c.IdPadre })
            .ToListAsync();
        var raices = cats.Where(c => c.IdPadre == null).ToList();
        var hijas = cats.Where(c => c.IdPadre != null).ToList();
        Assert.Single(raices);
        Assert.Equal("VESTUARIO", raices[0].Codigo);
        Assert.Equal(22, hijas.Count);
    }

    [DatosFiccusFact]
    public async Task Ficcus_MarcaFiccus()
    {
        using var ctx = fx.NewContext();
        Assert.Equal(1, await ctx.Marcas.CountAsync(m => m.IdTenant == Tenant && m.Codigo == "FICCUS"));
    }

    [DatosFiccusFact]
    public async Task Ficcus_OfertasConReferenciaVigente()
    {
        using var ctx = fx.NewContext();
        var ofertas = await ctx.Ofertas
            .Where(o => o.PresProd.VarProd.Prod.IdTenant == Tenant && o.Activo)
            .Select(o => new { o.IdPresProd, o.IdPrecioReferencia, o.FechaDesde })
            .ToListAsync();
        // Fuente: 5992 variantes con price < listPrice.
        Assert.Equal(5992, ofertas.Count);
        var precios = await ctx.Precios
            .Where(pr => pr.PresProd.VarProd.Prod.IdTenant == Tenant && pr.Activo)
            .Select(pr => new { pr.IdPrecio, pr.IdPresProd, pr.FecVig })
            .ToListAsync();
        var vigentePorPres = precios
            .GroupBy(p => p.IdPresProd)
            .ToDictionary(g => g.Key, g => g.OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio).First().IdPrecio);
        var sinRefVigente = ofertas
            .Where(o => !vigentePorPres.TryGetValue(o.IdPresProd, out var vig) || vig != o.IdPrecioReferencia)
            .ToList();
        Assert.Empty(sinRefVigente);
        // Carga única: todas con la misma FechaDesde (fecha de carga).
        Assert.Single(ofertas.Select(o => o.FechaDesde).Distinct());
    }

    [DatosFiccusFact]
    public async Task Ficcus_SinFugaEn4y6()
    {
        using var ctx = fx.NewContext();
        var cods5 = new HashSet<string>(await ctx.CodigosBarra
            .Where(c => c.PresProd.VarProd.Prod.IdTenant == Tenant)
            .Select(c => c.Codigo)
            .ToListAsync());
        var cods46 = await ctx.CodigosBarra
            .Where(c => c.PresProd.VarProd.Prod.IdTenant == 4 || c.PresProd.VarProd.Prod.IdTenant == 6)
            .Select(c => c.Codigo)
            .ToListAsync();
        Assert.DoesNotContain(cods46, c => cods5.Contains(c));
    }

    [DatosFiccusFact]
    public async Task Ficcus_SinCostoInventado()
    {
        using var ctx = fx.NewContext();
        Assert.Equal(0, await ctx.PreciosMayorista.CountAsync(m => m.PresProd.VarProd.Prod.IdTenant == Tenant));
        Assert.Equal(0, await ctx.Variantes.CountAsync(v => v.Prod.IdTenant == Tenant && v.CostoProm != 0));
    }
}

