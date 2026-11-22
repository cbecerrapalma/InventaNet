using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Inventa.WebAPI.Controllers;
using Xunit;

namespace Inventa.WebAPI.Tests;

// Minimal IConfiguration implementation for tests
internal sealed class TestConfig : IConfiguration
{
    private readonly Dictionary<string, string?> _dict = new();
    public TestConfig(string key, string? value) => _dict[key] = value;
    public string? this[string key] { get => _dict.TryGetValue(key, out var v) ? v : null; set => _dict[key] = value; }
    public IEnumerable<IConfigurationSection> GetChildren() => [];
    public IChangeToken GetReloadToken() => new CancellationChangeToken(new CancellationToken());
    public IConfigurationSection GetSection(string key) => new TestSection(_dict, key);
}

internal sealed class TestSection : IConfigurationSection
{
    private readonly Dictionary<string, string?> _dict;
    private readonly string _key;
    public TestSection(Dictionary<string, string?> dict, string key) { _dict = dict; _key = key; }
    public string Key => _key.Split(':').Last();
    public string Path => _key;
    public string? Value { get => _dict.TryGetValue(_key, out var v) ? v : null; set => _dict[_key] = value; }
    public IEnumerable<IConfigurationSection> GetChildren() => [];
    public IChangeToken GetReloadToken() => new CancellationChangeToken(new CancellationToken());
    public IConfigurationSection GetSection(string key) => new TestSection(_dict, _key + ":" + key);
    public string? this[string key] { get => _dict.TryGetValue(_key + ":" + key, out var v) ? v : null; set => _dict[_key + ":" + key] = value; }
}

internal sealed class FakeStorefrontSvc : IStorefrontServiceAsync
{
    private readonly Result<(int Total, IReadOnlyList<PublicCardItem> Items)> _cardsResult;
    private readonly Result<int> _tenantResult;

    public FakeStorefrontSvc(Result<(int, IReadOnlyList<PublicCardItem>)> cards, Result<int> tenant)
    {
        _cardsResult = cards;
        _tenantResult = tenant;
    }

    public Task<Result<int>> ResolveStoreTenantAsync(int idTenantPassPlat, CancellationToken ct = default) => Task.FromResult(_tenantResult);
    public Task<Result<IReadOnlyList<ActiveTenantItem>>> GetActiveTenantsAsync(CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<(int Total, IReadOnlyList<PublicCardItem> Items)>> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default) => Task.FromResult(_cardsResult);
    public Task<Result<PublicDetailView?>> GetPublicDetailViewAsync(int idTenant, long idProd, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<PublicCategoryItem>>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<PublicBrandItem>>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<StorefrontProfileBundle?>> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<TenantStorefront?>> GetProfileAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<TenantSocialLink>>> GetLinksAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<TenantBusinessHour>>> GetHoursAsync(int idTenant, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<TenantStorefront>> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<TenantSocialLink>> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<bool>> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default) => throw new NotImplementedException();
    public Task<Result<IReadOnlyList<TenantBusinessHour>>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default) => throw new NotImplementedException();
}

public sealed class PublicCatalog503Tests
{
    private static PublicCatalogController CreateController(IStorefrontServiceAsync svc, IConfiguration cfg)
    {
        var ctrl = new PublicCatalogController(svc, cfg);
        ctrl.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };
        return ctrl;
    }

    [Fact]
    public async Task ServiceReturnsCatalogoNoDisponible_ControllerReturns503WithRetryAfter()
    {
        var err = new Error("CATALOGO_NO_DISPONIBLE", "El catálogo no está disponible en este momento.").WithType(ErrorType.Database).WithHttpStatus(503);
        var svc = new FakeStorefrontSvc(
            Result<(int, IReadOnlyList<PublicCardItem>)>.Failure(err),
            Result<int>.Success(4));
        var cfg = new TestConfig("PublicCatalog:PassPlatTenantId", "99");
        var ctrl = CreateController(svc, cfg);

        var res = await ctrl.GetProducts(q: null, page: 1, pageSize: 20, idCategoria: null, ct: CancellationToken.None);

        var obj = Assert.IsType<ObjectResult>(res);
        Assert.Equal(503, obj.StatusCode);
        Assert.Equal("5", ctrl.Response.Headers["Retry-After"].ToString());
    }

    [Fact]
    public async Task ServiceReturnsOtherError_ControllerReturns500()
    {
        var err = new Error("OTHER", "algo").WithType(ErrorType.InternalError).WithHttpStatus(500);
        var svc = new FakeStorefrontSvc(
            Result<(int, IReadOnlyList<PublicCardItem>)>.Failure(err),
            Result<int>.Success(4));
        var cfg = new TestConfig("PublicCatalog:PassPlatTenantId", "99");
        var ctrl = CreateController(svc, cfg);

        var res = await ctrl.GetProducts(q: null, page: 1, pageSize: 20, idCategoria: null, ct: CancellationToken.None);

        var obj = Assert.IsType<StatusCodeResult>(res);
        Assert.Equal(500, obj.StatusCode);
    }

    [Fact]
    public async Task ServiceReturnsSuccess_ControllerReturnsOk()
    {
        var svc = new FakeStorefrontSvc(
            Result<(int, IReadOnlyList<PublicCardItem>)>.Success((0, Array.Empty<PublicCardItem>())),
            Result<int>.Success(4));
        var cfg = new TestConfig("PublicCatalog:PassPlatTenantId", "99");
        var ctrl = CreateController(svc, cfg);

        var res = await ctrl.GetProducts(q: null, page: 1, pageSize: 20, idCategoria: null, ct: CancellationToken.None);

        Assert.IsType<OkObjectResult>(res);
    }
}
