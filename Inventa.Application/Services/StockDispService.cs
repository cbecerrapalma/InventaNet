using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IStockDispService : IServiceSync<StockDisp, StockDispDto>
{
    Result<StockDispDto?> Get(int idTenant, long idPresProd, int idBodega);
    Result<decimal?> GetCantidad(int idTenant, long idPresProd, int idBodega);
    Result<bool> TieneStockSuficiente(int idTenant, long idPresProd, int idBodega, decimal cantidad);
    Result<IReadOnlyList<StockDispDto>> GetByBodega(int idTenant, int idBodega, bool soloConStock = false);
    Result<IReadOnlyList<StockDispDto>> GetAgotados(int idTenant, int idBodega);
    Result<IReadOnlyList<StockDispDto>> GetConStockNegativo(int idTenant, int idBodega);
}

public class StockDispService : ServiceSync<StockDisp, StockDispDto>, IStockDispService
{
    private readonly IStockDispRepository _repo;
    public StockDispService(IStockDispRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<StockDispDto?> Get(int idTenant, long idPresProd, int idBodega) => _repo.Get(idTenant, idPresProd, idBodega).Map(e => e == null ? null : Mapper.Map<StockDispDto>(e));
    public Result<decimal?> GetCantidad(int idTenant, long idPresProd, int idBodega)
    {
        var r = _repo.GetCantidad(idTenant, idPresProd, idBodega);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<bool> TieneStockSuficiente(int idTenant, long idPresProd, int idBodega, decimal cantidad) => _repo.TieneStockSuficiente(idTenant, idPresProd, idBodega, cantidad);
    public Result<IReadOnlyList<StockDispDto>> GetByBodega(int idTenant, int idBodega, bool soloConStock = false) => _repo.GetByBodega(idTenant, idBodega, soloConStock).Map(l => Mapper.Map<IReadOnlyList<StockDispDto>>(l));
    public Result<IReadOnlyList<StockDispDto>> GetAgotados(int idTenant, int idBodega) => _repo.GetAgotados(idTenant, idBodega).Map(l => Mapper.Map<IReadOnlyList<StockDispDto>>(l));
    public Result<IReadOnlyList<StockDispDto>> GetConStockNegativo(int idTenant, int idBodega) => _repo.GetConStockNegativo(idTenant, idBodega).Map(l => Mapper.Map<IReadOnlyList<StockDispDto>>(l));
}
