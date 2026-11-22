using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface ITenantService : IServiceSync<Tenant, TenantDto>
{
    Result<TenantDto?> GetByCodigo(string codigo);
    Result<IReadOnlyList<TenantDto>> GetActivos();
    Result<bool> ExistsByCodigo(string codigo);
    Result<int> CountActivos();
}

public class TenantService : ServiceSync<Tenant, TenantDto>, ITenantService
{
    private readonly ITenantRepository _repo;
    public TenantService(ITenantRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<TenantDto?> GetByCodigo(string codigo) => _repo.GetByCodigo(codigo).Map(e => e == null ? null : Mapper.Map<TenantDto>(e));
    public Result<IReadOnlyList<TenantDto>> GetActivos() => _repo.GetActivos().Map(l => Mapper.Map<IReadOnlyList<TenantDto>>(l));
    public Result<bool> ExistsByCodigo(string codigo) => _repo.ExistsByCodigo(codigo);
    public Result<int> CountActivos() => _repo.CountActivos();
}
