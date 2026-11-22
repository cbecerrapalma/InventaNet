using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IAtriValService : IServiceSync<AtriVal, AtriValDto>
{
    Result<IReadOnlyList<AtriValDto>> GetByAtriDef(long idAtriDef, bool soloActivos = true);
    Result<AtriValDto?> GetByCodigo(long idAtriDef, string codigo);
    Result<bool> ExistsByCodigo(long idAtriDef, string codigo);
    Result<bool> IsUsedByVariantes(long idAtriVal);
    Result<int> CountByAtriDef(long idAtriDef, bool soloActivos = true);
}

public class AtriValService : ServiceSync<AtriVal, AtriValDto>, IAtriValService
{
    private readonly IAtriValRepository _repo;
    public AtriValService(IAtriValRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<AtriValDto>> GetByAtriDef(long idAtriDef, bool soloActivos = true) => _repo.GetByAtriDef(idAtriDef, soloActivos).Map(l => Mapper.Map<IReadOnlyList<AtriValDto>>(l));
    public Result<AtriValDto?> GetByCodigo(long idAtriDef, string codigo) => _repo.GetByCodigo(idAtriDef, codigo).Map(e => e == null ? null : Mapper.Map<AtriValDto>(e));
    public Result<bool> ExistsByCodigo(long idAtriDef, string codigo) => _repo.ExistsByCodigo(idAtriDef, codigo);
    public Result<bool> IsUsedByVariantes(long idAtriVal) => _repo.IsUsedByVariantes(idAtriVal);
    public Result<int> CountByAtriDef(long idAtriDef, bool soloActivos = true) => _repo.CountByAtriDef(idAtriDef, soloActivos);
}
