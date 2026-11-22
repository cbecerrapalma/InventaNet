using System.Data;
using Microsoft.Data.SqlClient;
using CBP.Data.Asynchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S88.2 (movido desde Application.Async): repositorio para invocar
/// dbo.sp_ImportarProductos vía CBP ExecuteSPRawAsync.
/// Cero SQL en strings C#; SP por nombre + CommandType.StoredProcedure.
/// Recibe ítems TVP ya resueltos (ver <see cref="ProductoImportTvpItem"/>).
/// </summary>
public sealed class ProductoImportSpRepositoryAsync : IProductoImportSpRepositoryAsync
{
    // S91: umbral y timeout extendido para lotes grandes.
    // Gate0 (00-GATE0.md): 20000 filas → 5.1 s server. Sobre 5000 filas se otorga
    // 120 s SOLO a esta llamada; lotes chicos conservan el default (30 s CBP).
    private const int LoteGrandeUmbralFilas = 5000;
    private const int TimeoutLoteGrandeSeg = 120;

    private readonly InventaDbContext _ctx;

    public ProductoImportSpRepositoryAsync(InventaDbContext ctx)
    {
        _ctx = ctx;
    }

    public async Task<Result<ImportacionResultadoSp>> AplicarAsync(
        int idTenant,
        int idUsuario,
        IReadOnlyList<ProductoImportTvpItem> filas,
        CancellationToken ct)
    {
        if (idTenant <= 0) return Result<ImportacionResultadoSp>.Failure(new Error("IDTENANT_INVALIDO", "IdTenant inválido"));
        if (idUsuario <= 0) return Result<ImportacionResultadoSp>.Failure(new Error("IDUSUARIO_INVALIDO", "IdUsuario inválido"));
        if (filas == null || filas.Count == 0) return Result<ImportacionResultadoSp>.Failure(new Error("SIN_FILAS", "Sin filas para aplicar"));

        // S91: timeout extendido solo para lotes grandes; se restaura en finally.
        var timeoutExtendido = filas.Count > LoteGrandeUmbralFilas;
        int? timeoutPrevio = null;
        if (timeoutExtendido)
        {
            timeoutPrevio = _ctx.Database.GetCommandTimeout();
            _ctx.Database.SetCommandTimeout(TimeoutLoteGrandeSeg);
        }

        try
        {
            var tvp = BuildTvp(filas);

            var pIdTenant = new SqlParameter("@IdTenant", idTenant);
            var pIdUsuario = new SqlParameter("@IdUsuario", idUsuario);
            var pItems = new SqlParameter("@Items", SqlDbType.Structured)
            {
                TypeName = "dbo.ProductoImportItemType",
                Value = tvp
            };

            // Parámetros de salida
            var pProductosNuevos = new SqlParameter("@ProductosNuevos", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pProductosExist = new SqlParameter("@ProductosExist", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pVariantesNuevas = new SqlParameter("@VariantesNuevas", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pVariantesExist = new SqlParameter("@VariantesExist", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pPresentacionesNew = new SqlParameter("@PresentacionesNew", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pPreciosCreados = new SqlParameter("@PreciosCreados", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pCodBarrasCreados = new SqlParameter("@CodBarrasCreados", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pErrores = new SqlParameter("@Errores", SqlDbType.Int) { Direction = ParameterDirection.Output };
            var pResumenJson = new SqlParameter("@ResumenJson", SqlDbType.NVarChar, -1) { Direction = ParameterDirection.Output };

            var result = await RawQueryRepositoryFactoryAsync.Create(_ctx).ExecuteSPRawAsync(
                "dbo.sp_ImportarProductos",
                ct,
                pIdTenant, pIdUsuario, pItems,
                pProductosNuevos, pProductosExist, pVariantesNuevas, pVariantesExist,
                pPresentacionesNew, pPreciosCreados, pCodBarrasCreados, pErrores, pResumenJson);

            if (!result.IsSuccess)
            {
                var err = result.Error!;
                // CBP envuelve errores de SP en SP_EXECUTION_ERROR con detalles SqlErrorNumber/SqlErrorMessage
                if (err.Code == "SP_EXECUTION_ERROR" && err.Details != null)
                {
                    var knownCodes = new[] { "CATEGORIA_NO_EXISTE", "CATEGORIA_NO_RESUELTA", "MARCA_NO_EXISTE", "MARCA_NO_RESUELTA", "UNIDAD_NO_EXISTE", "PROVEEDOR_NO_EXISTE", "BODEGA_NO_EXISTE", "CODBAR_DUPLICADO", "CODBAR_EXISTE" };
                    if (err.Details.TryGetValue("SqlErrorMessage", out var sqlMsgObj) && sqlMsgObj is string sqlMsg)
                    {
                        foreach (var code in knownCodes)
                        {
                            if (sqlMsg.Contains(code))
                                return Result<ImportacionResultadoSp>.Failure(new Error(code, sqlMsg));
                        }
                    }
                }
                return Result<ImportacionResultadoSp>.Failure(err);
            }

            var res = new ImportacionResultadoSp
            {
                ProductosNuevos = (int)pProductosNuevos.Value!,
                ProductosExistentes = (int)pProductosExist.Value!,
                VariantesNuevas = (int)pVariantesNuevas.Value!,
                VariantesExistentes = (int)pVariantesExist.Value!,
                PresentacionesNuevas = (int)pPresentacionesNew.Value!,
                PreciosCreados = (int)pPreciosCreados.Value!,
                CodigosBarraCreados = (int)pCodBarrasCreados.Value!,
                Errores = (int)pErrores.Value!,
                ResumenJson = pResumenJson.Value?.ToString() ?? "{}"
            };

            return Result<ImportacionResultadoSp>.Success(res);
        }
        catch (Microsoft.Data.SqlClient.SqlException ex)
        {
            return HandleSqlException(ex);
        }
        catch (Microsoft.EntityFrameworkCore.DbUpdateException ex) when (ex.InnerException is Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            return HandleSqlException(sqlEx);
        }
        catch (Exception ex)
        {
            return Result<ImportacionResultadoSp>.Failure(new Error("SP_ERROR", $"Error invocando sp_ImportarProductos: {ex.Message}"));
        }
        finally
        {
            if (timeoutExtendido) _ctx.Database.SetCommandTimeout(timeoutPrevio);
        }
    }

    public async Task<EstadoComercialImportacion> GetEstadoComercialAsync(int idTenant, CancellationToken ct)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));

        var pres = await _ctx.Presentaciones.AsNoTracking()
            .Where(p => p.VarProd.Prod.IdTenant == idTenant)
            .Select(p => new { p.IdPresProd, p.IdVarProd, p.IdUnidad })
            .ToListAsync(ct);
        var precios = await _ctx.Precios.AsNoTracking()
            .Where(x => x.PresProd.VarProd.Prod.IdTenant == idTenant)
            .Select(x => new { x.IdPresProd, Id = x.IdPrecio, x.FecVig, x.Valor })
            .ToListAsync(ct);
        var ofertas = await _ctx.Ofertas.AsNoTracking()
            .Where(x => x.Activo && x.PresProd.VarProd.Prod.IdTenant == idTenant)
            .Select(x => new { x.IdPresProd, x.PrecioOferta, x.FechaDesde })
            .ToListAsync(ct);

        return new EstadoComercialImportacion
        {
            Presentaciones = pres
                .GroupBy(p => (p.IdVarProd, p.IdUnidad))
                .ToDictionary(g => g.Key, g => g.Min(p => p.IdPresProd)),
            UltimoPrecio = precios
                .GroupBy(x => x.IdPresProd)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.FecVig).ThenByDescending(x => x.Id).First().Valor),
            OfertasActivas = ofertas.Select(o => (o.IdPresProd, o.PrecioOferta, o.FechaDesde)).ToHashSet(),
        };
    }

    private static Result<ImportacionResultadoSp> HandleSqlException(Microsoft.Data.SqlClient.SqlException ex)
    {
        // Extraer código de error conocido del mensaje del SP
        var knownCodes = new[] { "CATEGORIA_NO_EXISTE", "CATEGORIA_NO_RESUELTA", "MARCA_NO_EXISTE", "MARCA_NO_RESUELTA", "UNIDAD_NO_EXISTE", "PROVEEDOR_NO_EXISTE", "BODEGA_NO_EXISTE", "CODBAR_DUPLICADO", "CODBAR_EXISTE" };
        var msg = ex.Message;
        foreach (var code in knownCodes)
        {
            if (msg.Contains(code))
                return Result<ImportacionResultadoSp>.Failure(new Error(code, msg));
        }
        return Result<ImportacionResultadoSp>.Failure(new Error("SP_ERROR", $"Error invocando sp_ImportarProductos: {msg}"));
    }

    private static DataTable BuildTvp(IReadOnlyList<ProductoImportTvpItem> filas)
    {
        // El orden ordinal DEBE coincidir con dbo.ProductoImportItemType v3 (S91.4: + Activo al final)
        // (los TVP Structured mapean por posición, no por nombre).
        var dt = new DataTable();
        dt.Columns.Add("CodigoProducto", typeof(string));
        dt.Columns.Add("NombreProducto", typeof(string));
        dt.Columns.Add("CategoriaPadre", typeof(string));
        dt.Columns.Add("Categoria", typeof(string));
        dt.Columns.Add("Marca", typeof(string));
        dt.Columns.Add("UnidadBase", typeof(string));
        dt.Columns.Add("Pesable", typeof(string));
        dt.Columns.Add("ManejaLote", typeof(string));
        dt.Columns.Add("DiasVencimiento", typeof(int));
        dt.Columns.Add("PublicarEnTienda", typeof(string));
        dt.Columns.Add("CodigoVariante", typeof(string));
        dt.Columns.Add("NombreVariante", typeof(string));
        dt.Columns.Add("CodigoBarras", typeof(string));
        dt.Columns.Add("TipoCod", typeof(byte));
        dt.Columns.Add("CodigosAdicionales", typeof(string));
        dt.Columns.Add("PrecioVenta", typeof(decimal));
        dt.Columns.Add("PrecioOferta", typeof(decimal));
        dt.Columns.Add("OfertaDesde", typeof(DateTime));
        dt.Columns.Add("OfertaHasta", typeof(DateTime));
        dt.Columns.Add("PrecioMayorista", typeof(decimal));
        dt.Columns.Add("CostoCompra", typeof(decimal));
        dt.Columns.Add("Proveedor", typeof(string));
        dt.Columns.Add("StockInicial", typeof(int));
        dt.Columns.Add("Bodega", typeof(string));
        dt.Columns.Add("Activo", typeof(string));

        dt.BeginLoadData();
        foreach (var f in filas)
        {
            dt.Rows.Add(
                f.CodigoProducto,
                f.NombreProducto,
                string.IsNullOrWhiteSpace(f.CategoriaPadre) ? DBNull.Value : f.CategoriaPadre,
                f.Categoria,
                string.IsNullOrWhiteSpace(f.Marca) ? DBNull.Value : f.Marca,
                f.UnidadBase,
                string.IsNullOrWhiteSpace(f.Pesable) ? DBNull.Value : f.Pesable,
                string.IsNullOrWhiteSpace(f.ManejaLote) ? DBNull.Value : f.ManejaLote,
                f.DiasVencimiento.HasValue ? f.DiasVencimiento.Value : DBNull.Value,
                string.IsNullOrWhiteSpace(f.PublicarEnTienda) ? DBNull.Value : f.PublicarEnTienda,
                f.CodigoVariante,
                f.NombreVariante,
                string.IsNullOrWhiteSpace(f.CodigoBarras) ? DBNull.Value : f.CodigoBarras,
                f.TipoCod.HasValue ? f.TipoCod.Value : DBNull.Value,
                string.IsNullOrWhiteSpace(f.CodigosAdicionales) ? DBNull.Value : f.CodigosAdicionales,
                f.PrecioVenta,
                f.PrecioOferta.HasValue ? f.PrecioOferta.Value : DBNull.Value,
                f.OfertaDesde.HasValue ? f.OfertaDesde.Value : DBNull.Value,
                f.OfertaHasta.HasValue ? f.OfertaHasta.Value : DBNull.Value,
                f.PrecioMayorista.HasValue ? f.PrecioMayorista.Value : DBNull.Value,
                f.CostoCompra.HasValue ? f.CostoCompra.Value : DBNull.Value,
                string.IsNullOrWhiteSpace(f.Proveedor) ? DBNull.Value : f.Proveedor,
                f.StockInicial,
                string.IsNullOrWhiteSpace(f.Bodega) ? DBNull.Value : f.Bodega,
                string.IsNullOrWhiteSpace(f.Activo) ? DBNull.Value : f.Activo
            );
        }
        dt.EndLoadData();
        return dt;
    }
}

/// <summary>
/// S88: Contrato del repositorio de importación vía SP.
/// </summary>
public interface IProductoImportSpRepositoryAsync
{
    Task<Result<ImportacionResultadoSp>> AplicarAsync(
        int idTenant,
        int idUsuario,
        IReadOnlyList<ProductoImportTvpItem> filas,
        CancellationToken ct);

    /// <summary>
    /// S91.4: estado comercial vigente del tenant para la vista previa de cambios de la importación,
    /// con la MISMA regla que dbo.sp_ImportarProductos: última fila por FecVig DESC, Id DESC (sin filtrar
    /// por fecha ni por Activo); compra por (proveedor, presentación); ofertas activas.
    /// </summary>
    Task<EstadoComercialImportacion> GetEstadoComercialAsync(int idTenant, CancellationToken ct);
}

/// <summary>S91.4: instantánea comercial (precios/ofertas/presentaciones) para comparar una importación.</summary>
public sealed class EstadoComercialImportacion
{
    /// <summary>(IdVarProd, IdUnidad) → IdPresProd.</summary>
    public IReadOnlyDictionary<(long IdVarProd, int IdUnidad), long> Presentaciones { get; init; } = new Dictionary<(long, int), long>();
    public IReadOnlyDictionary<long, decimal> UltimoPrecio { get; init; } = new Dictionary<long, decimal>();
    public IReadOnlySet<(long IdPresProd, decimal PrecioOferta, DateOnly FechaDesde)> OfertasActivas { get; init; } = new HashSet<(long, decimal, DateOnly)>();
}

/// <summary>
/// S88: Resultado de la ejecución del SP sp_ImportarProductos.
/// </summary>
public sealed class ImportacionResultadoSp
{
    public int ProductosNuevos { get; set; }
    public int ProductosExistentes { get; set; }
    public int VariantesNuevas { get; set; }
    public int VariantesExistentes { get; set; }
    public int PresentacionesNuevas { get; set; }
    public int PreciosCreados { get; set; }
    public int CodigosBarraCreados { get; set; }
    public int Errores { get; set; }
    public string ResumenJson { get; set; } = "{}";
}
