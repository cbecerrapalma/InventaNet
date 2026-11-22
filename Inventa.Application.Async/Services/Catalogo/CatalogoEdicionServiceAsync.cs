using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services.Precios;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services.Catalogo;

// ── S94: contratos de la edición del catálogo (nunca entidades hacia la API) ──
public sealed record NuevaVarianteDatos(string Codigo, string Nombre, decimal PrecioVenta, string? CodigoBarras = null, TipoCodigoAlta TipoCodigo = TipoCodigoAlta.Normal);
public sealed record NuevoProductoDatos(string Codigo, string Nombre, long IdCategoria, int? IdMarca, int IdUniBase, NuevaVarianteDatos Variante);
public sealed record EdicionProductoDatos(string Codigo, string Nombre, long IdCategoria, int? IdMarca, int IdUniBase);
public sealed record EdicionVarianteDatos(string Nombre, bool Activo);
public sealed record NuevoCodigoDatos(string Codigo, TipoCodigoAlta Tipo, bool Principal);

/// <summary>Identificadores afectados por una operación de edición.</summary>
public sealed record CatalogoEdicionResultadoDto(long IdProd, long? IdVarProd = null, long? IdPresProd = null, long? IdCodBar = null, string? CodigoBarras = null, long? IdPrecio = null);

public static class CodigosCatalogo
{
    public const string Validacion = "VALIDATION";
    public const string ProductoNoEncontrado = "PRODUCTO_NO_ENCONTRADO";
    public const string VarianteNoEncontrada = "VARIANTE_NO_ENCONTRADA";
    public const string CodigoNoEncontrado = "CODIGO_BARRAS_NO_ENCONTRADO";
    public const string ProductoDuplicado = "PRODUCTO_CODIGO_DUPLICADO";
    public const string VarianteDuplicada = "VARIANTE_CODIGO_DUPLICADO";
    public const string CodigoBarrasDuplicado = "CODIGO_BARRAS_DUPLICADO";
    public const string CodigoBarrasInvalido = "CODIGO_BARRAS_INVALIDO";
    public const string CategoriaInvalida = "CATEGORIA_INVALIDA";
    public const string MarcaInvalida = "MARCA_INVALIDA";
    public const string UnidadInvalida = "UNIDAD_INVALIDA";
    public const string UnidadConHistoria = "UNIDAD_CON_HISTORIA";
    public const string PrincipalNoQuitable = "CODIGO_PRINCIPAL_NO_QUITABLE";
    public const string PresentacionFaltante = "PRESENTACION_FALTANTE";
}

public interface ICatalogoEdicionServiceAsync
{
    /// <summary>Producto + primera variante + presentación 1:1 + código principal + precio oficial inicial, en una transacción.</summary>
    Task<Result<CatalogoEdicionResultadoDto>> CrearProductoAsync(int idTenant, NuevoProductoDatos datos, CancellationToken ct = default);
    /// <summary>Variante + presentación 1:1 + código principal + precio oficial inicial, en una transacción.</summary>
    Task<Result<CatalogoEdicionResultadoDto>> CrearVarianteAsync(int idTenant, long idProd, NuevaVarianteDatos datos, CancellationToken ct = default);
    /// <summary>Edita el producto y recalcula la descripción de sus presentaciones. La unidad base solo cambia sin historia.</summary>
    Task<Result<CatalogoEdicionResultadoDto>> EditarProductoAsync(int idTenant, long idProd, EdicionProductoDatos datos, CancellationToken ct = default);
    /// <summary>Edita nombre y estado de la variante; la presentación sigue a la variante.</summary>
    Task<Result<CatalogoEdicionResultadoDto>> EditarVarianteAsync(int idTenant, long idVarProd, EdicionVarianteDatos datos, CancellationToken ct = default);
    Task<Result<CatalogoEdicionResultadoDto>> AgregarCodigoAsync(int idTenant, long idVarProd, NuevoCodigoDatos datos, CancellationToken ct = default);
    Task<Result<CatalogoEdicionResultadoDto>> MarcarPrincipalAsync(int idTenant, long idCodBar, CancellationToken ct = default);
    Task<Result<CatalogoEdicionResultadoDto>> QuitarCodigoAsync(int idTenant, long idCodBar, CancellationToken ct = default);
}

/// <summary>
/// S94 (K1–K3, K5): ÚNICO escritor de <c>Prod</c>, <c>VarProd</c>, <c>PresProd</c> y <c>CodBar</c> en la ruta de la UI.
/// Invariantes (ADR-PROD-001 rev. 2): una presentación por variante, generada por el sistema y nunca recreada
/// (se conserva <c>IdPresProd</c>); al menos un código y exactamente un principal; el precio inicial lo escribe
/// <see cref="IGestionPreciosServiceAsync"/> (S93.1) dentro de la misma transacción; nada se borra si tiene historia.
/// </summary>
public sealed class CatalogoEdicionServiceAsync : ICatalogoEdicionServiceAsync
{
    private readonly ICatalogoEdicionRepositoryAsync _repo;
    private readonly IGestionPreciosServiceAsync _precios;

    public CatalogoEdicionServiceAsync(ICatalogoEdicionRepositoryAsync repo, IGestionPreciosServiceAsync precios)
    {
        _repo = repo;
        _precios = precios;
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> CrearProductoAsync(int idTenant, NuevoProductoDatos datos, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        ArgumentNullException.ThrowIfNull(datos);
        if (CatalogoEdicionReglas.ValidarTexto(datos.Codigo, "El código del producto", CatalogoEdicionReglas.MaxCodigoProducto, out var codigo) is { } e1) return Falla(CodigosCatalogo.Validacion, e1);
        if (CatalogoEdicionReglas.ValidarTexto(datos.Nombre, "El nombre del producto", CatalogoEdicionReglas.MaxNombreProducto, out var nombre) is { } e2) return Falla(CodigosCatalogo.Validacion, e2);
        if (ValidarVariante(datos.Variante, idTenant, out var vCodigo, out var vNombre, out var codBarras, out var tipoCod) is { } e3) return e3;

        return await _repo.EnTransaccionAsync(async t =>
        {
            if (await ValidarReferenciasAsync(idTenant, datos.IdCategoria, datos.IdMarca, datos.IdUniBase, t) is { } ref1) return (ref1, false);
            if (await _repo.ExisteCodigoProductoAsync(idTenant, codigo, null, t))
                return (Falla(CodigosCatalogo.ProductoDuplicado, $"Ya existe un producto con el código «{codigo}».", 409), false);
            if (codBarras is not null && await _repo.ExisteCodigoBarrasAsync(codBarras, t))
                return (CodigoEnUso(), false);

            var prod = new Prod
            {
                IdTenant = idTenant, Codigo = codigo, Nombre = nombre, IdCategoria = datos.IdCategoria,
                IdMarca = datos.IdMarca, IdUniBase = datos.IdUniBase, Activo = true, VisibleStore = false
            };
            _repo.AgregarProducto(prod);
            var (variante, pres, cod) = await NuevaVarianteAsync(idTenant, prod, vCodigo, vNombre, codBarras, tipoCod, t);
            await _repo.GuardarAsync(t);
            var precio = await PrecioInicialAsync(idTenant, pres.IdPresProd, datos.Variante.PrecioVenta, t);
            if (!precio.IsSuccess) return (Result<CatalogoEdicionResultadoDto>.Failure(precio.Error!), false);
            return (Ok(new CatalogoEdicionResultadoDto(prod.IdProd, variante.IdVarProd, pres.IdPresProd, cod.IdCodBar, cod.Codigo, precio.Value)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> CrearVarianteAsync(int idTenant, long idProd, NuevaVarianteDatos datos, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        ArgumentNullException.ThrowIfNull(datos);
        if (ValidarVariante(datos, idTenant, out var vCodigo, out var vNombre, out var codBarras, out var tipoCod) is { } e1) return e1;

        return await _repo.EnTransaccionAsync(async t =>
        {
            var prod = await _repo.GetProductoAsync(idTenant, idProd, t);
            if (prod is null) return (NoEncontrado(CodigosCatalogo.ProductoNoEncontrado, "El producto no existe o no pertenece a tu tenant."), false);
            if (await _repo.ExisteCodigoVarianteAsync(idProd, vCodigo, t))
                return (Falla(CodigosCatalogo.VarianteDuplicada, $"El producto ya tiene una variante con el código «{vCodigo}».", 409), false);
            if (codBarras is not null && await _repo.ExisteCodigoBarrasAsync(codBarras, t))
                return (CodigoEnUso(), false);

            var (variante, pres, cod) = await NuevaVarianteAsync(idTenant, prod, vCodigo, vNombre, codBarras, tipoCod, t);
            await _repo.GuardarAsync(t);
            var precio = await PrecioInicialAsync(idTenant, pres.IdPresProd, datos.PrecioVenta, t);
            if (!precio.IsSuccess) return (Result<CatalogoEdicionResultadoDto>.Failure(precio.Error!), false);
            return (Ok(new CatalogoEdicionResultadoDto(prod.IdProd, variante.IdVarProd, pres.IdPresProd, cod.IdCodBar, cod.Codigo, precio.Value)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> EditarProductoAsync(int idTenant, long idProd, EdicionProductoDatos datos, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        ArgumentNullException.ThrowIfNull(datos);
        if (CatalogoEdicionReglas.ValidarTexto(datos.Codigo, "El código del producto", CatalogoEdicionReglas.MaxCodigoProducto, out var codigo) is { } e1) return Falla(CodigosCatalogo.Validacion, e1);
        if (CatalogoEdicionReglas.ValidarTexto(datos.Nombre, "El nombre del producto", CatalogoEdicionReglas.MaxNombreProducto, out var nombre) is { } e2) return Falla(CodigosCatalogo.Validacion, e2);

        return await _repo.EnTransaccionAsync(async t =>
        {
            var prod = await _repo.GetProductoAsync(idTenant, idProd, t);
            if (prod is null) return (NoEncontrado(CodigosCatalogo.ProductoNoEncontrado, "El producto no existe o no pertenece a tu tenant."), false);
            if (await ValidarReferenciasAsync(idTenant, datos.IdCategoria, datos.IdMarca, datos.IdUniBase, t) is { } ref1) return (ref1, false);
            if (await _repo.ExisteCodigoProductoAsync(idTenant, codigo, idProd, t))
                return (Falla(CodigosCatalogo.ProductoDuplicado, $"Ya existe otro producto con el código «{codigo}».", 409), false);

            var presentaciones = prod.Variantes.SelectMany(v => v.Presentaciones).ToList();
            if (datos.IdUniBase != prod.IdUniBase)
            {
                if (await _repo.TieneHistoriaAsync(prod.Variantes.Select(v => v.IdVarProd).ToList(), presentaciones.Select(p => p.IdPresProd).ToList(), t))
                    return (Falla(CodigosCatalogo.UnidadConHistoria,
                        "No se puede cambiar la unidad base: el producto ya tiene stock, lotes, movimientos, ventas u órdenes de compra.", 409), false);
                prod.IdUniBase = datos.IdUniBase;
                foreach (var pp in presentaciones) pp.IdUnidad = datos.IdUniBase;
            }
            prod.Codigo = codigo;
            prod.Nombre = nombre;
            prod.IdCategoria = datos.IdCategoria;
            prod.IdMarca = datos.IdMarca;
            foreach (var v in prod.Variantes)
                foreach (var pp in v.Presentaciones)
                    pp.Descrip = CatalogoEdicionReglas.Descripcion(nombre, v.Nombre);
            await _repo.GuardarAsync(t);
            return (Ok(new CatalogoEdicionResultadoDto(prod.IdProd)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> EditarVarianteAsync(int idTenant, long idVarProd, EdicionVarianteDatos datos, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        ArgumentNullException.ThrowIfNull(datos);
        if (CatalogoEdicionReglas.ValidarTexto(datos.Nombre, "El nombre de la variante", CatalogoEdicionReglas.MaxNombreVariante, out var nombre) is { } e1) return Falla(CodigosCatalogo.Validacion, e1);

        return await _repo.EnTransaccionAsync(async t =>
        {
            var v = await _repo.GetVarianteAsync(idTenant, idVarProd, t);
            if (v is null) return (NoEncontrado(CodigosCatalogo.VarianteNoEncontrada, "La variante no existe o no pertenece a tu tenant."), false);
            v.Nombre = nombre;
            v.Activo = datos.Activo;
            foreach (var pp in v.Presentaciones)
            {
                pp.Descrip = CatalogoEdicionReglas.Descripcion(v.Prod.Nombre, nombre);
                pp.Activo = datos.Activo;
            }
            await _repo.GuardarAsync(t);
            return (Ok(new CatalogoEdicionResultadoDto(v.IdProd, v.IdVarProd, v.Presentaciones.FirstOrDefault()?.IdPresProd)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> AgregarCodigoAsync(int idTenant, long idVarProd, NuevoCodigoDatos datos, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        ArgumentNullException.ThrowIfNull(datos);
        var (tipoCod, error) = CatalogoEdicionReglas.ClasificarCodigo(datos.Codigo, datos.Tipo, idTenant);
        if (error is not null) return Falla(CodigosCatalogo.CodigoBarrasInvalido, error, 422);
        var codigo = datos.Codigo.Trim();

        return await _repo.EnTransaccionAsync(async t =>
        {
            var v = await _repo.GetVarianteAsync(idTenant, idVarProd, t);
            if (v is null) return (NoEncontrado(CodigosCatalogo.VarianteNoEncontrada, "La variante no existe o no pertenece a tu tenant."), false);
            var pres = v.Presentaciones.FirstOrDefault();
            if (pres is null) return (Falla(CodigosCatalogo.PresentacionFaltante, "La variante no tiene presentación; vuelve a importarla o contacta al administrador.", 409), false);
            if (await _repo.ExisteCodigoBarrasAsync(codigo, t)) return (CodigoEnUso(), false);

            var principal = datos.Principal || pres.CodigosBarra.All(c => !c.Principal);
            if (principal) await DegradarPrincipalAsync(pres, t); // el principal anterior queda como adicional
            var cod = new CodBar { IdPresProd = pres.IdPresProd, Codigo = codigo, TipoCod = tipoCod!.Value, Principal = principal };
            _repo.AgregarCodigo(cod);
            await _repo.GuardarAsync(t);
            return (Ok(new CatalogoEdicionResultadoDto(v.IdProd, v.IdVarProd, pres.IdPresProd, cod.IdCodBar, cod.Codigo)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> MarcarPrincipalAsync(int idTenant, long idCodBar, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        return await _repo.EnTransaccionAsync(async t =>
        {
            var cod = await _repo.GetCodigoAsync(idTenant, idCodBar, t);
            if (cod is null) return (NoEncontrado(CodigosCatalogo.CodigoNoEncontrado, "El código de barras no existe o no pertenece a tu tenant."), false);
            if (!cod.Principal)
            {
                await DegradarPrincipalAsync(cod.PresProd, t);
                cod.Principal = true;
                await _repo.GuardarAsync(t);
            }
            return (Ok(new CatalogoEdicionResultadoDto(cod.PresProd.VarProd.IdProd, cod.PresProd.IdVarProd, cod.IdPresProd, cod.IdCodBar, cod.Codigo)), true);
        }, ct);
    }

    public async Task<Result<CatalogoEdicionResultadoDto>> QuitarCodigoAsync(int idTenant, long idCodBar, CancellationToken ct = default)
    {
        if (idTenant <= 0) return Falla(CodigosCatalogo.Validacion, "IdTenant debe ser mayor que 0.", 400);
        return await _repo.EnTransaccionAsync(async t =>
        {
            var cod = await _repo.GetCodigoAsync(idTenant, idCodBar, t);
            if (cod is null) return (NoEncontrado(CodigosCatalogo.CodigoNoEncontrado, "El código de barras no existe o no pertenece a tu tenant."), false);
            if (cod.Principal)
                return (Falla(CodigosCatalogo.PrincipalNoQuitable, "No se puede quitar el código principal: marca otro como principal primero.", 409), false);
            _repo.QuitarCodigo(cod);
            await _repo.GuardarAsync(t);
            return (Ok(new CatalogoEdicionResultadoDto(cod.PresProd.VarProd.IdProd, cod.PresProd.IdVarProd, cod.IdPresProd, cod.IdCodBar, cod.Codigo)), true);
        }, ct);
    }

    // ── internos ──

    private Result<CatalogoEdicionResultadoDto>? ValidarVariante(NuevaVarianteDatos? v, int idTenant,
        out string codigo, out string nombre, out string? codBarras, out byte tipoCod)
    {
        codigo = nombre = ""; codBarras = null; tipoCod = CatalogoEdicionReglas.TipoInterno;
        if (v is null) return Falla(CodigosCatalogo.Validacion, "Faltan los datos de la variante.");
        if (CatalogoEdicionReglas.ValidarTexto(v.Codigo, "El código de la variante", CatalogoEdicionReglas.MaxCodigoVariante, out codigo) is { } e1) return Falla(CodigosCatalogo.Validacion, e1);
        if (CatalogoEdicionReglas.ValidarTexto(v.Nombre, "El nombre de la variante", CatalogoEdicionReglas.MaxNombreVariante, out nombre) is { } e2) return Falla(CodigosCatalogo.Validacion, e2);
        if (v.PrecioVenta <= 0) return Falla(CodigosCatalogo.Validacion, "El precio de venta debe ser mayor que 0.");
        if (!string.IsNullOrWhiteSpace(v.CodigoBarras))
        {
            var (tc, error) = CatalogoEdicionReglas.ClasificarCodigo(v.CodigoBarras, v.TipoCodigo, idTenant);
            if (error is not null) return Falla(CodigosCatalogo.CodigoBarrasInvalido, error, 422);
            codBarras = v.CodigoBarras.Trim();
            tipoCod = tc!.Value;
        }
        return null;
    }

    private async Task<Result<CatalogoEdicionResultadoDto>?> ValidarReferenciasAsync(int idTenant, long idCategoria, int? idMarca, int idUniBase, CancellationToken ct)
    {
        if (!await _repo.CategoriaValidaAsync(idTenant, idCategoria, ct)) return Falla(CodigosCatalogo.CategoriaInvalida, "La categoría no existe, está inactiva o no pertenece a tu tenant.");
        if (idMarca is int m && !await _repo.MarcaValidaAsync(idTenant, m, ct)) return Falla(CodigosCatalogo.MarcaInvalida, "La marca no existe, está inactiva o no pertenece a tu tenant.");
        if (!await _repo.UnidadExisteAsync(idUniBase, ct)) return Falla(CodigosCatalogo.UnidadInvalida, "La unidad base no existe.");
        return null;
    }

    /// <summary>Generador 1:1 (K1): variante + presentación (unidad base, Descrip del SP v5) + código principal.</summary>
    private async Task<(VarProd Variante, PresProd Pres, CodBar Codigo)> NuevaVarianteAsync(int idTenant, Prod prod,
        string codigo, string nombre, string? codBarras, byte tipoCod, CancellationToken ct)
    {
        var variante = new VarProd { Prod = prod, Codigo = codigo, Nombre = nombre, Activo = true };
        var pres = new PresProd
        {
            VarProd = variante, IdUnidad = prod.IdUniBase, Descrip = CatalogoEdicionReglas.Descripcion(prod.Nombre, nombre),
            EsVenta = true, EsCompra = true, Activo = true
        };
        if (codBarras is null)
        {
            codBarras = CatalogoEdicionReglas.SiguienteInterno(idTenant,
                await _repo.GetMaxCodigoInternoAsync(CatalogoEdicionReglas.PrefijoInterno(idTenant), ct));
            tipoCod = CatalogoEdicionReglas.TipoInterno;
        }
        var cod = new CodBar { PresProd = pres, Codigo = codBarras, TipoCod = tipoCod, Principal = true };
        _repo.AgregarVariante(variante);
        _repo.AgregarPresentacion(pres);
        _repo.AgregarCodigo(cod);
        return (variante, pres, cod);
    }

    /// <summary>Degrada el principal actual y guarda ANTES de promover otro (índice único filtrado <c>UX_CodBar_Principal</c>).</summary>
    private async Task<bool> DegradarPrincipalAsync(PresProd pres, CancellationToken ct)
    {
        var actuales = pres.CodigosBarra.Where(c => c.Principal).ToList();
        if (actuales.Count == 0) return false;
        foreach (var c in actuales) c.Principal = false;
        await _repo.GuardarAsync(ct);
        return true;
    }

    /// <summary>K5: el precio inicial lo escribe el escritor único de precios (S93.1), en la transacción abierta.</summary>
    private async Task<Result<long?>> PrecioInicialAsync(int idTenant, long idPresProd, decimal valor, CancellationToken ct)
    {
        var r = await _precios.AplicarAsync(idTenant, [new CambioPrecio(idPresProd, TipoCambioPrecio.PrecioOficial, Valor: valor)], ct);
        if (!r.IsSuccess) return Result<long?>.Failure(r.Error!);
        if (r.Value.HayErrores)
        {
            var e = r.Value.Errores[0];
            return Result<long?>.Failure(new Error(e.Codigo, e.Mensaje).WithType(ErrorType.Validation).WithHttpStatus(422));
        }
        return Result<long?>.Success(r.Value.PreciosCreados.Count > 0 ? r.Value.PreciosCreados[0] : null, allowNull: true);
    }

    private static Result<CatalogoEdicionResultadoDto> Ok(CatalogoEdicionResultadoDto dto) => Result<CatalogoEdicionResultadoDto>.Success(dto);

    private static Result<CatalogoEdicionResultadoDto> Falla(string codigo, string mensaje, int http = 422) =>
        Result<CatalogoEdicionResultadoDto>.Failure(new Error(codigo, mensaje)
            .WithType(http == 409 ? ErrorType.Conflict : ErrorType.Validation).WithHttpStatus(http));

    private static Result<CatalogoEdicionResultadoDto> NoEncontrado(string codigo, string mensaje) =>
        Result<CatalogoEdicionResultadoDto>.Failure(new Error(codigo, mensaje).WithType(ErrorType.NotFound).WithHttpStatus(404));

    // Unicidad global: el mensaje no revela si el código es de otro tenant.
    private static Result<CatalogoEdicionResultadoDto> CodigoEnUso() =>
        Falla(CodigosCatalogo.CodigoBarrasDuplicado, "Ese código de barras ya está en uso.", 409);
}
