using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IMarcaService : IServiceSync<Marca, MarcaDto>
{
    Result<IReadOnlyList<MarcaDto>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<MarcaDto?> GetByCodigo(int idTenant, string codigo);
    Result<IReadOnlyList<MarcaDto>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<int> CountByTenant(int idTenant, bool soloActivas = true);
}

public class MarcaService : ServiceSync<Marca, MarcaDto>, IMarcaService
{
    private readonly IMarcaRepository _repo;
    public MarcaService(IMarcaRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<MarcaDto>> GetByTenant(int idTenant, bool soloActivas = true) => _repo.GetByTenant(idTenant, soloActivas).Map(l => Mapper.Map<IReadOnlyList<MarcaDto>>(l));
    public Result<MarcaDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<MarcaDto>(e));
    public Result<IReadOnlyList<MarcaDto>> SearchByNombre(int idTenant, string nombre) => _repo.SearchByNombre(idTenant, nombre).Map(l => Mapper.Map<IReadOnlyList<MarcaDto>>(l));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<int> CountByTenant(int idTenant, bool soloActivas = true) => _repo.CountByTenant(idTenant, soloActivas);
}
