using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Tests;

// Fake manual mínimo (sin framework de mocks): solo los miembros usados por los
// tests tienen comportamiento; el resto falla alto si se invoca por error.
internal sealed class StubProdRepositoryAsync : IProdRepositoryAsync
{
    public Func<long, int, CancellationToken, Task<Prod?>>? GetByIdInTenant { get; set; }
    public Func<long, int, bool, CancellationToken, Task<Prod?>>? SetVisibleStore { get; set; }

    public Task<Prod?> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default) =>
        GetByIdInTenant is null
            ? throw new NotImplementedException(nameof(GetByIdInTenantAsync))
            : GetByIdInTenant(idProd, idTenant, ct);

    public Task<Prod?> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default) =>
        SetVisibleStore is null
            ? throw new NotImplementedException(nameof(SetVisibleStoreAsync))
            : SetVisibleStore(idProd, idTenant, visible, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<IReadOnlyList<Prod>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<IReadOnlyList<Prod>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<IReadOnlyList<Prod>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<Prod?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Prod?>();
    public Task<Prod?> GetWithVariantesAsync(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both, CancellationToken ct = default) => NiM<Prod?>();
    public Task<IReadOnlyList<Prod>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<IReadOnlyList<Prod>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<IReadOnlyList<Prod>> GetPesablesAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => NiM<int>();
    public Task<IReadOnlyList<Prod>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<Prod?> GetWithVariantesYStockAsync(long idProd, int idTenant, CancellationToken ct = default) => NiM<Prod?>();
    public Task<IReadOnlyList<Prod>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<Prod?> GetByIdAsync(long idProd, CancellationToken ct = default) => NiM<Prod?>();
    public Task<IReadOnlyList<Prod>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default) => NiM<IReadOnlyList<Prod>>();
    public Task<(IReadOnlyList<Prod> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<Prod> Items, int Total)>();
    public Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, long excludeIdProd, CancellationToken ct = default) => NiM<bool>();
    public Task<(Prod? Prod, bool Duplicado)> CreateAsync(Prod entity, CancellationToken ct = default) => NiM<(Prod? Prod, bool Duplicado)>();
    public Task<(Prod? Prod, bool Duplicado)> UpdateAsync(long idProd, int idTenant, string codigo, string nombre, long idCategoria, int? idMarca, CancellationToken ct = default) => NiM<(Prod? Prod, bool Duplicado)>();
    public Task<IReadOnlyList<ProdBaseRow>> GetBaseRowsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<ProdBaseRow>>();
    public Task<ProdPreviewHeaderData?> GetPreviewHeaderAsync(long idProd, int idTenant, CancellationToken ct = default) => NiM<ProdPreviewHeaderData?>();
}

