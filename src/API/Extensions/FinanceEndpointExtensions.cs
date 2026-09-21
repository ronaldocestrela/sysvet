using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Finance module HTTP endpoints (stub until AP/AR and cash flow APIs are implemented).
/// </summary>
public static class FinanceEndpointExtensions
{
    /// <summary>
    /// Maps Finance routes when the module exposes public APIs.
    /// </summary>
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder builder) => builder;
}
