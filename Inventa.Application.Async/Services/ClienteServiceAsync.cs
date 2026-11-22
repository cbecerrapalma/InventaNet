using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IClienteServiceAsync
{
    Task<Result<IReadOnlyList<Cliente>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Cliente?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<Cliente?>> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Cliente>>> GetByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Cliente>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default);
    Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default);
    Task<Result<bool>> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default);
    Task<Result<int>> CountByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default);
    Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default);
    Task<Result<Cliente?>> GetByIdAsync(int idCliente, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Cliente>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<Cliente> Items, int Total)>> SearchPagedAsync(int idTenant, string texto, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Cliente?>> GetByIdInTenantAsync(int idCliente, int idTenant, CancellationToken ct = default);
    Task<Result<(Cliente? Cliente, bool Duplicado)>> CreateAsync(Cliente entity, CancellationToken ct = default);
    Task<Result<(Cliente? Cliente, bool Duplicado)>> UpdateAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default);
}

public sealed class ClienteServiceAsync : IClienteServiceAsync
{
    private readonly IClienteRepositoryAsync _repo;
    public ClienteServiceAsync(IClienteRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<Cliente>>> GetByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var list = await _repo.GetByTenantAsync(idTenant, soloActivos, ct);
        return Result<IReadOnlyList<Cliente>>.Success(list);
    }

    public async Task<Result<Cliente?>> GetByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var e = await _repo.GetByCodigoAsync(idTenant, codigo, ct);
        return Result<Cliente?>.Success(e);
    }

    public async Task<Result<Cliente?>> GetByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        var e = await _repo.GetByRUTAsync(idTenant, rut, ct);
        return Result<Cliente?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Cliente>>> GetByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default)
    {
        var list = await _repo.GetByTipoClienteAsync(idTenant, tipoCliente, ct);
        return Result<IReadOnlyList<Cliente>>.Success(list);
    }

    public async Task<Result<IReadOnlyList<Cliente>>> SearchByNombreAsync(int idTenant, string nombre, CancellationToken ct = default)
    {
        var list = await _repo.SearchByNombreAsync(idTenant, nombre, ct);
        return Result<IReadOnlyList<Cliente>>.Success(list);
    }

    public async Task<Result<bool>> ExistsByCodigoAsync(int idTenant, string codigo, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByCodigoAsync(idTenant, codigo, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<bool>> ExistsByRUTAsync(int idTenant, string rut, CancellationToken ct = default)
    {
        var ok = await _repo.ExistsByRUTAsync(idTenant, rut, ct);
        return Result<bool>.Success(ok);
    }

    public async Task<Result<int>> CountByTipoClienteAsync(int idTenant, byte tipoCliente, CancellationToken ct = default)
    {
        var c = await _repo.CountByTipoClienteAsync(idTenant, tipoCliente, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<int>> CountByTenantAsync(int idTenant, bool soloActivos = true, CancellationToken ct = default)
    {
        var c = await _repo.CountByTenantAsync(idTenant, soloActivos, ct);
        return Result<int>.Success(c);
    }

    public async Task<Result<Cliente?>> GetByIdAsync(int idCliente, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idCliente, ct);
        return Result<Cliente?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Cliente>>> GetRecentAsync(int idTenant, int take, CancellationToken ct = default)
    {
        var list = await _repo.GetRecentAsync(idTenant, take, ct);
        return Result<IReadOnlyList<Cliente>>.Success(list);
    }

    public async Task<Result<(IReadOnlyList<Cliente> Items, int Total)>> SearchPagedAsync(int idTenant, string texto, int page, int pageSize, CancellationToken ct = default)
    {
        var result = await _repo.SearchPagedAsync(idTenant, texto, null, page, pageSize, ct);
        return Result<(IReadOnlyList<Cliente> Items, int Total)>.Success(result);
    }

    public async Task<Result<Cliente?>> GetByIdInTenantAsync(int idCliente, int idTenant, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInTenantAsync(idCliente, idTenant, ct);
        return Result<Cliente?>.Success(e);
    }

    public async Task<Result<(Cliente? Cliente, bool Duplicado)>> CreateAsync(Cliente entity, CancellationToken ct = default)
    {
        var result = await _repo.CreateAsync(entity, ct);
        return Result<(Cliente? Cliente, bool Duplicado)>.Success(result);
    }

    public async Task<Result<(Cliente? Cliente, bool Duplicado)>> UpdateAsync(int idCliente, int idTenant, string codigo, string nombre, string? rut, CancellationToken ct = default)
    {
        var result = await _repo.UpdateAsync(idCliente, idTenant, codigo, nombre, rut, ct);
        return Result<(Cliente? Cliente, bool Duplicado)>.Success(result);
    }
}
