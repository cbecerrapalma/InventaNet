using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface ICodBarService : IServiceSync<CodBar, CodBarDto>
{
    Result<CodBarDto?> GetByCodigo(string codigo, int idTenant);
    Result<bool> ExistsByCodigo(string codigo);
    Result<IReadOnlyList<CodBarDto>> GetByPresentacion(long idPresProd);
    Result<CodBarDto?> GetPrincipalByPresentacion(long idPresProd);
    Result<IReadOnlyList<CodBarDto>> GetByTipo(long idPresProd, byte tipoCod);
    Result<IReadOnlyList<CodBarDto>> GetByTenant(int idTenant);
}

public class CodBarService : ServiceSync<CodBar, CodBarDto>, ICodBarService
{
    private readonly ICodBarRepository _repo;
    public CodBarService(ICodBarRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<CodBarDto?> GetByCodigo(string codigo, int idTenant) => _repo.GetByCodigo(codigo, idTenant).Map(e => e == null ? null : Mapper.Map<CodBarDto>(e));
    public Result<bool> ExistsByCodigo(string codigo) => _repo.ExistsByCodigo(codigo);
    public Result<IReadOnlyList<CodBarDto>> GetByPresentacion(long idPresProd) => _repo.GetByPresentacion(idPresProd).Map(l => Mapper.Map<IReadOnlyList<CodBarDto>>(l));
    public Result<CodBarDto?> GetPrincipalByPresentacion(long idPresProd) => _repo.GetPrincipalByPresentacion(idPresProd).Map(e => e == null ? null : Mapper.Map<CodBarDto>(e));
    public Result<IReadOnlyList<CodBarDto>> GetByTipo(long idPresProd, byte tipoCod) => _repo.GetByTipo(idPresProd, tipoCod).Map(l => Mapper.Map<IReadOnlyList<CodBarDto>>(l));
    public Result<IReadOnlyList<CodBarDto>> GetByTenant(int idTenant) => _repo.GetByTenant(idTenant).Map(l => Mapper.Map<IReadOnlyList<CodBarDto>>(l));
}
