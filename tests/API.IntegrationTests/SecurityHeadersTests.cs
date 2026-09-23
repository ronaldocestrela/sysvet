using System.Net;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace API.IntegrationTests;

[Collection("IntegrationTests")]
public class SecurityHeadersTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public SecurityHeadersTests(WebApplicationFactory<Program> factory) => _factory = factory;

    [Fact]
    public async Task RootResponse_ShouldIncludeSecurityHeaders()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync("/");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Headers.GetValues("X-Content-Type-Options").Single().Should().Be("nosniff");
        response.Headers.GetValues("X-Frame-Options").Single().Should().Be("DENY");
    }
}
