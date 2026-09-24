using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Platform.Application.Status;

namespace API.Extensions;

/// <summary>Anonymous platform-facing endpoints (ADR-060).</summary>
public static class PlatformPublicEndpointsExtensions
{
    /// <summary>Maps the public status page JSON API.</summary>
    public static IEndpointRouteBuilder MapPlatformPublicEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/api/v1/public/status", async (IMediator mediator) =>
                (await mediator.Send(new GetPublicStatusQuery())).ToHttpResult())
            .WithTags("Public")
            .AllowAnonymous();

        return builder;
    }
}
