using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.Application.Tests;

internal sealed class ThrowingStorefrontRepo : IStorefrontRepositoryAsync
{
    private readonly Exception _ex;

    public ThrowingStorefrontRepo(Exception ex) => _ex = ex;

    public Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(
        int idTenant, string? q, int page, int pageSize, long? idCategoria,
        bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null,
        long? idMarca = null, string? orden = null, CancellationToken ct = default)
        => throw _ex;

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

public sealed class StorefrontService503Tests
{
    [Fact]
    public async Task CatalogoNoDisponibleException_Returns503Error()
    {
        var repo = new ThrowingStorefrontRepo(new CatalogoNoDisponibleException("CATALOGO_NO_DISPONIBLE", "Timeout"));
        var sut = new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance);

        var r = await sut.GetPublicCardsAsync(1, null, 1, 20, null, ct: CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CATALOGO_NO_DISPONIBLE", r.Error?.Code);
        Assert.Equal(503, r.Error?.HttpStatusCode);
        Assert.Equal(ErrorType.Database, r.Error?.Type);
    }

    [Fact]
    public async Task OtherException_NotCaught_Throws()
    {
        var repo = new ThrowingStorefrontRepo(new InvalidOperationException("boom"));
        var sut = new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.GetPublicCardsAsync(1, null, 1, 20, null, ct: CancellationToken.None));
    }
}
