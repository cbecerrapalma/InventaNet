using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

// S57.52 (S57.51-07): quinta pieza — carga y valida referencias (misma
// presentación/tenant) sin Pricing Engine (prohibido §53). Solo lectura.
public sealed record OfertaResolution(
    long IdPresProd,
    long? NormalVigenteId,
    decimal? NormalVigenteValor,
    Oferta? OfertaVigente,
    decimal? PrecioReferenciaValor,
    bool ReferenciaEsVigente,
    decimal? Efectivo,
    int? DescuentoPct);

public interface IPrecioReferenceResolverAsync
{
    Task<Result<OfertaResolution?>> ResolveAsync(int idTenant, long idPresProd, DateOnly? fecha, CancellationToken ct = default);
    Task<Result<IReadOnlyDictionary<long, OfertaResolution>>> ResolveManyAsync(int idTenant, IEnumerable<long> idsPresProd, DateOnly? fecha, CancellationToken ct = default);
}

public sealed class PrecioReferenceResolverAsync : IPrecioReferenceResolverAsync
{
    private readonly IOfertaRepositoryAsync _ofertas;
    private readonly IPrecioRepositoryAsync _precios;
    private readonly IPresProdRepositoryAsync _presProd;

    public PrecioReferenceResolverAsync(
        IOfertaRepositoryAsync ofertas,
        IPrecioRepositoryAsync precios,
        IPresProdRepositoryAsync presProd)
    {
        _ofertas = ofertas;
        _precios = precios;
        _presProd = presProd;
    }

    public async Task<Result<OfertaResolution?>> ResolveAsync(int idTenant, long idPresProd, DateOnly? fecha, CancellationToken ct = default)
    {
        var map = await ResolveManyAsync(idTenant, [idPresProd], fecha, ct);
        if (!map.IsSuccess) return Result<OfertaResolution?>.Failure(map.Error!);
        map.Value.TryGetValue(idPresProd, out var r);
        return Result<OfertaResolution?>.Success(r, allowNull: true);
    }

    public async Task<Result<IReadOnlyDictionary<long, OfertaResolution>>> ResolveManyAsync(int idTenant, IEnumerable<long> idsPresProd, DateOnly? fecha, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        var ids = idsPresProd.Distinct().ToList();
        var result = new Dictionary<long, OfertaResolution>();
        if (ids.Count == 0) return Result<IReadOnlyDictionary<long, OfertaResolution>>.Success(result);

        var fechaRef = fecha ?? PublicCatalogSpecs.Hoy();
        var tenantMap = await _presProd.GetTenantMapAsync(ids, ct);
        var ownIds = ids.Where(id => tenantMap.TryGetValue(id, out var t) && t == idTenant).ToList();
        if (ownIds.Count == 0) return Result<IReadOnlyDictionary<long, OfertaResolution>>.Success(result);

        // S93-K1: una sola regla (R1–R4 + R2'). Solo ofertas R2-vigentes
        // (incluye Publicado); desempate R2 (menor precio, mayor IdOferta);
        // efectiva solo si R2' (referencia == vigente y mejora el precio).
        var esVigente = PublicCatalogSpecs.OfertaVigenteEnMemoria(fechaRef);
        var ofertas = (await _ofertas.GetVigentesAsync(ownIds, fechaRef, ct)).Where(esVigente).ToList();
        var normales = await _precios.GetVigentesAsync(ownIds, fechaRef, ct);
        // S57.69: Precio = precio normal único (sin discriminator).
        var normalByPres = normales
            .GroupBy(p => p.IdPresProd)
            .ToDictionary(g => g.Key, g => PublicCatalogSpecs.PrecioVigente(g, fechaRef)!);
        // S93.1: misma selección que la tienda (primero efectivas R2', luego desempate);
        // si ninguna es efectiva se informa la ganadora R2 para diagnóstico.
        var ofertaByPres = ofertas.GroupBy(o => o.IdPresProd)
            .ToDictionary(g => g.Key, g =>
            {
                normalByPres.TryGetValue(g.Key, out var vig);
                return PublicCatalogSpecs.SeleccionarOfertaEfectiva(g, vig, fechaRef)
                    ?? g.OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta).First();
            });
        var precioById = normales.ToDictionary(p => p.IdPrecio);

        foreach (var id in ownIds)
        {
            normalByPres.TryGetValue(id, out var normal);
            ofertaByPres.TryGetValue(id, out var oferta);
            decimal? refValor = null;
            var refEsVigente = false;
            if (oferta != null)
            {
                if (!precioById.TryGetValue(oferta.IdPrecioReferencia, out var refPrecio))
                    refPrecio = await _precios.GetByIdAsync(oferta.IdPrecioReferencia, ct);
                refValor = refPrecio?.Valor;
                refEsVigente = normal != null && normal.IdPrecio == oferta.IdPrecioReferencia;
            }
            // S93-K1: Efectivo y % idénticos a la tienda (R2' + R3 + R4).
            var efectiva = oferta != null
                && PublicCatalogSpecs.OfertaEfectiva(oferta, normal?.IdPrecio, normal?.Valor);
            var efectivo = PublicCatalogSpecs.PrecioFinal(
                normal?.Valor ?? 0, efectiva ? oferta!.PrecioOferta : null);
            decimal? efectivoOut = normal == null ? null : efectivo;
            var dto = normal == null ? null
                : PublicCatalogSpecs.PctDescuento(normal.Valor, efectiva ? oferta!.PrecioOferta : null);
            result[id] = new OfertaResolution(id, normal?.IdPrecio, normal?.Valor, oferta, refValor, refEsVigente, efectivoOut, dto);
        }
        return Result<IReadOnlyDictionary<long, OfertaResolution>>.Success(result);
    }
}
