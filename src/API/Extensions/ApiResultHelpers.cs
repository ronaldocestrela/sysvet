using Core.Application.Common;
using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace API.Extensions;

/// <summary>
/// Shared minimal API helpers for consistent API contracts.
/// </summary>
public static class ApiResultHelpers
{
    /// <summary>
    /// Maps route/body identifier mismatch to standardized Problem Details.
    /// </summary>
    public static IResult RouteIdMismatch(HttpContext? httpContext = null) =>
        Result.Failure(ErrorCodes.Request.RouteIdMismatch).ToProblemDetails(httpContext);
}
