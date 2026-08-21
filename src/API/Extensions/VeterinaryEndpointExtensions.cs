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
            var result = await mediator.Send(command);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });

        group.MapGet("/daily", async ([FromQuery] Guid veterinarianId, [FromQuery] DateTimeOffset date, IMediator mediator) =>
        {
            var query = new GetDailyScheduleQuery(veterinarianId, date);
            var result = await mediator.Send(query);
            return result.IsSuccess ? Results.Ok(result.Value) : Results.BadRequest(result.Error);
        });
    }
}
