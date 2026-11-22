using AutoMapper;
using Inventa.Application.Dtos;
using Inventa.Domain.Entities;

namespace Inventa.Application.Profiles;

public class EntityMappingProfile : Profile
{
    public EntityMappingProfile()
    {
        // ── Catálogos simples ──
        CreateMap<Tenant, TenantDto>().ReverseMap();
        CreateMap<Unidad, UnidadDto>().ReverseMap();
        CreateMap<MetodoPago, MetodoPagoDto>().ReverseMap();

        // ── Entidades con tenant ──
        CreateMap<Bodega, BodegaDto>().ReverseMap();
        CreateMap<Marca, MarcaDto>().ReverseMap();
        CreateMap<Categoria, CategoriaDto>().ReverseMap();
        CreateMap<Cliente, ClienteDto>().ReverseMap();
        CreateMap<Proveedor, ProveedorDto>().ReverseMap();

        // ── Producto y derivados ──
        CreateMap<Prod, ProdDto>().ReverseMap();
        CreateMap<VarProd, VarProdDto>().ReverseMap();
        CreateMap<PresProd, PresProdDto>().ReverseMap();
        CreateMap<ProdConv, ProdConvDto>().ReverseMap();
        CreateMap<CodBar, CodBarDto>().ReverseMap();

        // ── Atributos ──
        CreateMap<AtriDef, AtriDefDto>().ReverseMap();
        CreateMap<AtriVal, AtriValDto>().ReverseMap();
        CreateMap<VarAtri, VarAtriDto>().ReverseMap();

        // ── Inventario ──
        CreateMap<Lote, LoteDto>().ReverseMap();
        CreateMap<Stock, StockDto>().ReverseMap();
        CreateMap<StockDisp, StockDispDto>().ReverseMap();
        CreateMap<MovInv, MovInvDto>().ReverseMap();

        // ── Precios ──
        CreateMap<Precio, PrecioDto>().ReverseMap();

        // ── Ventas ──
        CreateMap<Venta, VentaDto>().ReverseMap();
        CreateMap<VentaDetalle, VentaDetalleDto>().ReverseMap();
        CreateMap<VentaPago, VentaPagoDto>().ReverseMap();

        // ── Compras ──
        CreateMap<OrdenCompra, OrdenCompraDto>().ReverseMap();
        CreateMap<OrdenCompraDetalle, OrdenCompraDetalleDto>().ReverseMap();
    }
}
