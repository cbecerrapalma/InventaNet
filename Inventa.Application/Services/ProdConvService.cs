using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IProdConvService : IServiceSync<ProdConv, ProdConvDto>
{
    Result<IReadOnlyList<ProdConvDto>> GetByVariante(long idVarProd);
    Result<ProdConvDto?> Get(long idVarProd, int idUnidad);
    Result<decimal?> GetFactor(long idVarProd, int idUnidad);
    Result<bool> Exists(long idVarProd, int idUnidad);
    Result<IReadOnlyList<ProdConvDto>> GetByUnidad(int idUnidad);
    Result<decimal> ConvertirCantidad(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad);
}

public class ProdConvService : ServiceSync<ProdConv, ProdConvDto>, IProdConvService
{
    private readonly IProdConvRepository _repo;
    public ProdConvService(IProdConvRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<ProdConvDto>> GetByVariante(long idVarProd) => _repo.GetByVariante(idVarProd).Map(l => Mapper.Map<IReadOnlyList<ProdConvDto>>(l));
    public Result<ProdConvDto?> Get(long idVarProd, int idUnidad) => _repo.Get(idVarProd, idUnidad).Map(e => e == null ? null : Mapper.Map<ProdConvDto>(e));
    public Result<decimal?> GetFactor(long idVarProd, int idUnidad) => _repo.GetFactor(idVarProd, idUnidad);
    public Result<bool> Exists(long idVarProd, int idUnidad) => _repo.Exists(idVarProd, idUnidad);
    public Result<IReadOnlyList<ProdConvDto>> GetByUnidad(int idUnidad) => _repo.GetByUnidad(idUnidad).Map(l => Mapper.Map<IReadOnlyList<ProdConvDto>>(l));
    public Result<decimal> ConvertirCantidad(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad) => _repo.ConvertirCantidad(idVarProd, idUnidadOrigen, idUnidadDestino, cantidad);
}
