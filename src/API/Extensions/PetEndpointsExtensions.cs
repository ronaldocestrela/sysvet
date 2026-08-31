using Core.Application.Pets.Commands;
using Core.Application.Pets.Queries;
using MediatR;

using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

public static class PetEndpointsExtensions
{
    public static void MapPetEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1")
            .RequireAuthorization(new Microsoft.AspNetCore.Authorization.AuthorizeAttribute { Roles = "Admin,Veterinarian,Receptionist" });

        // Endpoints de Pets
        var petsGroup = group.MapGroup("/pets").WithTags("Pets");

        petsGroup.MapPost("/", async (HttpContext context, CreatePetCommand command, IMediator mediator) =>
        {
            var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
            Guid.TryParse(headerValue, out var key);
            var commandWithKey = command with { IdempotencyKey = key };
            var result = await mediator.Send(commandWithKey);
            return result.IsSuccess 
                ? Results.Created($"/api/v1/pets/{result.Value}", result.Value) 
                : result.ToProblemDetails();
        });

        petsGroup.MapPut("/{id:guid}", async (HttpContext context, Guid id, UpdatePetCommand command, IMediator mediator) =>
        {
            if (id != command.Id) return Results.BadRequest("O ID da rota difere do ID do comando.");
            
            var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
            Guid.TryParse(headerValue, out var key);
            var commandWithKey = command with { IdempotencyKey = key };
            var result = await mediator.Send(commandWithKey);
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
