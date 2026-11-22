using CBP.Results;
using Inventa.Application.Async.Models;
using Inventa.Application.Async.Services;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;
using Moq;
using Xunit;

namespace Inventa.Application.Tests;

// S126: el precio de una venta lo decide el servidor (R3, Precios-Modelo.md); el del cliente se ignora.
public sealed class VentaServiceAsyncTests
{
    private const int Tenant = 4;

    private sealed class Escenario
    {
        public Mock<IVentaRepositoryAsync> Ventas { get; } = new();
        public Mock<IPrecioReferenceResolverAsync> Resolver { get; } = new();
        public RegistrarVentaCmd? Capturado { get; private set; }
        public VentaServiceAsync Servicio { get; }

        public Escenario(decimal? efectivo)
        {
            var bodegas = new Mock<IBodegaRepositoryAsync>();
            bodegas.Setup(r => r.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Bodega { IdBodega = 1, IdTenant = Tenant, Activo = true });
            var clientes = new Mock<IClienteRepositoryAsync>();
            var presProd = new Mock<IPresProdRepositoryAsync>();
            presProd.Setup(r => r.GetTenantIdAsync(It.IsAny<long>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((int?)Tenant);
            var metodos = new Mock<IMetodoPagoRepositoryAsync>();
            metodos.Setup(r => r.ExistsAsync(It.IsAny<byte>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);
            var cfg = new Mock<ITenantConfigInventaRepositoryAsync>();
            cfg.Setup(r => r.GetByTenantAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((TenantConfigInventa?)null);

            var mapa = new Dictionary<long, OfertaResolution>();
            if (efectivo is not null)
                mapa[1] = new OfertaResolution(1, null, efectivo, null, null, false, efectivo, null);
            Resolver.Setup(r => r.ResolveManyAsync(It.IsAny<int>(), It.IsAny<IEnumerable<long>>(), It.IsAny<DateOnly?>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<IReadOnlyDictionary<long, OfertaResolution>>.Success(mapa));

            Ventas.Setup(r => r.RegistrarVentaAsync(It.IsAny<RegistrarVentaCmd>(), It.IsAny<CancellationToken>()))
                .Callback<RegistrarVentaCmd, CancellationToken>((c, _) => Capturado = c)
                .ReturnsAsync(Result<RegistrarVentaCmdResult>.Success(new RegistrarVentaCmdResult(1, 1500m, 0m, 0m, 1500m)));

            Servicio = new VentaServiceAsync(Ventas.Object, bodegas.Object, clientes.Object, presProd.Object, metodos.Object, cfg.Object, Resolver.Object);
        }
    }

    private static RegistrarVentaInput Input(decimal precioCliente, decimal descuento = 0m)
        => new(Tenant, 1, "V-TEST", null, 1, null,
            [new VentaItemInput(1, null, 1m, precioCliente, descuento)],
            [new VentaPagoInput(1, 1500m)]);

    // S127 (P23): precio efectivo 1500 y cantidad 1; los pagos con monto 0 no se envían.
    private static RegistrarVentaInput InputConPagos(decimal pago1, decimal pago2 = 0m)
        => new(Tenant, 1, "V-TEST", null, 1, null,
            [new VentaItemInput(1, null, 1m, 1500m)],
            [.. new[] { pago1, pago2 }.Where(m => m > 0).Select(m => new VentaPagoInput(1, m))]);

    [Theory]
    [InlineData(1499)]
    [InlineData(1501)]
    public async Task RegistrarVenta_PagosDistintosDelTotal_FallaSinRegistrar(decimal monto)
    {
        var e = new Escenario(1500m);

        var r = await e.Servicio.RegistrarVentaAsync(InputConPagos(monto), CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("PAGOS_DISTINTOS_DEL_TOTAL", r.Error!.Code);
        e.Ventas.Verify(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaCmd>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Theory]
    [InlineData(1500, 0)]
    [InlineData(1000, 500)]
    public async Task RegistrarVenta_PagosSumanElTotal_Registra(decimal pago1, decimal pago2)
    {
        var e = new Escenario(1500m);

        var r = await e.Servicio.RegistrarVentaAsync(InputConPagos(pago1, pago2), CancellationToken.None);

        Assert.True(r.IsSuccess);
        e.Ventas.Verify(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaCmd>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(999999)]
    public async Task RegistrarVenta_RegistraPrecioEfectivo_IgnoraElDelCliente(decimal precioCliente)
    {
        var e = new Escenario(1500m);

        var r = await e.Servicio.RegistrarVentaAsync(Input(precioCliente), CancellationToken.None);

        Assert.True(r.IsSuccess);
        Assert.NotNull(e.Capturado);
        Assert.Equal(1500m, Assert.Single(e.Capturado!.Items).PrecioUnitario);
    }

    [Fact]
    public async Task RegistrarVenta_SinPrecioVigente_FallaSinRegistrar()
    {
        var e = new Escenario(null);

        var r = await e.Servicio.RegistrarVentaAsync(Input(1500m), CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("SIN_PRECIO_VIGENTE", r.Error!.Code);
        e.Ventas.Verify(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaCmd>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegistrarVenta_DescuentoMayorQueSubtotalVigente_Falla()
    {
        var e = new Escenario(1500m);

        var r = await e.Servicio.RegistrarVentaAsync(Input(999999m, 5000m), CancellationToken.None);

        Assert.False(r.IsSuccess);
        Assert.Equal("VALIDATION", r.Error!.Code);
        e.Ventas.Verify(v => v.RegistrarVentaAsync(It.IsAny<RegistrarVentaCmd>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
