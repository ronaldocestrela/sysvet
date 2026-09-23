using Intelligence.Application.Dashboard.Commands;
using Intelligence.Application.Dashboard.Dtos;
using Intelligence.Application.Dashboard.Queries;
using Intelligence.Application.Reports;
using Intelligence.Application.Reports.Dtos;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>Intelligence module HTTP endpoints (roadmap 10.1).</summary>
public static class IntelligenceEndpointExtensions
{
    /// <summary>Maps tenant dashboard routes under <c>/api/v1/intelligence</c>.</summary>
    public static IEndpointRouteBuilder MapIntelligenceEndpoints(this IEndpointRouteBuilder builder)
    {
        var group = builder.MapGroup("/api/v1/intelligence")
            .RequireAuthorization()
            .WithTags("Intelligence");

        group.MapGet("/dashboard", async (IMediator mediator) =>
            (await mediator.Send(new GetTenantDashboardQuery())).ToHttpResult());

        group.MapGet("/dashboard-layouts/{accessProfileId:guid}", async (Guid accessProfileId, IMediator mediator) =>
            (await mediator.Send(new GetProfileDashboardLayoutQuery(accessProfileId))).ToHttpResult());

        group.MapPut("/dashboard-layouts/{accessProfileId:guid}", async (
            Guid accessProfileId,
            [FromBody] ProfileDashboardLayoutDto body,
            IMediator mediator) =>
            (await mediator.Send(new UpsertProfileDashboardLayoutCommand(
                accessProfileId,
                body.Slots))).ToHttpResult());

        group.MapGet("/reports/abc-customers", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            (await mediator.Send(new GetAbcCustomersReportQuery(from, to))).ToHttpResult());

        group.MapGet("/reports/abc-products", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            (await mediator.Send(new GetAbcProductsReportQuery(from, to))).ToHttpResult());

        group.MapGet("/reports/productivity", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            (await mediator.Send(new GetProductivityReportQuery(from, to))).ToHttpResult());

        group.MapGet("/reports/abc-customers/export", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            ToFileResult(await mediator.Send(new ExportAbcCustomersReportQuery(from, to))));

        group.MapGet("/reports/abc-products/export", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            ToFileResult(await mediator.Send(new ExportAbcProductsReportQuery(from, to))));

        group.MapGet("/reports/productivity/export", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            ToFileResult(await mediator.Send(new ExportProductivityReportQuery(from, to))));

        return builder;
    }

    private static IResult ToFileResult(Core.Domain.Result<IntelligenceReportFileDto> result)
    {
        if (result.IsFailure)
        {
            return result.ToHttpResult();
        }

        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }
}
