using CBP.Outbox;
using Inventa.Data.Context.Configurations;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace Inventa.Data.Context;

public sealed class InventaDbContext : DbContext
{
    public InventaDbContext(DbContextOptions<InventaDbContext> options)
        : base(options)
    {
    }

    // =========================
    // CORE
    // =========================

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Unidad> Unidades => Set<Unidad>();
    public DbSet<ProductoImagen> ProductoImagenes => Set<ProductoImagen>();
    public DbSet<Archivo> Archivos => Set<Archivo>();
    // PassPlat S136: Outbox genérico de CBP (tabla dbo.Outbox).
    public DbSet<OutboxMessage> Outbox => Set<OutboxMessage>();
    // S87: lotes auditables de importación.
    public DbSet<ImportacionLote> ImportacionLotes => Set<ImportacionLote>();

    // =========================
    // CATALOGO
    // =========================

    public DbSet<Marca> Marcas => Set<Marca>();
    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Prod> Productos => Set<Prod>();

    // =========================
    // VARIANTES
    // =========================

    public DbSet<VarProd> Variantes => Set<VarProd>();
    public DbSet<AtriDef> AtributosDef => Set<AtriDef>();
    public DbSet<AtriVal> AtributosVal => Set<AtriVal>();
    public DbSet<VarAtri> VarAtributos => Set<VarAtri>();

    // =========================
    // PRESENTACIONES
    // =========================

    public DbSet<PresProd> Presentaciones => Set<PresProd>();
    public DbSet<ProdConv> Conversiones => Set<ProdConv>();
    public DbSet<CodBar> CodigosBarra => Set<CodBar>();

    // =========================
    // INVENTARIO
    // =========================

    public DbSet<Bodega> Bodegas => Set<Bodega>();
    public DbSet<Lote> Lotes => Set<Lote>();
    public DbSet<Stock> Stock => Set<Stock>();
    public DbSet<StockDisp> StockDisponible => Set<StockDisp>();
    public DbSet<MovInv> MovimientosInventario => Set<MovInv>();

    // =========================
    // PRECIOS
    // =========================

    public DbSet<Precio> Precios => Set<Precio>();
    // S57.69: mayorista normalizado (relación dedicada, sin discriminator).
    public DbSet<PrecioMayorista> PreciosMayorista => Set<PrecioMayorista>();
    // S57.52: oferta con referencia obligatoria (solo Async, sin contraparte sync).
    public DbSet<Oferta> Ofertas => Set<Oferta>();
    // S57.66: promoción — aggregate root Promocion (solo Async, ADMIN-ONLY).
    public DbSet<Promocion> Promociones => Set<Promocion>();
    public DbSet<PromocionPresentacion> PromocionPresentaciones => Set<PromocionPresentacion>();
    public DbSet<PromocionRegla> PromocionReglas => Set<PromocionRegla>();
    public DbSet<PromocionBeneficio> PromocionBeneficios => Set<PromocionBeneficio>();

    // =========================
    // COMPRAS
    // =========================

    public DbSet<Proveedor> Proveedores => Set<Proveedor>();
    public DbSet<OrdenCompra> OrdenesCompra => Set<OrdenCompra>();
    public DbSet<OrdenCompraDetalle> OrdenCompraDetalle => Set<OrdenCompraDetalle>();

    // =========================
    // VENTAS
    // =========================

    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Venta> Ventas => Set<Venta>();
    public DbSet<VentaDetalle> VentaDetalle => Set<VentaDetalle>();

    public DbSet<MetodoPago> MetodosPago => Set<MetodoPago>();
    public DbSet<VentaPago> VentaPagos => Set<VentaPago>();

    // =========================
    // PASSPLAT MIRROR / CONFIG
    // =========================

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<UsuarioTenant> UsuarioTenants => Set<UsuarioTenant>();
    public DbSet<Configuracion> Configuraciones => Set<Configuracion>();
    public DbSet<TenantConfigInventa> TenantConfigs => Set<TenantConfigInventa>();
    public DbSet<TenantStorefront> TenantStorefronts => Set<TenantStorefront>();
    public DbSet<TenantSocialLink> TenantSocialLinks => Set<TenantSocialLink>();
    public DbSet<TenantBusinessHour> TenantBusinessHours => Set<TenantBusinessHour>();

    // =========================
    // MODEL BUILDER
    // =========================

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}