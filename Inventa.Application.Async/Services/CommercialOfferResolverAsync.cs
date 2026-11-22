using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services;

// S57.56: resolver único de representación comercial LEGACY-T3 vs NEW-Oferta.
// Solo lectura. Ningún path de producción lo consume mientras M6/M7 estén PENDING
// (equivalencia demostrada vía endpoint diagnóstico + E2E s57.56). Sin Pricing Engine,
// sin stacking, sin Promoción. Activación solo por contrato de cutover aprobado,
// jamás por variable de entorno (§26 S57.56).
public enum CommercialOfferStrategy
{
    LegacyOnly = 0,
    NewOnly = 1,
    NewPreferred = 2,
    DualDiagnostic = 3
}

public enum CommercialOfferSource
{
    None = 0,
    Legacy = 1,
    New = 2
}

// Política M6 explícita por llamada (no global, no ambiente):
// HonorPublicado=false = comportamiento público actual (ignora Publicado);
// HonorPublicado=true = semántica M6-B preparada (false => no elegible).
public sealed record CommercialOfferPolicy(bool HonorPublicado);

public sealed record LegacyOfferSignal(long IdPrecio, decimal Valor, DateOnly FecVig, bool Activo);

public sealed record NewOfferSignal(
    long IdOferta,
    long IdPrecioReferencia,
    decimal? ReferenciaValor,
    bool RefEsTipo1,
    bool RefMismaPres,
    decimal PrecioOferta,
    DateOnly FechaDesde,
    DateOnly? FechaHasta,
    bool Activo,
    bool Publicado,
    bool Elegible);

public sealed record CommercialOfferResolution(
    long IdPresProd,
    bool TieneOferta,
    CommercialOfferSource Source,
    decimal? Valor,
    bool EsDoble,
    string Strategy,
    LegacyOfferSignal? Legacy,
    NewOfferSignal? New);

public interface ICommercialOfferResolverAsync
{
    Task<Result<CommercialOfferResolution>> ResolveAsync(
        int idTenant,
        long idPresProd,
        CommercialOfferStrategy strategy,
        CommercialOfferPolicy policy,
        DateOnly? fecha = null,
        CancellationToken ct = default);
}

public sealed class CommercialOfferResolverAsync : ICommercialOfferResolverAsync
{
    private readonly IOfertaRepositoryAsync _ofertas;
    private readonly IPrecioRepositoryAsync _precios;
    private readonly IPresProdRepositoryAsync _presProd;

    public CommercialOfferResolverAsync(
        IOfertaRepositoryAsync ofertas,
        IPrecioRepositoryAsync precios,
        IPresProdRepositoryAsync presProd)
    {
        _ofertas = ofertas;
        _precios = precios;
        _presProd = presProd;
    }

    public async Task<Result<CommercialOfferResolution>> ResolveAsync(
        int idTenant,
        long idPresProd,
        CommercialOfferStrategy strategy,
        CommercialOfferPolicy policy,
        DateOnly? fecha = null,
        CancellationToken ct = default)
    {
        if (idTenant <= 0)
            return Result<CommercialOfferResolution>.Failure(new Error("VALIDATION", "IdTenant debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (idPresProd <= 0)
            return Result<CommercialOfferResolution>.Failure(new Error("VALIDATION", "IdPresProd debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));

        // Tenant isolation: misma convención que OfertaServiceAsync (404 cross-tenant).
        var tenantPres = await _presProd.GetTenantIdAsync(idPresProd, ct);
        if (tenantPres == null || tenantPres.Value != idTenant)
            return Result<CommercialOfferResolution>.Failure(new Error("PRES_PROD_NOT_FOUND", "Presentación no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));

        var hoy = fecha ?? DateOnly.FromDateTime(DateTime.UtcNow);

        // S57.69: LEGACY-T3 eliminado — dbo.Precio = precio normal único y la
        // señal de oferta sale exclusivamente de la tabla Oferta (canon S57.62).
        // Se mantiene el miembro `legacy: null` para no cambiar el contrato del
        // resolution/strategy-switch (LegacyOnly ⇒ (false, None)).
        LegacyOfferSignal? legacy = null;

        // NEW (§7): vigente temporal + activa; publicada según política M6 explícita.
        // Sin recalcular referencia: solo se reporta (Id, valor, existe, misma pres).
        var disponibles = await _ofertas.GetByPresProdAsync(idPresProd, ct);
        var vigente = disponibles
            .Where(o => o.Activo && o.FechaDesde <= hoy && (o.FechaHasta == null || o.FechaHasta >= hoy))
            .OrderByDescending(o => o.FechaDesde).ThenByDescending(o => o.IdOferta)
            .FirstOrDefault();
        NewOfferSignal? nuevo = null;
        if (vigente != null)
        {
            var referencia = await _precios.GetByIdAsync(vigente.IdPrecioReferencia, ct);
            var elegible = !policy.HonorPublicado || vigente.Publicado;
            nuevo = new NewOfferSignal(
                vigente.IdOferta, vigente.IdPrecioReferencia,
                referencia?.Valor,
                referencia != null,
                referencia != null && referencia.IdPresProd == idPresProd,
                vigente.PrecioOferta, vigente.FechaDesde, vigente.FechaHasta,
                vigente.Activo, vigente.Publicado, elegible);
        }

        // M7 (§8): unificación. DUAL solo diagnóstico (ambas señales reportadas).
        var elegibleNew = nuevo is { Elegible: true };
        var (tiene, source) = strategy switch
        {
            CommercialOfferStrategy.LegacyOnly => (legacy != null, legacy != null ? CommercialOfferSource.Legacy : CommercialOfferSource.None),
            CommercialOfferStrategy.NewOnly => (elegibleNew, elegibleNew ? CommercialOfferSource.New : CommercialOfferSource.None),
            CommercialOfferStrategy.NewPreferred => elegibleNew
                ? (true, CommercialOfferSource.New)
                : (legacy != null, legacy != null ? CommercialOfferSource.Legacy : CommercialOfferSource.None),
            _ => ((legacy != null || elegibleNew),
                elegibleNew ? CommercialOfferSource.New
                    : legacy != null ? CommercialOfferSource.Legacy : CommercialOfferSource.None)
        };
        var esDoble = legacy != null && elegibleNew;
        decimal? valor = source switch
        {
            CommercialOfferSource.New => nuevo!.PrecioOferta,
            CommercialOfferSource.Legacy => legacy!.Valor,
            _ => null
        };

        return Result<CommercialOfferResolution>.Success(new CommercialOfferResolution(
            idPresProd, tiene, source, valor, esDoble, strategy.ToString(), legacy, nuevo));
    }
}
