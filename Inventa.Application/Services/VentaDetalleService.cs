using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IVentaDetalleService : IServiceSync<VentaDetalle, VentaDetalleDto>
{
    Result<IReadOnlyList<VentaDetalleDto>> GetByVenta(long idVenta);
    Result<VentaDetalleDto?> GetByVentaYLinea(long idVenta, int numLinea);
    Result<decimal?> GetCantidadVendida(int idTenant, long idPresProd, DateTime desde, DateTime hasta);
    Result<decimal?> GetSubtotalVenta(long idVenta);
    Result<int> CountLineasByVenta(long idVenta);
}

public class VentaDetalleService : ServiceSync<VentaDetalle, VentaDetalleDto>, IVentaDetalleService
{
    private readonly IVentaDetalleRepository _repo;
    public VentaDetalleService(IVentaDetalleRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<VentaDetalleDto>> GetByVenta(long idVenta) => _repo.GetByVenta(idVenta).Map(l => Mapper.Map<IReadOnlyList<VentaDetalleDto>>(l));
    public Result<VentaDetalleDto?> GetByVentaYLinea(long idVenta, int numLinea) => _repo.GetByVentaYLinea(idVenta, numLinea).Map(e => e == null ? null : Mapper.Map<VentaDetalleDto>(e));
    public Result<decimal?> GetCantidadVendida(int idTenant, long idPresProd, DateTime desde, DateTime hasta)
    {
        var r = _repo.GetCantidadVendida(idTenant, idPresProd, desde, hasta);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<decimal?> GetSubtotalVenta(long idVenta)
    {
        var r = _repo.GetSubtotalVenta(idVenta);
        return r.IsFailure ? Result<decimal?>.Failure(r.Error!) : Result<decimal?>.Success(r.Value);
    }
    public Result<int> CountLineasByVenta(long idVenta) => _repo.CountLineasByVenta(idVenta);
}
