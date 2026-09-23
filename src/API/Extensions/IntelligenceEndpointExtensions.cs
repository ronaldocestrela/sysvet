using Intelligence.Application.Dashboard.Commands;
using Intelligence.Application.Dashboard.Dtos;
using Intelligence.Application.Dashboard.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>Intelligence module HTTP endpoints (roadmap 10.1).</summary>
public static class IntelligenceEndpointExtensions
{
    /// <summary>Maps tenant dashboard routes under <c>/api/v1/intelligence</c>.</summary>
    public static IEndpointRouteBuilder MapIntelligenceEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/intelligence")
            .RequireAuthorization()
            .WithTags("Intelligence");

        group.MapGet("/dashboard", async (IMediator mediator) =>
            (await mediator.Send(new GetTenantDashboardQuery())).ToHttpResult());

        group.MapGet("/dashboard-layouts/{accessProfileId:guid}", async (Guid accessProfileId, IMediator mediator) =>
            (await mediator.Send(new GetProfileDashboardLayoutQuery(accessProfileId))).ToHttpResult());

        group.MapPut("/dashboard-layouts/{accessProfileId:guid}", async (
            Guid accessProfileId,
            [FromBody] ProfileDashboardLayoutDto body,
            IMediator mediator) =>
            (await mediator.Send(new UpsertProfileDashboardLayoutCommand(
                accessProfileId,
                body.Slots))).ToHttpResult());

        return builder;
    }
}
