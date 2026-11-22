using CBP.Excel.Abstractions;
using CBP.Results;
using CBP.Results.Errors;
using CBP.Storage;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Inventa.Application.Async.Services.Importacion;

// ── DTOs de API ──
public sealed record PlantillaProductoResult(byte[] Contenido, string NombreArchivo);

public sealed record IncidenciaLoteDto(int Fila, string Columna, string Codigo, string Mensaje, string Severidad);

public sealed record LoteCreadoResult(
    long Id,
    string Estado,
    int Filas,
    int Productos,
    int Variantes,
    int Nuevos,
    int Existentes,
    int Errores,
    int Advertencias,
    bool YaAplicado,
    IReadOnlyList<IncidenciaLoteDto> Incidencias)
{
    /// <summary>S91.4: resumen de la vista previa de cambios (null si hay errores de validación).</summary>
    public ResumenCambios? Cambios { get; init; }
}

public sealed record LoteDetalleResult(
    long Id,
    string Tipo,
    string NombreArchivo,
    string Estado,
    int Filas,
    int Productos,
    int Variantes,
    int Nuevos,
    int Existentes,
    int Errores,
    int Advertencias,
    DateTime FecCrea,
    DateTime? FecAplicada,
    IReadOnlyList<IncidenciaLoteDto> Incidencias)
{
    /// <summary>S91.4: resumen de cambios guardado al validar (null en lotes anteriores a S91.4).</summary>
    public ResumenCambios? Cambios { get; init; }
}

public sealed record ReporteLoteResult(byte[] Contenido, string NombreArchivo);

public sealed record LoteItemResult(
    long Id, string Tipo, string Estado, int Filas, int Errores, DateTime FecCrea,
    string NombreArchivo, int Advertencias, DateTime? FecAplicada)
{
    /// <summary>S91.4: filas modificadas según la vista previa (null en lotes anteriores a S91.4).</summary>
    public int? Modificados { get; init; }
}

/// <summary>S91.4: página de la vista previa de cambios de un lote.</summary>
public sealed record CambiosPaginaResult(bool Disponible, IReadOnlyList<CambioFila> Items, int Page, int PageSize, int Total);

public interface IImportacionServiceAsync
{
    Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default);
    Task<Result<LoteCreadoResult>> ValidarProductosAsync(int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default);
    Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default);
    Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default);
    Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default);
    Task<Result<CambiosPaginaResult>> GetCambiosAsync(long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default);
}

public sealed class ImportacionServiceAsync : IImportacionServiceAsync
{
    private const long MaxBytes = 10L * 1024 * 1024;
    private const int MaxResumenIncidencias = 500;
    private const int MaxResumenBytes = 64 * 1024;

    /// <summary>S104: prefijo de las claves de los archivos de importación, en el contenedor privado del storage.</summary>
    public const string PrefijoArchivos = "importaciones/";
    private const string ContentTypeXlsx = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private static readonly string[] EncabezadosObligatorios =
        ["codigoproducto", "nombreproducto", "categoria", "unidadbase", "codigovariante", "nombrevariante", "precioventa"];

    private readonly IExcelTemplateGenerator _plantillas;
    private readonly IExcelReader _lector;
    private readonly IExcelSheetInspector _inspector;
    private readonly IExcelResultAnnotator _anotador;
    private readonly ProductoImportacionValidadorAsync _validador;
    private readonly IImportacionLoteRepositoryAsync _lotes;
    private readonly ICategoriaRepositoryAsync _categorias;
    private readonly IMarcaRepositoryAsync _marcas;
    private readonly IUnidadRepositoryAsync _unidades;
    private readonly IBodegaRepositoryAsync _bodegas;
    private readonly ITenantServiceAsync _tenants;
    private readonly IStorageProvider _archivos;
    private readonly IProductoImportSpRepositoryAsync _comercial;

    private static readonly JsonSerializerOptions JsonCambios = new(JsonSerializerDefaults.Web);

    public ImportacionServiceAsync(
        IExcelTemplateGenerator plantillas,
        IExcelReader lector,
        IExcelSheetInspector inspector,
        IExcelResultAnnotator anotador,
        ProductoImportacionValidadorAsync validador,
        IImportacionLoteRepositoryAsync lotes,
        ICategoriaRepositoryAsync categorias,
        IMarcaRepositoryAsync marcas,
        IUnidadRepositoryAsync unidades,
        IBodegaRepositoryAsync bodegas,
        ITenantServiceAsync tenants,
        IStorageProvider archivos,
        IProductoImportSpRepositoryAsync comercial)
    {
        _plantillas = plantillas;
        _lector = lector;
        _inspector = inspector;
        _anotador = anotador;
        _validador = validador;
        _lotes = lotes;
        _categorias = categorias;
        _marcas = marcas;
        _unidades = unidades;
        _bodegas = bodegas;
        _tenants = tenants;
        _archivos = archivos;
        _comercial = comercial;
    }

    public async Task<Result<PlantillaProductoResult>> GetPlantillaProductosAsync(int idTenant, CancellationToken ct = default)
    {
        var tenant = await GetTenantAsync(idTenant, ct);
        if (tenant is null)
            return Result<PlantillaProductoResult>.Failure(new Error("TENANT_NO_EXISTE", "Tenant no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404));

        var listas = await CargarListasAsync(idTenant, ct);
        var options = ProductoPlantilla.Build(listas.Unidades, listas.Bodegas, listas.Categorias, listas.Marcas);
        using var stream = await _plantillas.GenerateTemplateAsync<ProductoImportRow>(options, ct);
        using var ms = new MemoryStream();
        await stream.CopyToAsync(ms, ct);
        var codigo = SanearCodigo(tenant.Codigo, idTenant);
        return Result<PlantillaProductoResult>.Success(new(ms.ToArray(), $"Plantilla_Productos_{codigo}_{DateTime.UtcNow:yyyyMMdd}.xlsx"));
    }

    public async Task<Result<LoteCreadoResult>> ValidarProductosAsync(
        int idTenant, int idUsuario, string fileName, byte[] contenido, CancellationToken ct = default)
    {
        if (contenido.Length == 0)
            return Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_VACIO", "El archivo está vacío.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (contenido.Length > MaxBytes)
            return Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_MUY_GRANDE", "El archivo supera 10 MB.").WithType(ErrorType.Validation).WithHttpStatus(413));
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        if (ext == ".xlsm")
            return Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_MACROS", "No se aceptan archivos .xlsm con macros; suba .xlsx.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (ext != ".xlsx")
            return Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_TIPO", "Solo se aceptan archivos .xlsx.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (contenido.Length < 2 || contenido[0] != 0x50 || contenido[1] != 0x4B)
            return Result<LoteCreadoResult>.Failure(new Error("ARCHIVO_FIRMA", "El archivo no es un .xlsx válido (firma ZIP ausente).").WithType(ErrorType.Validation).WithHttpStatus(400));

        var sha = Convert.ToHexString(SHA256.HashData(contenido));
        var loteAplicado = await _lotes.GetUltimoAplicadoByShaAsync(idTenant, ImportacionLoteTipos.Productos, sha, ct);
        var yaAplicado = loteAplicado is not null;

        List<ExcelRow> filasExcel;
        List<string> encabezados;
        try
        {
            using var ms = new MemoryStream(contenido, writable: false);
            var info = _inspector.Inspect(ms, new ExcelReadOptions { SheetName = "Productos" });
            encabezados = info.Headers.ToList();
            using var ms2 = new MemoryStream(contenido, writable: false);
            filasExcel = (await LeerFilasAsync(ms2, ct)).ToList();
        }
        catch (InvalidOperationException ex)
        {
            return await CrearLoteEstructuralAsync(idTenant, idUsuario, fileName, sha, contenido,
                "ARCHIVO_LIMITE", $"No se pudo leer el archivo: {ex.Message}", ct);
        }

        var encabezadosNormalizados = encabezados
            .Select(h => CBP.Excel.Parsing.ExcelValueParser.NormalizeHeader(h.TrimEnd('*').Trim()))
            .ToList();
        var faltantes = EncabezadosObligatorios
            .Where(o => !encabezadosNormalizados.Contains(o))
            .ToList();
        if (faltantes.Count > 0)
        {
            return await CrearLoteEstructuralAsync(idTenant, idUsuario, fileName, sha, contenido,
                "PLANTILLA_COLUMNAS", $"Faltan columnas obligatorias: {string.Join(", ", faltantes)}. Descargue la plantilla vigente.", ct);
        }

        var validacion = await _validador.ValidarAsync(filasExcel, idTenant, ct);
        var incidencias = validacion.Errores
            .Select(e => new IncidenciaLoteDto(e.Fila, e.Columna, e.Codigo, e.Mensaje, "Error"))
            .Concat(validacion.Advertencias.Select(a => new IncidenciaLoteDto(a.Fila, a.Columna, a.Codigo, a.Mensaje, "Advertencia")))
            .OrderBy(i => i.Fila).ThenBy(i => i.Severidad)
            .ToList();
        // S91.4: vista previa de cambios (solo si no hay errores) y reglas de servidor.
        CambiosImportacion? cambios = null;
        if (validacion.Errores.Count == 0)
            cambios = await CalcularCambiosAsync(validacion, _comercial, idTenant, ct);
        var erroresExtra = 0;
        var advertenciasExtra = 0;
        var refLote = loteAplicado is null ? "" : $"lote #{loteAplicado.IdImportacionLote}";
        if (cambios is { Resumen.HayCambios: false })
        {
            erroresExtra++;
            var idem = loteAplicado is null ? ""
                : $" (idéntico al {refLote} aplicado el {loteAplicado.FecAplicada?.ToLocalTime():dd-MM-yyyy})";
            incidencias.Insert(0, new IncidenciaLoteDto(0, "", "SIN_CAMBIOS",
                $"El catálogo ya coincide con este archivo; no hay nada que aplicar{idem}.", "Error"));
        }
        else if (loteAplicado is not null)
        {
            advertenciasExtra++;
            var msg = cambios is null
                ? $"Este archivo ya se aplicó en el {refLote}."
                : $"Este archivo ya se aplicó en el {refLote}; se volverán a aplicar sus valores en {cambios.Resumen.Nuevos + cambios.Resumen.Modificados} filas.";
            incidencias.Insert(0, new IncidenciaLoteDto(0, "", cambios is null ? "ARCHIVO_YA_APLICADO" : "ARCHIVO_YA_APLICADO_CON_CAMBIOS", msg, "Advertencia"));
        }

        var totalErrores = validacion.Errores.Count + erroresExtra;
        var totalAdvertencias = validacion.Advertencias.Count + advertenciasExtra;
        var estado = totalErrores == 0 ? ImportacionLoteEstados.Validado : ImportacionLoteEstados.ConErrores;
        var resumen = ConstruirResumen(validacion, incidencias, cambios, totalErrores, totalAdvertencias);
        var lote = await _lotes.CreateAsync(new ImportacionLote
        {
            IdTenant = idTenant,
            Tipo = ImportacionLoteTipos.Productos,
            NombreArchivo = Path.GetFileName(fileName),
            Sha256 = sha,
            RutaArchivo = "",
            Estado = estado,
            Filas = validacion.Filas.Count,
            Productos = validacion.Productos,
            Variantes = validacion.Variantes,
            Nuevos = validacion.Nuevos,
            Existentes = validacion.Existentes,
            Errores = totalErrores,
            Advertencias = totalAdvertencias,
            IdUsuario = idUsuario,
            ResumenJson = resumen,
        }, ct);
        var ruta = RutaLote(idTenant, lote.IdImportacionLote);
        var guardado = await GuardarArchivoAsync(ruta, contenido, ContentTypeXlsx, ct);
        if (guardado.IsFailure) return Result<LoteCreadoResult>.Failure(guardado.Error!);
        await _lotes.UpdateRutaAsync(lote.IdImportacionLote, idTenant, ruta, ct);
        // S91.4: registro de cambios del lote (vista previa + auditoría) junto al archivo.
        if (cambios is not null)
        {
            var registrado = await GuardarArchivoAsync(RutaCambios(ruta), JsonSerializer.SerializeToUtf8Bytes(cambios.Filas, JsonCambios), "application/json", ct);
            if (registrado.IsFailure) return Result<LoteCreadoResult>.Failure(registrado.Error!);
        }

        return Result<LoteCreadoResult>.Success(new LoteCreadoResult(
            lote.IdImportacionLote, estado,
            validacion.Filas.Count, validacion.Productos, validacion.Variantes,
            validacion.Nuevos, validacion.Existentes,
            totalErrores, totalAdvertencias,
            yaAplicado, incidencias.Take(100).ToList())
        { Cambios = cambios?.Resumen });
    }

    public async Task<Result<LoteDetalleResult>> GetLoteAsync(long id, int idTenant, CancellationToken ct = default)
    {
        var lote = await _lotes.GetByIdInTenantAsync(id, idTenant, ct);
        if (lote is null)
            return Result<LoteDetalleResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<LoteDetalleResult>.Success(new LoteDetalleResult(
            lote.IdImportacionLote, lote.Tipo, lote.NombreArchivo, lote.Estado,
            lote.Filas, lote.Productos, lote.Variantes, lote.Nuevos, lote.Existentes,
            lote.Errores, lote.Advertencias, lote.FecCrea, lote.FecAplicada,
            LeerIncidencias(lote.ResumenJson))
        { Cambios = LeerResumenCambios(lote.ResumenJson) });
    }

    public async Task<Result<CambiosPaginaResult>> GetCambiosAsync(
        long id, int idTenant, string? accion, int page, int pageSize, CancellationToken ct = default)
    {
        var lote = await _lotes.GetByIdInTenantAsync(id, idTenant, ct);
        if (lote is null)
            return Result<CambiosPaginaResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        page = page < 1 ? 1 : page;
        pageSize = pageSize is 10 or 25 or 50 or 100 ? pageSize : 25;
        var bytes = string.IsNullOrEmpty(lote.RutaArchivo) ? null : await LeerArchivoAsync(_archivos, RutaCambios(lote.RutaArchivo), ct);
        if (bytes is null)
            return Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(false, [], page, pageSize, 0));
        var filas = JsonSerializer.Deserialize<List<CambioFila>>(bytes, JsonCambios) ?? [];
        IEnumerable<CambioFila> q = filas;
        if (!string.IsNullOrWhiteSpace(accion))
            q = q.Where(f => string.Equals(f.Accion, accion, StringComparison.OrdinalIgnoreCase));
        var lista = q.ToList();
        return Result<CambiosPaginaResult>.Success(new CambiosPaginaResult(
            true, lista.Skip((page - 1) * pageSize).Take(pageSize).ToList(), page, pageSize, lista.Count));
    }

    /// <summary>
    /// S91.4: vista previa de cambios con las mismas reglas del SP v3. La usan la validación y el aplicador
    /// (que la recalcula para detectar SIN_CAMBIOS y LOTE_DESACTUALIZADO).
    /// </summary>
    public static async Task<CambiosImportacion> CalcularCambiosAsync(
        ResultadoValidacionImportacion validacion, IProductoImportSpRepositoryAsync comercial, int idTenant, CancellationToken ct)
    {
        var estado = await comercial.GetEstadoComercialAsync(idTenant, ct);
        var items = validacion.Filas.Select(f => (f.RowNumber, ProductoImportTvpMapper.Map(f.Row))).ToList();
        return ProductoImportacionComparador.Comparar(items, validacion.Catalogo, estado, DateOnly.FromDateTime(DateTime.Now));
    }

    /// <summary>S91.4: el JSON de cambios vive junto al .xlsx del lote ({id}.cambios.json).</summary>
    public static string RutaCambios(string rutaArchivo) => Path.ChangeExtension(rutaArchivo, ".cambios.json").Replace('\\', '/');

    private static string RutaLote(int idTenant, long idLote) => $"{PrefijoArchivos}{idTenant}/{DateTime.UtcNow:yyyy}/{idLote}.xlsx";

    /// <summary>
    /// S104: lee un archivo de importación del contenedor privado. Aquí la API sí tiene los bytes, porque procesa el Excel.
    /// <c>null</c> = sin ruta, o el storage no lo entrega (no existe o no se pudo leer).
    /// </summary>
    internal static async Task<byte[]?> LeerArchivoAsync(IStorageProvider storage, string? ruta, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(ruta)) return null;
        var abierto = await storage.DownloadAsync(new StorageObjectRef(StorageContainers.Private, ruta), ct);
        if (abierto.IsFailure) return null;
        await using var origen = abierto.Value;
        using var ms = new MemoryStream();
        await origen.CopyToAsync(ms, ct);
        return ms.ToArray();
    }

    private async Task<Result> GuardarArchivoAsync(string ruta, byte[] contenido, string contentType, CancellationToken ct)
    {
        using var ms = new MemoryStream(contenido, writable: false);
        var guardado = await _archivos.UploadAsync(new StorageObjectRef(StorageContainers.Private, ruta), ms, contentType, ct);
        return guardado.IsSuccess ? Result.Success() : Result.Failure(guardado.Error!);
    }

    public async Task<Result<ReporteLoteResult>> GetReporteAsync(long id, int idTenant, CancellationToken ct = default)
    {
        var lote = await _lotes.GetByIdInTenantAsync(id, idTenant, ct);
        if (lote is null)
            return Result<ReporteLoteResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404));
        var original = await LeerArchivoAsync(_archivos, lote.RutaArchivo, ct);
        if (original is null)
            return Result<ReporteLoteResult>.Failure(new Error("LOTE_SIN_ARCHIVO", "Archivo del lote no encontrado.").WithType(ErrorType.NotFound).WithHttpStatus(404));

        var outcomes = new Dictionary<int, ExcelRowOutcome>();
        foreach (var g in LeerIncidencias(lote.ResumenJson).GroupBy(i => i.Fila).Where(g => g.Key > 0))
        {
            var peor = g.Any(i => i.Severidad == "Error") ? ExcelOutcomeSeverity.Error : ExcelOutcomeSeverity.Warning;
            var mensajes = string.Join(" | ", g.Select(i => $"[{i.Columna}] {i.Mensaje}").Take(5));
            outcomes[g.Key] = new ExcelRowOutcome { Severity = peor, Message = mensajes };
        }
        using var ms = new MemoryStream(original, writable: false);
        using var anotado = _anotador.Annotate(ms, outcomes, new ExcelAnnotateOptions { SheetName = "Productos" });
        using var salida = new MemoryStream();
        await anotado.CopyToAsync(salida, ct);
        var nombre = Path.GetFileNameWithoutExtension(lote.NombreArchivo) + $"_reporte_{id}.xlsx";
        return Result<ReporteLoteResult>.Success(new(salida.ToArray(), nombre));
    }

    public async Task<Result<(IReadOnlyList<LoteItemResult> Items, int Total)>> ListarLotesAsync(int idTenant, int page, int pageSize, CancellationToken ct = default)
    {
        var (items, total) = await _lotes.SearchPagedByTenantAsync(idTenant, page, pageSize, ct);
        return Result<(IReadOnlyList<LoteItemResult>, int)>.Success((
            items.Select(l => new LoteItemResult(l.IdImportacionLote, l.Tipo, l.Estado, l.Filas, l.Errores, l.FecCrea, l.NombreArchivo, l.Advertencias, l.FecAplicada)
            { Modificados = LeerResumenCambios(l.ResumenJson)?.Modificados }).ToList(), total));
    }

    // ── Privados ──
    private async Task<Tenant?> GetTenantAsync(int idTenant, CancellationToken ct)
    {
        var r = await _tenants.GetByIdAsync(idTenant, ct);
        return r.IsSuccess ? r.Value : null;
    }

    private async Task<(IReadOnlyList<string> Unidades, IReadOnlyList<string> Bodegas, IReadOnlyList<string> Categorias, IReadOnlyList<string> Marcas)> CargarListasAsync(int idTenant, CancellationToken ct)
    {
        var unidades = (await _unidades.GetAllOrdenadoAsync(ct)).Select(u => u.Codigo).ToList();
        var bodegas = (await _bodegas.GetByTenantAsync(idTenant, true, ct)).Select(b => b.Codigo).ToList();
        var categorias = await _categorias.GetByTenantAsync(idTenant, true, ct);
        var porId = categorias.ToDictionary(c => c.IdCategoria);
        var rutas = new List<string>();
        foreach (var c in categorias)
        {
            if (c.IdPadre is { } pid && porId.TryGetValue(pid, out var padre))
                rutas.Add($"{padre.Nombre.Trim()} > {c.Nombre.Trim()}");
            else
                rutas.Add(c.Nombre.Trim());
        }
        var marcas = (await _marcas.GetByTenantAsync(idTenant, true, ct)).Select(m => m.Nombre).ToList();
        return (unidades, bodegas, rutas.Distinct().OrderBy(x => x).ToList(), marcas.Distinct().OrderBy(x => x).ToList());
    }

    private async Task<List<ExcelRow>> LeerFilasAsync(Stream stream, CancellationToken ct)
    {
        var lista = new List<ExcelRow>();
        await foreach (var row in _lector.ReadRowsAsync(stream,
            new ExcelReadOptions { SheetName = "Productos", CultureName = "es-CL", MaxRows = 20000, MaxBytes = MaxBytes }, ct))
        {
            lista.Add(row);
        }
        return lista;
    }

    private async Task<Result<LoteCreadoResult>> CrearLoteEstructuralAsync(
        int idTenant, int idUsuario, string fileName, string sha, byte[] contenido,
        string codigo, string mensaje, CancellationToken ct)
    {
        var incidencias = new List<IncidenciaLoteDto> { new(0, "", codigo, mensaje, "Error") };
        var lote = await _lotes.CreateAsync(new ImportacionLote
        {
            IdTenant = idTenant,
            Tipo = ImportacionLoteTipos.Productos,
            NombreArchivo = Path.GetFileName(fileName),
            Sha256 = sha,
            RutaArchivo = "",
            Estado = ImportacionLoteEstados.ConErrores,
            IdUsuario = idUsuario,
            Errores = 1,
            ResumenJson = JsonSerializer.Serialize(new { errores = 1, incidencias }),
        }, ct);
        var ruta = RutaLote(idTenant, lote.IdImportacionLote);
        var guardado = await GuardarArchivoAsync(ruta, contenido, ContentTypeXlsx, ct);
        if (guardado.IsFailure) return Result<LoteCreadoResult>.Failure(guardado.Error!);
        await _lotes.UpdateRutaAsync(lote.IdImportacionLote, idTenant, ruta, ct);
        return Result<LoteCreadoResult>.Success(new LoteCreadoResult(
            lote.IdImportacionLote, ImportacionLoteEstados.ConErrores,
            0, 0, 0, 0, 0, 1, 0, false, incidencias));
    }

    private static string ConstruirResumen(ResultadoValidacionImportacion v, List<IncidenciaLoteDto> incidencias,
        CambiosImportacion? cambios, int errores, int advertencias)
    {
        var porCodigo = incidencias
            .GroupBy(i => i.Severidad + ":" + i.Codigo)
            .ToDictionary(g => g.Key, g => g.Count());
        var recorte = incidencias.Take(MaxResumenIncidencias).ToList();
        var json = JsonSerializer.Serialize(new
        {
            filas = v.Filas.Count,
            productos = v.Productos,
            variantes = v.Variantes,
            nuevos = v.Nuevos,
            existentes = v.Existentes,
            errores,
            advertencias,
            porCodigo,
            incidencias = recorte,
            cambios = cambios?.Resumen,
            cambiosSha256 = cambios?.Sha256,
        });
        while (Encoding.UTF8.GetByteCount(json) > MaxResumenBytes && recorte.Count > 0)
        {
            recorte.RemoveRange(recorte.Count - Math.Min(50, recorte.Count), Math.Min(50, recorte.Count));
            json = JsonSerializer.Serialize(new
            {
                filas = v.Filas.Count,
                productos = v.Productos,
                variantes = v.Variantes,
                nuevos = v.Nuevos,
                existentes = v.Existentes,
                errores,
                advertencias,
                porCodigo,
                incidencias = recorte,
                cambios = cambios?.Resumen,
                cambiosSha256 = cambios?.Sha256,
            });
        }
        return json;
    }

    /// <summary>S91.4: resumen de cambios guardado al validar (null en lotes anteriores).</summary>
    public static ResumenCambios? LeerResumenCambios(string? resumenJson)
    {
        if (string.IsNullOrEmpty(resumenJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(resumenJson);
            return doc.RootElement.TryGetProperty("cambios", out var c) && c.ValueKind == JsonValueKind.Object
                ? c.Deserialize<ResumenCambios>()
                : null;
        }
        catch (JsonException) { return null; }
    }

    /// <summary>S91.4: huella de la vista previa guardada al validar (null en lotes anteriores).</summary>
    public static string? LeerHuellaCambios(string? resumenJson)
    {
        if (string.IsNullOrEmpty(resumenJson)) return null;
        try
        {
            using var doc = JsonDocument.Parse(resumenJson);
            return doc.RootElement.TryGetProperty("cambiosSha256", out var h) && h.ValueKind == JsonValueKind.String ? h.GetString() : null;
        }
        catch (JsonException) { return null; }
    }

    private static IReadOnlyList<IncidenciaLoteDto> LeerIncidencias(string? resumenJson)
    {
        if (string.IsNullOrEmpty(resumenJson)) return [];
        try
        {
            using var doc = JsonDocument.Parse(resumenJson);
            if (!doc.RootElement.TryGetProperty("incidencias", out var arr))
                return [];
            return arr.Deserialize<List<IncidenciaLoteDto>>() ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string SanearCodigo(string? codigo, int idTenant)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return $"T{idTenant}";
        var sb = new StringBuilder();
        foreach (var ch in codigo.Trim())
        {
            if (char.IsLetterOrDigit(ch) || ch is '-' or '_') sb.Append(ch);
        }
        return sb.Length == 0 ? $"T{idTenant}" : sb.ToString();
    }
}
