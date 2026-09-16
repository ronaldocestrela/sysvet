using Core.Application.Preferences.Commands;
using Core.Application.Preferences.Dtos;
using Core.Application.Preferences.Queries;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace API.Extensions;

/// <summary>
/// Authenticated user preference endpoints.
/// </summary>
public static class PreferenceEndpointsExtensions
{
    /// <summary>
    /// Maps <c>/api/v1/me/preferences</c> routes.
    /// </summary>
    public static void MapPreferenceEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/me/preferences")
            .RequireAuthorization()
            .WithTags("Preferences");

        group.MapGet("/", GetPreferences)
            .WithSummary("Get my UI preferences")
            .Produces<UserPreferenceDto>(StatusCodes.Status200OK);

        group.MapPut("/", UpsertPreferences)
            .WithSummary("Update my UI preferences")
            .Produces(StatusCodes.Status204NoContent);
    }

    private static async Task<IResult> GetPreferences(IMediator mediator) =>
        (await mediator.Send(new GetMyPreferencesQuery())).ToHttpResult();

    private static async Task<IResult> UpsertPreferences([FromBody] UpsertMyPreferencesCommand command, IMediator mediator) =>
        (await mediator.Send(command)).ToHttpResult();
}
