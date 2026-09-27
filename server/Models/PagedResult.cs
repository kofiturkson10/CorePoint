namespace CompanyPortal.Api.Models;

// Generic wrapper returned by list endpoints that support search/page/pageSize query
// parameters (e.g. GET /api/employees, GET /api/documents), so the client can build
// pagination controls without a separate "how many are there" request.
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    public static PagedResult<T> Create(List<T> items, int totalCount, int page, int pageSize)
    {
        return new PagedResult<T>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = pageSize > 0 ? (int)Math.Ceiling(totalCount / (double)pageSize) : 0
        };
    }
}

// Shared page/pageSize validation for paginated list endpoints - invalid values (missing,
// zero, negative, or too large) silently fall back to sane defaults instead of erroring,
// so a bad query string can't 400 or blow up Skip/Take.
public static class PagingDefaults
{
    public const int DefaultPage = 1;
    public const int DefaultPageSize = 10;
    public const int MaxPageSize = 100;

    public static (int Page, int PageSize) Normalize(int page, int pageSize)
    {
        var normalizedPage = page > 0 ? page : DefaultPage;
        var normalizedPageSize = pageSize > 0 ? Math.Min(pageSize, MaxPageSize) : DefaultPageSize;
        return (normalizedPage, normalizedPageSize);
    }
}
