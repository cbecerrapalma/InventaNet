using Inventa.Web.Services.Http;
using Inventa.Web.Services.Storage;

namespace Inventa.Web.Services.Storage;

public sealed class TokenStorageAccessTokenProvider : IAccessTokenProvider
{
    private readonly ITokenStorage _storage;
    public TokenStorageAccessTokenProvider(ITokenStorage storage) => _storage = storage;
    public Task<string?> GetAccessTokenAsync(CancellationToken ct = default) => _storage.GetAccessTokenAsync(ct);
}
