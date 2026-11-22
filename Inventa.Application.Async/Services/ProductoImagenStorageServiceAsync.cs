using System.Globalization;
using CBP.Results;
using CBP.Results.Errors;
using CBP.Storage;
using CBP.Storage.Imaging;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging;

namespace Inventa.Application.Async.Services;

/// <summary>
/// Autorización de una subida directa: el navegador envía los bytes a <see cref="Url"/> con <see cref="Metodo"/> y
/// <see cref="Headers"/> (sin pasar por la API) y después confirma <see cref="IdArchivo"/>.
/// </summary>
public sealed record ProductoImagenSubida(long IdArchivo, string Url, string Metodo, IReadOnlyDictionary<string, string> Headers, DateTimeOffset Expira);

// S57.83 / S104: composición BD + storage de las imágenes de producto (scoping de tenant y contenedor por visibilidad).
// Servicio dedicado fuera del par fachada (S57.35 §4 exige 1:1 ProductoImagenServiceAsync ↔ repositorio por nombre).
// Desde S104 la API no recibe ni entrega el contenido: autoriza y firma (CBP.Storage), y confirma leyendo metadatos.
public interface IProductoImagenStorageServiceAsync
{
    /// <summary>Autoriza y firma la subida. Producto de otro tenant: 404 sin registrar ni firmar.</summary>
    Task<Result<ProductoImagenSubida>> IniciarSubidaAsync(long idProd, int idTenant, string? nombreArchivo, string? contentType, long tamanoBytes, CancellationToken ct = default);
    /// <summary>Verifica el objeto subido y crea la imagen. Repetir la confirmación devuelve la misma imagen.</summary>
    Task<Result<ProductoImagen>> ConfirmarSubidaAsync(long idProd, int idTenant, long idArchivo, CancellationToken ct = default);
    /// <summary>Imágenes activas con <c>Url</c> lista para el cliente: la pública, o una firmada de descarga.</summary>
    Task<Result<IReadOnlyList<ProductoImagen>>> GetActivesAsync(long idProd, int idTenant, CancellationToken ct = default);
    /// <summary>Elimina la imagen, su objeto del storage (best-effort) y marca el archivo como eliminado.</summary>
    Task<Result> DeleteAsync(long idProd, int idTenant, int idProductoImagen, CancellationToken ct = default);
    // S57.83-G1 / S105: reporte solo-lectura de huérfanos del tenant que consulta (nunca borra).
    Task<Result<IReadOnlyList<string>>> FindOrphanFilesAsync(int idTenant, CancellationToken ct = default);
    // S57.83-G3: mueve los objetos al contenedor acorde a la visibilidad (idempotente).
    Task<Result<int>> RelocateAsync(long idProd, int idTenant, bool makePublic, CancellationToken ct = default);
    /// <summary>S106: genera las variantes de una imagen ya creada (paso 3 del barrido); no cambia la imagen si falla.</summary>
    Task<Result> GenerarVariantesAsync(int idProductoImagen, long idProd, int idTenant, CancellationToken ct = default);
}

public sealed class ProductoImagenStorageServiceAsync : IProductoImagenStorageServiceAsync
{
    public const string App = "inventa";
    public const string Entidad = "producto";

    /// <summary>
    /// Prefijo de <see cref="ProductoImagen.Url"/> cuando el objeto no tiene URL pública (<c>private:{clave}</c>).
    /// Es un marcador interno: nunca sale de la API.
    /// </summary>
    public const string MarcadorPrivado = "private:";

    /// <summary>Caducidad de la URL firmada de descarga de una imagen privada.</summary>
    public static readonly TimeSpan DescargaTtl = TimeSpan.FromMinutes(5);

    private const int PaginaListado = 500;

    private readonly IProductoImagenRepositoryAsync _repo;
    private readonly IProdRepositoryAsync _prods;
    private readonly IArchivoRepositoryAsync _archivos;
    private readonly DirectUploadService _subidas;
    private readonly IStorageProvider _storage;
    private readonly ImageVariantService _variantes;
    private readonly ILogger<ProductoImagenStorageServiceAsync> _log;

    public ProductoImagenStorageServiceAsync(
        IProductoImagenRepositoryAsync repo,
        IProdRepositoryAsync prods,
        IArchivoRepositoryAsync archivos,
        DirectUploadService subidas,
        IStorageProvider storage,
        ImageVariantService variantes,
        ILogger<ProductoImagenStorageServiceAsync> log)
    {
        _repo = repo;
        _prods = prods;
        _archivos = archivos;
        _subidas = subidas;
        _storage = storage;
        _variantes = variantes;
        _log = log;
    }

    public async Task<Result<ProductoImagenSubida>> IniciarSubidaAsync(long idProd, int idTenant, string? nombreArchivo, string? contentType, long tamanoBytes, CancellationToken ct = default)
    {
        // S57.83-G3: scoping de tenant antes que nada. Producto ajeno => 404 (sin fuga, sin registrar ni firmar).
        var prod = await _prods.GetByIdInTenantAsync(idProd, idTenant, ct);
        if (prod is null) return Result<ProductoImagenSubida>.Failure(ProductoNoEncontrado());

        var invalid = ProductoImagenUploadValidator.ValidateDeclared(nombreArchivo, contentType, tamanoBytes);
        if (invalid is not null) return Result<ProductoImagenSubida>.Failure(invalid);

        var started = await _subidas.StartAsync(
            new UploadIntent(Inv(idTenant), App, Entidad, Inv(idProd), nombreArchivo!, contentType!, tamanoBytes,
                ContenedorDe(prod), ProductoImagenUploadValidator.Policy),
            ct);
        if (started.IsFailure) return Result<ProductoImagenSubida>.Failure(started.Error!);

        var ticket = started.Value.Ticket;
        return Result<ProductoImagenSubida>.Success(new ProductoImagenSubida(
            long.Parse(started.Value.UploadId, CultureInfo.InvariantCulture), ticket.Url, ticket.Method, ticket.Headers, ticket.ExpiresAt));
    }

    public async Task<Result<ProductoImagen>> ConfirmarSubidaAsync(long idProd, int idTenant, long idArchivo, CancellationToken ct = default)
    {
        var prod = await _prods.GetByIdInTenantAsync(idProd, idTenant, ct);
        if (prod is null) return Result<ProductoImagen>.Failure(ProductoNoEncontrado());

        // La subida debe ser de este tenant y de este producto; si no, no se distingue de una inexistente.
        var archivo = await _archivos.GetByIdAsync(idArchivo, idTenant, ct);
        if (archivo is null || archivo.Entidad != Entidad || archivo.IdEntidad != idProd)
            return Result<ProductoImagen>.Failure(StorageErrors.UploadNotFound());

        var confirmed = await _subidas.ConfirmAsync(Inv(idArchivo), Inv(idTenant), ProductoImagenUploadValidator.Policy, ct);
        if (confirmed.IsFailure) return Result<ProductoImagen>.Failure(confirmed.Error!);

        var subida = confirmed.Value.Upload;
        var obj = subida.Object;
        if (confirmed.Value.AlreadyConfirmed)
        {
            var existente = await _repo.GetByIdArchivoAsync(idArchivo, idProd, ct);
            if (existente is not null) return Result<ProductoImagen>.Success(await VistaAsync(existente, obj, ct));
        }

        // La visibilidad pudo cambiar entre iniciar y confirmar: el objeto queda en el contenedor que corresponde hoy.
        var destino = ContenedorDe(prod);
        if (!string.Equals(obj.Container, destino, StringComparison.OrdinalIgnoreCase))
        {
            var movido = await MoverAsync(idArchivo, idTenant, obj, destino, ct);
            if (movido.IsFailure) return Result<ProductoImagen>.Failure(movido.Error!);
            obj = movido.Value;
        }

        var creada = await _repo.CreateAsync(idProd, UrlAlmacenada(obj), subida.FileName, subida.ContentType, subida.ContentLength, idArchivo, ct);

        // S106: el servidor genera las variantes desde el original. Si fallan, la imagen queda creada sin ellas
        // (el barrido las completa) y el 201 no cambia.
        var generadas = await _variantes.GenerateAsync(obj, ProductoImagenVariantes.Todas, ct);
        if (generadas.IsSuccess)
        {
            var urls = VarianteUrls(generadas.Value);
            await _repo.UpdateVariantesAsync(creada.IdProductoImagen, urls.Miniatura, urls.Ficha, ct);
            creada.UrlMiniatura = urls.Miniatura;
            creada.UrlFicha = urls.Ficha;
        }
        else
        {
            _log.LogWarning("IMAGEN_VARIANTES_PENDIENTES: imagen {IdProductoImagen} sin variantes: {Code}", creada.IdProductoImagen, generadas.Error!.Code);
        }

        return Result<ProductoImagen>.Success(await VistaAsync(creada, obj, ct));
    }

    public async Task<Result> GenerarVariantesAsync(int idProductoImagen, long idProd, int idTenant, CancellationToken ct = default)
    {
        var img = await _repo.GetByIdInProductAsync(idProductoImagen, idProd, ct);
        if (img is null)
            return Result.Failure(new Error("IMAGEN_NO_ENCONTRADA", "Imagen no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        // Filas anteriores a S104: no hay objeto en el storage que procesar.
        if (img.IdArchivo is not { } id) return Result.Success();

        var archivo = await _archivos.GetByIdAsync(id, idTenant, ct);
        if (archivo is null) return Result.Success();

        var generadas = await _variantes.GenerateAsync(new StorageObjectRef(archivo.Contenedor, archivo.Clave), ProductoImagenVariantes.Todas, ct);
        if (generadas.IsFailure) return Result.Failure(generadas.Error!);

        var urls = VarianteUrls(generadas.Value);
        await _repo.UpdateVariantesAsync(idProductoImagen, urls.Miniatura, urls.Ficha, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProductoImagen>>> GetActivesAsync(long idProd, int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetActivesAsync(idProd, ct);
        var privados = list.Where(i => i.IdArchivo.HasValue && EsPrivada(i.Url)).Select(i => i.IdArchivo!.Value).ToList();
        var archivos = (await _archivos.GetByIdsAsync(privados, idTenant, ct)).ToDictionary(a => a.IdArchivo);

        var result = new List<ProductoImagen>(list.Count);
        foreach (var img in list)
        {
            var obj = img.IdArchivo is { } id && archivos.TryGetValue(id, out var a) ? new StorageObjectRef(a.Contenedor, a.Clave) : null;
            result.Add(await VistaAsync(img, obj, ct));
        }
        return Result<IReadOnlyList<ProductoImagen>>.Success(result);
    }

    public async Task<Result> DeleteAsync(long idProd, int idTenant, int idProductoImagen, CancellationToken ct = default)
    {
        var img = await _repo.GetByIdInProductAsync(idProductoImagen, idProd, ct);
        if (img is null)
            return Result.Failure(new Error("IMAGEN_NO_ENCONTRADA", "Imagen no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));

        // S125 D1: el Archivo se marca Eliminado antes de borrar la fila y de tocar el proveedor. Si algo posterior
        // falla o se cancela, el barrido (S105) lo purga; un Eliminado aún referenciado no se purga (ArchivoRepositoryAsync).
        var archivo = img.IdArchivo.HasValue ? await _archivos.GetByIdAsync(img.IdArchivo.Value, idTenant, ct) : null;
        if (archivo is not null)
            await _archivos.MarkDeletedAsync(archivo.IdArchivo, idTenant, ct);
        await _repo.DeleteAsync(idProd, idProductoImagen, ct);
        if (archivo is null) return Result.Success();

        // S57.82: eliminar el objeto después del commit (best-effort; la BD es la fuente de verdad y nunca revierte el 204).
        var borrado = await _storage.DeleteAsync(new StorageObjectRef(archivo.Contenedor, archivo.Clave), ct);
        if (borrado.IsFailure)
            _log.LogWarning("IMAGEN_ORPHAN_CANDIDATE: no se pudo eliminar {Contenedor}/{Clave}: {Code}", archivo.Contenedor, archivo.Clave, borrado.Error!.Code);

        // S106: las variantes se borran con el original (best-effort, mismo criterio que el objeto).
        var variantes = await _variantes.DeleteAsync(new StorageObjectRef(archivo.Contenedor, archivo.Clave), ProductoImagenVariantes.Todas, ct);
        if (variantes.IsFailure)
            _log.LogWarning("IMAGEN_ORPHAN_CANDIDATE: no se pudieron eliminar las variantes de {Contenedor}/{Clave}: {Code}", archivo.Contenedor, archivo.Clave, variantes.Error!.Code);

        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<string>>> FindOrphanFilesAsync(int idTenant, CancellationToken ct = default)
    {
        // S57.83-G1: reporte solo-lectura (objetos del storage sin fila viva en Archivo). Nunca borra.
        // S105 (veredicto S104, O5): solo el prefijo del tenant que consulta ("t{idTenant}/", StorageKey.Build).
        // Los archivos de importación ("importaciones/…") quedan fuera por el mismo prefijo.
        var prefijo = $"t{Inv(idTenant)}/";
        var orphans = new List<string>();
        foreach (var contenedor in new[] { StorageContainers.Public, StorageContainers.Private })
        {
            var vivos = new HashSet<string>(await _archivos.GetLiveKeysAsync(contenedor, idTenant, ct), StringComparer.OrdinalIgnoreCase);
            // S106: las variantes de cada clave viva también son legítimas (no tienen fila propia en Archivo).
            foreach (var clave in vivos.ToList())
                foreach (var spec in ProductoImagenVariantes.Todas)
                    vivos.Add(ImageVariantKeys.For(clave, spec));
            string? continuacion = null;
            do
            {
                var pagina = await _storage.ListAsync(contenedor, prefijo, PaginaListado, continuacion, ct);
                if (pagina.IsFailure) return Result<IReadOnlyList<string>>.Failure(pagina.Error!);
                orphans.AddRange(pagina.Value.Items
                    .Where(i => !vivos.Contains(i.Key))
                    .Select(i => $"{contenedor}/{i.Key}"));
                continuacion = pagina.Value.Continuation;
            } while (continuacion is not null);
        }
        orphans.Sort(StringComparer.Ordinal);
        return Result<IReadOnlyList<string>>.Success(orphans);
    }

    public async Task<Result<int>> RelocateAsync(long idProd, int idTenant, bool makePublic, CancellationToken ct = default)
    {
        var destino = makePublic ? StorageContainers.Public : StorageContainers.Private;
        var list = await _repo.GetActivesAsync(idProd, ct);
        var ids = list.Where(i => i.IdArchivo.HasValue).Select(i => i.IdArchivo!.Value).ToList();
        var archivos = (await _archivos.GetByIdsAsync(ids, idTenant, ct)).ToDictionary(a => a.IdArchivo);

        var moved = 0;
        foreach (var img in list)
        {
            if (img.IdArchivo is not { } id || !archivos.TryGetValue(id, out var archivo)) continue;
            if (string.Equals(archivo.Contenedor, destino, StringComparison.OrdinalIgnoreCase)) continue;

            var movido = await MoverAsync(id, idTenant, new StorageObjectRef(archivo.Contenedor, archivo.Clave), destino, ct);
            if (movido.IsFailure) return Result<int>.Failure(movido.Error!);
            await _repo.UpdateUrlAsync(img.IdProductoImagen, UrlAlmacenada(movido.Value), ct);

            // S106: las variantes siguen al original; solo se reescriben las que existen.
            var variantes = await _variantes.MoveAsync(new StorageObjectRef(archivo.Contenedor, archivo.Clave), destino, ProductoImagenVariantes.Todas, ct);
            if (variantes.IsFailure) return Result<int>.Failure(variantes.Error!);
            if (img.UrlMiniatura is not null || img.UrlFicha is not null)
                await _repo.UpdateVariantesAsync(
                    img.IdProductoImagen,
                    img.UrlMiniatura is null ? null : UrlAlmacenada(new StorageObjectRef(destino, ImageVariantKeys.For(archivo.Clave, ProductoImagenVariantes.Listado))),
                    img.UrlFicha is null ? null : UrlAlmacenada(new StorageObjectRef(destino, ImageVariantKeys.For(archivo.Clave, ProductoImagenVariantes.Ficha))),
                    ct);

            moved++;
        }
        return Result<int>.Success(moved);
    }

    // S57.83-G3: los no publicados (inactivo u oculto en la tienda) van al contenedor privado.
    private static string ContenedorDe(Prod prod) =>
        prod.Activo && prod.VisibleStore ? StorageContainers.Public : StorageContainers.Private;

    private static bool EsPrivada(string? url) =>
        url is not null && url.StartsWith(MarcadorPrivado, StringComparison.Ordinal);

    private static string Inv(long value) => value.ToString(CultureInfo.InvariantCulture);

    private static Error ProductoNoEncontrado() =>
        new Error("PRODUCTO_NO_ENCONTRADO", "Producto no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404);

    /// <summary>Valor que se guarda en <see cref="ProductoImagen.Url"/>: la URL pública, o el marcador privado.</summary>
    private string UrlAlmacenada(StorageObjectRef obj)
    {
        var publica = string.Equals(obj.Container, StorageContainers.Public, StringComparison.OrdinalIgnoreCase)
            ? (_storage as IStorageDirectAccess)?.GetPublicUrl(obj)
            : null;
        return publica ?? MarcadorPrivado + obj.Key;
    }

    private async Task<Result<StorageObjectRef>> MoverAsync(long idArchivo, int idTenant, StorageObjectRef origen, string contenedor, CancellationToken ct)
    {
        var destino = new StorageObjectRef(contenedor, origen.Key);
        var movido = await _storage.MoveAsync(origen, destino, ct);
        if (movido.IsFailure)
        {
            // Idempotente: un intento anterior pudo mover el objeto y fallar antes de registrar el contenedor.
            var existe = await _storage.ExistsAsync(destino, ct);
            if (existe.IsFailure || !existe.Value) return Result<StorageObjectRef>.Failure(movido.Error!);
        }
        await _archivos.UpdateContainerAsync(idArchivo, idTenant, contenedor, ct);
        return Result<StorageObjectRef>.Success(destino);
    }

    /// <summary>
    /// Copia de la imagen para el cliente (sin navegaciones): <c>Url</c> pública tal cual; el marcador privado se
    /// reemplaza por una URL firmada de descarga, o por vacío si no se puede firmar.
    /// </summary>
    private async Task<ProductoImagen> VistaAsync(ProductoImagen img, StorageObjectRef? obj, CancellationToken ct)
    {
        var url = img.Url;
        if (EsPrivada(url))
        {
            url = string.Empty;
            if (obj is not null && _storage is IStorageDirectAccess directo)
            {
                var firmada = await directo.CreateDownloadUrlAsync(obj, DescargaTtl, ct);
                if (firmada.IsSuccess) url = firmada.Value;
            }
        }
        return new ProductoImagen
        {
            IdProductoImagen = img.IdProductoImagen,
            IdProd = img.IdProd,
            Url = url,
            NombreArchivo = img.NombreArchivo,
            ContentType = img.ContentType,
            TamanoBytes = img.TamanoBytes,
            Orden = img.Orden,
            EsPrincipal = img.EsPrincipal,
            Activo = img.Activo,
            FecCrea = img.FecCrea,
            IdArchivo = img.IdArchivo,
            UrlMiniatura = await FirmarVarianteAsync(img.UrlMiniatura, obj, ProductoImagenVariantes.Listado, ct),
            UrlFicha = await FirmarVarianteAsync(img.UrlFicha, obj, ProductoImagenVariantes.Ficha, ct)
        };
    }

    /// <summary>
    /// S106: URL de una variante tal como se entrega al cliente. La pública va tal cual; el marcador privado se
    /// reemplaza por una URL firmada del objeto de la variante, o por vacío si no se puede firmar.
    /// </summary>
    private async Task<string?> FirmarVarianteAsync(string? url, StorageObjectRef? original, ImageVariantSpec spec, CancellationToken ct)
    {
        if (url is null) return null;
        if (!EsPrivada(url)) return url;
        if (original is null || _storage is not IStorageDirectAccess directo) return string.Empty;

        var obj = new StorageObjectRef(original.Container, ImageVariantKeys.For(original.Key, spec));
        var firmada = await directo.CreateDownloadUrlAsync(obj, DescargaTtl, ct);
        return firmada.IsSuccess ? firmada.Value : string.Empty;
    }

    /// <summary>URLs almacenadas de las variantes generadas, por nombre.</summary>
    private (string? Miniatura, string? Ficha) VarianteUrls(IReadOnlyList<ImageVariant> variantes)
    {
        string? miniatura = null;
        string? ficha = null;
        foreach (var variante in variantes)
        {
            if (string.Equals(variante.Name, ProductoImagenVariantes.ListadoNombre, StringComparison.Ordinal))
                miniatura = UrlAlmacenada(variante.Object);
            else if (string.Equals(variante.Name, ProductoImagenVariantes.FichaNombre, StringComparison.Ordinal))
                ficha = UrlAlmacenada(variante.Object);
        }
        return (miniatura, ficha);
    }
}
