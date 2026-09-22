using Automations.Application.Campaigns.Commands;
using Automations.Application.Campaigns.Queries;
using Automations.Application.Jobs.Commands;
using Automations.Application.Jobs.Queries;
using Automations.Application.Nps.Queries;
using Automations.Application.Settings.Commands;
using Automations.Application.Templates.Commands;
using Automations.Domain.Enums;
using Automations.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Automations module HTTP endpoints for templates and message jobs.
/// </summary>
public static class AutomationsEndpointExtensions
{
    /// <summary>
    /// Maps Automations routes under <c>/api/v1/automations</c>.
    /// </summary>
    public static IEndpointRouteBuilder MapAutomationsEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/automations").RequireAuthorization().WithTags("Automations");

        group.MapGet("/templates", async (IMediator mediator) =>
            (await mediator.Send(new ListMessageTemplatesQuery())).ToHttpResult());

        group.MapGet("/templates/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetMessageTemplateByIdQuery(id))).ToHttpResult());

        group.MapPost("/templates", async (HttpContext httpContext, [FromBody] CreateMessageTemplateCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        group.MapPut("/templates/{id:guid}", async (Guid id, HttpContext httpContext, [FromBody] UpdateMessageTemplateBody body, IMediator mediator) =>
            (await mediator.Send(new UpdateMessageTemplateCommand(id, body.Body, body.Subject, body.IsActive, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/jobs", async ([FromQuery] MessageJobStatus? status, [FromQuery] int? take, IMediator mediator) =>
            (await mediator.Send(new ListMessageJobsQuery(status, take ?? 50))).ToHttpResult());

        group.MapGet("/jobs/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetMessageJobByIdQuery(id))).ToHttpResult());

        group.MapPost("/jobs", async (HttpContext httpContext, [FromBody] EnqueueMessageJobCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        group.MapGet("/settings", async (IMediator mediator) =>
            (await mediator.Send(new GetAutomationsSettingsQuery())).ToHttpResult());

        group.MapPut("/settings", async (HttpContext httpContext, [FromBody] UpdateAutomationsSettingsBody body, IMediator mediator) =>
            (await mediator.Send(new UpdateAutomationsSettingsCommand(
                body.TimeZoneId,
                body.BusinessStart,
                body.BusinessEnd,
                body.BusinessDays,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/tutors/{tutorId:guid}/preferences", async (Guid tutorId, IMediator mediator) =>
            (await mediator.Send(new GetTutorMessagingPreferenceQuery(tutorId))).ToHttpResult());

        group.MapPut("/tutors/{tutorId:guid}/preferences", async (
            Guid tutorId,
            HttpContext httpContext,
            [FromBody] UpdateTutorPreferenceBody body,
            IMediator mediator) =>
            (await mediator.Send(new UpdateTutorMessagingPreferenceCommand(
                tutorId,
                body.WhatsAppEnabled,
                body.EmailEnabled,
                body.MarketingEnabled,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/campaigns", async (IMediator mediator) =>
            (await mediator.Send(new ListCampaignsQuery())).ToHttpResult());

        group.MapGet("/campaigns/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetCampaignByIdQuery(id))).ToHttpResult());

        group.MapPost("/campaigns", async (HttpContext httpContext, [FromBody] CreateCampaignBody body, IMediator mediator) =>
            (await mediator.Send(new CreateCampaignCommand(
                body.Name,
                body.SegmentKind,
                body.TemplateCode,
                body.InactiveDays,
                body.CooldownDays,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPut("/campaigns/{id:guid}", async (
            Guid id,
            HttpContext httpContext,
            [FromBody] UpdateCampaignBody body,
            IMediator mediator) =>
            (await mediator.Send(new UpdateCampaignCommand(
                id,
                body.Name,
                body.TemplateCode,
                body.Status,
                body.InactiveDays,
                body.CooldownDays,
                EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapPost("/campaigns/{id:guid}/launch", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new LaunchCampaignCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        group.MapGet("/campaigns/{id:guid}/runs", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new ListCampaignRunsQuery(id))).ToHttpResult());

        group.MapGet("/nps/report", async ([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, IMediator mediator) =>
        {
            var end = to ?? DateTimeOffset.UtcNow;
            var start = from ?? end.AddDays(-30);
            return (await mediator.Send(new GetNpsReportQuery(start, end))).ToHttpResult();
        });

        group.MapGet("/reports/return-frequency", async ([FromQuery] DateTimeOffset? from, [FromQuery] DateTimeOffset? to, IMediator mediator) =>
        {
            var end = to ?? DateTimeOffset.UtcNow;
            var start = from ?? end.AddDays(-30);
            return (await mediator.Send(new GetReturnFrequencyReportQuery(start, end))).ToHttpResult();
        });

        return builder;
    }

    private sealed record UpdateMessageTemplateBody(string Body, string? Subject, bool IsActive);

    private sealed record UpdateAutomationsSettingsBody(
        string TimeZoneId,
        string BusinessStart,
        string BusinessEnd,
        IReadOnlyList<string> BusinessDays);

    private sealed record UpdateTutorPreferenceBody(bool WhatsAppEnabled, bool EmailEnabled, bool MarketingEnabled);

    private sealed record CreateCampaignBody(
        string Name,
        CampaignSegmentKind SegmentKind,
        string? TemplateCode,
        int InactiveDays,
        int CooldownDays);

    private sealed record UpdateCampaignBody(
        string Name,
        string TemplateCode,
        CampaignStatus Status,
        int InactiveDays,
        int CooldownDays);
}
