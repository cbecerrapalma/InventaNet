using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Services;
using Inventa.Application.Async.Services.Catalogo;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/productos")]
[RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER")]
public class ProductosController : ControllerBase
{
    private readonly IProductoServiceAsync _svc;
    private readonly IVarProdServiceAsync _varSvc;
    private readonly ICodBarServiceAsync _codBarSvc;
    private readonly ICatalogoEdicionServiceAsync _edicion;
    public ProductosController(
        IProductoServiceAsync svc,
        IVarProdServiceAsync varSvc,
        ICodBarServiceAsync codBarSvc,
        ICatalogoEdicionServiceAsync edicion)
    {
        _svc = svc;
        _varSvc = varSvc;
        _codBarSvc = codBarSvc;
        _edicion = edicion;
    }

    [HttpGet]
    public async Task<IActionResult> List([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1;
            var ps = pageSize ?? 20;
            var rPaged = await _svc.SearchPagedAsync(idTenant, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Domain.Entities.Prod>.From(paged.Items, p, ps, paged.Total);
            return Ok(resp);
        }
        var r = await _svc.GetRecentAsync(idTenant, 100, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> Get(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetByIdInTenantAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value == null) return NotFound();
        return Ok(r.Value);
    }

    [HttpGet("codbar/{codigo}")]
    public async Task<IActionResult> GetByCodBar(string codigo, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _codBarSvc.GetByCodigoConPresentacionAsync(codigo, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value == null ? NotFound() : Ok(r.Value);
    }

    // S94 (K2): crear producto = producto + primera variante + presentación 1:1 + código principal + precio inicial,
    // en una transacción (único escritor: ICatalogoEdicionServiceAsync).
    [HttpPost]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_CREAR", "INVENTA_PRODUCTOS_DETALLE_CREAR")]
    public async Task<IActionResult> Create([FromBody] CrearProductoRequest dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (dto.Variante is null)
            return Responder(Result<CatalogoEdicionResultadoDto>.Failure(new Error(CodigosCatalogo.Validacion,
                "Un producto se crea con su primera variante (código, nombre y precio de venta).").WithType(ErrorType.Validation).WithHttpStatus(422)));
        var r = await _edicion.CrearProductoAsync(idTenant, new NuevoProductoDatos(dto.Codigo, dto.Nombre, dto.IdCategoria, dto.IdMarca, dto.IdUniBase,
            dto.Variante.ADatos()), ct);
        return r.IsSuccess ? CreatedAtAction(nameof(Get), new { id = r.Value.IdProd }, r.Value) : Responder(r);
    }

    // S94 (K1): editar recalcula PresProd.Descrip; la unidad base solo cambia si no hay historia (409).
    [HttpPut("{id:long}")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> Update(long id, [FromBody] CreateProdDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.EditarProductoAsync(idTenant, id, new EdicionProductoDatos(dto.Codigo, dto.Nombre, dto.IdCategoria, dto.IdMarca, dto.IdUniBase), ct);
        return r.IsSuccess ? Ok(r.Value) : Responder(r);
    }

    // S57.2 publishability toggle: explicit commercial opt-in, tenant-scoped, PRODUCTOS_PRINCIPAL_EDITAR o PRODUCTOS_DETALLE_EDITAR only.
    // S57.3 P1.3: backend re-validates (activo/identity/tenant); warnings do not block the explicit act.
    [HttpPatch("{id:long}/visibilidad")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> SetVisibilidad(long id, [FromBody] SetVisibilidadDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.SetVisibleStoreAsync(id, idTenant, dto.Visible, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return r.Value != null ? Ok(new { r.Value.IdProd, r.Value.VisibleStore }) : NotFound();
    }

    [HttpGet("{id:long}/preview")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_VER", "INVENTA_PRODUCTOS_DETALLE_VER", "INVENTA_PRECIOS_PRINCIPAL_VER", "INVENTA_COMPRAS_ORDENES_VER")]
    public async Task<IActionResult> Preview(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.GetPreviewHeaderAsync(id, idTenant, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        if (r.Value == null) return NotFound();
        var header = r.Value;
        // Note: full preview requires more data; returning header for now
        return Ok(new { header.IdProd, header.Codigo, header.Nombre, header.IdCategoria, header.CatNombre, header.IdMarca, header.MarcaNombre });
    }

    // S92: GET publishability se movió a ProductosAdminController (mismo contrato, ahora con diagnóstico real).

    // S57.45-R1 GAP-1: estructura comercial anidada (variante -> presentacion -> codbar).
    // Capa HTTP delgada: valida padre en tenant (404 si foraneo/inexistente), delega en
    // Application.Async existente. Mismo permiso que crear producto (PRODUCTOS_PRINCIPAL_CREAR o PRODUCTOS_DETALLE_CREAR):
    // sin permisos nuevos -> sin cambios en PassPlat.
    [HttpGet("{id:long}/variantes")]
    public async Task<IActionResult> GetVariantes(long id, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var rp = await _svc.GetByIdInTenantAsync(id, idTenant, ct);
        if (!rp.IsSuccess) return StatusCode(rp.Error!.HttpStatusCode ?? 400, rp.Error!.ToProblemDetails(HttpContext));
        if (rp.Value == null) return NotFound();
        var r = await _varSvc.GetByProductoAsync(id, true, ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }

    // S94 (K2): crear variante = variante + presentación 1:1 + código principal + precio inicial, en una transacción.
    [HttpPost("{id:long}/variantes")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_CREAR", "INVENTA_PRODUCTOS_DETALLE_CREAR")]
    public async Task<IActionResult> CreateVariante(long id, [FromBody] CrearVarianteRequest dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.CrearVarianteAsync(idTenant, id, dto.ADatos(), ct);
        return r.IsSuccess ? CreatedAtAction(nameof(Get), new { id }, r.Value) : Responder(r);
    }

    // S94 (K1): nombre y estado; la presentación sigue a la variante.
    [HttpPut("variantes/{idVariante:long}")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> UpdateVariante(long idVariante, [FromBody] EditarVarianteRequest dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.EditarVarianteAsync(idTenant, idVariante, new EdicionVarianteDatos(dto.Nombre, dto.Activo), ct);
        return r.IsSuccess ? Ok(r.Value) : Responder(r);
    }

    // S94 (K4): las presentaciones las genera el sistema (ADR-PROD-001 rev. 2).
    [HttpPost("variantes/{idVariante:long}/presentaciones")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_CREAR", "INVENTA_PRODUCTOS_DETALLE_CREAR")]
    public IActionResult CreatePresentacion(long idVariante) =>
        Retirado("Las presentaciones las genera el sistema: cada variante tiene exactamente una.");

    // S94 (K3): códigos de barra por variante (su única presentación).
    [HttpPost("variantes/{idVariante:long}/codbar")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> AgregarCodigo(long idVariante, [FromBody] AgregarCodigoRequest dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.AgregarCodigoAsync(idTenant, idVariante, new NuevoCodigoDatos(dto.Codigo, dto.Tipo, dto.Principal), ct);
        return r.IsSuccess ? CreatedAtAction(nameof(Get), new { id = r.Value.IdProd }, r.Value) : Responder(r);
    }

    [HttpPut("codbar/{idCodBar:long}/principal")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> MarcarPrincipal(long idCodBar, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.MarcarPrincipalAsync(idTenant, idCodBar, ct);
        return r.IsSuccess ? Ok(r.Value) : Responder(r);
    }

    [HttpDelete("codbar/{idCodBar:long}")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> QuitarCodigo(long idCodBar, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _edicion.QuitarCodigoAsync(idTenant, idCodBar, ct);
        return r.IsSuccess ? NoContent() : Responder(r);
    }

    // S94 (K3): ruta antigua por presentación; reemplazada por variantes/{id}/codbar.
    [HttpPost("presentaciones/{idPresentacion:long}/codbar")]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_CREAR", "INVENTA_PRODUCTOS_DETALLE_CREAR")]
    public IActionResult CreateCodBar(long idPresentacion) =>
        Retirado("Usa POST api/productos/variantes/{idVariante}/codbar.");

    private IActionResult Responder(Result<CatalogoEdicionResultadoDto> r) =>
        StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));

    private IActionResult Retirado(string detalle) =>
        StatusCode(StatusCodes.Status410Gone, new ProblemDetails
        {
            Status = StatusCodes.Status410Gone, Title = "Operación retirada", Detail = detalle,
            Type = "https://httpstatuses.io/410", Instance = HttpContext.Request.Path
        });
}
public record CreateProdDto(long IdCategoria, int? IdMarca, string Codigo, string Nombre, int IdUniBase);
public record SetVisibilidadDto(bool Visible);
public record CrearVarianteRequest(string Codigo, string Nombre, decimal PrecioVenta, string? CodigoBarras = null, TipoCodigoAlta TipoCodigo = TipoCodigoAlta.Normal)
{
    public NuevaVarianteDatos ADatos() => new(Codigo, Nombre, PrecioVenta, CodigoBarras, TipoCodigo);
}
public record CrearProductoRequest(string Codigo, string Nombre, long IdCategoria, int? IdMarca, int IdUniBase, CrearVarianteRequest? Variante);
public record EditarVarianteRequest(string Nombre, bool Activo);
public record AgregarCodigoRequest(string Codigo, TipoCodigoAlta Tipo = TipoCodigoAlta.Normal, bool Principal = false);


