using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface ILoteService : IServiceSync<Lote, LoteDto>
{
    Result<IReadOnlyList<LoteDto>> GetByVariante(int idTenant, long idVarProd, bool soloActivos = true);
    Result<IReadOnlyList<LoteDto>> GetProximosAVencer(int idTenant, int diasAlerta);
    Result<IReadOnlyList<LoteDto>> GetVencidos(int idTenant);
    Result<IReadOnlyList<LoteDto>> GetPorVencerConStock(int idTenant, int diasAlerta);
    Result<LoteDto?> GetByCodigo(int idTenant, long idVarProd, string codigo);
    Result<bool> ExistsByCodigo(int idTenant, long idVarProd, string codigo);
    Result<IReadOnlyList<LoteDto>> GetOrdenFEFO(int idTenant, long idVarProd, int idBodega);
}

public class LoteService : ServiceSync<Lote, LoteDto>, ILoteService
{
    private readonly ILoteRepository _repo;
    public LoteService(ILoteRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<LoteDto>> GetByVariante(int idTenant, long idVarProd, bool soloActivos = true) => _repo.GetByVariante(idTenant, idVarProd, soloActivos).Map(l => Mapper.Map<IReadOnlyList<LoteDto>>(l));
    public Result<IReadOnlyList<LoteDto>> GetProximosAVencer(int idTenant, int diasAlerta) => _repo.GetProximosAVencer(idTenant, diasAlerta).Map(l => Mapper.Map<IReadOnlyList<LoteDto>>(l));
    public Result<IReadOnlyList<LoteDto>> GetVencidos(int idTenant) => _repo.GetVencidos(idTenant).Map(l => Mapper.Map<IReadOnlyList<LoteDto>>(l));
    public Result<IReadOnlyList<LoteDto>> GetPorVencerConStock(int idTenant, int diasAlerta) => _repo.GetPorVencerConStock(idTenant, diasAlerta).Map(l => Mapper.Map<IReadOnlyList<LoteDto>>(l));
    public Result<LoteDto?> GetByCodigo(int idTenant, long idVarProd, string codigo) => _repo.GetByCodigo(idTenant, idVarProd, codigo).Map(e => e == null ? null : Mapper.Map<LoteDto>(e));
    public Result<bool> ExistsByCodigo(int idTenant, long idVarProd, string codigo) => _repo.ExistsByCodigo(idTenant, idVarProd, codigo);
    public Result<IReadOnlyList<LoteDto>> GetOrdenFEFO(int idTenant, long idVarProd, int idBodega) => _repo.GetOrdenFEFO(idTenant, idVarProd, idBodega).Map(l => Mapper.Map<IReadOnlyList<LoteDto>>(l));
}
