using CBP.MultiTenant.Abstractions;
using CBP.Results;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Infrastructure.Tenancy;

/// <summary>
/// Resuelve el tenant desde la BD de Inventa delegando a <see cref="ITenantRepository"/>.
///
/// El repositorio nuevo (<see cref="ITenantRepository"/>) implementa
/// <c>IRepositorySync&lt;Tenant&gt;</c>, por lo que <see cref="ITenantRepository.GetByCodigo"/>
/// es <b>síncrono</b> y retorna <c>Result&lt;Tenant?&gt;</c>.
///
/// <see cref="ITenantResolver{TTenant}.FindByCodigoAsync"/> sigue siendo async
/// porque es el contrato del framework CBP.MultiTenant (usado también en WebAPI).
/// En WinForms se cumple con <c>Task.FromResult</c> — sin bloqueo de hilo,
/// sin <c>async/await</c> innecesario.
///
/// Ciclo de vida: Scoped — vive junto con el DbContext del scope de inicialización
/// creado en <c>host.InitializeTenantAsync()</c>.
/// </summary>
public sealed class InventaTenantResolver : ITenantResolver<Tenant>
{
    private readonly ITenantRepository _repo;

    public InventaTenantResolver(ITenantRepository repo) => _repo = repo;

    /// <inheritdoc/>
    public Task<Result<Tenant?>> FindByCodigoAsync(string codigo, CancellationToken ct = default)
    {
        // El repositorio es síncrono (IRepositorySync). Task.FromResult envuelve
        // el resultado sin lanzar un thread adicional ni bloquear el hilo actual.
        // ct no se usa porque la operación síncrona no admite cancelación parcial;
        // si ya fue cancelada antes de llegar aquí, la propagación ocurre en el caller.
        ct.ThrowIfCancellationRequested();

        var result = _repo.GetByCodigo(codigo);
        return Task.FromResult(result);
    }
}