using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace API.Extensions;

/// <summary>
/// Registers and maps API health check endpoints for orchestrators and load balancers.
/// </summary>
public static class HealthCheckExtensions
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Adds the in-process API liveness check. Database checks are registered by <c>AddCoreModule</c>.
    /// </summary>
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services)
    {
        services.AddHealthChecks()
            .AddCheck("api", () => HealthCheckResult.Healthy("API process is running."), tags: ["live", "ready"]);

        return services;
    }

    /// <summary>
    /// Maps <c>/health/live</c>, <c>/health/ready</c>, and aggregated JSON <c>/health</c>.
    /// </summary>
    public static WebApplication MapApiHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("live"),
            AllowCachingResponses = false
        }).AllowAnonymous();

        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = registration => registration.Tags.Contains("ready"),
            AllowCachingResponses = false
        }).AllowAnonymous();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true,
            AllowCachingResponses = false,
            ResponseWriter = WriteHealthReportJsonAsync
        }).AllowAnonymous();

        return app;
    }

    private static Task WriteHealthReportJsonAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            duration = report.TotalDuration.ToString("c"),
            checks = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                duration = entry.Value.Duration.ToString("c"),
                description = entry.Value.Description
            })
        };

        return context.Response.WriteAsync(JsonSerializer.Serialize(payload, JsonOptions));
    }
}
