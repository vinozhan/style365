namespace Style365.Application.Common.Models;

public class PaginatedResult<T>
{
    public IEnumerable<T> Items { get; set; } = [];
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalItems { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalItems / PageSize);
    public bool HasPreviousPage => Page > 1;
    public bool HasNextPage => Page < TotalPages;

    public PaginatedResult()
    {
    }

    public PaginatedResult(IEnumerable<T> items, int page, int pageSize, int totalItems)
    {
        Items = items;
        Page = page;
        PageSize = pageSize;
        TotalItems = totalItems;
    }

    public static PaginatedResult<T> Create(IEnumerable<T> items, int page, int pageSize, int totalItems)
    {
        return new PaginatedResult<T>(items, page, pageSize, totalItems);
    }
}