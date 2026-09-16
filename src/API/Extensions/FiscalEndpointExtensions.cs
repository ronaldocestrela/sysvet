using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Fiscal module HTTP endpoints (stub until electronic invoicing is implemented).
/// </summary>
public static class FiscalEndpointExtensions
{
    /// <summary>
    /// Maps Fiscal routes when the module exposes public APIs.
    /// </summary>
    public static IEndpointRouteBuilder MapFiscalEndpoints(this IEndpointRouteBuilder builder) => builder;
}
