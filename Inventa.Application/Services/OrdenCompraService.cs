using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IOrdenCompraService : IServiceSync<OrdenCompra, OrdenCompraDto>
{
    Result<IReadOnlyList<OrdenCompraDto>> GetByTenant(int idTenant, byte? estado = null);
    Result<IReadOnlyList<OrdenCompraDto>> GetByProveedor(int idTenant, int idProveedor);
    Result<IReadOnlyList<OrdenCompraDto>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta);
    Result<OrdenCompraDto?> GetByNumOrden(int idTenant, string numOrden);
    Result<bool> ExistsByNumOrden(int idTenant, string numOrden);
    Result<Dictionary<byte, decimal>> GetTotalPorEstado(int idTenant);
}

public class OrdenCompraService : ServiceSync<OrdenCompra, OrdenCompraDto>, IOrdenCompraService
{
    private readonly IOrdenCompraRepository _repo;
    public OrdenCompraService(IOrdenCompraRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<OrdenCompraDto>> GetByTenant(int idTenant, byte? estado = null) => _repo.GetByTenant(idTenant, estado).Map(l => Mapper.Map<IReadOnlyList<OrdenCompraDto>>(l));
    public Result<IReadOnlyList<OrdenCompraDto>> GetByProveedor(int idTenant, int idProveedor) => _repo.GetByProveedor(idTenant, idProveedor).Map(l => Mapper.Map<IReadOnlyList<OrdenCompraDto>>(l));
    public Result<IReadOnlyList<OrdenCompraDto>> GetByRangoFecha(int idTenant, DateTime desde, DateTime hasta) => _repo.GetByRangoFecha(idTenant, desde, hasta).Map(l => Mapper.Map<IReadOnlyList<OrdenCompraDto>>(l));
    public Result<OrdenCompraDto?> GetByNumOrden(int idTenant, string numOrden) => _repo.GetByNumOrden(idTenant, numOrden).Map(e => e == null ? null : Mapper.Map<OrdenCompraDto>(e));
    public Result<bool> ExistsByNumOrden(int idTenant, string numOrden) => _repo.ExistsByNumOrden(idTenant, numOrden);
    public Result<Dictionary<byte, decimal>> GetTotalPorEstado(int idTenant)
        => _repo.GetTotalPorEstado(idTenant).Map(d => d.ToDictionary(kvp => kvp.Key, kvp => kvp.Value));
}
