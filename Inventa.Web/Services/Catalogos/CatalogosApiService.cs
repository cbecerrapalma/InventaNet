using Inventa.Web.Models.Catalogos;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Catalogos;

public sealed class CatalogosApiService
{
    private readonly ApiClient _api;
    public CatalogosApiService(ApiClient api) => _api = api;

    // Categorias tenant-scoped via /api/categorias
    public Task<List<CategoriaDto>?> GetCategoriasAsync(CancellationToken ct) => _api.GetAsync<List<CategoriaDto>>("api/categorias", ct);
    public Task<CategoriaDto?> GetCategoriaAsync(long id, CancellationToken ct) => _api.GetAsync<CategoriaDto>($"api/categorias/{id}", ct);
    public Task<CategoriaDto?> CreateCategoriaAsync(CreateCategoriaDto dto, CancellationToken ct) => _api.PostAsync<CreateCategoriaDto, CategoriaDto>("api/categorias", dto, ct);
    public Task<CategoriaDto?> UpdateCategoriaAsync(long id, CreateCategoriaDto dto, CancellationToken ct) => _api.PutAsync<CreateCategoriaDto, CategoriaDto>($"api/categorias/{id}", dto, ct);

    // Marcas tenant-scoped via /api/catalogos/marcas
    public Task<List<MarcaDto>?> GetMarcasAsync(CancellationToken ct) => _api.GetAsync<List<MarcaDto>>("api/catalogos/marcas", ct);
    public Task<MarcaDto?> CreateMarcaAsync(CreateMarcaDto dto, CancellationToken ct) => _api.PostAsync<CreateMarcaDto, MarcaDto>("api/catalogos/marcas", dto, ct);
    public Task<MarcaDto?> UpdateMarcaAsync(int id, CreateMarcaDto dto, CancellationToken ct) => _api.PutAsync<CreateMarcaDto, MarcaDto>($"api/catalogos/marcas/{id}", dto, ct);

    // Unidades global read-only
    public Task<List<UnidadDto>?> GetUnidadesAsync(CancellationToken ct) => _api.GetAsync<List<UnidadDto>>("api/catalogos/unidades", ct);

    // Metodos pago global
    public Task<List<MetodoPagoDto>?> GetMetodosPagoAsync(CancellationToken ct) => _api.GetAsync<List<MetodoPagoDto>>("api/catalogos/metodos-pago", ct);

    // Atributos master/detail
    public Task<List<AtriDefDto>?> GetAtributosAsync(int idCategoria, CancellationToken ct) => _api.GetAsync<List<AtriDefDto>>($"api/catalogos/atributos?idCategoria={idCategoria}", ct);
}
