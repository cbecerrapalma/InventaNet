using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IClienteRepository : IRepositorySync<Cliente>
{
    Result<IReadOnlyList<Cliente>> GetByTenant(int idTenant, bool soloActivos = true);
    Result<Cliente?> GetByCodigo(int idTenant, string codigo);
    Result<Cliente?> GetByRUT(int idTenant, string rut);

    /// <summary>
    /// TipoCliente: 1=Público General, 2=Frecuente, 3=Mayorista, 4=Corporativo.
    /// </summary>
    Result<IReadOnlyList<Cliente>> GetByTipoCliente(int idTenant, byte tipoCliente);

    Result<IReadOnlyList<Cliente>> SearchByNombre(int idTenant, string nombre);
    Result<bool> ExistsByCodigo(int idTenant, string codigo);
    Result<bool> ExistsByRUT(int idTenant, string rut);
    Result<int> CountByTipoCliente(int idTenant, byte tipoCliente);
}

public class ClienteRepository : RepositorySync<Cliente>, IClienteRepository
{
    // TipoCliente: 1=Público General, 2=Frecuente, 3=Mayorista, 4=Corporativo
    private const byte TipoClienteMin = 1;
    private const byte TipoClienteMax = 4;

    public ClienteRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Cliente>> GetByTenant(int idTenant, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Cliente>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            // IX_Cliente_Tenant (IdTenant, Activo)
            var query = DbSet.AsNoTracking().Where(c => c.IdTenant == idTenant);
            if (soloActivos) query = query.Where(c => c.Activo);
            return Result<IReadOnlyList<Cliente>>.Success(
                query.OrderBy(c => c.Nombre).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Cliente>>.Failure(
                CreateError(ex, nameof(GetByTenant)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<Cliente?> GetByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<Cliente?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Cliente?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            // UQ_Cliente (IdTenant, Codigo)
            return Result<Cliente?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(c => c.IdTenant == idTenant && c.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Cliente?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<Cliente?> GetByRUT(int idTenant, string rut)
    {
        if (Guard.Fail<Cliente?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<Cliente?>(Guard.StringRequerido(nameof(rut), rut)) is { } errStr) return errStr;
        try
        {
            return Result<Cliente?>.Success(
                DbSet.AsNoTracking().FirstOrDefault(c => c.IdTenant == idTenant && c.RUT == rut));
        }
        catch (Exception ex)
        {
            return Result<Cliente?>.Failure(
                CreateError(ex, nameof(GetByRUT))
                    .WithDetail("IdTenant", idTenant).WithDetail("RUT", rut));
        }
    }

    public Result<IReadOnlyList<Cliente>> GetByTipoCliente(int idTenant, byte tipoCliente)
    {
        if (Guard.Fail<IReadOnlyList<Cliente>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Cliente>>(Guard.EstadoInvalido(nameof(tipoCliente), tipoCliente, TipoClienteMin, TipoClienteMax)) is { } errTipo) return errTipo;
        try
        {
            return Result<IReadOnlyList<Cliente>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdTenant == idTenant && c.Activo && c.TipoCliente == tipoCliente)
                    .OrderBy(c => c.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Cliente>>.Failure(
                CreateError(ex, nameof(GetByTipoCliente))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("TipoCliente", tipoCliente));
        }
    }

    public Result<IReadOnlyList<Cliente>> SearchByNombre(int idTenant, string nombre)
    {
        if (Guard.Fail<IReadOnlyList<Cliente>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<IReadOnlyList<Cliente>>(Guard.StringRequerido(nameof(nombre), nombre)) is { } errStr) return errStr;
        try
        {
            return Result<IReadOnlyList<Cliente>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdTenant == idTenant && c.Activo
                             && EF.Functions.Like(c.Nombre, $"%{nombre}%"))
                    .OrderBy(c => c.Nombre)
                    .Take(50)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Cliente>>.Failure(
                CreateError(ex, nameof(SearchByNombre))
                    .WithDetail("IdTenant", idTenant).WithDetail("Nombre", nombre));
        }
    }

    public Result<bool> ExistsByCodigo(int idTenant, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(c => c.IdTenant == idTenant && c.Codigo == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant", idTenant).WithDetail("Codigo", codigo));
        }
    }

    public Result<bool> ExistsByRUT(int idTenant, string rut)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errId) return errId;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(rut), rut)) is { } errStr) return errStr;
        try
        {
            return Result<bool>.Success(
                DbSet.Any(c => c.IdTenant == idTenant && c.RUT == rut));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByRUT))
                    .WithDetail("IdTenant", idTenant).WithDetail("RUT", rut));
        }
    }

    public Result<int> CountByTipoCliente(int idTenant, byte tipoCliente)
    {
        if (Guard.Fail<int>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<int>(Guard.EstadoInvalido(nameof(tipoCliente), tipoCliente, TipoClienteMin, TipoClienteMax)) is { } errTipo) return errTipo;
        try
        {
            return Result<int>.Success(
                DbSet.Count(c => c.IdTenant == idTenant && c.Activo && c.TipoCliente == tipoCliente));
        }
        catch (Exception ex)
        {
            return Result<int>.Failure(
                CreateError(ex, nameof(CountByTipoCliente))
                    .WithDetail("IdTenant",    idTenant)
                    .WithDetail("TipoCliente", tipoCliente));
        }
    }
}
