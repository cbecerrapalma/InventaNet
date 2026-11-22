using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S58.04 P0-D: reglas del resolver de oferta comercial (S57.56/62/69).
// DB-free con fakes manuales. Garantía: precedencia, vigencia temporal,
// política Publicado (M6) y tenant isolation del resolver.
// Legacy: s57.56-diagnostic (endpoint E2E) → C# COVERAGE parcial (el spec NO se elimina).
public sealed class CommercialOfferResolverTests
{
    private static readonly DateOnly Hoy = new(2026, 10, 6);

    private static (CommercialOfferResolverAsync Sut, StubOfertaRepositoryAsync Ofertas, StubPrecioRepositoryAsync Precios)
        Sut(int tenantPres = 4, IReadOnlyList<Oferta>? ofertas = null, Precio? referencia = null)
    {
        var pres = new StubPresProdRepositoryAsync
        {
            GetTenantId = (_, _) => Task.FromResult<int?>(tenantPres),
        };
        var of = new StubOfertaRepositoryAsync
        {
            GetByPresProd = (_, _) => Task.FromResult(ofertas ?? (IReadOnlyList<Oferta>)[]),
        };
        var pr = new StubPrecioRepositoryAsync
        {
            GetById = (_, _) => Task.FromResult(referencia),
        };
        return (new CommercialOfferResolverAsync(of, pr, pres), of, pr);
    }

    private static Oferta Vigente(bool publicado = true, bool activo = true, DateOnly? hasta = null) => new()
    {
        IdOferta = 1, IdPresProd = 2, IdPrecioReferencia = 4,
        FechaDesde = new DateOnly(2026, 1, 1), FechaHasta = hasta,
        PrecioOferta = 1200m, Activo = activo, Publicado = publicado,
    };

    private static Precio Referencia() => new()
    {
        IdPrecio = 4, IdPresProd = 2, Valor = 1500m, Activo = true,
    };

    [Fact]
    public async Task TenantInvalido_400()
    {
        var (sut, _, _) = Sut();
        var r = await sut.ResolveAsync(0, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsFailure);
        Assert.Equal(400, r.Error!.HttpStatusCode);
    }

    [Fact]
    public async Task PresProdInvalido_400()
    {
        var (sut, _, _) = Sut();
        var r = await sut.ResolveAsync(4, 0, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsFailure);
        Assert.Equal(400, r.Error!.HttpStatusCode);
    }

    [Fact]
    public async Task CrossTenant_404_PresProdNotFound()
    {
        var (sut, _, _) = Sut(tenantPres: 5);
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsFailure);
        Assert.Equal("PRES_PROD_NOT_FOUND", r.Error!.Code);
        Assert.Equal(404, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task SinOfertas_NoTieneOferta()
    {
        var (sut, _, _) = Sut();
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.TieneOferta);
        Assert.Equal(CommercialOfferSource.None, r.Value.Source);
    }

    [Fact]
    public async Task VigentePublicada_ResuelveNew_ConValorOferta()
    {
        var (sut, _, _) = Sut(ofertas: [Vigente()], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.True(r.Value.TieneOferta);
        Assert.Equal(CommercialOfferSource.New, r.Value.Source);
        Assert.Equal(1200m, r.Value.Valor);
    }

    [Fact]
    public async Task NoPublicada_HonorPublicado_NoElegible()
    {
        var (sut, _, _) = Sut(ofertas: [Vigente(publicado: false)], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(true), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.TieneOferta);
        Assert.Equal(CommercialOfferSource.None, r.Value.Source);
    }

    [Fact]
    public async Task NoPublicada_SinHonor_ElegibleComportamientoPublico()
    {
        var (sut, _, _) = Sut(ofertas: [Vigente(publicado: false)], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.True(r.Value.TieneOferta);
    }

    [Fact]
    public async Task Vencida_NoResuelve()
    {
        var (sut, _, _) = Sut(ofertas: [Vigente(hasta: new DateOnly(2026, 1, 2))], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.TieneOferta);
    }

    [Fact]
    public async Task Inactiva_NoResuelve()
    {
        var (sut, _, _) = Sut(ofertas: [Vigente(activo: false)], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.NewOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.TieneOferta);
    }

    [Fact]
    public async Task LegacyOnly_SiempreNone_LegacyEliminado()
    {
        // S57.69: LEGACY-T3 eliminado; dbo.Precio es precio normal único.
        var (sut, _, _) = Sut(ofertas: [Vigente()], referencia: Referencia());
        var r = await sut.ResolveAsync(4, 2, CommercialOfferStrategy.LegacyOnly, new(false), Hoy, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.TieneOferta);
        Assert.Equal(CommercialOfferSource.None, r.Value.Source);
    }
}

