namespace Inventa.Application.Async.Models;

/// <summary>
/// S55.0 server paging envelope – mirrors CBP IPagedResult shape, used by Application.Async services.
/// Reuses CBP pagination semantics without duplicating framework.
/// </summary>
public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages)
{
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;
    public static PagedResult<T> Create(IReadOnlyList<T> items, int page, int pageSize, int totalCount)
    {
        var totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling(totalCount / (double)pageSize);
        return new(items, page, pageSize, totalCount, totalPages);
    }
}
