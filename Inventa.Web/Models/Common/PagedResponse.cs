namespace Inventa.Web.Models.Common;

/// <summary>
/// S55.0 server paging envelope - mirrors Inventa.WebAPI.Models.PagedResponse shape.
/// Single Web representation, no duplicate. Maps items/page/pageSize/totalCount/totalPages/hasNext/hasPrevious.
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
}
