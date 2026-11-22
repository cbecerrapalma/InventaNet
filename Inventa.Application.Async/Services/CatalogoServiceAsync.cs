using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Models;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface ICatalogoServiceAsync
{
    Task<Result<IReadOnlyList<Categoria>>> GetCategoriasAsync(int idTenant, CancellationToken ct = default);
    Task<Result<Categoria>> CreateCategoriaAsync(int idTenant, string codigo, string nombre, long? idPadre = null, short orden = 0, string? imagenUrl = null, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Marca>>> GetMarcasAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Unidad>>> GetUnidadesAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<AtriDef>>> GetAtriDefsAsync(int idCategoria, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MetodoPago>>> GetMetodosPagoAsync(CancellationToken ct = default);
    Task<Result<IReadOnlyList<Cliente>>> GetClientesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<PagedResult<Cliente>>> GetClientesPagedAsync(int idTenant, string? q, bool? activo, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Cliente>> CreateClienteAsync(int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Proveedor>>> GetProveedoresAsync(int idTenant, CancellationToken ct = default);
    Task<Result<PagedResult<Proveedor>>> GetProveedoresPagedAsync(int idTenant, string? q, bool? activo, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Proveedor?>> GetProveedorByIdAsync(int idProveedor, int idTenant, CancellationToken ct = default);
    Task<Result<Proveedor>> CreateProveedorAsync(int idTenant, string codigo, string razonSocial, CancellationToken ct = default);
    Task<Result<Proveedor>> UpdateProveedorAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default);
    Task<Result<Cliente?>> GetClienteByIdAsync(int idCliente, int idTenant, CancellationToken ct = default);
    Task<Result<Cliente>> UpdateClienteAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default);
    Task<Result<Categoria?>> GetCategoriaByIdAsync(long idCategoria, int idTenant, CancellationToken ct = default);
    Task<Result<Categoria>> UpdateCategoriaAsync(long idCategoria, int idTenant, string codigo, string nombre, long? idPadre = null, short orden = 0, string? imagenUrl = null, CancellationToken ct = default);
    Task<Result<Marca>> CreateMarcaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<Marca>> UpdateMarcaAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default);
}

/// <summary>
/// S57.34: servicio de catálogos 100% sobre repositorios async. Sin DbContext/EF.
/// </summary>
public sealed class CatalogoServiceAsync : ICatalogoServiceAsync
{
    private readonly ICategoriaRepositoryAsync _categorias;
    private readonly IMarcaRepositoryAsync _marcas;
    private readonly IUnidadRepositoryAsync _unidades;
    private readonly IAtriDefRepositoryAsync _atriDefs;
    private readonly IMetodoPagoRepositoryAsync _metodosPago;
    private readonly IClienteRepositoryAsync _clientes;
    private readonly IProveedorRepositoryAsync _proveedores;

    public CatalogoServiceAsync(
        ICategoriaRepositoryAsync categorias,
        IMarcaRepositoryAsync marcas,
        IUnidadRepositoryAsync unidades,
        IAtriDefRepositoryAsync atriDefs,
        IMetodoPagoRepositoryAsync metodosPago,
        IClienteRepositoryAsync clientes,
        IProveedorRepositoryAsync proveedores)
    {
        _categorias = categorias;
        _marcas = marcas;
        _unidades = unidades;
        _atriDefs = atriDefs;
        _metodosPago = metodosPago;
        _clientes = clientes;
        _proveedores = proveedores;
    }

    public async Task<Result<IReadOnlyList<Categoria>>> GetCategoriasAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _categorias.GetByTenantAsync(idTenant, false, ct);
        return Result<IReadOnlyList<Categoria>>.Success(list);
    }

    public async Task<Result<Categoria>> CreateCategoriaAsync(int idTenant, string codigo, string nombre, long? idPadre = null, short orden = 0, string? imagenUrl = null, CancellationToken ct = default)
    {
        var err = await ValidarJerarquiaCategoriaAsync(null, idTenant, idPadre, orden, imagenUrl, ct);
        if (err != null) return Result<Categoria>.Failure(err);
        var cat = new Categoria { IdTenant = idTenant, Codigo = codigo, Nombre = nombre, IdPadre = idPadre, Orden = orden, ImagenUrl = NormalizarImagenUrl(imagenUrl) };
        await _categorias.CreateAsync(cat, ct);
        return Result<Categoria>.Success(cat);
    }

    public async Task<Result<IReadOnlyList<Marca>>> GetMarcasAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _marcas.GetByTenantAsync(idTenant, false, ct);
        return Result<IReadOnlyList<Marca>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Unidad>>> GetUnidadesAsync(CancellationToken ct = default)
    {
        var list = await _unidades.GetAllOrdenadoAsync(ct);
        return Result<IReadOnlyList<Unidad>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<AtriDef>>> GetAtriDefsAsync(int idCategoria, CancellationToken ct = default)
    {
        var list = await _atriDefs.GetByCategoriaAsync(idCategoria, false, ct);
        return Result<IReadOnlyList<AtriDef>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<MetodoPago>>> GetMetodosPagoAsync(CancellationToken ct = default)
    {
        var list = await _metodosPago.GetAllAsync(ct);
        return Result<IReadOnlyList<MetodoPago>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Cliente>>> GetClientesAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _clientes.GetRecentAsync(idTenant, 200, ct);
        return Result<IReadOnlyList<Cliente>>.Success(list);
    }

    public async Task<Result<PagedResult<Cliente>>> GetClientesPagedAsync(int idTenant, string? q, bool? activo, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;
        // S115 D5: siempre pagina en la BD (antes, sin texto, devolvía hasta 200 en una sola página).
        var paged = await _clientes.SearchPagedAsync(idTenant, q, activo, page, pageSize, ct);
        return Result<PagedResult<Cliente>>.Success(PagedResult<Cliente>.Create(paged.Items, page, pageSize, paged.Total));
    }

    public async Task<Result<Cliente>> CreateClienteAsync(int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default)
    {
        var cod=(codigo??"").Trim(); var nom=(nombre??"").Trim(); var r=(rut??"").Trim();
        if(string.IsNullOrWhiteSpace(cod)) return Result<Cliente>.Failure(new Error("VALIDATION","Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(cod.Length>50) return Result<Cliente>.Failure(new Error("VALIDATION","Codigo excede 50").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(string.IsNullOrWhiteSpace(nom)) return Result<Cliente>.Failure(new Error("VALIDATION","Nombre es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(nom.Length>200) return Result<Cliente>.Failure(new Error("VALIDATION","Nombre excede 200").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(!string.IsNullOrEmpty(r) && r.Length>20) return Result<Cliente>.Failure(new Error("VALIDATION","RUT excede 20").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(await _clientes.ExistsByCodigoAsync(idTenant, cod, ct)) return Result<Cliente>.Failure(new Error("CLIENTE_CODIGO_DUPLICADO","Ya existe un cliente con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var c = new Cliente { IdTenant = idTenant, Codigo = cod, Nombre = nom, RUT = string.IsNullOrWhiteSpace(r)?null:r };
        var (created, dup) = await _clientes.CreateAsync(c, ct);
        if (dup || created is null) return Result<Cliente>.Failure(new Error("CLIENTE_CODIGO_DUPLICADO","Ya existe un cliente con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        return Result<Cliente>.Success(created);
    }

    public async Task<Result<IReadOnlyList<Proveedor>>> GetProveedoresAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _proveedores.GetRecentAsync(idTenant, 200, ct);
        return Result<IReadOnlyList<Proveedor>>.Success(list);
    }

    public async Task<Result<PagedResult<Proveedor>>> GetProveedoresPagedAsync(int idTenant, string? q, bool? activo, int page, int pageSize, CancellationToken ct = default)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 200) pageSize = 20;
        // S115 D5: siempre pagina en la BD (antes, sin texto, devolvía hasta 200 en una sola página).
        var paged = await _proveedores.SearchPagedAsync(idTenant, q, activo, page, pageSize, ct);
        return Result<PagedResult<Proveedor>>.Success(PagedResult<Proveedor>.Create(paged.Items, page, pageSize, paged.Total));
    }

    public async Task<Result<Proveedor?>> GetProveedorByIdAsync(int idProveedor, int idTenant, CancellationToken ct = default)
    {
        var p = await _proveedores.GetByIdInTenantAsync(idProveedor, idTenant, ct);
        if (p == null) return Result<Proveedor?>.Failure(new Error("NOT_FOUND", "Proveedor no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Proveedor?>.Success(p);
    }

    public async Task<Result<Proveedor>> CreateProveedorAsync(int idTenant, string codigo, string razonSocial, CancellationToken ct = default)
    {
        var cod=(codigo??"").Trim(); var raz=(razonSocial??"").Trim();
        if(string.IsNullOrWhiteSpace(cod)) return Result<Proveedor>.Failure(new Error("VALIDATION","Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(cod.Length>50) return Result<Proveedor>.Failure(new Error("VALIDATION","Codigo excede 50").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(string.IsNullOrWhiteSpace(raz)) return Result<Proveedor>.Failure(new Error("VALIDATION","RazonSocial es requerida").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(raz.Length>200) return Result<Proveedor>.Failure(new Error("VALIDATION","RazonSocial excede 200").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(await _proveedores.ExistsByCodigoAsync(idTenant, cod, ct)) return Result<Proveedor>.Failure(new Error("PROVEEDOR_CODIGO_DUPLICADO","Ya existe un proveedor con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var prov = new Proveedor { IdTenant = idTenant, Codigo = cod, RazonSocial = raz };
        var (created, dup) = await _proveedores.CreateAsync(prov, ct);
        if (dup || created is null) return Result<Proveedor>.Failure(new Error("PROVEEDOR_CODIGO_DUPLICADO","Ya existe un proveedor con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        return Result<Proveedor>.Success(created);
    }

    public async Task<Result<Proveedor>> UpdateProveedorAsync(int idProveedor, int idTenant, string codigo, string razonSocial, CancellationToken ct = default)
    {
        var cod=(codigo??"").Trim(); var raz=(razonSocial??"").Trim(); if(string.IsNullOrWhiteSpace(cod)) return Result<Proveedor>.Failure(new Error("VALIDATION","Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(cod.Length>50) return Result<Proveedor>.Failure(new Error("VALIDATION","Codigo excede 50").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(string.IsNullOrWhiteSpace(raz)) return Result<Proveedor>.Failure(new Error("VALIDATION","RazonSocial es requerida").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(raz.Length>200) return Result<Proveedor>.Failure(new Error("VALIDATION","RazonSocial excede 200").WithType(ErrorType.Validation).WithHttpStatus(400));
        var current = await _proveedores.GetByIdInTenantAsync(idProveedor, idTenant, ct);
        if (current == null) return Result<Proveedor>.Failure(new Error("NOT_FOUND", "Proveedor no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if(!string.Equals(current.Codigo, cod, StringComparison.OrdinalIgnoreCase)
            && await _proveedores.ExistsByCodigoAsync(idTenant, cod, ct)) return Result<Proveedor>.Failure(new Error("PROVEEDOR_CODIGO_DUPLICADO","Ya existe un proveedor con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var (updated, dup) = await _proveedores.UpdateAsync(idProveedor, idTenant, cod, raz, ct);
        if (dup || updated is null) return Result<Proveedor>.Failure(dup
            ? new Error("PROVEEDOR_CODIGO_DUPLICADO","Ya existe un proveedor con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409)
            : new Error("NOT_FOUND", "Proveedor no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Proveedor>.Success(updated);
    }

    public async Task<Result<Cliente?>> GetClienteByIdAsync(int idCliente, int idTenant, CancellationToken ct = default)
    {
        var c = await _clientes.GetByIdInTenantAsync(idCliente, idTenant, ct);
        if(c==null) return Result<Cliente?>.Failure(new Error("NOT_FOUND","Cliente no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Cliente?>.Success(c);
    }

    public async Task<Result<Cliente>> UpdateClienteAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default)
    {
        var cod=(codigo??"").Trim(); var nom=(nombre??"").Trim(); var r=(rut??"").Trim(); if(string.IsNullOrWhiteSpace(cod)) return Result<Cliente>.Failure(new Error("VALIDATION","Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(cod.Length>50) return Result<Cliente>.Failure(new Error("VALIDATION","Codigo excede 50").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(string.IsNullOrWhiteSpace(nom)) return Result<Cliente>.Failure(new Error("VALIDATION","Nombre es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(nom.Length>200) return Result<Cliente>.Failure(new Error("VALIDATION","Nombre excede 200").WithType(ErrorType.Validation).WithHttpStatus(400));
        if(!string.IsNullOrEmpty(r) && r.Length>20) return Result<Cliente>.Failure(new Error("VALIDATION","RUT excede 20").WithType(ErrorType.Validation).WithHttpStatus(400));
        var current = await _clientes.GetByIdInTenantAsync(idCliente, idTenant, ct);
        if (current == null) return Result<Cliente>.Failure(new Error("NOT_FOUND", "Cliente no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if(!string.Equals(current.Codigo, cod, StringComparison.OrdinalIgnoreCase)
            && await _clientes.ExistsByCodigoAsync(idTenant, cod, ct)) return Result<Cliente>.Failure(new Error("CLIENTE_CODIGO_DUPLICADO","Ya existe un cliente con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var (updated, dup) = await _clientes.UpdateAsync(idCliente, idTenant, cod, nom, string.IsNullOrWhiteSpace(r) ? null : r, ct);
        if (dup || updated is null) return Result<Cliente>.Failure(dup
            ? new Error("CLIENTE_CODIGO_DUPLICADO","Ya existe un cliente con ese código").WithType(ErrorType.Conflict).WithHttpStatus(409)
            : new Error("NOT_FOUND", "Cliente no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Cliente>.Success(updated);
    }

    public async Task<Result<Categoria?>> GetCategoriaByIdAsync(long idCategoria, int idTenant, CancellationToken ct = default)
    {
        var c = await _categorias.GetByIdInTenantAsync(idCategoria, idTenant, ct);
        // S57.46 CLASS-A: null => Failure 404 (antes Success(c) lanzaba => 500 en GET /api/categorias/{id}).
        if (c == null) return Result<Categoria?>.Failure(new Error("NOT_FOUND", "Categoria no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Categoria?>.Success(c);
    }

    public async Task<Result<Categoria>> UpdateCategoriaAsync(long idCategoria, int idTenant, string codigo, string nombre, long? idPadre = null, short orden = 0, string? imagenUrl = null, CancellationToken ct = default)
    {
        // S69: la entidad rastreada se muta aquí; UpdateAsync (mismo scope)
        // recupera la misma instancia y SaveChanges persiste todo.
        var actual = await _categorias.GetByIdInTenantAsync(idCategoria, idTenant, ct);
        if (actual == null) return Result<Categoria>.Failure(new Error("NOT_FOUND", "Categoria no encontrada").WithHttpStatus(404));
        var err = await ValidarJerarquiaCategoriaAsync(idCategoria, idTenant, idPadre, orden, imagenUrl, ct);
        if (err != null) return Result<Categoria>.Failure(err);
        if (idPadre.HasValue)
        {
            var todas = await _categorias.GetByTenantAsync(idTenant, false, ct);
            if (todas.Any(c => c.IdPadre == idCategoria))
                return Result<Categoria>.Failure(new Error("CATEGORIA_TIENE_HIJAS", "La categoría tiene subcategorías y no puede moverse bajo un padre").WithType(ErrorType.Conflict).WithHttpStatus(409));
        }
        actual.IdPadre = idPadre;
        actual.Orden = orden;
        actual.ImagenUrl = NormalizarImagenUrl(imagenUrl);
        var c = await _categorias.UpdateAsync(idCategoria, idTenant, codigo, nombre, ct);
        if (c == null) return Result<Categoria>.Failure(new Error("NOT_FOUND", "Categoria no encontrada").WithHttpStatus(404));
        return Result<Categoria>.Success(c);
    }

    // S69: valida jerarquía (autopadre, padre mismo tenant y raíz, orden,
    // imagen). Reusa GetByIdInTenantAsync/GetByTenantAsync; sin repo nuevo.
    private async Task<Error?> ValidarJerarquiaCategoriaAsync(long? idCategoria, int idTenant, long? idPadre, short orden, string? imagenUrl, CancellationToken ct)
    {
        // S70 (H6): rango válido 0..9999.
        if (orden < 0 || orden > 9999)
            return new Error("CATEGORIA_ORDEN_INVALIDO", "Orden debe estar entre 0 y 9999").WithType(ErrorType.Validation).WithHttpStatus(400);
        var img = NormalizarImagenUrl(imagenUrl);
        if (img != null && !CategoriaImagenUrlRule.EsValida(img))
            return new Error("CATEGORIA_IMAGENURL_INVALIDA", "ImagenUrl debe ser http(s) absoluta o relativa /uploads/... o /images/... (máx. 500)").WithType(ErrorType.Validation).WithHttpStatus(400);
        if (idPadre.HasValue)
        {
            if (idCategoria.HasValue && idPadre.Value == idCategoria.Value)
                return new Error("CATEGORIA_AUTOPADRE", "Una categoría no puede ser su propio padre").WithType(ErrorType.Validation).WithHttpStatus(400);
            // GetByIdInTenantAsync lanza con id <= 0: no existe => 404.
            var padre = idPadre.Value <= 0 ? null : await _categorias.GetByIdInTenantAsync(idPadre.Value, idTenant, ct);
            if (padre == null)
                return new Error("CATEGORIA_PADRE_NO_ENCONTRADA", "Categoría padre no encontrada en el tenant").WithType(ErrorType.NotFound).WithHttpStatus(404);
            if (padre.IdPadre.HasValue)
                return new Error("CATEGORIA_MAX_NIVELES", "El padre debe ser una categoría raíz (máximo 2 niveles)").WithType(ErrorType.Validation).WithHttpStatus(400);
        }
        return null;
    }

    private static string? NormalizarImagenUrl(string? imagenUrl)
        => string.IsNullOrWhiteSpace(imagenUrl) ? null : imagenUrl.Trim();

    public async Task<Result<Marca>> CreateMarcaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var m = new Marca { IdTenant = idTenant, Codigo = codigo, Nombre = nombre };
        await _marcas.CreateAsync(m, ct);
        return Result<Marca>.Success(m);
    }

    public async Task<Result<Marca>> UpdateMarcaAsync(int idMarca, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var m = await _marcas.UpdateAsync(idMarca, idTenant, codigo, nombre, ct);
        if (m == null) return Result<Marca>.Failure(new Error("NOT_FOUND", "Marca no encontrada").WithHttpStatus(404));
        return Result<Marca>.Success(m);
    }
}
