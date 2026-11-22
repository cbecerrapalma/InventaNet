using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface ICategoriaService : IServiceSync<Categoria, CategoriaDto>
{
    Result<IReadOnlyList<CategoriaDto>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<CategoriaDto?> GetByCodigo(int idTenant, string codigo);
    Result<IReadOnlyList<CategoriaDto>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountProductosByCategoria(long idCategoria);
}

public class CategoriaService : ServiceSync<Categoria, CategoriaDto>, ICategoriaService
{
    private readonly ICategoriaRepository _repo;
    public CategoriaService(ICategoriaRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<CategoriaDto>> GetByTenant(int idTenant, bool soloActivas = true) => _repo.GetByTenant(idTenant, soloActivas).Map(l => Mapper.Map<IReadOnlyList<CategoriaDto>>(l));
    public Result<CategoriaDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<CategoriaDto>(e));
    public Result<IReadOnlyList<CategoriaDto>> SearchByNombre(int idTenant, string nombre) => _repo.SearchByNombre(idTenant, nombre).Map(l => Mapper.Map<IReadOnlyList<CategoriaDto>>(l));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<int> CountProductosByCategoria(long idCategoria) => _repo.CountProductosByCategoria(idCategoria);
}
