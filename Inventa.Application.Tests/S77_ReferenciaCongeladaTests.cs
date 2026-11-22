using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.Application.Tests;

// S77 R2'/R4': referencia congelada de ofertas (Oferta.IdPrecioReferencia).
// OfertaEfectiva + detalle con stubs en memoria. Sin BD, sin paquetes nuevos.
public sealed class S77_ReferenciaCongeladaTests
{
    private static Oferta O(long id, long pres, long idRef, decimal oferta, Precio? pref = null) =>
        new()
        {
            IdOferta = id,
            IdPresProd = pres,
            IdPrecioReferencia = idRef,
            PrecioOferta = oferta,
            FechaDesde = PublicCatalogSpecs.Hoy().AddDays(-10),
            FechaHasta = null,
            Activo = true,
            Publicado = true,
            PrecioReferencia = pref ?? new() { IdPrecio = idRef, IdPresProd = pres, Valor = 100m, FecVig = PublicCatalogSpecs.Hoy().AddDays(-20), Activo = true },
        };

    // ---- OfertaEfectiva ----
    [Fact]
    public void Efectiva_ReferenciaVigenteYOfertaMenor() =>
        Assert.True(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 10, 80m), 10, 100m));

    [Fact]
    public void NoEfectiva_ReferenciaNoVigente() =>
        Assert.False(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 9, 80m), 10, 100m));

    [Fact]
    public void NoEfectiva_OfertaMayorOIgualQueReferencia()
    {
        Assert.False(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 10, 100m), 10, 100m));
        Assert.False(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 10, 150m), 10, 100m));
    }

    [Fact]
    public void NoEfectiva_SinVigenteOSinReferencia()
    {
        Assert.False(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 10, 80m), null, 100m));
        Assert.False(PublicCatalogSpecs.OfertaEfectiva(O(1, 7, 10, 80m), 10, null));
    }

    // ---- Detalle (stub del repositorio) ----
    private sealed class DetailStub(PublicDetailBundle bundle) : IStorefrontRepositoryAsync
    {
        public Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default)
            => Task.FromResult<PublicDetailBundle?>(bundle);

        private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
            throw new NotImplementedException(m);
        private static Task NiM([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
            throw new NotImplementedException(m);

        public Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default) => NiM<StoreTenantResolution?>();
        public Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default) => NiM<IReadOnlyList<ActiveTenantItem>>();
        public Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default) => NiM<(int, IReadOnlyList<PublicCardItem>)>();
        public Task<IReadOnlyList<PublicCategoryItem>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<PublicCategoryItem>>();
        public Task<IReadOnlyList<PublicBrandItem>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<PublicBrandItem>>();
        public Task<StorefrontProfileBundle?> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default) => NiM<StorefrontProfileBundle?>();
        public Task<TenantStorefront?> GetProfileAsync(int idTenant, CancellationToken ct = default) => NiM<TenantStorefront?>();
        public Task<IReadOnlyList<TenantSocialLink>> GetLinksAsync(int idTenant, bool soloActivas, CancellationToken ct = default) => NiM<IReadOnlyList<TenantSocialLink>>();
        public Task<IReadOnlyList<TenantBusinessHour>> GetHoursAsync(int idTenant, bool soloActivas, CancellationToken ct = default) => NiM<IReadOnlyList<TenantBusinessHour>>();
        public Task<TenantStorefront> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default) => NiM<TenantStorefront>();
        public Task<TenantSocialLink> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default) => NiM<TenantSocialLink>();
        public Task<bool> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default) => NiM<bool>();
        public Task<IReadOnlyList<TenantBusinessHour>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default) => NiM<IReadOnlyList<TenantBusinessHour>>();
    }

    private static PublicDetailBundle Bundle(params Oferta[] ofertas)
    {
        var hoy = PublicCatalogSpecs.Hoy();
        return new PublicDetailBundle(
            1, "P-1", "Prod 1", 26, "Poleras", 27, "Marca propia",
            [new VarProd { IdVarProd = 11, IdProd = 1, Codigo = "P-1-V01", Nombre = "V01", Activo = true }],
            [new PresProd { IdPresProd = 100, IdVarProd = 11, Descrip = "Unidad", IdUnidad = 1, Activo = true, EsVenta = true }],
            [new Precio { IdPrecio = 10, IdPresProd = 100, Valor = 100m, FecVig = hoy.AddDays(-20), Activo = true }],
            ofertas,
            new Dictionary<int, string> { [1] = "Unidad" },
            null,
            []);
    }

    private static async Task<PublicDetailPresView> Pres(params Oferta[] ofertas)
    {
        var sut = new StorefrontServiceAsync(new DetailStub(Bundle(ofertas)), NullLogger<StorefrontServiceAsync>.Instance);
        var r = await sut.GetPublicDetailViewAsync(5, 1);
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        return Assert.Single(r.Value!.Presentaciones);
    }

    [Fact]
    public async Task Detalle_OfertaConReferenciaVigente_EfectivaYPorcentajeContraReferencia()
    {
        var pr = await Pres(O(1, 100, 10, 80m));
        Assert.Equal(80m, pr.PrecioFinal);
        var of = Assert.Single(pr.Ofertas);
        Assert.Equal(100m, of.PrecioReferenciaValor);
        Assert.True(of.ReferenciaEsVigente);
        Assert.Equal(20, of.PctDescuento);
        Assert.Equal(20, pr.PctDescuento);
    }

    [Fact]
    public async Task Detalle_OfertaConReferenciaNoVigente_Excluida()
    {
        var pr = await Pres(O(1, 100, 9, 80m, new Precio { IdPrecio = 9, IdPresProd = 100, Valor = 100m, FecVig = PublicCatalogSpecs.Hoy().AddDays(-30), Activo = true }));
        Assert.Empty(pr.Ofertas);
        Assert.Equal(100m, pr.PrecioFinal);
        Assert.Null(pr.PctDescuento);
    }

    [Fact]
    public async Task Detalle_SinOfertas_SinReferencia()
    {
        var pr = await Pres();
        Assert.Empty(pr.Ofertas);
        Assert.Equal(100m, pr.PrecioFinal);
        Assert.Null(pr.PctDescuento);
    }
}

