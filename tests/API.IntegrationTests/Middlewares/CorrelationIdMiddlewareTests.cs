using API.Middlewares;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace API.IntegrationTests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_WithoutHeader_SetsTraceIdentifier()
    {
        var context = new DefaultHttpContext();
        var services = new ServiceCollection();
        services.AddLogging();
        context.RequestServices = services.BuildServiceProvider();

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.TraceIdentifier.Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task InvokeAsync_WithHeader_EchoesCorrelationId()
    {
        const string expected = "test-correlation-abc";
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Correlation-Id"] = expected;

        var services = new ServiceCollection();
        services.AddLogging();
        context.RequestServices = services.BuildServiceProvider();

        RequestDelegate next = _ => Task.CompletedTask;
        var middleware = new CorrelationIdMiddleware(next);

        await middleware.InvokeAsync(context);

        context.TraceIdentifier.Should().Be(expected);
    }
}
