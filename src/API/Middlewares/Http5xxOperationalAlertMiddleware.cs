namespace API.Middlewares;

/// <summary>Counts HTTP 5xx responses for operational alerting without affecting readiness probes.</summary>
public sealed class Http5xxOperationalAlertMiddleware
{
    private readonly RequestDelegate _next;

    /// <summary>Creates the middleware.</summary>
    public Http5xxOperationalAlertMiddleware(RequestDelegate next) => _next = next;

    /// <summary>Records 5xx after the pipeline completes.</summary>
    public async Task InvokeAsync(HttpContext context, Operations.OperationalAlertCoordinator coordinator)
    {
        await _next(context);

        if (context.Response.StatusCode >= 500)
        {
            coordinator.RecordHttp5xx();
        }
    }
}
