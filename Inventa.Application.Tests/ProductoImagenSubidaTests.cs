using System.Globalization;
using CBP.Results;
using CBP.Storage;
using CBP.Storage.Imaging;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace Inventa.Application.Tests;

// S104: subida directa de imágenes de producto. La API autoriza y firma (iniciar) y verifica el objeto que el
// navegador dejó en el storage (confirmar); nunca recibe el contenido.
public sealed class ProductoImagenSubidaTests
{
    private const int Tenant = 5;
    private const long IdProd = 8449;
    private static readonly byte[] Png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, 0x00];

    private sealed class Escenario
    {
        public StubStorageProvider Storage { get; } = new();
        public FakeArchivos Archivos { get; } = new();
        public FakeImagenes Imagenes { get; } = new();
        public ImageVariantService Variantes { get; }
        public ProductoImagenStorageServiceAsync Servicio { get; }

        public Escenario(bool visible = true)
        {
            var prods = new StubProdRepositoryAsync
            {
                GetByIdInTenant = (id, tenant, _) => Task.FromResult<Prod?>(id == IdProd && tenant == Tenant
                    ? new Prod { IdProd = IdProd, IdTenant = Tenant, Codigo = "FIC-10047", Nombre = "S104", Activo = true, VisibleStore = visible }
                    : null),
            };
            var subidas = new DirectUploadService(Storage, Archivos, NullLogger<DirectUploadService>.Instance);
            Variantes = new ImageVariantService(Storage, new FakeProcesador(), Options.Create(new ImagingOptions()),
                NullLogger<ImageVariantService>.Instance);
            Servicio = new ProductoImagenStorageServiceAsync(Imagenes, prods, Archivos, subidas, Storage, Variantes,
                NullLogger<ProductoImagenStorageServiceAsync>.Instance);
        }
    }

    [Fact]
    public async Task Iniciar_producto_de_otro_tenant_devuelve_404_sin_registrar_ni_firmar()
    {
        var e = new Escenario();

        var r = await e.Servicio.IniciarSubidaAsync(IdProd, 999, "foto.png", "image/png", Png.Length, TestContext.Current.CancellationToken);

        Assert.True(r.IsFailure);
        Assert.Equal("PRODUCTO_NO_ENCONTRADO", r.Error!.Code);
        Assert.Equal(404, r.Error.HttpStatusCode);
        Assert.Empty(e.Archivos.Filas);
        Assert.Empty(e.Storage.Firmados);
    }

    [Fact]
    public async Task Confirmar_objeto_valido_crea_la_imagen_con_IdArchivo_y_url_publica()
    {
        var ct = TestContext.Current.CancellationToken;
        var e = new Escenario();
        var iniciada = await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "foto.png", "image/png", Png.Length, ct);
        Assert.True(iniciada.IsSuccess);
        Assert.Equal("PUT", iniciada.Value.Metodo);
        Assert.Equal("image/png", iniciada.Value.Headers["Content-Type"]);
        var archivo = Assert.Single(e.Archivos.Filas);
        Assert.Equal(StorageContainers.Public, archivo.Contenedor);
        Assert.StartsWith("t5/inventa/producto/8449/", archivo.Clave);
        Assert.EndsWith(".png", archivo.Clave);

        // El navegador envía los bytes directo al storage con la URL firmada.
        e.Storage.Objetos[new StorageObjectRef(archivo.Contenedor, archivo.Clave)] = Png;
        var confirmada = await e.Servicio.ConfirmarSubidaAsync(IdProd, Tenant, iniciada.Value.IdArchivo, ct);

        Assert.True(confirmada.IsSuccess);
        Assert.Equal(iniciada.Value.IdArchivo, confirmada.Value.IdArchivo);
        Assert.Equal("uploads/" + archivo.Clave, confirmada.Value.Url);
        Assert.Equal("foto.png", confirmada.Value.NombreArchivo);
        Assert.Equal("image/png", confirmada.Value.ContentType);
        Assert.Equal(Png.Length, confirmada.Value.TamanoBytes);
        Assert.Equal(ArchivoEstado.Confirmado, archivo.Estado);
        var guardada = Assert.Single(e.Imagenes.Filas);
        Assert.Equal("uploads/" + archivo.Clave, guardada.Url);

        // Confirmar otra vez devuelve la misma imagen, sin duplicarla.
        var repetida = await e.Servicio.ConfirmarSubidaAsync(IdProd, Tenant, iniciada.Value.IdArchivo, ct);
        Assert.True(repetida.IsSuccess);
        Assert.Equal(confirmada.Value.IdProductoImagen, repetida.Value.IdProductoImagen);
        Assert.Single(e.Imagenes.Filas);
    }

    [Fact]
    public async Task Confirmar_tamano_o_firma_distintos_rechaza_borra_el_objeto_y_no_crea_imagen()
    {
        var ct = TestContext.Current.CancellationToken;
        // Llegó otra cosa que la autorizada: un byte de más, o el tamaño declarado sin la firma de un PNG.
        byte[][] contenidos = [[.. Png, 0x00], new byte[Png.Length]];
        foreach (var contenido in contenidos)
        {
            var e = new Escenario();
            var iniciada = await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "foto.png", "image/png", Png.Length, ct);
            var archivo = Assert.Single(e.Archivos.Filas);
            var obj = new StorageObjectRef(archivo.Contenedor, archivo.Clave);
            e.Storage.Objetos[obj] = contenido;

            var r = await e.Servicio.ConfirmarSubidaAsync(IdProd, Tenant, iniciada.Value.IdArchivo, ct);

            Assert.True(r.IsFailure);
            Assert.Equal(StorageErrors.UploadInvalidCode, r.Error!.Code);
            Assert.Equal(422, r.Error.HttpStatusCode);
            Assert.Contains(obj, e.Storage.Eliminados);
            Assert.DoesNotContain(obj, e.Storage.Objetos.Keys);
            Assert.Equal(ArchivoEstado.Rechazado, archivo.Estado);
            Assert.Empty(e.Imagenes.Filas);
        }
    }

    [Fact]
    public async Task Huerfanos_solo_del_tenant_que_consulta()
    {
        var ct = TestContext.Current.CancellationToken;
        var e = new Escenario();
        await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "foto.png", "image/png", Png.Length, ct);
        var vivo = Assert.Single(e.Archivos.Filas);
        e.Storage.Objetos[new StorageObjectRef(vivo.Contenedor, vivo.Clave)] = Png;
        e.Storage.Objetos[new StorageObjectRef(StorageContainers.Public, "t5/inventa/producto/8449/huerfano.png")] = Png;
        e.Storage.Objetos[new StorageObjectRef(StorageContainers.Private, "t4/inventa/producto/1/ajeno.png")] = Png;
        e.Storage.Objetos[new StorageObjectRef(StorageContainers.Public, "t50/inventa/producto/1/ajeno.png")] = Png;
        e.Storage.Objetos[new StorageObjectRef(StorageContainers.Private, "importaciones/5/2026/1.xlsx")] = Png;

        var r = await e.Servicio.FindOrphanFilesAsync(Tenant, ct);

        Assert.True(r.IsSuccess);
        Assert.Equal("public/t5/inventa/producto/8449/huerfano.png", Assert.Single(r.Value));
    }

    [Fact]
    public async Task Barrido_borra_objeto_y_fila_y_conserva_la_fila_si_el_storage_falla()
    {
        var ct = TestContext.Current.CancellationToken;
        var e = new Escenario();
        await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "a.png", "image/png", Png.Length, ct);
        await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "b.png", "image/png", Png.Length, ct);
        var purgable = e.Archivos.Filas[0];
        var fallido = e.Archivos.Filas[1];
        var objPurgable = new StorageObjectRef(purgable.Contenedor, purgable.Clave);
        var objFallido = new StorageObjectRef(fallido.Contenedor, fallido.Clave);
        e.Storage.Objetos[objPurgable] = Png;
        e.Storage.Objetos[objFallido] = Png;
        e.Storage.FallaBorrar.Add(objFallido);
        var barrido = new ArchivoBarridoServiceAsync(e.Archivos, e.Servicio, e.Storage, e.Variantes, NullLogger<ArchivoBarridoServiceAsync>.Instance);

        var r = await barrido.BarrerAsync(DateTime.MaxValue, 50, ct);

        Assert.True(r.IsSuccess);
        Assert.Equal(new ArchivoBarridoResumen(1, 1, 0, 0), r.Value);
        Assert.DoesNotContain(objPurgable, e.Storage.Objetos.Keys);
        Assert.Contains(objFallido, e.Storage.Objetos.Keys);
        Assert.Equal(fallido.IdArchivo, Assert.Single(e.Archivos.Filas).IdArchivo);
    }

    [Fact]
    public async Task Confirmar_genera_variantes_y_borrar_las_elimina()
    {
        var ct = TestContext.Current.CancellationToken;
        var e = new Escenario();
        var iniciada = await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "foto.png", "image/png", Png.Length, ct);
        var archivo = Assert.Single(e.Archivos.Filas);
        var original = new StorageObjectRef(archivo.Contenedor, archivo.Clave);
        e.Storage.Objetos[original] = Png;

        var confirmada = await e.Servicio.ConfirmarSubidaAsync(IdProd, Tenant, iniciada.Value.IdArchivo, ct);

        Assert.True(confirmada.IsSuccess);
        var guardada = Assert.Single(e.Imagenes.Filas);
        var raiz = archivo.Clave[..^4]; // sin la extensión del original
        Assert.Equal("uploads/" + raiz + "-listado.webp", guardada.UrlMiniatura);
        Assert.Equal("uploads/" + raiz + "-ficha.webp", guardada.UrlFicha);
        Assert.Equal(guardada.UrlMiniatura, confirmada.Value.UrlMiniatura);
        Assert.Equal(guardada.UrlFicha, confirmada.Value.UrlFicha);

        var listado = new StorageObjectRef(StorageContainers.Public, raiz + "-listado.webp");
        var ficha = new StorageObjectRef(StorageContainers.Public, raiz + "-ficha.webp");
        Assert.Contains(listado, e.Storage.Objetos.Keys);
        Assert.Contains(ficha, e.Storage.Objetos.Keys);

        // Borrar la imagen arrastra el original y sus variantes: no quedan objetos ni filas.
        var borrado = await e.Servicio.DeleteAsync(IdProd, Tenant, guardada.IdProductoImagen, ct);

        Assert.True(borrado.IsSuccess);
        Assert.DoesNotContain(original, e.Storage.Objetos.Keys);
        Assert.DoesNotContain(listado, e.Storage.Objetos.Keys);
        Assert.DoesNotContain(ficha, e.Storage.Objetos.Keys);
        Assert.Contains(listado, e.Storage.Eliminados);
        Assert.Contains(ficha, e.Storage.Eliminados);
        Assert.Equal(ArchivoEstado.Eliminado, archivo.Estado);
        Assert.Empty(e.Imagenes.Filas);
    }

    [Fact]
    public async Task Borrar_con_proveedor_que_lanza_deja_el_archivo_eliminado_y_el_barrido_lo_purga()
    {
        // S125 D1: con el orden anterior el Archivo quedaba Confirmado sin referencia y ningún proceso lo purgaba.
        var ct = TestContext.Current.CancellationToken;
        var e = new Escenario();
        var iniciada = await e.Servicio.IniciarSubidaAsync(IdProd, Tenant, "foto.png", "image/png", Png.Length, ct);
        var archivo = Assert.Single(e.Archivos.Filas);
        var original = new StorageObjectRef(archivo.Contenedor, archivo.Clave);
        e.Storage.Objetos[original] = Png;
        var confirmada = await e.Servicio.ConfirmarSubidaAsync(IdProd, Tenant, iniciada.Value.IdArchivo, ct);
        Assert.True(confirmada.IsSuccess);
        var guardada = Assert.Single(e.Imagenes.Filas);
        e.Storage.LanzaBorrar.Add(original);

        await Assert.ThrowsAsync<System.IO.IOException>(() => e.Servicio.DeleteAsync(IdProd, Tenant, guardada.IdProductoImagen, ct));

        Assert.Empty(e.Imagenes.Filas);
        Assert.Equal(ArchivoEstado.Eliminado, archivo.Estado);
        Assert.Contains(original, e.Storage.Objetos.Keys);

        e.Storage.LanzaBorrar.Clear();
        var barrido = new ArchivoBarridoServiceAsync(e.Archivos, e.Servicio, e.Storage, e.Variantes, NullLogger<ArchivoBarridoServiceAsync>.Instance);
        var r = await barrido.BarrerAsync(DateTime.MaxValue, 50, ct);

        Assert.True(r.IsSuccess);
        Assert.Equal(new ArchivoBarridoResumen(1, 0, 0, 0), r.Value);
        Assert.DoesNotContain(original, e.Storage.Objetos.Keys);
        Assert.Empty(e.Archivos.Filas);
    }

    /// <summary>Tabla Archivo en memoria: filtra por tenant como el repositorio real.</summary>
    private sealed class FakeArchivos : IArchivoRepositoryAsync
    {
        public List<Archivo> Filas { get; } = [];

        /// <summary>Archivos que alguna ProductoImagen referencia (el repositorio real lo mira en la BD).</summary>
        public HashSet<long> Referenciados { get; } = [];

        private static string Inv(long value) => value.ToString(CultureInfo.InvariantCulture);
        private Archivo? Buscar(string uploadId) => Filas.FirstOrDefault(a => Inv(a.IdArchivo) == uploadId);

        public Task<Result<string>> RegisterPendingAsync(PendingUpload upload, CancellationToken ct = default)
        {
            var fila = new Archivo
            {
                IdArchivo = Filas.Count + 1,
                IdTenant = int.Parse(upload.Tenant, CultureInfo.InvariantCulture),
                Proveedor = upload.Provider,
                Contenedor = upload.Object.Container,
                Clave = upload.Object.Key,
                Entidad = upload.Entity,
                IdEntidad = long.Parse(upload.EntityId, CultureInfo.InvariantCulture),
                NombreArchivo = upload.FileName,
                ContentType = upload.ContentType,
                TamanoBytes = upload.ContentLength,
                Estado = ArchivoEstado.Pendiente,
            };
            Filas.Add(fila);
            return Task.FromResult(Result<string>.Success(Inv(fila.IdArchivo)));
        }

        public Task<Result<UploadRecord?>> GetAsync(string uploadId, string tenant, CancellationToken ct = default)
        {
            var a = Buscar(uploadId);
            var record = a is null || Inv(a.IdTenant) != tenant
                ? null
                : new UploadRecord(Inv(a.IdArchivo), Inv(a.IdTenant), new StorageObjectRef(a.Contenedor, a.Clave), a.Entidad, Inv(a.IdEntidad),
                    a.NombreArchivo, a.ContentType, a.TamanoBytes, (UploadState)a.Estado, a.ETag);
            return Task.FromResult(Result<UploadRecord?>.Success(record, allowNull: true));
        }

        public Task<Result> MarkConfirmedAsync(string uploadId, StorageObjectInfo info, CancellationToken ct = default)
        {
            var a = Buscar(uploadId)!;
            a.Estado = ArchivoEstado.Confirmado;
            a.ETag = info.ETag;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> MarkRejectedAsync(string uploadId, CancellationToken ct = default)
        {
            Buscar(uploadId)!.Estado = ArchivoEstado.Rechazado;
            return Task.FromResult(Result.Success());
        }

        public Task<Result<long>> GetTenantUsageAsync(string tenant, CancellationToken ct = default) =>
            Task.FromResult(Result<long>.Success(0));

        public Task<Archivo?> GetByIdAsync(long idArchivo, int idTenant, CancellationToken ct = default) =>
            Task.FromResult(Filas.FirstOrDefault(a => a.IdArchivo == idArchivo && a.IdTenant == idTenant));

        public Task<IReadOnlyList<Archivo>> GetByIdsAsync(IReadOnlyCollection<long> idsArchivo, int idTenant, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Archivo>>(Filas.Where(a => a.IdTenant == idTenant && idsArchivo.Contains(a.IdArchivo)).ToList());

        public Task MarkDeletedAsync(long idArchivo, int idTenant, CancellationToken ct = default)
        {
            Filas.First(a => a.IdArchivo == idArchivo && a.IdTenant == idTenant).Estado = ArchivoEstado.Eliminado;
            return Task.CompletedTask;
        }

        public Task UpdateContainerAsync(long idArchivo, int idTenant, string contenedor, CancellationToken ct = default)
        {
            Filas.First(a => a.IdArchivo == idArchivo && a.IdTenant == idTenant).Contenedor = contenedor;
            return Task.CompletedTask;
        }

        public Task<IReadOnlyList<string>> GetLiveKeysAsync(string contenedor, int idTenant, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<string>>(Filas.Where(a => a.Contenedor == contenedor && a.IdTenant == idTenant && a.Estado <= ArchivoEstado.Confirmado).Select(a => a.Clave).ToList());

        public Task<IReadOnlyList<Archivo>> GetPurgablesAsync(string proveedor, DateTime limite, int max, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<Archivo>>(Filas
                .Where(a => a.Proveedor == proveedor && a.Estado != ArchivoEstado.Confirmado && (a.FecEstado ?? a.FecCrea) < limite && !Referenciados.Contains(a.IdArchivo))
                .Take(max)
                .ToList());

        public Task<bool> DeletePurgedAsync(long idArchivo, byte estado, CancellationToken ct = default) =>
            Task.FromResult(estado != ArchivoEstado.Confirmado && !Referenciados.Contains(idArchivo)
                && Filas.RemoveAll(a => a.IdArchivo == idArchivo && a.Estado == estado) > 0);

        public Task<IReadOnlyList<ArchivoDesubicado>> GetDesubicadosAsync(int max, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ArchivoDesubicado>>([]);

        public Task<IReadOnlyList<ImagenSinVariantes>> GetSinVariantesAsync(int max, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ImagenSinVariantes>>([]);

        public Task<int> CerrarConfirmadosSinImagenAsync(string entidad, DateTime limite, int max, CancellationToken ct = default) =>
            Task.FromResult(0);
    }

    /// <summary>Tabla ProductoImagen en memoria: solo lo que usa el flujo de subida.</summary>
    private sealed class FakeImagenes : IProductoImagenRepositoryAsync
    {
        public List<ProductoImagen> Filas { get; } = [];

        public Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, CancellationToken ct = default) =>
            CreateAsync(idProd, url, nombreArchivo, contentType, tamanoBytes, null, ct);

        public Task<ProductoImagen> CreateAsync(long idProd, string url, string nombreArchivo, string contentType, long tamanoBytes, long? idArchivo, CancellationToken ct = default)
        {
            var fila = new ProductoImagen
            {
                IdProductoImagen = Filas.Count + 1, IdProd = idProd, Url = url, NombreArchivo = nombreArchivo,
                ContentType = contentType, TamanoBytes = tamanoBytes, Orden = Filas.Count, EsPrincipal = Filas.Count == 0, IdArchivo = idArchivo,
            };
            Filas.Add(fila);
            return Task.FromResult(fila);
        }

        public Task<ProductoImagen?> GetByIdArchivoAsync(long idArchivo, long idProd, CancellationToken ct = default) =>
            Task.FromResult(Filas.FirstOrDefault(i => i.IdArchivo == idArchivo && i.IdProd == idProd));

        public Task<IReadOnlyList<ProductoImagen>> GetActivesAsync(long idProd, CancellationToken ct = default) =>
            Task.FromResult<IReadOnlyList<ProductoImagen>>(Filas.Where(i => i.IdProd == idProd && i.Activo).ToList());

        public Task<ProductoImagen?> GetPrincipalAsync(long idProd, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<ProductoImagen?> GetByIdInProductAsync(int idProductoImagen, long idProd, CancellationToken ct = default) =>
            Task.FromResult(Filas.FirstOrDefault(i => i.IdProductoImagen == idProductoImagen && i.IdProd == idProd));
        public Task SetPrincipalAsync(long idProd, int idProductoImagen, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<ProductoImagen>> ReorderAsync(long idProd, IReadOnlyList<int> orderedIds, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<(string Url, string NombreArchivo)> DeleteAsync(long idProd, int idProductoImagen, CancellationToken ct = default)
        {
            var fila = Filas.First(i => i.IdProductoImagen == idProductoImagen && i.IdProd == idProd);
            Filas.Remove(fila);
            return Task.FromResult((fila.Url, fila.NombreArchivo));
        }
        public Task<IReadOnlyList<long>> GetActiveProdIdsAsync(int idTenant, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<string?> GetPrincipalUrlAsync(long idProd, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<IReadOnlyList<string>> GetAllUrlsAsync(CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateUrlAsync(int idProductoImagen, string newUrl, CancellationToken ct = default) => throw new NotSupportedException();
        public Task UpdateVariantesAsync(int idProductoImagen, string? urlMiniatura, string? urlFicha, CancellationToken ct = default)
        {
            var fila = Filas.First(i => i.IdProductoImagen == idProductoImagen);
            fila.UrlMiniatura = urlMiniatura;
            fila.UrlFicha = urlFicha;
            return Task.CompletedTask;
        }
    }

    /// <summary>S106: procesador que no decodifica; solo devuelve bytes y el lado máximo pedido.</summary>
    private sealed class FakeProcesador : IImageProcessor
    {
        private static readonly byte[] Contenido = [0x52, 0x49, 0x46, 0x46, 0x01];

        public Task<Result<ImageInfo>> ProbeAsync(Stream source, CancellationToken ct = default) =>
            Task.FromResult(Result<ImageInfo>.Success(new ImageInfo(400, 300, "image/webp")));

        public Task<Result<ProcessedImage>> ResizeAsync(Stream source, ImageVariantSpec spec, CancellationToken ct = default) =>
            Task.FromResult(Result<ProcessedImage>.Success(new ProcessedImage(Contenido, spec.MaxSide, spec.MaxSide / 2, "image/webp")));
    }
}
