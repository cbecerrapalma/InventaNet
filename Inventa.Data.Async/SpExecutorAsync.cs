using CBP.Results;
using Inventa.Data.Context;
using Inventa.Data.Repositories;
using Inventa.Data.Repositories.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace Inventa.Data.Async;

/// <summary>
/// Executor async real (no sync-over-async). Reutiliza SpContracts de Inventa.Data.
/// </summary>
public static class SpExecutorAsync
{
    public static Task<Result<RegistrarVentaResult>> RegistrarVentaAsync(
        InventaDbContext ctx, RegistrarVentaRequest req, CancellationToken ct = default)
        => SpExecutor.RegistrarVentaAsync(ctx, req, ct);

    public static Task<Result> AnularVentaAsync(
        InventaDbContext ctx, AnularVentaRequest req, CancellationToken ct = default)
        => SpExecutor.AnularVentaAsync(ctx, req, ct);

    public static Task<Result> RecibirOrdenCompraAsync(
        InventaDbContext ctx, RecibirOrdenCompraRequest req, CancellationToken ct = default)
        => SpExecutor.RecibirOrdenCompraAsync(ctx, req, ct);

    public static Task<Result> AjustarInventarioAsync(
        InventaDbContext ctx, AjustarInventarioRequest req, CancellationToken ct = default)
        => SpExecutor.AjustarInventarioAsync(ctx, req, ct);

    public static Task<Result> TransferirEntresBodegasAsync(
        InventaDbContext ctx, TransferirEntresBodegasRequest req, CancellationToken ct = default)
        => SpExecutor.TransferirEntresBodegasAsync(ctx, req, ct);
}
