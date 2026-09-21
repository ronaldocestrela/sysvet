using Core.Domain;
using Fiscal.Application.Documents;
using Fiscal.Application.Issuer;
using Fiscal.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace API.Extensions;

/// <summary>
/// Fiscal module HTTP endpoints for NF-e and NFS-e Nacional.
/// </summary>
public static class FiscalEndpointExtensions
{
    /// <summary>
    /// Maps Fiscal routes.
    /// </summary>
    public static IEndpointRouteBuilder MapFiscalEndpoints(this IEndpointRouteBuilder builder)
    {
        var issuer = builder.MapGroup("/api/v1/fiscal/issuer").RequireAuthorization().WithTags("Fiscal");

        issuer.MapGet("/", async (IMediator mediator) =>
            (await mediator.Send(new GetIssuerProfileQuery())).ToHttpResult());

        issuer.MapPut("/", async (HttpContext httpContext, [FromBody] UpsertIssuerProfileCommand command, IMediator mediator) =>
            (await mediator.Send(command)).ToHttpResult());

        issuer.MapGet("/pos-bundle", async (IMediator mediator) =>
            (await mediator.Send(new GetFiscalPosBundleQuery())).ToHttpResult());

        issuer.MapPost("/certificate", async (HttpContext httpContext, HttpRequest request, IMediator mediator) =>
        {
            if (!request.HasFormContentType)
            {
                return Results.BadRequest(new { error = "Expected multipart/form-data with file and password." });
            }

            var form = await request.ReadFormAsync(httpContext.RequestAborted);
            var file = form.Files.GetFile("file");
            if (file is null || file.Length == 0)
            {
                return Results.BadRequest(new { error = "Certificate file is required." });
            }

            var password = form["password"].ToString();
            if (string.IsNullOrWhiteSpace(password))
            {
                return Results.BadRequest(new { error = "Certificate password is required." });
            }

            await using var stream = file.OpenReadStream();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms, httpContext.RequestAborted);
            return (await mediator.Send(new UploadIssuerCertificateCommand(ms.ToArray(), password))).ToHttpResult();
        });

        var documents = builder.MapGroup("/api/v1/fiscal-documents").RequireAuthorization().WithTags("Fiscal");

        documents.MapGet("/", async (
            [FromQuery] Guid? orderId,
            [FromQuery] FiscalDocumentStatus? status,
            IMediator mediator) =>
            (await mediator.Send(new ListFiscalDocumentsQuery(orderId, status))).ToHttpResult());

        documents.MapGet("/{id:guid}", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new GetFiscalDocumentByIdQuery(id))).ToHttpResult());

        documents.MapPost("/nfce/transmit", async (HttpContext httpContext, [FromBody] TransmitNfceCommand command, IMediator mediator) =>
        {
            EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(command)).ToHttpResult();
        });

        documents.MapPost("/{id:guid}/reconcile", async (Guid id, IMediator mediator) =>
            (await mediator.Send(new ReconcileNfceStatusCommand(id))).ToHttpResult());

        documents.MapPost("/from-order", async (HttpContext httpContext, [FromBody] IssueFromOrderBody body, IMediator mediator) =>
        {
            EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new IssueFromOrderCommand(body.OrderId))).ToHttpResult();
        });

        documents.MapPost("/{id:guid}/cancel", async (Guid id, HttpContext httpContext, [FromBody] CancelFiscalDocumentBody body, IMediator mediator) =>
        {
            EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new CancelFiscalDocumentCommand(id, body.Justification))).ToHttpResult();
        });

        documents.MapPost("/{id:guid}/correction-letter", async (Guid id, HttpContext httpContext, [FromBody] IssueCorrectionLetterBody body, IMediator mediator) =>
        {
            EndpointIdempotency.ReadKey(httpContext);
            return (await mediator.Send(new IssueCorrectionLetterCommand(id, body.CorrectionText))).ToHttpResult();
        });

        documents.MapGet("/{id:guid}/xml", async (Guid id, IMediator mediator) =>
            ToFileResult(await mediator.Send(new DownloadFiscalXmlQuery(id))));

        documents.MapGet("/{id:guid}/danfe", async (Guid id, IMediator mediator) =>
            ToFileResult(await mediator.Send(new DownloadFiscalDanfeQuery(id))));

        return builder;
    }

    private static IResult ToFileResult(Result<FiscalFileDownloadDto?> result)
    {
        if (result.IsFailure)
        {
            return result.ToProblemDetails();
        }

        if (result.Value is null)
        {
            return Results.NotFound();
        }

        return Results.File(result.Value.Content, result.Value.ContentType, result.Value.FileName);
    }

    private sealed record IssueFromOrderBody(Guid OrderId);

    private sealed record CancelFiscalDocumentBody(string Justification);

    private sealed record IssueCorrectionLetterBody(string CorrectionText);
}
