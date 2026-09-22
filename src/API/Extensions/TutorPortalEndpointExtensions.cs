using Core.Application.Authorization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TutorPortal.Application.Auth.Commands;
using TutorPortal.Application.Auth.Queries;

namespace API.Extensions;

/// <summary>
/// Maps tutor portal routes under <c>/api/v1/tutor-portal</c>.
/// </summary>
public static class TutorPortalEndpointExtensions
{
    /// <summary>
    /// Registers tutor self-registration, login, refresh, and profile endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapTutorPortalEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/tutor-portal")
            .WithTags("TutorPortal");

        group.MapPost("/register", async ([FromBody] RegisterTutorCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/login", async ([FromBody] TutorLoginCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/refresh", async ([FromBody] RefreshTutorTokenCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/me", async (IMediator mediator) =>
            (await mediator.Send(new GetTutorPortalMeQuery())).ToHttpResult())
            .RequireAuthorization(AuthorizationPolicies.TutorPortal);

        return builder;
    }
}
