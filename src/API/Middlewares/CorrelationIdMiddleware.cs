using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace API.Middlewares;

/// <summary>
/// Propagates a correlation identifier for each HTTP request so logs and responses share the same trace id.
/// </summary>
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    /// <summary>
    /// Creates the middleware that reads or assigns <c>X-Correlation-Id</c>.
    /// </summary>
    /// <param name="next">The next delegate in the HTTP pipeline.</param>
    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    /// <summary>
    /// Ensures every request has a correlation id on <see cref="HttpContext.TraceIdentifier"/>, response headers, and log scope.
    /// </summary>
    /// <param name="context">The current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);

        if (StringValues.IsNullOrEmpty(correlationId))
        {
            correlationId = Activity.Current?.Id ?? context.TraceIdentifier;
        }

        context.TraceIdentifier = correlationId.ToString();

        context.Response.OnStarting(() =>
        {
            if (!context.Response.Headers.ContainsKey(CorrelationIdHeaderName))
            {
                context.Response.Headers.Append(CorrelationIdHeaderName, new[] { context.TraceIdentifier });
            }

            return Task.CompletedTask;
        });

        var logger = context.RequestServices.GetRequiredService<ILogger<CorrelationIdMiddleware>>();
        using (logger.BeginScope(new Dictionary<string, object> { ["CorrelationId"] = context.TraceIdentifier }))
        {
            await _next(context);
        }
    }
}
