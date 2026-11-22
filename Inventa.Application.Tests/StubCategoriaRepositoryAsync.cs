using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Tests;

// S69: fake manual mínimo para jerarquía de categorías. Solo los miembros
// usados por CatalogoServiceAsync tienen comportamiento; el resto falla alto.
internal sealed class StubCategoriaRepositoryAsync : ICategoriaRepositoryAsync
{
    public Func<long, int, CancellationToken, Task<Categoria?>>? GetByIdInTenant { get; set; }
    public Func<int, bool, CancellationToken, Task<IReadOnlyList<Categoria>>>? GetByTenant { get; set; }
    public Func<Categoria, CancellationToken, Task<Categoria>>? Create { get; set; }
    public Func<long, int, string, string, CancellationToken, Task<Categoria?>>? Update { get; set; }

    public Task<Categoria?> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default) =>
        GetByIdInTenant is null
            ? throw new NotImplementedException(nameof(GetByIdInTenantAsync))
            : GetByIdInTenant(idCategoria, idTenant, ct);

    public Task<IReadOnlyList<Categoria>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) =>
        GetByTenant is null
            ? throw new NotImplementedException(nameof(GetByTenantAsync))
            : GetByTenant(idTenant, soloActivas, ct);

    public Task<Categoria> CreateAsync(Categoria entity, CancellationToken ct = default) =>
        Create is null
            ? throw new NotImplementedException(nameof(CreateAsync))
            : Create(entity, ct);

    public Task<Categoria?> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default) =>
        Update is null
            ? throw new NotImplementedException(nameof(UpdateAsync))
            : Update(idCategoria, idTenant, codigo, nombre, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<Categoria?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<IReadOnlyList<Categoria>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default) => NiM<IReadOnlyList<Categoria>>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<int> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default) => NiM<int>();
    public Task<Categoria?> GetByIdAsync(long idCategoria, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<IReadOnlyList<long>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<long>>();
}

