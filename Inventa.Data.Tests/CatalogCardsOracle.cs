using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S74 §6: oráculo independiente de StorefrontRepositoryAsync.GetPublicCardsAsync.
// Implementa R1–R5 compilando las expresiones de PublicCatalogSpecs sobre listas en
// memoria, más la semántica documentada del SP (script
// D:\CODIGOS\BBDD\Inventa\Inventa_S77_001_sp_Store_CatalogoCards_RefCongelada.sql,
// líneas 10–28: desempate R5 por IdPresProd ASC, imagen con desempate IdProductoImagen ASC,
// categoría padre-raíz, LIKE sin escape, orden con NULLS LAST, TotalCount real).
// S77 R2': oferta efectiva = OfertaEfectiva (referencia == vigente y oferta < referencia);
// PrecioReferencia y % contra la referencia congelada.
//
// DESVIACIÓN documentada vs plan §6: la consulta EF copiada en
// S72_BaselineSqlTests.cs NO es traducible por EF Core 10 (CaptureBaselineSql falla
// con InvalidOperationException "could not be translated"), por lo que no puede
// ejecutarse verbatim. Este oráculo usa las mismas expresiones de PublicCatalogSpecs
// compiladas en memoria, que es el uso previsto por el propio Specs ("las pruebas
// las compilan sobre listas en memoria"). Las cargas a BBDD son consultas EF
// triviales (Where + Contains + ToList), siempre traducibles, solo lectura.
public static class CatalogCardsOracle
{
    public sealed record Params(
        int IdTenant, string? Q, long? IdCategoria, int? IdMarca,
        bool SoloOfertas, decimal? PrecioMin, decimal? PrecioMax, string? Orden);

    public static (int Total, IReadOnlyList<PublicCardItem> All) Build(InventaDbContext ctx, Params p)
    {
        var hoy = PublicCatalogSpecs.Hoy();
        var esPrecio = PublicCatalogSpecs.PrecioCandidato(hoy).Compile();
        var esOferta = PublicCatalogSpecs.OfertaVigente(hoy).Compile();

        var prods = ctx.Productos.AsNoTracking().Where(pr => pr.IdTenant == p.IdTenant).ToList();
        var prodIds = prods.Select(pr => pr.IdProd).ToList();
        var variantes = ctx.Variantes.AsNoTracking().Where(v => prodIds.Contains(v.IdProd)).ToList();
        var varIds = variantes.Select(v => v.IdVarProd).ToList();
        var pres = ctx.Presentaciones.AsNoTracking().Where(pp => varIds.Contains(pp.IdVarProd)).ToList();
        var presIds = pres.Select(pp => pp.IdPresProd).ToList();
        var precios = ctx.Precios.AsNoTracking().Where(pc => presIds.Contains(pc.IdPresProd)).ToList();
        var ofertas = ctx.Ofertas.AsNoTracking().Where(o => presIds.Contains(o.IdPresProd)).ToList();
        var imagenes = ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => prodIds.Contains(pi.IdProd) && pi.Activo).ToList();
        // HALLAZGO S74 (datos, no código): 11 productos del tenant 5 referencian
        // categorías del tenant 4 (Ids 2 y 4). El SP hace INNER JOIN a Categoria
        // SIN filtro de tenant — igual que la navegación EF original
        // (pr.Categoria por FK) — y los incluye. El oráculo replica el join sin
        // filtro de tenant; no se toca el SP ni los datos (ver Pendientes en RESULT).
        var catById = ctx.Categorias.AsNoTracking().ToDictionary(c => c.IdCategoria);
        // SP: LEFT JOIN Marca sin filtro de tenant → idem.
        var marcaById = ctx.Marcas.AsNoTracking().ToDictionary(m => m.IdMarca);
        var uniById = ctx.Unidades.AsNoTracking().ToDictionary(u => u.IdUnidad);

        var varsByProd = variantes.GroupBy(v => v.IdProd)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<VarProd>)g.ToList());
        var presByVar = pres.GroupBy(pp => pp.IdVarProd)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<PresProd>)g.ToList());
        var preciosByPres = precios.GroupBy(pc => pc.IdPresProd)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Precio>)g.ToList());
        // S77: valor de la referencia congelada (IdPrecio PK global).
        var precioById = precios.ToDictionary(pc => pc.IdPrecio);
        decimal? RefDe(Oferta o) =>
            precioById.TryGetValue(o.IdPrecioReferencia, out var p) ? p.Valor : null;
        var ofertasByPres = ofertas.GroupBy(o => o.IdPresProd)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<Oferta>)g.ToList());
        var imgByProd = imagenes.GroupBy(pi => pi.IdProd)
            .ToDictionary(g => g.Key, g => (IReadOnlyList<ProductoImagen>)g.ToList());

        // EsPublicable (misma condición que PublicCatalogSpecs.EsPublicable).
        bool Publicable(Prod pr) =>
            pr.Activo && pr.VisibleStore
            && varsByProd.TryGetValue(pr.IdProd, out var vs)
            && vs.Any(v => v.Activo
                && presByVar.TryGetValue(v.IdVarProd, out var ps)
                && ps.Any(pp => pp.Activo && pp.EsVenta));

        // Filtro de categoría (réplica exacta del SP: hoja = igualdad; raíz =
        // propia + hijas activas; categoría intermedia = solo igualdad).
        bool CatOk(Prod pr)
        {
            if (p.IdCategoria == null) return true;
            if (pr.IdCategoria == p.IdCategoria.Value) return true;
            return catById.TryGetValue(pr.IdCategoria, out var c)
                && c.IdPadre == p.IdCategoria.Value
                && c.Activo
                && catById.TryGetValue(p.IdCategoria.Value, out var padre)
                && padre.IdPadre == null;
        }

        bool QOk(Prod pr) =>
            string.IsNullOrWhiteSpace(p.Q)
            || pr.Codigo.Contains(p.Q, StringComparison.OrdinalIgnoreCase)
            || pr.Nombre.Contains(p.Q, StringComparison.OrdinalIgnoreCase);

        var cards = new List<PublicCardItem>();
        foreach (var pr in prods)
        {
            if (!Publicable(pr)) continue;
            if (!CatOk(pr)) continue;
            if (p.IdMarca != null && pr.IdMarca != p.IdMarca.Value) continue;
            if (!QOk(pr)) continue;
            // INNER JOIN del SP: sin categoría o unidad la fila no existe.
            if (!catById.TryGetValue(pr.IdCategoria, out var cat)) continue;
            if (!uniById.TryGetValue(pr.IdUniBase, out var uni)) continue;
            marcaById.TryGetValue(pr.IdMarca ?? -1, out var marca);

            // Presentaciones vendibles del producto.
            var vend = new List<PresProd>();
            if (varsByProd.TryGetValue(pr.IdProd, out var vs))
                foreach (var v in vs)
                {
                    if (!v.Activo) continue;
                    if (presByVar.TryGetValue(v.IdVarProd, out var ps))
                        vend.AddRange(ps.Where(pp => pp.Activo && pp.EsVenta));
                }

            // R1 + R2' por presentación.
            var perPres = new List<(long IdPres, decimal? Vig, decimal? OfPrecio, decimal? RefValor, DateOnly? OfHasta)>();
            foreach (var pp in vend)
            {
                preciosByPres.TryGetValue(pp.IdPresProd, out var pcs);
                var vigRow = (pcs ?? []).Where(esPrecio)
                    .OrderByDescending(pc => pc.FecVig).ThenByDescending(pc => pc.IdPrecio)
                    .FirstOrDefault();
                var vig = vigRow != null ? (decimal?)vigRow.Valor : null;
                var vigId = vigRow != null ? (long?)vigRow.IdPrecio : null;
                ofertasByPres.TryGetValue(pp.IdPresProd, out var ofs);
                var of = (ofs ?? []).Where(o => esOferta(o) && PublicCatalogSpecs.OfertaEfectiva(o, vigId, RefDe(o)))
                    .OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta)
                    .Select(o => new { o.PrecioOferta, RefValor = RefDe(o), o.FechaHasta }).FirstOrDefault();
                perPres.Add((pp.IdPresProd, vig, of?.PrecioOferta, of?.RefValor, of != null ? of.FechaHasta : null));
            }

            // R5: menor Final, desempate IdPresProd ASC (regla determinista del SP).
            var cands = perPres.Where(x => x.Vig.HasValue).ToList();
            var best = cands.OrderBy(x => x.OfPrecio ?? x.Vig!.Value).ThenBy(x => x.IdPres).FirstOrDefault();
            decimal? final = best.Vig.HasValue ? (best.OfPrecio ?? best.Vig.Value) : null;
            var tieneOferta = perPres.Any(x => x.OfPrecio.HasValue);

            // Legacy H2: MIN de TODAS las filas Precio candidatas R1 de presentaciones vendibles.
            var vendIds = new HashSet<long>(vend.Select(v => v.IdPresProd));
            var legacy = precios.Where(pc => vendIds.Contains(pc.IdPresProd) && esPrecio(pc))
                .Select(pc => (decimal?)pc.Valor).OrderBy(v => v).FirstOrDefault();

            var img = imgByProd.TryGetValue(pr.IdProd, out var ims)
                ? ims.OrderByDescending(pi => pi.EsPrincipal).ThenBy(pi => pi.Orden)
                    // S106/S107: la grilla usa la variante 'listado' cuando existe (StorefrontRepositoryAsync).
                    .ThenBy(pi => pi.IdProductoImagen).Select(pi => pi.UrlMiniatura ?? pi.Url).FirstOrDefault()
                : null;

            if (p.SoloOfertas && !tieneOferta) continue;
            if (p.PrecioMin.HasValue && (!final.HasValue || final.Value < p.PrecioMin.Value)) continue;
            if (p.PrecioMax.HasValue && (!final.HasValue || final.Value > p.PrecioMax.Value)) continue;

            // Mapeo idéntico a StorefrontRepositoryAsync.MapRow (R3–R4' S77).
            if (final.HasValue && best.OfPrecio.HasValue && best.RefValor.HasValue && best.RefValor.Value <= 0)
                throw new InvalidOperationException(
                    $"Oráculo: IdProd={pr.IdProd} tiene oferta con referencia<=0 (el SP dividiría por cero).");
            cards.Add(new PublicCardItem(
                pr.IdProd, pr.Codigo, pr.Nombre, pr.IdCategoria, cat.Nombre, pr.IdMarca,
                marca?.Nombre, uni.Nombre, img, legacy, tieneOferta, final,
                best.OfPrecio.HasValue ? best.RefValor : null,
                best.RefValor.HasValue
                    ? PublicCatalogSpecs.PctDescuento(best.RefValor.Value, best.OfPrecio)
                    : null,
                best.OfPrecio.HasValue ? best.OfHasta : null));
        }

        return (cards.Count, Order(cards, p.Orden));
    }

    // Comparador que aproxima Latin1_General_CI_AI (case + accent insensitive)
    // para replicar el ORDER BY del SP. Sin esto, OrderBy Ordinal diverge en
    // nombres con tildes/ñ (1344 nombres distintos con acento en tenant 5).
    private static readonly System.Globalization.CompareInfo CiAi =
        System.Globalization.CultureInfo.GetCultureInfo("es-ES").CompareInfo;
    private static readonly IComparer<string> CiAiComparer = Comparer<string>.Create((a, b) =>
        CiAi.Compare(a ?? "", b ?? "",
            System.Globalization.CompareOptions.IgnoreCase | System.Globalization.CompareOptions.IgnoreNonSpace));

    // Réplica exacta del ORDER BY del SP (NULLS LAST + desempates).
    // Claves verificadas contra el script del SP: precio_asc/desc → Final,Nombre,IdProd;
    // descuento_desc → grupo-con-oferta, OfertaPrecio/PrecioReferencia, Nombre, IdProd;
    // nombre_asc → Nombre,IdProd; defecto → Codigo,IdProd. Strings con CiAi (≈CI_AI).
    // Supuesto documentado: la división decimal C# no replica al bit la aritmética
    // decimal de SQL en ratios ultra-cercanos — el test lo reportaría como mismatch.
    private static IReadOnlyList<PublicCardItem> Order(List<PublicCardItem> cards, string? orden)
    {
        var o = orden is "precio_asc" or "precio_desc" or "descuento_desc" or "nombre_asc"
            ? orden : "defecto";
        return o switch
        {
            "precio_asc" => cards.OrderBy(c => c.PrecioFinalDesde.HasValue ? 0 : 1)
                .ThenBy(c => c.PrecioFinalDesde)
                .ThenBy(c => c.Nombre, CiAiComparer).ThenBy(c => c.IdProd).ToList(),
            "precio_desc" => cards.OrderBy(c => c.PrecioFinalDesde.HasValue ? 0 : 1)
                .ThenByDescending(c => c.PrecioFinalDesde)
                .ThenBy(c => c.Nombre, CiAiComparer).ThenBy(c => c.IdProd).ToList(),
            // SP agrupa por OfertaPrecio IS NULL (= sin oferta efectiva = Referencia null;
            // OJO: OfertaHasta puede ser null CON oferta vigente — no sirve para agrupar).
            "descuento_desc" => cards.OrderBy(c => c.PrecioReferencia.HasValue ? 0 : 1)
                .ThenBy(c => c.PrecioReferencia.HasValue
                    ? c.PrecioFinalDesde!.Value / c.PrecioReferencia.Value : decimal.MaxValue)
                .ThenBy(c => c.Nombre, CiAiComparer).ThenBy(c => c.IdProd).ToList(),
            "nombre_asc" => cards.OrderBy(c => c.Nombre, CiAiComparer)
                .ThenBy(c => c.IdProd).ToList(),
            _ => cards.OrderBy(c => c.Codigo, CiAiComparer)
                .ThenBy(c => c.IdProd).ToList(),
        };
    }
}

