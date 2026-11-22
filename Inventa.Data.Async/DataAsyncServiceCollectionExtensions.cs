using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventa.Data.Async;

/// <summary>Extensiones de DI para registrar repositorios async de Inventa.Data.Async.</summary>
public static class DataAsyncServiceCollectionExtensions
{
    /// <summary>
    /// Registra todos los repositorios async de Inventa.Data.Async.
    /// Debe llamarse después de AddDbContext<InventaDbContext> y AddUnitOfWorkAsync.
    /// </summary>
    public static IServiceCollection AddDataAsync(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services);

        // ── Catalog Core ──
        services.AddScoped<IProdRepositoryAsync, ProdRepositoryAsync>();
        services.AddScoped<IVarProdRepositoryAsync, VarProdRepositoryAsync>();
        services.AddScoped<IPresProdRepositoryAsync, PresProdRepositoryAsync>();
        services.AddScoped<IProdConvRepositoryAsync, ProdConvRepositoryAsync>();
        services.AddScoped<ICodBarRepositoryAsync, CodBarRepositoryAsync>();
        services.AddScoped<ICategoriaRepositoryAsync, CategoriaRepositoryAsync>();
        services.AddScoped<IMarcaRepositoryAsync, MarcaRepositoryAsync>();
        services.AddScoped<IUnidadRepositoryAsync, UnidadRepositoryAsync>();
        services.AddScoped<IAtriDefRepositoryAsync, AtriDefRepositoryAsync>();
        services.AddScoped<IAtriValRepositoryAsync, AtriValRepositoryAsync>();
        services.AddScoped<IVarAtriRepositoryAsync, VarAtriRepositoryAsync>();
        services.AddScoped<IPrecioRepositoryAsync, PrecioRepositoryAsync>();
        // S57.69: mayorista normalizado.
        services.AddScoped<IPrecioMayoristaRepositoryAsync, PrecioMayoristaRepositoryAsync>();

        // ── Inventory ──
        services.AddScoped<IBodegaRepositoryAsync, BodegaRepositoryAsync>();
        services.AddScoped<ILoteRepositoryAsync, LoteRepositoryAsync>();
        services.AddScoped<IStockRepositoryAsync, StockRepositoryAsync>();
        services.AddScoped<IStockDispRepositoryAsync, StockDispRepositoryAsync>();
        services.AddScoped<IMovInvRepositoryAsync, MovInvRepositoryAsync>();

        // ── Compras ──
        services.AddScoped<IProveedorRepositoryAsync, ProveedorRepositoryAsync>();
        services.AddScoped<IOrdenCompraRepositoryAsync, OrdenCompraRepositoryAsync>();
        services.AddScoped<IOrdenCompraDetalleRepositoryAsync, OrdenCompraDetalleRepositoryAsync>();

        // ── Ventas ──
        services.AddScoped<IClienteRepositoryAsync, ClienteRepositoryAsync>();
        services.AddScoped<IVentaRepositoryAsync, VentaRepositoryAsync>();
        services.AddScoped<IVentaDetalleRepositoryAsync, VentaDetalleRepositoryAsync>();
        services.AddScoped<IVentaPagoRepositoryAsync, VentaPagoRepositoryAsync>();
        services.AddScoped<IMetodoPagoRepositoryAsync, MetodoPagoRepositoryAsync>();

        // ── Config / Identity mirrors ──
        services.AddScoped<IUsuarioRepositoryAsync, UsuarioRepositoryAsync>();
        services.AddScoped<IUsuarioTenantRepositoryAsync, UsuarioTenantRepositoryAsync>();
        services.AddScoped<ITenantRepositoryAsync, TenantRepositoryAsync>();
        services.AddScoped<IConfiguracionRepositoryAsync, ConfiguracionRepositoryAsync>();
        services.AddScoped<ITenantConfigInventaRepositoryAsync, TenantConfigInventaRepositoryAsync>();

        // ── S57.34: storefront (sin contraparte sync; gap S57 cubierto solo en Async) ──
        services.AddScoped<IStorefrontRepositoryAsync, StorefrontRepositoryAsync>();

        // ── S57.34: imágenes de producto (sin contraparte sync) ──
        services.AddScoped<IProductoImagenRepositoryAsync, ProductoImagenRepositoryAsync>();

        // ── S104: tabla Archivo (storage); también es el IUploadRegistry de CBP.Storage ──
        services.AddScoped<IArchivoRepositoryAsync, ArchivoRepositoryAsync>();
        services.AddScoped<CBP.Storage.IUploadRegistry>(sp => sp.GetRequiredService<IArchivoRepositoryAsync>());

        // ── S87: lote auditable de importación ──
        services.AddScoped<IImportacionLoteRepositoryAsync, ImportacionLoteRepositoryAsync>();

        // ── S88.2: repositorio SP de importación (movido desde Application.Async) ──
        services.AddScoped<IProductoImportSpRepositoryAsync, ProductoImportSpRepositoryAsync>();
        services.AddScoped<IProductoFichaRepositoryAsync, ProductoFichaRepositoryAsync>(); // S92
        services.AddScoped<IPreciosGrillaRepositoryAsync, PreciosGrillaRepositoryAsync>(); // S93.1
        services.AddScoped<ICatalogoEdicionRepositoryAsync, CatalogoEdicionRepositoryAsync>(); // S94

        // ── S57.52: oferta con referencia obligatoria (sin contraparte sync) ──
        services.AddScoped<IOfertaRepositoryAsync, OfertaRepositoryAsync>();

        // ── S57.66: promoción (aggregate root Promocion; sin contraparte sync) ──
        services.AddScoped<IPromocionRepositoryAsync, PromocionRepositoryAsync>();
        services.AddScoped<IPromocionPresentacionRepositoryAsync, PromocionPresentacionRepositoryAsync>();
        services.AddScoped<IPromocionReglaRepositoryAsync, PromocionReglaRepositoryAsync>();
        services.AddScoped<IPromocionBeneficioRepositoryAsync, PromocionBeneficioRepositoryAsync>();

        return services;
    }
}