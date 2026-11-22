using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IProductoServiceAsync
{
    Task<Result<IReadOnlyList<Prod>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Prod?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<Prod?>> GetWithVariantesAsync(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetPesablesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Prod?>> GetWithVariantesYStockAsync(long idProd, int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default);
    Task<Result<Prod?>> GetByIdAsync(long idProd, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Prod> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Prod?>> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoExcludingAsync(int idTenant, string codigo, long excludeIdProd, CancellationToken ct = default);
    Task<Result<Prod?>> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProdBaseRow>>> GetBaseRowsAsync(int idTenant, CancellationToken ct = default);
    Task<Result<ProdPreviewHeaderData?>> GetPreviewHeaderAsync(long idProd, int idTenant, CancellationToken ct = default);
}

public sealed class ProductoServiceAsync : IProductoServiceAsync
{
    private readonly IProdRepositoryAsync _repo;
    private readonly IProductoImagenStorageServiceAsync _imagenes;
    public ProductoServiceAsync(IProdRepositoryAsync repo, IProductoImagenStorageServiceAsync imagenes) => (_repo, _imagenes) = (repo, imagenes);

    public async Task<Result<IReadOnlyList<Prod>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByCategoriaAsync(idTenant, idCategoria, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByMarcaAsync(idTenant, idMarca, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<Prod?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<Prod?>> GetWithVariantesAsync(long idProd, int idTenant, VarianteLoadOption loadOptions = VarianteLoadOption.Both, CancellationToken ct = default)
    {
        var e = await _repo.GetWithVariantesAsync(idProd, idTenant, loadOptions, ct);
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<Prod>>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.SearchByNombreAsync(idTenant, nombre, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetQueManejanLoteAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetPesablesAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetPesablesAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, soloActivos, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<Prod>>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.SearchByCodigoOrNombreAsync(idTenant, texto, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<Prod?>> GetWithVariantesYStockAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetWithVariantesYStockAsync(idProd, idTenant, ct);
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetConControlVencimientoAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<Prod?>> GetByIdAsync(long idProd, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idProd, ct);
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Prod> Items, int Total)>> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, like, page, pageSize, ct);
        return Result<(IReadOnlyList<Prod> Items, int Total)>.Success(result);
    }

    public async Task<Result<Prod?>> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idProd, idTenant, ct);
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<bool>> ExistsByCodigoExcludingAsync(int idTenant, string codigo, long excludeIdProd, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoExcludingAsync(idTenant, codigo, excludeIdProd, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<Prod?>> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default)
    {
        var e = await _repo.SetVisibleStoreAsync(idProd, idTenant, visible, ct);
        if (e is not null)
        {
            // S57.83-G3: reubicar imágenes al namespace acorde (idempotente; re-PATCH repara).
            // Flag primero (fuente de verdad), archivos después; fallo => 500 explícito.
            // S104: el storage informa sus fallos como Result; el catch cubre un fallo de disco fuera del proveedor.
            var pendiente = new Error("IMAGEN_RELOCATE_ERROR", "Visibilidad aplicada; reubicación de imágenes pendiente").WithType(ErrorType.InternalError).WithHttpStatus(500);
            try
            {
                var reubicadas = await _imagenes.RelocateAsync(idProd, idTenant, visible && e.Activo, ct);
                if (reubicadas.IsFailure) return Result<Prod?>.Failure(pendiente);
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
            {
                return Result<Prod?>.Failure(pendiente);
            }
        }
        return Result<Prod?>.Success(e, allowNull: true);
    }

    public async Task<Result<IReadOnlyList<ProdBaseRow>>> GetBaseRowsAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetBaseRowsAsync(idTenant, ct);
        return Result<IReadOnlyList<ProdBaseRow>>.Success(list);
    }

    public async Task<Result<ProdPreviewHeaderData?>> GetPreviewHeaderAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetPreviewHeaderAsync(idProd, idTenant, ct);
        return Result<ProdPreviewHeaderData?>.Success(e, allowNull: true);
    }
}
