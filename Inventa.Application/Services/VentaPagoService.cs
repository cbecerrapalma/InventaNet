using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IVentaPagoService : IServiceSync<VentaPago, VentaPagoDto>
{
    Result<IReadOnlyList<VentaPagoDto>> GetByVenta(long idVenta);
    Result<decimal?> GetTotalPagado(long idVenta);
    Result<IReadOnlyList<VentaPagoDto>> GetByMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta);
    Result<decimal?> GetTotalPorMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta);
}

public class VentaPagoService : ServiceSync<VentaPago, VentaPagoDto>, IVentaPagoService
{
    private readonly IVentaPagoRepository _repo;
    public VentaPagoService(IVentaPagoRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<VentaPagoDto>> GetByVenta(long idVenta) => _repo.GetByVenta(idVenta).Map(l => Mapper.Map<IReadOnlyList<VentaPagoDto>>(l));
    public Result<decimal?> GetTotalPagado(long idVenta)
    {
        var r = _repo.GetTotalPagado(idVenta);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<IReadOnlyList<VentaPagoDto>> GetByMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta) => _repo.GetByMetodoPago(idTenant, idMetodoPago, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<VentaPagoDto>>(l));
    public Result<decimal?> GetTotalPorMetodoPago(int idTenant, int idMetodoPago, DateTime desde, DateTime hasta)
    {
        var r = _repo.GetTotalPorMetodoPago(idTenant, idMetodoPago, desde, hasta);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
}
