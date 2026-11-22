using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/public")]
public class PublicTenantsController : ControllerBase
{
    private readonly IStorefrontServiceAsync _svc;
    public PublicTenantsController(IStorefrontServiceAsync svc) => _svc = svc;

    [HttpGet("tenants")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTenants(CancellationToken ct)
    {
        var r = await _svc.GetActiveTenantsAsync(ct);
        if (!r.IsSuccess) return StatusCode(500);
        return Ok(r.Value.Select(t => new { t.IdTenant, t.Codigo, t.Nombre, t.IdTenantPassPlat }).ToList());
    }
}
