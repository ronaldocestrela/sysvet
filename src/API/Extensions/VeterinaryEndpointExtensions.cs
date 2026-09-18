using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Appointments.Queries;
using Veterinary.Application.ScheduleSlots.Commands;
using Veterinary.Application.ScheduleSlots.Queries;

namespace API.Extensions;

/// <summary>
/// Veterinary module minimal API endpoints.
/// </summary>
public static class VeterinaryEndpointExtensions
{
    /// <summary>
    /// Maps appointments, schedule slots, vaccines, and hospitalization routes.
    /// </summary>
    public static IEndpointRouteBuilder MapVeterinaryEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/appointments").RequireAuthorization().WithTags("Veterinary", "Appointments");

        group.MapPost("/", async (HttpContext httpContext, [FromBody] ScheduleAppointmentCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            var commandWithKey = command with { IdempotencyKey = key };
            return (await mediator.Send(commandWithKey)).ToHttpResult();
        });

        group.MapGet("/daily", async ([FromQuery] Guid? veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetDailyScheduleQuery(veterinarianId, date))).ToHttpResult());

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetAppointmentByIdQuery(id))).ToHttpResult());

        group.MapPut("/{id:guid}/reschedule", async (Guid id, HttpContext httpContext, [FromBody] RescheduleRequest request, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new RescheduleAppointmentCommand(id, request.NewDate, IdempotencyKey: key))).ToHttpResult();
        });

        group.MapPost("/{id:guid}/confirm", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new ConfirmAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/start", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new StartAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/complete", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CompleteAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/no-show", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new MarkNoShowAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CancelAppointmentCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/{id:guid}/records", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.CreateMedicalRecordCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var slotsGroup = builder.MapGroup("/api/v1/schedule-slots").RequireAuthorization().WithTags("Veterinary", "ScheduleSlots");

        slotsGroup.MapGet("/", async ([FromQuery] Guid veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetScheduleSlotsQuery(veterinarianId, date))).ToHttpResult());

        slotsGroup.MapPost("/availability", async (HttpContext httpContext, [FromBody] DefineDailyAvailabilityCommand command, IMediator mediator) =>
        {
            var key = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command with { IdempotencyKey = key })).ToHttpResult();
        });

        slotsGroup.MapPost("/{id:guid}/block", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new BlockScheduleSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        slotsGroup.MapPost("/{id:guid}/unblock", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new UnblockScheduleSlotCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var recordsGroup = builder.MapGroup("/api/v1/medical-records").RequireAuthorization().WithTags("Veterinary", "MedicalRecords");

        recordsGroup.MapGet("/by-pet/{petId:guid}", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetPetClinicalTimelineQuery(petId))).ToHttpResult());

        recordsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetMedicalRecordByIdQuery(id))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/anamnesis", async (Guid id, HttpContext httpContext, [FromBody] AnamnesisRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.UpdateAnamnesisCommand(id, body.Anamnesis, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/vitals", async (Guid id, HttpContext httpContext, [FromBody] VitalSignsRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.RecordVitalSignsCommand(
                id, body.WeightKg, body.TemperatureC, body.HeartRateBpm, body.RespiratoryRateBpm,
                body.MucousMembranes, body.CapillaryRefillTime, body.MeasuredAt, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/diagnosis", async (Guid id, HttpContext httpContext, [FromBody] DiagnosisRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.SetDiagnosisCommand(id, body.Diagnosis, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPatch("/{id:guid}/conduct", async (Guid id, HttpContext httpContext, [FromBody] ConductRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.SetConductCommand(id, body.Conduct, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPost("/{id:guid}/evolution", async (Guid id, HttpContext httpContext, [FromBody] EvolutionNoteRequest body, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.AddEvolutionNoteCommand(id, body.Text, body.NoteId, body.RecordedAt, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        recordsGroup.MapPost("/{id:guid}/finalize", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.FinalizeMedicalRecordCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var petsGroup = builder.MapGroup("/api/v1/pets").RequireAuthorization().WithTags("Veterinary", "Pets (Veterinary)");

        petsGroup.MapGet("/{petId:guid}/medical-records", async (Guid petId, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Queries.GetPetClinicalTimelineQuery(petId))).ToHttpResult());

        petsGroup.MapPost("/{petId:guid}/vaccines", async (Guid petId, [FromBody] Veterinary.Application.Vaccines.Commands.RegisterVaccineDoseCommand command, IMediator mediator) =>
        {
            if (petId != command.PetId)
            {
                return Result.Failure<Guid>(new Error("Vaccine.MismatchedPetId", "URL petId does not match command.")).ToHttpResult();
            }

            return (await mediator.Send(command)).ToHttpResult();
        });

        var hospGroup = builder.MapGroup("/api/v1/hospitalizations").RequireAuthorization().WithTags("Veterinary", "Hospitalizations");
        hospGroup.MapPost("/", async ([FromBody] Veterinary.Application.Hospitalizations.Commands.AdmitPetCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        hospGroup.MapPost("/{id:guid}/discharge", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new Veterinary.Application.Hospitalizations.Commands.DischargePetCommand(id));
            return result.IsSuccess ? Results.NoContent() : result.ToProblemDetails();
        });

        hospGroup.MapPost("/{id:guid}/prescriptions/execute", async (Guid id, [FromBody] Veterinary.Application.Hospitalizations.Commands.ExecutePrescriptionCommand command, IMediator mediator) =>
        {
            if (id != command.HospitalizationId)
            {
                return Result.Failure<Guid>(new Error("Hospitalization.MismatchedId", "URL id does not match command.")).ToHttpResult();
            }

            return (await mediator.Send(command)).ToHttpResult();
        });

        return builder;
    }

    /// <summary>
    /// Request body for rescheduling an appointment.
    /// </summary>
    /// <param name="NewDate">New scheduled date and time.</param>
    public record RescheduleRequest(DateTimeOffset NewDate);

    /// <summary>Anamnesis update body.</summary>
    public record AnamnesisRequest(string Anamnesis);

    /// <summary>Vital signs update body.</summary>
    public record VitalSignsRequest(
        decimal WeightKg,
        decimal TemperatureC,
        int? HeartRateBpm,
        int? RespiratoryRateBpm,
        string MucousMembranes,
        string CapillaryRefillTime,
        DateTimeOffset MeasuredAt);

    /// <summary>Diagnosis update body.</summary>
    public record DiagnosisRequest(string Diagnosis);

    /// <summary>Conduct update body.</summary>
    public record ConductRequest(string Conduct);

    /// <summary>Evolution note append body.</summary>
    public record EvolutionNoteRequest(string Text, Guid NoteId = default, DateTimeOffset? RecordedAt = null);
}
