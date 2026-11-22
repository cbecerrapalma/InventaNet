using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface IProdConvRepository : IRepositorySync<ProdConv>
{
    Result<IReadOnlyList<ProdConv>> GetByVariante(long idVarProd);
    Result<ProdConv?> Get(long idVarProd, int idUnidad);
    Result<decimal?> GetFactor(long idVarProd, int idUnidad);
    Result<bool> Exists(long idVarProd, int idUnidad);
    Result<IReadOnlyList<ProdConv>> GetByUnidad(int idUnidad);
    Result<decimal> ConvertirCantidad(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad);
}

public class ProdConvRepository : RepositorySync<ProdConv>, IProdConvRepository
{
    public ProdConvRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<ProdConv>> GetByVariante(long idVarProd)
    {
        if (Guard.Fail<IReadOnlyList<ProdConv>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<ProdConv>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdVarProd == idVarProd)
                    .Include(c => c.Unidad)
                    .OrderBy(c => c.Unidad.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProdConv>>.Failure(
                CreateError(ex, nameof(GetByVariante)).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<ProdConv?> Get(long idVarProd, int idUnidad)
    {
        if (Guard.Fail<ProdConv?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<ProdConv?>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } errU) return errU;
        try
        {
            return Result<ProdConv?>.Success(
                DbSet.AsNoTracking()
                    .Include(c => c.Unidad)
                    .FirstOrDefault(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad));
        }
        catch (Exception ex)
        {
            return Result<ProdConv?>.Failure(
                CreateError(ex, nameof(Get))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdUnidad", idUnidad));
        }
    }

    public Result<decimal?> GetFactor(long idVarProd, int idUnidad)
    {
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<decimal?>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } errU) return errU;
        try
        {
            return Result<decimal?>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad)
                    .Select(c => (decimal?)c.Factor)
                    .FirstOrDefault());
        }
        catch (Exception ex)
        {
            return Result<decimal?>.Failure(
                CreateError(ex, nameof(GetFactor))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdUnidad", idUnidad));
        }
    }

    public Result<bool> Exists(long idVarProd, int idUnidad)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } errU) return errU;
        try
        {
            return Result<bool>.Success(DbSet.Any(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(Exists))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("IdUnidad", idUnidad));
        }
    }

    public Result<IReadOnlyList<ProdConv>> GetByUnidad(int idUnidad)
    {
        if (Guard.Fail<IReadOnlyList<ProdConv>>(Guard.IdInvalido(nameof(idUnidad), idUnidad)) is { } err) return err;
        try
        {
            return Result<IReadOnlyList<ProdConv>>.Success(
                DbSet.AsNoTracking()
                    .Where(c => c.IdUnidad == idUnidad)
                    .Include(c => c.VarProd)
                    .OrderBy(c => c.VarProd.Nombre)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<ProdConv>>.Failure(
                CreateError(ex, nameof(GetByUnidad)).WithDetail("IdUnidad", idUnidad));
        }
    }

    public Result<decimal> ConvertirCantidad(
        long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad)
    {
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idUnidadOrigen), idUnidadOrigen)) is { } errO) return errO;
        if (Guard.Fail<decimal>(Guard.IdInvalido(nameof(idUnidadDestino), idUnidadDestino)) is { } errD) return errD;
        if (Guard.Fail<decimal>(Guard.Rango(nameof(cantidad), cantidad, 0.0001m, decimal.MaxValue)) is { } errC) return errC;
        if (idUnidadOrigen == idUnidadDestino) return Result<decimal>.Success(cantidad);
        try
        {
            var factores = DbSet.AsNoTracking()
                .Where(c => c.IdVarProd == idVarProd
                         && (c.IdUnidad == idUnidadOrigen || c.IdUnidad == idUnidadDestino))
                .ToDictionary(c => c.IdUnidad, c => c.Factor);

            if (!factores.TryGetValue(idUnidadOrigen, out var factorOrigen))
                return Result<decimal>.Failure(
                    new Error("CONVERSION_NOT_FOUND",
                        $"No existe conversión para variante {idVarProd}, unidad origen {idUnidadOrigen}")
                        .WithType(ErrorType.NotFound).WithHttpStatus(404));

            if (!factores.TryGetValue(idUnidadDestino, out var factorDestino))
                return Result<decimal>.Failure(
                    new Error("CONVERSION_NOT_FOUND",
                        $"No existe conversión para variante {idVarProd}, unidad destino {idUnidadDestino}")
                        .WithType(ErrorType.NotFound).WithHttpStatus(404));

            if (factorDestino == 0)
                return Result<decimal>.Failure(
                    new Error("CONVERSION_ZERO_FACTOR", "El factor de conversión destino no puede ser cero")
                        .WithType(ErrorType.Validation).WithHttpStatus(400));

            return Result<decimal>.Success(cantidad * factorOrigen / factorDestino);
        }
        catch (Exception ex)
        {
            return Result<decimal>.Failure(
                CreateError(ex, nameof(ConvertirCantidad))
                    .WithDetail("IdVarProd", idVarProd).WithDetail("Cantidad", cantidad));
        }
    }
}
