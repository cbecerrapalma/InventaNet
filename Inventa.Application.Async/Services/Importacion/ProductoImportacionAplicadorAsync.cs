using CBP.Results;
using CBP.Results.Errors;
using CBP.Excel.Abstractions;
using CBP.Storage;
using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services.Importacion;

/// <summary>
/// S88: Aplicador de importación validada — re-valida, genera EAN-13 internos,
/// invoca SP set-based, fase de stock vía sp_AjustarInventario, actualiza lote.
/// </summary>
public sealed class ProductoImportacionAplicadorAsync : IProductoImportacionAplicadorAsync
{
    private readonly IImportacionLoteRepositoryAsync _lotes;
    private readonly IStorageProvider _archivos;
    private readonly ProductoImportacionValidadorAsync _validador;
    private readonly IProductoImportSpRepositoryAsync _spRepo;
    private readonly IStockRepositoryAsync _stockRepo;
    private readonly IProdRepositoryAsync _prodRepo;
    private readonly IVarProdRepositoryAsync _varRepo;
    private readonly IPresProdRepositoryAsync _presRepo;
    private readonly IBodegaRepositoryAsync _bodegaRepo;
    private readonly ICatalogoEdicionRepositoryAsync _transaccion;

    public ProductoImportacionAplicadorAsync(
        IImportacionLoteRepositoryAsync lotes,
        IStorageProvider archivos,
        ProductoImportacionValidadorAsync validador,
        IProductoImportSpRepositoryAsync spRepo,
        IStockRepositoryAsync stockRepo,
        IProdRepositoryAsync prodRepo,
        IVarProdRepositoryAsync varRepo,
        IPresProdRepositoryAsync presRepo,
        IBodegaRepositoryAsync bodegaRepo,
        ICatalogoEdicionRepositoryAsync transaccion)
    {
        _lotes = lotes;
        _archivos = archivos;
        _validador = validador;
        _spRepo = spRepo;
        _stockRepo = stockRepo;
        _prodRepo = prodRepo;
        _varRepo = varRepo;
        _presRepo = presRepo;
        _bodegaRepo = bodegaRepo;
        _transaccion = transaccion;
    }

    public async Task<Result<AplicarImportacionResult>> AplicarAsync(
        long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct)
    {
        // 1. Obtener lote y verificar estado
        var lote = await _lotes.GetByIdInTenantAsync(idImportacionLote, idTenant, ct);
        if (lote is null)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_NO_EXISTE", "Lote no existe.").WithType(ErrorType.NotFound).WithHttpStatus(404));

        // S91: Aplicado/Fallido → 409 (idempotencia); otros estados no aplicables → 400.
        if (lote.Estado == ImportacionLoteEstados.Aplicado)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_YA_APLICADO", "El lote ya fue aplicado; no se puede aplicar dos veces.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        if (lote.Estado == ImportacionLoteEstados.Fallido)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_FALLIDO", "El lote está en estado Fallido; valide un archivo corregido.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        if (lote.Estado != ImportacionLoteEstados.Validado && lote.Estado != ImportacionLoteEstados.ConErrores)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_ESTADO_INVALIDO", $"Lote en estado '{lote.Estado}'; solo 'Validado' o 'ConErrores' se pueden aplicar.").WithType(ErrorType.Validation).WithHttpStatus(400));

        if (lote.Tipo != ImportacionLoteTipos.Productos)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_TIPO_INVALIDO", "Solo lotes tipo PRODUCTOS son soportados.").WithType(ErrorType.Validation).WithHttpStatus(400));

        // 2. Leer archivo original y re-validar
        var archivo = await ImportacionServiceAsync.LeerArchivoAsync(_archivos, lote.RutaArchivo, ct);
        if (archivo is null)
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_SIN_ARCHIVO", "Archivo del lote no encontrado en almacenamiento.").WithType(ErrorType.NotFound).WithHttpStatus(404));

        List<ExcelRow> filasExcel;
        try
        {
            using var ms = new MemoryStream(archivo, writable: false);
            filasExcel = (await _validador.LeerFilasAsync(ms, ct)).ToList();
        }
        catch (Exception ex)
        {
            return Result<AplicarImportacionResult>.Failure(new Error("ARCHIVO_CORRUPTO", $"No se pudo releer el archivo: {ex.Message}").WithType(ErrorType.InternalError).WithHttpStatus(500));
        }

        var validacion = await _validador.ValidarAsync(filasExcel, idTenant, ct);

        // 3. Si hay errores duros, fallar (el lote ya está en ConErrores, pero al aplicar exigimos Validado limpio)
        if (validacion.Errores.Count > 0)
        {
            await _lotes.UpdateEstadoAsync(idImportacionLote, idTenant, ImportacionLoteEstados.Fallido, ct);
            return Result<AplicarImportacionResult>.Failure(new Error("VALIDACION_FALLIDA", "El lote contiene errores de validación; corríjalos y vuelva a validar.").WithType(ErrorType.Validation).WithHttpStatus(400));
        }

        // 3b. S91.4: recalcular la vista previa con el catálogo actual (mismas reglas que el SP v3).
        var cambios = await ImportacionServiceAsync.CalcularCambiosAsync(validacion, _spRepo, idTenant, ct);
        if (!cambios.Resumen.HayCambios)
            return Result<AplicarImportacionResult>.Failure(new Error("SIN_CAMBIOS", "El catálogo ya coincide con este archivo; no hay nada que aplicar.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var huellaValidada = ImportacionServiceAsync.LeerHuellaCambios(lote.ResumenJson);
        if (huellaValidada is not null && !string.Equals(huellaValidada, cambios.Sha256, StringComparison.Ordinal))
            return Result<AplicarImportacionResult>.Failure(new Error("LOTE_DESACTUALIZADO", "El catálogo cambió desde que validaste este archivo. Vuelve a subirlo para ver los cambios actualizados.").WithType(ErrorType.Conflict).WithHttpStatus(409));

        // 4. S92 K7: el código interno de variantes NUEVAS sin CodigoBarras lo genera dbo.sp_ImportarProductos
        //    (paso 3: MAX de la BD, TipoCod = 3). En existentes, vacío conserva (S91.4). C# ya no genera códigos.
        var filasConEan = validacion.Filas.Select(f => f.Row).ToList();

        // 5–7. S132: catálogo, stock inicial y estado del lote en UNA transacción (todo o nada).
        var catalogoFallo = false;
        var resultado = await _transaccion.EnTransaccionAsync<Result<AplicarImportacionResult>>(async t =>
        {
            // 5. Invocar SP dbo.sp_ImportarProductos (filas mapeadas a TVP resuelto)
            var spResult = await _spRepo.AplicarAsync(idTenant, idUsuario, filasConEan.Select(ProductoImportTvpMapper.Map).ToList(), t);
            if (!spResult.IsSuccess)
            {
                catalogoFallo = true;
                return (Result<AplicarImportacionResult>.Failure(spResult.Error!), false);
            }

            // 6. Fase STOCK: ejecutar sp_AjustarInventario para StockInicial > 0
            var stockResult = await EjecutarFaseStockAsync(idTenant, idUsuario, filasConEan, t);
            if (!stockResult.IsSuccess)
            {
                return (Result<AplicarImportacionResult>.Failure(
                    new Error("STOCK_INICIAL_FALLIDO", $"No se pudo cargar el stock inicial y se revirtió el lote completo: no se aplicó ningún cambio. Revisa bodegas y cantidades y vuelve a aplicar. Detalle: {stockResult.Error!.Message}")
                        .WithType(ErrorType.Conflict).WithHttpStatus(409)), false);
            }

            // 7. Actualizar lote con resultados
            var resumen = ConstruirResumenAplicacion(spResult.Value, stockResult.Value);
            await _lotes.UpdateResultadoAsync(idImportacionLote, idTenant, ImportacionLoteEstados.Aplicado,
                FusionarResumen(lote.ResumenJson, resumen), t);

            return (Result<AplicarImportacionResult>.Success(new AplicarImportacionResult
            {
                IdImportacionLote = idImportacionLote,
                Estado = ImportacionLoteEstados.Aplicado,
                ProductosNuevos = spResult.Value.ProductosNuevos,
                ProductosExistentes = spResult.Value.ProductosExistentes,
                VariantesNuevas = spResult.Value.VariantesNuevas,
                VariantesExistentes = spResult.Value.VariantesExistentes,
                PresentacionesNuevas = spResult.Value.PresentacionesNuevas,
                PreciosCreados = spResult.Value.PreciosCreados,
                CodigosBarraCreados = spResult.Value.CodigosBarraCreados,
                StockAjustado = stockResult.Value,
                ResumenJson = resumen
            }), true);
        }, ct);

        if (catalogoFallo)
            await _lotes.UpdateEstadoAsync(idImportacionLote, idTenant, ImportacionLoteEstados.Fallido, ct);

        return resultado;
    }

    private async Task<Result<int>> EjecutarFaseStockAsync(
        int idTenant, int idUsuario, IReadOnlyList<ProductoImportRow> filas, CancellationToken ct)
    {
        var itemsConStock = filas
            .Where(f => int.TryParse(f.StockInicial, out var si) && si > 0 && !string.IsNullOrWhiteSpace(f.Bodega))
            .GroupBy(f => f.Bodega)
            .ToList();

        var totalAjustado = 0;

        foreach (var grupoBodega in itemsConStock)
        {
            var bodega = await _bodegaRepo.GetByCodigoAsync(idTenant, grupoBodega.Key, ct);
            if (bodega is null)
            {
                var primera = grupoBodega.First();
                return Result<int>.Failure(new Error("STOCK_INICIAL_SIN_DESTINO", $"Fila {primera.CodigoProducto}/{primera.CodigoVariante}: no se encontró la bodega '{grupoBodega.Key}' para cargar el stock inicial.").WithType(ErrorType.Conflict).WithHttpStatus(409));
            }

            var ajustes = new List<AjusteItemInput>();
            foreach (var f in grupoBodega)
            {
                // Buscar la presentación creada (unidad base del producto)
                var prodResult = await _prodRepo.GetByCodigoAsync(idTenant, f.CodigoProducto, ct);
                if (prodResult is null)
                    return Result<int>.Failure(new Error("STOCK_INICIAL_SIN_DESTINO", $"Fila {f.CodigoProducto}/{f.CodigoVariante}: no se encontró el producto para cargar el stock inicial.").WithType(ErrorType.Conflict).WithHttpStatus(409));
                var varResult = await _varRepo.GetByProductoAsync(prodResult.IdProd, true, ct);
                var variante = varResult.FirstOrDefault(v => v.Codigo == f.CodigoVariante);
                if (variante is null)
                    return Result<int>.Failure(new Error("STOCK_INICIAL_SIN_DESTINO", $"Fila {f.CodigoProducto}/{f.CodigoVariante}: no se encontró la variante para cargar el stock inicial.").WithType(ErrorType.Conflict).WithHttpStatus(409));
                
                // Buscar presentación por variante y unidad base
                var presResult = await _presRepo.GetByVarianteAsync(variante.IdVarProd, true, ct);
                var pres = presResult.FirstOrDefault(p => p.IdUnidad == prodResult.IdUniBase);
                if (pres is null)
                    return Result<int>.Failure(new Error("STOCK_INICIAL_SIN_DESTINO", $"Fila {f.CodigoProducto}/{f.CodigoVariante}: no se encontró la presentación de la unidad base para cargar el stock inicial.").WithType(ErrorType.Conflict).WithHttpStatus(409));

                var stockInicial = int.TryParse(f.StockInicial, out var si) ? si : 0;
                ajustes.Add(new AjusteItemInput(pres.IdPresProd, null, stockInicial, decimal.TryParse(f.CostoCompra, out var c) ? c : 0m));
            }

            if (ajustes.Count > 0)
            {
                var input = new AjustarInventarioInput(idTenant, bodega.IdBodega, ajustes, idUsuario);
                var spRes = await _stockRepo.AjustarInventarioAsync(SpInputMapper.ToCmd(input), ct);
                if (!spRes.IsSuccess)
                    return Result<int>.Failure(spRes.Error!);
                totalAjustado += ajustes.Count;
            }
        }

        return Result<int>.Success(totalAjustado);
    }

    /// <summary>
    /// S91.2: conserva el resumen de validación del lote (conteos e incidencias, que alimentan el detalle y el
    /// reporte) y agrega el resultado de la aplicación bajo "aplicacion"; antes se sobrescribía y se perdían.
    /// </summary>
    private static string FusionarResumen(string? resumenValidacion, string resumenAplicacion)
    {
        System.Text.Json.Nodes.JsonObject raiz;
        try
        {
            raiz = System.Text.Json.Nodes.JsonNode.Parse(string.IsNullOrEmpty(resumenValidacion) ? "{}" : resumenValidacion)
                as System.Text.Json.Nodes.JsonObject ?? new System.Text.Json.Nodes.JsonObject();
        }
        catch (System.Text.Json.JsonException)
        {
            raiz = new System.Text.Json.Nodes.JsonObject();
        }
        raiz["aplicacion"] = System.Text.Json.Nodes.JsonNode.Parse(resumenAplicacion);
        return raiz.ToJsonString();
    }

    private string ConstruirResumenAplicacion(ImportacionResultadoSp spRes, int stockAjustado)
    {
        return System.Text.Json.JsonSerializer.Serialize(new
        {
            spRes.ProductosNuevos,
            spRes.ProductosExistentes,
            spRes.VariantesNuevas,
            spRes.VariantesExistentes,
            spRes.PresentacionesNuevas,
            spRes.PreciosCreados,
            spRes.CodigosBarraCreados,
            spRes.Errores,
            StockAjustado = stockAjustado,
            spRes.ResumenJson
        });
    }
}

/// <summary>
/// S88: Contrato del aplicador de importación.
/// </summary>
public interface IProductoImportacionAplicadorAsync
{
    Task<Result<AplicarImportacionResult>> AplicarAsync(
        long idImportacionLote, int idTenant, int idUsuario, CancellationToken ct);
}

/// <summary>
/// S88: Resultado de la aplicación de importación.
/// </summary>
public sealed class AplicarImportacionResult
{
    public long IdImportacionLote { get; set; }
    public string Estado { get; set; } = "";
    public int ProductosNuevos { get; set; }
    public int ProductosExistentes { get; set; }
    public int VariantesNuevas { get; set; }
    public int VariantesExistentes { get; set; }
    public int PresentacionesNuevas { get; set; }
    public int PreciosCreados { get; set; }
    public int CodigosBarraCreados { get; set; }
    public int StockAjustado { get; set; }
    public string ResumenJson { get; set; } = "{}";
}