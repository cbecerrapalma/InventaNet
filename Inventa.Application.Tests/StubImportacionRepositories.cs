using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Tests;

// S87.2 C5: fakes manuales mínimos para ProductoImportacionValidadorAsync.
// Solo GetByTenantAsync/GetAllOrdenadoAsync tienen comportamiento; el resto falla alto.
internal sealed class StubCategoriaImportRepo : ICategoriaRepositoryAsync
{
    public IReadOnlyList<Categoria> Datos { get; set; } = [];
    public Task<IReadOnlyList<Categoria>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Categoria?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<IReadOnlyList<Categoria>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default) => NiM<IReadOnlyList<Categoria>>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<int> CountProductosByCategoriaAsync(long idCategoria, CancellationToken ct = default) => NiM<int>();
    public Task<Categoria?> GetByIdInTenantAsync(long idCategoria, int idTenant, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<Categoria> CreateAsync(Categoria entity, CancellationToken ct = default) => NiM<Categoria>();
    public Task<Categoria?> UpdateAsync(long idCategoria, int idTenant, string codigo, string nombre, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<Categoria?> GetByIdAsync(long idCategoria, CancellationToken ct = default) => NiM<Categoria?>();
    public Task<IReadOnlyList<long>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<long>>();
}

internal sealed class StubMarcaImportRepo : IMarcaRepositoryAsync
{
    public IReadOnlyList<Marca> Datos { get; set; } = [];
    public Task<IReadOnlyList<Marca>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Marca?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Marca?>();
    public Task<IReadOnlyList<Marca>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default) => NiM<IReadOnlyList<Marca>>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<int> CountByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => NiM<int>();
    public Task<Marca?> GetByIdInTenantAsync(int idMarca, int idTenant, CancellationToken ct = default) => NiM<Marca?>();
    public Task<Marca> CreateAsync(Marca entity, CancellationToken ct = default) => NiM<Marca>();
    public Task<Marca?> UpdateAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default) => NiM<Marca?>();
    public Task<Marca?> GetByIdAsync(int idMarca, CancellationToken ct = default) => NiM<Marca?>();
    public Task<IReadOnlyList<int>> GetInactiveIdsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<int>>();
}

internal sealed class StubUnidadImportRepo : IUnidadRepositoryAsync
{
    public IReadOnlyList<Unidad> Datos { get; set; } = [];
    public Task<IReadOnlyList<Unidad>> GetAllOrdenadoAsync(CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Unidad?> GetByCodigoAsync(string codigo, CancellationToken ct = default) => NiM<Unidad?>();
    public Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<bool> IsUsedByProductosAsync(int idUnidad, CancellationToken ct = default) => NiM<bool>();
    public Task<Unidad?> GetByIdAsync(int idUnidad, CancellationToken ct = default) => NiM<Unidad?>();
    public Task<IReadOnlyDictionary<int, string>> GetMapAsync(CancellationToken ct = default) => NiM<IReadOnlyDictionary<int, string>>();
}

internal sealed class StubBodegaImportRepo : IBodegaRepositoryAsync
{
    public IReadOnlyList<Bodega> Datos { get; set; } = [];
    public Task<IReadOnlyList<Bodega>> GetByTenantAsync(int idTenant, bool soloActivas = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Bodega?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Bodega?>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<bool> IsActivaAsync(int idTenant, int idBodega, CancellationToken ct = default) => NiM<bool>();
    public Task<Bodega?> GetByIdAsync(int idBodega, CancellationToken ct = default) => NiM<Bodega?>();
    public Task<int> CountActivasAsync(int idTenant, CancellationToken ct = default) => NiM<int>();
    public Task<int> CountByTenantAsync(int idTenant, CancellationToken ct = default) => NiM<int>();
    public Task<IReadOnlyList<Bodega>> GetOrderedByTenantAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<Bodega>>();
    public Task<(IReadOnlyList<Bodega> Items, int Total)> SearchPagedAsync(int idTenant, string? like, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<Bodega> Items, int Total)>();
    public Task<Bodega?> GetByIdInTenantAsync(int idBodega, int idTenant, CancellationToken ct = default) => NiM<Bodega?>();
    public Task<bool> ExistsByCodigoExcludingAsync(int idTenant, string codigo, int excludeIdBodega, CancellationToken ct = default) => NiM<bool>();
    public Task<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)> CreateWithQuotaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default) => NiM<(Bodega? Bodega, bool Duplicado, int? MaxExcedido)>();
    public Task<(Bodega? Bodega, bool Duplicado)> UpdateAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default) => NiM<(Bodega? Bodega, bool Duplicado)>();
}

internal sealed class StubProveedorImportRepo : IProveedorRepositoryAsync
{
    public IReadOnlyList<Proveedor> Datos { get; set; } = [];
    public Task<IReadOnlyList<Proveedor>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Proveedor?> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<Proveedor?>();
    public Task<Proveedor?> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default) => NiM<Proveedor?>();
    public Task<IReadOnlyList<Proveedor>> SearchByRazonSocialAsync(int idTenant, string texto, CancellationToken ct = default) => NiM<IReadOnlyList<Proveedor>>();
    public Task<bool> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<bool> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default) => NiM<bool>();
    public Task<int> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => NiM<int>();
    public Task<IReadOnlyList<ProveedorConOCPendiente>> GetConOrdenesPendientesAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<ProveedorConOCPendiente>>();
    public Task<Proveedor?> GetByIdAsync(int idProveedor, CancellationToken ct = default) => NiM<Proveedor?>();
    public Task<IReadOnlyList<Proveedor>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default) => NiM<IReadOnlyList<Proveedor>>();
    public Task<(IReadOnlyList<Proveedor> Items, int Total)> SearchPagedAsync(int idTenant, string? texto, bool? activo, int page, int pageSize, CancellationToken ct = default) => NiM<(IReadOnlyList<Proveedor> Items, int Total)>();
    public Task<Proveedor?> GetByIdInTenantAsync(int idProveedor, int idTenant, CancellationToken ct = default) => NiM<Proveedor?>();
    public Task<(Proveedor? Proveedor, bool Duplicado)> CreateAsync(Proveedor entity, CancellationToken ct = default) => NiM<(Proveedor? Proveedor, bool Duplicado)>();
    public Task<(Proveedor? Proveedor, bool Duplicado)> UpdateAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default) => NiM<(Proveedor? Proveedor, bool Duplicado)>();
}

internal sealed class StubProdImportRepo : IProdRepositoryAsync
{
    public IReadOnlyList<Prod> Datos { get; set; } = [];
    public Task<IReadOnlyList<Prod>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<Prod?> GetByIdInTenantAsync(long idProd, int idTenant, CancellationToken ct = default) => NiM<Prod?>();
    public Task<Prod?> SetVisibleStoreAsync(long idProd, int idTenant, bool visible, CancellationToken ct = default) => NiM<Prod?>();
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

internal sealed class StubVarProdImportRepo : IVarProdRepositoryAsync
{
    public IReadOnlyList<VarProd> Datos { get; set; } = [];
    public Task<IReadOnlyList<VarProd>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<IReadOnlyList<VarProd>> GetByProductoAsync(long idProd, bool soloActivos = true, CancellationToken ct = default) => NiM<IReadOnlyList<VarProd>>();
    public Task<VarProd?> GetByCodigoAsync(long idProd, string codigo, CancellationToken ct = default) => NiM<VarProd?>();
    public Task<VarProd?> GetWithPresentacionesAsync(long idVarProd, CancellationToken ct = default) => NiM<VarProd?>();
    public Task<bool> ExistsByCodigoAsync(long idProd, string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<IReadOnlyList<VarProd>> GetConCostoCargadoAsync(long idProd, CancellationToken ct = default) => NiM<IReadOnlyList<VarProd>>();
    public Task<IReadOnlyList<VarProd>> GetConBajoStockAsync(int idTenant, decimal umbralStock = 0m, CancellationToken ct = default) => NiM<IReadOnlyList<VarProd>>();
    public Task<VarProd?> GetByIdAsync(long idVarProd, CancellationToken ct = default) => NiM<VarProd?>();
    public Task<(VarProd? VarProd, bool Duplicado)> CreateAsync(VarProd entity, CancellationToken ct = default) => NiM<(VarProd? VarProd, bool Duplicado)>();
    public Task<IReadOnlyList<long>> GetSaleProdIdsAsync(int idTenant, CancellationToken ct = default) => NiM<IReadOnlyList<long>>();
    public Task<IReadOnlyList<VarProd>> GetAllByProductoAsync(long idProd, CancellationToken ct = default) => NiM<IReadOnlyList<VarProd>>();
    public Task<IReadOnlyList<VarProd>> GetActiveByProdAsync(long idProd, CancellationToken ct = default) => NiM<IReadOnlyList<VarProd>>();
}

internal sealed class StubCodBarImportRepo : ICodBarRepositoryAsync
{
    public IReadOnlyList<CodBar> Datos { get; set; } = [];
    public Task<IReadOnlyList<CodBar>> GetByTenantAsync(int idTenant, CancellationToken ct = default) => Task.FromResult(Datos);
    private static Task<TResult> NiM<TResult>([System.Runtime.CompilerServices.CallerMemberName] string m = "") => throw new NotImplementedException(m);
    public Task<CodBar?> GetByCodigoAsync(string codigo, int idTenant, CancellationToken ct = default) => NiM<CodBar?>();
    public Task<CodBar?> GetByCodigoConPresentacionAsync(string codigo, int idTenant, CancellationToken ct = default) => NiM<CodBar?>();
    public Task<IReadOnlyList<CodBar>> GetByPresentacionAsync(long idPresProd, CancellationToken ct = default) => NiM<IReadOnlyList<CodBar>>();
    public Task<CodBar?> GetPrincipalByPresentacionAsync(long idPresProd, CancellationToken ct = default) => NiM<CodBar?>();
    public Task<bool> ExistsByCodigoAsync(string codigo, CancellationToken ct = default) => NiM<bool>();
    public Task<IReadOnlyList<CodBar>> GetByTipoAsync(long idPresProd, byte tipoCod, CancellationToken ct = default) => NiM<IReadOnlyList<CodBar>>();
    public Task<CodBar?> GetByCodigoGlobalAsync(string codigo, CancellationToken ct = default) => NiM<CodBar?>();
    public Task<(CodBar? CodBar, bool Duplicado)> CreateAsync(CodBar entity, CancellationToken ct = default) => NiM<(CodBar? CodBar, bool Duplicado)>();
}

