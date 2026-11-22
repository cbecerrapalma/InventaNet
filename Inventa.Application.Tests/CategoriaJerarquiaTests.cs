using Inventa.Application.Async.Services;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S69: validaciones de jerarquía (2 niveles) en CatalogoServiceAsync.
public sealed class CategoriaJerarquiaTests
{
    private static Categoria Raiz(long id, int tenant = 6) =>
        new() { IdCategoria = id, IdTenant = tenant, Codigo = $"R{id}", Nombre = $"Raíz {id}" };

    private static CatalogoServiceAsync Sut(StubCategoriaRepositoryAsync repo) =>
        new(repo, null!, null!, null!, null!, null!, null!);

    [Fact]
    public async Task Create_PadreInexistente_404()
    {
        var repo = new StubCategoriaRepositoryAsync
        {
            GetByIdInTenant = (_, _, _) => Task.FromResult<Categoria?>(null),
        };

        var r = await Sut(repo).CreateCategoriaAsync(6, "H1", "Hija", idPadre: 999, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("CATEGORIA_PADRE_NO_ENCONTRADA", r.Error!.Code);
        Assert.Equal(404, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task Create_PadreNoRaiz_400()
    {
        var padre = Raiz(2); padre.IdPadre = 1; // hija (nivel 2), no raíz
        var repo = new StubCategoriaRepositoryAsync
        {
            GetByIdInTenant = (id, _, _) => Task.FromResult<Categoria?>(id == 2 ? padre : null),
        };

        var r = await Sut(repo).CreateCategoriaAsync(6, "N3", "Nivel3", idPadre: 2, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("CATEGORIA_MAX_NIVELES", r.Error!.Code);
        Assert.Equal(400, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task Update_Autopadre_400()
    {
        var actual = Raiz(5);
        var repo = new StubCategoriaRepositoryAsync
        {
            GetByIdInTenant = (id, _, _) => Task.FromResult<Categoria?>(id == 5 ? actual : null),
        };

        var r = await Sut(repo).UpdateCategoriaAsync(5, 6, "R5", "Raíz 5", idPadre: 5, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("CATEGORIA_AUTOPADRE", r.Error!.Code);
        Assert.Equal(400, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task Update_ReparentConHijas_409()
    {
        var actual = Raiz(5);
        var padre = Raiz(2);
        var hija = Raiz(7); hija.IdPadre = 5;
        var repo = new StubCategoriaRepositoryAsync
        {
            GetByIdInTenant = (id, _, _) => Task.FromResult<Categoria?>(
                id == 5 ? actual : id == 2 ? padre : null),
            GetByTenant = (_, _, _) => Task.FromResult<IReadOnlyList<Categoria>>([actual, padre, hija]),
            Update = (id, _, _, _, _) => Task.FromResult<Categoria?>(actual),
        };

        var r = await Sut(repo).UpdateCategoriaAsync(5, 6, "R5", "Raíz 5", idPadre: 2, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("CATEGORIA_TIENE_HIJAS", r.Error!.Code);
        Assert.Equal(409, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task Create_ImagenUrlInvalida_400()
    {
        var repo = new StubCategoriaRepositoryAsync();

        var r = await Sut(repo).CreateCategoriaAsync(6, "M1", "Mala", imagenUrl: "javascript:alert(1)", ct: Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("CATEGORIA_IMAGENURL_INVALIDA", r.Error!.Code);
        Assert.Equal(400, r.Error.HttpStatusCode);
    }

    [Fact]
    public async Task Create_HijaValida_PersisteJerarquia()
    {
        var padre = Raiz(2);
        Categoria? creada = null;
        var repo = new StubCategoriaRepositoryAsync
        {
            GetByIdInTenant = (id, _, _) => Task.FromResult<Categoria?>(id == 2 ? padre : null),
            Create = (e, _) => { creada = e; return Task.FromResult(e); },
        };

        var r = await Sut(repo).CreateCategoriaAsync(6, "H1", "Hija", idPadre: 2, orden: 3, imagenUrl: "/uploads/h1.png", Xunit.TestContext.Current.CancellationToken);

        Assert.True(r.IsSuccess);
        Assert.NotNull(creada);
        Assert.Equal(2, creada!.IdPadre);
        Assert.Equal(3, creada.Orden);
        Assert.Equal("/uploads/h1.png", creada.ImagenUrl);
    }

    // S70 (H6): Orden válido 0..9999.
    [Theory]
    [InlineData(-1, false)]
    [InlineData(0, true)]
    [InlineData(9999, true)]
    [InlineData(10000, false)]
    public async Task Create_Orden_Rango_0_9999(short orden, bool ok)
    {
        Categoria? creada = null;
        var repo = new StubCategoriaRepositoryAsync
        {
            Create = (e, _) => { creada = e; return Task.FromResult(e); },
        };

        var r = await Sut(repo).CreateCategoriaAsync(6, "O1", "Ord", orden: orden, ct: Xunit.TestContext.Current.CancellationToken);

        Assert.Equal(ok, r.IsSuccess);
        if (ok) Assert.Equal(orden, creada!.Orden);
        else Assert.Equal("CATEGORIA_ORDEN_INVALIDO", r.Error!.Code);
    }
}