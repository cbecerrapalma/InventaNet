using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IOfertaServiceAsync
{
    Task<Result<Oferta?>> GetByIdAsync(int idTenant, long idOferta, CancellationToken ct = default);
    Task<Result<IReadOnlyList<Oferta>>> GetByPresProdAsync(int idTenant, long idPresProd, CancellationToken ct = default);
    Task<Result<Oferta?>> GetVigenteAsync(int idTenant, long idPresProd, DateOnly? fecha, CancellationToken ct = default);
    // S57.64 R8: aprobación editorial (Publicado=true, idempotente). 404 si inexistente/otro tenant.
    Task<Result<Oferta>> AprobarAsync(int idTenant, long idOferta, CancellationToken ct = default);
}

// S57.52: oferta con referencia obligatoria (P11-P16). Servicio async-only,
// S93.1: la creación se movió a GestionPreciosServiceAsync (único escritor de Precio/Oferta).
// sin contraparte sync (tabla nueva, como ProductoImagen/Storefront).
public sealed class OfertaServiceAsync : IOfertaServiceAsync
{
    private readonly IOfertaRepositoryAsync _repo;
    private readonly IPrecioRepositoryAsync _precios;
    private readonly IPresProdRepositoryAsync _presProd;

    public OfertaServiceAsync(
        IOfertaRepositoryAsync repo,
        IPrecioRepositoryAsync precios,
        IPresProdRepositoryAsync presProd)
    {
        _repo = repo;
        _precios = precios;
        _presProd = presProd;
    }

    // S57.64 R8: aprobación editorial = Publicado=true. Sin cambio de esquema,
    // idempotente (re-aprobar => 200 mismo estado), tenant-scoped.
    public async Task<Result<Oferta>> AprobarAsync(int idTenant, long idOferta, CancellationToken ct = default)
    {
        var existing = await GetByIdAsync(idTenant, idOferta, ct);
        if (!existing.IsSuccess) return Result<Oferta>.Failure(existing.Error!);
        if (existing.Value == null)
            return Result<Oferta>.Failure(new Error("OFERTA_NOT_FOUND", "Oferta no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        var updated = await _repo.SetPublicadoAsync(idOferta, true, ct);
        if (updated == null)
            return Result<Oferta>.Failure(new Error("OFERTA_NOT_FOUND", "Oferta no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<Oferta>.Success(updated);
    }

    public async Task<Result<Oferta?>> GetByIdAsync(int idTenant, long idOferta, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdAsync(idOferta, ct);
        if (e == null) return Result<Oferta?>.Success(null, allowNull: true);
        var tenantPres = await _presProd.GetTenantIdAsync(e.IdPresProd, ct);
        if (tenantPres == null || tenantPres.Value != idTenant) return Result<Oferta?>.Success(null, allowNull: true);
        return Result<Oferta?>.Success(e);
    }

    public async Task<Result<IReadOnlyList<Oferta>>> GetByPresProdAsync(int idTenant, long idPresProd, CancellationToken ct = default)
    {
        var tenantPres = await _presProd.GetTenantIdAsync(idPresProd, ct);
        if (tenantPres == null || tenantPres.Value != idTenant)
            return Result<IReadOnlyList<Oferta>>.Failure(new Error("PRES_PROD_NOT_FOUND", "Presentación no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        return Result<IReadOnlyList<Oferta>>.Success(await _repo.GetByPresProdAsync(idPresProd, ct));
    }

    public async Task<Result<Oferta?>> GetVigenteAsync(int idTenant, long idPresProd, DateOnly? fecha, CancellationToken ct = default)
    {
        var tenantPres = await _presProd.GetTenantIdAsync(idPresProd, ct);
        if (tenantPres == null || tenantPres.Value != idTenant)
            return Result<Oferta?>.Failure(new Error("PRES_PROD_NOT_FOUND", "Presentación no encontrada").WithType(ErrorType.NotFound).WithHttpStatus(404));
        var e = await _repo.GetVigenteAsync(idPresProd, fecha ?? DateOnly.FromDateTime(DateTime.UtcNow), ct);
        return Result<Oferta?>.Success(e, allowNull: true);
    }
}
