using Inventa.Web.Models.Dashboard;
using Inventa.Web.Services.Http;

namespace Inventa.Web.Services.Dashboard;

public sealed class DashboardApiService
{
    private readonly ApiClient _api;
    public DashboardApiService(ApiClient api) => _api = api;

    public Task<DashboardSummaryDto?> GetSummaryAsync(CancellationToken ct)
        => _api.GetAsync<DashboardSummaryDto>("api/dashboard/summary", ct);
}
