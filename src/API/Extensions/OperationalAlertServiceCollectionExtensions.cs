using API.HealthChecks;
using API.Middlewares;
using API.Operations;
using Core.Application.Sync;
using Core.Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Platform.Application.Abstractions;
using Platform.Infrastructure.Health;

namespace API.Extensions;

/// <summary>Registers operational alerting, ops health checks and observer wiring (10.5).</summary>
public static class OperationalAlertServiceCollectionExtensions
{
    /// <summary>Adds alert coordinator, middleware dependencies and ops-tagged health checks.</summary>
    public static IServiceCollection AddOperationalAlerts(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatedOptions<ObservabilityOptions>(configuration, ObservabilityOptions.SectionName);
        services.AddValidatedOptions<BackupOptions>(configuration, BackupOptions.SectionName);

        services.AddSingleton<OperationalAlertCoordinator>();
        services.Replace(ServiceDescriptor.Singleton<ISyncPushObserver>(sp => sp.GetRequiredService<OperationalAlertCoordinator>()));
        services.Replace(ServiceDescriptor.Singleton<IBillingChargeObserver>(sp => sp.GetRequiredService<OperationalAlertCoordinator>()));

        services.AddHealthChecks()
            .AddCheck<OpsHttp5xxHealthCheck>("ops-http-5xx", failureStatus: HealthStatus.Degraded, tags: ["ops"])
            .AddCheck<OpsSyncPushHealthCheck>("ops-sync-push", failureStatus: HealthStatus.Degraded, tags: ["ops"])
            .AddCheck<OpsBillingFailuresHealthCheck>("ops-billing-failures", failureStatus: HealthStatus.Degraded, tags: ["ops"]);

        return services;
    }

    /// <summary>Records HTTP 5xx responses after the pipeline runs.</summary>
    public static WebApplication UseOperationalAlertMiddleware(this WebApplication app)
    {
        app.UseMiddleware<Http5xxOperationalAlertMiddleware>();
        return app;
    }

    /// <summary>Anonymous probe route for integration tests (Development only).</summary>
    public static WebApplication MapOperationalAlertProbe(this WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            app.MapGet("/internal/ops-probe-500", (HttpContext _) => throw new InvalidOperationException("Operational alert probe."))
                .AllowAnonymous();
        }

        return app;
    }
}
