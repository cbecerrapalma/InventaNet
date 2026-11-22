namespace Inventa.Data.Async.Repositories;

// S70 (H1): cálculo puro (sin EF) del árbol público de categorías, reglas S70 §4.2.
// CategoryRow representa TODAS las categorías activas del tenant con su conteo
// de productos publicables propios. Salida plana: cada raíz seguida de sus hijas.
public sealed record CategoryRow(
    long IdCategoria, string Codigo, string Nombre, long? IdPadre,
    short Orden, string? ImagenUrl, int PublicablesPropios);

public static class PublicCategoryHierarchy
{
    public static IReadOnlyList<PublicCategoryItem> Build(IEnumerable<CategoryRow> rows)
    {
        var list = rows.ToList();
        if (list.Count == 0) return [];
        var byId = list.ToDictionary(r => r.IdCategoria);
        // 1. Padre efectivo: solo si está en rows (activo) y es raíz.
        var efectivo = list.ToDictionary(
            r => r.IdCategoria,
            r => r.IdPadre.HasValue
                && byId.TryGetValue(r.IdPadre.Value, out var p)
                && p.IdPadre == null
                    ? r.IdPadre : null);
        var hijasPorPadre = list
            .Where(r => efectivo[r.IdCategoria].HasValue)
            .GroupBy(r => efectivo[r.IdCategoria]!.Value)
            .ToDictionary(g => g.Key, g => Ordenar(g).ToList());
        var resultado = new List<PublicCategoryItem>(list.Count);
        // 4. Raíces ordenadas; cada raíz seguida de sus hijas (mismo criterio).
        foreach (var raiz in Ordenar(list.Where(r => efectivo[r.IdCategoria] == null)))
        {
            hijasPorPadre.TryGetValue(raiz.IdCategoria, out var hijas);
            var publicadas = hijas == null
                ? []
                : hijas.Where(h => h.PublicablesPropios > 0).ToList();
            // 3. Raíz publicada si tiene propios o ≥ 1 hija publicada.
            if (raiz.PublicablesPropios == 0 && publicadas.Count == 0) continue;
            // 3. Cantidad = propios + Σ hijas publicadas.
            var cantidad = raiz.PublicablesPropios + publicadas.Sum(h => h.PublicablesPropios);
            resultado.Add(new PublicCategoryItem(
                raiz.IdCategoria, raiz.Codigo, raiz.Nombre, cantidad,
                null, raiz.Orden, raiz.ImagenUrl));
            // 2. Hija publicada si PublicablesPropios > 0; Cantidad = propios.
            foreach (var h in publicadas)
                resultado.Add(new PublicCategoryItem(
                    h.IdCategoria, h.Codigo, h.Nombre, h.PublicablesPropios,
                    raiz.IdCategoria, h.Orden, h.ImagenUrl));
        }
        return resultado;
    }

    private static IOrderedEnumerable<CategoryRow> Ordenar(IEnumerable<CategoryRow> fuente) =>
        fuente.OrderBy(r => r.Orden)
            .ThenBy(r => r.Nombre, StringComparer.OrdinalIgnoreCase)
            .ThenBy(r => r.IdCategoria);
}
