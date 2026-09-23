using Core.Domain;

namespace Core.Application.Common;

/// <summary>
/// Normalizes list pagination parameters for API queries (max page size 100).
/// </summary>
public sealed record PageRequest(int Page, int PageSize)
{
    /// <summary>Default page size when callers omit or pass invalid values.</summary>
    public const int DefaultPageSize = 20;

    /// <summary>Hard upper bound for any list or take parameter.</summary>
    public const int MaxPageSize = 100;

    /// <summary>
    /// Validates and clamps pagination input.
    /// </summary>
    /// <param name="page">One-based page number from the client.</param>
    /// <param name="pageSize">Requested page size (defaults when below 1).</param>
    public static Result<PageRequest> TryCreate(int page, int pageSize)
    {
        if (page < 1)
        {
            return Result.Failure<PageRequest>(ErrorCodes.Pagination.InvalidPage);
        }

        if (pageSize < 1)
        {
            pageSize = DefaultPageSize;
        }

        if (pageSize > MaxPageSize)
        {
            return Result.Failure<PageRequest>(ErrorCodes.Pagination.PageSizeTooLarge);
        }

        return Result.Success(new PageRequest(page, pageSize));
    }

    /// <summary>Validates a take/skip style limit used by legacy list endpoints.</summary>
    public static Result<int> TryNormalizeTake(int take)
    {
        if (take < 1)
        {
            take = DefaultPageSize;
        }

        if (take > MaxPageSize)
        {
            return Result.Failure<int>(ErrorCodes.Pagination.PageSizeTooLarge);
        }

        return Result.Success(take);
    }
}
