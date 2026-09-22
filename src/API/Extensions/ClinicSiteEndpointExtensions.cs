using API.Filters;
using ClinicSite.Application.Commands;
using ClinicSite.Application.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Clinic site staff and public HTTP endpoints (Fase 8.7).
/// </summary>
public static class ClinicSiteEndpointExtensions
{
    /// <summary>
    /// Maps staff clinic site routes under <c>/api/v1/clinic-site</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapClinicSiteEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/clinic-site")
            .RequireAuthorization()
            .WithTags("ClinicSite");

        group.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new GetClinicSiteQuery())).ToHttpResult());

        group.MapPut("/", async ([FromBody] UpdateClinicSiteProfileBody body, IMediator mediator) =>
            (await mediator.Send(new UpdateClinicSiteProfileCommand(
                body.DisplayName,
                body.Tagline,
                body.Street,
                body.Number,
                body.Complement,
                body.District,
                body.City,
                body.State,
                body.PostalCode,
                body.Phone,
                body.Email,
                body.WhatsApp,
                body.LogoUrl,
                body.Slug))).ToHttpResult());

        group.MapPut("/services", async ([FromBody] ReplaceServicesBody body, IMediator mediator) =>
            (await mediator.Send(new ReplaceClinicSiteServicesCommand(body.Services ?? []))).ToHttpResult());

        group.MapPut("/team", async ([FromBody] ReplaceTeamBody body, IMediator mediator) =>
            (await mediator.Send(new ReplaceClinicSiteTeamCommand(body.Team ?? []))).ToHttpResult());

        group.MapPut("/hours", async ([FromBody] ReplaceHoursBody body, IMediator mediator) =>
            (await mediator.Send(new ReplaceClinicSiteHoursCommand(body.Hours ?? []))).ToHttpResult());

        group.MapPost("/publish", async (IMediator mediator) =>
            (await mediator.Send(new PublishClinicSiteCommand())).ToHttpResult());

        group.MapPost("/unpublish", async (IMediator mediator) =>
            (await mediator.Send(new UnpublishClinicSiteCommand())).ToHttpResult());

        return builder;
    }

    /// <summary>
    /// Maps anonymous public site API under <c>/api/v1/public/clinic-sites</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapClinicSitePublicEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/public/clinic-sites")
            .WithTags("ClinicSitePublic")
            .AddEndpointFilter<ClinicSitePublicTenantFilter>();

        group.MapGet("/{slug}", async (string slug, IMediator mediator) =>
            (await mediator.Send(new GetPublicClinicSiteQuery(slug))).ToHttpResult());

        return builder;
    }

    private sealed record UpdateClinicSiteProfileBody(
        string DisplayName,
        string? Tagline,
        string Street,
        string Number,
        string? Complement,
        string District,
        string City,
        string State,
        string PostalCode,
        string Phone,
        string Email,
        string? WhatsApp,
        string? LogoUrl,
        string Slug);

    private sealed record ReplaceServicesBody(IReadOnlyList<ClinicSiteServiceDto>? Services);

    private sealed record ReplaceTeamBody(IReadOnlyList<ClinicSiteTeamMemberDto>? Team);

    private sealed record ReplaceHoursBody(IReadOnlyList<ClinicSiteHoursDto>? Hours);
}
