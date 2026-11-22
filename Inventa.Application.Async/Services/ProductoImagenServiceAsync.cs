using CBP.Results;
using CBP.Results.Errors;
using Inventa.Data.Async.Repositories;
using Inventa.Domain.Entities;

namespace Inventa.Application.Async.Services;

public interface IProductoImagenServiceAsync
{
    Task<Result<IReadOnlyList<ProductoImagen>>> GetActivesAsync(long idProd, CancellationToken ct = default);
    Task<Result<ProductoImagen?>> GetPrincipalAsync(long idProd, CancellationToken ct = default);
    Task<Result<ProductoImagen?>> GetByIdInProductAsync(int idProductoImagen, long idProd, CancellationToken ct = default);
    Task<Result> SetPrincipalAsync(long idProd, int idProductoImagen, CancellationToken ct = default);
    Task<Result<IReadOnlyList<ProductoImagen>>> ReorderAsync(long idProd, IReadOnlyList<int> orderedIds, CancellationToken ct = default);
    Task<Result<(string Url, string NombreArchivo)>> DeleteAsync(long idProd, int idProductoImagen, CancellationToken ct = default);
    Task<Result<IReadOnlyList<long>>> GetActiveProdIdsAsync(int idTenant, CancellationToken ct = default);
    Task<Result<string?>> GetPrincipalUrlAsync(long idProd, CancellationToken ct = default);
}

public sealed class ProductoImagenServiceAsync : IProductoImagenServiceAsync
{
    private readonly IProductoImagenRepositoryAsync _repo;
    public ProductoImagenServiceAsync(IProductoImagenRepositoryAsync repo) => _repo = repo;

    public async Task<Result<IReadOnlyList<ProductoImagen>>> GetActivesAsync(long idProd, CancellationToken ct = default)
    {
        var list = await _repo.GetActivesAsync(idProd, ct);
        return Result<IReadOnlyList<ProductoImagen>>.Success(list);
    }

    public async Task<Result<ProductoImagen?>> GetPrincipalAsync(long idProd, CancellationToken ct = default)
    {
        var e = await _repo.GetPrincipalAsync(idProd, ct);
        return Result<ProductoImagen?>.Success(e);
    }

    public async Task<Result<ProductoImagen?>> GetByIdInProductAsync(int idProductoImagen, long idProd, CancellationToken ct = default)
    {
        var e = await _repo.GetByIdInProductAsync(idProductoImagen, idProd, ct);
        return Result<ProductoImagen?>.Success(e);
    }

    public async Task<Result> SetPrincipalAsync(long idProd, int idProductoImagen, CancellationToken ct = default)
    {
        await _repo.SetPrincipalAsync(idProd, idProductoImagen, ct);
        return Result.Success();
    }

    public async Task<Result<IReadOnlyList<ProductoImagen>>> ReorderAsync(long idProd, IReadOnlyList<int> orderedIds, CancellationToken ct = default)
    {
        var list = await _repo.ReorderAsync(idProd, orderedIds, ct);
        return Result<IReadOnlyList<ProductoImagen>>.Success(list);
    }

    public async Task<Result<(string Url, string NombreArchivo)>> DeleteAsync(long idProd, int idProductoImagen, CancellationToken ct = default)
    {
        // S104: solo la fila. El borrado completo (fila + objeto del storage + Archivo) es
        // IProductoImagenStorageServiceAsync.DeleteAsync, que es el que usa la API.
        var result = await _repo.DeleteAsync(idProd, idProductoImagen, ct);
        return Result<(string Url, string NombreArchivo)>.Success(result);
    }

    public async Task<Result<IReadOnlyList<long>>> GetActiveProdIdsAsync(int idTenant, CancellationToken ct = default)
    {
        var list = await _repo.GetActiveProdIdsAsync(idTenant, ct);
        return Result<IReadOnlyList<long>>.Success(list);
    }

    public async Task<Result<string?>> GetPrincipalUrlAsync(long idProd, CancellationToken ct = default)
    {
        var url = await _repo.GetPrincipalUrlAsync(idProd, ct);
        return Result<string?>.Success(url);
    }
}
