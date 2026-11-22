using System.Globalization;
using CBP.Results;
using CBP.Storage;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

// S104: persistencia de la tabla Archivo. Implementa el puerto IUploadRegistry de CBP.Storage
// (flujo de subida directa) y las lecturas/cambios que necesita el servicio de imágenes.
// Toda lectura filtra por IdTenant; los cambios de estado actúan sobre la fila que esa lectura ya validó.
public interface IArchivoRepositoryAsync : IUploadRegistry
{
    Task<Archivo?> GetByIdAsync(long idArchivo, int idTenant, CancellationToken ct = default);
    Task<IReadOnlyList<Archivo>> GetByIdsAsync(IReadOnlyCollection<long> idsArchivo, int idTenant, CancellationToken ct = default);
    Task MarkDeletedAsync(long idArchivo, int idTenant, CancellationToken ct = default);
    Task UpdateContainerAsync(long idArchivo, int idTenant, string contenedor, CancellationToken ct = default);
    // Claves vivas (pendientes o confirmadas) de un contenedor y un tenant, para el reporte de huérfanos (solo lectura).
    Task<IReadOnlyList<string>> GetLiveKeysAsync(string contenedor, int idTenant, CancellationToken ct = default);

    // S105 (barrido, P9): proceso de sistema sin usuario, por eso recorre todos los tenants; nunca entrega filas a un cliente.
    // Purgables: del proveedor dado, pendientes creados antes de 'limite' o rechazados/eliminados cerrados antes de 'limite',
    // y sin ninguna ProductoImagen que los referencie.
    Task<IReadOnlyList<Archivo>> GetPurgablesAsync(string proveedor, DateTime limite, int max, CancellationToken ct = default);
    // Borra la fila solo si sigue en 'estado' (nunca un confirmado) y sin imagen que la referencie. true = se borró.
    Task<bool> DeletePurgedAsync(long idArchivo, byte estado, CancellationToken ct = default);
    // S132: confirmados de 'entidad' cerrados antes de 'limite' y sin ProductoImagen (la creación falló tras confirmar) pasan a Eliminado; devuelve cuántos.
    Task<int> CerrarConfirmadosSinImagenAsync(string entidad, DateTime limite, int max, CancellationToken ct = default);
    // Productos con alguna imagen activa cuyo archivo confirmado no está en el contenedor que pide su visibilidad.
    Task<IReadOnlyList<ArchivoDesubicado>> GetDesubicadosAsync(int max, CancellationToken ct = default);
    // S106: imágenes activas, con archivo confirmado y sin variante 'listado' generada (paso 3 del barrido).
    Task<IReadOnlyList<ImagenSinVariantes>> GetSinVariantesAsync(int max, CancellationToken ct = default);
}

/// <summary>S105: producto cuyas imágenes deben ir al contenedor público (<see cref="Publico"/>) o al privado.</summary>
public sealed record ArchivoDesubicado(long IdProd, int IdTenant, bool Publico);

/// <summary>S106: imagen de producto a la que le falta la variante de listado; el barrido la genera.</summary>
public sealed record ImagenSinVariantes(int IdProductoImagen, long IdProd, int IdTenant);

public class ArchivoRepositoryAsync : IArchivoRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public ArchivoRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<Result<string>> RegisterPendingAsync(PendingUpload upload, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(upload);
        if (!TryParseTenant(upload.Tenant, out var idTenant) || !long.TryParse(upload.EntityId, NumberStyles.None, CultureInfo.InvariantCulture, out var idEntidad))
            return Result<string>.Failure(StorageErrors.InvalidKey("el tenant y el id de la entidad deben ser numéricos."));

        var entity = new Archivo
        {
            IdTenant = idTenant,
            Proveedor = upload.Provider,
            Contenedor = upload.Object.Container,
            Clave = upload.Object.Key,
            Entidad = upload.Entity,
            IdEntidad = idEntidad,
            NombreArchivo = upload.FileName,
            ContentType = upload.ContentType,
            TamanoBytes = upload.ContentLength,
            Estado = ArchivoEstado.Pendiente
        };
        _ctx.Archivos.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return Result<string>.Success(entity.IdArchivo.ToString(CultureInfo.InvariantCulture));
    }

    public async Task<Result<UploadRecord?>> GetAsync(string uploadId, string tenant, CancellationToken ct = default)
    {
        if (!TryParseId(uploadId, out var idArchivo) || !TryParseTenant(tenant, out var idTenant))
            return Result<UploadRecord?>.Success(null, allowNull: true);

        var a = await _ctx.Archivos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdArchivo == idArchivo && x.IdTenant == idTenant, ct);
        if (a is null) return Result<UploadRecord?>.Success(null, allowNull: true);

        var record = new UploadRecord(
            a.IdArchivo.ToString(CultureInfo.InvariantCulture),
            a.IdTenant.ToString(CultureInfo.InvariantCulture),
            new StorageObjectRef(a.Contenedor, a.Clave),
            a.Entidad,
            a.IdEntidad.ToString(CultureInfo.InvariantCulture),
            a.NombreArchivo,
            a.ContentType,
            a.TamanoBytes,
            (UploadState)a.Estado,
            a.ETag);
        return Result<UploadRecord?>.Success(record);
    }

    public async Task<Result> MarkConfirmedAsync(string uploadId, StorageObjectInfo info, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(info);
        var a = await FindPendingAsync(uploadId, ct);
        if (a is null) return Result.Failure(StorageErrors.UploadNotFound());
        a.Estado = ArchivoEstado.Confirmado;
        a.TamanoBytes = info.Size;
        a.ETag = info.ETag;
        a.FecEstado = DateTime.Now;
        await _ctx.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result> MarkRejectedAsync(string uploadId, CancellationToken ct = default)
    {
        var a = await FindPendingAsync(uploadId, ct);
        if (a is null) return Result.Failure(StorageErrors.UploadNotFound());
        a.Estado = ArchivoEstado.Rechazado;
        a.FecEstado = DateTime.Now;
        await _ctx.SaveChangesAsync(ct);
        return Result.Success();
    }

    public async Task<Result<long>> GetTenantUsageAsync(string tenant, CancellationToken ct = default)
    {
        if (!TryParseTenant(tenant, out var idTenant)) return Result<long>.Success(0);
        var bytes = await _ctx.Archivos.AsNoTracking()
            .Where(x => x.IdTenant == idTenant && (x.Estado == ArchivoEstado.Pendiente || x.Estado == ArchivoEstado.Confirmado))
            .SumAsync(x => (long?)x.TamanoBytes, ct);
        return Result<long>.Success(bytes ?? 0);
    }

    public async Task<Archivo?> GetByIdAsync(long idArchivo, int idTenant, CancellationToken ct = default)
    {
        return await _ctx.Archivos.AsNoTracking()
            .FirstOrDefaultAsync(x => x.IdArchivo == idArchivo && x.IdTenant == idTenant, ct);
    }

    public async Task<IReadOnlyList<Archivo>> GetByIdsAsync(IReadOnlyCollection<long> idsArchivo, int idTenant, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsArchivo);
        if (idsArchivo.Count == 0) return [];
        return await _ctx.Archivos.AsNoTracking()
            .Where(x => x.IdTenant == idTenant && idsArchivo.Contains(x.IdArchivo))
            .ToListAsync(ct);
    }

    public async Task MarkDeletedAsync(long idArchivo, int idTenant, CancellationToken ct = default)
    {
        var a = await _ctx.Archivos.FirstOrDefaultAsync(x => x.IdArchivo == idArchivo && x.IdTenant == idTenant, ct);
        if (a is null) return;
        a.Estado = ArchivoEstado.Eliminado;
        a.FecEstado = DateTime.Now;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateContainerAsync(long idArchivo, int idTenant, string contenedor, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(contenedor)) throw new ArgumentException("Requerido", nameof(contenedor));
        var a = await _ctx.Archivos.FirstOrDefaultAsync(x => x.IdArchivo == idArchivo && x.IdTenant == idTenant, ct);
        if (a is null) throw new InvalidOperationException("Archivo no encontrado");
        a.Contenedor = contenedor;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetLiveKeysAsync(string contenedor, int idTenant, CancellationToken ct = default)
    {
        return await _ctx.Archivos.AsNoTracking()
            .Where(x => x.IdTenant == idTenant && x.Contenedor == contenedor && (x.Estado == ArchivoEstado.Pendiente || x.Estado == ArchivoEstado.Confirmado))
            .Select(x => x.Clave)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Archivo>> GetPurgablesAsync(string proveedor, DateTime limite, int max, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(proveedor)) throw new ArgumentException("Requerido", nameof(proveedor));
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        return await _ctx.Archivos.AsNoTracking()
            .Where(x => x.Proveedor == proveedor
                && ((x.Estado == ArchivoEstado.Pendiente && x.FecCrea < limite)
                    || ((x.Estado == ArchivoEstado.Rechazado || x.Estado == ArchivoEstado.Eliminado) && (x.FecEstado ?? x.FecCrea) < limite))
                && !_ctx.ProductoImagenes.Any(p => p.IdArchivo == x.IdArchivo))
            .OrderBy(x => x.IdArchivo)
            .Take(max)
            .ToListAsync(ct);
    }

    public async Task<bool> DeletePurgedAsync(long idArchivo, byte estado, CancellationToken ct = default)
    {
        if (estado == ArchivoEstado.Confirmado) return false;
        var borradas = await _ctx.Archivos
            .Where(x => x.IdArchivo == idArchivo && x.Estado == estado && !_ctx.ProductoImagenes.Any(p => p.IdArchivo == x.IdArchivo))
            .ExecuteDeleteAsync(ct);
        return borradas > 0;
    }

    public async Task<int> CerrarConfirmadosSinImagenAsync(string entidad, DateTime limite, int max, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(entidad)) throw new ArgumentException("Requerido", nameof(entidad));
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        var ids = await _ctx.Archivos.AsNoTracking()
            .Where(x => x.Estado == ArchivoEstado.Confirmado && x.Entidad == entidad
                && (x.FecEstado ?? x.FecCrea) < limite
                && !_ctx.ProductoImagenes.Any(p => p.IdArchivo == x.IdArchivo))
            .OrderBy(x => x.IdArchivo)
            .Select(x => x.IdArchivo)
            .Take(max)
            .ToListAsync(ct);
        if (ids.Count == 0) return 0;
        // Se repiten las guardas: entre la lectura y el cambio puede haberse creado la imagen.
        return await _ctx.Archivos
            .Where(x => ids.Contains(x.IdArchivo) && x.Estado == ArchivoEstado.Confirmado
                && !_ctx.ProductoImagenes.Any(p => p.IdArchivo == x.IdArchivo))
            .ExecuteUpdateAsync(s => s
                .SetProperty(x => x.Estado, ArchivoEstado.Eliminado)
                .SetProperty(x => x.FecEstado, (DateTime?)DateTime.Now), ct);
    }

    public async Task<IReadOnlyList<ArchivoDesubicado>> GetDesubicadosAsync(int max, CancellationToken ct = default)
    {
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        var query =
            from pi in _ctx.ProductoImagenes.AsNoTracking()
            join a in _ctx.Archivos.AsNoTracking() on pi.IdArchivo equals (long?)a.IdArchivo
            join p in _ctx.Productos.AsNoTracking() on pi.IdProd equals p.IdProd
            where pi.Activo && a.Estado == ArchivoEstado.Confirmado && a.IdTenant == p.IdTenant
                && a.Contenedor != (p.Activo && p.VisibleStore ? StorageContainers.Public : StorageContainers.Private)
            select new { p.IdProd, p.IdTenant, Publico = p.Activo && p.VisibleStore };
        var filas = await query.Distinct().OrderBy(x => x.IdProd).Take(max).ToListAsync(ct);
        return filas.Select(x => new ArchivoDesubicado(x.IdProd, x.IdTenant, x.Publico)).ToList();
    }

    public async Task<IReadOnlyList<ImagenSinVariantes>> GetSinVariantesAsync(int max, CancellationToken ct = default)
    {
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        // S106: se usan las navegaciones (Archivo/Prod) en vez de joins con claves anulables, que EF no traduce.
        return await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.Activo && pi.UrlMiniatura == null
                && pi.Archivo != null && pi.Archivo.Estado == ArchivoEstado.Confirmado
                && pi.Archivo.IdTenant == pi.Prod.IdTenant)
            .OrderBy(pi => pi.IdProductoImagen)
            .Take(max)
            .Select(pi => new ImagenSinVariantes(pi.IdProductoImagen, pi.IdProd, pi.Archivo!.IdTenant))
            .ToListAsync(ct);
    }

    private async Task<Archivo?> FindPendingAsync(string uploadId, CancellationToken ct)
    {
        if (!TryParseId(uploadId, out var idArchivo)) return null;
        return await _ctx.Archivos.FirstOrDefaultAsync(x => x.IdArchivo == idArchivo && x.Estado == ArchivoEstado.Pendiente, ct);
    }

    private static bool TryParseId(string? value, out long id) =>
        long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;

    private static bool TryParseTenant(string? value, out int idTenant) =>
        int.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out idTenant) && idTenant > 0;
}
