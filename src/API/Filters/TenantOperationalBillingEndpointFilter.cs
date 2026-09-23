using Core.Domain;
using Platform.Application.Abstractions;
using Platform.Domain.Entities;
using PlatformBillingErrors = Platform.Domain.ErrorCodes;

namespace API.Filters;

/// <summary>
/// Blocks tenant operational APIs when SaaS billing is locked (9.5).
/// </summary>
public sealed class TenantOperationalBillingEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var path = context.HttpContext.Request.Path;
        if (TenantEndpointAllowlist.IsExempt(path) || IsBillingAllowlisted(path))
        {
            return await next(context);
        }

        if (!path.StartsWithSegments("/api/v1"))
        {
            return await next(context);
        }

        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (tenantContext.TenantId == Guid.Empty)
        {
            return await next(context);
        }

        var reader = context.HttpContext.RequestServices.GetRequiredService<ITenantBillingStandingReader>();
        var standing = await reader.GetStandingAsync(tenantContext.TenantId, context.HttpContext.RequestAborted);
        if (standing != BillingStanding.Locked)
        {
            return await next(context);
        }

        return Results.Json(
            new
            {
                error = PlatformBillingErrors.Billing.OperationalLocked.Message,
                code = PlatformBillingErrors.Billing.OperationalLocked.Code
            },
            statusCode: StatusCodes.Status403Forbidden);
    }

    private static bool IsBillingAllowlisted(PathString path)
    {
        var value = path.Value ?? string.Empty;
        return value.StartsWith("/api/v1/billing/", StringComparison.OrdinalIgnoreCase);
    }
}
