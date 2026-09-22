using API.Filters;
using Core.Application.Common.Interfaces;
using Core.Domain;
using Core.Infrastructure.Tenancy;
using Microsoft.Extensions.Options;
using Platform.Application.Tenancy;
using Platform.Domain.ValueObjects;
using PlatformTenancyErrors = Platform.Domain.ErrorCodes;

namespace API.Middlewares;

/// <summary>
/// Resolves <see cref="ITenantContext"/> from JWT, headers, or host (Platform 9.1 / ADR-046).
/// </summary>
public sealed class TenantResolutionMiddleware
{
    private static readonly HashSet<string> ReservedHostLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "localhost",
        "127",
        "www",
        "api",
        "app"
    };

    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public TenantResolutionMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Resolves tenant context for the request.</summary>
    public async Task InvokeAsync(HttpContext context)
    {
        var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
        var tenancySettings = context.RequestServices.GetRequiredService<IOptions<TenancySettings>>().Value;
        var slugLookup = context.RequestServices.GetRequiredService<ITenantSlugLookup>();
        var tenantSignInGate = context.RequestServices.GetRequiredService<ITenantSignInGate>();

        var jwtTenantId = ResolveJwtTenantId(context);
        if (jwtTenantId != Guid.Empty)
        {
            ApplyTenant(tenantContext, jwtTenantId);
            ResolveUserId(context, tenantContext);

            var alternateTenantId = await ResolveAlternateTenantIdAsync(context, slugLookup, context.RequestAborted);
            if (alternateTenantId is { } alt && alt != jwtTenantId)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new { error = PlatformTenancyErrors.Tenancy.TenantMismatch.Message });
                return;
            }

            if (await BlockInactiveTenantAsync(context, tenantContext, tenantSignInGate))
            {
                return;
            }

            await _next(context);
            return;
        }

        var resolved = await ResolveAlternateTenantIdAsync(context, slugLookup, context.RequestAborted);
        if (resolved is { } tenantId)
        {
            ApplyTenant(tenantContext, tenantId);
        }
        else if (tenancySettings.SingleTenantId is { } single && single != Guid.Empty)
        {
            ApplyTenant(tenantContext, single);
        }
        else
        {
            tenantContext.SchemaName = tenancySettings.DefaultSchema;
        }

        ResolveUserId(context, tenantContext);

        if (await BlockInactiveTenantAsync(context, tenantContext, tenantSignInGate))
        {
            return;
        }

        await _next(context);
    }

    private static async Task<bool> BlockInactiveTenantAsync(
        HttpContext context,
        ITenantContext tenantContext,
        ITenantSignInGate tenantSignInGate)
    {
        if (!RequiresActiveTenant(context.Request.Path) || tenantContext.TenantId == Guid.Empty)
        {
            return false;
        }

        var active = await tenantSignInGate.EnsureActiveAsync(tenantContext.TenantId, context.RequestAborted);
        if (active.IsSuccess)
        {
            return false;
        }

        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        await context.Response.WriteAsJsonAsync(new
        {
            error = PlatformTenancyErrors.Tenant.NotActive.Message,
            code = PlatformTenancyErrors.Tenant.NotActive.Code
        });
        return true;
    }

    private static bool RequiresActiveTenant(PathString path)
    {
        var value = path.Value ?? string.Empty;
        if (!value.StartsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        if (TenantEndpointAllowlist.IsExempt(path))
        {
            return false;
        }

        if (value.StartsWith("/api/v1/platform/", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        return true;
    }

    private static Guid ResolveJwtTenantId(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Guid.Empty;
        }

        var tenantIdClaim = context.User.FindFirst("TenantId");
        return tenantIdClaim is not null && Guid.TryParse(tenantIdClaim.Value, out var tenantId)
            ? tenantId
            : Guid.Empty;
    }

    private static void ResolveUserId(HttpContext context, ITenantContext tenantContext)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)
            ?? context.User.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub);
        if (userIdClaim is not null && Guid.TryParse(userIdClaim.Value, out var userId))
        {
            tenantContext.UserId = userId;
        }
    }

    private static async Task<Guid?> ResolveAlternateTenantIdAsync(
        HttpContext context,
        ITenantSlugLookup slugLookup,
        CancellationToken cancellationToken)
    {
        if (context.Request.Headers.TryGetValue(TenantResolutionHeaders.TenantId, out var tenantHeader)
            && Guid.TryParse(tenantHeader.ToString(), out var headerTenantId)
            && headerTenantId != Guid.Empty)
        {
            return headerTenantId;
        }

        if (context.Request.Headers.TryGetValue(TenantResolutionHeaders.TenantSlug, out var slugHeader))
        {
            var slug = TenantSlug.Normalize(slugHeader.ToString());
            return await slugLookup.ResolveTenantIdBySlugAsync(slug, cancellationToken);
        }

        var hostLabel = ResolveHostLabel(context.Request.Host.Host);
        if (hostLabel is null)
        {
            return null;
        }

        return await slugLookup.ResolveTenantIdBySlugAsync(hostLabel, cancellationToken);
    }

    /// <summary>Extracts tenant slug from host for tests and resolution.</summary>
    public static string? ResolveHostLabel(string? host)
    {
        if (string.IsNullOrWhiteSpace(host))
        {
            return null;
        }

        var lower = host.ToLowerInvariant();
        if (lower is "localhost" or "127.0.0.1")
        {
            return null;
        }

        var parts = lower.Split('.', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length < 2)
        {
            return null;
        }

        var label = parts[0];
        if (ReservedHostLabels.Contains(label))
        {
            return null;
        }

        return TenantSlug.IsValid(label) ? label : null;
    }

    private static void ApplyTenant(ITenantContext tenantContext, Guid tenantId)
    {
        tenantContext.TenantId = tenantId;
        tenantContext.SchemaName = TenantSchema.FromId(tenantId);
    }
}
