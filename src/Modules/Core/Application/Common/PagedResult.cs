namespace Core.Application.Common;

/// <summary>
/// API-facing paginated response for list queries.
/// </summary>
/// <typeparam name="T">DTO type.</typeparam>
/// <param name="Items">Items in the current page.</param>
/// <param name="Page">One-based page number.</param>
/// <param name="PageSize">Maximum items per page.</param>
/// <param name="TotalCount">Total matching rows across all pages.</param>
public sealed record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount);
