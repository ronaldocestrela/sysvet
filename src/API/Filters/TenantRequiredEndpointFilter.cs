using Core.Domain;
using PlatformTenancyErrors = Platform.Domain.ErrorCodes;

namespace API.Filters;

/// <summary>
/// Requires a resolved tenant on staff API routes (Platform 9.1).
/// </summary>
public sealed class TenantRequiredEndpointFilter : IEndpointFilter
{
    /// <inheritdoc />
    public ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var path = context.HttpContext.Request.Path;
        if (TenantEndpointAllowlist.IsExempt(path))
        {
            return next(context);
        }

        if (!path.StartsWithSegments("/api/v1"))
        {
            return next(context);
        }

        var tenantContext = context.HttpContext.RequestServices.GetRequiredService<ITenantContext>();
        if (tenantContext.TenantId == Guid.Empty)
        {
            return ValueTask.FromResult<object?>(Results.Json(
                new { error = PlatformTenancyErrors.Tenancy.TenantRequired.Message, code = PlatformTenancyErrors.Tenancy.TenantRequired.Code },
                statusCode: StatusCodes.Status401Unauthorized));
        }

        return next(context);
    }
}

/// <summary>Paths that do not require tenant resolution before handlers run.</summary>
public static class TenantEndpointAllowlist
{
    /// <summary>Returns true when tenant enforcement should be skipped.</summary>
    public static bool IsExempt(PathString path)
    {
        var value = path.Value ?? string.Empty;

        if (value is "/" or "")
        {
            return true;
        }

        if (value.StartsWith("/health", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.StartsWith("/openapi", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/scalar", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.StartsWith("/hubs/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.StartsWith("/api/v1/public/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.Equals("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/v1/auth/refresh", StringComparison.OrdinalIgnoreCase)
            || value.Equals("/api/v1/auth/register", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.StartsWith("/api/v1/tutor-portal/login", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/tutor-portal/register", StringComparison.OrdinalIgnoreCase)
            || value.StartsWith("/api/v1/tutor-portal/refresh", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        if (value.StartsWith("/api/v1/platform/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        return false;
    }
}
