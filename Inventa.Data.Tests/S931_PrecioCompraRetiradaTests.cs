using Inventa.Data.Async.Repositories;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Inventa.Data.Tests;

/// <summary>
/// S93.1 C9 — regresión de F1 (S93 borró dbo.PrecioCompra con código que aún la usaba):
/// el modelo EF ya no la mapea, y la ficha y la vista previa de importación funcionan sin ella.
/// </summary>
[Collection("OfficialLocal")]
public sealed class S931_PrecioCompraRetiradaTests(OfficialLocalFixture fx)
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public void ModeloEf_NoMapeaPrecioCompra()
    {
        using var ctx = fx.NewContext();
        Assert.DoesNotContain(ctx.Model.GetEntityTypes(), e => e.GetTableName() == "PrecioCompra" || e.ClrType.Name == "PrecioCompra");
    }

    [Fact]
    public async Task FichaYVistaPreviaDeImportacion_NoUsanPrecioCompra()
    {
        using var ctx = fx.NewContext();
        var tenant = OfficialLocalFixture.MasterIdTenant;
        var idProd = await fx.MasterIdProdAsync();

        var ficha = await new ProductoFichaRepositoryAsync(ctx).GetFichaAsync(tenant, idProd, Ct);
        Assert.NotNull(ficha);

        var comercial = await new ProductoImportSpRepositoryAsync(ctx)
            .GetEstadoComercialAsync(tenant, Ct);
        Assert.NotEmpty(comercial.Presentaciones);
    }
}
