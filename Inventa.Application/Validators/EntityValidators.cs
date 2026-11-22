using FluentValidation;
using Inventa.Application.Dtos;

namespace Inventa.Application.Validators;

public class TenantValidator : AbstractValidator<TenantDto>
{
    public TenantValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class UnidadValidator : AbstractValidator<UnidadDto>
{
    public UnidadValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(50);
    }
}

public class MetodoPagoValidator : AbstractValidator<MetodoPagoDto>
{
    public MetodoPagoValidator()
    {
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(10);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(50);
    }
}

public class BodegaValidator : AbstractValidator<BodegaDto>
{
    public BodegaValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class MarcaValidator : AbstractValidator<MarcaDto>
{
    public MarcaValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class CategoriaValidator : AbstractValidator<CategoriaDto>
{
    public CategoriaValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class ClienteValidator : AbstractValidator<ClienteDto>
{
    public ClienteValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RUT).MaximumLength(20).When(x => x.RUT != null);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
        RuleFor(x => x.TipoCliente).InclusiveBetween((byte)1, (byte)4);
    }
}

public class ProveedorValidator : AbstractValidator<ProveedorDto>
{
    public ProveedorValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.RazonSocial).NotEmpty().MaximumLength(150);
        RuleFor(x => x.RUT).MaximumLength(20).When(x => x.RUT != null);
        RuleFor(x => x.Email).EmailAddress().When(x => x.Email != null);
    }
}

public class ProdValidator : AbstractValidator<ProdDto>
{
    public ProdValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdCategoria).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.IdUniBase).GreaterThan(0);
    }
}

public class VarProdValidator : AbstractValidator<VarProdDto>
{
    public VarProdValidator()
    {
        RuleFor(x => x.IdProd).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(150);
        RuleFor(x => x.CostoProm).GreaterThanOrEqualTo(0);
    }
}

public class PresProdValidator : AbstractValidator<PresProdDto>
{
    public PresProdValidator()
    {
        RuleFor(x => x.IdVarProd).GreaterThan(0);
        RuleFor(x => x.IdUnidad).GreaterThan(0);
        RuleFor(x => x.Descrip).NotEmpty().MaximumLength(100);
    }
}

public class ProdConvValidator : AbstractValidator<ProdConvDto>
{
    public ProdConvValidator()
    {
        RuleFor(x => x.IdVarProd).GreaterThan(0);
        RuleFor(x => x.IdUnidad).GreaterThan(0);
        RuleFor(x => x.Factor).GreaterThan(0);
    }
}

public class CodBarValidator : AbstractValidator<CodBarDto>
{
    public CodBarValidator()
    {
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.TipoCod).InclusiveBetween((byte)1, (byte)5);
    }
}

public class AtriDefValidator : AbstractValidator<AtriDefDto>
{
    public AtriDefValidator()
    {
        RuleFor(x => x.IdCategoria).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
    }
}

public class AtriValValidator : AbstractValidator<AtriValDto>
{
    public AtriValValidator()
    {
        RuleFor(x => x.IdAtriDef).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Nombre).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Orden).GreaterThanOrEqualTo(0);
    }
}

public class VarAtriValidator : AbstractValidator<VarAtriDto>
{
    public VarAtriValidator()
    {
        RuleFor(x => x.IdVarProd).GreaterThan(0);
        RuleFor(x => x.IdAtriVal).GreaterThan(0);
    }
}

public class LoteValidator : AbstractValidator<LoteDto>
{
    public LoteValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdVarProd).GreaterThan(0);
        RuleFor(x => x.Codigo).NotEmpty().MaximumLength(50);
        RuleFor(x => x.FecVen).GreaterThan(DateOnly.FromDateTime(DateTime.UtcNow));
    }
}

public class StockValidator : AbstractValidator<StockDto>
{
    public StockValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdBodega).GreaterThan(0);
        RuleFor(x => x.IdVarProd).GreaterThan(0);
        RuleFor(x => x.CantAct).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CostoProm).GreaterThanOrEqualTo(0);
    }
}

public class StockDispValidator : AbstractValidator<StockDispDto>
{
    public StockDispValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.IdBodega).GreaterThan(0);
    }
}

public class MovInvValidator : AbstractValidator<MovInvDto>
{
    public MovInvValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdBodega).GreaterThan(0);
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.TipoMov).InclusiveBetween((byte)1, (byte)6);
        RuleFor(x => x.CantCom).NotEqual(0);
        RuleFor(x => x.CostoUni).GreaterThanOrEqualTo(0);
    }
}

public class PrecioValidator : AbstractValidator<PrecioDto>
{
    public PrecioValidator()
    {
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.Valor).GreaterThanOrEqualTo(0);
    }
}

// S57.69: mayorista normalizado (sin discriminator que validar).
public class PrecioMayoristaValidator : AbstractValidator<PrecioMayoristaDto>
{
    public PrecioMayoristaValidator()
    {
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.Valor).GreaterThanOrEqualTo(0);
    }
}

public class VentaValidator : AbstractValidator<VentaDto>
{
    public VentaValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdBodega).GreaterThan(0);
        RuleFor(x => x.NumVenta).NotEmpty().MaximumLength(20);
        RuleFor(x => x.TipoDoc).InclusiveBetween((byte)1, (byte)3);
        RuleFor(x => x.SubTotal).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Descuento).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Impuesto).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Estado).InclusiveBetween((byte)1, (byte)2);
    }
}

public class VentaDetalleValidator : AbstractValidator<VentaDetalleDto>
{
    public VentaDetalleValidator()
    {
        RuleFor(x => x.IdVenta).GreaterThan(0);
        RuleFor(x => x.NumLinea).GreaterThan(0);
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0);
    }
}

public class VentaPagoValidator : AbstractValidator<VentaPagoDto>
{
    public VentaPagoValidator()
    {
        RuleFor(x => x.IdVenta).GreaterThan(0);
        RuleFor(x => x.IdMetodoPago).GreaterThan(0);
        RuleFor(x => x.Monto).GreaterThan(0);
    }
}

public class OrdenCompraValidator : AbstractValidator<OrdenCompraDto>
{
    public OrdenCompraValidator()
    {
        RuleFor(x => x.IdTenant).GreaterThan(0);
        RuleFor(x => x.IdProveedor).GreaterThan(0);
        RuleFor(x => x.NumOrden).NotEmpty().MaximumLength(20);
        RuleFor(x => x.Estado).InclusiveBetween((byte)1, (byte)5);
        RuleFor(x => x.Total).GreaterThanOrEqualTo(0);
    }
}

public class OrdenCompraDetalleValidator : AbstractValidator<OrdenCompraDetalleDto>
{
    public OrdenCompraDetalleValidator()
    {
        RuleFor(x => x.IdOrdenCompra).GreaterThan(0);
        RuleFor(x => x.IdPresProd).GreaterThan(0);
        RuleFor(x => x.Cantidad).GreaterThan(0);
        RuleFor(x => x.PrecioUnitario).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Subtotal).GreaterThanOrEqualTo(0);
    }
}
