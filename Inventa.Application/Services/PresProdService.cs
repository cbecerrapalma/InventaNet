using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IPresProdService : IServiceSync<PresProd, PresProdDto>
{
    Result<IReadOnlyList<PresProdDto>> GetByVariante(long idVarProd, bool soloActivos = true);
    Result<IReadOnlyList<PresProdDto>> GetByUsoPresentacion(long idVarProd, bool esVenta);
    Result<bool> ExistsByVarianteUnidad(long idVarProd, int idUnidad);
    Result<IReadOnlyList<PresProdDto>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<IReadOnlyList<PresProdDto>> GetByProducto(long idProd, bool soloActivos = true);
}

public class PresProdService : ServiceSync<PresProd, PresProdDto>, IPresProdService
{
    private readonly IPresProdRepository _repo;
    public PresProdService(IPresProdRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<PresProdDto>> GetByVariante(long idVarProd, bool soloActivos = true) => _repo.GetByVariante(idVarProd, soloActivos).Map(l => Mapper.Map<IReadOnlyList<PresProdDto>>(l));
    public Result<IReadOnlyList<PresProdDto>> GetByUsoPresentacion(long idVarProd, bool esVenta) => _repo.GetByUsoPresentacion(idVarProd, esVenta).Map(l => Mapper.Map<IReadOnlyList<PresProdDto>>(l));
    public Result<bool> ExistsByVarianteUnidad(long idVarProd, int idUnidad) => _repo.ExistsByVarianteUnidad(idVarProd, idUnidad);
    public Result<IReadOnlyList<PresProdDto>> GetByTenant(int idTenant, bool soloActivos = true) => _repo.GetByTenant(idTenant, soloActivos).Map(l => Mapper.Map<IReadOnlyList<PresProdDto>>(l));
    public Result<IReadOnlyList<PresProdDto>> GetByProducto(long idProd, bool soloActivos = true) => _repo.GetByProducto(idProd, soloActivos).Map(l => Mapper.Map<IReadOnlyList<PresProdDto>>(l));
}
