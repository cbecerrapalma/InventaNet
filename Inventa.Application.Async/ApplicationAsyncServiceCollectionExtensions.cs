using Inventa.Application.Async.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Inventa.Application.Async;

/// <summary>
/// Extensiones de DI para registrar todos los servicios async de Inventa.Application.Async.
/// Debe llamarse después de AddDataAsync().
/// </summary>
public static class ApplicationAsyncServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los servicios async de Inventa.Application.Async.
    /// </summary>
    public static IServiceCollection AddApplicationAsync(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ── Catalog / Product ──
        services.AddScoped<IProductoServiceAsync, ProductoServiceAsync>();
        services.AddScoped<IProductoFichaServiceAsync, ProductoFichaServiceAsync>(); // S92
        // S57.36 §4: counterpart explícito de ProdService (Sync). ProductoServiceAsync = agregado interno.
        services.AddScoped<IProdServiceAsync, ProdServiceAsync>();
        services.AddScoped<ICatalogoServiceAsync, CatalogoServiceAsync>();

        // ── Inventory ──
        services.AddScoped<IInventarioServiceAsync, InventarioServiceAsync>();

        // ── Sales ──
        services.AddScoped<IVentaServiceAsync, VentaServiceAsync>();

        // ── Purchases ──
        services.AddScoped<IOrdenCompraServiceAsync, OrdenCompraServiceAsync>();

        // ── Dashboard / Images ──
        services.AddScoped<IDashboardServiceAsync, DashboardServiceAsync>();
        services.AddScoped<IProductoImagenServiceAsync, ProductoImagenServiceAsync>();
        // S57.83: composición DB+FS (fuera del par fachada S57.35 §4).
        services.AddScoped<IProductoImagenStorageServiceAsync, ProductoImagenStorageServiceAsync>();
        // S107 (P10): importación de imágenes por URL; RemoteUploadService (CBP.Storage) lo registra Inventa.WebAPI.
        services.AddScoped<IProductoImagenImportacionServiceAsync, ProductoImagenImportacionServiceAsync>();
        // S105: barrido del storage (P9), lo usan el servicio periódico y el endpoint manual de Inventa.WebAPI.
        services.AddScoped<IArchivoBarridoServiceAsync, ArchivoBarridoServiceAsync>();

        // ── Identity / Tenant Sync ──
        services.AddScoped<ITenantSyncServiceAsync, TenantSyncServiceAsync>();
        services.AddScoped<IUsuarioSyncService, UsuarioSyncService>();
        services.AddScoped<IUsuarioTenantSyncService, UsuarioTenantSyncService>();

        // ── S57.34: query services (controllers must not inject repos) ──
        services.AddScoped<IConfiguracionServiceAsync, ConfiguracionServiceAsync>();
        services.AddScoped<ITenantServiceAsync, TenantServiceAsync>();
        services.AddScoped<IUsuarioServiceAsync, UsuarioServiceAsync>();
        services.AddScoped<IStorefrontServiceAsync, StorefrontServiceAsync>();

        // ── S57.35: facade services 1:1 counterparts ──
        services.AddScoped<IAtriDefServiceAsync, AtriDefServiceAsync>();
        services.AddScoped<IAtriValServiceAsync, AtriValServiceAsync>();
        services.AddScoped<IBodegaServiceAsync, BodegaServiceAsync>();
        services.AddScoped<ICategoriaServiceAsync, CategoriaServiceAsync>();
        services.AddScoped<IClienteServiceAsync, ClienteServiceAsync>();
        services.AddScoped<ICodBarServiceAsync, CodBarServiceAsync>();
        services.AddScoped<ILoteServiceAsync, LoteServiceAsync>();
        services.AddScoped<IMarcaServiceAsync, MarcaServiceAsync>();
        services.AddScoped<IMetodoPagoServiceAsync, MetodoPagoServiceAsync>();
        services.AddScoped<IMovInvServiceAsync, MovInvServiceAsync>();
        services.AddScoped<IOrdenCompraDetalleServiceAsync, OrdenCompraDetalleServiceAsync>();
        services.AddScoped<IPrecioServiceAsync, PrecioServiceAsync>();
        services.AddScoped<IPresProdServiceAsync, PresProdServiceAsync>();
        services.AddScoped<IProdConvServiceAsync, ProdConvServiceAsync>();
        services.AddScoped<IProveedorServiceAsync, ProveedorServiceAsync>();
        services.AddScoped<IStockServiceAsync, StockServiceAsync>();
        services.AddScoped<IStockDispServiceAsync, StockDispServiceAsync>();
        services.AddScoped<IUnidadServiceAsync, UnidadServiceAsync>();
        services.AddScoped<IVarAtriServiceAsync, VarAtriServiceAsync>();
        services.AddScoped<IVarProdServiceAsync, VarProdServiceAsync>();
        services.AddScoped<IVentaDetalleServiceAsync, VentaDetalleServiceAsync>();
        services.AddScoped<IVentaPagoServiceAsync, VentaPagoServiceAsync>();

        // ── S87.2: importación de productos (plantilla + validación + lote; sin escritura de catálogo) ──
        services.AddScoped<Services.Importacion.ProductoImportacionValidadorAsync>();
        services.AddScoped<Services.Importacion.IImportacionServiceAsync, Services.Importacion.ImportacionServiceAsync>();
        // ── S88: importación - aplicador (el repo SP vive en Data.Async, S88.2) ──
        services.AddScoped<Services.Importacion.IProductoImportacionAplicadorAsync, Services.Importacion.ProductoImportacionAplicadorAsync>();

        // ── S57.52: oferta con referencia obligatoria + resolver (sin contraparte sync) ──
        services.AddScoped<IOfertaServiceAsync, OfertaServiceAsync>();
        // S93.1: «Precios y ofertas» — lectura (grilla) y único escritor de Precio/Oferta.
        services.AddScoped<IPrecioGrillaServiceAsync, PrecioGrillaServiceAsync>();
        services.AddScoped<Inventa.Application.Async.Services.Precios.IGestionPreciosServiceAsync, Inventa.Application.Async.Services.Precios.GestionPreciosServiceAsync>();
        services.AddScoped<Inventa.Application.Async.Services.Catalogo.ICatalogoEdicionServiceAsync, Inventa.Application.Async.Services.Catalogo.CatalogoEdicionServiceAsync>(); // S94
        services.AddScoped<IPrecioReferenceResolverAsync, PrecioReferenceResolverAsync>();

        // ── S57.56: resolver comercial LEGACY-T3 vs NEW-Oferta (diagnóstico; producción
        // no lo consume hasta cutover con M6/M7 APPROVED) ──
        services.AddScoped<ICommercialOfferResolverAsync, CommercialOfferResolverAsync>();

        // ── S57.66: promoción ADMIN-ONLY (aggregate service único; sin contraparte sync) ──
        services.AddScoped<IPromocionServiceAsync, PromocionServiceAsync>();

        // ── S57.69: mayorista normalizado (sin contraparte sync agregada; WinForms usa repo+write-service) ──
        services.AddScoped<IPrecioMayoristaServiceAsync, PrecioMayoristaServiceAsync>();

        return services;
    }
}