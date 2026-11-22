using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using Inventa.Data.Context;

namespace CBP.Data.Abstractions;

/// <summary>
/// Shim to restore legacy Inventa calls to GetRepository / GetCustomRepository
/// after CBP S25 made IUnitOfWorkSync agnostic (no generic, no repository methods on interface).
/// The underlying implementation is still UnitOfWorkSync{InventaDbContext} which retains those members.
/// </summary>
public static class UnitOfWorkSyncExtensions
{
    public static IRepositorySync<TEntity> GetRepository<TEntity>(this IUnitOfWorkSync uow)
        where TEntity : class
    {
        if (uow is UnitOfWorkSync<InventaDbContext> impl)
            return impl.GetRepository<TEntity>();
        throw new InvalidOperationException(
            $"IUnitOfWorkSync is not UnitOfWorkSync<InventaDbContext> (actual: {uow.GetType().Name}). " +
            "Ensure AddUnitOfWorkSync<InventaDbContext>() was called.");
    }

    public static TRepository GetCustomRepository<TRepository>(this IUnitOfWorkSync uow)
        where TRepository : class
    {
        if (uow is UnitOfWorkSync<InventaDbContext> impl)
            return impl.GetCustomRepository<TRepository>();
        throw new InvalidOperationException(
            $"IUnitOfWorkSync is not UnitOfWorkSync<InventaDbContext> (actual: {uow.GetType().Name}).");
    }
}
