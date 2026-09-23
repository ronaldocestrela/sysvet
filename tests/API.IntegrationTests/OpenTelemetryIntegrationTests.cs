using System.Diagnostics;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class OpenTelemetryIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OpenTelemetryIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task HealthLive_Should_EmitAspNetCoreTrace()
    {
        Activity? captured = null;
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name.Contains("Microsoft.AspNetCore", StringComparison.Ordinal),
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStarted = activity => captured = activity
        };
        ActivitySource.AddActivityListener(listener);

        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health/live");
        response.EnsureSuccessStatusCode();

        captured.Should().NotBeNull();
        captured!.Tags.Should().Contain(t => t.Key == "correlation.id" || t.Key == "http.response.status_code");
    }
}
