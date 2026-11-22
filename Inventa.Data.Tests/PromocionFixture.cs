using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Tests;

// S58.10: fixture oficial de escritura para el agregado Promocion.
// Marca: Codigo 'S5810-*' (T4). Sin Bodega/Proveedor/MetodoPago.
// Solo lectura de maestros: master ABA-T0001 (T4), resuelto por codigo.
public sealed class PromocionFixture
{
    public const int Tenant = 4;
    public const string CodigoPrefix = "S5810-";
    // S86: resuelto por codigo (ABA-T0001/T4) en SetupAsync, no por id fijo.
    public long IdPresProd { get; private set; }

    // Referencia de precio congelada (P12/P16): CK_PromocionPres_Muestra exige
    // IdPrecioReferencia NOT NULL cuando EsMuestra=0. Solo lectura, jamás se modifica.
    public long IdPrecioRef { get; private set; }

    private readonly OfficialLocalFixture _fx;

    public PromocionFixture(OfficialLocalFixture fx) => _fx = fx;

    public async Task SetupAsync()
    {
        using var ctx = _fx.NewContext();
        if (!await ctx.Database.CanConnectAsync())
            throw new InvalidOperationException("S5810: SQL no disponible. STOP, no skip.");
        (_, IdPresProd) = await _fx.MasterVarPresAsync();
        if (!await ctx.Presentaciones.AnyAsync(p => p.IdPresProd == IdPresProd))
            throw new InvalidOperationException("S5810: presentación master inexistente. STOP.");
        IdPrecioRef = await ctx.Precios.Where(p => p.IdPresProd == IdPresProd)
            .OrderBy(p => p.IdPrecio).Select(p => p.IdPrecio).FirstOrDefaultAsync();
        if (IdPrecioRef <= 0)
            throw new InvalidOperationException("S5810: sin Precio para la presentación master. STOP.");
        if (await ResidueCountAsync() != 0)
            throw new InvalidOperationException(
                "S5810: residuo de fixture previo; cleanup incompleto. STOP, no continuar.");
    }

    public PromocionRepositoryAsync NewRepo(InventaDbContext ctx) => new(ctx);

    public static Promocion Header(string codigo) => new()
    {
        IdTenant = Tenant,
        Codigo = codigo,
        Nombre = "S5810 " + codigo,
        TipoMecanica = 1,   // 1=2x1
        PoliticaPrecio = 1, // 1=A-congelado
        FechaDesde = DateOnly.FromDateTime(DateTime.UtcNow),
        Prioridad = 100,
        Exclusiva = false,
        Activo = true,
        Publicado = false
    };

    public static PromocionPresentacion Pres(long idPresProd, long? idPrecioRef = null) => new()
    {
        IdPresProd = idPresProd,
        IdPrecioReferencia = idPrecioRef,
        Rol = 1, // 1=Gatillo
        Cantidad = 2m,
        EsMuestra = false
    };

    public static PromocionRegla Regla() => new()
    {
        TipoRegla = 7, // CantMin
        Operador = 1,
        Valor = "2"
    };

    public static PromocionBeneficio Beneficio() => new()
    {
        TipoBeneficio = 1, // Pct
        Valor = 50m
    };

    public async Task CleanupAsync()
    {
        using var ctx = _fx.NewContext();
        var ids = await ctx.Promociones
            .Where(p => p.IdTenant == Tenant && p.Codigo.StartsWith(CodigoPrefix))
            .Select(p => p.IdPromocion)
            .ToListAsync();
        if (ids.Count > 0)
        {
            ctx.PromocionBeneficios.RemoveRange(
                ctx.PromocionBeneficios.Where(b => ids.Contains(b.IdPromocion)));
            ctx.PromocionReglas.RemoveRange(
                ctx.PromocionReglas.Where(r => ids.Contains(r.IdPromocion)));
            ctx.PromocionPresentaciones.RemoveRange(
                ctx.PromocionPresentaciones.Where(p => ids.Contains(p.IdPromocion)));
            await ctx.SaveChangesAsync();
            ctx.Promociones.RemoveRange(ctx.Promociones.Where(p => ids.Contains(p.IdPromocion)));
            await ctx.SaveChangesAsync();
        }
    }

    public async Task<int> ResidueCountAsync()
    {
        using var ctx = _fx.NewContext();
        var ids = await ctx.Promociones
            .Where(p => p.IdTenant == Tenant && p.Codigo.StartsWith(CodigoPrefix))
            .Select(p => p.IdPromocion)
            .ToListAsync();
        return ids.Count
            + await ctx.PromocionPresentaciones.CountAsync(p => ids.Contains(p.IdPromocion))
            + await ctx.PromocionReglas.CountAsync(r => ids.Contains(r.IdPromocion))
            + await ctx.PromocionBeneficios.CountAsync(b => ids.Contains(b.IdPromocion));
    }

    public async Task<(int Cab, int Pres, int Reg, int Ben)> CountAsync(string codigo)
    {
        using var ctx = _fx.NewContext();
        var ids = await ctx.Promociones
            .Where(p => p.IdTenant == Tenant && p.Codigo == codigo)
            .Select(p => p.IdPromocion)
            .ToListAsync();
        return (ids.Count,
            await ctx.PromocionPresentaciones.CountAsync(p => ids.Contains(p.IdPromocion)),
            await ctx.PromocionReglas.CountAsync(r => ids.Contains(r.IdPromocion)),
            await ctx.PromocionBeneficios.CountAsync(b => ids.Contains(b.IdPromocion)));
    }
}

