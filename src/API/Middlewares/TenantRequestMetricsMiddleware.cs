using Core.Domain;
using Platform.Application.Abstractions;

namespace API.Middlewares;

/// <summary>Increments per-tenant daily API request counters (9.7).</summary>
public sealed class TenantRequestMetricsMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public TenantRequestMetricsMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Records a request after the pipeline completes when tenant is resolved.</summary>
    public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext, ITenantRequestRecorder recorder)
    {
        await _next(context);

        if (tenantContext.TenantId == Guid.Empty)
        {
            return;
        }

        var path = context.Request.Path.Value ?? string.Empty;
        if (!path.StartsWith("/api/v1", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        try
        {
            await recorder.RecordAsync(tenantContext.TenantId, context.RequestAborted);
        }
        catch
        {
            // Metrics must not break API responses.
        }
    }
}
