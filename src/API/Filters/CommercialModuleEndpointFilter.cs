using API.Routing;
using Core.Application.Entitlements;
using Core.Domain;
using PlatformEntitlementErrors = Platform.Domain.ErrorCodes;

namespace API.Filters;

/// <summary>
/// Returns 403 when the tenant lacks entitlement for the route commercial module (9.3).
/// </summary>
public sealed class CommercialModuleEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var path = context.HttpContext.Request.Path;
        if (TenantEndpointAllowlist.IsExempt(path) || CommercialModuleRouteMapper.IsCoreExempt(path))
        {
            return await next(context);
        }

        var module = CommercialModuleRouteMapper.ResolveModule(path);
        if (module is null)
        {
            return await next(context);
        }

        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (tenantContext.TenantId == Guid.Empty)
        {
            return await next(context);
        }

        var reader = context.HttpContext.RequestServices.GetRequiredService<ITenantEntitlementReader>();
        if (await reader.IsModuleEnabledAsync(tenantContext.TenantId, module.Value, context.HttpContext.RequestAborted))
        {
            return await next(context);
        }

        return Results.Json(
            new { error = PlatformEntitlementErrors.Entitlement.ModuleDisabled.Message, code = PlatformEntitlementErrors.Entitlement.ModuleDisabled.Code },
            statusCode: StatusCodes.Status403Forbidden);
    }
}
