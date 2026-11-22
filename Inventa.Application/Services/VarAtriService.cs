using AutoMapper;
using CBP.Results;
using CBP.Services.Abstractions;
using CBP.Services.Sync;
using Inventa.Application.Dtos;
using Inventa.Data.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Services;

public interface IVarAtriService : IServiceSync<VarAtri, VarAtriDto>
{
    Result<IReadOnlyList<VarAtriDto>> GetByVariante(long idVarProd);
    Result<VarAtriDto?> Get(long idVarProd, long idAtriVal);
    Result<bool> Exists(long idVarProd, long idAtriVal);
    Result<IReadOnlyList<VarAtriDto>> GetByVarianteConDetalle(long idVarProd);
    Result<IReadOnlyList<long>> GetVarianteIdsByAtriVal(long idAtriVal);
    Result<int> CountByVariante(long idVarProd);
    Result<bool> VarianteCumpleAtributosObligatorios(long idVarProd, long idCategoria);
}

public class VarAtriService : ServiceSync<VarAtri, VarAtriDto>, IVarAtriService
{
    private readonly IVarAtriRepository _repo;
    public VarAtriService(IVarAtriRepository repo, IMapper mapper) : base(repo, mapper) { _repo = repo; }
    public Result<IReadOnlyList<VarAtriDto>> GetByVariante(long idVarProd) => _repo.GetByVariante(idVarProd).Map(l => Mapper.Map<IReadOnlyList<VarAtriDto>>(l));
    public Result<VarAtriDto?> Get(long idVarProd, long idAtriVal) => _repo.Get(idVarProd, idAtriVal).Map(e => e == null ? null : Mapper.Map<VarAtriDto>(e));
    public Result<bool> Exists(long idVarProd, long idAtriVal) => _repo.Exists(idVarProd, idAtriVal);
    public Result<IReadOnlyList<VarAtriDto>> GetByVarianteConDetalle(long idVarProd) => _repo.GetByVarianteConDetalle(idVarProd).Map(l => Mapper.Map<IReadOnlyList<VarAtriDto>>(l));
    public Result<IReadOnlyList<long>> GetVarianteIdsByAtriVal(long idAtriVal)
        => _repo.GetVarianteIdsByAtriVal(idAtriVal).Map(l => (IReadOnlyList<long>)l.ToList());
    public Result<int> CountByVariante(long idVarProd) => _repo.CountByVariante(idVarProd);
    public Result<bool> VarianteCumpleAtributosObligatorios(long idVarProd, long idCategoria) => _repo.VarianteCumpleAtributosObligatorios(idVarProd, idCategoria);
}
