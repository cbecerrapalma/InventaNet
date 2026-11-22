using CBP.Results;
using Inventa.Application.Async.Services.Catalogo;
using Inventa.Application.Async.Services.Importacion;
using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Xunit;

namespace Inventa.Application.Tests;

/// <summary>
/// S94 K7: reglas puras de la edición del catálogo (iguales a dbo.sp_ImportarProductos v5) y el servicio
/// único de escritura con dobles en memoria (sin BD). La integración real está en Inventa.Data.Tests (S94_CatalogoEdicionDataTests).
/// </summary>
public sealed class S94_CatalogoEdicionTests
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;
    private const int Tenant = 4;

    // ── Reglas ──

    [Theory]
    [InlineData("Polera", "Talla 4", "Polera / Talla 4")]
    [InlineData("Arroz", "1 kg", "Arroz / 1 kg")]
    public void Descripcion_EsProductoBarraVariante(string prod, string var, string esperado) =>
        Assert.Equal(esperado, CatalogoEdicionReglas.Descripcion(prod, var));

    [Fact]
    public void Descripcion_SeRecortaA100_ComoLeftDelSp()
    {
        var d = CatalogoEdicionReglas.Descripcion(new string('P', 95), "Talla XL");
        Assert.Equal(100, d.Length);
        Assert.Equal(new string('P', 95) + " / Ta", d);
    }

    [Theory]
    [InlineData(null, "2004000000013")]                 // primer interno del tenant
    [InlineData("2004000000013", "2004000000020")]      // MAX + 1, con DV EAN-13
    [InlineData("2005000000017", "2004000000013")]      // otro prefijo no cuenta
    public void SiguienteInterno_MaxMasUno_ConDv(string? mayor, string esperado)
    {
        Assert.Equal(esperado, CatalogoEdicionReglas.SiguienteInterno(Tenant, mayor));
        Assert.True(EanCodigoValidator.EsEstandar(esperado, out _));
    }

    [Theory]
    [InlineData("7801234567894", TipoCodigoAlta.Normal, (byte)1)]
    [InlineData("96385074", TipoCodigoAlta.Normal, (byte)1)]       // EAN-8
    [InlineData("036000291452", TipoCodigoAlta.Normal, (byte)1)]   // UPC-A
    [InlineData("12345678901", TipoCodigoAlta.Proveedor, (byte)4)]
    public void ClasificarCodigo_Validos(string codigo, TipoCodigoAlta tipo, byte tipoCod)
    {
        var (tc, error) = CatalogoEdicionReglas.ClasificarCodigo(codigo, tipo, Tenant);
        Assert.Null(error);
        Assert.Equal(tipoCod, tc);
    }

    [Theory]
    [InlineData("7801234567890", TipoCodigoAlta.Normal, "dígito verificador")]
    [InlineData("2004000000020", TipoCodigoAlta.Normal, "reservado")]
    [InlineData("12AB", TipoCodigoAlta.Proveedor, "solo admite dígitos")]
    [InlineData("12345", TipoCodigoAlta.Proveedor, "entre 6 y 20")]
    [InlineData("", TipoCodigoAlta.Normal, "Ingresa")]
    [InlineData("2004000000020", (TipoCodigoAlta)3, "Tipo de código no válido")]
    public void ClasificarCodigo_Rechaza(string codigo, TipoCodigoAlta tipo, string fragmento)
    {
        var (tc, error) = CatalogoEdicionReglas.ClasificarCodigo(codigo, tipo, Tenant);
        Assert.Null(tc);
        Assert.Contains(fragmento, error);
    }

    // ── Servicio con dobles ──

    private sealed class RepoFalso : ICatalogoEdicionRepositoryAsync
    {
        public List<Prod> Productos { get; } = [];
        public List<VarProd> Variantes { get; } = [];
        public List<PresProd> Presentaciones { get; } = [];
        public List<CodBar> Codigos { get; } = [];
        public HashSet<string> CodigosExistentes { get; } = [];
        public bool Historia { get; set; }
        public bool? Confirmado { get; private set; }
        private long _id = 100;

        public async Task<T> EnTransaccionAsync<T>(Func<CancellationToken, Task<(T Resultado, bool Confirmar)>> trabajo, CancellationToken ct = default)
        {
            var (r, ok) = await trabajo(ct);
            Confirmado = ok;
            return r;
        }
        public Task<Prod?> GetProductoAsync(int idTenant, long idProd, CancellationToken ct = default) =>
            Task.FromResult(Productos.FirstOrDefault(p => p.IdProd == idProd && p.IdTenant == idTenant));
        public Task<VarProd?> GetVarianteAsync(int idTenant, long idVarProd, CancellationToken ct = default) =>
            Task.FromResult(Variantes.FirstOrDefault(v => v.IdVarProd == idVarProd && v.Prod.IdTenant == idTenant));
        public Task<CodBar?> GetCodigoAsync(int idTenant, long idCodBar, CancellationToken ct = default) =>
            Task.FromResult(Codigos.FirstOrDefault(c => c.IdCodBar == idCodBar && c.PresProd.VarProd.Prod.IdTenant == idTenant));
        public Task<bool> CategoriaValidaAsync(int idTenant, long idCategoria, CancellationToken ct = default) => Task.FromResult(idCategoria == 1);
        public Task<bool> MarcaValidaAsync(int idTenant, int idMarca, CancellationToken ct = default) => Task.FromResult(idMarca == 1);
        public Task<bool> UnidadExisteAsync(int idUnidad, CancellationToken ct = default) => Task.FromResult(idUnidad is 1 or 2);
        public Task<bool> ExisteCodigoProductoAsync(int idTenant, string codigo, long? excluir, CancellationToken ct = default) =>
            Task.FromResult(Productos.Any(p => p.IdTenant == idTenant && p.Codigo == codigo && p.IdProd != excluir));
        public Task<bool> ExisteCodigoVarianteAsync(long idProd, string codigo, CancellationToken ct = default) =>
            Task.FromResult(Variantes.Any(v => v.Prod.IdProd == idProd && v.Codigo == codigo));
        public Task<bool> ExisteCodigoBarrasAsync(string codigo, CancellationToken ct = default) =>
            Task.FromResult(CodigosExistentes.Contains(codigo) || Codigos.Any(c => c.Codigo == codigo));
        public Task<string?> GetMaxCodigoInternoAsync(string prefijo, CancellationToken ct = default) =>
            Task.FromResult(Codigos.Where(c => c.TipoCod == 3 && c.Codigo.StartsWith(prefijo)).Select(c => c.Codigo).DefaultIfEmpty().Max());
        public Task<bool> TieneHistoriaAsync(IReadOnlyCollection<long> v, IReadOnlyCollection<long> p, CancellationToken ct = default) => Task.FromResult(Historia);
        public void AgregarProducto(Prod producto) { producto.IdProd = ++_id; Productos.Add(producto); }
        public void AgregarVariante(VarProd variante)
        {
            variante.IdVarProd = ++_id; variante.IdProd = variante.Prod.IdProd; Variantes.Add(variante);
            variante.Prod.Variantes.Add(variante);
        }
        public void AgregarPresentacion(PresProd p)
        {
            p.IdPresProd = ++_id; p.IdVarProd = p.VarProd.IdVarProd; Presentaciones.Add(p);
            p.VarProd.Presentaciones.Add(p);
        }
        public void AgregarCodigo(CodBar c)
        {
            c.IdCodBar = ++_id;
            c.PresProd ??= Presentaciones.Single(p => p.IdPresProd == c.IdPresProd);
            c.IdPresProd = c.PresProd.IdPresProd;
            Codigos.Add(c);
            if (!c.PresProd.CodigosBarra.Contains(c)) c.PresProd.CodigosBarra.Add(c);
        }
        public void QuitarCodigo(CodBar c) { Codigos.Remove(c); c.PresProd.CodigosBarra.Remove(c); }
        public Task GuardarAsync(CancellationToken ct = default) => Task.CompletedTask;
    }

    private sealed class PreciosFalso : IGestionPreciosServiceAsync
    {
        public List<CambioPrecio> Aplicados { get; } = [];
        public ErrorCambioPrecio? Error { get; set; }
        public Task<Result<ResultadoCambiosPreciosDto>> SimularAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default) => throw new NotSupportedException();
        public Task<Result<ResultadoCambiosPreciosDto>> AplicarAsync(int idTenant, IReadOnlyList<CambioPrecio> cambios, CancellationToken ct = default)
        {
            if (Error is not null)
                return Task.FromResult(Result<ResultadoCambiosPreciosDto>.Success(new(false, [Error], [], [], [], [], [])));
            Aplicados.AddRange(cambios);
            return Task.FromResult(Result<ResultadoCambiosPreciosDto>.Success(new(true, [], [], [], [], [], [555L])));
        }
    }

    private static (CatalogoEdicionServiceAsync Svc, RepoFalso Repo, PreciosFalso Precios) Crear()
    {
        var repo = new RepoFalso();
        var precios = new PreciosFalso();
        return (new CatalogoEdicionServiceAsync(repo, precios), repo, precios);
    }

    private static NuevoProductoDatos Producto(string? codBarras = null, TipoCodigoAlta tipo = TipoCodigoAlta.Normal, decimal precio = 12990m) =>
        new("P1", "Polera", 1, null, 1, new NuevaVarianteDatos("4", "Talla 4", precio, codBarras, tipo));

    [Fact]
    public async Task CrearProducto_GeneraVariantePresentacionCodigoInternoYPrecio()
    {
        var (svc, repo, precios) = Crear();
        var r = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        Assert.True(r.IsSuccess, r.Error?.Message);
        Assert.True(repo.Confirmado);
        var pres = Assert.Single(repo.Presentaciones);
        Assert.Equal(("Polera / Talla 4", 1, true, true), (pres.Descrip, pres.IdUnidad, pres.EsVenta, pres.EsCompra));
        var cod = Assert.Single(repo.Codigos);
        Assert.Equal(("2004000000013", (byte)3, true), (cod.Codigo, cod.TipoCod, cod.Principal));
        var c = Assert.Single(precios.Aplicados);
        Assert.Equal((pres.IdPresProd, TipoCambioPrecio.PrecioOficial, 12990m), (c.IdPresProd, c.Tipo, c.Valor!.Value));
        Assert.Equal((555L, "2004000000013"), (r.Value.IdPrecio!.Value, r.Value.CodigoBarras));
    }

    [Fact]
    public async Task CrearProducto_ConEan_UsaTipoNormal()
    {
        var (svc, repo, _) = Crear();
        var r = await svc.CrearProductoAsync(Tenant, Producto(" 7801234567894 "), Ct);
        Assert.True(r.IsSuccess, r.Error?.Message);
        Assert.Equal(("7801234567894", (byte)1), (repo.Codigos[0].Codigo, repo.Codigos[0].TipoCod));
    }

    [Theory]
    [InlineData("", "Polera", 12990, "código del producto")]
    [InlineData("P1", " ", 12990, "nombre del producto")]
    [InlineData("P1", "Polera", 0, "precio de venta")]
    public async Task CrearProducto_Validaciones422_SinAbrirTransaccion(string codigo, string nombre, decimal precio, string fragmento)
    {
        var (svc, repo, _) = Crear();
        var r = await svc.CrearProductoAsync(Tenant, new(codigo, nombre, 1, null, 1, new NuevaVarianteDatos("4", "Talla 4", precio)), Ct);
        Assert.Equal(422, r.Error!.HttpStatusCode);
        Assert.Contains(fragmento, r.Error.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Null(repo.Confirmado);
    }

    [Fact]
    public async Task CrearProducto_CodigoDuplicadoYReferenciasInvalidas()
    {
        var (svc, repo, _) = Crear();
        Assert.True((await svc.CrearProductoAsync(Tenant, Producto(), Ct)).IsSuccess);
        var dup = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        Assert.Equal((CodigosCatalogo.ProductoDuplicado, 409), (dup.Error!.Code, dup.Error.HttpStatusCode));
        Assert.False(repo.Confirmado);

        var cat = await svc.CrearProductoAsync(Tenant, new("P2", "X", 99, null, 1, new NuevaVarianteDatos("A", "A", 1)), Ct);
        Assert.Equal(CodigosCatalogo.CategoriaInvalida, cat.Error!.Code);
        var marca = await svc.CrearProductoAsync(Tenant, new("P2", "X", 1, 99, 1, new NuevaVarianteDatos("A", "A", 1)), Ct);
        Assert.Equal(CodigosCatalogo.MarcaInvalida, marca.Error!.Code);
    }

    [Fact]
    public async Task CrearProducto_CodigoDeBarrasEnUso_409_MensajeSinDatosDeOtroTenant()
    {
        var (svc, repo, _) = Crear();
        repo.CodigosExistentes.Add("7801234567894");
        var r = await svc.CrearProductoAsync(Tenant, Producto("7801234567894"), Ct);
        Assert.Equal((CodigosCatalogo.CodigoBarrasDuplicado, 409, "Ese código de barras ya está en uso."), (r.Error!.Code, r.Error.HttpStatusCode, r.Error.Message));
        Assert.Empty(repo.Productos);
    }

    [Fact]
    public async Task CrearProducto_PrecioRechazado_RevierteTodo()
    {
        var (svc, repo, precios) = Crear();
        precios.Error = new ErrorCambioPrecio(0, 0, CodigosPrecio.PrecioInvalido, "El precio debe ser un monto entero.");
        var r = await svc.CrearProductoAsync(Tenant, Producto(precio: 1.5m), Ct);
        Assert.Equal((CodigosPrecio.PrecioInvalido, 422), (r.Error!.Code, r.Error.HttpStatusCode));
        Assert.False(repo.Confirmado);
    }

    [Fact]
    public async Task CrearVariante_ProductoInexistenteYCodigoRepetido()
    {
        var (svc, repo, _) = Crear();
        var nf = await svc.CrearVarianteAsync(Tenant, 999, new NuevaVarianteDatos("B", "B", 1000), Ct);
        Assert.Equal(404, nf.Error!.HttpStatusCode);

        var p = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        var dup = await svc.CrearVarianteAsync(Tenant, p.Value.IdProd, new NuevaVarianteDatos("4", "Otra", 1000), Ct);
        Assert.Equal(CodigosCatalogo.VarianteDuplicada, dup.Error!.Code);

        var ok = await svc.CrearVarianteAsync(Tenant, p.Value.IdProd, new NuevaVarianteDatos("6", "Talla 6", 1000), Ct);
        Assert.True(ok.IsSuccess, ok.Error?.Message);
        Assert.Equal("2004000000020", ok.Value.CodigoBarras); // segundo interno: MAX + 1
        Assert.Equal(2, repo.Presentaciones.Count);
    }

    [Fact]
    public async Task EditarProducto_UnidadConHistoria_409_YSinHistoriaActualizaPresentaciones()
    {
        var (svc, repo, _) = Crear();
        var p = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        repo.Historia = true;
        var conHistoria = await svc.EditarProductoAsync(Tenant, p.Value.IdProd, new("P1", "Polera", 1, null, 2), Ct);
        Assert.Equal((CodigosCatalogo.UnidadConHistoria, 409), (conHistoria.Error!.Code, conHistoria.Error.HttpStatusCode));
        Assert.Equal(1, repo.Presentaciones[0].IdUnidad);

        repo.Historia = false;
        var ok = await svc.EditarProductoAsync(Tenant, p.Value.IdProd, new("P1", "Polera Kids", 1, null, 2), Ct);
        Assert.True(ok.IsSuccess, ok.Error?.Message);
        Assert.Equal(("Polera Kids / Talla 4", 2), (repo.Presentaciones[0].Descrip, repo.Presentaciones[0].IdUnidad));
    }

    [Fact]
    public async Task EditarVariante_InactivaTambienLaPresentacion()
    {
        var (svc, repo, _) = Crear();
        var p = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        var r = await svc.EditarVarianteAsync(Tenant, p.Value.IdVarProd!.Value, new("Talla 4 años", false), Ct);
        Assert.True(r.IsSuccess, r.Error?.Message);
        Assert.Equal(("Polera / Talla 4 años", false), (repo.Presentaciones[0].Descrip, repo.Presentaciones[0].Activo));
        Assert.Equal(404, (await svc.EditarVarianteAsync(5, p.Value.IdVarProd!.Value, new("X", true), Ct)).Error!.HttpStatusCode);
    }

    [Fact]
    public async Task Codigos_UnSoloPrincipal_YElPrincipalNoSeQuita()
    {
        var (svc, repo, _) = Crear();
        var p = await svc.CrearProductoAsync(Tenant, Producto(), Ct);
        var idVar = p.Value.IdVarProd!.Value;

        var adicional = await svc.AgregarCodigoAsync(Tenant, idVar, new("7801234567894", TipoCodigoAlta.Normal, false), Ct);
        Assert.True(adicional.IsSuccess, adicional.Error?.Message);
        Assert.Single(repo.Codigos, c => c.Principal);

        var nuevoPrincipal = await svc.AgregarCodigoAsync(Tenant, idVar, new("123456789", TipoCodigoAlta.Proveedor, true), Ct);
        Assert.True(nuevoPrincipal.IsSuccess);
        Assert.Equal(nuevoPrincipal.Value.IdCodBar, Assert.Single(repo.Codigos, c => c.Principal).IdCodBar);

        var quitar = await svc.QuitarCodigoAsync(Tenant, nuevoPrincipal.Value.IdCodBar!.Value, Ct);
        Assert.Equal(CodigosCatalogo.PrincipalNoQuitable, quitar.Error!.Code);

        Assert.True((await svc.MarcarPrincipalAsync(Tenant, adicional.Value.IdCodBar!.Value, Ct)).IsSuccess);
        Assert.Equal(adicional.Value.IdCodBar, Assert.Single(repo.Codigos, c => c.Principal).IdCodBar);
        Assert.True((await svc.QuitarCodigoAsync(Tenant, nuevoPrincipal.Value.IdCodBar!.Value, Ct)).IsSuccess);
        Assert.Equal(2, repo.Codigos.Count);

        var invalido = await svc.AgregarCodigoAsync(Tenant, idVar, new("7801234567890", TipoCodigoAlta.Normal, false), Ct);
        Assert.Equal((CodigosCatalogo.CodigoBarrasInvalido, 422), (invalido.Error!.Code, invalido.Error.HttpStatusCode));
    }
}
