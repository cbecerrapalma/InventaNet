using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace Inventa.Application;

/// <summary>Extensiones de DI para registrar servicios de Inventa.Application.</summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios, AutoMapper profiles y validators de Inventa.Application.
    /// </summary>
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ── AutoMapper ──
        services.AddAutoMapper(cfg => cfg.AddMaps(typeof(ApplicationServiceCollectionExtensions).Assembly));

        // ── FluentValidation ──
        var assembly = typeof(ApplicationServiceCollectionExtensions).Assembly;
        var validatorTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType)
            .Select(t => new { Type = t, Interface = t.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IValidator<>)) })
            .Where(x => x.Interface != null);

        foreach (var v in validatorTypes)
        {
            services.AddTransient(v.Interface!, v.Type);
        }

        // ── Auto-scan servicios ──
        var serviceTypes = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Service"))
            .SelectMany(t => t.GetInterfaces(), (type, iface) => new { type, iface })
            .Where(t => t.iface.Name.EndsWith("Service"))
            .ToList();

        foreach (var svc in serviceTypes)
        {
            services.Add(new ServiceDescriptor(svc.iface, svc.type, lifetime));
        }

        return services;
    }
}
