using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IMetodoPagoService : IServiceSync<MetodoPago, MetodoPagoDto>
{
    Result<IReadOnlyList<MetodoPagoDto>> GetActivos();
    Result<MetodoPagoDto?> GetByCodigo(string codigo);
    Result<bool> ExistsByCodigo(string codigo);
}

public class MetodoPagoService : ServiceSync<MetodoPago, MetodoPagoDto>, IMetodoPagoService
{
    private readonly IMetodoPagoRepository _repo;
    public MetodoPagoService(IMetodoPagoRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<MetodoPagoDto>> GetActivos() => _repo.GetActivos().Map(l => Mapper.Map<IReadOnlyList<MetodoPagoDto>>(l));
    public Result<MetodoPagoDto?> GetByCodigo(string codigo) => _repo.GetByCodigo(codigo).Map(e => e == null ? null : Mapper.Map<MetodoPagoDto>(e));
    public Result<bool> ExistsByCodigo(string codigo) => _repo.ExistsByCodigo(codigo);
}
