using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services;
using Inventa.WebAPI.Models;
using Microsoft.AspNetCore.Authorization;
using CBP.WebApi.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/inventario")]
[Authorize]
public class InventarioController : ControllerBase
{
    private readonly IInventarioServiceAsync _svc;
    public InventarioController(IInventarioServiceAsync svc) => _svc = svc;

    [HttpPost("transferir")]
    [RequierePermiso("INVENTA_INVENTARIO_TRANSFERENCIAS_TRANSFERIR")]
    public async Task<IActionResult> Transferir([FromBody] TransferirDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var idUsuario = (int)HttpContext.Items["IdUsuario"]!;
        var req = new TransferirEntreBodegasInput(idTenant, dto.IdBodegaOrig, dto.IdBodegaDest,
            dto.Items.Select(i => new TransferenciaItemInput(i.IdPresProd, i.IdLote, i.Cantidad)).ToList(), idUsuario);
        var r = await _svc.TransferirAsync(req, ct);
        return r.IsSuccess ? Ok() : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpPost("ajustar")]
    [RequierePermiso("INVENTA_INVENTARIO_AJUSTES_AJUSTAR")]
    public async Task<IActionResult> Ajustar([FromBody] AjustarDto dto, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        var idUsuario = (int)HttpContext.Items["IdUsuario"]!;
        var req = new AjustarInventarioInput(idTenant, dto.IdBodega,
            dto.Items.Select(i => new AjusteItemInput(i.IdPresProd, i.IdLote, i.CantAjuste, i.CostoUnitario)).ToList(), idUsuario);
        var r = await _svc.AjustarAsync(req, ct);
        return r.IsSuccess ? Ok() : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("stock")]
    [RequierePermiso("INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_LOTES_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER")]
    public async Task<IActionResult> GetStock([FromQuery] int? idBodega, [FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetStockPagedAsync(idTenant, idBodega, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Data.Async.Repositories.StockListadoRow>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetStockAsync(idTenant, idBodega, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("stock-disp")]
    [RequierePermiso("INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_LOTES_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER")]
    public async Task<IActionResult> GetStockDisp([FromQuery] int? idBodega, [FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetStockDispPagedAsync(idTenant, idBodega, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Data.Async.Repositories.StockDispListadoRow>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetStockDispAsync(idTenant, idBodega, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("movimientos")]
    [RequierePermiso("INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_LOTES_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER")]
    public async Task<IActionResult> GetMovimientos([FromQuery] int? idBodega, [FromQuery] byte? tipoMov, [FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        // S114 D3: paginado si se pide página, búsqueda o tipo; sin ellos, la lista reciente de siempre.
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q) || tipoMov.HasValue)
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetMovimientosPagedAsync(idTenant, idBodega, tipoMov, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Data.Async.Repositories.MovInvListadoRow>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetMovimientosAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }

    [HttpGet("lotes")]
    [RequierePermiso("INVENTA_INVENTARIO_STOCK_VER", "INVENTA_INVENTARIO_STOCK_DISPONIBLE_VER", "INVENTA_INVENTARIO_MOVIMIENTOS_VER", "INVENTA_INVENTARIO_LOTES_VER", "INVENTA_INVENTARIO_TRANSFERENCIAS_VER", "INVENTA_INVENTARIO_AJUSTES_VER")]
    public async Task<IActionResult> GetLotes([FromQuery] string? q, [FromQuery] int? page, [FromQuery] int? pageSize, CancellationToken ct)
    {
        var idTenant = (int)HttpContext.Items["IdTenantLocal"]!;
        if (page.HasValue || pageSize.HasValue || !string.IsNullOrWhiteSpace(q))
        {
            var p = page ?? 1; var ps = pageSize ?? 20;
            var rPaged = await _svc.GetLotesPagedAsync(idTenant, q, p, ps, ct);
            if (!rPaged.IsSuccess) return StatusCode(rPaged.Error!.HttpStatusCode ?? 400, rPaged.Error!.ToProblemDetails(HttpContext));
            var paged = rPaged.Value;
            var resp = PagedResponse<Inventa.Domain.Entities.Lote>.From(paged.Items, paged.Page, paged.PageSize, paged.TotalCount);
            return Ok(resp);
        }
        var r = await _svc.GetLotesAsync(idTenant, ct);
        return r.IsSuccess ? Ok(r.Value) : StatusCode(r.Error!.HttpStatusCode ?? 400, r.Error!.ToProblemDetails(HttpContext));
    }
}
public record TransferenciaItemDto(long IdPresProd, long? IdLote, decimal Cantidad);
public record TransferirDto(int IdBodegaOrig, int IdBodegaDest, List<TransferenciaItemDto> Items);
public record AjusteItemDto(long IdPresProd, long? IdLote, decimal CantAjuste, decimal CostoUnitario = 0m);
public record AjustarDto(int IdBodega, List<AjusteItemDto> Items);



