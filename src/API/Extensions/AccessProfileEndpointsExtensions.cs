using Core.Application.AccessProfiles.Commands;
using Core.Application.AccessProfiles.Dtos;
using Core.Application.AccessProfiles.Queries;
using Core.Application.Authorization;
using Core.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// Access profile admin endpoints.
/// </summary>
public static class AccessProfileEndpointsExtensions
{
    /// <summary>
    /// Maps access profile and permission catalog routes.
    /// </summary>
    public static void MapAccessProfileEndpoints(this IEndpointRouteBuilder builder)
    {
        var profiles = builder.MapGroup("/api/v1/access-profiles")
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithTags("Core", "AccessProfiles");

        profiles.MapGet("/", ListProfiles)
            .WithSummary("List access profiles")
            .Produces<PagedResult<AccessProfileDto>>(StatusCodes.Status200OK);

        profiles.MapGet("/{id:guid}", GetProfile)
            .WithSummary("Get access profile")
            .Produces<AccessProfileDto>(StatusCodes.Status200OK);

        profiles.MapPost("/", CreateProfile)
            .WithSummary("Create custom access profile")
            .Produces<Guid>(StatusCodes.Status201Created);

        profiles.MapPut("/{id:guid}", UpdateProfile)
            .WithSummary("Update access profile matrix")
            .Produces(StatusCodes.Status204NoContent);

        profiles.MapDelete("/{id:guid}", DeleteProfile)
            .WithSummary("Delete custom access profile")
            .Produces(StatusCodes.Status204NoContent);

        builder.MapGet("/api/v1/permissions", ListPermissions)
            .RequireAuthorization(AuthorizationPolicies.Admin)
            .WithTags("Core", "AccessProfiles")
            .WithSummary("List permission catalog")
            .Produces<IReadOnlyList<string>>(StatusCodes.Status200OK);
    }

    private static async Task<IResult> ListProfiles([AsParameters] ListAccessProfilesQuery query, IMediator mediator) =>
        (await mediator.Send(query)).ToHttpResult();

    private static async Task<IResult> GetProfile(Guid id, IMediator mediator) =>
        (await mediator.Send(new GetAccessProfileByIdQuery(id))).ToHttpResult();

    private static async Task<IResult> CreateProfile([FromBody] CreateAccessProfileCommand command, IMediator mediator)
    {
        var result = await mediator.Send(command);
        return result.IsSuccess
            ? Results.Created($"/api/v1/access-profiles/{result.Value}", result.Value)
            : result.ToProblemDetails();
    }

    private static async Task<IResult> UpdateProfile(Guid id, [FromBody] UpdateAccessProfileBody body, IMediator mediator)
    {
        var result = await mediator.Send(new UpdateAccessProfileCommand(id, body.Name, body.Description, body.PermissionCodes));
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteProfile(Guid id, IMediator mediator) =>
        (await mediator.Send(new DeleteAccessProfileCommand(id))).ToHttpResult();

    private static async Task<IResult> ListPermissions(IMediator mediator) =>
        (await mediator.Send(new ListPermissionsQuery())).ToHttpResult();

    private sealed record UpdateAccessProfileBody(string? Name, string? Description, IReadOnlyList<string> PermissionCodes);
}
