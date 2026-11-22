using System.Net;
using System.Text;
using Inventa.Store.Models;
using Inventa.Store.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>
/// S61.5 §3.7: JSON que envía el servidor (camelCase ASP.NET) con y sin branding.
/// </summary>
public sealed class StorefrontBrandingContractTests
{
    // Forma real del servidor: camelCase (AddJsonOptions sin NamingPolicy => camelCase).
    const string SinBranding = """
        {"nombreLegal":"ABARROTES S.A.","nombreFantasia":null,"logoUrl":"/img/logo.png",
         "descripcion":null,"contacto":null,"redes":[],"horarios":[],
         "moneda":"CLP","anuncio":null,"politicas":null}
        """;

    const string ConBranding = """
        {"nombreLegal":"ABARROTES S.A.","nombreFantasia":null,"logoUrl":"/img/logo.png",
         "descripcion":null,"contacto":null,"redes":[],"horarios":[],
         "moneda":"CLP","anuncio":null,"politicas":null,
         "branding":{"colorAcento":"#0F766E","faviconUrl":"/img/fav.ico",
                     "anuncioTexto":"OFERTA","perfilStore":"General","moneda":"CLP"}}
        """;

    sealed class JsonHandler(string json) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            });
    }

    sealed class FakeConfig(Dictionary<string, string?> values) : IConfiguration
    {
        public string? this[string key]
        {
            get => values.TryGetValue(key, out var v) ? v : null;
            set => values[key] = value;
        }
        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new FakeSection(key, this[key]);
    }

    sealed class FakeSection(string key, string? value) : IConfigurationSection
    {
        public string Key { get; } = key;
        public string Path => Key;
        public string? Value { get; set; } = value;
        public string? this[string key] { get => null; set { } }
        public IEnumerable<IConfigurationSection> GetChildren() => [];
        public IChangeToken GetReloadToken() => new CancellationChangeToken(CancellationToken.None);
        public IConfigurationSection GetSection(string key) => new FakeSection(key, null);
    }

    static StorefrontState BuildState(string json)
    {
        var config = new FakeConfig(new Dictionary<string, string?>
        {
            ["ApiBaseUrl"] = "https://localhost:5002",
        });
        var api = new StorefrontApiService(
            new HttpClient(new JsonHandler(json)) { BaseAddress = new Uri("https://localhost:5002") });
        return new StorefrontState(api, config);
    }

    [Fact]
    public async Task SinBranding_Estado_Valores_Por_Defecto()
    {
        var state = BuildState(SinBranding);
        var perfil = await state.GetAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.NotNull(perfil);
        Assert.Equal("CLP", state.Moneda);
        Assert.Null(state.Branding.ColorAcento);
        Assert.Null(state.Branding.FaviconUrl);
        Assert.Null(state.Branding.AnuncioTexto);
        Assert.Equal("General", state.Branding.PerfilStore);
    }

    [Fact]
    public async Task ConBranding_Estado_Poblado()
    {
        var state = BuildState(ConBranding);
        _ = await state.GetAsync(Xunit.TestContext.Current.CancellationToken);
        Assert.Equal("#0F766E", state.Branding.ColorAcento);
        Assert.Equal("https://localhost:5002/img/fav.ico",
            state.ResolveAssetUrl(state.Branding.FaviconUrl));
        Assert.Equal("OFERTA", state.Branding.AnuncioTexto);
        Assert.Equal("CLP", state.Moneda);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", null)]
    [InlineData("https://cdn.x/y.png", "https://cdn.x/y.png")]
    [InlineData("//cdn.x/y.png", "https://cdn.x/y.png")]
    [InlineData("/img/a.png", "https://localhost:5002/img/a.png")]
    public void ResolveAssetUrl_Casos(string? entrada, string? esperado) =>
        Assert.Equal(esperado, BuildState(SinBranding).ResolveAssetUrl(entrada));
}

