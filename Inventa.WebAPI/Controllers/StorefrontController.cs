using CBP.WebApi.Extensions;
using Inventa.Application.Async.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Inventa.WebAPI.Controllers;

/// <summary>S57.1 Backoffice administration of tenant storefront profile (Inventa-owned commercial content).</summary>
[ApiController]
[Route("api/storefront")]
[Authorize]
public class StorefrontController : ControllerBase
{
    private static readonly HashSet<string> AllowedLinkTypes = new(StringComparer.OrdinalIgnoreCase)
        { "facebook", "instagram", "tiktok", "x", "linkedin", "youtube", "whatsapp", "web" };
    private readonly IStorefrontServiceAsync _svc;
    public StorefrontController(IStorefrontServiceAsync svc) => _svc = svc;

    private int LocalTenant() => HttpContext.Items.TryGetValue("IdTenantLocal", out var v) && v is int i ? i : 0;

    [HttpGet]
    [RequierePermiso("INVENTA_CONFIGURACION_STOREFRONT_VER")]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var id = LocalTenant();
        var p = await _svc.GetProfileAsync(id, ct);
        if (!p.IsSuccess) return StatusCode(500);
        if (p.Value == null) return NotFound();
        var links = await _svc.GetLinksAsync(id, ct);
        var hours = await _svc.GetHoursAsync(id, ct);
        return Ok(new { p = p.Value, social = links.Value, hours = hours.Value });
    }

    [HttpPut]
    [RequierePermiso("INVENTA_CONFIGURACION_STOREFRONT_EDITAR")]
    public async Task<IActionResult> Upsert([FromBody] UpsertStorefrontDto dto, CancellationToken ct)
    {
        var id = LocalTenant();
        if (id <= 0) return Forbid();
        var bad = Validate(dto);
        if (bad != null) return BadRequest(new { codigo = "VALIDATION", mensaje = bad });
        var current = await _svc.GetProfileAsync(id, ct);
        var p = current.Value ?? new Inventa.Domain.Entities.TenantStorefront { IdTenant = id };
        p.NombreFantasia = N(dto.NombreFantasia, 200); p.DescripcionCorta = N(dto.DescripcionCorta, 500);
        p.Descripcion = N(dto.Descripcion, 4000); p.LogoUrl = N(dto.LogoUrl, 500);
        p.Telefono = N(dto.Telefono, 50); p.WhatsApp = N(dto.WhatsApp, 50); p.Email = N(dto.Email, 255);
        p.Direccion = N(dto.Direccion, 300); p.Comuna = N(dto.Comuna, 100); p.Ciudad = N(dto.Ciudad, 100);
        p.Region = N(dto.Region, 100); p.SitioWeb = N(dto.SitioWeb, 300);
        p.Publicado = dto.Publicado; p.FecMod = DateTime.UtcNow;
        var up = await _svc.UpsertProfileAsync(p, ct);
        return Ok(up.Value);
    }

    [HttpPost("social")]
    [RequierePermiso("INVENTA_CONFIGURACION_STOREFRONT_EDITAR")]
    public async Task<IActionResult> AddLink([FromBody] UpsertSocialDto dto, CancellationToken ct)
    {
        var id = LocalTenant();
        if (!AllowedLinkTypes.Contains(dto.Tipo ?? "")) return BadRequest(new { codigo = "VALIDATION", mensaje = "Tipo de red no permitido" });
        if (!IsHttpUrl(dto.Url)) return BadRequest(new { codigo = "VALIDATION", mensaje = "Url inválida" });
        var e = new Inventa.Domain.Entities.TenantSocialLink { IdTenant = id, Tipo = dto.Tipo!.ToLowerInvariant(), Url = dto.Url!, Orden = dto.Orden, Activo = true };
        var r = await _svc.AddLinkAsync(e, ct);
        return StatusCode(201, r.Value);
    }

    [HttpDelete("social/{linkId:int}")]
    [RequierePermiso("INVENTA_CONFIGURACION_STOREFRONT_EDITAR")]
    public async Task<IActionResult> RemoveLink(int linkId, CancellationToken ct)
    {
        var id = LocalTenant();
        var r = await _svc.RemoveLinkAsync(id, linkId, ct);
        if (!r.Value) return NotFound();
        return NoContent();
    }

    [HttpPut("hours")]
    [RequierePermiso("INVENTA_CONFIGURACION_STOREFRONT_EDITAR")]
    public async Task<IActionResult> PutHours([FromBody] List<UpsertHourDto> dtos, CancellationToken ct)
    {
        var id = LocalTenant();
        if (dtos == null || dtos.Count == 0 || dtos.Count > 7) return BadRequest(new { codigo = "VALIDATION", mensaje = "Se requieren 1 a 7 días" });
        if (dtos.Select(d => d.DiaSemana).Distinct().Count() != dtos.Count) return BadRequest(new { codigo = "VALIDATION", mensaje = "Días duplicados" });
        foreach (var d in dtos)
        {
            if (d.DiaSemana > 6) return BadRequest(new { codigo = "VALIDATION", mensaje = "DiaSemana 0-6" });
            if (!d.Cerrado && !ValidRange(d.Apertura1, d.Cierre1) && !ValidRange(d.Apertura2, d.Cierre2))
                return BadRequest(new { codigo = "VALIDATION", mensaje = $"Rango inválido día {d.DiaSemana}" });
        }
        var hours = dtos.Select(d => new Inventa.Domain.Entities.TenantBusinessHour
        {
            IdTenant = id, DiaSemana = d.DiaSemana, Apertura1 = d.Apertura1, Cierre1 = d.Cierre1,
            Apertura2 = d.Apertura2, Cierre2 = d.Cierre2, Cerrado = d.Cerrado, Activo = true
        }).ToList();
        var r = await _svc.ReplaceHoursAsync(id, hours, ct);
        return Ok(r.Value);
    }

    static string? N(string? v, int max) => string.IsNullOrWhiteSpace(v) ? null : v.Trim()[..Math.Min(v.Trim().Length, max)];
    static bool IsHttpUrl(string? u) => Uri.TryCreate(u, UriKind.Absolute, out var x) && (x.Scheme == "http" || x.Scheme == "https");
    static bool ValidRange(TimeSpan? a, TimeSpan? c) => a.HasValue && c.HasValue && a < c;
    static string? Validate(UpsertStorefrontDto d)
    {
        if (d.Email != null && !d.Email.Contains('@')) return "Email inválido";
        if (d.LogoUrl != null && !IsHttpUrl(d.LogoUrl) && !d.LogoUrl.StartsWith('/')) return "LogoUrl inválida";
        if (d.SitioWeb != null && !IsHttpUrl(d.SitioWeb)) return "SitioWeb inválido";
        return null;
    }
}

public sealed record UpsertStorefrontDto(string? NombreFantasia, string? DescripcionCorta, string? Descripcion, string? LogoUrl, string? Telefono, string? WhatsApp, string? Email, string? Direccion, string? Comuna, string? Ciudad, string? Region, string? SitioWeb, bool Publicado);
public sealed record UpsertSocialDto(string? Tipo, string? Url, int Orden);
public sealed record UpsertHourDto(byte DiaSemana, TimeSpan? Apertura1, TimeSpan? Cierre1, TimeSpan? Apertura2, TimeSpan? Cierre2, bool Cerrado);
