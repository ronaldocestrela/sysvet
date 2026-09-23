using Core.Infrastructure.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace API.Extensions;

/// <summary>Registers OpenTelemetry tracing and metrics for the SysVet API (10.5).</summary>
public static class OpenTelemetryServiceCollectionExtensions
{
    /// <summary>Adds ASP.NET Core, HTTP client and runtime instrumentation with optional OTLP export.</summary>
    public static IServiceCollection AddSysVetOpenTelemetry(this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var observability = configuration.GetSection(ObservabilityOptions.SectionName).Get<ObservabilityOptions>() ?? new ObservabilityOptions();

        services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("sysvet-api"))
            .WithTracing(tracing =>
            {
                tracing
                    .SetSampler(new TraceIdRatioBasedSampler(observability.TraceSampleRatio))
                    .AddAspNetCoreInstrumentation(options =>
                    {
                        options.EnrichWithHttpRequest = (activity, request) =>
                        {
                            activity.SetTag("correlation.id", request.HttpContext.TraceIdentifier);
                        };
                    })
                    .AddHttpClientInstrumentation();

                if (environment.IsDevelopment() && observability.ConsoleExporter)
                {
                    tracing.AddConsoleExporter();
                }

                if (!string.IsNullOrWhiteSpace(observability.OtlpEndpoint))
                {
                    tracing.AddOtlpExporter(options => options.Endpoint = new Uri(observability.OtlpEndpoint));
                }
            })
            .WithMetrics(metrics =>
            {
                metrics
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddMeter("SysVet.Operations");

                if (environment.IsDevelopment() && observability.ConsoleExporter)
                {
                    metrics.AddConsoleExporter();
                }

                if (!string.IsNullOrWhiteSpace(observability.OtlpEndpoint))
                {
                    metrics.AddOtlpExporter(options => options.Endpoint = new Uri(observability.OtlpEndpoint));
                }
            });

        return services;
    }
}
