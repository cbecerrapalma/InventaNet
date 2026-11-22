using Inventa.Web.Models.Storefront;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Storefront;

// S57.2 P1: Backoffice storefront administration client. All tenant scoping server-side via JWT.
public sealed class StorefrontApiService
{
    private readonly ApiClient _api;
    public StorefrontApiService(ApiClient api) => _api = api;

    public Task<StorefrontGetResponse?> GetAsync(CancellationToken ct) => _api.GetAsync<StorefrontGetResponse>("api/storefront", ct);
    public Task<StorefrontProfileDto?> SaveProfileAsync(StorefrontProfileDto dto, CancellationToken ct) => _api.PutAsync<StorefrontProfileDto, StorefrontProfileDto>("api/storefront", dto, ct);
    public Task<SocialLinkDto?> AddLinkAsync(UpsertSocialDto dto, CancellationToken ct) => _api.PostAsync<UpsertSocialDto, SocialLinkDto>("api/storefront/social", dto, ct);
    public Task RemoveLinkAsync(int linkId, CancellationToken ct) => _api.DeleteAsync($"api/storefront/social/{linkId}", ct);
    public Task<List<BusinessHourDto>?> SaveHoursAsync(List<BusinessHourDto> dtos, CancellationToken ct) => _api.PutAsync<List<BusinessHourDto>, List<BusinessHourDto>>("api/storefront/hours", dtos, ct);
}
