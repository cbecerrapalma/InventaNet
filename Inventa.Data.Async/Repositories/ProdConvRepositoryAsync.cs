using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IProdConvRepositoryAsync
{
    Task<IReadOnlyList<ProdConv>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default);
    Task<ProdConv?> GetAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<decimal?> GetFactorAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<bool> ExistsAsync(long idVarProd, int idUnidad, CancellationToken ct = default);
    Task<IReadOnlyList<ProdConv>> GetByUnidadAsync(int idUnidad, CancellationToken ct = default);
    Task<Result<decimal>> ConvertirCantidadAsync(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad, CancellationToken ct = default);
}

public class ProdConvRepositoryAsync : IProdConvRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public ProdConvRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<IReadOnlyList<ProdConv>> GetByVarianteAsync(long idVarProd, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));

        return await _ctx.Conversiones
            .AsNoTracking()
            .Where(c => c.IdVarProd == idVarProd)
            .Include(c => c.Unidad)
            .OrderBy(c => c.Unidad!.Nombre)
            .ToListAsync(ct);
    }

    public async Task<ProdConv?> GetAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Conversiones
            .AsNoTracking()
            .Include(c => c.Unidad)
            .FirstOrDefaultAsync(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad, ct);
    }

    public async Task<decimal?> GetFactorAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Conversiones
            .AsNoTracking()
            .Where(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad)
            .Select(c => (decimal?)c.Factor)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<bool> ExistsAsync(long idVarProd, int idUnidad, CancellationToken ct = default)
    {
        if (idVarProd <= 0) throw new ArgumentOutOfRangeException(nameof(idVarProd));
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Conversiones
            .AnyAsync(c => c.IdVarProd == idVarProd && c.IdUnidad == idUnidad, ct);
    }

    public async Task<IReadOnlyList<ProdConv>> GetByUnidadAsync(int idUnidad, CancellationToken ct = default)
    {
        if (idUnidad <= 0) throw new ArgumentOutOfRangeException(nameof(idUnidad));

        return await _ctx.Conversiones
            .AsNoTracking()
            .Where(c => c.IdUnidad == idUnidad)
            .Include(c => c.VarProd)
            .OrderBy(c => c.VarProd!.Nombre)
            .ToListAsync(ct);
    }

    public async Task<Result<decimal>> ConvertirCantidadAsync(long idVarProd, int idUnidadOrigen, int idUnidadDestino, decimal cantidad, CancellationToken ct = default)
    {
        if (idVarProd <= 0) return Result<decimal>.Failure(CommonErrors.Validation.OutOfRange(nameof(idVarProd), 1, long.MaxValue));
        if (idUnidadOrigen <= 0) return Result<decimal>.Failure(CommonErrors.Validation.OutOfRange(nameof(idUnidadOrigen), 1, int.MaxValue));
        if (idUnidadDestino <= 0) return Result<decimal>.Failure(CommonErrors.Validation.OutOfRange(nameof(idUnidadDestino), 1, int.MaxValue));
        if (cantidad <= 0m) return Result<decimal>.Failure(CommonErrors.Validation.OutOfRange(nameof(cantidad), 0.0001m, decimal.MaxValue));
        if (idUnidadOrigen == idUnidadDestino) return Result<decimal>.Success(cantidad);

        try
        {
            var factores = await _ctx.Conversiones
                .AsNoTracking()
                .Where(c => c.IdVarProd == idVarProd
                         && (c.IdUnidad == idUnidadOrigen || c.IdUnidad == idUnidadDestino))
                .ToDictionaryAsync(c => c.IdUnidad, c => c.Factor, ct);

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
                new Error("DATABASE_ERROR", $"Error de base de datos en ConvertirCantidad")
                    .WithType(ErrorType.Database).WithHttpStatus(503)
                    .WithDetail("IdVarProd", idVarProd).WithDetail("Cantidad", cantidad)
                    .WithDetail("ExceptionType", ex.GetType().Name).WithDetail("ExceptionMessage", ex.Message));
        }
    }
}



