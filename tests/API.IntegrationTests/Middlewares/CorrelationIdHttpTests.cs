using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;

namespace API.IntegrationTests.Middlewares;

[Collection("IntegrationTests")]
public class CorrelationIdHttpTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public CorrelationIdHttpTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetRoot_WithoutHeader_ReturnsCorrelationId()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/");

        response.Headers.TryGetValues("X-Correlation-Id", out var values).Should().BeTrue();
        values!.First().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task GetRoot_WithHeader_EchoesCorrelationId()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Correlation-Id", "abc");

        var response = await client.GetAsync("/");

        response.Headers.GetValues("X-Correlation-Id").First().Should().Be("abc");
    }
}
