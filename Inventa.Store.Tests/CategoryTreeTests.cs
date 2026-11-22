using Inventa.Store.Models;
using Xunit;

namespace Inventa.Store.Tests;

/// <summary>S62.6 modelo de árbol con API plana (S62.1-SHELL §3).</summary>
public sealed class CategoryTreeTests
{
    [Fact]
    public void Build_Null_ListaVacia() => Assert.Empty(CategoryTree.Build(null));

    [Fact]
    public void Build_Vacio_ListaVacia() =>
        Assert.Empty(CategoryTree.Build([]));

    // S69: con igual Orden rige Nombre (ya no se preserva el orden del servidor).
    [Fact]
    public void Build_MismoOrden_OrdenaPorNombre()
    {
        var src = new[]
        {
            new PublicCategoryDto(3, "C3", "Tercera"),
            new PublicCategoryDto(1, "C1", "Primera"),
            new PublicCategoryDto(2, "C2", "Segunda"),
        };
        var nodos = CategoryTree.Build(src);
        Assert.Equal([1, 2, 3], nodos.Select(n => n.Id));
        Assert.All(nodos, n => Assert.Empty(n.Hijos));
    }

    // S69: Cantidad es passthrough del servidor (default 0, no null).
    [Fact]
    public void Build_Cantidad_Passthrough() =>
        Assert.All(CategoryTree.Build([new PublicCategoryDto(1, "C1", "Una", 5)]),
            n => Assert.Equal(5, n.Cantidad));

    [Fact]
    public void Build_Nombres_Intactos() =>
        Assert.Equal("Lácteos y Más",
            CategoryTree.Build([new PublicCategoryDto(7, "C7", "Lácteos y Más")])[0].Nombre);

    // S69: jerarquía de 2 niveles por IdPadre.
    [Fact]
    public void Build_Jerarquia_DosNiveles()
    {
        var src = new[]
        {
            new PublicCategoryDto(1, "R1", "Raíz", 2),
            new PublicCategoryDto(2, "H1", "Hija A", 1, 1, 0),
            new PublicCategoryDto(3, "H2", "Hija B", 1, 1, 1),
        };
        var nodos = CategoryTree.Build(src);
        var raiz = Assert.Single(nodos);
        Assert.Equal(1, raiz.Id);
        Assert.Equal([2, 3], raiz.Hijos.Select(h => h.Id));
        Assert.All(raiz.Hijos, h => Assert.Empty(h.Hijos));
    }

    // S69: huérfana (padre ausente del payload) => raíz, nunca se pierde.
    [Fact]
    public void Build_Huerfanas_SonRaiz()
    {
        var src = new[]
        {
            new PublicCategoryDto(9, "H", "Huérfana", 1, 99, 0),
        };
        var nodos = CategoryTree.Build(src);
        var unica = Assert.Single(nodos);
        Assert.Equal(9, unica.Id);
        Assert.Empty(unica.Hijos);
    }

    // S69: raíces e hijas ordenan por Orden asc, luego Nombre.
    [Fact]
    public void Build_Ordena_PorOrdenLuegoNombre()
    {
        var src = new[]
        {
            new PublicCategoryDto(1, "B", "Beta", 0, null, 1),
            new PublicCategoryDto(2, "A", "Alfa", 0, null, 1),
            new PublicCategoryDto(3, "C", "Primera", 0, null, 0),
        };
        var nodos = CategoryTree.Build(src);
        Assert.Equal([3, 2, 1], nodos.Select(n => n.Id));
    }

    // S69: payload de API sin S69 (sin IdPadre) => todo raíz (compat).
    [Fact]
    public void Build_PayloadSinJerarquia_TodoRaiz()
    {
        var src = new[]
        {
            new PublicCategoryDto(1, "C1", "Una", 2),
            new PublicCategoryDto(2, "C2", "Dos", 3),
        };
        var nodos = CategoryTree.Build(src);
        Assert.Equal(2, nodos.Count);
        Assert.All(nodos, n => Assert.Empty(n.Hijos));
    }
}

