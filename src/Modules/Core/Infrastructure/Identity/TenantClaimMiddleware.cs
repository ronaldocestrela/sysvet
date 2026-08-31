using Core.Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Core.Infrastructure.Identity;

public class TenantClaimMiddleware
{
    private readonly RequestDelegate _next;

    public TenantClaimMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
        {
            var tenantIdClaim = context.User.FindFirst("TenantId");
            if (tenantIdClaim != null && !string.IsNullOrWhiteSpace(tenantIdClaim.Value))
            {
                var tenantContext = context.RequestServices.GetRequiredService<ITenantContext>();
                
                if (Guid.TryParse(tenantIdClaim.Value, out var tenantId))
                {
                    tenantContext.TenantId = tenantId;
                    tenantContext.SchemaName = $"tenant_{tenantId.ToString("N").ToLowerInvariant()}";
                }

                var userIdClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
                if (userIdClaim != null && Guid.TryParse(userIdClaim.Value, out var userId))
                {
                    tenantContext.UserId = userId;
                }
            }
        }

        await _next(context);
    }
}
