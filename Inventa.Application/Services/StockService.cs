using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IStockService : IServiceSync<Stock, StockDto>
{
    Result<IReadOnlyList<StockDto>> GetByTenantBodega(int idTenant, int idBodega, bool incluirLote = false);
    Result<IReadOnlyList<StockDto>> GetByVariante(int idTenant, long idVarProd);
    Result<StockDto?> Get(int idTenant, int idBodega, long idVarProd, long? idLote);
    Result<IReadOnlyList<StockDto>> GetConStockNegativo(int idTenant);
    Result<decimal?> GetCantidadTotal(int idTenant, long idVarProd);
    Result<decimal?> GetCantidadEnBodega(int idTenant, int idBodega, long idVarProd);
    Result<decimal?> GetCostoPromedio(int idTenant, int idBodega, long idVarProd);
}

public class StockService : ServiceSync<Stock, StockDto>, IStockService
{
    private readonly IStockRepository _repo;
    public StockService(IStockRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<StockDto>> GetByTenantBodega(int idTenant, int idBodega, bool incluirLote = false) => _repo.GetByTenantBodega(idTenant, idBodega, incluirLote).Map(l => Mapper.Map<IReadOnlyList<StockDto>>(l));
    public Result<IReadOnlyList<StockDto>> GetByVariante(int idTenant, long idVarProd) => _repo.GetByVariante(idTenant, idVarProd).Map(l => Mapper.Map<IReadOnlyList<StockDto>>(l));
    public Result<StockDto?> Get(int idTenant, int idBodega, long idVarProd, long? idLote) => _repo.Get(idTenant, idBodega, idVarProd, idLote).Map(e => e == null ? null : Mapper.Map<StockDto>(e));
    public Result<IReadOnlyList<StockDto>> GetConStockNegativo(int idTenant) => _repo.GetConStockNegativo(idTenant).Map(l => Mapper.Map<IReadOnlyList<StockDto>>(l));
    public Result<decimal?> GetCantidadTotal(int idTenant, long idVarProd)
    {
        var r = _repo.GetCantidadTotal(idTenant, idVarProd);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<decimal?> GetCantidadEnBodega(int idTenant, int idBodega, long idVarProd)
    {
        var r = _repo.GetCantidadEnBodega(idTenant, idBodega, idVarProd);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<decimal?> GetCostoPromedio(int idTenant, int idBodega, long idVarProd)
    {
        var r = _repo.GetCostoPromedio(idTenant, idBodega, idVarProd);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
}
