using CBP.Results;
using CBP.Storage;

namespace Inventa.Application.Tests;

/// <summary>S104: proveedor de storage en memoria con acceso directo, para probar los servicios sin disco ni red.</summary>
internal class StubStorageProvider : IStorageProvider, IStorageDirectAccess
{
    public Dictionary<StorageObjectRef, byte[]> Objetos { get; } = [];
    public List<StorageObjectRef> Subidos { get; } = [];
    public List<StorageObjectRef> Eliminados { get; } = [];
    public List<StorageUploadRequest> Firmados { get; } = [];

    /// <summary>Contenido que entrega la descarga de un objeto que no está en <see cref="Objetos"/> (null = no existe).</summary>
    public byte[]? ContenidoPorDefecto { get; set; }

    public string Name => "Stub";
    public StorageCapabilities Capabilities => StorageCapabilities.DirectAccess;

    public Task<Result<StorageObjectInfo>> UploadAsync(StorageObjectRef obj, Stream content, string contentType, CancellationToken ct = default)
    {
        using var ms = new MemoryStream();
        content.CopyTo(ms);
        Objetos[obj] = ms.ToArray();
        Subidos.Add(obj);
        return Task.FromResult(Result<StorageObjectInfo>.Success(Info(obj)));
    }

    public Task<Result<Stream>> DownloadAsync(StorageObjectRef obj, CancellationToken ct = default)
    {
        var bytes = Objetos.TryGetValue(obj, out var propios) ? propios : ContenidoPorDefecto;
        return Task.FromResult(bytes is null
            ? Result<Stream>.Failure(StorageErrors.NotFound(obj))
            : Result<Stream>.Success(new MemoryStream(bytes, writable: false)));
    }

    /// <summary>Objetos cuyo borrado falla (simula un proveedor caído).</summary>
    public HashSet<StorageObjectRef> FallaBorrar { get; } = [];

    /// <summary>S125: objetos cuyo borrado lanza (simula una excepción del proveedor o una cancelación).</summary>
    public HashSet<StorageObjectRef> LanzaBorrar { get; } = [];

    public Task<Result> DeleteAsync(StorageObjectRef obj, CancellationToken ct = default)
    {
        if (LanzaBorrar.Contains(obj))
            throw new System.IO.IOException("borrado simulado: excepción del proveedor");
        if (FallaBorrar.Contains(obj))
            return Task.FromResult(Result.Failure(StorageErrors.IoError("borrado simulado fallido")));
        Eliminados.Add(obj);
        Objetos.Remove(obj);
        return Task.FromResult(Result.Success());
    }

    public Task<Result<StorageObjectInfo>> CopyAsync(StorageObjectRef source, StorageObjectRef destination, CancellationToken ct = default)
    {
        if (!Objetos.TryGetValue(source, out var bytes))
            return Task.FromResult(Result<StorageObjectInfo>.Failure(StorageErrors.NotFound(source)));
        Objetos[destination] = bytes;
        return Task.FromResult(Result<StorageObjectInfo>.Success(Info(destination)));
    }

    public async Task<Result<StorageObjectInfo>> MoveAsync(StorageObjectRef source, StorageObjectRef destination, CancellationToken ct = default)
    {
        var copiado = await CopyAsync(source, destination, ct);
        if (copiado.IsSuccess && source != destination) Objetos.Remove(source);
        return copiado;
    }

    public Task<Result<StorageObjectInfo?>> GetAsync(StorageObjectRef obj, CancellationToken ct = default) =>
        Task.FromResult(Result<StorageObjectInfo?>.Success(Objetos.ContainsKey(obj) ? Info(obj) : null, allowNull: true));

    public Task<Result<bool>> ExistsAsync(StorageObjectRef obj, CancellationToken ct = default) =>
        Task.FromResult(Result<bool>.Success(Objetos.ContainsKey(obj)));

    public Task<Result<StoragePage>> ListAsync(string container, string? prefix, int pageSize = 100, string? continuation = null, CancellationToken ct = default)
    {
        var items = Objetos.Keys
            .Where(o => o.Container == container && o.Key.StartsWith(prefix ?? string.Empty, StringComparison.Ordinal))
            .Select(Info)
            .ToList();
        return Task.FromResult(Result<StoragePage>.Success(new StoragePage(items, null)));
    }

    public Task<Result<byte[]>> ReadRangeAsync(StorageObjectRef obj, long offset, int count, CancellationToken ct = default) =>
        Task.FromResult(Objetos.TryGetValue(obj, out var bytes)
            ? Result<byte[]>.Success(bytes.Skip((int)offset).Take(count).ToArray())
            : Result<byte[]>.Failure(StorageErrors.NotFound(obj)));

    public Task<Result<StorageUploadTicket>> CreateUploadUrlAsync(StorageUploadRequest request, CancellationToken ct = default)
    {
        Firmados.Add(request);
        var headers = new Dictionary<string, string> { ["Content-Type"] = request.ContentType };
        return Task.FromResult(Result<StorageUploadTicket>.Success(new StorageUploadTicket(
            $"_storage/{request.Object.Container}/{request.Object.Key}?exp=1&sig=x", "PUT", headers, DateTimeOffset.UtcNow.Add(request.Ttl))));
    }

    public Task<Result<string>> CreateDownloadUrlAsync(StorageObjectRef obj, TimeSpan ttl, CancellationToken ct = default) =>
        Task.FromResult(Result<string>.Success($"_storage/{obj.Container}/{obj.Key}?exp=1&sig=x"));

    public string? GetPublicUrl(StorageObjectRef obj) =>
        obj.Container == StorageContainers.Public ? $"uploads/{obj.Key}" : null;

    private StorageObjectInfo Info(StorageObjectRef obj) =>
        new(obj.Container, obj.Key, Objetos[obj].Length, "application/octet-stream", $"etag-{Objetos[obj].Length}", DateTimeOffset.UtcNow);
}
