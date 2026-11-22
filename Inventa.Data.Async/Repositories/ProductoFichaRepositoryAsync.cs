using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S92: lectura para la administración de productos — diagnóstico por producto, listado con filtros
/// en el servidor y ficha completa de un producto. Solo EF/LINQ (sin SQL en strings); las reglas de
/// publicación/precio/oferta salen de <see cref="PublicCatalogSpecs"/>.
/// </summary>
public interface IProductoFichaRepositoryAsync
{
    Task<IReadOnlyList<ProductoDiagnosticoRow>> GetDiagnosticosAsync(int idTenant, CancellationToken ct = default);
    Task<(IReadOnlyList<ProductoDiagnosticoRow> Items, int Total)> SearchListadoAsync(int idTenant, ProductoListadoFiltro filtro, int page, int pageSize, CancellationToken ct = default);
    Task<IReadOnlyDictionary<long, (decimal Min, decimal Max)>> GetRangoPrecioVigenteAsync(IReadOnlyCollection<long> idsProd, CancellationToken ct = default);
    Task<FichaProductoBundle?> GetFichaAsync(int idTenant, long idProd, CancellationToken ct = default);
}

/// <summary>S92 K4: filtros del listado (todos opcionales, se aplican en SQL).</summary>
public sealed record ProductoListadoFiltro(
    string? Texto = null, string? Estado = null, bool? ConPrecio = null, bool? ConImagen = null,
    bool? ConMarca = null, long? IdCategoria = null, bool? Activo = null);

public sealed record FichaCabecera(
    long IdProd, string Codigo, string Nombre, string Categoria, string? CategoriaPadre, string? Marca,
    string UnidadCodigo, string UnidadNombre, bool EsPesable, bool ManejaLote, int? DiasVen,
    bool Activo, bool VisibleStore);

public sealed record FichaProductoBundle(
    FichaCabecera Cabecera,
    ProductoDiagnosticoRow Diagnostico,
    IReadOnlyList<VarProd> Variantes,
    IReadOnlyList<PresProd> Presentaciones,
    IReadOnlyList<CodBar> Codigos,
    IReadOnlyList<Precio> PreciosCandidatos,
    IReadOnlyList<Oferta> OfertasVigentes,
    IReadOnlyDictionary<long, decimal> StockPorPresentacion,
    int Imagenes,
    string? ImagenPrincipal,
    DateOnly Hoy);

public sealed class ProductoFichaRepositoryAsync : IProductoFichaRepositoryAsync
{
    private readonly InventaDbContext _ctx;

    public ProductoFichaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    private IQueryable<ProductoDiagnosticoRow> Diagnosticos(int idTenant, DateOnly hoy, IQueryable<Prod>? baseQuery = null)
        => (baseQuery ?? _ctx.Productos.AsNoTracking().Where(p => p.IdTenant == idTenant))
            .Select(PublicCatalogSpecs.Diagnostico(hoy, _ctx.ProductoImagenes.AsNoTracking()));

    public async Task<IReadOnlyList<ProductoDiagnosticoRow>> GetDiagnosticosAsync(int idTenant, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        return await Diagnosticos(idTenant, PublicCatalogSpecs.Hoy()).ToListAsync(ct);
    }

    public async Task<(IReadOnlyList<ProductoDiagnosticoRow> Items, int Total)> SearchListadoAsync(
        int idTenant, ProductoListadoFiltro filtro, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (page < 1) throw new ArgumentOutOfRangeException(nameof(page));
        if (pageSize is < 1 or > 100) throw new ArgumentOutOfRangeException(nameof(pageSize));

        var prods = _ctx.Productos.AsNoTracking().Where(p => p.IdTenant == idTenant);
        var ids = ProdRepositoryAsync.IdsPorTexto(_ctx, idTenant, filtro.Texto);
        if (ids is not null) prods = prods.Where(p => ids.Contains(p.IdProd));
        if (filtro.IdCategoria is { } cat) prods = prods.Where(p => p.IdCategoria == cat || p.Categoria.IdPadre == cat);
        if (filtro.Activo is { } act) prods = prods.Where(p => p.Activo == act);
        if (filtro.ConMarca is { } cm) prods = prods.Where(p => (p.IdMarca != null) == cm);

        var q = Diagnosticos(idTenant, PublicCatalogSpecs.Hoy(), prods);
        if (filtro.ConPrecio is { } cp) q = q.Where(r => r.TienePrecio == cp);
        if (filtro.ConImagen is { } ci) q = q.Where(r => r.TieneImagen == ci);
        q = filtro.Estado switch
        {
            EstadosTienda.Publicado => q.Where(r => r.VisibleStore && r.Activo && r.Vendible && r.TienePrecio),
            EstadosTienda.PublicadoConAdvertencia => q.Where(r => r.VisibleStore && !(r.Activo && r.Vendible && r.TienePrecio)),
            EstadosTienda.Listo => q.Where(r => !r.VisibleStore && r.Activo && r.Vendible),
            EstadosTienda.Pendiente => q.Where(r => !r.VisibleStore && r.Activo && !r.Vendible),
            EstadosTienda.Inactivo => q.Where(r => !r.VisibleStore && !r.Activo),
            _ => q,
        };

        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(r => r.Codigo).ThenBy(r => r.IdProd)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<IReadOnlyDictionary<long, (decimal Min, decimal Max)>> GetRangoPrecioVigenteAsync(
        IReadOnlyCollection<long> idsProd, CancellationToken ct = default)
    {
        if (idsProd.Count == 0) return new Dictionary<long, (decimal, decimal)>();
        var ids = idsProd.ToList();
        var filas = await _ctx.Precios.AsNoTracking()
            .Where(PublicCatalogSpecs.PrecioCandidato(PublicCatalogSpecs.Hoy()))
            .Where(pc => ids.Contains(pc.PresProd.VarProd.IdProd)
                && pc.PresProd.Activo && pc.PresProd.EsVenta && pc.PresProd.VarProd.Activo)
            .Select(pc => new { pc.PresProd.VarProd.IdProd, pc.IdPresProd, pc.IdPrecio, pc.FecVig, pc.Valor })
            .ToListAsync(ct);
        // R1 por presentación (mayor FecVig, luego mayor IdPrecio); rango por producto.
        return filas
            .GroupBy(f => f.IdPresProd)
            .Select(g => g.OrderByDescending(f => f.FecVig).ThenByDescending(f => f.IdPrecio).First())
            .GroupBy(f => f.IdProd)
            .ToDictionary(g => g.Key, g => (g.Min(f => f.Valor), g.Max(f => f.Valor)));
    }

    public async Task<FichaProductoBundle?> GetFichaAsync(int idTenant, long idProd, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        if (idProd <= 0) throw new ArgumentOutOfRangeException(nameof(idProd));
        var hoy = PublicCatalogSpecs.Hoy();

        var prodQ = _ctx.Productos.AsNoTracking().Where(p => p.IdProd == idProd && p.IdTenant == idTenant);
        var cab = await prodQ
            .Select(p => new FichaCabecera(
                p.IdProd, p.Codigo, p.Nombre, p.Categoria.Nombre,
                p.Categoria.Padre != null ? p.Categoria.Padre.Nombre : null,
                p.Marca != null ? p.Marca.Nombre : null,
                p.UnidadBase.Codigo, p.UnidadBase.Nombre, p.EsPesable, p.ManejaLot, p.DiasVen, p.Activo, p.VisibleStore))
            .FirstOrDefaultAsync(ct);
        if (cab is null) return null;

        var diag = await Diagnosticos(idTenant, hoy, prodQ).FirstAsync(ct);
        var variantes = await _ctx.Variantes.AsNoTracking().Where(v => v.IdProd == idProd)
            .OrderBy(v => v.Codigo).ToListAsync(ct);
        var varIds = variantes.Select(v => v.IdVarProd).ToList();
        var pres = await _ctx.Presentaciones.AsNoTracking().Where(pp => varIds.Contains(pp.IdVarProd)).ToListAsync(ct);
        var presIds = pres.Select(pp => pp.IdPresProd).ToList();
        var codigos = await _ctx.CodigosBarra.AsNoTracking().Where(c => presIds.Contains(c.IdPresProd))
            .OrderByDescending(c => c.Principal).ThenBy(c => c.Codigo).ToListAsync(ct);
        var precios = await _ctx.Precios.AsNoTracking().Where(PublicCatalogSpecs.PrecioCandidato(hoy))
            .Where(pc => presIds.Contains(pc.IdPresProd)).ToListAsync(ct);
        var ofertas = await _ctx.Ofertas.AsNoTracking().Include(o => o.PrecioReferencia)
            .Where(PublicCatalogSpecs.OfertaVigente(hoy))
            .Where(o => presIds.Contains(o.IdPresProd)).ToListAsync(ct);
        var stock = await _ctx.StockDisponible.AsNoTracking()
            .Where(s => s.IdTenant == idTenant && presIds.Contains(s.IdPresProd))
            .GroupBy(s => s.IdPresProd)
            .Select(g => new { IdPresProd = g.Key, Cant = g.Sum(s => s.CantDisp) })
            .ToDictionaryAsync(x => x.IdPresProd, x => x.Cant, ct);
        var imagenes = _ctx.ProductoImagenes.AsNoTracking().Where(i => i.IdProd == idProd && i.Activo);
        var nImagenes = await imagenes.CountAsync(ct);
        var principal = await imagenes.OrderByDescending(i => i.EsPrincipal).ThenBy(i => i.Orden)
            .Select(i => i.Url).FirstOrDefaultAsync(ct);

        return new FichaProductoBundle(cab, diag, variantes, pres, codigos, precios, ofertas,
            stock, nImagenes, principal, hoy);
    }
}
