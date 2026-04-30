namespace TaskManager.Application.Common;

public class PagedResponse<T>
{
    public IEnumerable<T> Data { get; set; } = Enumerable.Empty<T>();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    public bool HasNext => Page < TotalPages;
    public bool HasPrev => Page > 1;

    public static PagedResponse<T> Create(IEnumerable<T> data, int page, int pageSize, int totalCount) =>
        new() { Data = data, Page = page, PageSize = pageSize, TotalCount = totalCount };
}
