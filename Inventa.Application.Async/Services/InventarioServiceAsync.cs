using CBP.Results;
using CBP.Results.Errors;
using Inventa.Application.Async.Models;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IInventarioServiceAsync
{
    Task<Result> TransferirAsync(TransferirEntreBodegasInput req, CancellationToken ct = default);
    Task<Result> AjustarAsync(AjustarInventarioInput req, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Bodega>>> GetBodegasAsync(int idTenant, CancellationToken ct = default);
    Task<Result<PagedResult<Bodega>>> GetBodegasPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Bodega>> CreateBodegaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Stock>>> GetStockAsync(int idTenant, int? idBodega, CancellationToken ct = default);
    Task<Result<PagedResult<StockListadoRow>>> GetStockPagedAsync(int idTenant, int? idBodega, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<MovInv>>> GetMovimientosAsync(int idTenant, CancellationToken ct = default);
    Task<Result<PagedResult<MovInvListadoRow>>> GetMovimientosPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<Bodega?>> GetBodegaByIdAsync(int idBodega, int idTenant, CancellationToken ct = default);
    Task<Result<Bodega>> UpdateBodegaAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default);
    Task<Result<IReadOnlyList<StockDisp>>> GetStockDispAsync(int idTenant, int? idBodega, CancellationToken ct = default);
    Task<Result<PagedResult<StockDispListadoRow>>> GetStockDispPagedAsync(int idTenant, int? idBodega, string? q, int page, int pageSize, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Lote>>> GetLotesAsync(int idTenant, CancellationToken ct = default);
    Task<Result<PagedResult<Lote>>> GetLotesPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default);
}

/// <summary>
/// S57.34: servicio de inventario 100% sobre repositorios async. Sin DbContext/EF.
/// </summary>
public sealed class InventarioServiceAsync : IInventarioServiceAsync
{
    private readonly IBodegaRepositoryAsync _bodegas;
    private readonly IPresProdRepositoryAsync _presProds;
    private readonly ILoteRepositoryAsync _lotes;
    private readonly IStockRepositoryAsync _stockRepo;
    private readonly IMovInvRepositoryAsync _movs;
    private readonly IStockDispRepositoryAsync _stockDisp;

    public InventarioServiceAsync(
        IBodegaRepositoryAsync bodegas,
        IPresProdRepositoryAsync presProds,
        ILoteRepositoryAsync lotes,
        IStockRepositoryAsync stockRepo,
        IMovInvRepositoryAsync movs,
        IStockDispRepositoryAsync stockDisp)
    {
        _bodegas = bodegas;
        _presProds = presProds;
        _lotes = lotes;
        _stockRepo = stockRepo;
        _movs = movs;
        _stockDisp = stockDisp;
    }

    public async Task<Result> AjustarAsync(AjustarInventarioInput req, CancellationToken ct = default)
    {
        if (req.Items == null || req.Items.Count == 0)
            return Result.Failure(new Error("AJUSTE_SIN_ITEMS", "El ajuste debe contener al menos un ítem.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.IdBodega <= 0)
            return Result.Failure(new Error("VALIDATION", "IdBodega debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
        var bodega = await _bodegas.GetByIdAsync(req.IdBodega, ct);
        if (bodega == null)
            return Result.Failure(new Error("BODEGA_NOT_FOUND", "Bodega no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (!bodega.Activo)
            return Result.Failure(new Error("BODEGA_INACTIVA", "Bodega no está activa.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (bodega.IdTenant != req.IdTenant)
            return Result.Failure(new Error("BODEGA_TENANT_MISMATCH", "Bodega no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
        // duplicate PresProd+IdLote
        if (req.Items.GroupBy(i => (i.IdPresProd, i.IdLote)).Any(g => g.Count() > 1))
            return Result.Failure(new Error("DUPLICATE_ITEM", "No se permiten líneas duplicadas (mismo PresProd+Lote).").WithType(ErrorType.Validation).WithHttpStatus(400));
        foreach (var it in req.Items)
        {
            if (it.IdPresProd <= 0)
                return Result.Failure(new Error("VALIDATION", "IdPresProd debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.CantAjuste == 0)
                return Result.Failure(new Error("AJUSTE_CANTIDAD_CERO", "La cantidad de ajuste no puede ser cero.").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.CostoUnitario < 0)
                return Result.Failure(new Error("VALIDATION", "CostoUnitario no puede ser negativo").WithType(ErrorType.Validation).WithHttpStatus(400));
            var pres = await _presProds.GetWithHierarchyAsync(it.IdPresProd, ct);
            if (pres == null)
                return Result.Failure(new Error("PRES_PROD_NOT_FOUND", $"Presentación {it.IdPresProd} no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
            if (pres.VarProd.Prod.IdTenant != req.IdTenant)
                return Result.Failure(new Error("PRES_PROD_TENANT_MISMATCH", "Presentación no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
            if (it.IdLote != null)
            {
                var lote = await _lotes.GetByIdAsync(it.IdLote.Value, ct);
                if (lote == null)
                    return Result.Failure(new Error("LOTE_NOT_FOUND", $"Lote {it.IdLote} no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
                if (lote.IdTenant != req.IdTenant)
                    return Result.Failure(new Error("LOTE_TENANT_MISMATCH", "Lote no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
                if (lote.IdVarProd != pres.IdVarProd)
                    return Result.Failure(new Error("LOTE_PRODUCT_MISMATCH", "Lote no pertenece a la variante de la presentación").WithType(ErrorType.Validation).WithHttpStatus(400));
            }
        }
        return await _stockRepo.AjustarInventarioAsync(SpInputMapper.ToCmd(req), ct);
    }

    public async Task<Result> TransferirAsync(TransferirEntreBodegasInput req, CancellationToken ct = default)
    {
        if (req.Items == null || req.Items.Count == 0)
            return Result.Failure(new Error("TRANSFERENCIA_SIN_ITEMS", "La transferencia debe contener al menos un ítem.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.IdBodegaOrig <= 0 || req.IdBodegaDest <= 0)
            return Result.Failure(new Error("VALIDATION", "IdBodega debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (req.IdBodegaOrig == req.IdBodegaDest)
            return Result.Failure(new Error("BODEGA_MISMA_ORIGEN_DEST", "La bodega de origen y destino no pueden ser la misma.").WithType(ErrorType.Validation).WithHttpStatus(400));
        var bodOrig = await _bodegas.GetByIdAsync(req.IdBodegaOrig, ct);
        if (bodOrig == null)
            return Result.Failure(new Error("BODEGA_NOT_FOUND", "Bodega origen no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (!bodOrig.Activo)
            return Result.Failure(new Error("BODEGA_ORIGEN_INACTIVA", "Bodega origen no está activa.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (bodOrig.IdTenant != req.IdTenant)
            return Result.Failure(new Error("BODEGA_TENANT_MISMATCH", "Bodega origen no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
        var bodDest = await _bodegas.GetByIdAsync(req.IdBodegaDest, ct);
        if (bodDest == null)
            return Result.Failure(new Error("BODEGA_NOT_FOUND", "Bodega destino no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (!bodDest.Activo)
            return Result.Failure(new Error("BODEGA_DEST_INACTIVA", "Bodega destino no está activa.").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (bodDest.IdTenant != req.IdTenant)
            return Result.Failure(new Error("BODEGA_TENANT_MISMATCH", "Bodega destino no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
        if (req.Items.GroupBy(i => (i.IdPresProd, i.IdLote)).Any(g=>g.Count()>1))
            return Result.Failure(new Error("DUPLICATE_ITEM", "No se permiten líneas duplicadas (mismo PresProd+Lote).").WithType(ErrorType.Validation).WithHttpStatus(400));
        foreach (var it in req.Items)
        {
            if (it.IdPresProd <= 0)
                return Result.Failure(new Error("VALIDATION", "IdPresProd debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            if (it.Cantidad <= 0)
                return Result.Failure(new Error("VALIDATION", "Cantidad debe ser >0").WithType(ErrorType.Validation).WithHttpStatus(400));
            var pres = await _presProds.GetWithHierarchyAsync(it.IdPresProd, ct);
            if (pres == null)
                return Result.Failure(new Error("PRES_PROD_NOT_FOUND", $"Presentación {it.IdPresProd} no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
            if (pres.VarProd.Prod.IdTenant != req.IdTenant)
                return Result.Failure(new Error("PRES_PROD_TENANT_MISMATCH", "Presentación no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
            if (it.IdLote != null)
            {
                var lote = await _lotes.GetByIdAsync(it.IdLote.Value, ct);
                if (lote == null)
                    return Result.Failure(new Error("LOTE_NOT_FOUND", $"Lote {it.IdLote} no encontrado").WithType(ErrorType.NotFound).WithHttpStatus(404));
                if (lote.IdTenant != req.IdTenant)
                    return Result.Failure(new Error("LOTE_TENANT_MISMATCH", "Lote no pertenece al tenant").WithType(ErrorType.Forbidden).WithHttpStatus(403));
                if (lote.IdVarProd != pres.IdVarProd)
                    return Result.Failure(new Error("LOTE_PRODUCT_MISMATCH", "Lote no pertenece a la variante de la presentación").WithType(ErrorType.Validation).WithHttpStatus(400));
            }
        }
        return await _stockRepo.TransferirEntresBodegasAsync(SpInputMapper.ToCmd(req), ct);
    }

    public async Task<Result<IReadOnlyList<Bodega>>> GetBodegasAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _bodegas.GetOrderedByTenantAsync(idTenant, ct);
        return Result<IReadOnlyList<Bodega>>.Success(list);
    }

    public async Task<Result<PagedResult<Bodega>>> GetBodegasPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = pageSize switch {20=>20,50=>50,100=>100,_=>Math.Clamp(pageSize,1,100)};
        if(pageSize is not (20 or 50 or 100)) pageSize=20;
        q=q?.Trim(); if(q!=null&&q.Length>100) q=q[..100];
        var like = string.IsNullOrWhiteSpace(q) ? null : $"%{q}%";
        var (items, total) = await _bodegas.SearchPagedAsync(idTenant, like, page, pageSize, ct);
        return Result<PagedResult<Bodega>>.Success(PagedResult<Bodega>.Create(items,page,pageSize,total));
    }

    public async Task<Result<Bodega>> CreateBodegaAsync(int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var cod = (codigo ?? "").Trim();
        var nom = (nombre ?? "").Trim();
        if (string.IsNullOrWhiteSpace(cod))
            return Result<Bodega>.Failure(new Error("VALIDATION", "Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (cod.Length > 20)
            return Result<Bodega>.Failure(new Error("VALIDATION", "Codigo excede longitud máxima 20").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (string.IsNullOrWhiteSpace(nom))
            return Result<Bodega>.Failure(new Error("VALIDATION", "Nombre es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (nom.Length > 100)
            return Result<Bodega>.Failure(new Error("VALIDATION", "Nombre excede longitud máxima 100").WithType(ErrorType.Validation).WithHttpStatus(400));
        var (bodega, dup, max) = await _bodegas.CreateWithQuotaAsync(idTenant, cod, nom, ct);
        if (dup) return Result<Bodega>.Failure(new Error("BODEGA_CODIGO_DUPLICADO", "Ya existe una bodega con ese código en este tenant.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        if (max.HasValue) return Result<Bodega>.Failure(new Error("MAX_BODEGAS_EXCEDIDO", $"Se alcanzó el límite de {max} bodegas para este tenant.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        return Result<Bodega>.Success(bodega!);
    }

    public async Task<Result<IReadOnlyList<Stock>>> GetStockAsync(int idTenant, int? idBodega, CancellationToken ct = default)
    {
        var list = await _stockRepo.GetByTenantAsync(idTenant, idBodega, 200, ct);
        return Result<IReadOnlyList<Stock>>.Success(list);
    }

    public async Task<Result<PagedResult<StockListadoRow>>> GetStockPagedAsync(int idTenant, int? idBodega, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page=page<1?1:page; pageSize=pageSize switch{20=>20,50=>50,100=>100,_=>Math.Clamp(pageSize,1,100)}; if(pageSize is not (20 or 50 or 100)) pageSize=20; q=q?.Trim(); if(q!=null&&q.Length>100) q=q[..100];
        // S114 D2: q es texto de producto (código, nombre, variante o código de barras); ya no es un id de bodega.
        var (items, total) = await _stockRepo.SearchPagedAsync(idTenant, idBodega, q, page, pageSize, ct);
        return Result<PagedResult<StockListadoRow>>.Success(PagedResult<StockListadoRow>.Create(items,page,pageSize,total));
    }

    public async Task<Result<IReadOnlyList<MovInv>>> GetMovimientosAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _movs.GetRecentAsync(idTenant, 200, ct);
        return Result<IReadOnlyList<MovInv>>.Success(list);
    }

    public async Task<Result<PagedResult<MovInvListadoRow>>> GetMovimientosPagedAsync(int idTenant, int? idBodega, byte? tipoMov, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page=page<1?1:page; pageSize=pageSize switch{20=>20,50=>50,100=>100,_=>Math.Clamp(pageSize,1,100)}; if(pageSize is not (20 or 50 or 100)) pageSize=20; q=q?.Trim(); if(q!=null&&q.Length>100) q=q[..100];
        // S114 D2/D3: q es texto de producto; tipoMov filtra en la consulta.
        var (items, total) = await _movs.SearchPagedAsync(idTenant, idBodega, tipoMov, q, page, pageSize, ct);
        return Result<PagedResult<MovInvListadoRow>>.Success(PagedResult<MovInvListadoRow>.Create(items,page,pageSize,total));
    }

    public async Task<Result<Bodega?>> GetBodegaByIdAsync(int idBodega, int idTenant, CancellationToken ct = default)
    {
        var b = await _bodegas.GetByIdInTenantAsync(idBodega, idTenant, ct);
        if (b == null) return Result<Bodega?>.Failure(new Error("BODEGA_NOT_FOUND", "Bodega no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Bodega?>.Success(b);
    }

    public async Task<Result<Bodega>> UpdateBodegaAsync(int idBodega, int idTenant, string codigo, string nombre, CancellationToken ct = default)
    {
        var cod = (codigo ?? "").Trim();
        var nom = (nombre ?? "").Trim();
        if (string.IsNullOrWhiteSpace(cod))
            return Result<Bodega>.Failure(new Error("VALIDATION", "Codigo es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (cod.Length > 20)
            return Result<Bodega>.Failure(new Error("VALIDATION", "Codigo excede longitud máxima 20").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (string.IsNullOrWhiteSpace(nom))
            return Result<Bodega>.Failure(new Error("VALIDATION", "Nombre es requerido").WithType(ErrorType.Validation).WithHttpStatus(400));
        if (nom.Length > 100)
            return Result<Bodega>.Failure(new Error("VALIDATION", "Nombre excede longitud máxima 100").WithType(ErrorType.Validation).WithHttpStatus(400));
        var current = await _bodegas.GetByIdInTenantAsync(idBodega, idTenant, ct);
        if (current == null) return Result<Bodega>.Failure(new Error("NOT_FOUND", "Bodega no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        if (!string.Equals(current.Codigo, cod, StringComparison.OrdinalIgnoreCase)
            && await _bodegas.ExistsByCodigoExcludingAsync(idTenant, cod, idBodega, ct))
            return Result<Bodega>.Failure(new Error("BODEGA_CODIGO_DUPLICADO", "Ya existe una bodega con ese código en este tenant.").WithType(ErrorType.Conflict).WithHttpStatus(409));
        var (updated, dup) = await _bodegas.UpdateAsync(idBodega, idTenant, cod, nom, ct);
        if (dup || updated is null) return Result<Bodega>.Failure(dup
            ? new Error("BODEGA_CODIGO_DUPLICADO", "Ya existe una bodega con ese código en este tenant.").WithType(ErrorType.Conflict).WithHttpStatus(409)
            : new Error("NOT_FOUND", "Bodega no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Bodega>.Success(updated);
    }

    public async Task<Result<IReadOnlyList<StockDisp>>> GetStockDispAsync(int idTenant, int? idBodega, CancellationToken ct = default)
    {
        var list = await _stockDisp.GetByTenantAsync(idTenant, idBodega, 200, ct);
        return Result<IReadOnlyList<StockDisp>>.Success(list);
    }

    public async Task<Result<PagedResult<StockDispListadoRow>>> GetStockDispPagedAsync(int idTenant, int? idBodega, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page=page<1?1:page; pageSize=pageSize switch{20=>20,50=>50,100=>100,_=>Math.Clamp(pageSize,1,100)}; if(pageSize is not (20 or 50 or 100)) pageSize=20; q=q?.Trim(); if(q!=null&&q.Length>100) q=q[..100];
        // S114 D2: q es texto de producto (antes se ignoraba).
        var (items, total) = await _stockDisp.SearchPagedAsync(idTenant, idBodega, q, page, pageSize, ct);
        return Result<PagedResult<StockDispListadoRow>>.Success(PagedResult<StockDispListadoRow>.Create(items,page,pageSize,total));
    }

    public async Task<Result<IReadOnlyList<Lote>>> GetLotesAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _lotes.GetRecentAsync(idTenant, 200, ct);
        return Result<IReadOnlyList<Lote>>.Success(list);
    }

    public async Task<Result<PagedResult<Lote>>> GetLotesPagedAsync(int idTenant, string? q, int page, int pageSize, CancellationToken ct = default)
    {
        page=page<1?1:page; pageSize=pageSize switch{20=>20,50=>50,100=>100,_=>Math.Clamp(pageSize,1,100)}; if(pageSize is not (20 or 50 or 100)) pageSize=20; q=q?.Trim(); if(q!=null&&q.Length>100) q=q[..100];
        var like = string.IsNullOrWhiteSpace(q) ? null : $"%{q}%";
        var (items, total) = await _lotes.SearchPagedAsync(idTenant, like, page, pageSize, ct);
        return Result<PagedResult<Lote>>.Success(PagedResult<Lote>.Create(items,page,pageSize,total));
    }
}
