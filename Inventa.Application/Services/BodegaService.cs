using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IBodegaService : IServiceSync<Bodega, BodegaDto>
{
    Result<IReadOnlyList<BodegaDto>> GetByTenant(int idTenant, bool soloActivas = true);
    Result<BodegaDto?> GetByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> IsActiva(int idTenant, int idBodega);
}

public class BodegaService : ServiceSync<Bodega, BodegaDto>, IBodegaService
{
    private readonly IBodegaRepository _repo;
    public BodegaService(IBodegaRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<BodegaDto>> GetByTenant(int idTenant, bool soloActivas = true) => _repo.GetByTenant(idTenant, soloActivas).Map(l => Mapper.Map<IReadOnlyList<BodegaDto>>(l));
    public Result<BodegaDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<BodegaDto>(e));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<bool> IsActiva(int idTenant, int idBodega) => _repo.IsActiva(idTenant, idBodega);
}
