using MediatR;
using Microsoft.AspNetCore.Mvc;
using Veterinary.Application.Appointments.Commands;
using Veterinary.Application.Appointments.Queries;

namespace API.Extensions;

public static class VeterinaryEndpointExtensions
{
    public static void MapVeterinaryEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/appointments").RequireAuthorization().WithTags("Appointments");

        group.MapPost("/", async ([FromBody] ScheduleAppointmentCommand command, IMediator mediator) =>
        {
            try
            {
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
            }
            catch(Exception ex)
            {
                return Results.BadRequest("EXCEPTION: " + ex.ToString());
            }
        }).RequireAuthorization();

        group.MapGet("/daily", async ([FromQuery] Guid veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
        {
            var query = new GetDailyScheduleQuery(veterinarianId, date);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
        group.MapPut("/{id:guid}/reschedule", async (Guid id, [FromBody] RescheduleRequest request, IMediator mediator) =>
        {
            var command = new RescheduleAppointmentCommand(id, request.NewDate);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        group.MapPost("/{id:guid}/records", async (Guid id, IMediator mediator) =>
        {
            var command = new Veterinary.Application.MedicalRecords.Commands.CreateMedicalRecordCommand(id);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        var petsGroup = builder.MapGroup("/api/v1/pets").RequireAuthorization().WithTags("Pets (Veterinary)");
        petsGroup.MapPost("/{petId:guid}/vaccines", async (Guid petId, [FromBody] Veterinary.Application.Vaccines.Commands.RegisterVaccineDoseCommand command, IMediator mediator) =>
        {
            if (petId != command.PetId)
                return Results.BadRequest(new Core.Domain.Error("Vaccine.MismatchedPetId", "URL petId does not match command."));

            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        var hospGroup = builder.MapGroup("/api/v1/hospitalizations").RequireAuthorization().WithTags("Hospitalizations");
        hospGroup.MapPost("/", async ([FromBody] Veterinary.Application.Hospitalizations.Commands.AdmitPetCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        hospGroup.MapPost("/{id:guid}/discharge", async (Guid id, IMediator mediator) =>
        {
            var command = new Veterinary.Application.Hospitalizations.Commands.DischargePetCommand(id);
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.NoContent() : Results.BadRequest(result.Error);
        });

        hospGroup.MapPost("/{id:guid}/prescriptions/execute", async (Guid id, [FromBody] Veterinary.Application.Hospitalizations.Commands.ExecutePrescriptionCommand command, IMediator mediator) =>
        {
            if (id != command.HospitalizationId)
                return Results.BadRequest(new Core.Domain.Error("Hospitalization.MismatchedId", "URL id does not match command."));

            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }

    public record RescheduleRequest(DateTimeOffset NewDate);
}
