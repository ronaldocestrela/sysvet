using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Appointments.Queries;

namespace API.Extensions;

/// <summary>
/// Veterinary module minimal API endpoints.
/// </summary>
public static class VeterinaryEndpointExtensions
{
    /// <summary>
    /// Maps appointments, vaccines, and hospitalization routes.
    /// </summary>
    public static IEndpointRouteBuilder MapVeterinaryEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/appointments").RequireAuthorization().WithTags("Appointments");

        group.MapPost("/", async ([FromBody] ScheduleAppointmentCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/daily", async ([FromQuery] Guid veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
            (await mediator.Send(new GetDailyScheduleQuery(veterinarianId, date))).ToHttpResult());

        group.MapPut("/{id:guid}/reschedule", async (Guid id, [FromBody] RescheduleRequest request, IMediator mediator) =>
            (await mediator.Send(new RescheduleAppointmentCommand(id, request.NewDate))).ToHttpResult());

        group.MapPost("/{id:guid}/records", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new Veterinary.Application.MedicalRecords.Commands.CreateMedicalRecordCommand(id))).ToHttpResult());

        var petsGroup = builder.MapGroup("/api/v1/pets").RequireAuthorization().WithTags("Pets (Veterinary)");
        petsGroup.MapPost("/{petId:guid}/vaccines", async (Guid petId, [FromBody] Veterinary.Application.Vaccines.Commands.RegisterVaccineDoseCommand command, IMediator mediator) =>
        {
            if (petId != command.PetId)
            {
                return Result.Failure<Guid>(new Error("Vaccine.MismatchedPetId", "URL petId does not match command.")).ToHttpResult();
            }

            return (await mediator.Send(command)).ToHttpResult();
        });

        var hospGroup = builder.MapGroup("/api/v1/hospitalizations").RequireAuthorization().WithTags("Hospitalizations");
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
}
