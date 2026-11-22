using CBP.Results;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

// S57.36 §4: counterpart Async explícito y trazable de ProdService (Sync).
// DIRECT-1:1 sobre IProdRepositoryAsync. ProductoServiceAsync permanece como
// INTERNAL ORCHESTRATOR (agregado); este servicio es la relación Sync→Async explícita.
public interface IProdServiceAsync
{
    Task<Result<IReadOnlyList<Prod>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Prod?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetPesablesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Prod>>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default);
}

public sealed class ProdServiceAsync : IProdServiceAsync
{
    private readonly IProdRepositoryAsync _repo;
    public ProdServiceAsync(IProdRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Prod>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetByCategoriaAsync(int idTenant, long idCategoria, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByCategoriaAsync(idTenant, idCategoria, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetByMarcaAsync(int idTenant, int idMarca, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByMarcaAsync(idTenant, idMarca, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<Prod?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Prod?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Prod>>> SearchByNombreAsync(int idTenant, string nombre, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.SearchByNombreAsync(idTenant, nombre, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, soloActivos, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<IReadOnlyList<Prod>>> SearchByCodigoOrNombreAsync(int idTenant, string texto, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.SearchByCodigoOrNombreAsync(idTenant, texto, soloActivos, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetQueManejanLoteAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetQueManejanLoteAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetPesablesAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetPesablesAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Prod>>> GetConControlVencimientoAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetConControlVencimientoAsync(idTenant, ct);
        return Result<IReadOnlyList<Prod>>.Success(list);
    }
}
