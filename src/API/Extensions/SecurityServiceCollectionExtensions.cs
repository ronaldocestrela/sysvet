using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace API.Extensions;

/// <summary>
/// Security hardening services (rate limiting).
/// </summary>
public static class SecurityServiceCollectionExtensions
{
    /// <summary>Rate limit policy name for authentication endpoints.</summary>
    public const string AuthRateLimitPolicy = "auth";

    /// <summary>Registers rate limiting for login and tutor portal registration.</summary>
    public static IServiceCollection AddSysVetSecurity(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy(AuthRateLimitPolicy, httpContext =>
            {
                var partitionKey = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                return RateLimitPartition.GetFixedWindowLimiter(partitionKey, _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 20,
                    Window = TimeSpan.FromMinutes(1),
                    QueueLimit = 0
                });
            });
        });

        return services;
    }

    /// <summary>Applies security headers middleware.</summary>
    public static WebApplication UseSysVetSecurityHeaders(this WebApplication app)
    {
        app.UseMiddleware<Middlewares.SecurityHeadersMiddleware>();
        if (!app.Environment.IsDevelopment())
        {
            app.UseHsts();
        }

        return app;
    }
}
