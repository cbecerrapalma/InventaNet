namespace Inventa.WebAPI.Models;

/// <summary>
/// S55.0 common paging envelope – mirrors CBP IPagedResult shape, reused across all paged endpoints.
/// No duplicate of CBP infrastructure, just HTTP DTO. Backward compatible: old callers ignore paging if not sent.
/// </summary>
public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public static PagedResponse<T> From(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new(items, page, pageSize, totalCount, totalPages);
    }
}
