using Core.Application.Authorization;
using Core.Application.Common;
using Core.Application.Tutors.Commands;
using Core.Application.Tutors.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// CRM tutor REST endpoints under <c>/api/v1/tutors</c>.
/// </summary>
public static class TutorEndpointsExtensions
{
    /// <summary>
    /// Maps tutor CRUD routes protected by <see cref="AuthorizationPolicies.ClinicStaff"/>.
    /// </summary>
    public static void MapTutorEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/tutors")
            .RequireAuthorization(AuthorizationPolicies.ClinicStaff)
            .WithTags("Tutors");

        group.MapPost("/", CreateTutor)
            .WithName("CreateTutor")
            .WithSummary("Create a tutor")
            .WithDescription("Registers a new tutor (client) with CPF, e-mail, and phone.")
            .Produces<Guid>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPut("/{id:guid}", UpdateTutor)
            .WithName("UpdateTutor")
            .WithSummary("Update a tutor")
            .WithDescription("Updates mutable tutor fields. CPF cannot be changed.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/{id:guid}", GetTutorById)
            .WithName("GetTutorById")
            .WithSummary("Get tutor by id")
            .Produces<TutorDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapGet("/", ListTutors)
            .WithName("ListTutors")
            .WithSummary("List tutors")
            .WithDescription("Paginated list with optional name and CPF filters.")
            .Produces<PagedResult<TutorDto>>(StatusCodes.Status200OK);

        group.MapDelete("/{id:guid}", DeleteTutor)
            .WithName("DeleteTutor")
            .WithSummary("Soft-delete a tutor")
            .WithDescription("Marks the tutor and linked pets as deleted.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound);
    }

    private static async Task<IResult> CreateTutor(HttpContext context, CreateTutorCommand command, IMediator mediator)
    {
        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var commandWithKey = command with { IdempotencyKey = key };
        var result = await mediator.Send(commandWithKey);
        return result.IsSuccess
            ? result.ToCreatedAt($"/api/v1/tutors/{result.Value}")
            : result.ToProblemDetails();
    }

    private static async Task<IResult> UpdateTutor(HttpContext context, Guid id, UpdateTutorCommand command, IMediator mediator)
    {
        if (id != command.Id) return Results.BadRequest("O ID da rota difere do ID do comando.");

        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var commandWithKey = command with { IdempotencyKey = key };
        var result = await mediator.Send(commandWithKey);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetTutorById(Guid id, IMediator mediator)
    {
        var result = await mediator.Send(new GetTutorByIdQuery(id));
        return result.ToHttpResult();
    }

    private static async Task<IResult> ListTutors([AsParameters] ListTutorsQuery query, IMediator mediator)
    {
        var result = await mediator.Send(query);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteTutor(HttpContext context, Guid id, IMediator mediator)
    {
        var headerValue = context.Request.Headers["Idempotency-Key"].FirstOrDefault();
        Guid.TryParse(headerValue, out var key);
        var result = await mediator.Send(new DeleteTutorCommand(id, key));
        return result.ToHttpResult();
    }
}
