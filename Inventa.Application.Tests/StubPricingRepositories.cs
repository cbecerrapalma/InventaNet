using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Tests;

// Fakes manuales mínimos para CommercialOfferResolver (sin framework de mocks).
internal sealed class StubOfertaRepositoryAsync : IOfertaRepositoryAsync
{
    public Func<long, CancellationToken, Task<IReadOnlyList<Oferta>>>? GetByPresProd { get; set; }

    public Task<IReadOnlyList<Oferta>> GetByPresProdAsync(long idPresProd, CancellationToken ct = default) =>
        GetByPresProd is null
            ? throw new NotImplementedException(nameof(GetByPresProdAsync))
            : GetByPresProd(idPresProd, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<Oferta?> GetByIdAsync(long idOferta, CancellationToken ct = default) => NiM<Oferta?>();
    public Task<Oferta?> GetVigenteAsync(long idPresProd, DateOnly fecha, CancellationToken ct = default) => NiM<Oferta?>();
    public Task<IReadOnlyList<Oferta>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly fecha, CancellationToken ct = default) => NiM<IReadOnlyList<Oferta>>();
    public Task<bool> ExistsOverlapAsync(long idPresProd, DateOnly desde, DateOnly? hasta, CancellationToken ct = default) => NiM<bool>();
    public Task<Oferta?> SetPublicadoAsync(long idOferta, bool publicado, CancellationToken ct = default) => NiM<Oferta?>();
}

internal sealed class StubPrecioRepositoryAsync : IPrecioRepositoryAsync
{
    public Func<long, CancellationToken, Task<Precio?>>? GetById { get; set; }

    public Task<Precio?> GetByIdAsync(long idPrecio, CancellationToken ct = default) =>
        GetById is null
            ? throw new NotImplementedException(nameof(GetByIdAsync))
            : GetById(idPrecio, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<Precio?> GetVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default) => NiM<Precio?>();
    public Task<IReadOnlyList<Precio>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default) => NiM<IReadOnlyList<Precio>>();
    public Task<IReadOnlyList<Precio>> GetHistorialAsync(long idPresProd, CancellationToken ct = default) => NiM<IReadOnlyList<Precio>>();
    public Task<bool> ExisteVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default) => NiM<bool>();
    public Task<decimal?> GetValorVigenteAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default) => NiM<decimal?>();
    public Task<IReadOnlyList<Precio>> GetByPresProdOrRecentAsync(long? idPresProd, int take, CancellationToken ct = default) => NiM<IReadOnlyList<Precio>>();
    public Task<(IReadOnlyList<Precio> Items, int Total)> SearchPagedByTenantAsync(int idTenant, long? idPresProd, string? like, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<Precio> Items, int Total)>();
    public Task<IReadOnlyList<Precio>> GetVigentesAsync(IEnumerable<long> idsPresProd, DateOnly today, CancellationToken ct = default) => NiM<IReadOnlyList<Precio>>();
    public Task<IReadOnlyList<long>> GetValidPriceProdIdsAsync(int idTenant, DateOnly today, CancellationToken ct = default) => NiM<IReadOnlyList<long>>();
    public Task<Precio> InsertAsync(Precio entity, CancellationToken ct = default) => NiM<Precio>();
    public Task<Precio> UpdateAsync(Precio entity, CancellationToken ct = default) => NiM<Precio>();
    public Task<bool> DeleteAsync(long idPrecio, CancellationToken ct = default) => NiM<bool>();
}

internal sealed class StubPresProdRepositoryAsync : IPresProdRepositoryAsync
{
    public Func<long, CancellationToken, Task<int?>>? GetTenantId { get; set; }

    public Task<int?> GetTenantIdAsync(long idPresProd, CancellationToken ct = default) =>
        GetTenantId is null
            ? throw new NotImplementedException(nameof(GetTenantIdAsync))
            : GetTenantId(idPresProd, ct);

    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") =>
        throw new NotImplementedException(m);

    public Task<IReadOnlyList<PresProd>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<IReadOnlyList<PresProd>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<PresProd?> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default) => NiM<PresProd?>();
    public Task<bool> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default) => NiM<bool>();
    public Task<IReadOnlyList<PresProd>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<IReadOnlyList<PresProd>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<IReadOnlyDictionary<long, int>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default) => NiM<IReadOnlyDictionary<long, int>>();
    public Task<PresProd?> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default) => NiM<PresProd?>();
    public Task<PresProd?> GetByIdAsync(long idPresProd, CancellationToken ct = default) => NiM<PresProd?>();
    public Task<IReadOnlyList<PresProd>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<IReadOnlyList<PresProd>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default) => NiM<IReadOnlyList<PresProd>>();
    public Task<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>();
    public Task<PresProd> CreateAsync(PresProd entity, CancellationToken ct = default) => NiM<PresProd>();
}

