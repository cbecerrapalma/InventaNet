using Inventa.Application.Async.Services;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

// S107 (P10): importa hasta 3 imágenes por producto desde URLs de hosts permitidos (Storage:Remote:AllowedHosts).
// Hasta 25 productos por llamada, identificados por código en el tenant de la sesión. Reanudable: un producto que
// ya tiene imágenes se omite. Responde 200 con el resultado por producto.
[ApiController]
[Route("api/productos/imagenes/importaciones")]
public class ProductoImagenImportacionController : ControllerBase
{
    private readonly IProductoImagenImportacionServiceAsync _svc;
    public ProductoImagenImportacionController(IProductoImagenImportacionServiceAsync svc) => _svc = svc;

    public sealed record ImportarImagenesDto(IReadOnlyList<ImagenImportacionItem>? Items);

    [HttpPost]
    [RequierePermiso("INVENTA_PRODUCTOS_PRINCIPAL_EDITAR", "INVENTA_PRODUCTOS_DETALLE_EDITAR")]
    public async Task<IActionResult> Importar([FromBody] ImportarImagenesDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var r = await _svc.ImportarAsync(idTenant, dto.Items ?? [], ct);
        if (!r.IsSuccess) return StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
        return Ok(r.Value);
    }
}
