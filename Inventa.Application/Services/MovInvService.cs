using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IMovInvService : IServiceSync<MovInv, MovInvDto>
{
    Result<IReadOnlyList<MovInvDto>> GetByPresentacion(int idTenant, long idPresProd, DateTime desde, DateTime hasta);
    Result<IReadOnlyList<MovInvDto>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta);
    Result<IReadOnlyList<MovInvDto>> GetByTipoMov(int idTenant, byte tipoMov, DateTime desde, DateTime hasta);
    Result<IReadOnlyList<MovInvDto>> GetByLote(int idTenant, long idLote);
    Result<decimal?> GetSumaCantBase(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta);
    Result<IReadOnlyList<MovInvDto>> GetUltimosMovimientos(int idTenant, int idBodega, int cantidad = 50);
    Result<IReadOnlyList<MovInvDto>> GetKardexVariante(int idTenant, long idVarProd, DateTime desde, DateTime hasta);
}

public class MovInvService : ServiceSync<MovInv, MovInvDto>, IMovInvService
{
    private readonly IMovInvRepository _repo;
    public MovInvService(IMovInvRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<MovInvDto>> GetByPresentacion(int idTenant, long idPresProd, DateTime desde, DateTime hasta) => _repo.GetByPresentacion(idTenant, idPresProd, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
    public Result<IReadOnlyList<MovInvDto>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta) => _repo.GetByBodega(idTenant, idBodega, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
    public Result<IReadOnlyList<MovInvDto>> GetByTipoMov(int idTenant, byte tipoMov, DateTime desde, DateTime hasta) => _repo.GetByTipoMov(idTenant, tipoMov, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
    public Result<IReadOnlyList<MovInvDto>> GetByLote(int idTenant, long idLote) => _repo.GetByLote(idTenant, idLote).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
    public Result<decimal?> GetSumaCantBase(int idTenant, long idPresProd, int idBodega, byte tipoMov, DateTime desde, DateTime hasta)
    {
        var r = _repo.GetSumaCantBase(idTenant, idPresProd, idBodega, tipoMov, desde, hasta);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<IReadOnlyList<MovInvDto>> GetUltimosMovimientos(int idTenant, int idBodega, int cantidad = 50) => _repo.GetUltimosMovimientos(idTenant, idBodega, cantidad).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
    public Result<IReadOnlyList<MovInvDto>> GetKardexVariante(int idTenant, long idVarProd, DateTime desde, DateTime hasta) => _repo.GetKardexVariante(idTenant, idVarProd, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<MovInvDto>>(l));
}
