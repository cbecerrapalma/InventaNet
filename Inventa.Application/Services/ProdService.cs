using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IProdService : IServiceSync<Prod, ProdDto>
{
    Result<IReadOnlyList<ProdDto>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<ProdDto>> GetByCategoria(int idTenant, long idCategoria, bool soloActivos = true);
    Result<IReadOnlyList<ProdDto>> GetByMarca(int idTenant, int idMarca, bool soloActivos = true);
    Result<ProdDto?> GetByCodigo(int idTenant, string codigo);
    Result<IReadOnlyList<ProdDto>> SearchByNombre(int idTenant, string nombre, bool soloActivos = true);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<ProdDto>> SearchByCodigoOrNombre(int idTenant, string texto, bool soloActivos = true);
    Result<IReadOnlyList<ProdDto>> GetQueManejanLote(int idTenant);
    Result<IReadOnlyList<ProdDto>> GetPesables(int idTenant);
    Result<IReadOnlyList<ProdDto>> GetConControlVencimiento(int idTenant);
}

public class ProdService : ServiceSync<Prod, ProdDto>, IProdService
{
    private readonly IProdRepository _repo;
    public ProdService(IProdRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<ProdDto>> GetByTenant(int idTenant, bool soloActivos = true) => _repo.GetByTenant(idTenant, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<IReadOnlyList<ProdDto>> GetByCategoria(int idTenant, long idCategoria, bool soloActivos = true) => _repo.GetByCategoria(idTenant, idCategoria, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<IReadOnlyList<ProdDto>> GetByMarca(int idTenant, int idMarca, bool soloActivos = true) => _repo.GetByMarca(idTenant, idMarca, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<ProdDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<ProdDto>(e));
    public Result<IReadOnlyList<ProdDto>> SearchByNombre(int idTenant, string nombre, bool soloActivos = true) => _repo.SearchByNombre(idTenant, nombre, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<int> CountByTenant(int idTenant, bool soloActivos = true) => _repo.CountByTenant(idTenant, soloActivos);
    public Result<IReadOnlyList<ProdDto>> SearchByCodigoOrNombre(int idTenant, string texto, bool soloActivos = true) => _repo.SearchByCodigoOrNombre(idTenant, texto, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<IReadOnlyList<ProdDto>> GetQueManejanLote(int idTenant) => _repo.GetQueManejanLote(idTenant).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<IReadOnlyList<ProdDto>> GetPesables(int idTenant) => _repo.GetPesables(idTenant).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
    public Result<IReadOnlyList<ProdDto>> GetConControlVencimiento(int idTenant) => _repo.GetConControlVencimiento(idTenant).Map(l => Mapper.Map<IReadOnlyList<ProdDto>>(l));
}
