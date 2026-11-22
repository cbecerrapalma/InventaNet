using CBP.MultiTenant.Abstractions;
using Inventa.Domain.Entities;

namespace Inventa.Infrastructure.Tenancy;

/// <summary>
/// Mapea la entidad <see cref="Tenant"/> de Inventa → <see cref="TenantInfo"/>.
/// Las colecciones de navegación EF son ignoradas deliberadamente.
/// </summary>
public sealed class InventaTenantMapper : ITenantMapper<Tenant>
{
    public TenantInfo Map(Tenant t) =>
        TenantInfo.Create(t.IdTenant, t.Codigo, t.Nombre, t.Activo);
}