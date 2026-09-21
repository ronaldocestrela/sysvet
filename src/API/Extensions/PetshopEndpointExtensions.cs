using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Petshop.Application.GroomingAppointments.Commands;
using Petshop.Application.GroomingAppointments.Queries;
using Petshop.Application.GroomingRecords;
using Petshop.Application.GroomingServices;
using Petshop.Application.GroomingSlots.Commands;
using Petshop.Application.GroomingSlots.Queries;

namespace API.Extensions;

/// <summary>
/// Petshop module minimal API endpoints for grooming salon operations.
/// </summary>
public static class PetshopEndpointExtensions
{
    /// <summary>
    /// Maps grooming appointments, slots, services, and pet history routes.
    /// </summary>
    public static IEndpointRouteBuilder MapPetshopEndpoints(this IEndpointRouteBuilder builder)
    {
        var appointments = builder.MapGroup("/api/v1/grooming-appointments").RequireAuthorization().WithTags("Petshop", "Grooming");

        appointments.MapPost("/", async (HttpContext httpContext, [FromBody] ScheduleGroomingAppointmentCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command with { IdempotencyKey = key })).ToHttpResult();
        });

        appointments.MapGet("/daily", async ([FromQuery] Guid? groomerId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetDailyGroomingScheduleQuery(groomerId, date))).ToHttpResult());

        appointments.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetGroomingAppointmentByIdQuery(id))).ToHttpResult());

        appointments.MapGet("/{id:guid}/record", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetGroomingRecordByAppointmentQuery(id))).ToHttpResult());

        appointments.MapPut("/{id:guid}/reschedule", async (Guid id, HttpContext httpContext, [FromBody] RescheduleGroomingRequest body, IMediator mediator) =>
            (await mediator.Send(new RescheduleGroomingAppointmentCommand(id, body.NewDate, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPost("/{id:guid}/confirm", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new ConfirmGroomingAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPost("/{id:guid}/start", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new StartGroomingAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPost("/{id:guid}/complete", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CompleteGroomingAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPost("/{id:guid}/no-show", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new MarkNoShowGroomingAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CancelGroomingAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        appointments.MapPut("/{id:guid}/record", async (Guid id, HttpContext httpContext, [FromBody] UpdateGroomingRecordBody body, IMediator mediator) =>
            (await mediator.Send(new UpdateGroomingRecordCommand(id, body.CoatNotes, body.SupplyLines, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var slots = builder.MapGroup("/api/v1/grooming-slots").RequireAuthorization().WithTags("Petshop", "GroomingSlots");

        slots.MapGet("/", async ([FromQuery] Guid groomerId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetGroomingScheduleSlotsQuery(groomerId, date))).ToHttpResult());

        slots.MapPost("/availability", async (HttpContext httpContext, [FromBody] DefineGroomingDailyAvailabilityCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        slots.MapPost("/{id:guid}/block", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new BlockGroomingSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        slots.MapPost("/{id:guid}/unblock", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new UnblockGroomingSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var services = builder.MapGroup("/api/v1/grooming-services").RequireAuthorization().WithTags("Petshop", "GroomingServices");

        services.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new ListGroomingServicesQuery())).ToHttpResult());

        services.MapPut("/", async (HttpContext httpContext, [FromBody] UpsertGroomingServiceCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        builder.MapGet("/api/v1/pets/{petId:guid}/grooming-history", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new GetPetGroomingHistoryQuery(petId))).ToHttpResult())
            .RequireAuthorization()
            .WithTags("Petshop", "GroomingHistory");

        return builder;
    }

    private sealed record RescheduleGroomingRequest(DateTimeOffset NewDate);

    private sealed record UpdateGroomingRecordBody(string CoatNotes, IReadOnlyList<GroomingRecordSupplyLineDto> SupplyLines);
}
