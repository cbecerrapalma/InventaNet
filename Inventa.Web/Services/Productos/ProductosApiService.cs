using Inventa.Web.Models.Common;
using Inventa.Web.Models.Precios;
using Inventa.Web.Models.Productos;
using Inventa.Web.Services.Http;

using System;

namespace Inventa.Web.Services.Productos;

public sealed class ProductosApiService
{
    private readonly ApiClient _api;
    public ProductosApiService(ApiClient api) => _api = api;

    public Task<List<ProdDto>?> ListAsync(CancellationToken ct) => _api.GetAsync<List<ProdDto>>("api/productos", ct);
    public Task<PagedResponse<ProdDto>?> ListPagedAsync(string? q, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<ProdDto>>($"api/productos?{string.Join("&", qs)}", ct);
    }
    // S117 (P19): buscador de presentaciones; uso = "venta" | "compra"; idBodega trae el disponible (venta).
    public Task<PagedResponse<PresentacionBusquedaDto>?> SearchPresentacionesPagedAsync(string? q, string uso, int? idBodega, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string>();
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        qs.Add($"uso={Uri.EscapeDataString(uso)}");
        if (idBodega is > 0) qs.Add($"idBodega={idBodega}");
        qs.Add($"page={page}"); qs.Add($"pageSize={pageSize}");
        return _api.GetAsync<PagedResponse<PresentacionBusquedaDto>>($"api/productos/presentaciones/search?{string.Join("&", qs)}", ct);
    }
    public Task<ProdDto?> GetAsync(long id, CancellationToken ct) => _api.GetAsync<ProdDto>($"api/productos/{id}", ct);
    // S57.2 publishability toggle (PRODUCTOS_PRINCIPAL_EDITAR o PRODUCTOS_DETALLE_EDITAR, tenant-scoped server-side).
    // S57.3 returns applied flag + warnings (backend re-validates; warnings never block the explicit act).
    public Task<VisibilidadResultDto?> SetVisibleStoreAsync(long id, bool visible, CancellationToken ct) => _api.PatchAsync<object, VisibilidadResultDto?>($"api/productos/{id}/visibilidad", new { visible }, ct);
    // S57.3 P1 review cockpit: centralized diagnostics, single request (VER perm, tenant-scoped).
    public Task<List<PublishabilityDto>?> GetPublishabilityAsync(CancellationToken ct) => _api.GetAsync<List<PublishabilityDto>>("api/productos/publishability", ct);
    // S57.3 P3 publication preview: same public DTO the store would show (VisibleStore bypassed server-side).
    public Task<StorefrontProductDetailDto?> GetPreviewAsync(long id, CancellationToken ct) => _api.GetAsync<StorefrontProductDetailDto?>($"api/productos/{id}/preview", ct);
    public Task<List<VarProdDto>?> GetVariantesAsync(long idProd, CancellationToken ct) => _api.GetAsync<List<VarProdDto>>($"api/productos/{idProd}/variantes", ct);
    // S92: la ruta GET api/productos/{idVar}/presentaciones no existe (H1); la ficha trae variantes+presentación+códigos.
    public Task<FichaProductoDto?> GetFichaAsync(long idProd, CancellationToken ct) => _api.GetAsync<FichaProductoDto>($"api/productos/{idProd}/ficha", ct);
    /// <summary>S92: presentación (única, generada) de una variante, leída desde la ficha del producto.</summary>
    public async Task<List<PresProdDto>> GetPresentacionesDeVarianteAsync(long idProd, long idVar, CancellationToken ct)
    {
        var ficha = await GetFichaAsync(idProd, ct);
        var v = ficha?.Variantes.FirstOrDefault(x => x.IdVarProd == idVar);
        return v?.Presentacion is { } p
            ? [new PresProdDto { IdPresProd = p.IdPresProd, IdVarProd = idVar, Descrip = p.Descrip, Activo = p.Activo, EsVenta = p.EsVenta }]
            : [];
    }
    public Task<ProductoResumenDto?> GetResumenAsync(CancellationToken ct) => _api.GetAsync<ProductoResumenDto>("api/productos/resumen", ct);
    public Task<PagedResponse<ProductoListadoItemDto>?> GetListadoAsync(string? q, string? estado, string? precio, string? imagen, string? marca,
        long? idCategoria, bool? activo, int page, int pageSize, CancellationToken ct)
    {
        var qs = new List<string> { $"page={page}", $"pageSize={pageSize}" };
        if (!string.IsNullOrWhiteSpace(q)) qs.Add($"q={Uri.EscapeDataString(q.Trim())}");
        if (!string.IsNullOrWhiteSpace(estado)) qs.Add($"estado={Uri.EscapeDataString(estado)}");
        if (!string.IsNullOrWhiteSpace(precio)) qs.Add($"precio={precio}");
        if (!string.IsNullOrWhiteSpace(imagen)) qs.Add($"imagen={imagen}");
        if (!string.IsNullOrWhiteSpace(marca)) qs.Add($"marca={marca}");
        if (idCategoria.HasValue) qs.Add($"idCategoria={idCategoria}");
        if (activo.HasValue) qs.Add($"activo={activo.Value.ToString().ToLowerInvariant()}");
        return _api.GetAsync<PagedResponse<ProductoListadoItemDto>>($"api/productos/listado?{string.Join("&", qs)}", ct);
    }
    public Task<CodBarDto?> GetCodBarAsync(string codigo, CancellationToken ct) => _api.GetAsync<CodBarDto>($"api/productos/codbar/{Uri.EscapeDataString(codigo)}", ct);

    // S94: escritura del catálogo por el servicio único del servidor (producto + variante + presentación 1:1 +
    // código principal + precio inicial). Las presentaciones ya no se crean a mano (410 en el servidor).
    public Task<CatalogoEdicionResultadoDto?> CreateAsync(ProductoNuevoDto dto, CancellationToken ct) =>
        _api.PostAsync<ProductoNuevoDto, CatalogoEdicionResultadoDto>("api/productos", dto, ct);
    public Task<CatalogoEdicionResultadoDto?> UpdateAsync(long id, CreateProdDto dto, CancellationToken ct) =>
        _api.PutAsync<CreateProdDto, CatalogoEdicionResultadoDto>($"api/productos/{id}", dto, ct);
    public Task<CatalogoEdicionResultadoDto?> CreateVarianteAsync(long idProd, VarianteNuevaDto dto, CancellationToken ct) =>
        _api.PostAsync<VarianteNuevaDto, CatalogoEdicionResultadoDto>($"api/productos/{idProd}/variantes", dto, ct);
    public Task<CatalogoEdicionResultadoDto?> UpdateVarianteAsync(long idVarProd, VarianteEdicionDto dto, CancellationToken ct) =>
        _api.PutAsync<VarianteEdicionDto, CatalogoEdicionResultadoDto>($"api/productos/variantes/{idVarProd}", dto, ct);
    public Task<CatalogoEdicionResultadoDto?> AgregarCodigoAsync(long idVarProd, CodigoNuevoDto dto, CancellationToken ct) =>
        _api.PostAsync<CodigoNuevoDto, CatalogoEdicionResultadoDto>($"api/productos/variantes/{idVarProd}/codbar", dto, ct);
    public Task<CatalogoEdicionResultadoDto?> MarcarPrincipalAsync(long idCodBar, CancellationToken ct) =>
        _api.PutAsync<object, CatalogoEdicionResultadoDto>($"api/productos/codbar/{idCodBar}/principal", new { }, ct);
    public Task QuitarCodigoAsync(long idCodBar, CancellationToken ct) => _api.DeleteAsync($"api/productos/codbar/{idCodBar}", ct);

    // Imagenes
    public Task<List<ProductoImagenDto>?> ListImagenesAsync(long idProd, CancellationToken ct) => _api.GetAsync<List<ProductoImagenDto>>($"api/productos/{idProd}/imagenes", ct);
    // S104: subida directa. La API autoriza y firma (subidas); el navegador envía los bytes al storage con esa
    // autorización (CBP.Blazor DirectUploadClient) y después confirma. La API nunca recibe el contenido.
    public Task<ProductoImagenSubidaDto?> IniciarSubidaImagenAsync(long idProd, string nombreArchivo, string contentType, long tamanoBytes, CancellationToken ct) =>
        _api.PostAsync<object, ProductoImagenSubidaDto>($"api/productos/{idProd}/imagenes/subidas", new { nombreArchivo, contentType, tamanoBytes }, ct);
    public Task<ProductoImagenDto?> ConfirmarSubidaImagenAsync(long idProd, long idArchivo, CancellationToken ct) =>
        _api.PostAsync<object, ProductoImagenDto>($"api/productos/{idProd}/imagenes/subidas/{idArchivo}/confirmar", new { }, ct);
    public Task SetPrincipalAsync(long idProd, int idImagen, CancellationToken ct) => _api.PutAsync<object>($"api/productos/{idProd}/imagenes/{idImagen}/principal", new {}, ct);
    public Task<List<ProductoImagenDto>?> ReorderAsync(long idProd, IReadOnlyList<int> ids, CancellationToken ct) => _api.PutAsync<object, List<ProductoImagenDto>>($"api/productos/{idProd}/imagenes/reorder", new { ids }, ct);
    public Task DeleteImagenAsync(long idProd, int idImagen, CancellationToken ct) => _api.DeleteAsync($"api/productos/{idProd}/imagenes/{idImagen}", ct);

    // Catalogs
    public Task<List<CategoriaDto>?> GetCategoriasAsync(CancellationToken ct) => _api.GetAsync<List<CategoriaDto>>("api/catalogos/categorias", ct);
    public Task<List<MarcaDto>?> GetMarcasAsync(CancellationToken ct) => _api.GetAsync<List<MarcaDto>>("api/catalogos/marcas", ct);
    public Task<List<UnidadDto>?> GetUnidadesAsync(CancellationToken ct) => _api.GetAsync<List<UnidadDto>>("api/catalogos/unidades", ct);
    public Task<List<ProveedorDto>?> GetProveedoresAsync(CancellationToken ct) => _api.GetAsync<List<ProveedorDto>>("api/proveedores", ct);
}
