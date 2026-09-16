using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Petshop module HTTP endpoints (stub until grooming/bath features are implemented).
/// </summary>
public static class PetshopEndpointExtensions
{
    /// <summary>
    /// Maps Petshop routes when the module exposes public APIs.
    /// </summary>
    public static IEndpointRouteBuilder MapPetshopEndpoints(this IEndpointRouteBuilder builder) => builder;
}
