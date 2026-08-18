using Core.Application.Tutors.Commands;
using Core.Application.Tutors.Queries;
using MediatR;

namespace API.Extensions;

public static class TutorEndpointsExtensions
{
    public static void MapTutorEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/tutors").RequireAuthorization().WithTags("Tutors");

        group.MapPost("/", async (RegisterTutorCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.Created($"/api/v1/tutors/{result.Value}", result.Value) 
                : result.ToProblemDetails();
        });

        group.MapPut("/{id:guid}", async (Guid id, UpdateTutorCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("O ID da rota difere do ID do comando.");
            
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.NoContent() 
                : result.ToProblemDetails();
        });

        group.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetTutorByIdQuery(id));
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        group.MapGet("/", async ([AsParameters] ListTutorsQuery query, IMediator mediator) =>
        {
            var result = await mediator.Send(query);
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });
    }
}
