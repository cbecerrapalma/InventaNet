using Inventa.Application.Async.Models;
using Inventa.Data.Async.Repositories;

namespace Inventa.Application.Async.Services;

public interface IDashboardServiceAsync
{
    Task<DashboardSummaryDto> GetSummaryAsync(int idTenant, IReadOnlySet<string> permissions, CancellationToken ct = default);
}

public sealed class DashboardServiceAsync : IDashboardServiceAsync
{
    private readonly IProdRepositoryAsync _prodRepo;
    private readonly IClienteRepositoryAsync _clienteRepo;
    private readonly IProveedorRepositoryAsync _proveedorRepo;
    private readonly IBodegaRepositoryAsync _bodegaRepo;
    private readonly IStockRepositoryAsync _stockRepo;
    private readonly ILoteRepositoryAsync _loteRepo;
    private readonly IVentaRepositoryAsync _ventaRepo;
    private readonly IOrdenCompraRepositoryAsync _ocRepo;

    public DashboardServiceAsync(
        IProdRepositoryAsync prodRepo,
        IClienteRepositoryAsync clienteRepo,
        IProveedorRepositoryAsync proveedorRepo,
        IBodegaRepositoryAsync bodegaRepo,
        IStockRepositoryAsync stockRepo,
        ILoteRepositoryAsync loteRepo,
        IVentaRepositoryAsync ventaRepo,
        IOrdenCompraRepositoryAsync ocRepo)
    {
        _prodRepo = prodRepo;
        _clienteRepo = clienteRepo;
        _proveedorRepo = proveedorRepo;
        _bodegaRepo = bodegaRepo;
        _stockRepo = stockRepo;
        _loteRepo = loteRepo;
        _ventaRepo = ventaRepo;
        _ocRepo = ocRepo;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync(int idTenant, IReadOnlySet<string> permissions, CancellationToken ct = default)
    {
        var hasProd = permissions.Contains("INVENTA_PRODUCTOS_PRINCIPAL_VER");
        var hasCli = permissions.Contains("INVENTA_CLIENTES_PRINCIPAL_VER");
        var hasProv = permissions.Contains("INVENTA_PROVEEDORES_PRINCIPAL_VER");
        var hasBod = permissions.Contains("INVENTA_BODEGAS_PRINCIPAL_VER");
        var hasStock = permissions.Contains("INVENTA_INVENTARIO_STOCK_VER");
        var hasVenta = permissions.Contains("INVENTA_VENTAS_PRINCIPAL_VER");
        var hasOC = permissions.Contains("INVENTA_COMPRAS_ORDENES_VER");

        ProductosSummaryDto? prod = null;
        ClientesSummaryDto? cli = null;
        ProveedoresSummaryDto? prov = null;
        BodegasSummaryDto? bod = null;
        StockSummaryDto? stock = null;
        LotesSummaryDto? lotes = null;
        VentasSummaryDto? ventas = null;
        ComprasSummaryDto? compras = null;

        // Sequential to avoid DbContext concurrency — tenant-first, aggregates in DB (Count/Sum) no materialization
        if (hasProd)
        {
            var total = await _prodRepo.CountByTenantAsync(idTenant, false, ct);
            var activos = total == 0 ? 0 : await _prodRepo.CountByTenantAsync(idTenant, true, ct);
            prod = new ProductosSummaryDto(total, activos, total - activos);
        }

        if (hasCli)
        {
            var total = await _clienteRepo.CountByTenantAsync(idTenant, false, ct);
            var activos = total == 0 ? 0 : await _clienteRepo.CountByTenantAsync(idTenant, true, ct);
            cli = new ClientesSummaryDto(total, activos, total - activos);
        }

        if (hasProv)
        {
            var total = await _proveedorRepo.CountByTenantAsync(idTenant, false, ct);
            var activos = total == 0 ? 0 : await _proveedorRepo.CountByTenantAsync(idTenant, true, ct);
            prov = new ProveedoresSummaryDto(total, activos, total - activos);
        }

        if (hasBod)
        {
            var total = await _bodegaRepo.CountByTenantAsync(idTenant, ct);
            var activas = total == 0 ? 0 : await _bodegaRepo.CountActivasAsync(idTenant, ct);
            bod = new BodegasSummaryDto(total, activas, total - activas);
        }

        if (hasStock)
        {
            var (registros, cantidad, agotados) = await _stockRepo.GetSummaryAsync(idTenant, ct);
            stock = new StockSummaryDto(registros, cantidad, agotados);

            // Lotes shares STOCK_VER per Index.razor
            var (lotTotal, vencidos, proximos) = await _loteRepo.GetVencimientoSummaryAsync(idTenant, 30, ct);
            lotes = new LotesSummaryDto(lotTotal, vencidos, proximos);
        }

        if (hasVenta)
        {
            var (registros, completadas, anuladas, sub, desc, imp, tot) = await _ventaRepo.GetSummaryAsync(idTenant, ct);
            ventas = new VentasSummaryDto(registros, completadas, anuladas, sub, desc, imp, tot);
        }

        if (hasOC)
        {
            var (registros, pend, rec, canc, tot) = await _ocRepo.GetSummaryAsync(idTenant, ct);
            compras = new ComprasSummaryDto(registros, pend, rec, canc, tot);
        }

        return new DashboardSummaryDto(
            DateTime.UtcNow,
            prod, cli, prov, bod, stock, lotes, ventas, compras);
    }
}
