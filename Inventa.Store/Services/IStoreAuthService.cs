using System.Security.Claims;
using Inventa.Store.Models;

namespace Inventa.Store.Services;

public interface IStoreAuthService
{
    Task<StoreAuthResult<StoreTokenPair>> LoginAsync(string identificador, string password, CancellationToken ct = default);
    Task<StoreAuthResult<StoreTokenPair>> RefreshAsync(CancellationToken ct = default);
    Task LogoutAsync(CancellationToken ct = default);
    Task<ClaimsPrincipal?> GetUserAsync(CancellationToken ct = default);
}