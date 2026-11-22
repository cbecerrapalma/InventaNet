using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IUnidadService : IServiceSync<Unidad, UnidadDto>
{
    Result<UnidadDto?> GetByCodigo(string codigo);
    Result<IReadOnlyList<UnidadDto>> GetAllOrdenado();
    Result<bool> ExistsByCodigo(string codigo);
    Result<bool> IsUsedByProductos(int idUnidad);
}

public class UnidadService : ServiceSync<Unidad, UnidadDto>, IUnidadService
{
    private readonly IUnidadRepository _repo;
    public UnidadService(IUnidadRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<UnidadDto?> GetByCodigo(string codigo) => _repo.GetByCodigo(codigo).Map(e => e == null ? null : Mapper.Map<UnidadDto>(e));
    public Result<IReadOnlyList<UnidadDto>> GetAllOrdenado() => _repo.GetAllOrdenado().Map(l => Mapper.Map<IReadOnlyList<UnidadDto>>(l));
    public Result<bool> ExistsByCodigo(string codigo) => _repo.ExistsByCodigo(codigo);
    public Result<bool> IsUsedByProductos(int idUnidad) => _repo.IsUsedByProductos(idUnidad);
}
