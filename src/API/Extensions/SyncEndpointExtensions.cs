using Core.Application.Sync;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// Minimal API routes for offline sync push/pull (ADR-002).
/// </summary>
public static class SyncEndpointExtensions
{
    /// <summary>
    /// Maps sync endpoints under /api/v1/sync.
    /// </summary>
    public static void MapSyncEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/sync")
            .RequireAuthorization(Core.Application.Authorization.AuthorizationPolicies.ClinicStaff)
            .WithTags("Core", "Sync");

        group.MapPost("/push", async ([FromBody] List<SyncOutboxMessageDto> messages, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(new PushSyncBatchCommand(messages ?? []));
            return result.ToHttpResult(httpContext);
        });

        group.MapGet("/pull", async ([FromQuery] DateTimeOffset since, [FromQuery] int? take, IMediator mediator, HttpContext httpContext) =>
        {
            var result = await mediator.Send(new PullChangesQuery(since, take ?? 100));
            return result.ToHttpResult(httpContext);
        });
    }
}
