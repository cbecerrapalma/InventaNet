using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

[ApiController]
[Route("api/usuarios")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioServiceAsync _usuarioSvc;
    public UsuariosController(IUsuarioServiceAsync usuarioSvc) => _usuarioSvc = usuarioSvc;

    [HttpGet("{id:int}")]
    public async Task<IActionResult> Get(int id, CancellationToken ct)
    {
        var r = await _usuarioSvc.GetByIdAsync(id, ct);
        if (!r.IsSuccess) return NotFound();
        return Ok(r.Value);
    }

    [HttpGet("{id:int}/tenants")]
    public async Task<IActionResult> GetTenants(int id, CancellationToken ct)
    {
        var r = await _usuarioSvc.GetTenantsByUsuarioAsync(id, ct);
        return Ok(r.Value);
    }
}

