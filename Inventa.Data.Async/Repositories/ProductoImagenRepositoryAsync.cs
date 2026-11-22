using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

// S57.34: repo async-only (sin contraparte sync). Toda la persistencia de
// imágenes de producto vive aquí; Application.Async no toca DbContext.
public interface IProductoImagenRepositoryAsync
{
    Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, CancellationToken ct = default);
    // S104: imagen cuyo contenido está en el storage (fila de Archivo ya confirmada).
    Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, long? idArchivo, CancellationToken ct = default);
    // S104: imagen creada a partir de un archivo (confirmación repetida, idempotente).
    Task<ProductoImagen?> GetByIdArchivoAsync(long idArchivo, long idProd, CancellationToken ct = default);
    Task<IReadOnlyList<ProductoImagen>> GetActivesAsync(long idProd, CancellationToken ct = default);
    Task<ProductoImagen?> GetPrincipalAsync(long idProd, CancellationToken ct = default);
    Task<ProductoImagen?> GetByIdInProductAsync(int idProductoImagen, long idProd, CancellationToken ct = default);
    Task SetPrincipalAsync(long idProd, int idProductoImagen, CancellationToken ct = default);
    Task<IReadOnlyList<ProductoImagen>> ReorderAsync(long idProd, IReadOnlyList<int> orderedIds, CancellationToken ct = default);
    Task<(string Url, string NombreArchivo)> DeleteAsync(long idProd, int idProductoImagen, CancellationToken ct = default);
    // S57.34: diagnóstico/vista previa para ProductoServiceAsync (sin contraparte sync).
    Task<IReadOnlyList<long>> GetActiveProdIdsAsync(int idTenant, CancellationToken ct = default);
    Task<string?> GetPrincipalUrlAsync(long idProd, CancellationToken ct = default);
    // S57.83-G1: todas las Urls (solo lectura, para reporte de huérfanos).
    Task<IReadOnlyList<string>> GetAllUrlsAsync(CancellationToken ct = default);
    // S57.83-G3: reubicación de Url en toggle de visibilidad.
    Task UpdateUrlAsync(int idProductoImagen, string newUrl, CancellationToken ct = default);
    // S106: URLs de las variantes generadas ('listado' 400 px y 'ficha' 1200 px); null = sin generar.
    Task UpdateVariantesAsync(int idProductoImagen, string? urlMiniatura, string? urlFicha, CancellationToken ct = default);
}

public class ProductoImagenRepositoryAsync : IProductoImagenRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public ProductoImagenRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, CancellationToken ct = default) =>
        CreateAsync(idProd, url, nombreArchivo, contentType, tamanoBytes, null, ct);

    public async Task<ProductoImagen?> GetByIdArchivoAsync(long idArchivo, long idProd, CancellationToken ct = default)
    {
        if (idArchivo <= 0) throw new ArgumentOutOfRangeException(nameof(idArchivo));
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.ProductoImagenes.AsNoTracking()
            .FirstOrDefaultAsync(pi => pi.IdArchivo == idArchivo && pi.IdProd == idProd, ct);
    }

    public async Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, long? idArchivo, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (string.IsNullOrWhiteSpace(url)) throw new ArgumentException("Requerido", nameof(url));
        if (string.IsNullOrWhiteSpace(nombreArchivo)) throw new ArgumentException("Requerido", nameof(nombreArchivo));

        var count = await _ctx.ProductoImagenes.CountAsync(pi => pi.IdProd == idProd, ct);
        var entity = new ProductoImagen
        {
            IdProd = idProd,
            Url = url,
            NombreArchivo = nombreArchivo,
            ContentType = contentType,
            TamanoBytes = tamanoBytes,
            Orden = count,
            EsPrincipal = count == 0,
            Activo = true,
            IdArchivo = idArchivo
        };
        _ctx.ProductoImagenes.Add(entity);
        await _ctx.SaveChangesAsync(ct);
        return entity;
    }

    public async Task<IReadOnlyList<ProductoImagen>> GetActivesAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.ProductoImagenes
            .Where(pi => pi.IdProd == idProd && pi.Activo)
            .OrderBy(pi => pi.Orden)
            .ToListAsync(ct);
    }

    public async Task<ProductoImagen?> GetPrincipalAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.IdProd == idProd && pi.Activo)
            .OrderByDescending(pi => pi.EsPrincipal)
            .ThenBy(pi => pi.Orden)
            .FirstOrDefaultAsync(ct);
    }

    public async Task<ProductoImagen?> GetByIdInProductAsync(int idProductoImagen, long idProd, CancellationToken ct = default)
    {
        if (idProductoImagen <= 0) throw new ArgumentOutOfRangeException(nameof(idProductoImagen));
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.ProductoImagenes
            .FirstOrDefaultAsync(pi => pi.IdProductoImagen == idProductoImagen && pi.IdProd == idProd, ct);
    }

    public async Task SetPrincipalAsync(long idProd, int idProductoImagen, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idProductoImagen <= 0) throw new ArgumentOutOfRangeException(nameof(idProductoImagen));

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        var actives = await _ctx.ProductoImagenes.Where(pi => pi.IdProd == idProd && pi.Activo).ToListAsync(ct);
        foreach (var pi in actives) pi.EsPrincipal = false;
        var target = actives.FirstOrDefault(pi => pi.IdProductoImagen == idProductoImagen);
        if (target != null) target.EsPrincipal = true;
        await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task<IReadOnlyList<ProductoImagen>> ReorderAsync(long idProd, IReadOnlyList<int> orderedIds, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        ArgumentNullException.ThrowIfNull(orderedIds);

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        var actives = await _ctx.ProductoImagenes.Where(pi => pi.IdProd == idProd && pi.Activo).ToListAsync(ct);
        var map = actives.ToDictionary(a => a.IdProductoImagen);
        for (int i = 0; i < orderedIds.Count; i++)
            map[orderedIds[i]].Orden = i;
        await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return await _ctx.ProductoImagenes.Where(pi => pi.IdProd == idProd && pi.Activo).OrderBy(pi => pi.Orden).ToListAsync(ct);
    }

    public async Task<(string Url, string NombreArchivo)> DeleteAsync(long idProd, int idProductoImagen, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        if (idProductoImagen <= 0) throw new ArgumentOutOfRangeException(nameof(idProductoImagen));

        await using var tx = await _ctx.Database.BeginTransactionAsync(ct);
        var target = await _ctx.ProductoImagenes
            .FirstOrDefaultAsync(pi => pi.IdProductoImagen == idProductoImagen && pi.IdProd == idProd, ct);
        if (target == null) throw new InvalidOperationException("Imagen no encontrada");
        var wasPrincipal = target.EsPrincipal;
        var info = (target.Url, target.NombreArchivo);
        _ctx.ProductoImagenes.Remove(target);
        await _ctx.SaveChangesAsync(ct);
        if (wasPrincipal)
        {
            var next = await _ctx.ProductoImagenes.Where(pi => pi.IdProd == idProd).OrderBy(pi => pi.Orden).FirstOrDefaultAsync(ct);
            if (next != null)
            {
                next.EsPrincipal = true;
                await _ctx.SaveChangesAsync(ct);
            }
        }
        // Re-normalize orden gaps
        var remaining = await _ctx.ProductoImagenes.Where(pi => pi.IdProd == idProd).OrderBy(pi => pi.Orden).ToListAsync(ct);
        for (int i = 0; i < remaining.Count; i++) remaining[i].Orden = i;
        if (remaining.Count > 0) await _ctx.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
        return info;
    }

    public async Task<IReadOnlyList<long>> GetActiveProdIdsAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        return await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.Activo && pi.Prod.IdTenant == idTenant)
            .Select(pi => pi.IdProd).Distinct().ToListAsync(ct);
    }

    public async Task<string?> GetPrincipalUrlAsync(long idProd, CancellationToken ct = default)
    {
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));

        return await _ctx.ProductoImagenes.AsNoTracking()
            .Where(pi => pi.IdProd == idProd && pi.Activo)
            .OrderByDescending(pi => pi.EsPrincipal).ThenBy(pi => pi.Orden)
            .Select(pi => pi.Url).FirstOrDefaultAsync(ct);
    }

    public async Task<IReadOnlyList<string>> GetAllUrlsAsync(CancellationToken ct = default)
    {
        return await _ctx.ProductoImagenes.AsNoTracking()
            .Select(pi => pi.Url).ToListAsync(ct);
    }

    public async Task UpdateUrlAsync(int idProductoImagen, string newUrl, CancellationToken ct = default)
    {
        if (idProductoImagen <= 0) throw new ArgumentOutOfRangeException(nameof(idProductoImagen));
        if (string.IsNullOrWhiteSpace(newUrl)) throw new ArgumentException("Requerido", nameof(newUrl));
        var e = await _ctx.ProductoImagenes.FirstOrDefaultAsync(pi => pi.IdProductoImagen == idProductoImagen, ct);
        if (e == null) throw new InvalidOperationException("Imagen no encontrada");
        e.Url = newUrl;
        await _ctx.SaveChangesAsync(ct);
    }

    public async Task UpdateVariantesAsync(int idProductoImagen, string? urlMiniatura, string? urlFicha, CancellationToken ct = default)
    {
        if (idProductoImagen <= 0) throw new ArgumentOutOfRangeException(nameof(idProductoImagen));
        var e = await _ctx.ProductoImagenes.FirstOrDefaultAsync(pi => pi.IdProductoImagen == idProductoImagen, ct);
        if (e == null) throw new InvalidOperationException("Imagen no encontrada");
        e.UrlMiniatura = urlMiniatura;
        e.UrlFicha = urlFicha;
        await _ctx.SaveChangesAsync(ct);
    }
}
