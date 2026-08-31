using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Clients.Infrastructure.Http;
using Core.Domain;
using FluentAssertions;
using RichardSzalay.MockHttp;
using Xunit;

namespace Clients.Tests.Http;

public class ApiClientTests
{
    private readonly MockHttpMessageHandler _mockHttp;
    private readonly ApiClient _apiClient;

    public ApiClientTests()
    {
        _mockHttp = new MockHttpMessageHandler();
        var httpClient = _mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://api.sysvet.com");
        _apiClient = new ApiClient(httpClient);
    }

    [Fact]
    public async Task GetAsync_Returns_Success_Result_When_Ok()
    {
        // Arrange
        var expectedData = new { Id = 1, Name = "Test" };
        _mockHttp.When("/api/test")
            .Respond("application/json", JsonSerializer.Serialize(expectedData));

        // Act
        var result = await _apiClient.GetAsync<TestDto>("/api/test");

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Name.Should().Be("Test");
    }

    [Fact]
    public async Task GetAsync_Returns_Failure_Result_With_ProblemDetails_When_BadRequest()
    {
        // Arrange
        var problem = new HttpProblemDetails
        {
            Type = "https://sysvet.com/errors/validation",
            Title = "Validation Error",
            Status = 400,
            Detail = "Invalid request."
        };
        _mockHttp.When("/api/test")
            .Respond(HttpStatusCode.BadRequest, "application/json", JsonSerializer.Serialize(problem));

        // Act
        var result = await _apiClient.GetAsync<TestDto>("/api/test");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Code.Should().Be("400");
        result.Error.Message.Should().Be("Validation Error: Invalid request.");
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
