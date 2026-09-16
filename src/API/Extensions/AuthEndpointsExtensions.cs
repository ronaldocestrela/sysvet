using Core.Application.Auth.Commands;
using Core.Application.Auth.Queries;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Maps authentication routes under <c>/api/v1/auth</c>.
/// </summary>
public static class AuthEndpointsExtensions
{
    /// <summary>
    /// Registers login, refresh, profile, and optional development registration endpoints.
    /// </summary>
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder builder, IHostEnvironment environment)
    {
        var group = builder.MapGroup("/api/v1/auth")
            .WithTags("Auth");

        group.MapPost("/login", async ([FromBody] LoginCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapPost("/refresh", async ([FromBody] RefreshTokenCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        group.MapGet("/me", async (IMediator mediator) =>
            (await mediator.Send(new GetCurrentUserQuery())).ToHttpResult())
            .RequireAuthorization();

        if (environment.IsDevelopment())
        {
            group.MapPost("/register", async ([FromBody] RegisterUserCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/v1/auth/me", result.Value)
                    : result.ToProblemDetails();
            });
        }

        return builder;
    }
}
