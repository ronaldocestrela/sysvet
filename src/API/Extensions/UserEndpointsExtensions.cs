using Core.Application.Authorization;
using Core.Application.Common;
using Core.Application.Users.Commands;
using Core.Application.Users.Dtos;
using Core.Application.Users.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// Staff user admin endpoints under <c>/api/v1/users</c>.
/// </summary>
public static class UserEndpointsExtensions
{
    /// <summary>
    /// Maps tenant staff user CRUD routes.
    /// </summary>
    public static void MapUserEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/users")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithTags("Users");

        group.MapPost("/", CreateUser)
            .WithSummary("Create staff user")
            .Produces<string>(StatusCodes.Status201Created)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapGet("/", ListUsers)
            .WithSummary("List staff users")
            .Produces<PagedResult<StaffUserDto>>(StatusCodes.Status200OK);

        group.MapGet("/{userId}", GetUserById)
            .WithSummary("Get staff user by id")
            .Produces<StaffUserDto>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound);

        group.MapPut("/{userId}", UpdateUser)
            .WithSummary("Update staff user profile assignment")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{userId}/disable", DisableUser)
            .WithSummary("Disable staff user")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{userId}/enable", EnableUser)
            .WithSummary("Enable staff user")
            .Produces(StatusCodes.Status204NoContent);

        group.MapPost("/{userId}/reset-password", ResetPassword)
            .WithSummary("Reset staff user password")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> CreateUser([FromBody] CreateUserCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/v1/users/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> ListUsers([AsParameters] ListUsersQuery query, IMediator mediator) =>
        (await mediator.Send(query)).ToHttpResult();

    private static async Task<IResult> GetUserById(string userId, IMediator mediator) =>
        (await mediator.Send(new GetUserByIdQuery(userId))).ToHttpResult();

    private static async Task<IResult> UpdateUser(string userId, [FromBody] UpdateUserBody body, IMediator mediator)
    {
        var result = await mediator.Send(new UpdateUserCommand(userId, body.AccessProfileId, body.DisplayName));
        return result.ToHttpResult();
    }

    private static async Task<IResult> DisableUser(string userId, IMediator mediator) =>
        (await mediator.Send(new SetUserDisabledCommand(userId, true))).ToHttpResult();

    private static async Task<IResult> EnableUser(string userId, IMediator mediator) =>
        (await mediator.Send(new SetUserDisabledCommand(userId, false))).ToHttpResult();

    private static async Task<IResult> ResetPassword(string userId, [FromBody] ResetPasswordBody body, IMediator mediator) =>
        (await mediator.Send(new ResetUserPasswordCommand(userId, body.NewPassword))).ToHttpResult();

    private sealed record UpdateUserBody(Guid AccessProfileId, string? DisplayName);

    private sealed record ResetPasswordBody(string NewPassword);
}
