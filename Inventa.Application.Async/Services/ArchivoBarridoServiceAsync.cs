using CBP.Results;
using CBP.Storage;
using CBP.Storage.Imaging;
using Inventa.Data.Async.Repositories;
using Microsoft.Extensions.Logging;

namespace Inventa.Application.Async.Services;

/// <summary>Resultado de una vuelta del barrido del storage.</summary>
public sealed record ArchivoBarridoResumen(int Purgados, int Fallidos, int Reubicados, int Variantes);

/// <summary>
/// S105 (P9): limpieza diferida del storage sobre la tabla <c>Archivo</c>, que hace de cola; sin outbox (decisión del dueño 2026-11-11; dictamen S132 en Datos-Gobernanza §6).
/// Purga objeto y fila de pendientes vencidos, rechazados y eliminados, y repara la ubicación público/privado.
/// </summary>
public interface IArchivoBarridoServiceAsync
{
    /// <summary>Una vuelta: hasta <paramref name="lote"/> archivos creados o cerrados antes de <paramref name="limite"/>.</summary>
    Task<Result<ArchivoBarridoResumen>> BarrerAsync(DateTime limite, int lote, CancellationToken ct = default);
}

public sealed class ArchivoBarridoServiceAsync : IArchivoBarridoServiceAsync
{
    private readonly IArchivoRepositoryAsync _archivos;
    private readonly IProductoImagenStorageServiceAsync _imagenes;
    private readonly IStorageProvider _storage;
    private readonly ImageVariantService _variantes;
    private readonly ILogger<ArchivoBarridoServiceAsync> _log;

    public ArchivoBarridoServiceAsync(
        IArchivoRepositoryAsync archivos,
        IProductoImagenStorageServiceAsync imagenes,
        IStorageProvider storage,
        ImageVariantService variantes,
        ILogger<ArchivoBarridoServiceAsync> log)
    {
        _archivos = archivos;
        _imagenes = imagenes;
        _storage = storage;
        _variantes = variantes;
        _log = log;
    }

    public async Task<Result<ArchivoBarridoResumen>> BarrerAsync(DateTime limite, int lote, CancellationToken ct = default)
    {
        if (lote <= 0) throw new ArgumentOutOfRangeException(nameof(lote));
        int purgados = 0, fallidos = 0, reubicados = 0, variantes = 0;

        // 1) Primero el objeto, después la fila: si el storage falla, la fila queda y la próxima vuelta reintenta.
        //    Solo filas del proveedor activo: un objeto de otro proveedor no se puede borrar desde este.
        foreach (var a in await _archivos.GetPurgablesAsync(_storage.Name, limite, lote, ct))
        {
            var borrado = await _storage.DeleteAsync(new StorageObjectRef(a.Contenedor, a.Clave), ct);
            if (borrado.IsFailure)
            {
                fallidos++;
                _log.LogWarning("ARCHIVO_BARRIDO_FALLO: archivo {IdArchivo} {Contenedor}/{Clave}: {Code}", a.IdArchivo, a.Contenedor, a.Clave, borrado.Error!.Code);
                continue;
            }
            // S106: las variantes del objeto se borran con él; si quedan, la fila también queda.
            var variantesBorradas = await _variantes.DeleteAsync(new StorageObjectRef(a.Contenedor, a.Clave), ProductoImagenVariantes.Todas, ct);
            if (variantesBorradas.IsFailure)
            {
                fallidos++;
                _log.LogWarning("ARCHIVO_BARRIDO_FALLO: variantes de archivo {IdArchivo} {Contenedor}/{Clave}: {Code}", a.IdArchivo, a.Contenedor, a.Clave, variantesBorradas.Error!.Code);
                continue;
            }
            if (await _archivos.DeletePurgedAsync(a.IdArchivo, a.Estado, ct))
            {
                purgados++;
                _log.LogInformation("ARCHIVO_PURGADO: archivo {IdArchivo} tenant {IdTenant} estado {Estado} {Contenedor}/{Clave}", a.IdArchivo, a.IdTenant, a.Estado, a.Contenedor, a.Clave);
            }
        }

        // 2) Reubicación que quedó pendiente al cambiar la visibilidad: el mismo método idempotente que usa el PATCH.
        foreach (var d in await _archivos.GetDesubicadosAsync(lote, ct))
        {
            var movidos = await _imagenes.RelocateAsync(d.IdProd, d.IdTenant, d.Publico, ct);
            if (movidos.IsSuccess)
            {
                reubicados += movidos.Value;
                continue;
            }
            fallidos++;
            _log.LogWarning("ARCHIVO_BARRIDO_FALLO: reubicar producto {IdProd} tenant {IdTenant}: {Code}", d.IdProd, d.IdTenant, movidos.Error!.Code);
        }

        // 3) S106: imágenes confirmadas a las que les faltan variantes (la generación falló al confirmar, o son
        //    anteriores al sprint). Generar es idempotente: sobrescribe la variante que ya existiera.
        foreach (var falta in await _archivos.GetSinVariantesAsync(lote, ct))
        {
            var generadas = await _imagenes.GenerarVariantesAsync(falta.IdProductoImagen, falta.IdProd, falta.IdTenant, ct);
            if (generadas.IsSuccess)
            {
                variantes++;
                continue;
            }
            fallidos++;
            _log.LogWarning("ARCHIVO_BARRIDO_FALLO: variantes de imagen {IdProductoImagen}: {Code}", falta.IdProductoImagen, generadas.Error!.Code);
        }

        // 4) S132: confirmados de producto que nunca llegaron a tener imagen (falló la creación tras confirmar): pasan a eliminados y la próxima vuelta los purga.
        var cerrados = await _archivos.CerrarConfirmadosSinImagenAsync(ProductoImagenStorageServiceAsync.Entidad, limite, lote, ct);
        if (cerrados > 0)
            _log.LogWarning("ARCHIVO_CONFIRMADO_SIN_IMAGEN: {Cerrados} archivos confirmados sin imagen pasan a eliminados", cerrados);

        return Result<ArchivoBarridoResumen>.Success(new ArchivoBarridoResumen(purgados, fallidos, reubicados, variantes));
    }
}
