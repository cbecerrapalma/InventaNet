using Inventa.Data.Context;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Async.Repositories;

/// <summary>
/// S93.1 C2/C3 — lectura de la página «Precios y ofertas» y aplicación atómica de los cambios.
/// Solo EF/LINQ (sin SQL en strings). Las reglas salen de <see cref="PublicCatalogSpecs"/>:
/// R1 (precio vigente) y R2 + R2' + desempate de la tienda (sp_Store_CatalogoCards v4).
/// Todos los filtros se aplican en SQL antes de paginar.
/// </summary>
public interface IPreciosGrillaRepositoryAsync
{
    Task<(IReadOnlyList<PrecioGrillaRow> Items, int Total)> GetPaginaAsync(int idTenant, PreciosGrillaFiltro filtro, DateOnly hoy, int page, int pageSize, CancellationToken ct = default);
    Task<PreciosResumenRow> GetResumenAsync(int idTenant, DateOnly hoy, CancellationToken ct = default);
    Task<IReadOnlyList<PrecioGrillaRow>> GetPorIdsAsync(int idTenant, IReadOnlyCollection<long> idsPresProd, DateOnly hoy, CancellationToken ct = default);
    Task<PreciosHistorial?> GetHistorialAsync(int idTenant, long idPresProd, DateOnly hoy, CancellationToken ct = default);
    Task<IReadOnlyDictionary<long, SnapshotPrecios>> GetSnapshotsAsync(IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default);
    /// <summary>
    /// Carga las instantáneas DENTRO de una transacción serializable, llama a <paramref name="planificar"/> y,
    /// si devuelve operaciones, las aplica y confirma. Si devuelve <c>null</c> no escribe nada.
    /// </summary>
    Task<(TResult Resultado, IReadOnlyDictionary<int, long> PreciosNuevos)> EjecutarAsync<TResult>(IReadOnlyCollection<long> idsPresProd,
        Func<IReadOnlyDictionary<long, SnapshotPrecios>, (IReadOnlyList<OperacionPrecio>? Ops, TResult Resultado)> planificar,
        CancellationToken ct = default);
}

/// <summary>Estados de filtro de la grilla (S93 K2).</summary>
public enum FiltroEstadoPrecio { Todos = 0, SinPrecio, ConOferta, SinOferta, OfertaSinEfecto, VenceEn7Dias, Programado }

public sealed record PreciosGrillaFiltro(
    string? Texto = null, long? IdCategoria = null, int? IdMarca = null,
    FiltroEstadoPrecio Estado = FiltroEstadoPrecio.Todos, bool IncluirInactivos = false);

/// <summary>Fila de la grilla (clase con setters para que EF pueda filtrar y ordenar sobre la proyección).</summary>
public sealed class PrecioGrillaRow
{
    public long IdPresProd { get; set; }
    public long IdProd { get; set; }
    public long IdVarProd { get; set; }
    public string CodigoProducto { get; set; } = "";
    public string CodigoVariante { get; set; } = "";
    public string Producto { get; set; } = "";
    public string Variante { get; set; } = "";
    public string Categoria { get; set; } = "";
    public string? Marca { get; set; }
    public string? CodigoBarras { get; set; }
    public bool Vendible { get; set; }
    public bool Publicable { get; set; }
    public decimal CostoProm { get; set; }
    public long? IdPrecio { get; set; }
    public decimal? PrecioValor { get; set; }
    public DateOnly? PrecioDesde { get; set; }
    public long? IdPrecioProgramado { get; set; }
    public decimal? PrecioProgramadoValor { get; set; }
    public DateOnly? PrecioProgramadoDesde { get; set; }
    public long? IdOferta { get; set; }
    public decimal? OfertaPrecio { get; set; }
    public DateOnly? OfertaDesde { get; set; }
    public DateOnly? OfertaHasta { get; set; }
    public int OfertasActivasHoy { get; set; }
    public long? IdOfertaProgramada { get; set; }
    public decimal? OfertaProgramadaPrecio { get; set; }
    public DateOnly? OfertaProgramadaDesde { get; set; }
    public DateOnly? OfertaProgramadaHasta { get; set; }
}

public sealed record PreciosResumenRow(int Vendibles, int SinPrecio, int ConOferta, int VencenEn7Dias, int OfertasSinEfecto, int Programados);

public sealed record PreciosHistorial(PrecioGrillaRow Fila, IReadOnlyList<Precio> Precios, IReadOnlyList<Oferta> Ofertas);

/// <summary>Estado completo de precios de una presentación (para planificar cambios).</summary>
public sealed record SnapshotPrecios(
    long IdPresProd, int IdTenant, string CodigoVariante, string Nombre,
    IReadOnlyList<Precio> Precios, IReadOnlyList<Oferta> OfertasActivas);

/// <summary>Operaciones de escritura que produce el planificador (Application) y aplica este repositorio.</summary>
public abstract record OperacionPrecio(long IdPresProd);
/// <summary>INSERT de una vigencia. <paramref name="Clave"/> identifica el precio nuevo dentro del lote.</summary>
public sealed record OpInsertarPrecio(long IdPresProd, int Clave, decimal Valor, DateOnly FecVig) : OperacionPrecio(IdPresProd);
/// <summary>INSERT de una oferta; la referencia es un precio existente o uno nuevo del lote (por clave).</summary>
public sealed record OpInsertarOferta(long IdPresProd, long? IdPrecioReferencia, int? ClavePrecioNuevo,
    DateOnly Desde, DateOnly? Hasta, decimal PrecioOferta, decimal PctReferencial) : OperacionPrecio(IdPresProd);
/// <summary>Termina una oferta ya empezada: FechaHasta = fecha (≥ FechaDesde).</summary>
public sealed record OpCerrarOferta(long IdPresProd, long IdOferta, DateOnly FechaHasta) : OperacionPrecio(IdPresProd);
/// <summary>Anula una oferta que aún no empieza (Activo = 0).</summary>
public sealed record OpAnularOferta(long IdPresProd, long IdOferta) : OperacionPrecio(IdPresProd);
/// <summary>Anula una vigencia programada que aún no rige (Activo = 0).</summary>
public sealed record OpAnularPrecio(long IdPresProd, long IdPrecio) : OperacionPrecio(IdPresProd);

public sealed class PreciosGrillaRepositoryAsync : IPreciosGrillaRepositoryAsync
{
    private readonly InventaDbContext _ctx;
    public PreciosGrillaRepositoryAsync(InventaDbContext ctx) => _ctx = ctx;

    public async Task<(IReadOnlyList<PrecioGrillaRow> Items, int Total)> GetPaginaAsync(
        int idTenant, PreciosGrillaFiltro filtro, DateOnly hoy, int page, int pageSize, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        ArgumentNullException.ThrowIfNull(filtro);
        var q = Filtrar(Filas(idTenant, filtro, hoy, null), filtro.Estado, hoy);
        var total = await q.CountAsync(ct);
        var items = await q.OrderBy(r => r.Producto).ThenBy(r => r.Variante).ThenBy(r => r.IdPresProd)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<PreciosResumenRow> GetResumenAsync(int idTenant, DateOnly hoy, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        var hasta7 = hoy.AddDays(7);
        // Una sola consulta agregada sobre la proyección (no carga filas en memoria).
        var r = await Filas(idTenant, new PreciosGrillaFiltro(), hoy, null)
            .GroupBy(_ => 1)
            .Select(g => new PreciosResumenRow(
                g.Count(),
                g.Count(x => x.IdPrecio == null),
                g.Count(x => x.IdOferta != null),
                g.Count(x => x.IdOferta != null && x.OfertaHasta != null && x.OfertaHasta <= hasta7),
                g.Count(x => x.OfertasActivasHoy > (x.IdOferta != null ? 1 : 0)),
                g.Count(x => x.IdPrecioProgramado != null || x.IdOfertaProgramada != null)))
            .FirstOrDefaultAsync(ct);
        return r ?? new PreciosResumenRow(0, 0, 0, 0, 0, 0);
    }

    public async Task<IReadOnlyList<PrecioGrillaRow>> GetPorIdsAsync(int idTenant, IReadOnlyCollection<long> idsPresProd, DateOnly hoy, CancellationToken ct = default)
    {
        if (idTenant <= 0) throw new ArgumentOutOfRangeException(nameof(idTenant));
        ArgumentNullException.ThrowIfNull(idsPresProd);
        if (idsPresProd.Count == 0) return [];
        var ids = idsPresProd.Distinct().ToList();
        return await Filas(idTenant, new PreciosGrillaFiltro(IncluirInactivos: true), hoy, ids)
            .OrderBy(r => r.Producto).ThenBy(r => r.Variante).ThenBy(r => r.IdPresProd)
            .ToListAsync(ct);
    }

    public async Task<PreciosHistorial?> GetHistorialAsync(int idTenant, long idPresProd, DateOnly hoy, CancellationToken ct = default)
    {
        var fila = (await GetPorIdsAsync(idTenant, [idPresProd], hoy, ct)).FirstOrDefault();
        if (fila is null) return null;
        var precios = await _ctx.Precios.AsNoTracking().Where(p => p.IdPresProd == idPresProd)
            .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio).ToListAsync(ct);
        var ofertas = await _ctx.Ofertas.AsNoTracking().Where(o => o.IdPresProd == idPresProd)
            .OrderByDescending(o => o.FechaDesde).ThenByDescending(o => o.IdOferta).ToListAsync(ct);
        return new PreciosHistorial(fila, precios, ofertas);
    }

    public Task<IReadOnlyDictionary<long, SnapshotPrecios>> GetSnapshotsAsync(IReadOnlyCollection<long> idsPresProd, CancellationToken ct = default) =>
        CargarSnapshotsAsync(idsPresProd, ct);

    public async Task<(TResult Resultado, IReadOnlyDictionary<int, long> PreciosNuevos)> EjecutarAsync<TResult>(IReadOnlyCollection<long> idsPresProd,
        Func<IReadOnlyDictionary<long, SnapshotPrecios>, (IReadOnlyList<OperacionPrecio>? Ops, TResult Resultado)> planificar,
        CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(idsPresProd);
        ArgumentNullException.ThrowIfNull(planificar);
        // Si ya hay una transacción abierta (p. ej. una prueba que revierte), se participa de ella sin confirmar.
        if (_ctx.Database.CurrentTransaction is not null)
        {
            var snapshotsAmb = await CargarSnapshotsAsync(idsPresProd, ct);
            var (opsAmb, resAmb) = planificar(snapshotsAmb);
            if (opsAmb is null || opsAmb.Count == 0) return (resAmb, new Dictionary<int, long>());
            return (resAmb, await AplicarAsync(opsAmb, ct));
        }

        const int maxReintentos = 2;
        for (var intento = 0; ; intento++)
        {
            _ctx.ChangeTracker.Clear();
            await using var tx = await _ctx.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable, ct);
            try
            {
                var snapshots = await CargarSnapshotsAsync(idsPresProd, ct);
                var (ops, resultado) = planificar(snapshots);
                if (ops is null || ops.Count == 0)
                {
                    await tx.RollbackAsync(ct);
                    return (resultado, new Dictionary<int, long>());
                }
                var nuevos = await AplicarAsync(ops, ct);
                await tx.CommitAsync(ct);
                return (resultado, nuevos);
            }
            catch (Exception ex) when (EsDeadlock(ex) && intento < maxReintentos)
            {
                try { await tx.RollbackAsync(ct); } catch (InvalidOperationException) { /* ya revertida */ }
                await Task.Delay((intento == 0 ? 100 : 400) + Random.Shared.Next(0, 50), ct);
            }
            finally
            {
                _ctx.ChangeTracker.Clear();
            }
        }
    }

    private async Task<IReadOnlyDictionary<int, long>> AplicarAsync(IReadOnlyList<OperacionPrecio> ops, CancellationToken ct)
    {
        var nuevos = new Dictionary<int, Precio>();
        var idsOferta = ops.Select(o => o switch { OpCerrarOferta c => c.IdOferta, OpAnularOferta a => a.IdOferta, _ => 0L })
            .Where(id => id > 0).Distinct().ToList();
        var idsPrecio = ops.OfType<OpAnularPrecio>().Select(a => a.IdPrecio).Distinct().ToList();
        var ofertas = idsOferta.Count == 0 ? new Dictionary<long, Oferta>()
            : await _ctx.Ofertas.Where(o => idsOferta.Contains(o.IdOferta)).ToDictionaryAsync(o => o.IdOferta, ct);
        var precios = idsPrecio.Count == 0 ? new Dictionary<long, Precio>()
            : await _ctx.Precios.Where(p => idsPrecio.Contains(p.IdPrecio)).ToDictionaryAsync(p => p.IdPrecio, ct);
        var ahora = DateTime.Now;

        foreach (var op in ops)
        {
            switch (op)
            {
                case OpInsertarPrecio ip:
                    var precio = new Precio { IdPresProd = ip.IdPresProd, Valor = ip.Valor, FecVig = ip.FecVig, Activo = true };
                    _ctx.Precios.Add(precio);
                    nuevos[ip.Clave] = precio;
                    break;
                case OpInsertarOferta io:
                    var oferta = new Oferta
                    {
                        IdPresProd = io.IdPresProd,
                        FechaDesde = io.Desde,
                        FechaHasta = io.Hasta,
                        PrecioOferta = io.PrecioOferta,
                        PctReferencial = io.PctReferencial,
                        Prioridad = 100,
                        Activo = true,
                        Publicado = true,   // D2: una oferta de la página rige en tienda y POS por igual.
                        OrigenMigracion = 0,
                        FecCrea = ahora
                    };
                    if (io.ClavePrecioNuevo is int clave) oferta.PrecioReferencia = nuevos[clave];
                    else oferta.IdPrecioReferencia = io.IdPrecioReferencia!.Value;
                    _ctx.Ofertas.Add(oferta);
                    break;
                case OpCerrarOferta co:
                    ofertas[co.IdOferta].FechaHasta = co.FechaHasta;
                    ofertas[co.IdOferta].FecMod = ahora;
                    break;
                case OpAnularOferta ao:
                    ofertas[ao.IdOferta].Activo = false;
                    ofertas[ao.IdOferta].FecMod = ahora;
                    break;
                case OpAnularPrecio ap:
                    precios[ap.IdPrecio].Activo = false;
                    break;
                default:
                    throw new InvalidOperationException($"Operación de precio desconocida: {op.GetType().Name}");
            }
        }
        await _ctx.SaveChangesAsync(ct);
        return nuevos.ToDictionary(kv => kv.Key, kv => kv.Value.IdPrecio);
    }

    private async Task<IReadOnlyDictionary<long, SnapshotPrecios>> CargarSnapshotsAsync(IReadOnlyCollection<long> idsPresProd, CancellationToken ct)
    {
        var ids = idsPresProd.Distinct().ToList();
        if (ids.Count == 0) return new Dictionary<long, SnapshotPrecios>();
        var cab = await _ctx.Presentaciones.AsNoTracking().Where(pp => ids.Contains(pp.IdPresProd))
            .Select(pp => new { pp.IdPresProd, pp.VarProd.Prod.IdTenant, pp.VarProd.Codigo, Nombre = pp.VarProd.Prod.Nombre + " / " + pp.VarProd.Nombre })
            .ToListAsync(ct);
        var precios = await _ctx.Precios.AsNoTracking().Where(p => ids.Contains(p.IdPresProd)).ToListAsync(ct);
        var ofertas = await _ctx.Ofertas.AsNoTracking().Where(o => ids.Contains(o.IdPresProd) && o.Activo).ToListAsync(ct);
        var pPorPres = precios.ToLookup(p => p.IdPresProd);
        var oPorPres = ofertas.ToLookup(o => o.IdPresProd);
        return cab.ToDictionary(c => c.IdPresProd, c => new SnapshotPrecios(
            c.IdPresProd, c.IdTenant, c.Codigo, c.Nombre, pPorPres[c.IdPresProd].ToList(), oPorPres[c.IdPresProd].ToList()));
    }

    // Proyección única de la grilla. Paso 1: presentación + ids de precio vigente/programado;
    // paso 2: valores del precio vigente; paso 3: oferta efectiva (R2 + R2' + desempate, igual
    // que la tienda) y oferta programada; paso 4: valores de esas ofertas.
    private IQueryable<PrecioGrillaRow> Filas(int idTenant, PreciosGrillaFiltro filtro, DateOnly hoy, List<long>? ids)
    {
        var pres = _ctx.Presentaciones.AsNoTracking().Where(pp => pp.VarProd.Prod.IdTenant == idTenant);
        if (ids is not null) pres = pres.Where(pp => ids.Contains(pp.IdPresProd));
        if (!filtro.IncluirInactivos) pres = pres.Where(pp => pp.Activo && pp.EsVenta && pp.VarProd.Activo);
        if (filtro.IdCategoria is long cat)
            pres = pres.Where(pp => pp.VarProd.Prod.IdCategoria == cat || pp.VarProd.Prod.Categoria.IdPadre == cat);
        if (filtro.IdMarca is int marca) pres = pres.Where(pp => pp.VarProd.Prod.IdMarca == marca);
        var porTexto = IdsPresPorTexto(idTenant, filtro.Texto);
        if (porTexto is not null) pres = pres.Where(pp => porTexto.Contains(pp.IdPresProd));

        var paso1 = pres.Select(pp => new
        {
            pp.IdPresProd,
            pp.VarProd.IdProd,
            pp.IdVarProd,
            CodigoProducto = pp.VarProd.Prod.Codigo,
            CodigoVariante = pp.VarProd.Codigo,
            Producto = pp.VarProd.Prod.Nombre,
            Variante = pp.VarProd.Nombre,
            Categoria = pp.VarProd.Prod.Categoria.Nombre,
            Marca = pp.VarProd.Prod.Marca != null ? pp.VarProd.Prod.Marca.Nombre : null,
            CodigoBarras = pp.CodigosBarra.OrderByDescending(c => c.Principal).ThenBy(c => c.IdCodBar).Select(c => c.Codigo).FirstOrDefault(),
            Vendible = pp.Activo && pp.EsVenta && pp.VarProd.Activo,
            Publicable = pp.Activo && pp.EsVenta && pp.VarProd.Activo && pp.VarProd.Prod.Activo && pp.VarProd.Prod.VisibleStore,
            pp.VarProd.CostoProm,
            IdPrecio = pp.Precios.Where(p => p.Activo && p.FecVig <= hoy)
                .OrderByDescending(p => p.FecVig).ThenByDescending(p => p.IdPrecio).Select(p => (long?)p.IdPrecio).FirstOrDefault(),
            IdPrecioProgramado = pp.Precios.Where(p => p.Activo && p.FecVig > hoy)
                .OrderBy(p => p.FecVig).ThenBy(p => p.IdPrecio).Select(p => (long?)p.IdPrecio).FirstOrDefault(),
            OfertasActivasHoy = pp.Ofertas.Count(o => o.Activo && o.FechaDesde <= hoy && (o.FechaHasta == null || o.FechaHasta >= hoy)),
            IdOfertaProgramada = pp.Ofertas.Where(o => o.Activo && o.FechaDesde > hoy)
                .OrderBy(o => o.FechaDesde).ThenBy(o => o.IdOferta).Select(o => (long?)o.IdOferta).FirstOrDefault(),
        });

        var paso2 =
            from a in paso1
            join pv in _ctx.Precios on a.IdPrecio equals (long?)pv.IdPrecio into pvj
            from pv in pvj.DefaultIfEmpty()
            join pg in _ctx.Precios on a.IdPrecioProgramado equals (long?)pg.IdPrecio into pgj
            from pg in pgj.DefaultIfEmpty()
            select new
            {
                a,
                PrecioValor = pv != null ? (decimal?)pv.Valor : null,
                PrecioDesde = pv != null ? (DateOnly?)pv.FecVig : null,
                ProgValor = pg != null ? (decimal?)pg.Valor : null,
                ProgDesde = pg != null ? (DateOnly?)pg.FecVig : null,
            };

        var paso3 = paso2.Select(b => new
        {
            b.a, b.PrecioValor, b.PrecioDesde, b.ProgValor, b.ProgDesde,
            IdOferta = _ctx.Ofertas
                .Where(o => o.IdPresProd == b.a.IdPresProd && o.Activo && o.Publicado
                    && o.FechaDesde <= hoy && (o.FechaHasta == null || o.FechaHasta >= hoy)
                    && b.a.IdPrecio != null && o.IdPrecioReferencia == b.a.IdPrecio
                    && o.PrecioOferta < b.PrecioValor)
                .OrderBy(o => o.PrecioOferta).ThenByDescending(o => o.IdOferta)
                .Select(o => (long?)o.IdOferta).FirstOrDefault(),
        });

        return
            from c in paso3
            join of in _ctx.Ofertas on c.IdOferta equals (long?)of.IdOferta into ofj
            from of in ofj.DefaultIfEmpty()
            join op in _ctx.Ofertas on c.a.IdOfertaProgramada equals (long?)op.IdOferta into opj
            from op in opj.DefaultIfEmpty()
            select new PrecioGrillaRow
            {
                IdPresProd = c.a.IdPresProd,
                IdProd = c.a.IdProd,
                IdVarProd = c.a.IdVarProd,
                CodigoProducto = c.a.CodigoProducto,
                CodigoVariante = c.a.CodigoVariante,
                Producto = c.a.Producto,
                Variante = c.a.Variante,
                Categoria = c.a.Categoria,
                Marca = c.a.Marca,
                CodigoBarras = c.a.CodigoBarras,
                Vendible = c.a.Vendible,
                Publicable = c.a.Publicable,
                CostoProm = c.a.CostoProm,
                IdPrecio = c.a.IdPrecio,
                PrecioValor = c.PrecioValor,
                PrecioDesde = c.PrecioDesde,
                IdPrecioProgramado = c.a.IdPrecioProgramado,
                PrecioProgramadoValor = c.ProgValor,
                PrecioProgramadoDesde = c.ProgDesde,
                IdOferta = c.IdOferta,
                OfertaPrecio = of != null ? (decimal?)of.PrecioOferta : null,
                OfertaDesde = of != null ? (DateOnly?)of.FechaDesde : null,
                OfertaHasta = of != null ? of.FechaHasta : null,
                OfertasActivasHoy = c.a.OfertasActivasHoy,
                IdOfertaProgramada = c.a.IdOfertaProgramada,
                OfertaProgramadaPrecio = op != null ? (decimal?)op.PrecioOferta : null,
                OfertaProgramadaDesde = op != null ? (DateOnly?)op.FechaDesde : null,
                OfertaProgramadaHasta = op != null ? op.FechaHasta : null,
            };
    }

    private static IQueryable<PrecioGrillaRow> Filtrar(IQueryable<PrecioGrillaRow> q, FiltroEstadoPrecio estado, DateOnly hoy)
    {
        var hasta7 = hoy.AddDays(7);
        return estado switch
        {
            FiltroEstadoPrecio.SinPrecio => q.Where(r => r.IdPrecio == null),
            FiltroEstadoPrecio.ConOferta => q.Where(r => r.IdOferta != null),
            FiltroEstadoPrecio.SinOferta => q.Where(r => r.IdOferta == null),
            FiltroEstadoPrecio.OfertaSinEfecto => q.Where(r => r.OfertasActivasHoy > (r.IdOferta != null ? 1 : 0)),
            FiltroEstadoPrecio.VenceEn7Dias => q.Where(r => r.IdOferta != null && r.OfertaHasta != null && r.OfertaHasta <= hasta7),
            FiltroEstadoPrecio.Programado => q.Where(r => r.IdPrecioProgramado != null || r.IdOfertaProgramada != null),
            _ => q,
        };
    }

    // Búsqueda por presentación con UNION de ids (S91.3: OR + EXISTS correlacionado era lento).
    private IQueryable<long>? IdsPresPorTexto(int idTenant, string? texto)
    {
        var t = texto?.Trim();
        if (string.IsNullOrEmpty(t)) return null;
        if (t.Length > 100) t = t[..100];
        var porProducto = _ctx.Presentaciones
            .Where(pp => pp.VarProd.Prod.IdTenant == idTenant && (pp.VarProd.Prod.Codigo.Contains(t) || pp.VarProd.Prod.Nombre.Contains(t)))
            .Select(pp => pp.IdPresProd);
        var porVariante = _ctx.Presentaciones
            .Where(pp => pp.VarProd.Prod.IdTenant == idTenant && (pp.VarProd.Codigo.Contains(t) || pp.VarProd.Nombre.Contains(t)))
            .Select(pp => pp.IdPresProd);
        var porCodigo = _ctx.CodigosBarra
            .Where(cb => cb.PresProd.VarProd.Prod.IdTenant == idTenant && cb.Codigo.Contains(t))
            .Select(cb => cb.IdPresProd);
        return porProducto.Union(porVariante).Union(porCodigo);
    }

    private static bool EsDeadlock(Exception ex)
    {
        for (var e = ex; e is not null; e = e.InnerException)
            if (e is Microsoft.Data.SqlClient.SqlException sql && sql.Number == 1205) return true;
        return false;
    }
}
