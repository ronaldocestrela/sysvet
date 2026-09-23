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
using TutorPortal.Application.Scheduling;
using TutorPortal.Application.Scheduling.Commands;
using TutorPortal.Application.Scheduling.Queries;

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
            (await mediator.Send(command)).ToHttpResult())
            .RequireRateLimiting(SecurityServiceCollectionExtensions.AuthRateLimitPolicy);

        group.MapPost("/login", async ([FromBody] TutorLoginCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult())
            .RequireRateLimiting(SecurityServiceCollectionExtensions.AuthRateLimitPolicy);

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

        var bookingGroup = petsGroup.MapGroup("/{petId:guid}/booking");

        bookingGroup.MapGet("/services", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new ListTutorBookableServicesQuery(petId))).ToHttpResult());

        bookingGroup.MapGet("/professionals", async (
            Guid petId,
            TutorBookingKind kind,
            Guid serviceId,
            DateTimeOffset date,
            IMediator mediator) =>
            (await mediator.Send(new ListTutorBookingProfessionalsQuery(petId, kind, serviceId, date))).ToHttpResult());

        bookingGroup.MapGet("/slots", async (
            Guid petId,
            TutorBookingKind kind,
            Guid serviceId,
            Guid professionalId,
            DateTimeOffset date,
            IMediator mediator) =>
            (await mediator.Send(new ListTutorAvailableSlotsQuery(petId, kind, serviceId, professionalId, date))).ToHttpResult());

        bookingGroup.MapGet("/appointments", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new ListTutorPetAppointmentsQuery(petId))).ToHttpResult());

        bookingGroup.MapPost("/appointments", async (Guid petId, [FromBody] BookTutorAppointmentRequest body, IMediator mediator) =>
            (await mediator.Send(new BookTutorAppointmentCommand(
                petId,
                body.Kind,
                body.ServiceId,
                body.ProfessionalId,
                body.Date,
                body.Id,
                body.IdempotencyKey))).ToHttpResult());

        bookingGroup.MapPost("/appointments/{appointmentId:guid}/cancel", async (
            Guid petId,
            Guid appointmentId,
            [FromBody] CancelTutorAppointmentRequest body,
            IMediator mediator) =>
            (await mediator.Send(new CancelTutorAppointmentCommand(petId, body.Kind, appointmentId))).ToHttpResult());

        var pushGroup = group.MapGroup("/push").RequireAuthorization(AuthorizationPolicies.TutorPortal);

        pushGroup.MapGet("/vapid-public-key", async (IMediator mediator) =>
            (await mediator.Send(new GetTutorPushVapidPublicKeyQuery())).ToHttpResult());

        pushGroup.MapPost("/subscribe", async ([FromBody] SubscribeTutorPushCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        pushGroup.MapPost("/unsubscribe", async ([FromBody] UnsubscribeTutorPushCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        return builder;
    }

    /// <summary>JSON body for tutor appointment booking.</summary>
    private sealed record BookTutorAppointmentRequest(
        TutorBookingKind Kind,
        Guid ServiceId,
        Guid ProfessionalId,
        DateTimeOffset Date,
        Guid Id,
        Guid IdempotencyKey = default);

    /// <summary>JSON body for tutor appointment cancellation.</summary>
    private sealed record CancelTutorAppointmentRequest(TutorBookingKind Kind);
}
