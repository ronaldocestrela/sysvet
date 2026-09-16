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

    [Fact]
    public async Task GetAsync_Deserializes_PagedResult()
    {
        var payload = new PagedResultDto<TutorDto>
        {
            Items = [new TutorDto { Id = Guid.NewGuid(), Name = "Ana" }],
            Page = 1,
            PageSize = 10,
            TotalCount = 1
        };
        _mockHttp.When("/api/v1/tutors")
            .Respond("application/json", JsonSerializer.Serialize(payload));

        var result = await _apiClient.GetAsync<PagedResultDto<TutorDto>>("/api/v1/tutors");

        result.IsSuccess.Should().BeTrue();
        result.Value.Items.Should().HaveCount(1);
        result.Value.Items[0].Name.Should().Be("Ana");
    }

    [Fact]
    public async Task PostAsync_Sends_Idempotency_Key_And_Returns_Created_Body()
    {
        var id = Guid.NewGuid();
        _mockHttp.Expect(HttpMethod.Post, "/api/v1/tutors")
            .WithHeaders("Idempotency-Key", id.ToString())
            .Respond(HttpStatusCode.Created, "application/json", JsonSerializer.Serialize(id));

        var result = await _apiClient.PostAsync<CreateTutorRequest, Guid>(
            "/api/v1/tutors",
            new CreateTutorRequest { Id = id, Name = "Test" },
            id);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(id);
        _mockHttp.VerifyNoOutstandingExpectation();
    }

    [Fact]
    public async Task PutAsync_Returns_Success_On_NoContent()
    {
        var tutorId = Guid.NewGuid();
        _mockHttp.When(HttpMethod.Put, $"/api/v1/tutors/{tutorId}")
            .Respond(HttpStatusCode.NoContent);

        var result = await _apiClient.PutAsync(
            $"/api/v1/tutors/{tutorId}",
            new UpdateTutorRequest { Id = tutorId, Name = "Updated" });

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task DeleteAsync_Returns_Success_On_NoContent()
    {
        var tutorId = Guid.NewGuid();
        _mockHttp.When(HttpMethod.Delete, $"/api/v1/tutors/{tutorId}")
            .Respond(HttpStatusCode.NoContent);

        var result = await _apiClient.DeleteAsync($"/api/v1/tutors/{tutorId}");

        result.IsSuccess.Should().BeTrue();
    }

    private class TestDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
