using CBP.Data.Abstractions;
using CBP.Data.Asynchronous;
using Inventa.Data.Async.Repositories;
using Inventa.Data.Context;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Inventa.Data.Tests;

// S74 §6: equivalencia SP (dbo.sp_Store_CatalogoCards vía StorefrontRepositoryAsync)
// vs oráculo independiente (CatalogCardsOracle: PublicCatalogSpecs compiladas en memoria).
// Solo lectura (OfficialLocal). Compara TODAS las páginas (pageSize=100): totalCount,
// orden de IdProd, sin duplicados y los 11 campos por card.
[Collection("OfficialLocal")]
public sealed class S74CatalogCardsEquivalenceTests : IDisposable
{
    private readonly OfficialLocalFixture _fx;
    private readonly ServiceProvider _provider;
    private readonly IServiceScope _scope;

    public S74CatalogCardsEquivalenceTests(OfficialLocalFixture fx)
    {
        _fx = fx;
        var services = new ServiceCollection();
        services.AddDbContext<InventaDbContext>(o => o.UseSqlServer(OfficialLocalFixture.ConnectionString));
        services.AddUnitOfWorkAsync<InventaDbContext>();
        _provider = services.BuildServiceProvider();
        _scope = _provider.CreateScope();
    }

    public void Dispose()
    {
        _scope.Dispose();
        _provider.Dispose();
    }

    private (StorefrontRepositoryAsync Repo, InventaDbContext Ctx) Build()
    {
        var ctx = _scope.ServiceProvider.GetRequiredService<InventaDbContext>();
        var uow = _scope.ServiceProvider.GetRequiredService<IUnitOfWorkAsync>();
        return (new StorefrontRepositoryAsync(ctx, uow), ctx);
    }

    // S86: T4 (ABARROTES/PP2) con 10 productos fixture VisibleStore=1.
    // S90: también T5 (VESTUARIO/PP3) con el catálogo Ficcus (2.754 productos / 12.114 presentaciones).
    private static int ResolveTenant(InventaDbContext ctx, int idTenantPassPlat = 2) =>
        ctx.Tenants.AsNoTracking().Where(t => t.IdTenantPassPlat == idTenantPassPlat)
            .Select(t => t.IdTenant).Single();

    // Resuelve los casos simbólicos de categoría/marca por tenant (sin ids fijos).
    private static (long? Cat, long? Marca) Resolver(InventaDbContext ctx, int tenant, long? idCategoria, string? marcaCaso)
    {
        long? cat = idCategoria switch
        {
            -1 => ctx.Categorias.AsNoTracking()
                .Where(c => c.IdTenant == tenant && c.Activo
                    && !ctx.Categorias.Any(h => h.IdPadre == c.IdCategoria)
                    && ctx.Productos.Any(p => p.IdCategoria == c.IdCategoria && p.IdTenant == tenant
                        && p.Activo && p.VisibleStore))
                .OrderBy(c => c.IdCategoria).Select(c => c.IdCategoria).First(),
            // S90: raíz con hijas activas que tengan productos publicables; si el tenant no
            // tiene jerarquía, cualquier raíz con productos (igual de válido para comparar).
            -2 => ctx.Categorias.AsNoTracking()
                .Where(c => c.IdTenant == tenant && c.IdPadre == null && c.Activo)
                .OrderByDescending(c => ctx.Categorias.Any(h => h.IdPadre == c.IdCategoria && h.Activo
                        && ctx.Productos.Any(p => p.IdCategoria == h.IdCategoria && p.IdTenant == tenant
                            && p.Activo && p.VisibleStore)))
                .ThenBy(c => c.IdCategoria).Select(c => (long?)c.IdCategoria).FirstOrDefault(),
            _ => idCategoria,
        };
        long? marca = marcaCaso switch
        {
            "EXISTENTE" => ctx.Marcas.AsNoTracking()
                .Where(m => m.IdTenant == tenant && m.Activo)
                .OrderBy(m => m.IdMarca).Select(m => (long)m.IdMarca).First(),
            "INEXISTENTE" => 999999L,
            "OVERFLOW" => (long)int.MaxValue + 1,
            _ => null,
        };
        return (cat, marca);
    }

    public static TheoryData<int, string, string?, long?, string?, bool, decimal?, decimal?, string?> VariantsPorTenant()
    {
        var data = new TheoryData<int, string, string?, long?, string?, bool, decimal?, decimal?, string?>();
        foreach (var pp in new[] { 2, 3 })
            foreach (var r in BaseVariants)
                data.Add(pp, r.Nombre, r.Q, r.Cat, r.Marca,
                    r.SoloOfertas, r.Min, r.Max, r.Orden);
        return data;
    }

    // S92.2-K4 (xUnit v3): TheoryDataRow<> ya no admite indizacion; la tabla base vive
    // en tuplas y ambos MemberData se construyen desde ella (mismos 16 casos x tenant).
    private static readonly (string Nombre, string? Q, long? Cat, string? Marca, bool SoloOfertas, decimal? Min, decimal? Max, string? Orden)[] BaseVariants =
    [
        ("defecto", null, null, null, false, null, null, null),
        ("precio_asc", null, null, null, false, null, null, "precio_asc"),
        ("precio_desc", null, null, null, false, null, null, "precio_desc"),
        ("descuento_desc", null, null, null, false, null, null, "descuento_desc"),
        ("nombre_asc", null, null, null, false, null, null, "nombre_asc"),
        ("soloOfertas", null, null, null, true, null, null, null),
        ("soloOfertas+descuento_desc", null, null, null, true, null, null, "descuento_desc"),
        ("idCategoria-hoja", null, -1, null, false, null, null, null),
        ("idCategoria-raiz", null, -2, null, false, null, null, "nombre_asc"),
        ("q=pol", "pol", null, null, false, null, null, null),
        ("q=blancos", "  ", null, null, false, null, null, null),
        ("precioMin-Max", null, null, null, false, 5000m, 20000m, null),
        ("idMarca-existente", null, null, "EXISTENTE", false, null, null, null),
        ("idMarca-inexistente", null, null, "INEXISTENTE", false, null, null, null),
        // Adicionales S74 (B4/B5): orden desconocido → defecto; idMarca overflow → (0, []).
        ("orden-desconocido", null, null, null, false, null, null, "zzz"),
        ("idMarca-overflow", null, null, "OVERFLOW", false, null, null, null),
    ];

    public static TheoryData<string, string?, long?, string?, bool, decimal?, decimal?, string?> Variants()
    {
        // nombre, q, idCategoria("HOJA"=resuelve hoja con publicables), idMarca
        // ("EXISTENTE"/"INEXISTENTE"/"OVERFLOW"=fuera de rango int), soloOfertas, min, max, orden.
        var data = new TheoryData<string, string?, long?, string?, bool, decimal?, decimal?, string?>();
        foreach (var r in BaseVariants)
            data.Add(r.Nombre, r.Q, r.Cat, r.Marca, r.SoloOfertas, r.Min, r.Max, r.Orden);
        return data;
    }

    [Theory]
    [MemberData(nameof(VariantsPorTenant))]
    public async Task Variante_Equivale_Al_Oraculo(
        int tenantPassPlat, string nombre, string? q, long? idCategoria, string? marcaCaso,
        bool soloOfertas, decimal? min, decimal? max, string? orden)
    {
        var (repo, ctx) = Build();
        var tenant = ResolveTenant(ctx, tenantPassPlat);
        nombre = $"PP{tenantPassPlat}:{nombre}";

        var (cat, marcaLong) = Resolver(ctx, tenant, idCategoria, marcaCaso);
        if (marcaCaso == "INEXISTENTE")
            Assert.False(ctx.Marcas.Any(m => m.IdMarca == 999999), "La marca 999999 no debe existir.");

        var oParams = new CatalogCardsOracle.Params(
            tenant, q, cat, marcaLong is null or > int.MaxValue or < int.MinValue ? null : (int?)marcaLong,
            soloOfertas, min, max, orden);
        // OVERFLOW: el repo devuelve (0, []) sin llamar al SP; el oráculo se fuerza a vacío.
        var (expTotal, expAll) = marcaCaso == "OVERFLOW"
            ? (0, (IReadOnlyList<PublicCardItem>)Array.Empty<PublicCardItem>())
            : CatalogCardsOracle.Build(ctx, oParams);

        var got = new List<PublicCardItem>();
        var total = -1;
        var page = 1;
        while (true)
        {
            var (t, items) = await repo.GetPublicCardsAsync(
                tenant, q, page, 100, cat, soloOfertas, min, max, marcaLong, orden, Xunit.TestContext.Current.CancellationToken);
            total = t;
            got.AddRange(items);
            if (items.Count < 100) break;
            page++;
            Assert.True(page <= 100, $"[{nombre}] paginación sin fin (page={page}).");
        }

        Assert.Equal(expTotal, total);
        Assert.Equal(expAll.Count, got.Count);
        Assert.Equal(got.Count, got.Select(c => c.IdProd).Distinct().Count());
        Assert.Equal(expAll.Select(c => c.IdProd), got.Select(c => c.IdProd));
        for (var i = 0; i < expAll.Count; i++)
        {
            var e = expAll[i];
            var g = got[i];
            Assert.True(e.Equals(g),
                $"[{nombre}][{i}] IdProd={e.IdProd} difiere:\n esperado={e}\n obtenido={g}");
        }
    }

    // S90: el SP v4 pagina ANTES de enriquecer en la ruta rápida; se comparan las páginas
    // 1, 2 y la última con PageSize 24 (tamaño real del Store) contra el oráculo.
    [Theory]
    [MemberData(nameof(VariantsPorTenant))]
    public async Task Paginas_24_Equivalen_Al_Oraculo(
        int tenantPassPlat, string nombre, string? q, long? idCategoria, string? marcaCaso,
        bool soloOfertas, decimal? min, decimal? max, string? orden)
    {
        if (marcaCaso == "OVERFLOW") return; // el repo no llama al SP (cubierto por la otra teoría)
        const int size = 24;
        var (repo, ctx) = Build();
        var tenant = ResolveTenant(ctx, tenantPassPlat);
        nombre = $"PP{tenantPassPlat}:{nombre}";
        var (cat, marcaLong) = Resolver(ctx, tenant, idCategoria, marcaCaso);

        var (expTotal, expAll) = CatalogCardsOracle.Build(ctx, new CatalogCardsOracle.Params(
            tenant, q, cat, (int?)marcaLong, soloOfertas, min, max, orden));
        var ultima = Math.Max(1, (expTotal + size - 1) / size);

        foreach (var page in new[] { 1, 2, ultima }.Distinct())
        {
            var (total, items) = await repo.GetPublicCardsAsync(
                tenant, q, page, size, cat, soloOfertas, min, max, marcaLong, orden, Xunit.TestContext.Current.CancellationToken);
            var esperado = expAll.Skip((page - 1) * size).Take(size).ToList();
            Assert.Equal(expTotal, total);
            Assert.Equal(esperado.Select(c => c.IdProd), items.Select(c => c.IdProd));
            for (var i = 0; i < esperado.Count; i++)
                Assert.True(esperado[i].Equals(items[i]),
                    $"[{nombre}][p{page}][{i}] IdProd={esperado[i].IdProd} difiere:\n esperado={esperado[i]}\n obtenido={items[i]}");
        }
    }

    [Fact]
    public async Task Pagina_Fuera_De_Rango_Items_Vacios_Total_Real()
    {
        var (repo, ctx) = Build();
        var tenant = ResolveTenant(ctx);
        var (expTotal, _) = CatalogCardsOracle.Build(
            ctx, new CatalogCardsOracle.Params(tenant, null, null, null, false, null, null, null));
        // S86: la semilla es minima (10 productos); la pagina se calcula mas
        // alla del total en vez de exigir >100.
        var page = expTotal / 100 + 2;
        var (total, items) = await repo.GetPublicCardsAsync(tenant, null, page, 100, null, ct: Xunit.TestContext.Current.CancellationToken);
        Assert.Empty(items);
        Assert.Equal(expTotal, total);
    }

    [Fact]
    public void MapRow_Oferta_Con_FechaHasta_Mapea_DateOnly()
    {
        // Cubre la rama que los datos actuales no ejercitan (0 ofertas con FechaHasta).
        // S77 R2': el row simula el SP con S77 (incluye PrecioReferencia).
        var row = new StorefrontRepositoryAsync.SpPublicCardRow
        {
            IdProd = 1, Codigo = "X", Nombre = "X", IdCategoria = 1, CatNombre = "C",
            TieneOferta = true, PrecioDesde = 100m, PrecioFinal = 80m,
            PrecioVigente = 100m, OfertaPrecio = 80m, PrecioReferencia = 100m,
            OfertaHasta = new DateOnly(2026, 12, 31),
        };
        var c = StorefrontRepositoryAsync.MapRow(row);
        Assert.Equal(80m, c.PrecioFinalDesde);
        Assert.Equal(100m, c.PrecioReferencia);
        Assert.Equal(PublicCatalogSpecs.PctDescuento(100m, 80m), c.PctDescuento);
        Assert.Equal(new DateOnly(2026, 12, 31), c.OfertaHasta);
        Assert.True(c.TieneOferta);
    }

    [Fact]
    public void MapRow_Sin_Oferta_Referencia_Nula_Sin_Tachado()
    {
        // Regresión B1 a nivel de mapeo.
        var row = new StorefrontRepositoryAsync.SpPublicCardRow
        {
            IdProd = 2, Codigo = "Y", Nombre = "Y", IdCategoria = 1, CatNombre = "C",
            TieneOferta = false, PrecioDesde = 100m, PrecioFinal = 100m,
            PrecioVigente = 100m, OfertaPrecio = null, OfertaHasta = null,
        };
        var c = StorefrontRepositoryAsync.MapRow(row);
        Assert.Null(c.PrecioReferencia);
        Assert.Null(c.PctDescuento);
        Assert.Null(c.OfertaHasta);
        Assert.Equal(100m, c.PrecioFinalDesde);
    }

    [Fact]
    public void MapRow_OfertaHasta_Huerfana_Se_Ignora()
    {
        // OfertaHasta sin OfertaPrecio (inconsistencia defensiva) → null.
        var row = new StorefrontRepositoryAsync.SpPublicCardRow
        {
            IdProd = 3, Codigo = "Z", Nombre = "Z", IdCategoria = 1, CatNombre = "C",
            TieneOferta = false, PrecioVigente = 50m, OfertaPrecio = null,
            OfertaHasta = new DateOnly(2027, 1, 1),
        };
        var c = StorefrontRepositoryAsync.MapRow(row);
        Assert.Null(c.OfertaHasta);
        Assert.Null(c.PrecioReferencia);
    }

    [Fact]
    public void MapRow_S77_Referencia_Congelada_Distinta_De_Vigente()
    {
        // R2'/R4': la card usa la referencia del SP, no el vigente recalculado.
        var row = new StorefrontRepositoryAsync.SpPublicCardRow
        {
            IdProd = 4, Codigo = "W", Nombre = "W", IdCategoria = 1, CatNombre = "C",
            TieneOferta = true, PrecioDesde = 90m, PrecioFinal = 80m,
            PrecioVigente = 90m, OfertaPrecio = 80m, PrecioReferencia = 100m,
            OfertaHasta = null,
        };
        var c = StorefrontRepositoryAsync.MapRow(row);
        Assert.Equal(100m, c.PrecioReferencia);
        Assert.Equal(PublicCatalogSpecs.PctDescuento(100m, 80m), c.PctDescuento);
    }
}
