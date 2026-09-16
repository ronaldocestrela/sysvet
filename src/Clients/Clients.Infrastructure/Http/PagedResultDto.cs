namespace Clients.Infrastructure.Http;

/// <summary>
/// Client-side mirror of the API paginated list envelope (camelCase JSON).
/// </summary>
/// <typeparam name="T">Item type in the current page.</typeparam>
public sealed class PagedResultDto<T>
{
    /// <summary>Items in the current page.</summary>
    public IReadOnlyList<T> Items { get; set; } = [];

    /// <summary>One-based page number.</summary>
    public int Page { get; set; }

    /// <summary>Maximum items per page.</summary>
    public int PageSize { get; set; }

    /// <summary>Total matching rows across all pages.</summary>
    public int TotalCount { get; set; }
}
