using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IClienteService : IServiceSync<Cliente, ClienteDto>
{
    Result<IReadOnlyList<ClienteDto>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<ClienteDto?> GetByCodigo(int idTenant, string codigo);
    Result<ClienteDto?> GetByRUT(int idTenant, string rut);
    Result<IReadOnlyList<ClienteDto>> GetByTipoCliente(int idTenant, byte tipoCliente);
    Result<IReadOnlyList<ClienteDto>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByRUT(int idTenant, string rut);
    Result<int> CountByTipoCliente(int idTenant, byte tipoCliente);
}

public class ClienteService : ServiceSync<Cliente, ClienteDto>, IClienteService
{
    private readonly IClienteRepository _repo;
    public ClienteService(IClienteRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<ClienteDto>> GetByTenant(int idTenant, bool soloActivos = true) => _repo.GetByTenant(idTenant, soloActivos).Map(l => Mapper.Map<IReadOnlyList<ClienteDto>>(l));
    public Result<ClienteDto?> GetByCodigo(int idTenant, string codigo) => _repo.GetByCodigo(idTenant, codigo).Map(e => e == null ? null : Mapper.Map<ClienteDto>(e));
    public Result<ClienteDto?> GetByRUT(int idTenant, string rut) => _repo.GetByRUT(idTenant, rut).Map(e => e == null ? null : Mapper.Map<ClienteDto>(e));
    public Result<IReadOnlyList<ClienteDto>> GetByTipoCliente(int idTenant, byte tipoCliente) => _repo.GetByTipoCliente(idTenant, tipoCliente).Map(l => Mapper.Map<IReadOnlyList<ClienteDto>>(l));
    public Result<IReadOnlyList<ClienteDto>> SearchByNombre(int idTenant, string nombre) => _repo.SearchByNombre(idTenant, nombre).Map(l => Mapper.Map<IReadOnlyList<ClienteDto>>(l));
    public Result<bool> ExistsByCodigo(int idTenant, string codigo) => _repo.ExistsByCodigo(idTenant, codigo);
    public Result<bool> ExistsByRUT(int idTenant, string rut) => _repo.ExistsByRUT(idTenant, rut);
    public Result<int> CountByTipoCliente(int idTenant, byte tipoCliente) => _repo.CountByTipoCliente(idTenant, tipoCliente);
}
