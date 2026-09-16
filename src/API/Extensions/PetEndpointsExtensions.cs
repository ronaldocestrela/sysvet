using Core.Application.Authorization;
using Core.Application.Common;
using Core.Application.Pets.Commands;
using Core.Application.Pets.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// CRM pet REST endpoints under <c>/api/v1/pets</c>.
/// </summary>
public static class PetEndpointsExtensions
{
    /// <summary>
    /// Maps pet CRUD routes protected by <see cref="AuthorizationPolicies.ClinicStaff"/>.
    /// </summary>
    public static void MapPetEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1")
            .RequireAuthorization(AuthorizationPolicies.ClinicStaff);

        var petsGroup = group.MapGroup("/pets").WithTags("Core", "Pets");

        petsGroup.MapPost("/", CreatePet)
            .WithName("CreatePet")
            .WithSummary("Create a pet")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        petsGroup.MapPut("/{id:guid}", UpdatePet)
            .WithName("UpdatePet")
            .WithSummary("Update a pet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        petsGroup.MapGet("/{id:guid}", GetPetById)
            .WithName("GetPetById")
            .WithSummary("Get pet by id")
            .Produces<PetDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        petsGroup.MapGet("/", ListPets)
            .WithName("ListPets")
            .WithSummary("List pets")
            .WithDescription("Paginated list with optional tutor id and name filters.")
            .Produces<PagedResult<PetDto>>(StatusCodes.Status200OK);

        petsGroup.MapDelete("/{id:guid}", DeletePet)
            .WithName("DeletePet")
            .WithSummary("Soft-delete a pet")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/tutors/{tutorId:guid}/pets", ListPetsByTutor)
            .WithName("ListPetsByTutor")
            .WithSummary("List pets for a tutor")
            .WithTags("Core", "Pets")
            .Produces<PagedResult<PetDto>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> CreatePet(HttpContext context, CreatePetCommand command, IMediator mediator)
    {
        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var commandWithKey = command with { IdempotencyKey = key };
        var result = await mediator.Send(commandWithKey);
        return result.IsSuccess
            ? result.ToCreatedAt($"/api/v1/pets/{result.Value}")
            : result.ToProblemDetails();
    }

    private static async Task<IResult> UpdatePet(HttpContext context, Guid id, UpdatePetCommand command, IMediator mediator)
    {
        if (id != command.Id) return ApiResultHelpers.RouteIdMismatch(context);

        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var commandWithKey = command with { IdempotencyKey = key };
        var result = await mediator.Send(commandWithKey);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetPetById(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetPetByIdQuery(id));
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListPets([AsParameters] ListPetsQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListPetsByTutor(Guid tutorId, [AsParameters] ListPetsQuery query, IMediator mediator)
    {
        var queryWithTutor = query with { TutorId = tutorId };
        var result = await mediator.Send(queryWithTutor);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeletePet(HttpContext context, Guid id, IMediator mediator)
    {
        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var result = await mediator.Send(new DeletePetCommand(id, key));
        return result.ToHttpResult();
    }
}
