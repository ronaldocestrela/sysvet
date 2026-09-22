using Automations.Application.Abstractions;
using Core.Domain;
using Microsoft.AspNetCore.Http;

namespace API.Filters;

/// <summary>
/// Resolves tenant schema from a signed NPS token before public handlers run.
/// </summary>
public sealed class NpsPublicTenantFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var token = context.HttpContext.Request.RouteValues["token"] as string;
        if (string.IsNullOrWhiteSpace(token))
        {
            return Results.BadRequest(new { error = "Token is required." });
        }

        var tokenService = context.HttpContext.RequestServices.GetRequiredService<INpsSurveyTokenService>();
        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        var parsed = tokenService.Validate(token, DateTimeOffset.UtcNow);
        if (parsed.IsFailure)
        {
            return Results.BadRequest(new { error = parsed.Error.Message });
        }

        tenantContext.TenantId = parsed.Value.TenantId;
        tenantContext.SchemaName = TenantSchema.FromId(parsed.Value.TenantId);
        return await next(context);
    }
}
