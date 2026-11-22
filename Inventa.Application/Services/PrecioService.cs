using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IPrecioService : IServiceSync<Precio, PrecioDto>
{
    // S57.69: precio normal único (sin discriminator).
    Result<PrecioDto?> GetVigente(long idPresProd, DateOnly? fecha = null);
    Result<IReadOnlyList<PrecioDto>> GetByPresentacion(long idPresProd);
    Result<IReadOnlyList<PrecioDto>> GetHistorial(long idPresProd);
    Result<bool> ExisteVigente(long idPresProd, DateOnly? fecha = null);
    Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null);
}

public class PrecioService : ServiceSync<Precio, PrecioDto>, IPrecioService
{
    private readonly IPrecioRepository _repo;
    public PrecioService(IPrecioRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<PrecioDto?> GetVigente(long idPresProd, DateOnly? fecha = null) => _repo.GetVigente(idPresProd, fecha).Map(e => e == null ? null : Mapper.Map<PrecioDto>(e));
    public Result<IReadOnlyList<PrecioDto>> GetByPresentacion(long idPresProd) => _repo.GetByPresentacion(idPresProd).Map(l => Mapper.Map<IReadOnlyList<PrecioDto>>(l));
    public Result<IReadOnlyList<PrecioDto>> GetHistorial(long idPresProd) => _repo.GetHistorial(idPresProd).Map(l => Mapper.Map<IReadOnlyList<PrecioDto>>(l));
    public Result<bool> ExisteVigente(long idPresProd, DateOnly? fecha = null) => _repo.ExisteVigente(idPresProd, fecha);
    public Result<decimal?> GetValorVigente(long idPresProd, DateOnly? fecha = null) => _repo.GetValorVigente(idPresProd, fecha);
}
