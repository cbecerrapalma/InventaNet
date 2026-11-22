using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services.Precios;

/// <summary>Resultado de simular o aplicar un lote. Si hay errores (422) o conflictos (409) no se escribió nada.</summary>
public sealed record ResultadoCambiosPreciosDto(
    bool Aplicado,
    IReadOnlyList<ErrorCambioPrecio> Errores,
    IReadOnlyList<long> Desactualizados,
    IReadOnlyList<OmitidoCambioPrecio> Omitidos,
    IReadOnlyList<CambioPresentacionResumen> Presentaciones,
    IReadOnlyList<FilaPrecioDto> Filas,
    IReadOnlyList<long> PreciosCreados)
{
    public bool HayErrores => Errores.Count > 0;
    public bool HayConflicto => Desactualizados.Count > 0;
}

public interface IGestionPreciosServiceAsync
{
    /// <summary>Calcula el plan (antes → después, avisos, omitidos, errores) sin escribir.</summary>
    Task<Result<ResultadoCambiosPreciosDto>> SimularAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default);
    /// <summary>Recalcula el plan dentro de una transacción serializable y, si es válido, lo aplica entero.</summary>
    Task<Result<ResultadoCambiosPreciosDto>> AplicarAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default);
}

/// <summary>
/// S93.1 (K3): ÚNICO escritor web de <c>Precio</c> y <c>Oferta</c>. Los endpoints antiguos (POST api/precios,
/// POST api/ofertas, terminar oferta) delegan aquí. El plan lo calcula <see cref="PlanificadorPrecios"/> (puro);
/// la escritura es una sola transacción (<see cref="IPreciosGrillaRepositoryAsync.EjecutarAsync{TResult}"/>).
/// </summary>
public sealed class GestionPreciosServiceAsync : IGestionPreciosServiceAsync
{
    private readonly IPreciosGrillaRepositoryAsync _repo;
    private readonly IPrecioGrillaServiceAsync _grilla;

    public GestionPreciosServiceAsync(IPreciosGrillaRepositoryAsync repo, IPrecioGrillaServiceAsync grilla)
    {
        _repo = repo;
        _grilla = grilla;
    }

    public async Task<Result<ResultadoCambiosPreciosDto>> SimularAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default)
    {
        if (Validar(idTenant, cambios) is { } err) return Result<ResultadoCambiosPreciosDto>.Failure(err);
        var (_, dec) = await _grilla.GetMonedaAsync(idTenant, ct);
        var ids = cambios.Select(c => c.IdPresProd).Distinct().ToList();
        var snaps = await _repo.GetSnapshotsAsync(ids, ct);
        var plan = PlanificadorPrecios.Planificar(cambios, snaps, idTenant, PublicCatalogSpecs.Hoy(), dec);
        return Result<ResultadoCambiosPreciosDto>.Success(ADto(plan, aplicado: false, [], []));
    }

    public async Task<Result<ResultadoCambiosPreciosDto>> AplicarAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default)
    {
        if (Validar(idTenant, cambios) is { } err) return Result<ResultadoCambiosPreciosDto>.Failure(err);
        var (_, dec) = await _grilla.GetMonedaAsync(idTenant, ct);
        var ids = cambios.Select(c => c.IdPresProd).Distinct().ToList();
        var hoy = PublicCatalogSpecs.Hoy();
        var (plan, nuevos) = await _repo.EjecutarAsync(ids, snaps =>
        {
            var p = PlanificadorPrecios.Planificar(cambios, snaps, idTenant, hoy, dec);
            return (p.Valido ? p.Operaciones : null, p);
        }, ct);
        var aplicado = plan.Valido && plan.Operaciones.Count > 0;
        var filas = aplicado ? await _grilla.GetFilasAsync(idTenant, plan.Presentaciones.Select(r => r.IdPresProd).ToList(), ct) : [];
        return Result<ResultadoCambiosPreciosDto>.Success(ADto(plan, aplicado, filas, nuevos.Values.ToList()));
    }

    private static Error? Validar(int idTenant, IReadOnlyList<CambioPrecio>? cambios)
    {
        if (idTenant <= 0)
            return new Error("VALIDATION", "IdTenant debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400);
        if (cambios is null || cambios.Count == 0)
            return new Error("VALIDATION", "El lote no contiene cambios.").WithType(ErrorType.Validation).WithHttpStatus(400);
        if (cambios.Count > CodigosPrecio.MaxCambios)
            return new Error(CodigosPrecio.LoteDemasiadoGrande, $"Un lote admite hasta {CodigosPrecio.MaxCambios} cambios.")
                .WithType(ErrorType.Validation).WithHttpStatus(422);
        return null;
    }

    private static ResultadoCambiosPreciosDto ADto(PlanCambiosPrecios p, bool aplicado, IReadOnlyList<FilaPrecioDto> filas, IReadOnlyList<long> creados) =>
        new(aplicado, p.Errores, p.Desactualizados, p.Omitidos, p.Presentaciones, filas, creados);
}
