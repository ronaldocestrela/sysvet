using API.Extensions;
using Core.Domain;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Platform.Application.Billing;

namespace API.Extensions;

/// <summary>Anonymous Asaas webhook for SaaS billing (9.4).</summary>
public static class PlatformBillingWebhookEndpointExtensions
{
    /// <summary>Maps <c>POST /api/v1/platform/webhooks/asaas</c>.</summary>
    public static IEndpointRouteBuilder MapPlatformBillingWebhookEndpoints(this IEndpointRouteBuilder builder)
    {
        builder.MapPost("/api/v1/platform/webhooks/asaas", HandleAsaasWebhook)
            .WithTags("Platform")
            .AllowAnonymous();

        return builder;
    }

    private static async Task<IResult> HandleAsaasWebhook(
        HttpContext httpContext,
        [FromBody] AsaasWebhookPayload payload,
        IMediator mediator)
    {
        httpContext.Request.Headers.TryGetValue("asaas-access-token", out var token);
        var result = await mediator.Send(new ProcessAsaasWebhookCommand(token.ToString(), payload));
        return result.ToHttpResult(httpContext);
    }
}
