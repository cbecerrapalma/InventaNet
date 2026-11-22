using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IProveedorService : IServiceSync<Proveedor, ProveedorDto>
{
    Result<IReadOnlyList<ProveedorDto>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<ProveedorDto?> GetByCodigo(int idTenant, string codigo);
    Result<ProveedorDto?> GetByRUT(int idTenant, string rut);
    Result<IReadOnlyList<ProveedorDto>> SearchByRazonSocial(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByRUT(int idTenant, string rut);
    Result<int> CountByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<ProveedorDto>> GetConOrdenesPendientes(int idTenant);
}

public class ProveedorService : ServiceSync<Proveedor, ProveedorDto>, IProveedorService
{
    private readonly IProveedorRepository _repo;
    public ProveedorService(IProveedorRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<ProveedorDto>> GetByTenant(int idTenant, bool soloActivos = true) => _repo.GetByTenant(idTenant, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ProveedorDto>>(l));
    public Result<ProveedorDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<ProveedorDto>(e));
    public Result<ProveedorDto?> GetByRUT(int idTenant, string rut) => _repo.GetByRUT(idTenant, rut).Map(e => e == null ? null : Mapper.Map<ProveedorDto>(e));
    public Result<IReadOnlyList<ProveedorDto>> SearchByRazonSocial(int idTenant, string nombre) => _repo.SearchByRazonSocial(idTenant, nombre).Map(l => Mapper.Map<IReadOnlyList<ProveedorDto>>(l));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<bool> ExistsByRUT(int idTenant, string rut) => _repo.ExistsByRUT(idTenant, rut);
    public Result<int> CountByTenant(int idTenant, bool soloActivos = true) => _repo.CountByTenant(idTenant, soloActivos);
    public Result<IReadOnlyList<ProveedorDto>> GetConOrdenesPendientes(int idTenant) => _repo.GetConOrdenesPendientes(idTenant).Map(l => Mapper.Map<IReadOnlyList<ProveedorDto>>(l));
}
