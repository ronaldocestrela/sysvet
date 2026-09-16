using System.Net;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class HealthCheckTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public HealthCheckTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task HealthLive_Should_ReturnOk_EvenWhenDatabaseIsUnreachable()
    {
        using var factory = CreateFactoryWithUnreachableDatabase();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Be("Healthy");
    }

    [Fact]
    public async Task HealthAggregate_Should_ReturnJsonWithCoreDbCheck()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");

        using var json = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
        json.RootElement.GetProperty("status").GetString().Should().Be("Healthy");
        json.RootElement.GetProperty("checks").EnumerateArray()
            .Select(c => c.GetProperty("name").GetString())
            .Should().Contain("core-db");
    }

    [Fact]
    public async Task HealthReady_Should_ReturnServiceUnavailable_WhenDatabaseIsUnreachable()
    {
        using var factory = CreateFactoryWithUnreachableDatabase();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HealthAggregate_Should_ReturnServiceUnavailable_WhenDatabaseIsUnreachable()
    {
        using var factory = CreateFactoryWithUnreachableDatabase();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        response.StatusCode.Should().Be(HttpStatusCode.ServiceUnavailable);
    }

    [Fact]
    public async Task HealthEndpoints_Should_BeAnonymous_WithoutAuthorizationHeader()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Authorization = null;

        var live = await client.GetAsync("/health/live");
        var ready = await client.GetAsync("/health/ready");
        var aggregate = await client.GetAsync("/health");

        live.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        ready.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
        aggregate.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task HealthLive_Should_ReturnCorrelationIdHeader()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        response.Headers.TryGetValues("X-Correlation-Id", out var values).Should().BeTrue();
        values!.First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task HealthLive_Should_EchoProvidedCorrelationId()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "integration-health-trace");

        var response = await client.GetAsync("/health/live");

        response.Headers.GetValues("X-Correlation-Id").First().Should().Be("integration-health-trace");
    }

    private static WebApplicationFactory<Program> CreateFactoryWithUnreachableDatabase()
    {
        return new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseSetting(WebHostDefaults.EnvironmentKey, Environments.Development);
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        "Server=127.0.0.1,59999;Database=health_check_test;User Id=sa;Password=Invalid!;TrustServerCertificate=True;Connect Timeout=1",
                    ["Database:Provider"] = "SqlServer",
                    ["Database:ConnectionStringName"] = "DefaultConnection",
                    ["TenancySettings:DefaultSchema"] = "dbo",
                    ["JwtSettings:Secret"] = "integration-test-secret-min-16",
                    ["JwtSettings:Issuer"] = "sysvet-api",
                    ["JwtSettings:Audience"] = "sysvet-clients",
                    ["JwtSettings:ExpiryMinutes"] = "60"
                });
            });
        });
    }
}
