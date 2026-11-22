using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

public interface IImportacionLoteRepositoryAsync
{
    Task<ImportacionLote> CreateAsync(ImportacionLote entity, CancellationToken ct = default);
    Task UpdateRutaAsync(long id, int idTenant, string rutaArchivo, CancellationToken ct = default);
    Task<ImportacionLote?> GetByIdInTenantAsync(long id, int idTenant, CancellationToken ct = default);
    Task<bool> ExistsAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default);
    /// <summary>S91.4: último lote APLICADO del tenant con el mismo SHA-256 (para citarlo en los avisos).</summary>
    Task<ImportacionLote?> GetUltimoAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default);
    Task<(IReadOnlyList<ImportacionLote> Items, int Total)> SearchPagedByTenantAsync(int idTenant, int page, int pageSize, CancellationToken ct = default);
    Task UpdateEstadoAsync(long id, int idTenant, string estado, CancellationToken ct = default);
    Task UpdateResultadoAsync(long id, int idTenant, string estado, string resumenJson, CancellationToken ct = default);
}

public class ImportacionLoteRepositoryAsync : IImportacionLoteRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public ImportacionLoteRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<ImportacionLote> CreateAsync(ImportacionLote entity, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(entity);
        if (entity.IdTenant <= 0) throw new ArgumentOutOfRangeException(nameof(entity.IdTenant));
        _ctx.ImportacionLotes.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task UpdateRutaAsync(long id, int idTenant, string rutaArchivo, CancellationToken ct = default)
    {
        var lote = await _ctx.ImportacionLotes
            .FirstOrDefaultAsync(l => l.IdImportacionLote == id && l.IdTenant == idTenant, ct);
        if (lote is null) throw new InvalidOperationException("Lote no encontrado en el tenant.");
        lote.RutaArchivo = rutaArchivo;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<ImportacionLote?> GetByIdInTenantAsync(long id, int idTenant, CancellationToken ct = default)
    {
        if (id <= 0) throw new ArgumentOutOfRangeException(nameof(id));
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.ImportacionLotes
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.IdImportacionLote == id && l.IdTenant == idTenant, ct);
    }

    public async Task<bool> ExistsAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.ImportacionLotes
            .AsNoTracking()
            .AnyAsync(l => l.IdTenant == idTenant
                && l.Tipo == tipo
                && l.Sha256 == sha256
                && l.Estado == ImportacionLoteEstados.Aplicado, ct);
    }

    public async Task<ImportacionLote?> GetUltimoAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.ImportacionLotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant
                && l.Tipo == tipo
                && l.Sha256 == sha256
                && l.Estado == ImportacionLoteEstados.Aplicado)
            .OrderByDescending(l => l.FecAplicada).ThenByDescending(l => l.IdImportacionLote)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<(IReadOnlyList<ImportacionLote> Items, int Total)> SearchPagedByTenantAsync(int idTenant, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _ctx.ImportacionLotes
            .AsNoTracking()
            .Where(l => l.IdTenant == idTenant);

        var total = await query.CountAsync(ct);
        var items = await query
            .OrderByDescending(l => l.FecCrea)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);
        return (items, total);
    }

    public async Task UpdateEstadoAsync(long id, int idTenant, string estado, CancellationToken ct = default)
    {
        var lote = await _ctx.ImportacionLotes
            .FirstOrDefaultAsync(l => l.IdImportacionLote == id && l.IdTenant == idTenant, ct);
        if (lote is null) throw new InvalidOperationException("Lote no encontrado en el tenant.");
        lote.Estado = estado;
        if (estado == ImportacionLoteEstados.Aplicado)
            lote.FecAplicada = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateResultadoAsync(long id, int idTenant, string estado, string resumenJson, CancellationToken ct = default)
    {
        var lote = await _ctx.ImportacionLotes
            .FirstOrDefaultAsync(l => l.IdImportacionLote == id && l.IdTenant == idTenant, ct);
        if (lote is null) throw new InvalidOperationException("Lote no encontrado en el tenant.");
        lote.Estado = estado;
        lote.ResumenJson = resumenJson;
        if (estado == ImportacionLoteEstados.Aplicado)
            lote.FecAplicada = DateTime.UtcNow;
        await _ctx.SaveChangesAsync(ct);
    }
}
