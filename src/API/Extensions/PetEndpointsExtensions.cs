using Core.Application.Pets.Commands;
using Core.Application.Pets.Queries;
using MediatR;

namespace API.Extensions;

public static class PetEndpointsExtensions
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1").RequireAuthorization();

        // Endpoints de Pets
        var petsGroup = group.MapGroup("/pets").WithTags("Pets");

        petsGroup.MapPost("/", async (CreatePetCommand command, IMediator mediator) =>
        {
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.Created($"/api/v1/pets/{result.Value}", result.Value) 
                : result.ToProblemDetails();
        });

        petsGroup.MapPut("/{id:guid}", async (Guid id, UpdatePetCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("O ID da rota difere do ID do comando.");
            
            var result = await mediator.Send(command);
            return result.IsSuccess 
                ? Results.NoContent() 
                : result.ToProblemDetails();
        });

        petsGroup.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
        {
            var result = await mediator.Send(new GetPetByIdQuery(id));
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        });

        // Endpoint aninhado em Tutors
        group.MapGet("/tutors/{tutorId:guid}/pets", async (Guid tutorId, [AsParameters] ListPetsQuery query, IMediator mediator) =>
        {
            // Força o TutorId na query
            var queryWithTutor = query with { TutorId = tutorId };
            var result = await mediator.Send(queryWithTutor);
            return result.IsSuccess 
                ? Results.Ok(result.Value) 
                : result.ToProblemDetails();
        }).WithTags("Pets");
    }
}
