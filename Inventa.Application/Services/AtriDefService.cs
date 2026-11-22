using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IAtriDefService : IServiceSync<AtriDef, AtriDefDto>
{
    Result<IReadOnlyList<AtriDefDto>> GetByCategoria(long idCategoria, bool soloActivos = true);
    Result<AtriDefDto?> GetByCodigo(long idCategoria, string codigo);
    Result<bool> ExistsByCodigo(long idCategoria, string codigo);
    Result<bool> TieneValores(long idAtriDef);
    Result<int> CountByCategoria(long idCategoria, bool soloActivos = true);
}

public class AtriDefService : ServiceSync<AtriDef, AtriDefDto>, IAtriDefService
{
    private readonly IAtriDefRepository _repo;
    public AtriDefService(IAtriDefRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<AtriDefDto>> GetByCategoria(long idCategoria, bool soloActivos = true) => _repo.GetByCategoria(idCategoria, soloActivos).Map(l => Mapper.Map<IReadOnlyList<AtriDefDto>>(l));
    public Result<AtriDefDto?> GetByCodigo(long idCategoria, string codigo) => _repo.GetByCodigo(idCategoria, codigo).Map(e => e == null ? null : Mapper.Map<AtriDefDto>(e));
    public Result<bool> ExistsByCodigo(long idCategoria, string codigo) => _repo.ExistsByCodigo(idCategoria, codigo);
    public Result<bool> TieneValores(long idAtriDef) => _repo.TieneValores(idAtriDef);
    public Result<int> CountByCategoria(long idCategoria, bool soloActivos = true) => _repo.CountByCategoria(idCategoria, soloActivos);
}
