using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IVarProdService : IServiceSync<VarProd, VarProdDto>
{
    Result<IReadOnlyList<VarProdDto>> GetByProducto(long idProd, bool soloActivos = true);
    Result<VarProdDto?> GetByCodigo(long idProd, string codigo);
    Result<bool> ExistsByCodigo(long idProd, string codigo);
    Result<IReadOnlyList<VarProdDto>> GetConCostoCargado(long idProd);
    Result<IReadOnlyList<VarProdDto>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<VarProdDto>> GetConBajoStock(int idTenant, decimal umbral);
}

public class VarProdService : ServiceSync<VarProd, VarProdDto>, IVarProdService
{
    private readonly IVarProdRepository _repo;
    public VarProdService(IVarProdRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<VarProdDto>> GetByProducto(long idProd, bool soloActivos = true) => _repo.GetByProducto(idProd, soloActivos).Map(l => Mapper.Map<IReadOnlyList<VarProdDto>>(l));
    public Result<VarProdDto?> GetByCodigo(long idProd, string codigo) => _repo.GetByCodigo(idProd, codigo).Map(e => e == null ? null : Mapper.Map<VarProdDto>(e));
    public Result<bool> ExistsByCodigo(long idProd, string codigo) => _repo.ExistsByCodigo(idProd, codigo);
    public Result<IReadOnlyList<VarProdDto>> GetConCostoCargado(long idProd) => _repo.GetConCostoCargado(idProd).Map(l => Mapper.Map<IReadOnlyList<VarProdDto>>(l));
    public Result<IReadOnlyList<VarProdDto>> GetByTenant(int idTenant, bool soloActivos = true) => _repo.GetByTenant(idTenant, soloActivos).Map(l => Mapper.Map<IReadOnlyList<VarProdDto>>(l));
    public Result<IReadOnlyList<VarProdDto>> GetConBajoStock(int idTenant, decimal umbral) => _repo.GetConBajoStock(idTenant, umbral).Map(l => Mapper.Map<IReadOnlyList<VarProdDto>>(l));
}
