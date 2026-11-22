namespace Inventa.Store.Models;

// S62.2 nodo de árbol para el menú de categorías.
// S69: arma 2 niveles desde IdPadre. Huérfanas (padre ausente) => raíz.
// Orden: Orden asc, luego Nombre. Cantidad: la del servidor (puede ser 0 si
// el API no la trae).
public sealed record CategoryNode(long Id, string Nombre, int? Cantidad, IReadOnlyList<CategoryNode> Hijos);

public static class CategoryTree
{
    public static IReadOnlyList<CategoryNode> Build(IEnumerable<PublicCategoryDto>? source)
    {
        if (source is null) return [];
        var list = source.ToList();
        if (list.Count == 0) return [];
        var byId = list.ToDictionary(c => c.IdCategoria);
        var childrenOf = new Dictionary<long, List<PublicCategoryDto>>();
        var roots = new List<PublicCategoryDto>();
        foreach (var c in list)
        {
            // Huérfana (padre ausente del payload) => raíz, nunca se pierde.
            if (c.IdPadre.HasValue && byId.ContainsKey(c.IdPadre.Value))
            {
                if (!childrenOf.TryGetValue(c.IdPadre.Value, out var kids))
                    childrenOf[c.IdPadre.Value] = kids = [];
                kids.Add(c);
            }
            else
            {
                roots.Add(c);
            }
        }
        // El API garantiza máx. 2 niveles; ante un payload más profundo se
        // anida tal cual (sin perder nodos), sin lógica especulativa.
        static IReadOnlyList<CategoryNode> KidsOf(long id, Dictionary<long, List<PublicCategoryDto>> childrenOf)
        {
            if (!childrenOf.TryGetValue(id, out var kids)) return [];
            return kids.OrderBy(k => k.Orden).ThenBy(k => k.Nombre, StringComparer.OrdinalIgnoreCase)
                .Select(k => new CategoryNode(k.IdCategoria, k.Nombre, k.Cantidad, KidsOf(k.IdCategoria, childrenOf)))
                .ToList();
        }
        return roots.OrderBy(r => r.Orden).ThenBy(r => r.Nombre, StringComparer.OrdinalIgnoreCase)
            .Select(r => new CategoryNode(r.IdCategoria, r.Nombre, r.Cantidad, KidsOf(r.IdCategoria, childrenOf)))
            .ToList();
    }

    // S71 helpers de lectura (sin cambiar Build): raíz que contiene un id
    // (la propia raíz o el padre de una hija) y búsqueda por id.
    public static CategoryNode? RaizDe(IReadOnlyList<CategoryNode>? raices, long? id)
    {
        if (raices is null || id is null) return null;
        foreach (var r in raices)
        {
            if (r.Id == id.Value) return r;
            foreach (var h in r.Hijos)
                if (h.Id == id.Value) return r;
        }
        return null;
    }

    public static CategoryNode? Buscar(IReadOnlyList<CategoryNode>? raices, long? id)
    {
        if (raices is null || id is null) return null;
        foreach (var r in raices)
        {
            if (r.Id == id.Value) return r;
            foreach (var h in r.Hijos)
                if (h.Id == id.Value) return h;
        }
        return null;
    }
}
