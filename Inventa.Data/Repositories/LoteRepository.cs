using CBP.Data.Abstractions;
using CBP.Data.Synchronous;
using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Context;
using Inventa.Data.Repositories.Internal;
using Inventa.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Inventa.Data.Repositories;

public interface ILoteRepository : IRepositorySync<Lote>
{
    Result<IReadOnlyList<Lote>> GetByVariante(int idTenant, long idVarProd, bool soloActivos = true);

    /// <summary>
    /// Lotes próximos a vencer en el rango de días indicado.
    /// Usa IX_Lote_Ven (IdVarProd, FecVen).
    /// </summary>
    Result<IReadOnlyList<Lote>> GetProximosAVencer(int idTenant, int diasAlerta = 30);

    Result<IReadOnlyList<Lote>> GetVencidos(int idTenant);

    /// <summary>
    /// Lotes próximos a vencer que tienen stock físico positivo en al menos una bodega.
    /// JOIN directo con Stock — IX_Lote_Ven + IX_Stock_Busq.
    /// </summary>
    Result<IReadOnlyList<Lote>> GetPorVencerConStock(int idTenant, int diasAlerta = 30);

    Result<Lote?> GetByCodigo(int idTenant, long idVarProd, string codigo);
    Result<bool> ExistsByCodigo(int idTenant, long idVarProd, string codigo);

    /// <summary>
    /// Lotes activos ordenados por FecVen ASC (FEFO) con stock > 0 en la bodega.
    /// Sugiere el lote a consumir en el POS respetando First Expired First Out.
    /// Lotes sin FecVen se ordenan al final (no caducan).
    /// </summary>
    Result<IReadOnlyList<Lote>> GetOrdenFEFO(int idTenant, long idVarProd, int idBodega);

    /// <summary>
    /// Lotes dentro del umbral de alerta personalizado definido por Prod.DiasVen.
    /// A diferencia de GetProximosAVencer (umbral global), este método usa el plazo
    /// de vida útil específico de cada producto para determinar si un lote está en alerta.
    ///
    /// ESTRATEGIA DE FILTRADO (dos pasos):
    ///   1. SQL: filtro conservador — lotes activos con FecVen entre hoy y hoy + maxDiasVen
    ///      (máximo DiasVen de todos los productos del tenant). Reduce el conjunto de filas
    ///      que llegan a memoria sin necesidad de artimética de fechas por fila en SQL.
    ///   2. Memoria: filtro exacto — aplica el DiasVen específico de cada producto.
    ///
    /// Esto evita el .ToList() prematuro anterior (que traía TODOS los lotes con FecVen
    /// a memoria independientemente del umbral).
    /// Solo incluye lotes con stock positivo en al menos una bodega del tenant.
    /// </summary>
    Result<IReadOnlyList<Lote>> GetAlertasVencimientoByDiasVen(int idTenant);
}

public class LoteRepository : RepositorySync<Lote>, ILoteRepository
{
    public LoteRepository(InventaDbContext context) : base(context) { }

    public Result<IReadOnlyList<Lote>> GetByVariante(int idTenant, long idVarProd, bool soloActivos = true)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        try
        {
            var query = DbSet.AsNoTracking()
                .Where(l => l.IdTenant == idTenant && l.IdVarProd == idVarProd);
            if (soloActivos) query = query.Where(l => l.Activo);
            // IX_Lote_Ven (IdVarProd, FecVen) — order alineado con el índice
            return Result<IReadOnlyList<Lote>>.Success(
                query.OrderBy(l => l.FecVen).ThenBy(l => l.Codigo).ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetByVariante))
                    .WithDetail("IdTenant", idTenant).WithDetail("IdVarProd", idVarProd));
        }
    }

    public Result<IReadOnlyList<Lote>> GetProximosAVencer(int idTenant, int diasAlerta = 30)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.Rango(nameof(diasAlerta), diasAlerta, 1, 365)) is { } errD) return errD;
        try
        {
            var hoy    = DateOnly.FromDateTime(DateTime.UtcNow);
            var limite = hoy.AddDays(diasAlerta);
            return Result<IReadOnlyList<Lote>>.Success(
                DbSet.AsNoTracking()
                    .Where(l => l.IdTenant == idTenant
                             && l.Activo
                             && l.FecVen.HasValue
                             && l.FecVen.Value >= hoy
                             && l.FecVen.Value <= limite)
                    .Include(l => l.VarProd)
                    .OrderBy(l => l.FecVen)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetProximosAVencer))
                    .WithDetail("IdTenant", idTenant).WithDetail("DiasAlerta", diasAlerta));
        }
    }

    public Result<IReadOnlyList<Lote>> GetVencidos(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            return Result<IReadOnlyList<Lote>>.Success(
                DbSet.AsNoTracking()
                    .Where(l => l.IdTenant == idTenant && l.Activo
                             && l.FecVen.HasValue && l.FecVen.Value < hoy)
                    .Include(l => l.VarProd)
                    .OrderBy(l => l.FecVen)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetVencidos)).WithDetail("IdTenant", idTenant));
        }
    }

    public Result<IReadOnlyList<Lote>> GetPorVencerConStock(int idTenant, int diasAlerta = 30)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.Rango(nameof(diasAlerta), diasAlerta, 1, 365)) is { } errD) return errD;
        try
        {
            var hoy    = DateOnly.FromDateTime(DateTime.UtcNow);
            var limite = hoy.AddDays(diasAlerta);
            return Result<IReadOnlyList<Lote>>.Success(
                DbSet.AsNoTracking()
                    .Where(l => l.IdTenant == idTenant
                             && l.Activo
                             && l.FecVen.HasValue
                             && l.FecVen.Value >= hoy
                             && l.FecVen.Value <= limite
                             && Context.Set<Stock>().Any(s =>
                                    s.IdTenant  == idTenant
                                 && s.IdVarProd == l.IdVarProd
                                 && s.IdLote    == l.IdLote
                                 && s.CantAct   > 0))
                    .Include(l => l.VarProd)
                    .OrderBy(l => l.FecVen)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetPorVencerConStock))
                    .WithDetail("IdTenant", idTenant).WithDetail("DiasAlerta", diasAlerta));
        }
    }

    public Result<Lote?> GetByCodigo(int idTenant, long idVarProd, string codigo)
    {
        if (Guard.Fail<Lote?>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<Lote?>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<Lote?>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            // UQ_Lote (IdTenant, IdVarProd, Codigo)
            return Result<Lote?>.Success(
                DbSet.AsNoTracking()
                    .FirstOrDefault(l => l.IdTenant  == idTenant
                                      && l.IdVarProd == idVarProd
                                      && l.Codigo    == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<Lote?>.Failure(
                CreateError(ex, nameof(GetByCodigo))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdVarProd", idVarProd)
                    .WithDetail("Codigo",    codigo));
        }
    }

    public Result<bool> ExistsByCodigo(int idTenant, long idVarProd, string codigo)
    {
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<bool>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<bool>(Guard.StringRequerido(nameof(codigo), codigo)) is { } errStr) return errStr;
        try
        {
            var codigoNorm = codigo.ToUpperInvariant();
            return Result<bool>.Success(
                DbSet.Any(l => l.IdTenant  == idTenant
                            && l.IdVarProd == idVarProd
                            && l.Codigo    == codigoNorm));
        }
        catch (Exception ex)
        {
            return Result<bool>.Failure(
                CreateError(ex, nameof(ExistsByCodigo))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdVarProd", idVarProd)
                    .WithDetail("Codigo",    codigo));
        }
    }

    public Result<IReadOnlyList<Lote>> GetOrdenFEFO(int idTenant, long idVarProd, int idBodega)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } errT) return errT;
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idVarProd), idVarProd)) is { } errV) return errV;
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idBodega), idBodega)) is { } errB) return errB;
        try
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);
            return Result<IReadOnlyList<Lote>>.Success(
                DbSet.AsNoTracking()
                    .Where(l => l.IdTenant  == idTenant
                             && l.IdVarProd == idVarProd
                             && l.Activo
                             && (!l.FecVen.HasValue || l.FecVen.Value > hoy)
                             && Context.Set<Stock>().Any(s =>
                                    s.IdTenant  == idTenant
                                 && s.IdBodega  == idBodega
                                 && s.IdVarProd == idVarProd
                                 && s.IdLote    == l.IdLote
                                 && s.CantAct   > 0))
                    .OrderBy(l => l.FecVen.HasValue ? 0 : 1)
                    .ThenBy(l => l.FecVen)
                    .ToList().AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetOrdenFEFO))
                    .WithDetail("IdTenant",  idTenant)
                    .WithDetail("IdVarProd", idVarProd)
                    .WithDetail("IdBodega",  idBodega));
        }
    }

    public Result<IReadOnlyList<Lote>> GetAlertasVencimientoByDiasVen(int idTenant)
    {
        if (Guard.Fail<IReadOnlyList<Lote>>(Guard.IdInvalido(nameof(idTenant), idTenant)) is { } err) return err;
        try
        {
            var hoy = DateOnly.FromDateTime(DateTime.UtcNow);

            // BUG FIX v3: el .ToList() prematuro traía todos los lotes del tenant
            // a memoria para filtrar por DiasVen en C#. Con volumen alto esto es crítico.
            //
            // SOLUCIÓN en dos pasos:
            //
            // PASO 1 — SQL: filtro conservador usando el DiasVen MÁXIMO de todos los
            // productos activos del tenant. Esto acota el window de fechas en SQL
            // sin necesidad de aritmética por fila, aprovechando IX_Lote_Ven (IdVarProd, FecVen).
            // El resultado puede incluir algunos lotes que el filtro exacto descartará
            // (falsos positivos), pero nunca excluirá lotes que sí deben aparecer.
            //
            // PASO 2 — Memoria: filtro exacto aplicando el DiasVen de cada producto.
            // El conjunto en memoria es pequeño porque ya fue acotado en SQL.

            // Obtener el máximo DiasVen del tenant para el filtro conservador
            var maxDiasVen = Context.Set<Prod>()
                .Where(p => p.IdTenant == idTenant && p.Activo && p.DiasVen.HasValue)
                .Select(p => (int?)p.DiasVen)
                .Max() ?? 0;

            // Si ningún producto tiene DiasVen definido, no hay alertas que calcular
            if (maxDiasVen == 0)
                return Result<IReadOnlyList<Lote>>.Success([]);

            var limiteConservador = hoy.AddDays(maxDiasVen);

            // Paso 1: traer solo los lotes dentro del window conservador
            // con stock positivo y con DiasVen definido en su producto
            var candidatos = DbSet.AsNoTracking()
                .Include(l => l.VarProd)
                    .ThenInclude(v => v.Prod)
                .Where(l => l.IdTenant == idTenant
                         && l.Activo
                         && l.FecVen.HasValue
                         && l.FecVen.Value >= hoy
                         && l.FecVen.Value <= limiteConservador
                         && l.VarProd.Prod.DiasVen.HasValue
                         && Context.Set<Stock>().Any(s =>
                                s.IdTenant  == idTenant
                             && s.IdVarProd == l.IdVarProd
                             && s.IdLote    == l.IdLote
                             && s.CantAct   > 0))
                .ToList();

            // Paso 2: filtro exacto en memoria — aplica DiasVen del producto específico
            var alertas = candidatos
                .Where(l => l.FecVen!.Value <= hoy.AddDays(l.VarProd.Prod.DiasVen!.Value))
                .OrderBy(l => l.FecVen)
                .ThenBy(l => l.VarProd.Prod.Nombre)
                .ToList();

            return Result<IReadOnlyList<Lote>>.Success(alertas.AsReadOnly());
        }
        catch (Exception ex)
        {
            return Result<IReadOnlyList<Lote>>.Failure(
                CreateError(ex, nameof(GetAlertasVencimientoByDiasVen))
                    .WithDetail("IdTenant", idTenant));
        }
    }
}
