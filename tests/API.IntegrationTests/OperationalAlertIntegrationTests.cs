using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class OperationalAlertIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OperationalAlertIntegrationTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task OpsProbe500_Should_ReturnInternalServerError_AndLiveStillHealthy()
    {
        var client = _factory.CreateClient();
        var probe = await client.GetAsync("/internal/ops-probe-500");
        probe.StatusCode.Should().Be(HttpStatusCode.InternalServerError);

        var live = await client.GetAsync("/health/live");
        live.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task HealthAggregate_Should_IncludeOpsChecks()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/health");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        var names = json.RootElement.GetProperty("checks").EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .ToList();

        names.Should().Contain("ops-http-5xx");
        names.Should().Contain("ops-sync-push");
        names.Should().Contain("ops-billing-failures");
    }

    [Fact]
    public void Host_ShouldFailFast_WhenTraceSampleRatioInvalid()
    {
        var act = () =>
        {
            using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment(Environments.Development);
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["Observability:TraceSampleRatio"] = "1.5"
                    });
                });
            });

            factory.CreateClient();
        };

        act.Should().Throw<Exception>();
    }
}
