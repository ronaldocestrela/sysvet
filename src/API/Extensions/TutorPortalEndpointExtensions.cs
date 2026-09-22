using Core.Application.Authorization;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using TutorPortal.Application.Auth.Commands;
using TutorPortal.Application.Auth.Queries;
using TutorPortal.Application.PetHealth.Queries;
using TutorPortal.Application.Push.Commands;
using TutorPortal.Application.Push.Queries;

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

        var petsGroup = group.MapGroup("/pets").RequireAuthorization(AuthorizationPolicies.TutorPortal);

        petsGroup.MapGet("/{petId:guid}/vaccination-card", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new GetTutorVaccinationCardQuery(petId))).ToHttpResult());

        petsGroup.MapGet("/{petId:guid}/exams", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new ListTutorPetExamsQuery(petId))).ToHttpResult());

        petsGroup.MapGet("/{petId:guid}/timeline", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new ListTutorPetTimelineQuery(petId))).ToHttpResult());

        var pushGroup = group.MapGroup("/push").RequireAuthorization(AuthorizationPolicies.TutorPortal);

        pushGroup.MapGet("/vapid-public-key", async (IMediator mediator) =>
            (await mediator.Send(new GetTutorPushVapidPublicKeyQuery())).ToHttpResult());

        pushGroup.MapPost("/subscribe", async ([FromBody] SubscribeTutorPushCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        pushGroup.MapPost("/unsubscribe", async ([FromBody] UnsubscribeTutorPushCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        return builder;
    }
}
