using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IVentaService : IServiceSync<Venta, VentaDto>
{
    Result<IReadOnlyList<VentaDto>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<IReadOnlyList<VentaDto>> GetByCliente(int idTenant, int idCliente);
    Result<IReadOnlyList<VentaDto>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta);
    Result<VentaDto?> GetByNumVenta(int idTenant, string numVenta);
    Result<bool> ExistsByNumVenta(int idTenant, string numVenta);
    Result<decimal> GetTotalVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<int> CountVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null);
    Result<IReadOnlyList<dynamic>> GetResumenPorTipoDoc(int idTenant, DateTime desde, DateTime hasta);
    Result<IReadOnlyList<VentaDto>> GetAnuladas(int idTenant, DateTime desde, DateTime hasta);
}

public class VentaService : ServiceSync<Venta, VentaDto>, IVentaService
{
    private readonly IVentaRepository _repo;
    public VentaService(IVentaRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<VentaDto>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta, byte? estado = null) => _repo.GetByRangoFecha(idTenant, desde, hasta, estado).Map(l => Mapper.Map<IReadOnlyList<VentaDto>>(l));
    public Result<IReadOnlyList<VentaDto>> GetByCliente(int idTenant, int idCliente) => _repo.GetByCliente(idTenant, idCliente).Map(l => Mapper.Map<IReadOnlyList<VentaDto>>(l));
    public Result<IReadOnlyList<VentaDto>> GetByBodega(int idTenant, int idBodega, DateTime desde, DateTime hasta) => _repo.GetByBodega(idTenant, idBodega, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<VentaDto>>(l));
    public Result<VentaDto?> GetByNumVenta(int idTenant, string numVenta) => _repo.GetByNumVenta(idTenant, numVenta).Map(e => e == null ? null : Mapper.Map<VentaDto>(e));
    public Result<bool> ExistsByNumVenta(int idTenant, string numVenta) => _repo.ExistsByNumVenta(idTenant, numVenta);
    public Result<decimal> GetTotalVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null) => _repo.GetTotalVentas(idTenant, desde, hasta, estado);
    public Result<int> CountVentas(int idTenant, DateTime desde, DateTime hasta, byte? estado = null) => _repo.CountVentas(idTenant, desde, hasta, estado);
    public Result<IReadOnlyList<dynamic>> GetResumenPorTipoDoc(int idTenant, DateTime desde, DateTime hasta)
        => _repo.GetResumenPorTipoDoc(idTenant, desde, hasta).Map(l => (IReadOnlyList<dynamic>)l.ToList());
    public Result<IReadOnlyList<VentaDto>> GetAnuladas(int idTenant, DateTime desde, DateTime hasta) => _repo.GetAnuladas(idTenant, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<VentaDto>>(l));
}
