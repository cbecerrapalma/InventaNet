using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace Inventa.Application.Tests;

internal sealed class FakeStorefrontRepo : IStorefrontRepositoryAsync
{
    private readonly StorefrontProfileBundle? _profile;
    private readonly PublicDetailBundle? _detail;

    public FakeStorefrontRepo(StorefrontProfileBundle? profile = null, PublicDetailBundle? detail = null)
    {
        _profile = profile;
        _detail = detail;
    }

    public Task<StoreTenantResolution?> ResolveTenantByPassPlatAsync(int idTenantPassPlat, CancellationToken ct = default)
        => Task.FromResult<StoreTenantResolution?>(new StoreTenantResolution(1, true));

    public Task<IReadOnlyList<ActiveTenantItem>> GetActiveTenantsAsync(CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<ActiveTenantItem>>([]);

    public Task<(int Total, IReadOnlyList<PublicCardItem> Items)> GetPublicCardsAsync(
        int idTenant, string? q, int page, int pageSize, long? idCategoria,
        bool soloOfertas = false, decimal? precioMin = null, decimal? precioMax = null,
        long? idMarca = null, string? orden = null, CancellationToken ct = default)
        => Task.FromResult<(int, IReadOnlyList<PublicCardItem>)>((0, []));

    public Task<PublicDetailBundle?> GetPublicDetailAsync(int idTenant, long idProd, CancellationToken ct = default)
        => Task.FromResult(_detail);

    public Task<IReadOnlyList<PublicCategoryItem>> GetPublicCategoriesAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PublicCategoryItem>>([]);

    public Task<IReadOnlyList<PublicBrandItem>> GetPublicBrandsAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<PublicBrandItem>>([]);

    public Task<StorefrontProfileBundle?> GetPublishedProfileAsync(int idTenant, CancellationToken ct = default)
    {
        // Simula el filtro por Publicado del repo real
        if (_profile?.Profile?.Publicado == true)
            return Task.FromResult<StorefrontProfileBundle?>(_profile);
        return Task.FromResult<StorefrontProfileBundle?>(null);
    }

    public Task<TenantStorefront?> GetProfileAsync(int idTenant, CancellationToken ct = default)
        => Task.FromResult<TenantStorefront?>(null);

    public Task<IReadOnlyList<TenantSocialLink>> GetLinksAsync(int idTenant, bool soloActivas, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<TenantSocialLink>>([]);

    public Task<IReadOnlyList<TenantBusinessHour>> GetHoursAsync(int idTenant, bool soloActivas, CancellationToken ct = default)
        => Task.FromResult<IReadOnlyList<TenantBusinessHour>>([]);

    public Task<TenantStorefront> UpsertProfileAsync(TenantStorefront profile, CancellationToken ct = default)
        => Task.FromResult(profile);

    public Task<TenantSocialLink> AddLinkAsync(TenantSocialLink link, CancellationToken ct = default)
        => Task.FromResult(link);

    public Task<bool> RemoveLinkAsync(int idTenant, int idLink, CancellationToken ct = default)
        => Task.FromResult(true);

    public Task<IReadOnlyList<TenantBusinessHour>> ReplaceHoursAsync(int idTenant, IReadOnlyList<TenantBusinessHour> hours, CancellationToken ct = default)
        => Task.FromResult(hours);
}

internal sealed class FakeEmailSender : IContactoEmailSender
{
    private readonly Result<ContactoEmailResult> _result;
    public Task<Result<ContactoEmailResult>> LastCall { get; private set; } = Task.FromResult(Result<ContactoEmailResult>.Success(new ContactoEmailResult("TRACK-123", 1)));
    public int CallCount { get; private set; }
    public string LastTemplateCode { get; private set; } = "";
    public string LastDestinatario { get; private set; } = "";
    public Dictionary<string, string> LastVariables { get; private set; } = new();

    public FakeEmailSender(Result<ContactoEmailResult>? result = null)
    {
        _result = result ?? Result<ContactoEmailResult>.Success(new ContactoEmailResult("TRACK-123", 1));
    }

    public Task<Result<ContactoEmailResult>> SendAsync(
        string codigoPlantilla,
        string destinatario,
        string nombreDestinatario,
        Dictionary<string, string> variables,
        string cultura,
        CancellationToken ct = default)
    {
        CallCount++;
        LastTemplateCode = codigoPlantilla;
        LastDestinatario = destinatario;
        LastVariables = variables;
        LastCall = Task.FromResult(_result);
        return LastCall;
    }
}

public sealed class ContactoServiceAsyncTests
{
    private static StorefrontProfileBundle ProfileWithEmail(string email = "tienda@ejemplo.cl", string nombreFantasia = "Mi Tienda")
    {
        var tenant = new TenantStorefront
        {
            IdTenant = 1,
            NombreFantasia = nombreFantasia,
            Email = email,
            Publicado = true
        };
        return new StorefrontProfileBundle("Tenant Legal", tenant, [], [], "CLP");
    }

    private static StorefrontProfileBundle ProfileWithoutEmail()
    {
        var tenant = new TenantStorefront
        {
            IdTenant = 1,
            NombreFantasia = "Mi Tienda",
            Email = null,
            Publicado = true
        };
        return new StorefrontProfileBundle("Tenant Legal", tenant, [], [], "CLP");
    }

    private static StorefrontProfileBundle UnpublishedProfile(string email = "tienda@ejemplo.cl")
    {
        var tenant = new TenantStorefront
        {
            IdTenant = 1,
            NombreFantasia = "Mi Tienda",
            Email = email,
            Publicado = false
        };
        return new StorefrontProfileBundle("Tenant Legal", tenant, [], [], "CLP");
    }

    private static PublicContactRequest ValidRequest(long? productoId = null)
        => new PublicContactRequest(
            Nombre: "Juan Pérez",
            Email: "juan@ejemplo.cl",
            Telefono: "+56912345678",
            Mensaje: "Hola, quisiera información sobre sus productos.",
            Consentimiento: true,
            ProductoId: productoId,
            Website: "");

    [Fact]
    public async Task PerfilSinEmail_ReturnsNoConfigurado()
    {
        var repo = new FakeStorefrontRepo(ProfileWithoutEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var r = await sut.EnviarAsync(999, ValidRequest(), null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", r.Error?.Code);
        Assert.Equal(503, r.Error?.HttpStatusCode);
        Assert.Equal(0, sender.CallCount);
    }

    [Fact]
    public async Task PerfilNoPublicado_ReturnsNoConfigurado()
    {
        var repo = new FakeStorefrontRepo(UnpublishedProfile());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var r = await sut.EnviarAsync(999, ValidRequest(), null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_NO_CONFIGURADO", r.Error?.Code);
        Assert.Equal(503, r.Error?.HttpStatusCode);
        Assert.Equal(0, sender.CallCount);
    }

    [Fact]
    public async Task HoneypotLleno_Returns202SinEnviar()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Website = "http://spam.com" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Null(r.Value?.TrackingId);
        Assert.Null(r.Value?.IdEmailLog);
        Assert.Equal(0, sender.CallCount);
    }

    [Fact]
    public async Task ProductoNoPublicable_IgnoradoVariablesVacias()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail(), null);
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest(productoId: 42);

        var r = await sut.EnviarAsync(999, req, "https://store.example.com/categoria", CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Equal(1, sender.CallCount);
        Assert.Equal("inventa-store-contacto", sender.LastTemplateCode);
        Assert.Equal("tienda@ejemplo.cl", sender.LastDestinatario);
        Assert.Equal("", sender.LastVariables["ProductoNombre"]);
        Assert.Equal("", sender.LastVariables["ProductoUrl"]);
        Assert.Equal("https://store.example.com/categoria", sender.LastVariables["OrigenUrl"]);
        Assert.Contains("NombreTienda", sender.LastVariables.Keys);
        Assert.Contains("NombreContacto", sender.LastVariables.Keys);
        Assert.Contains("EmailContacto", sender.LastVariables.Keys);
        Assert.Contains("Telefono", sender.LastVariables.Keys);
        Assert.Contains("Mensaje", sender.LastVariables.Keys);
        Assert.Contains("FechaHora", sender.LastVariables.Keys);
        // es-CL usa dd/MM/yyyy HH:mm, pero en algunos entornos puede ser dd-MM-yyyy HH:mm
Assert.Matches(@"^\d{2}[-/]\d{2}[-/]\d{4} \d{2}:\d{2}$", sender.LastVariables["FechaHora"]);
    }

    [Fact]
    public async Task ProductoPublicable_EnriqueceVariables()
    {
        var detail = new PublicDetailBundle(
            IdProd: 42,
            Codigo: "PROD-42",
            Nombre: "Producto Test",
            IdCategoria: 1,
            CatNombre: "Cat",
            IdMarca: null,
            MarcaNombre: null,
            Variantes: [],
            Presentaciones: [],
            Precios: [],
            OfertasVigentes: [],
            Unidades: new Dictionary<int, string>(),
            ImagenPrincipalUrl: null,
            Imagenes: []);

        var repo = new FakeStorefrontRepo(ProfileWithEmail(), detail);
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest(productoId: 42);

        var r = await sut.EnviarAsync(999, req, "https://store.example.com/categoria", CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Equal(1, sender.CallCount);
        Assert.Equal("Producto Test", sender.LastVariables["ProductoNombre"]);
        Assert.Equal("/productos/42", sender.LastVariables["ProductoUrl"]);
    }

    [Fact]
    public async Task DestinatarioSiempreEmailTienda_NoControladoPorCliente()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail("tienda-oficial@dominio.cl"));
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Email = "atacante@malicioso.com" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.Equal("tienda-oficial@dominio.cl", sender.LastDestinatario);
        Assert.NotEqual("atacante@malicioso.com", sender.LastDestinatario);
        Assert.Equal("atacante@malicioso.com", sender.LastVariables["EmailContacto"]);
    }

    [Fact]
    public async Task ValidacionNombreMuyCorto_ReturnsValidacion()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Nombre = "J" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_VALIDACION", r.Error?.Code);
        Assert.Equal(400, r.Error?.HttpStatusCode);
        Assert.Contains("Nombre", r.Error?.Message);
    }

    [Fact]
    public async Task ValidacionEmailInvalido_ReturnsValidacion()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Email = "no-es-email" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_VALIDACION", r.Error?.Code);
        Assert.Contains("Email", r.Error?.Message);
    }

    [Fact]
    public async Task ValidacionMensajeMuyCorto_ReturnsValidacion()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Mensaje = "Corto" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_VALIDACION", r.Error?.Code);
        Assert.Contains("Mensaje", r.Error?.Message);
    }

    [Fact]
    public async Task ValidacionSinConsentimiento_ReturnsValidacion()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Consentimiento = false };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_VALIDACION", r.Error?.Code);
        Assert.Contains("Consentimiento", r.Error?.Message);
    }

    [Fact]
    public async Task TelefonoInvalido_ReturnsValidacion()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Telefono = "abc-def" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("CONTACTO_VALIDACION", r.Error?.Code);
        Assert.Contains("Telefono", r.Error?.Message);
    }

    [Fact]
    public async Task TelefonoValidoConFormato_Pasa()
    {
        var repo = new FakeStorefrontRepo(ProfileWithEmail());
        var sender = new FakeEmailSender();
        var sut = new ContactoServiceAsync(
            new StorefrontServiceAsync(repo, NullLogger<StorefrontServiceAsync>.Instance),
            sender,
            NullLogger<ContactoServiceAsync>.Instance);

        var req = ValidRequest() with { Telefono = "+56 9 1234 5678" };

        var r = await sut.EnviarAsync(999, req, null, CancellationToken.None);

        Assert.True(r.IsSuccess);
    }
}
