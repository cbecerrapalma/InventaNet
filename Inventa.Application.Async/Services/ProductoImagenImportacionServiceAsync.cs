using System.Globalization;
using CBP.Results;
using CBP.Results.Errors;
using CBP.Storage;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Inventa.Application.Async.Services;

/// <summary>S107: producto a importar y sus URLs de imagen (se usan las 3 primeras distintas).</summary>
public sealed record ImagenImportacionItem(string? CodigoProducto, IReadOnlyList<string>? Urls);

/// <summary>
/// Resultado por producto. <c>Estado</c>: <c>importado</c> (al menos una imagen), <c>omitido</c> (ya tenía imágenes),
/// <c>no_encontrado</c> (el código no existe en el tenant) o <c>error</c> (ninguna imagen). <c>Error</c>: la primera causa.
/// </summary>
public sealed record ImagenImportacionResultado(string CodigoProducto, string Estado, int Imagenes, string? Error);

// S107 (P10): importa imágenes de producto desde URLs de hosts permitidos (Storage:Remote:AllowedHosts).
// Reanudable: un producto que ya tiene imágenes activas se omite. Cada imagen sigue el flujo de una subida directa
// (registro en Archivo, verificación de tamaño y firma, ProductoImagen y variantes de S106).
public interface IProductoImagenImportacionServiceAsync
{
    Task<Result<IReadOnlyList<ImagenImportacionResultado>>> ImportarAsync(int idTenant, IReadOnlyList<ImagenImportacionItem> items, CancellationToken ct = default);
}

public sealed class ProductoImagenImportacionServiceAsync : IProductoImagenImportacionServiceAsync
{
    public const int MaxProductosPorLote = 25;
    public const int MaxImagenesPorProducto = 3;

    private readonly IProdRepositoryAsync _prods;
    private readonly IProductoImagenRepositoryAsync _imagenes;
    private readonly IProductoImagenStorageServiceAsync _store;
    private readonly RemoteUploadService _remoto;
    private readonly ILogger<ProductoImagenImportacionServiceAsync> _log;

    public ProductoImagenImportacionServiceAsync(
        IProdRepositoryAsync prods,
        IProductoImagenRepositoryAsync imagenes,
        IProductoImagenStorageServiceAsync store,
        RemoteUploadService remoto,
        ILogger<ProductoImagenImportacionServiceAsync> log)
    {
        _prods = prods;
        _imagenes = imagenes;
        _store = store;
        _remoto = remoto;
        _log = log;
    }

    public async Task<Result<IReadOnlyList<ImagenImportacionResultado>>> ImportarAsync(int idTenant, IReadOnlyList<ImagenImportacionItem> items, CancellationToken ct = default)
    {
        if (items is null || items.Count == 0 || items.Count > MaxProductosPorLote)
            return Result<IReadOnlyList<ImagenImportacionResultado>>.Failure(
                new Error("IMAGENES_LOTE_INVALIDO", $"El lote debe traer entre 1 y {MaxProductosPorLote} productos.")
                    .WithType(ErrorType.Validation).WithHttpStatus(422));

        var resultados = new List<ImagenImportacionResultado>(items.Count);
        foreach (var item in items)
            resultados.Add(await ImportarProductoAsync(idTenant, item, ct));
        return Result<IReadOnlyList<ImagenImportacionResultado>>.Success(resultados);
    }

    private async Task<ImagenImportacionResultado> ImportarProductoAsync(int idTenant, ImagenImportacionItem item, CancellationToken ct)
    {
        var codigo = item.CodigoProducto?.Trim() ?? string.Empty;
        var prod = codigo.Length == 0 ? null : await _prods.GetByCodigoAsync(idTenant, codigo, ct);
        if (prod is null) return new ImagenImportacionResultado(codigo, "no_encontrado", 0, null);

        var actuales = await _imagenes.GetActivesAsync(prod.IdProd, ct);
        if (actuales.Count > 0) return new ImagenImportacionResultado(codigo, "omitido", 0, null);

        var urls = (item.Urls ?? [])
            .Where(u => !string.IsNullOrWhiteSpace(u))
            .Select(u => u.Trim())
            .Distinct(StringComparer.Ordinal)
            .Take(MaxImagenesPorProducto)
            .ToList();
        var creadas = 0;
        var primerError = urls.Count == 0 ? "Sin URLs." : null;
        foreach (var url in urls)
        {
            var error = await ImportarImagenAsync(prod, idTenant, url, ct);
            if (error is null) creadas++;
            else primerError ??= error;
        }

        return creadas > 0
            ? new ImagenImportacionResultado(codigo, "importado", creadas, primerError)
            : new ImagenImportacionResultado(codigo, "error", 0, primerError);
    }

    // null = imagen creada; si no, el motivo.
    private async Task<string?> ImportarImagenAsync(Prod prod, int idTenant, string url, CancellationToken ct)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var origen)) return "URL inválida.";

        // Mismo contenedor que una subida directa (ProductoImagenStorageServiceAsync.ContenedorDe).
        var contenedor = prod.Activo && prod.VisibleStore ? StorageContainers.Public : StorageContainers.Private;
        var subida = await _remoto.UploadFromUrlAsync(origen,
            new RemoteUploadIntent(Inv(idTenant), ProductoImagenStorageServiceAsync.App, ProductoImagenStorageServiceAsync.Entidad,
                Inv(prod.IdProd), contenedor, ProductoImagenUploadValidator.Policy),
            ct);
        if (subida.IsFailure)
        {
            _log.LogWarning("IMAGEN_IMPORTACION_FALLIDA: producto {IdProd} desde {Host}: {Code}", prod.IdProd, origen.Host, subida.Error!.Code);
            return subida.Error!.Message;
        }

        var idArchivo = long.Parse(subida.Value.UploadId, CultureInfo.InvariantCulture);
        var creada = await _store.ConfirmarSubidaAsync(prod.IdProd, idTenant, idArchivo, ct);
        return creada.IsSuccess ? null : creada.Error!.Message;
    }

    private static string Inv(long value) => value.ToString(CultureInfo.InvariantCulture);
}
