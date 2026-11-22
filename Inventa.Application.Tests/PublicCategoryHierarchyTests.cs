using Inventa.Data.Async.Repositories;
using Xunit;

namespace Inventa.Application.Tests;

// S70 (H1): agregación jerárquica de PublicCategoryHierarchy.Build.
public sealed class PublicCategoryHierarchyTests
{
    private static CategoryRow R(long id, string nombre = "N", int propios = 1, long? padre = null, short orden = 0) =>
        new(id, $"C{id}", nombre, padre, orden, null, propios);

    [Fact]
    public void Plano_TodasRaiz_IgualConjuntoPrevio()
    {
        var rows = new[] { R(3, "Tercera", 2), R(1, "Primera", 1), R(2, "Segunda", 0) };
        var items = PublicCategoryHierarchy.Build(rows);
        // La de 0 propios sin hijas no se publica (igual que el Any() previo).
        Assert.Equal([1, 3], items.Select(i => i.IdCategoria));
        Assert.Equal(1, items[0].Cantidad);
        Assert.Equal(2, items[1].Cantidad);
        Assert.All(items, i => Assert.Null(i.IdPadre));
    }

    [Fact]
    public void PadreSinPropios_ConDosHijas_PublicaConSuma()
    {
        var rows = new[] { R(1, "Padre", 0), R(2, "Hija A", 10, 1, 2), R(3, "Hija B", 23, 1, 1) };
        var items = PublicCategoryHierarchy.Build(rows);
        Assert.Equal(3, items.Count);
        Assert.Equal((1, 33, null), (items[0].IdCategoria, items[0].Cantidad, items[0].IdPadre));
        // Hijas ordenadas por Orden: B(1) antes que A(2).
        Assert.Equal([3, 2], items.Skip(1).Select(i => i.IdCategoria));
        Assert.All(items.Skip(1), i => Assert.Equal(1, i.IdPadre));
    }

    [Fact]
    public void PadreConPropios_MasHijas_Suma()
    {
        var rows = new[] { R(1, "Padre", 5), R(2, "Hija", 7, 1) };
        var items = PublicCategoryHierarchy.Build(rows);
        Assert.Equal(12, items[0].Cantidad);
        Assert.Equal(7, items[1].Cantidad);
    }

    [Fact]
    public void HijaSinPublicables_NoAparece_PeroPadreSi()
    {
        var rows = new[] { R(1, "Padre", 4), R(2, "Vacia", 0, 1) };
        var items = PublicCategoryHierarchy.Build(rows);
        var unico = Assert.Single(items);
        Assert.Equal(1, unico.IdCategoria);
        Assert.Equal(4, unico.Cantidad);
    }

    [Fact]
    public void PadreInactivoAusente_HijaComoRaiz()
    {
        var rows = new[] { R(2, "Huerfana", 6, 99) };
        var items = PublicCategoryHierarchy.Build(rows);
        var unica = Assert.Single(items);
        Assert.Equal(2, unica.IdCategoria);
        Assert.Null(unica.IdPadre);
        Assert.Equal(6, unica.Cantidad);
    }

    [Fact]
    public void TresNivelesCorruptos_SeAplanaARaiz()
    {
        // 1 raíz, 2 hija de 1, 3 hija de 2 (corrupto): 3 => raíz efectiva.
        var rows = new[] { R(1, "Raiz", 1), R(2, "Media", 2, 1), R(3, "Nieta", 3, 2) };
        var items = PublicCategoryHierarchy.Build(rows);
        // Raíces efectivas ordenadas por Nombre: 3 ("Nieta") antes que 1 ("Raiz", con hija 2).
        Assert.Equal([3, 1, 2], items.Select(i => i.IdCategoria));
        Assert.Null(items[0].IdPadre);
        Assert.Equal(3, items[0].Cantidad);
        Assert.Equal(3, items[1].Cantidad); // 1 + 2
    }

    [Fact]
    public void Orden_RigeSobre_Nombre_LuegoId()
    {
        var rows = new[]
        {
            R(10, "Alfa", 1, null, 1),
            R(20, "Beta", 1, null, 1),
            R(30, "Cero", 1, null, 0),
        };
        var items = PublicCategoryHierarchy.Build(rows);
        Assert.Equal([30, 10, 20], items.Select(i => i.IdCategoria));
    }

    [Fact]
    public void PadreSinPropiosNiHijasPublicadas_NoAparece()
    {
        var rows = new[] { R(1, "Vacio", 0), R(2, "HijaVacia", 0, 1) };
        Assert.Empty(PublicCategoryHierarchy.Build(rows));
    }

    [Fact]
    public void Vacio_ListaVacia() =>
        Assert.Empty(PublicCategoryHierarchy.Build([]));
}

