using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.Application.Tests;

// S63: normalización del servicio (patrón existente: inválido => default).
// Fake capturador mínimo del repositorio.
internal sealed class CapturingStorefrontRepo : IStorefrontRepositoryAsync
{
    public (bool Solo, decimal? Min, decimal? Max, long? Marca, string? Orden)? CardsArgs;

    public Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(
        int idTenant, string? q, int page, int pageSize, long? idCategoria,
        bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null,
        long? idMarca = null, string? orden = null, CancellationToken ct = default)
    {
        CardsArgs = (soloOfertas, precioMin, precioMax, idMarca, orden);
        return Task.FromResult<(int, IReadOnlyList<PublicCardItem>)>((0, []));
    }

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);
    private static Task NiM([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default) => NiM<StoreTenantResolution?>();
    public Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default) => NiM<IReadOnlyList<ActiveTenantItem>>();
    public Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default) => NiM<PublicDetailBundle?>();
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

public sealed class StorefrontCardsNormalizationTests
{
    private static async Task<(bool Solo, decimal? Min, decimal? Max, long? Marca, string? Orden)> Args(
        bool solo = false, decimal? min = null, decimal? max = null, long? marca = null, string? orden = null)
    {
        var repo = new CapturingStorefrontRepo();
        var sut = new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance);
        await sut.GetPublicCardsAsync(4, null, 1, 20, null, solo, min, max, marca, orden);
        return repo.CardsArgs!.Value;
    }

    [Fact] public async Task PrecioMinNegativo_SeIgnora() =>
        Assert.Null((await Args(min: -5m)).Min);

    [Fact] public async Task PrecioMaxNegativo_SeIgnora() =>
        Assert.Null((await Args(max: -1m)).Max);

    [Fact]
    public async Task MinMayorQueMax_Swap()
    {
        var a = await Args(min: 500m, max: 100m);
        Assert.Equal(100m, a.Min);
        Assert.Equal(500m, a.Max);
    }

    [Fact] public async Task IdMarcaCeroONegativo_SeIgnora()
    {
        Assert.Null((await Args(marca: 0)).Marca);
        Assert.Null((await Args(marca: -3)).Marca);
    }

    [Fact] public async Task IdMarcaValido_Pasa() =>
        Assert.Equal(7, (await Args(marca: 7)).Marca);

    [Theory]
    [InlineData(null, "defecto")]
    [InlineData("raro", "defecto")]
    [InlineData("PRECIO_ASC", "defecto")]
    [InlineData("precio_asc", "precio_asc")]
    [InlineData("precio_desc", "precio_desc")]
    [InlineData("descuento_desc", "descuento_desc")]
    [InlineData("nombre_asc", "nombre_asc")]
    public async Task Orden_Normaliza(string? orden, string esperado) =>
        Assert.Equal(esperado, (await Args(orden: orden)).Orden);

    [Fact] public async Task SoloOfertas_Pasa() =>
        Assert.True((await Args(solo: true)).Solo);
}

