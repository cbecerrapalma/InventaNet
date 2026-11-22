using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.Application.Tests;

// S60.1: branding público (sanitización en el servicio, punto único StorefrontBranding).
// Fakes manuales mínimos (sin framework de mocks), mismo estilo que StubProdRepositoryAsync.
internal sealed class StubStorefrontRepositoryAsync : IStorefrontRepositoryAsync
{
    public Func<int, CancellationToken, Task<StorefrontProfileBundle?>>? GetPublishedProfile { get; set; }
    public Func<int, CancellationToken, Task<TenantStorefront?>>? GetProfile { get; set; }
    public Func<TenantStorefront, CancellationToken, Task<TenantStorefront>>? UpsertProfile { get; set; }

    public Task<StorefrontProfileBundle?> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default) =>
        GetPublishedProfile is null ? throw new NotImplementedException(nameof(GetPublishedProfileAsync)) : GetPublishedProfile(idTenant, ct);
    public Task<TenantStorefront?> GetProfileAsync(int idTenant, CancellationToken ct = default) =>
        GetProfile is null ? throw new NotImplementedException(nameof(GetProfileAsync)) : GetProfile(idTenant, ct);
    public Task<TenantStorefront> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default) =>
        UpsertProfile is null ? throw new NotImplementedException(nameof(UpsertProfileAsync)) : UpsertProfile(profile, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);
    private static Task NiM([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default) => NiM<StoreTenantResolution?>();
    public Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default) => NiM<IReadOnlyList<ActiveTenantItem>>();
    // S63: firma extendida (parámetros opcionales).
    public Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(int idTenant, string? q, int page, int pageSize, long? idCategoria, bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null, long? idMarca = null, string? orden = null, CancellationToken ct = default) => NiM<(int, IReadOnlyList<PublicCardItem>)>();
    public Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default) => NiM<PublicDetailBundle?>();
    public Task<IReadOnlyList<PublicCategoryItem>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<PublicCategoryItem>>();
    // S63: miembro nuevo de interfaz (GetPublicBrandsAsync).
    public Task<IReadOnlyList<PublicBrandItem>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<PublicBrandItem>>();
    public Task<IReadOnlyList<TenantSocialLink>> GetLinksAsync(int idTenant, bool soloActivas, CancellationToken ct = default) => NiM<IReadOnlyList<TenantSocialLink>>();
    public Task<IReadOnlyList<TenantBusinessHour>> GetHoursAsync(int idTenant, bool soloActivas, CancellationToken ct = default) => NiM<IReadOnlyList<TenantBusinessHour>>();
    public Task<TenantSocialLink> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default) => NiM<TenantSocialLink>();
    public Task<bool> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default) => NiM<bool>();
    public Task<IReadOnlyList<TenantBusinessHour>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default) => NiM<IReadOnlyList<TenantBusinessHour>>();
}

public sealed class StorefrontBrandingTests
{
    private static TenantStorefront Profile(int tenant = 4) => new()
    {
        IdTenant = tenant,
        NombreFantasia = "Tienda S60",
        ColorAcento = "#0F766E",
        FaviconUrl = "/img/tenant/fav.png",
        AnuncioTexto = "Retiro en tienda sin costo",
        PerfilStore = "General",
        Publicado = true,
    };

    private static StorefrontServiceAsync Sut(StubStorefrontRepositoryAsync repo) => new(repo, NullLogger<StorefrontServiceAsync>.Instance);

    // xUnit 2.9: Assert.NotNull retorna void → este helper desempaqueta el Result.
    private static StorefrontProfileBundle Bundle(CBP.Results.Result<StorefrontProfileBundle?> r)
    {
        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        return r.Value!;
    }

    private static StubStorefrontRepositoryAsync RepoCon(TenantStorefront p, string? moneda) =>
        new()
        {
            GetPublishedProfile = (_, _) => Task.FromResult<StorefrontProfileBundle?>(
                new StorefrontProfileBundle("ABARROTES", p, [], [], moneda)),
        };

    // 1. Perfil publicado devuelve branding con valores válidos.
    [Fact]
    public async Task PerfilPublicado_DevuelveBrandingValido()
    {
        var r = await Sut(RepoCon(Profile(), "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        var b = Bundle(r);
        Assert.Equal("#0F766E", b.Profile.ColorAcento);
        Assert.Equal("/img/tenant/fav.png", b.Profile.FaviconUrl);
        Assert.Equal("Retiro en tienda sin costo", b.Profile.AnuncioTexto);
        Assert.Equal("General", b.Profile.PerfilStore);
        Assert.Equal("CLP", b.Moneda);
    }

    // 2. colorAcento inválido → null; minúsculas → mayúsculas.
    [Theory]
    [InlineData("red", null)]
    [InlineData("#12", null)]
    [InlineData("#GGGGGG", null)]
    [InlineData("javascript:#0F766E", null)]
    [InlineData("#0f766e", "#0F766E")]
    [InlineData("#0F766E", "#0F766E")]
    [InlineData(null, null)]
    [InlineData("   ", null)]
    public async Task ColorAcento_SeSanitiza(string? entrada, string? esperado)
    {
        var p = Profile(); p.ColorAcento = entrada;
        var r = await Sut(RepoCon(p, "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(esperado, Bundle(r).Profile.ColorAcento);
    }

    // 3. faviconUrl con javascript:/data: → null; /x.png y https:// se conservan.
    [Theory]
    [InlineData("javascript:alert(1)", null)]
    [InlineData("data:image/png;base64,xx", null)]
    [InlineData("/x.png", "/x.png")]
    [InlineData("https://cdn.tienda.cl/fav.ico", "https://cdn.tienda.cl/fav.ico")]
    [InlineData("http://tienda.cl/fav.ico", "http://tienda.cl/fav.ico")]
    [InlineData("//evil/x.png", null)]
    public async Task FaviconUrl_SeSanitiza(string? entrada, string? esperado)
    {
        var p = Profile(); p.FaviconUrl = entrada;
        var r = await Sut(RepoCon(p, "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(esperado, Bundle(r).Profile.FaviconUrl);
    }

    // 4. anuncioTexto solo espacios → null; 250 caracteres → 200.
    [Fact]
    public async Task Anuncio_SoloEspacios_Null()
    {
        var p = Profile(); p.AnuncioTexto = "   ";
        var r = await Sut(RepoCon(p, "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Null(Bundle(r).Profile.AnuncioTexto);
    }

    [Fact]
    public async Task Anuncio_Largo_SeCortaA200()
    {
        var p = Profile(); p.AnuncioTexto = new string('a', 250);
        var r = await Sut(RepoCon(p, "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(200, Bundle(r).Profile.AnuncioTexto!.Length);
    }

    // 5. perfilStore desconocido → General; ausente → General.
    [Theory]
    [InlineData("Ropa", "General")]
    [InlineData("general", "General")]
    [InlineData("Apparel", "Apparel")]
    [InlineData("Grocery", "Grocery")]
    [InlineData("Pharmacy", "Pharmacy")]
    [InlineData("Hardware", "Hardware")]
    [InlineData(null, "General")]
    public async Task PerfilStore_SeSanitiza(string? entrada, string esperado)
    {
        var p = Profile(); p.PerfilStore = entrada!;
        var r = await Sut(RepoCon(p, "CLP")).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(esperado, Bundle(r).Profile.PerfilStore);
    }

    // 6. moneda: sin fila → CLP; "usd" → USD; basura → CLP.
    [Theory]
    [InlineData(null, "CLP")]
    [InlineData("usd", "USD")]
    [InlineData("CLP", "CLP")]
    [InlineData("pesos", "CLP")]
    [InlineData("XX1", "CLP")]
    public async Task Moneda_SeSanitiza(string? entrada, string esperada)
    {
        var r = await Sut(RepoCon(Profile(), entrada)).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(esperada, Bundle(r).Moneda);
    }

    // 7. Perfil no publicado → null (el controlador lo mapea a 204, sin cambios).
    [Fact]
    public async Task PerfilNoPublicado_RetornaNull()
    {
        var repo = new StubStorefrontRepositoryAsync
        {
            GetPublishedProfile = (_, _) => Task.FromResult<StorefrontProfileBundle?>(null),
        };
        var r = await Sut(repo).GetPublishedProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value);
    }

    // 8a. El guardado (Upsert) no altera las columnas de branding.
    [Fact]
    public async Task UpsertProfile_PreservaBranding()
    {
        TenantStorefront? guardado = null;
        var repo = new StubStorefrontRepositoryAsync
        {
            UpsertProfile = (p, _) => { guardado = p; return Task.FromResult(p); },
        };
        // Flujo backoffice (StorefrontController.Upsert): entidad cargada + asignación
        // solo de campos conocidos; las columnas S60 viajan intactas.
        var p = Profile();
        p.NombreFantasia = "Nuevo nombre";

        var r = await Sut(repo).UpsertProfileAsync(p, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.Equal("#0F766E", guardado!.ColorAcento);
        Assert.Equal("/img/tenant/fav.png", guardado.FaviconUrl);
        Assert.Equal("Retiro en tienda sin costo", guardado.AnuncioTexto);
        Assert.Equal("General", guardado.PerfilStore);
    }

    // 8b. La lectura de backoffice (GetProfileAsync) devuelve valores CRUDOS:
    // lo que el backoffice lee es lo que reescribe → roundtrip seguro.
    [Fact]
    public async Task GetProfile_Backoffice_DevuelveCrudoSinSanitizar()
    {
        var crudo = Profile(); crudo.ColorAcento = "red";
        var repo = new StubStorefrontRepositoryAsync
        {
            GetProfile = (_, _) => Task.FromResult<TenantStorefront?>(crudo),
        };
        var r = await Sut(repo).GetProfileAsync(4, Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.NotNull(r.Value);
        Assert.Equal("red", r.Value!.ColorAcento);
    }
}

