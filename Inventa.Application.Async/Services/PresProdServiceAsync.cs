using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IPresProdServiceAsync
{
    Task<Result<IReadOnlyList<PresProd>>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PresProd>>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default);
    Task<Result<PresProd?>> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default);
    Task<Result<bool>> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PresProd>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PresProd>>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default);
    Task<Result<int?>> GetTenantIdAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyDictionary<long, int>>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default);
    Task<Result<PresProd?>> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<PresProd?>> GetByIdAsync(long idPresProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PresProd>>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<PresProd>>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default);
}

public sealed class PresProdServiceAsync : IPresProdServiceAsync
{
    private readonly IPresProdRepositoryAsync _repo;
    private readonly IPrecioReferenceResolverAsync _precioResolver;
    public PresProdServiceAsync(IPresProdRepositoryAsync repo, IPrecioReferenceResolverAsync precioResolver)
    {
        _repo = repo;
        _precioResolver = precioResolver;
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetByVarianteAsync(long idVarProd, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByVarianteAsync(idVarProd, soloActivas, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetByUsoPresentacionAsync(long idVarProd, bool paraVenta, CancellationToken ct = default)
    {
        var list = await _repo.GetByUsoPresentacionAsync(idVarProd, paraVenta, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<PresProd?>> GetWithPreciosVigentesAsync(long idPresProd, DateOnly? fecha = null, CancellationToken ct = default)
    {
        var e = await _repo.GetWithPreciosVigentesAsync(idPresProd, fecha, ct);
        return Result<PresProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<bool>> ExistsByVarianteUnidadAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByVarianteUnidadAsync(idVarProd, idUnidad, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivas, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetByProductoAsync(long idProd, bool soloActivas = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByProductoAsync(idProd, soloActivas, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<int?>> GetTenantIdAsync(long idPresProd, CancellationToken ct = default)
    {
        var tenantId = await _repo.GetTenantIdAsync(idPresProd, ct);
        return Result<int?>.Success(tenantId);
    }

    public async Task<Result<IReadOnlyDictionary<long, int>>> GetTenantMapAsync(IEnumerable<long> idsPresProd, CancellationToken ct = default)
    {
        var map = await _repo.GetTenantMapAsync(idsPresProd, ct);
        return Result<IReadOnlyDictionary<long, int>>.Success(map);
    }

    public async Task<Result<PresProd?>> GetWithHierarchyAsync(long idPresProd, CancellationToken ct = default)
    {
        var e = await _repo.GetWithHierarchyAsync(idPresProd, ct);
        return Result<PresProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<PresProd?>> GetByIdAsync(long idPresProd, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idPresProd, ct);
        return Result<PresProd?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetAllByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetAllByVarianteAsync(idVarProd, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<PresProd>>> GetActiveVentaByVariantesAsync(IEnumerable<long> idsVarProd, CancellationToken ct = default)
    {
        var list = await _repo.GetActiveVentaByVariantesAsync(idsVarProd, ct);
        return Result<IReadOnlyList<PresProd>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>> SearchPagedByTenantAsync(int idTenant, string? texto, bool paraVenta, int? idBodega, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedByTenantAsync(idTenant, texto, paraVenta, idBodega, page, pageSize, ct);
        if (paraVenta && result.Items.Count > 0)
        {
            // S126: en venta, el precio sugerido es el efectivo (R3), el mismo que registra la venta.
            var map = await _precioResolver.ResolveManyAsync(idTenant, result.Items.Select(r => r.IdPresProd), null, ct);
            if (!map.IsSuccess) return Result<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>.Failure(map.Error!);
            IReadOnlyList<PresentacionBusquedaRow> filas = result.Items
                .Select(r => r with { PrecioVigente = map.Value.TryGetValue(r.IdPresProd, out var res) ? res.Efectivo : null })
                .ToList();
            return Result<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>.Success((filas, result.Total));
        }
        return Result<(IReadOnlyList<PresentacionBusquedaRow> Items, int Total)>.Success(result);
    }

}
