using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace API.Middlewares;

public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

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
