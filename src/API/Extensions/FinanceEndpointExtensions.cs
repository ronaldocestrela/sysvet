using Core.Domain;
using Finance.Application.Categories;
using Finance.Application.CostCenters;
using Finance.Application.Projections;
using Finance.Application.Reports;
using Finance.Application.Reconciliation;
using Finance.Application.Reconciliation.Dtos;
using Finance.Application.Titles.Commands;
using Finance.Application.Titles.Queries;
using Finance.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Finance module HTTP endpoints for AP/AR and projections.
/// </summary>
public static class FinanceEndpointExtensions
{
    /// <summary>
    /// Maps Finance routes.
    /// </summary>
    public static IEndpointRouteBuilder MapFinanceEndpoints(this IEndpointRouteBuilder builder)
    {
        var titles = builder.MapGroup("/api/v1/financial-titles").RequireAuthorization().WithTags("Finance");

        titles.MapGet("/", async (
            [FromQuery] TitleDirection? direction,
            [FromQuery] TitleStatus? status,
            [FromQuery] PartyKind? partyKind,
            [FromQuery] Guid? partyId,
            [FromQuery] DateOnly? dueFrom,
            [FromQuery] DateOnly? dueTo,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IMediator mediator) =>
            (await mediator.Send(new ListFinancialTitlesQuery(direction, status, partyKind, partyId, dueFrom, dueTo, page <= 0 ? 1 : page, pageSize))).ToHttpResult());

        titles.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetFinancialTitleByIdQuery(id))).ToHttpResult());

        titles.MapPost("/", async (HttpContext httpContext, [FromBody] CreateManualFinancialTitleCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        titles.MapPost("/{id:guid}/settle", async (Guid id, HttpContext httpContext, [FromBody] SettleFinancialTitleBody body, IMediator mediator) =>
            (await mediator.Send(new SettleFinancialTitleCommand(id, body.Amount, body.Method, body.PaidAt, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        titles.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, IMediator mediator) =>
            (await mediator.Send(new CancelFinancialTitleCommand(id, EndpointIdempotency.ReadKey(httpContext)))).ToHttpResult());

        var categories = builder.MapGroup("/api/v1/financial-categories").RequireAuthorization().WithTags("Finance");
        categories.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new ListFinancialCategoriesQuery())).ToHttpResult());
        categories.MapPut("/", async (HttpContext httpContext, [FromBody] UpsertFinancialCategoryCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        var costCenters = builder.MapGroup("/api/v1/cost-centers").RequireAuthorization().WithTags("Finance");
        costCenters.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new ListCostCentersQuery())).ToHttpResult());
        costCenters.MapPut("/", async (HttpContext httpContext, [FromBody] UpsertCostCenterCommand command, IMediator mediator) =>
            (await mediator.Send(command with { IdempotencyKey = EndpointIdempotency.ReadKey(httpContext) })).ToHttpResult());

        var finance = builder.MapGroup("/api/v1/finance").RequireAuthorization().WithTags("Finance");
        finance.MapGet("/projection", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            (await mediator.Send(new GetBalanceProjectionQuery(from, to))).ToHttpResult());
        finance.MapGet("/cash-flow", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, IMediator mediator) =>
            (await mediator.Send(new GetCashFlowQuery(from, to))).ToHttpResult());
        finance.MapGet("/dre", async ([FromQuery] int year, [FromQuery] int month, IMediator mediator) =>
            (await mediator.Send(new GetSimplifiedDreQuery(year, month))).ToHttpResult());
        finance.MapGet("/statements/export", async ([FromQuery] DateOnly from, [FromQuery] DateOnly to, [FromQuery] string format, IMediator mediator) =>
        {
            if (!Enum.TryParse<FinanceExportFormat>(format, ignoreCase: true, out var exportFormat))
            {
                return Results.BadRequest(new { error = "Invalid format. Use csv or pdf." });
            }

            return ToFileResult(await mediator.Send(new ExportFinanceStatementsQuery(from, to, exportFormat)));
        });
        finance.MapGet("/parties/{partyKind}/{partyId:guid}/ledger", async (PartyKind partyKind, Guid partyId, IMediator mediator) =>
            (await mediator.Send(new GetPartyLedgerQuery(partyKind, partyId))).ToHttpResult());

        finance.MapGet("/card-reconciliations", async (IMediator mediator) =>
            (await mediator.Send(new ListCardReconciliationsQuery())).ToHttpResult());
        finance.MapGet("/card-reconciliations/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetCardReconciliationByIdQuery(id))).ToHttpResult());
        finance.MapPost("/card-reconciliations", async (HttpContext httpContext, [FromBody] ImportCardStatementCommand command, IMediator mediator) =>
        {
            command.IdempotencyKey = EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });
        finance.MapGet("/card-settlements/unmatched", async (IMediator mediator) =>
            (await mediator.Send(new GetUnmatchedCardSettlementsQuery())).ToHttpResult());

        return builder;
    }

    private static IResult ToFileResult(Core.Domain.Result<ReportFileDto> result)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    private sealed record SettleFinancialTitleBody(decimal Amount, string Method, DateTimeOffset? PaidAt);
}
