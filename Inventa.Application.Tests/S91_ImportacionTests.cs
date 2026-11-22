using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using CBP.Excel.Abstractions;
using CBP.Excel.Parsing;
using CBP.Results;
using CBP.Storage;
using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

// S91: YaAplicado en ValidarProductosAsync + listado extendido (servicio).
public sealed class S91_ImportacionTests
{
    private const int Tenant = 4;
    private const int Usuario = 3;
    private static readonly byte[] FakeXlsx = [0x50, 0x4B, 0x09, 0x09];

    private sealed class StubPlantillas : IExcelTemplateGenerator
    {
        public Stream GenerateTemplate<T>(ExcelTemplateOptions options) where T : class => throw new System.NotImplementedException();
        public Task<Stream> GenerateTemplateAsync<T>(ExcelTemplateOptions options, CancellationToken cancellationToken = default) where T : class => throw new System.NotImplementedException();
    }

    private sealed class StubLector : IExcelReader
    {
        private readonly ExcelRow _fila;
        public StubLector(ExcelRow fila) => _fila = fila;
        public IReadOnlyList<ExcelRow> ReadRows(Stream stream, ExcelReadOptions options) => [_fila];
        public IReadOnlyList<T> ReadRows<T>(Stream stream, ExcelReadOptions options) where T : class, new() => [];
        public async IAsyncEnumerable<ExcelRow> ReadRowsAsync(Stream stream, ExcelReadOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            yield return _fila;
            await Task.CompletedTask;
        }
        public async IAsyncEnumerable<T> ReadRowsAsync<T>(Stream stream, ExcelReadOptions options, [EnumeratorCancellation] CancellationToken cancellationToken = default) where T : class, new()
        {
            yield break;
        }
    }

    private sealed class StubInspector : IExcelSheetInspector
    {
        public ExcelSheetInfo Inspect(Stream stream, ExcelReadOptions options) => new()
        {
            SheetNames = ["Productos"],
            Headers = ["CodigoProducto *", "NombreProducto *", "Categoria *", "UnidadBase *",
                "CodigoVariante *", "NombreVariante *", "PrecioVenta *"],
            DataRowCount = 1
        };
    }

    private sealed class StubAnotador : IExcelResultAnnotator
    {
        public Stream Annotate(Stream original, IReadOnlyDictionary<int, ExcelRowOutcome> outcomes, ExcelAnnotateOptions options) => throw new System.NotImplementedException();
        public Task<Stream> AnnotateAsync(Stream original, IReadOnlyDictionary<int, ExcelRowOutcome> outcomes, ExcelAnnotateOptions options, CancellationToken cancellationToken = default) => throw new System.NotImplementedException();
    }

    private sealed class StubTenants : ITenantServiceAsync
    {
        private static Task<Result<TResult>> NiM<TResult>() => throw new System.NotImplementedException();
        public Task<Result<IReadOnlyList<Tenant>>> GetAllAsync(CancellationToken ct = default) => NiM<IReadOnlyList<Tenant>>();
        public Task<Result<Tenant?>> GetByIdAsync(int idTenant, CancellationToken ct = default) => NiM<Tenant?>();
        public Task<Result<Tenant?>> GetByCodigoAsync(string codigo, CancellationToken ct = default) => NiM<Tenant?>();
        public Task<Result<IReadOnlyList<Tenant>>> GetActivosAsync(CancellationToken ct = default) => NiM<IReadOnlyList<Tenant>>();
        public Task<Result<bool>> ExistsByCodigoAsync(string codigo, CancellationToken ct = default) => NiM<bool>();
        public Task<Result<int>> CountActivosAsync(CancellationToken ct = default) => NiM<int>();
    }

    private sealed class StubComercial : IProductoImportSpRepositoryAsync
    {
        public Task<Result<ImportacionResultadoSp>> AplicarAsync(int idTenant, int idUsuario, IReadOnlyList<ProductoImportTvpItem> filas, CancellationToken ct) => throw new System.NotImplementedException();
        public EstadoComercialImportacion Estado { get; set; } = new();
        public Task<EstadoComercialImportacion> GetEstadoComercialAsync(int idTenant, CancellationToken ct) => Task.FromResult(Estado);
    }

    private sealed class StubLotesSvc : IImportacionLoteRepositoryAsync
    {
        public bool YaAplicado { get; set; }
        public ImportacionLote? Creado { get; private set; }
        public bool SaveCalled { get; private set; }
        public IReadOnlyList<ImportacionLote> PageItems { get; set; } = [];
        public int PageTotal { get; set; }
        public Task<ImportacionLote> CreateAsync(ImportacionLote entity, CancellationToken ct = default)
        {
            entity.IdImportacionLote = 7;
            Creado = entity;
            return Task.FromResult(entity);
        }
        public Task UpdateRutaAsync(long id, int idTenant, string rutaArchivo, CancellationToken ct = default)
        {
            Creado!.RutaArchivo = rutaArchivo;
            return Task.CompletedTask;
        }
        public Task<ImportacionLote?> GetByIdInTenantAsync(long id, int idTenant, CancellationToken ct = default) => Task.FromResult(Creado);
        public Task<bool> ExistsAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default) => Task.FromResult(YaAplicado);
        public Task<ImportacionLote?> GetUltimoAplicadoByShaAsync(int idTenant, string tipo, string sha256, CancellationToken ct = default)
            => Task.FromResult(YaAplicado ? new ImportacionLote { IdImportacionLote = 3, IdTenant = idTenant, Estado = ImportacionLoteEstados.Aplicado, FecAplicada = new System.DateTime(2026, 10, 1, 12, 0, 0, System.DateTimeKind.Utc) } : null);
        public Task<(IReadOnlyList<ImportacionLote> Items, int Total)> SearchPagedByTenantAsync(int idTenant, int page, int pageSize, CancellationToken ct = default) => Task.FromResult((PageItems, PageTotal));
        public Task UpdateEstadoAsync(long id, int idTenant, string estado, CancellationToken ct = default) => Task.CompletedTask;
        public Task UpdateResultadoAsync(long id, int idTenant, string estado, string resumenJson, CancellationToken ct = default) => Task.CompletedTask;
    }

    // S104: los archivos de importación van al contenedor privado del storage (CBP.Storage).
    private sealed class StubArchivosSvc : StubStorageProvider
    {
        public bool SaveCalled => Subidos.Count > 0;
        public string? RutaGuardada => Subidos.Count > 0 ? Subidos[^1].Key : null;
        public void Quitar(string ruta) => Objetos.Remove(new StorageObjectRef(StorageContainers.Private, ruta));
    }

    private static ImportacionServiceAsync Servicio(StubLotesSvc lotes, StubArchivosSvc archivos, bool catalogoIgual = false)
    {
        // S91.4: catalogoIgual => el catálogo coincide exactamente con FilaValida (códigos como los resuelve el SP: UPPER(nombre)).
        var categorias = new StubCategoriaImportRepo { Datos = [new Categoria { IdCategoria = 1, IdTenant = Tenant, Codigo = catalogoIgual ? "ABARROTES" : "CAT-01", Nombre = "Abarrotes" }] };
        var marcas = new StubMarcaImportRepo { Datos = [new Marca { IdMarca = 1, IdTenant = Tenant, Codigo = catalogoIgual ? "FICCUS" : "FIC", Nombre = "Ficcus" }] };
        var unidades = new StubUnidadImportRepo { Datos = [new Unidad { IdUnidad = 1, Codigo = "UN", Nombre = "Unidad" }] };
        var bodegas = new StubBodegaImportRepo { Datos = [new Bodega { IdBodega = 1, IdTenant = Tenant, Codigo = "BOD-01", Nombre = "Central" }] };
        var proveedores = new StubProveedorImportRepo { Datos = [new Proveedor { IdProveedor = 1, IdTenant = Tenant, Codigo = "PROV-01" }] };
        var prods = new StubProdImportRepo();
        var vars = new StubVarProdImportRepo();
        var cods = new StubCodBarImportRepo();
        var comercial = new StubComercial();
        if (catalogoIgual)
        {
            var prod = new Prod { IdProd = 1, IdTenant = Tenant, Codigo = "PROD-001", Nombre = "Arroz Grano Largo", IdCategoria = 1, IdMarca = 1, IdUniBase = 1, Activo = true, VisibleStore = true };
            var variante = new VarProd { IdVarProd = 50, IdProd = 1, Codigo = "PROD-001-UN", Nombre = "Unidad", Prod = prod };
            var pres = new PresProd { IdPresProd = 100, IdVarProd = 50, IdUnidad = 1, Descrip = "x", VarProd = variante };
            prods.Datos = [prod];
            vars.Datos = [variante];
            cods.Datos = [new CodBar { IdCodBar = 1, IdPresProd = 100, Codigo = "5901234123457", TipoCod = 1, Principal = true, PresProd = pres }];
            comercial.Estado = new EstadoComercialImportacion
            {
                Presentaciones = new Dictionary<(long, int), long> { [(50L, 1)] = 100L },
                UltimoPrecio = new Dictionary<long, decimal> { [100L] = 9990m },
            };
        }
        var lector = new StubLector(FilaValida());
        var validador = new ProductoImportacionValidadorAsync(
            categorias, marcas, unidades, bodegas, prods, vars, cods, lector);
        return new ImportacionServiceAsync(
            new StubPlantillas(), lector, new StubInspector(), new StubAnotador(),
            validador, lotes, categorias, marcas, unidades, bodegas,
            new StubTenants(), archivos, comercial);
    }

    private static ExcelRow Fila(int n, Dictionary<string, string> base_, params (string Col, string Val)[] cambios)
    {
        var dict = new Dictionary<string, object?>();
        foreach (var (k, v) in base_) dict[ExcelValueParser.NormalizeHeader(k)] = v;
        foreach (var (c, v) in cambios) dict[ExcelValueParser.NormalizeHeader(c)] = v;
        return new ExcelRow { RowNumber = n, Values = dict };
    }

    private static ExcelRow FilaValida() => Fila(2, new Dictionary<string, string>
    {
        ["CodigoProducto *"] = "PROD-001",
        ["NombreProducto *"] = "Arroz Grano Largo",
        ["Categoria *"] = "Abarrotes",
        ["Marca"] = "Ficcus",
        ["UnidadBase *"] = "UN",
        ["Pesable (S/N)"] = "N",
        ["ManejaLote (S/N)"] = "N",
        ["PublicarEnTienda (S/N)"] = "S",
        ["Activo (S/N)"] = "S",
        ["CodigoVariante *"] = "PROD-001-UN",
        ["NombreVariante *"] = "Unidad",
        ["CodigoBarras"] = "5901234123457",
        ["PrecioVenta *"] = "9990",
    });

    [Fact]
    public async Task Validar_ArchivoYaAplicado_MarcaYaAplicadoYAdvierte()
    {
        var lotes = new StubLotesSvc { YaAplicado = true };
        var archivos = new StubArchivosSvc();
        var r = await Servicio(lotes, archivos).ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.True(r.Value.YaAplicado);
        Assert.Equal(ImportacionLoteEstados.Validado, r.Value.Estado);
        Assert.Equal(1, r.Value.Advertencias);
        // S91.4: el archivo trae cambios (producto nuevo) => advertencia citando el lote aplicado, no bloqueo.
        Assert.Equal("ARCHIVO_YA_APLICADO_CON_CAMBIOS", r.Value.Incidencias[0].Codigo);
        Assert.Contains("lote #3", r.Value.Incidencias[0].Mensaje);
        Assert.Equal(1, lotes.Creado!.Advertencias);
        Assert.True(archivos.SaveCalled);
    }

    [Fact]
    public async Task S914_Validar_SinCambios_EsErrorConErroresYCitaLoteAplicado()
    {
        var lotes = new StubLotesSvc { YaAplicado = true };
        var archivos = new StubArchivosSvc();
        var r = await Servicio(lotes, archivos, catalogoIgual: true).ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.Equal(ImportacionLoteEstados.ConErrores, r.Value.Estado);
        Assert.Equal(1, r.Value.Errores);
        Assert.Equal("SIN_CAMBIOS", r.Value.Incidencias[0].Codigo);
        Assert.Contains("idéntico al lote #3", r.Value.Incidencias[0].Mensaje);
        Assert.NotNull(r.Value.Cambios);
        Assert.False(r.Value.Cambios!.HayCambios);
        Assert.Equal(1, r.Value.Cambios.SinCambios);
        Assert.Contains("\"cambiosSha256\"", lotes.Creado!.ResumenJson);
    }

    [Fact]
    public async Task S914_Validar_ConCambios_GuardaJsonDeCambiosJuntoAlArchivo()
    {
        var lotes = new StubLotesSvc();
        var archivos = new StubArchivosSvc();
        var r = await Servicio(lotes, archivos).ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.Equal(1, r.Value.Cambios!.Nuevos);
        Assert.EndsWith("/7.cambios.json", archivos.RutaGuardada);
        Assert.Equal(ImportacionServiceAsync.RutaCambios(lotes.Creado!.RutaArchivo), archivos.RutaGuardada);
    }

    [Fact]
    public async Task S914_GetCambios_FiltraPaginaYNormalizaTamano()
    {
        var lotes = new StubLotesSvc();
        var archivos = new StubArchivosSvc();
        var svc = Servicio(lotes, archivos);
        await svc.ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        var nuevos = await svc.GetCambiosAsync(7, Tenant, "Nuevo", 1, 7, Xunit.TestContext.Current.CancellationToken);
        Assert.True(nuevos.Value.Disponible);
        Assert.Equal(1, nuevos.Value.Total);
        Assert.Equal(25, nuevos.Value.PageSize); // 7 no permitido => 25
        Assert.Equal("PROD-001", nuevos.Value.Items[0].CodigoProducto);
        var modificados = await svc.GetCambiosAsync(7, Tenant, "Modificado", 1, 10, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(0, modificados.Value.Total);
        var pagina2 = await svc.GetCambiosAsync(7, Tenant, null, 2, 10, Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(pagina2.Value.Items);
    }

    [Fact]
    public async Task S914_GetCambios_LoteSinJson_DisponibleFalse_YLoteInexistente404()
    {
        var lotes = new StubLotesSvc();
        var archivos = new StubArchivosSvc();
        var svc = Servicio(lotes, archivos);
        await svc.ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        archivos.Quitar(ImportacionServiceAsync.RutaCambios(lotes.Creado!.RutaArchivo));
        var r = await svc.GetCambiosAsync(7, Tenant, null, 1, 10, Xunit.TestContext.Current.CancellationToken);
        Assert.False(r.Value.Disponible);
        var vacio = await Servicio(new StubLotesSvc(), new StubArchivosSvc()).GetCambiosAsync(7, Tenant, null, 1, 10, Xunit.TestContext.Current.CancellationToken);
        Assert.Equal(404, vacio.Error!.HttpStatusCode);
    }

    [Fact]
    public async Task Validar_ArchivoNuevo_NoMarcaYaAplicado()
    {
        var lotes = new StubLotesSvc { YaAplicado = false };
        var r = await Servicio(lotes, new StubArchivosSvc()).ValidarProductosAsync(Tenant, Usuario, "lote.xlsx", FakeXlsx, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        Assert.False(r.Value.YaAplicado);
        Assert.Equal(0, r.Value.Advertencias);
        Assert.DoesNotContain(r.Value.Incidencias, i => i.Codigo.StartsWith("ARCHIVO_YA_APLICADO"));
    }

    [Fact]
    public async Task Listar_IncluyeNombreArchivoAdvertenciasFecAplicada()
    {
        var fec = new System.DateTime(2026, 10, 1, 12, 0, 0, System.DateTimeKind.Utc);
        var lotes = new StubLotesSvc
        {
            PageItems = [new ImportacionLote
            {
                IdImportacionLote = 5, IdTenant = Tenant, Tipo = "PRODUCTOS",
                NombreArchivo = "lote-5.xlsx", Estado = "Aplicado",
                Filas = 10, Errores = 0, Advertencias = 3, FecCrea = fec, FecAplicada = fec
            }],
            PageTotal = 1
        };
        var r = await Servicio(lotes, new StubArchivosSvc()).ListarLotesAsync(Tenant, 1, 20, Xunit.TestContext.Current.CancellationToken);
        Assert.True(r.IsSuccess);
        var item = Assert.Single(r.Value.Items);
        Assert.Equal("lote-5.xlsx", item.NombreArchivo);
        Assert.Equal(3, item.Advertencias);
        Assert.Equal(fec, item.FecAplicada);
        Assert.Equal(1, r.Value.Total);
    }
}

