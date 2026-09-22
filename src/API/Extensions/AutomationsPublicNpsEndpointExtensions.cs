using API.Filters;
using Automations.Application.Nps.Commands;
using Automations.Application.Nps.Dtos;
using Automations.Application.Nps.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Anonymous NPS survey endpoints for tutors (Fase 8.3).
/// </summary>
public static class AutomationsPublicNpsEndpointExtensions
{
    /// <summary>
    /// Maps public NPS routes under <c>/api/v1/public/nps</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapAutomationsPublicNpsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/public/nps")
            .WithTags("NpsPublic")
            .AddEndpointFilter<NpsPublicTenantFilter>();

        group.MapGet("/{token}", async (string token, IMediator mediator) =>
            (await mediator.Send(new GetNpsPublicPreviewQuery(token))).ToHttpResult());

        group.MapPost("/{token}", async (string token, [FromBody] SubmitNpsBody body, IMediator mediator) =>
            (await mediator.Send(new SubmitNpsResponseCommand(token, body.Score, body.Comment))).ToHttpResult());

        return builder;
    }

    private sealed record SubmitNpsBody(int Score, string? Comment);
}
