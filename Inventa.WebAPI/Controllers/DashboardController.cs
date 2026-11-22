using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/dashboard")]
[Authorize]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardServiceAsync _svc;
    public DashboardController(IDashboardServiceAsync svc) => _svc = svc;

    [HttpGet("summary")]
    public async Task<IActionResult> Summary(CancellationToken ct)
    {
        if (!HttpContext.Items.TryGetValue("IdTenantLocal", out var t) || t is not int idTenant)
            return Unauthorized();
        var perms = User.FindAll("permissions").Select(c => c.Value)
            .Concat(User.FindAll("permiso").Select(c => c.Value))
            .Concat(User.FindAll("permission").Select(c => c.Value))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
        // also support single claim with multiple values
        var dto = await _svc.GetSummaryAsync(idTenant, perms, ct);
        return Ok(dto);
    }
}

